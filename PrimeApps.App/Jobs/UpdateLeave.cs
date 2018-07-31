using System;
using System.Collections.Generic;
using System.Data;
using PrimeApps.Model.Context;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Helpers.QueryTranslation;
using PrimeApps.Model.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace PrimeApps.App.Jobs
{
	public class UpdateLeave
	{
		private CurrentUser _currentUser;
		private ICalculationHelper _calculationHelper;
		private IConfiguration _configuration;
		private IHttpContextAccessor _context;
		private IServiceScopeFactory _serviceScopeFactory;
		public UpdateLeave(ICalculationHelper calculationHelper, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor context)
		{
			_context = context;
			_currentUser = UserHelper.GetCurrentUser(_context);
			_calculationHelper = calculationHelper;
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public async Task Update()
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				using (var _tenantRepository = new TenantRepository(platformDatabaseContext, _configuration))
				{
					var tenants = await _tenantRepository.GetAllActive();

					foreach (var tenant in tenants)
					{
						try
						{
							using (var _platformWarehouseRepository = new PlatformWarehouseRepository(platformDatabaseContext, _configuration))
							using (var _analyticRepository = new AnalyticRepository(databaseContext, _configuration))
							{
								_platformWarehouseRepository.CurrentUser = _analyticRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = tenant.OwnerId };

								var warehouse = new Model.Helpers.Warehouse(_analyticRepository, _configuration);

								var warehouseEntity = await _platformWarehouseRepository.GetByTenantId(tenant.Id);

								if (warehouseEntity != null)
									warehouse.DatabaseName = warehouseEntity.DatabaseName;
								else
									warehouse.DatabaseName = "0";

								using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
								{
									_moduleRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = tenant.OwnerId };

									var izinTurleriModule = await _moduleRepository.GetByName("izin_turleri");

									if (izinTurleriModule == null)
										continue;

									using (var _recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
									{
										_recordRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = tenant.OwnerId };

										var module = await _moduleRepository.GetByName("calisanlar") ??
													 await _moduleRepository.GetByName("human_resources");

										if (module == null)
											continue;

										var findRequestCalisan = new FindRequest
										{
											Filters = new List<Filter>
											{
												new Filter {Field = "deleted", Operator = Operator.Equals, Value = false, No = 1}
											},
											Limit = 99999,
											Offset = 0
										};

										var findRequestIzinler = new FindRequest
										{
											Filters = new List<Filter>
											{
												new Filter {Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1},
												new Filter {Field = "deleted", Operator = Operator.Equals, Value = false, No = 2}
											},
											Limit = 99999,
											Offset = 0
										};

										var calisanlar = _recordRepository.Find(module.Name, findRequestCalisan, false);

										var izinler = _recordRepository.Find("izin_turleri", findRequestIzinler, false).First;

										if (izinler.IsNullOrEmpty())
											continue;

										foreach (JObject calisan in calisanlar)
										{
											var kalanIzinHakki = 0;
											if (calisan["ise_baslama_tarihi"].IsNullOrEmpty())
												continue;

											var iseBaslamaTarihi = (string)calisan["ise_baslama_tarihi"];
											var bugun = DateTime.UtcNow;

											var calismayaBasladigiZaman = DateTime.ParseExact(iseBaslamaTarihi, "MM/dd/yyyy h:mm:ss", null);

											var izinYenilemeTarihi = new DateTime(bugun.Year, calismayaBasladigiZaman.Month, calismayaBasladigiZaman.Day);

											var dayDiff = bugun - calismayaBasladigiZaman;
											var dayDiffYear = dayDiff.Days / 365;

											if (dayDiffYear <= 0)
												continue;

											if (bugun.Date == izinYenilemeTarihi.Date)
											{
												//var devredecekIzin = (int) calisan["kalan_yillik_izin_suresi"];

												if (!calisan["kalan_izin_hakki"].IsNullOrEmpty())
												{
													kalanIzinHakki = (int)calisan["kalan_izin_hakki"];
												}

												var devredecekIzin = 0;


												var izinKurali = _recordRepository.GetById(izinTurleriModule, (int)izinler["id"], false);

												if ((bool)izinKurali["yillik_izine_ek_izin_suresi_ekle"] &&
													!izinKurali["yillik_izine_ek_izin_suresi_gun"].IsNullOrEmpty() && (int)izinKurali["yillik_izine_ek_izin_suresi_gun"] != 0 &&
													!(bool)izinKurali["ek_izin_sonraki_yillara_devreder"])
												{
													var ekIzinSuresi = (int)izinKurali["yillik_izine_ek_izin_suresi_gun"];

													if (kalanIzinHakki > ekIzinSuresi)
													{
														kalanIzinHakki = kalanIzinHakki - ekIzinSuresi;
													}
													else if (kalanIzinHakki > 0)
													{
														kalanIzinHakki = 0;
													}
												}

												if (!izinKurali["sonraki_doneme_devredilen_izin_gun"].IsNullOrEmpty())
												{
													if ((int)izinKurali["sonraki_doneme_devredilen_izin_gun"] <= kalanIzinHakki)
														devredecekIzin = (int)izinKurali["sonraki_doneme_devredilen_izin_gun"];
													else
														devredecekIzin = kalanIzinHakki;
												}
												else
													devredecekIzin = kalanIzinHakki;

												calisan["sabit_devreden_izin"] = devredecekIzin;
												calisan["devreden_izin"] = devredecekIzin;
												await _recordRepository.Update(calisan, module, isUtc: false);

												await _calculationHelper.YillikIzinHesaplama((int)calisan["id"], (int)izinler["id"], warehouse);
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