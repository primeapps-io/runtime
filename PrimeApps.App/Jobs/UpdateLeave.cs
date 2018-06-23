using System;
using System.Collections.Generic;
using System.Data;
using Hangfire;
using PrimeApps.App.Jobs.QueueAttributes;
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

namespace PrimeApps.App.Jobs
{
    public class UpdateLeave
    {
        private IConfiguration _configuration;

        public UpdateLeave(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [CommonQueue, DisableConcurrentExecution(360)]
        public async Task Update()
        {
            using (var platformDatabaseContext = new PlatformDBContext())
            using (var tenantRepository = new TenantRepository(platformDatabaseContext))
            using (var platformWarehouseRepository = new PlatformWarehouseRepository(platformDatabaseContext))
            {
                var tenants = await tenantRepository.GetAllActive();

                foreach (var tenant in tenants)
                {
                    try
                    {
                        using (var databaseContext = new TenantDBContext(tenant.Id))
                        {
                            using (var analyticRepository = new AnalyticRepository(databaseContext, _configuration))
                            {
                                var warehouse = new Model.Helpers.Warehouse(analyticRepository, _configuration);

                                var warehouseEntity = await platformWarehouseRepository.GetByTenantId(tenant.Id);

                                if (warehouseEntity != null)
                                    warehouse.DatabaseName = warehouseEntity.DatabaseName;
                                else
                                    warehouse.DatabaseName = "0";

                                using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
                                {
                                    var izinTurleriModule = await moduleRepository.GetByName("izin_turleri");

                                    if (izinTurleriModule == null)
                                        continue;

                                    using (var recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
                                    {
                                        var module = await moduleRepository.GetByName("calisanlar") ??
                                                     await moduleRepository.GetByName("human_resources");

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

                                        var calisanlar = recordRepository.Find(module.Name, findRequestCalisan, false);

                                        var izinler = recordRepository.Find("izin_turleri", findRequestIzinler, false).First;

                                        if (izinler.IsNullOrEmpty())
                                            continue;

                                        foreach (JObject calisan in calisanlar)
                                        {
                                            var kalanIzinHakki = 0;
                                            if (calisan["ise_baslama_tarihi"].IsNullOrEmpty())
                                                continue;

                                            var iseBaslamaTarihi = (string) calisan["ise_baslama_tarihi"];
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
                                                    kalanIzinHakki = (int) calisan["kalan_izin_hakki"];
                                                }

                                                var devredecekIzin = 0;


                                                var izinKurali = recordRepository.GetById(izinTurleriModule, (int) izinler["id"], false);

                                                if ((bool) izinKurali["yillik_izine_ek_izin_suresi_ekle"] &&
                                                    !izinKurali["yillik_izine_ek_izin_suresi_gun"].IsNullOrEmpty() && (int) izinKurali["yillik_izine_ek_izin_suresi_gun"] != 0 &&
                                                    !(bool) izinKurali["ek_izin_sonraki_yillara_devreder"])
                                                {
                                                    var ekIzinSuresi = (int) izinKurali["yillik_izine_ek_izin_suresi_gun"];

                                                    if (kalanIzinHakki > ekIzinSuresi)
                                                    {
                                                        kalanIzinHakki = kalanIzinHakki - ekIzinSuresi;
                                                    }
                                                    else if (kalanIzinHakki > 0)
                                                    {
                                                        kalanIzinHakki = 0;
                                                    }
                                                }

                                                if (!izinKurali["sonraki_doneme_devredilen_izin_gun"].IsNullOrEmpty() &&
                                                    (int) izinKurali["sonraki_doneme_devredilen_izin_gun"] <= kalanIzinHakki)
                                                {
                                                    devredecekIzin = (int) izinKurali["sonraki_doneme_devredilen_izin_gun"];
                                                }

                                                calisan["sabit_devreden_izin"] = devredecekIzin;
                                                calisan["devreden_izin"] = devredecekIzin;
                                                await recordRepository.Update(calisan, module);

                                                await CalculationHelper.YillikIzinHesaplama((int) calisan["id"], (int) izinler["id"], recordRepository, moduleRepository);
                                            }
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
                            var innerEx = (PostgresException) ex.InnerException;
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