using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Helpers.QueryTranslation;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs
{
	public class EmployeeCalculation
	{
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;

		public EmployeeCalculation(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public async Task Calculate()
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				var platformDatabaseContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
				previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

				using (var tenantRepository = new TenantRepository(platformDatabaseContext, _configuration))//, cacheHelper))
				{
					var tenants = await tenantRepository.GetAllActive();

					foreach (var tenant in tenants)
					{
						if (tenant.AppId != 4)
							continue;

						try
						{
							using (var platformWarehouseRepository = new PlatformWarehouseRepository(platformDatabaseContext, _configuration))//, cacheHelper))
							using (var analyticRepository = new AnalyticRepository(databaseContext, _configuration))
							{

								platformWarehouseRepository.CurrentUser = analyticRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = tenant.OwnerId, PreviewMode = previewMode };

								var warehouse = new Model.Helpers.Warehouse(analyticRepository, _configuration);

								var warehouseEntity = await platformWarehouseRepository.GetByTenantId(tenant.Id);

								if (warehouseEntity != null)
									warehouse.DatabaseName = warehouseEntity.DatabaseName;
								else
									warehouse.DatabaseName = "0";

								using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
								using (var recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
								{

									moduleRepository.CurrentUser = recordRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = tenant.OwnerId, PreviewMode = previewMode };

									var module = await moduleRepository.GetByName("calisanlar");

									if (module == null)
										continue;

									if (!module.Fields.Any(x => x.Name == "yasi"))
										continue;

									if (!module.Fields.Any(x => x.Name == "toplam_deneyim"))
										continue;

									var findRequest = new FindRequest
									{
										Fields = new List<string> { "dogum_tarihi", "yasi", "ise_baslama_tarihi", "deneyim_yil", "deneyim_ay", "onceki_deneyim_yil", "onceki_deneyim_ay", "toplam_deneyim", "toplam_deneyim_firma" },
										Filters = null,
										Limit = 9999,
										Offset = 0
									};

									//if calisanlar_d table exists  ise_baslama_tarihi_2 column we just added findRequest
									if (module.Fields.Any(x => x.Name == "ise_baslama_tarihi_2"))
										findRequest.Fields.Add("ise_baslama_tarihi_2");

									var calisanlar = recordRepository.Find("calisanlar", findRequest, false);

									foreach (JObject calisan in calisanlar)
									{
										if (!calisan["dogum_tarihi"].IsNullOrEmpty())
										{
											var today = DateTime.Today;
											var birthDate = (DateTime)calisan["dogum_tarihi"];
											var age = today.Year - birthDate.Year;

											if (birthDate > today.AddYears(-age))
												age--;

											calisan["yasi"] = age;

											try
											{
												await recordRepository.Update(calisan, module);
											}
											catch (Exception ex)
											{
												ErrorHandler.LogError(ex, "tenant_id:" + tenant.Id + "module_name:" + module.Name);
												continue;
											}
										}

										if (!calisan["ise_baslama_tarihi_2"].IsNullOrEmpty() && calisan["deneyim_yil"].IsNullOrEmpty())
										{
											var timespan = DateTime.UtcNow.Subtract((DateTime)calisan["ise_baslama_tarihi_2"]);
											var calisanDeneyimYil = (int)Math.Floor(timespan.TotalDays / 365);

											if (calisanDeneyimYil < 0)
												calisanDeneyimYil = 0;

											calisan["deneyim_yil"] = calisanDeneyimYil;

											if ((int)calisan["deneyim_yil"] > 0)
											{
												var calisanDeneyimAy = (int)Math.Floor(timespan.TotalDays / 30) - ((int)calisan["deneyim_yil"] * 12);
												if (calisanDeneyimAy < 0)
													calisanDeneyimAy = 0;

												calisan["deneyim_ay"] = calisanDeneyimAy;
											}
											else
											{
												var calisanDeneyimAy = (int)Math.Floor(timespan.TotalDays / 30);
												if (calisanDeneyimAy < 0)
													calisanDeneyimAy = 0;
												calisan["deneyim_ay"] = calisanDeneyimAy;
											}

											var deneyimAyStr = (string)calisan["deneyim_ay"];

											calisan["toplam_deneyim_firma"] = calisan["deneyim_yil"] + "." + (deneyimAyStr.Length == 1 ? "0" + deneyimAyStr : deneyimAyStr);
											calisan["toplam_deneyim_firma_yazi"] = calisan["deneyim_yil"] + " yıl " + deneyimAyStr + " ay";

											if (calisan["onceki_deneyim_yil"].IsNullOrEmpty())
												calisan["onceki_deneyim_yil"] = 0;

											if (calisan["onceki_deneyim_ay"].IsNullOrEmpty())
												calisan["onceki_deneyim_ay"] = 0;

											var deneyimYil = (int)calisan["deneyim_yil"] + (int)calisan["onceki_deneyim_yil"];
											var deneyimAy = (int)calisan["deneyim_ay"] + (int)calisan["onceki_deneyim_ay"];

											if (deneyimAy > 12)
											{
												deneyimAy -= 12;
												deneyimYil += 1;
											}

											calisan["toplam_deneyim"] = deneyimYil + "." + (deneyimAy.ToString().Length == 1 ? "0" + deneyimAy : deneyimAy.ToString());
											calisan["toplam_deneyim_yazi"] = deneyimYil + " yıl " + deneyimAy + " ay";

											try
											{
												await recordRepository.Update(calisan, module);
											}
											catch (Exception ex)
											{
												ErrorHandler.LogError(ex, "tenant_id:" + tenant.Id + "module_name:" + module.Name);
												continue;
											}
										}
									}
								}
							}
						}
						//TODO: ex.InnerException.InnerException olabilir
						catch (DataException ex)
						{
							if (ex.InnerException is PostgresException)
							{
								var innerEx = (PostgresException)ex.InnerException;

								if (innerEx.SqlState == PostgreSqlStateCodes.DatabaseDoesNotExist)
									continue;
							}

							throw;
						}
					}
				}
			}
		}
	}
}