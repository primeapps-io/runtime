﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Helpers
{

	public interface ICalculationHelper
	{
		Task Calculate(int recordId, Module module, UserItem appUser, Warehouse warehouse, OperationType operationType, BeforeCreateUpdate BeforeCreateUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest);
		Task<bool> YillikIzinHesaplama(int userId, int izinTuruId);
		Task<bool> DeleteAnnualLeave(int userId, int izinTuruId, JObject record);
		Task<bool> CalculateTimesheet(JArray timesheetItemsRecords, UserItem appUser, Module timesheetItemModule, Module timesheetModule);
		Task<decimal> CalculateAccountBalance(JObject record, string currency, UserItem appUser, Module currentAccountModule, Picklist currencyPicklistSalesInvoice, Module module);
		Task<decimal> CalculateSupplierBalance(JObject record, string currency, UserItem appUser, Module currentAccountModule, Picklist currencyPicklistPurchaseInvoice, Module module);
		Task<decimal> CalculateKasaBalance(JObject record, Picklist hareketTipleri, UserItem appUser, Module kasaHareketiModule);
		Task<decimal> CalculateBankaBalance(JObject record, Picklist hareketTipleri, UserItem appUser, Module bankaHareketiModule);
		Task<decimal> CalculateStock(JObject record, UserItem appUser, Module stockTransactionModule);
	}

	public class CalculationHelper : ICalculationHelper
	{
		private IServiceScopeFactory _serviceScopeFactory;
		private IHttpContextAccessor _context;
		private IConfiguration _configuration;
		private CurrentUser _currentUser;
		public CalculationHelper(IHttpContextAccessor context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
		{
			_context = context;
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
			_currentUser = UserHelper.GetCurrentUser(_context);
		}

		public async Task Calculate(int recordId, Module module, UserItem appUser, Warehouse warehouse, OperationType operationType, BeforeCreateUpdate BeforeCreateUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest)
		{
			try
			{
				using (var _scope = _serviceScopeFactory.CreateScope())
				{
					var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
					using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
					{
						using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
						{
							using (var recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
							{
								moduleRepository.UserId = appUser.TenantId;
								recordRepository.UserId = appUser.TenantId;
								picklistRepository.UserId = appUser.TenantId;

								moduleRepository.CurrentUser = recordRepository.CurrentUser = picklistRepository.CurrentUser = _currentUser;

								warehouse.DatabaseName = appUser.WarehouseDatabaseName;
								var record = recordRepository.GetById(module, recordId, true, null, true);

								switch (module.Name)
								{
									case "sirket_ici_kariyer":
										var calisanlarModule = await moduleRepository.GetByName("calisanlar");
										var lookupModules = new List<Module>();
										lookupModules.Add(calisanlarModule);
										lookupModules.Add(Model.Helpers.ModuleHelper.GetFakeUserModule());

										record = recordRepository.GetById(module, recordId, true, lookupModules, true);
										var calisan = recordRepository.GetById(calisanlarModule, (int)record["personel.id"], true, lookupModules, true);

										var calisanUpdate = new JObject();
										calisanUpdate["id"] = calisan["id"];

										if (!record["gorev_yeri"].IsNullOrEmpty())
											calisanUpdate["lokasyon"] = record["gorev_yeri"];

										if (!record["bolum"].IsNullOrEmpty())
											calisanUpdate["departman"] = record["bolum"];

										if (!record["is_alani"].IsNullOrEmpty())
											calisanUpdate["is_alani"] = record["is_alani"];

										if (!record["unvan"].IsNullOrEmpty())
											calisanUpdate["unvan"] = record["unvan"];

										if (!record["1yoneticisi.id"].IsNullOrEmpty())
											calisanUpdate["yoneticisi"] = record["1yoneticisi.id"];

										if (!record["2_yoneticisi.id"].IsNullOrEmpty())
											calisanUpdate["2_yonetici"] = record["2_yoneticisi.id"];

										if (!record["direktor.id"].IsNullOrEmpty())
											calisanUpdate["direktor"] = record["direktor.id"];

										if (!record["gmy.id"].IsNullOrEmpty())
											calisanUpdate["gmy"] = record["gmy.id"];

										await recordRepository.Update(calisanUpdate, calisanlarModule);

										string mailSubject;
										string mailBody;

										using (var templateRepostory = new TemplateRepository(databaseContext, _configuration))
										{
											var mailTemplate = await templateRepostory.GetById(48);//Organizasyonel değişiklik bildirimi
											mailSubject = mailTemplate.Subject;
											mailBody = mailTemplate.Content;
										}

										var ccList = new List<string>();
										ccList.Add("hr@etiya.com");

										if (!record["1yoneticisi.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)record["1yoneticisi.e_posta"]))
											ccList.Add((string)record["1yoneticisi.e_posta"]);

										if (!record["2_yoneticisi.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)record["2_yoneticisi.e_posta"]))
											ccList.Add((string)record["2_yoneticisi.e_posta"]);

										if (!record["direktor.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)record["direktor.e_posta"]))
											ccList.Add((string)record["direktor.e_posta"]);

										if (!record["gmy.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)record["gmy.e_posta"]))
											ccList.Add((string)record["gmy.e_posta"]);

										if (!calisan["yoneticisi.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)calisan["yoneticisi.e_posta"]))
											ccList.Add((string)calisan["yoneticisi.e_posta"]);

										if (!calisan["2_yonetici.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)calisan["2_yonetici.e_posta"]))
											ccList.Add((string)calisan["2_yonetici.e_posta"]);

										if (!calisan["direktor.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)calisan["direktor.e_posta"]))
											ccList.Add((string)calisan["direktor.e_posta"]);

										if (!calisan["gmy.e_posta"].IsNullOrEmpty() && !ccList.Contains((string)calisan["gmy.e_posta"]))
											ccList.Add((string)calisan["gmy.e_posta"]);

										//TODO Removed
										/*using (var session = Provider.SessionFactory.OpenSession())
                                        {
                                            using (var transaction = session.BeginTransaction())
                                            {
                                                var externalEmail = new Email(mailSubject, mailBody);
                                                externalEmail.AddRecipient("finans@etiya.com");
                                                externalEmail.AddToQueue(appUser.TenantId, appUser: appUser, cc: string.Join(",", ccList), moduleId: module.Id, recordId: (int)record["id"], addRecordSummary: false);

                                                transaction.Commit();
                                            }
                                        }*/
										break;
									case "ise_alim_talepleri":
										var iseAlimTalebiModule = await moduleRepository.GetByName("ise_alim_talepleri");
										var userRequest = new FindRequest { Fields = new List<string> { "email" }, Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)record["owner"], No = 1 } }, Limit = 1 };
										var userData = recordRepository.Find("users", userRequest, false);
										var calisanRequest = new FindRequest();
										var calisanData = new JArray();
										var calisanObj = new JObject();
										if (!userData.IsNullOrEmpty())
										{
											calisanRequest = new FindRequest { Fields = new List<string> { "yoneticisi.calisanlar.e_posta" }, Filters = new List<Filter> { new Filter { Field = "e_posta", Operator = Operator.Is, Value = userData.First()["email"], No = 1 } }, Limit = 1 };
											calisanData = recordRepository.Find("calisanlar", calisanRequest, false);
											calisanObj = (JObject)calisanData.First();
											var moduleCalisan = await moduleRepository.GetByName("calisanlar");
											var departmanPicklist = moduleCalisan.Fields.Single(x => x.Name == "departman");
											var departmanPicklistItem = await picklistRepository.FindItemByLabel(departmanPicklist.PicklistId.Value, (string)record["bolum"], appUser.TenantLanguage);

											if (!calisanObj["yoneticisi.calisanlar.e_posta"].IsNullOrEmpty())
											{
												record["custom_approver"] = calisanObj["yoneticisi.calisanlar.e_posta"];
												int j = 1;
												for (var i = 2; i < 6; i++)
												{
													if (!calisanObj["yoneticisi.calisanlar.e_posta"].IsNullOrEmpty())
													{
														j++;
														calisanRequest = new FindRequest { Fields = new List<string> { "yoneticisi.calisanlar.e_posta" }, Filters = new List<Filter> { new Filter { Field = "e_posta", Operator = Operator.Is, Value = (string)calisanObj["yoneticisi.calisanlar.e_posta"], No = 1 }, new Filter { Field = "calisma_durumu", Operator = Operator.Is, Value = "Aktif", No = 2 } }, Limit = 1 };
														calisanData = recordRepository.Find("calisanlar", calisanRequest, false);
														calisanObj = (JObject)calisanData.First();

														if (!calisanObj["yoneticisi.calisanlar.e_posta"].IsNullOrEmpty())
															record["custom_approver_" + i] = calisanObj["yoneticisi.calisanlar.e_posta"];
														else
															j--;
													}
												}
												if (departmanPicklistItem != null && departmanPicklistItem?.Value != "ceo_approve")
													record["custom_approver_" + j] = null;
											}
										}
										await recordRepository.Update(record, iseAlimTalebiModule);
										break;
									case "sales_invoices":
										var salesInvoiceModule = await moduleRepository.GetByName("sales_invoices");
										var accountModule = await moduleRepository.GetByName("accounts");
										var salesInvoiceStagePicklist = salesInvoiceModule.Fields.Single(x => x.Name == "asama");
										var salesInvoiceStagePicklistItem = await picklistRepository.FindItemByLabel(salesInvoiceStagePicklist.PicklistId.Value, (string)record["asama"], appUser.TenantLanguage);
										var currentAccountModule = await moduleRepository.GetByName("current_accounts");
										var findRequestCurrentAccountRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "satis_faturasi", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
										var currentAccountRecord = recordRepository.Find("current_accounts", findRequestCurrentAccountRecord);

										//Firma Obj
										var recordAccount = new JObject();
										decimal recordAccountBalance;

										//para birimini bulma
										var currencyFieldSalesInvoice = salesInvoiceModule.Fields.Single(x => x.Name == "currency");
										var currencyPicklistSalesInvoice = await picklistRepository.GetById(currencyFieldSalesInvoice.PicklistId.Value);
										var currencySalesInvoice = currencyPicklistSalesInvoice.Items.Single(x => appUser.TenantLanguage == "tr" ? x.LabelTr == (string)record["currency"] : x.LabelEn == (string)record["currency"]).SystemCode;

										//Sipariş dönüştürmede otomaik oluşturulan veya manuel eklenen satış faturası
										if (salesInvoiceStagePicklistItem.SystemCode == "onaylandi" && operationType != OperationType.delete)
										{
											var recordCurrentAccount = new JObject();

											//create ya da update için cari hesabın alanlarını oluşturma ya da güncelleme
											recordCurrentAccount["owner"] = record["owner"];
											recordCurrentAccount["currency"] = record["currency"];
											recordCurrentAccount["customer"] = record["account"];
											recordCurrentAccount["satis_faturasi"] = record["id"];
											recordCurrentAccount["date"] = record["fatura_tarihi"];
											var transactionField = currentAccountModule.Fields.Single(x => x.Name == "transaction_type");
											var transactionTypes = await picklistRepository.GetById(transactionField.PicklistId.Value);
											recordCurrentAccount["transaction_type"] = appUser.TenantLanguage == "tr" ? transactionTypes.Items.Single(x => x.SystemCode == "sales_invoice").LabelTr : transactionTypes.Items.Single(x => x.SystemCode == "sales_invoice").LabelEn;
											recordCurrentAccount["transaction_type_system"] = "sales_invoice";

											//para birimine göre satış faturasını oluşturma
											switch (currencySalesInvoice)
											{
												case "try":
													recordCurrentAccount["borc_tl"] = record["grand_total"];
													recordCurrentAccount["bakiye_tl"] = 0;

													if (currentAccountRecord.IsNullOrEmpty())
														await recordRepository.Create(recordCurrentAccount, currentAccountModule);
													else
													{
														recordCurrentAccount["id"] = currentAccountRecord.First()["id"];
														await recordRepository.Update(recordCurrentAccount, currentAccountModule);
													}

													recordAccountBalance = await CalculateAccountBalance(record, currencySalesInvoice, appUser, currentAccountModule, currencyPicklistSalesInvoice, module);
													recordAccount["balance"] = recordAccountBalance;

													break;
												case "eur":
													recordCurrentAccount["borc_euro"] = record["grand_total"];
													recordCurrentAccount["bakiye_euro"] = 0;

													if (currentAccountRecord.IsNullOrEmpty())
														await recordRepository.Create(recordCurrentAccount, currentAccountModule);
													else
													{
														recordCurrentAccount["id"] = currentAccountRecord.First()["id"];
														await recordRepository.Update(recordCurrentAccount, currentAccountModule);
													}

													recordAccountBalance = await CalculateAccountBalance(record, currencySalesInvoice, appUser, currentAccountModule, currencyPicklistSalesInvoice, module);
													recordAccount["bakiye_eur"] = recordAccountBalance;

													break;
												case "usd":
													recordCurrentAccount["borc_usd"] = record["grand_total"];
													recordCurrentAccount["bakiye_usd"] = 0;

													if (currentAccountRecord.IsNullOrEmpty())
														await recordRepository.Create(recordCurrentAccount, currentAccountModule);
													else
													{
														recordCurrentAccount["id"] = currentAccountRecord.First()["id"];
														await recordRepository.Update(recordCurrentAccount, currentAccountModule);
													}

													recordAccountBalance = await CalculateAccountBalance(record, currencySalesInvoice, appUser, currentAccountModule, currencyPicklistSalesInvoice, module);
													recordAccount["bakiye_usd"] = recordAccountBalance;

													break;
											}
										}
										else if (salesInvoiceStagePicklistItem.SystemCode == "iptal_edildi" || operationType == OperationType.delete)
										{
											if (!currentAccountRecord.IsNullOrEmpty())
											{
												var currentAccountRecordObj = (JObject)currentAccountRecord.First();
												await recordRepository.Delete(currentAccountRecordObj, currentAccountModule);

												switch (currencySalesInvoice)
												{
													case "try":
														recordAccountBalance = await CalculateAccountBalance(record, currencySalesInvoice, appUser, currentAccountModule, currencyPicklistSalesInvoice, module);
														recordAccount["balance"] = recordAccountBalance;
														break;
													case "eur":
														recordAccountBalance = await CalculateAccountBalance(record, currencySalesInvoice, appUser, currentAccountModule, currencyPicklistSalesInvoice, module);
														recordAccount["bakiye_eur"] = recordAccountBalance;
														break;
													case "usd":
														recordAccountBalance = await CalculateAccountBalance(record, currencySalesInvoice, appUser, currentAccountModule, currencyPicklistSalesInvoice, module);
														recordAccount["bakiye_usd"] = recordAccountBalance;
														break;
												}
											}
										}

										//firmanın balance'ını güncelleme
										recordAccount["id"] = record["account"];
										await recordRepository.Update(recordAccount, accountModule);

										break;
									case "purchase_invoices":
										var purchaseInvoiceModule = await moduleRepository.GetByName("purchase_invoices");
										var supplierModule = await moduleRepository.GetByName("suppliers");
										var purchaseInvoiceStagePicklist = purchaseInvoiceModule.Fields.Single(x => x.Name == "asama");
										var purchaseInvoiceStagePicklistItem = await picklistRepository.FindItemByLabel(purchaseInvoiceStagePicklist.PicklistId.Value, (string)record["asama"], appUser.TenantLanguage);
										var currentSupplierModule = await moduleRepository.GetByName("current_accounts");
										var findRequestCurrentAccountRecordForPurchase = new FindRequest { Filters = new List<Filter> { new Filter { Field = "alis_faturasi", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
										var currentAccountRecordForPurchase = recordRepository.Find("current_accounts", findRequestCurrentAccountRecordForPurchase);

										//Firma Obj
										var recordSupplier = new JObject();
										decimal recordSupplierBalance;

										//para birimini bulma
										var currencyFieldPurchaseInvoice = purchaseInvoiceModule.Fields.Single(x => x.Name == "currency");
										var currencyPicklistPurchaseInvoice = await picklistRepository.GetById(currencyFieldPurchaseInvoice.PicklistId.Value);
										var currencyPurchaseInvoice = currencyPicklistPurchaseInvoice.Items.Single(x => appUser.TenantLanguage == "tr" ? x.LabelTr == (string)record["currency"] : x.LabelEn == (string)record["currency"]).SystemCode;

										//Sipariş dönüştürmede otomaik oluşturulan veya manuel eklenen satış faturası
										if (purchaseInvoiceStagePicklistItem.SystemCode == "onaylandi" && operationType != OperationType.delete)
										{
											var recordCurrentAccount = new JObject();

											//create ya da update için cari hesabın alanlarını oluşturma ya da güncelleme
											recordCurrentAccount["owner"] = record["owner"];
											recordCurrentAccount["currency"] = record["fatura_para_birimi"];
											recordCurrentAccount["supplier"] = record["tedarikci"];
											recordCurrentAccount["alis_faturasi"] = record["id"];
											recordCurrentAccount["date"] = record["fatura_tarihi"];
											var transactionField = currentSupplierModule.Fields.Single(x => x.Name == "transaction_type");
											var transactionTypes = await picklistRepository.GetById(transactionField.PicklistId.Value);
											recordCurrentAccount["transaction_type"] = appUser.TenantLanguage == "tr" ? transactionTypes.Items.Single(x => x.SystemCode == "purchase_invoice").LabelTr : transactionTypes.Items.Single(x => x.SystemCode == "purchase_invoice").LabelEn;
											recordCurrentAccount["transaction_type_system"] = "purchase_invoice";

											//para birimine göre satış faturasını oluşturma
											switch (currencyPurchaseInvoice)
											{
												case "try":
													recordCurrentAccount["alacak"] = record["grand_total"];
													recordCurrentAccount["bakiye_tl"] = 0;

													if (currentAccountRecordForPurchase.IsNullOrEmpty())
														await recordRepository.Create(recordCurrentAccount, currentSupplierModule);
													else
													{
														recordCurrentAccount["id"] = currentAccountRecordForPurchase.First()["id"];
														await recordRepository.Update(recordCurrentAccount, currentSupplierModule);
													}

													recordSupplierBalance = await CalculateSupplierBalance(record, currencyPurchaseInvoice, appUser, currentSupplierModule, currencyPicklistPurchaseInvoice, module);
													recordSupplier["balance"] = recordSupplierBalance;

													break;
												case "eur":
													recordCurrentAccount["alacak_euro"] = record["grand_total"];
													recordCurrentAccount["bakiye_euro"] = 0;

													if (currentAccountRecordForPurchase.IsNullOrEmpty())
														await recordRepository.Create(recordCurrentAccount, currentSupplierModule);
													else
													{
														recordCurrentAccount["id"] = currentAccountRecordForPurchase.First()["id"];
														await recordRepository.Update(recordCurrentAccount, currentSupplierModule);
													}

													recordSupplierBalance = await CalculateSupplierBalance(record, currencyPurchaseInvoice, appUser, currentSupplierModule, currencyPicklistPurchaseInvoice, module);
													recordSupplier["bakiye_euro"] = recordSupplierBalance;

													break;
												case "usd":
													recordCurrentAccount["alacak_usd"] = record["grand_total"];
													recordCurrentAccount["bakiye_usd"] = 0;

													if (currentAccountRecordForPurchase.IsNullOrEmpty())
														await recordRepository.Create(recordCurrentAccount, currentSupplierModule);
													else
													{
														recordCurrentAccount["id"] = currentAccountRecordForPurchase.First()["id"];
														await recordRepository.Update(recordCurrentAccount, currentSupplierModule);
													}

													recordSupplierBalance = await CalculateSupplierBalance(record, currencyPurchaseInvoice, appUser, currentSupplierModule, currencyPicklistPurchaseInvoice, module);
													recordSupplier["bakiye_usd"] = recordSupplierBalance;

													break;
											}
										}
										else if (purchaseInvoiceStagePicklistItem.SystemCode == "iptal_edildi" || operationType == OperationType.delete)
										{
											if (!currentAccountRecordForPurchase.IsNullOrEmpty())
											{
												var currentAccountRecordObj = (JObject)currentAccountRecordForPurchase.First();
												await recordRepository.Delete(currentAccountRecordObj, currentSupplierModule);

												switch (currencyPurchaseInvoice)
												{
													case "try":
														recordSupplierBalance = await CalculateSupplierBalance(record, currencyPurchaseInvoice, appUser, currentSupplierModule, currencyPicklistPurchaseInvoice, module);
														recordSupplier["balance"] = recordSupplierBalance;
														break;
													case "eur":
														recordSupplierBalance = await CalculateSupplierBalance(record, currencyPurchaseInvoice, appUser, currentSupplierModule, currencyPicklistPurchaseInvoice, module);
														recordSupplier["bakiye_euro"] = recordSupplierBalance;
														break;
													case "usd":
														recordSupplierBalance = await CalculateSupplierBalance(record, currencyPurchaseInvoice, appUser, currentSupplierModule, currencyPicklistPurchaseInvoice, module);
														recordSupplier["bakiye_usd"] = recordSupplierBalance;
														break;
												}
											}
										}

										//firmanın balance'ını güncelleme
										recordSupplier["id"] = record["tedarikci"];
										await recordRepository.Update(recordSupplier, supplierModule);

										break;

									case "current_accounts":
										var currentAccountModuleObj = await moduleRepository.GetByName("current_accounts");
										var currencyFieldCurrentAccount = currentAccountModuleObj.Fields.Single(x => x.Name == "currency");
										var currencyPicklistCurrentAccount = await picklistRepository.GetById(currencyFieldCurrentAccount.PicklistId.Value);
										var currencyCurrentAccount = currencyPicklistCurrentAccount.Items.Single(x => appUser.TenantLanguage == "tr" ? x.LabelTr == (string)record["currency"] : x.LabelEn == (string)record["currency"]).SystemCode;

										//tahsilat
										if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
										{
											//Firma Obj
											var parentAccountRecord = new JObject();
											decimal parentAccountRecordBalance;


											var parentAccountModule = await moduleRepository.GetByName("accounts");

											switch (currencyCurrentAccount)
											{
												case "try":
													parentAccountRecordBalance = await CalculateAccountBalance(record, currencyCurrentAccount, appUser, currentAccountModuleObj, currencyPicklistCurrentAccount, module);
													parentAccountRecord["balance"] = parentAccountRecordBalance;
													break;
												case "eur":
													parentAccountRecordBalance = await CalculateAccountBalance(record, currencyCurrentAccount, appUser, currentAccountModuleObj, currencyPicklistCurrentAccount, module);
													parentAccountRecord["bakiye_eur"] = parentAccountRecordBalance;
													break;
												case "usd":
													parentAccountRecordBalance = await CalculateAccountBalance(record, currencyCurrentAccount, appUser, currentAccountModuleObj, currencyPicklistCurrentAccount, module);
													parentAccountRecord["bakiye_usd"] = parentAccountRecordBalance;
													break;
											}

											//firmanın balance'ını güncelleme
											parentAccountRecord["id"] = record["customer"];
											await recordRepository.Update(parentAccountRecord, parentAccountModule);
										}
										//ödeme
										else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
										{
											//Tedarikci Obj
											var parentSupplierRecord = new JObject();
											decimal parentSupplierRecordBalance;

											var parentSupplierModule = await moduleRepository.GetByName("suppliers");

											switch (currencyCurrentAccount)
											{
												case "try":
													parentSupplierRecordBalance = await CalculateSupplierBalance(record, currencyCurrentAccount, appUser, currentAccountModuleObj, currencyPicklistCurrentAccount, module);
													parentSupplierRecord["balance"] = parentSupplierRecordBalance;
													break;
												case "eur":
													parentSupplierRecordBalance = await CalculateSupplierBalance(record, currencyCurrentAccount, appUser, currentAccountModuleObj, currencyPicklistCurrentAccount, module);
													parentSupplierRecord["bakiye_euro"] = parentSupplierRecordBalance;
													break;
												case "usd":
													parentSupplierRecordBalance = await CalculateSupplierBalance(record, currencyCurrentAccount, appUser, currentAccountModuleObj, currencyPicklistCurrentAccount, module);
													parentSupplierRecord["bakiye_usd"] = parentSupplierRecordBalance;
													break;
											}

											//Tedarikcinin balance'ını güncelleme
											parentSupplierRecord["id"] = record["supplier"];
											await recordRepository.Update(parentSupplierRecord, parentSupplierModule);
										}

										//oto kasa hareketi ekleme, güncelleme ya da silme
										if (operationType != OperationType.delete)
										{
											if (!record["kasa"].IsNullOrEmpty())
											{
												var kasaHareketiModule = await moduleRepository.GetByName("kasa_hareketleri");
												var kasaModule = await moduleRepository.GetByName("kasalar");
												var findRequestKasaHareketi = new FindRequest { Filters = new List<Filter> { new Filter { Field = "ilgili_cari_hareket", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
												var currentKasaHareketiRecord = recordRepository.Find("kasa_hareketleri", findRequestKasaHareketi);
												var kasaHareketiRecord = new JObject();
												kasaHareketiRecord["owner"] = record["owner"];
												kasaHareketiRecord["islem_tarihi"] = record["date"];
												kasaHareketiRecord["aciklama"] = record["description"];

												kasaHareketiRecord["ilgili_cari_hareket"] = record["id"];
												kasaHareketiRecord["kasa"] = record["kasa"];
												switch (currencyCurrentAccount)
												{
													case "try":
														if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
															kasaHareketiRecord["borc"] = record["alacak"];
														else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
															kasaHareketiRecord["alacak"] = record["borc_tl"];
														break;
													case "eur":
														if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
															kasaHareketiRecord["borc"] = record["alacak_euro"];
														else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
															kasaHareketiRecord["alacak"] = record["borc_euro"];
														break;
													case "usd":
														if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
															kasaHareketiRecord["borc"] = record["alacak_usd"];
														else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
															kasaHareketiRecord["alacak"] = record["borc_usd"];
														break;
												}

												var hareketTipiField = kasaHareketiModule.Fields.Single(x => x.Name == "hareket_tipi");
												var hareketTipleri = await picklistRepository.GetById(hareketTipiField.PicklistId.Value);

												if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
													kasaHareketiRecord["hareket_tipi"] = appUser.TenantLanguage == "tr" ? hareketTipleri.Items.Single(x => x.SystemCode == "para_girisi").LabelTr : hareketTipleri.Items.Single(x => x.SystemCode == "para_girisi").LabelEn;
												else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
													kasaHareketiRecord["hareket_tipi"] = appUser.TenantLanguage == "tr" ? hareketTipleri.Items.Single(x => x.SystemCode == "para_cikisi").LabelTr : hareketTipleri.Items.Single(x => x.SystemCode == "para_cikisi").LabelEn;


												if (!currentKasaHareketiRecord.IsNullOrEmpty())
												{
													kasaHareketiRecord["id"] = currentKasaHareketiRecord.First()["id"];
													await recordRepository.Update(kasaHareketiRecord, kasaHareketiModule);
												}
												else
													await recordRepository.Create(kasaHareketiRecord, kasaHareketiModule);

												//kasa hareketlerinin ve ana kasanın bakiyesini güncelleme
												decimal kasaBalance = await CalculateKasaBalance(record, hareketTipleri, appUser, kasaHareketiModule);
												var kasaRecord = new JObject();
												kasaRecord["id"] = record["kasa"];
												kasaRecord["guncel_bakiye"] = kasaBalance;
												await recordRepository.Update(kasaRecord, kasaModule);
											}
											else if (!record["banka"].IsNullOrEmpty())
											{
												var bankaHareketiModule = await moduleRepository.GetByName("banka_hareketleri");
												var bankaModule = await moduleRepository.GetByName("bankalar");
												var findRequestBankaHareketi = new FindRequest { Filters = new List<Filter> { new Filter { Field = "ilgili_cari_hareket", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
												var currentBankaHareketiRecord = recordRepository.Find("banka_hareketleri", findRequestBankaHareketi);
												var bankaHareketiRecord = new JObject();
												bankaHareketiRecord["owner"] = record["owner"];
												bankaHareketiRecord["islem_tarihi"] = record["date"];
												bankaHareketiRecord["aciklama"] = record["description"];
												bankaHareketiRecord["ilgili_cari_hareket"] = record["id"];
												bankaHareketiRecord["banka"] = record["banka"];
												switch (currencyCurrentAccount)
												{
													case "try":
														if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
															bankaHareketiRecord["borc"] = record["alacak"];
														else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
															bankaHareketiRecord["alacak"] = record["borc_tl"];
														break;
													case "eur":
														if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
															bankaHareketiRecord["borc"] = record["alacak_euro"];
														else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
															bankaHareketiRecord["alacak"] = record["borc_euro"];
														break;
													case "usd":
														if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
															bankaHareketiRecord["borc"] = record["alacak_usd"];
														else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
															bankaHareketiRecord["alacak"] = record["borc_usd"];
														break;
												}

												var hareketTipiField = bankaHareketiModule.Fields.Single(x => x.Name == "hareket_tipi");
												var hareketTipleri = await picklistRepository.GetById(hareketTipiField.PicklistId.Value);
												if (!record["customer"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "collection")
													bankaHareketiRecord["hareket_tipi"] = appUser.TenantLanguage == "tr" ? hareketTipleri.Items.Single(x => x.SystemCode == "para_girisi").LabelTr : hareketTipleri.Items.Single(x => x.SystemCode == "para_girisi").LabelEn;
												else if (!record["supplier"].IsNullOrEmpty() && (string)record["transaction_type_system"] == "payment")
													bankaHareketiRecord["hareket_tipi"] = appUser.TenantLanguage == "tr" ? hareketTipleri.Items.Single(x => x.SystemCode == "para_cikisi").LabelTr : hareketTipleri.Items.Single(x => x.SystemCode == "para_cikisi").LabelEn;

												if (!currentBankaHareketiRecord.IsNullOrEmpty())
												{
													bankaHareketiRecord["id"] = currentBankaHareketiRecord.First()["id"];
													await recordRepository.Update(bankaHareketiRecord, bankaHareketiModule);
												}
												else
													await recordRepository.Create(bankaHareketiRecord, bankaHareketiModule);

												//banka hareketlerinin ve ana bankanın bakiyesini güncelleme
												decimal bankaBalance = await CalculateBankaBalance(record, hareketTipleri, appUser, bankaHareketiModule);
												var bankaRecord = new JObject();
												bankaRecord["id"] = record["banka"];
												bankaRecord["guncel_bakiye"] = bankaBalance;
												await recordRepository.Update(bankaRecord, bankaModule);
											}
										}
										else
										{
											if (!record["kasa"].IsNullOrEmpty())
											{
												var kasaHareketiModule = await moduleRepository.GetByName("kasa_hareketleri");
												var kasaModule = await moduleRepository.GetByName("kasalar");
												var findRequestKasaHareketi = new FindRequest { Filters = new List<Filter> { new Filter { Field = "ilgili_cari_hareket", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
												var currentKasaHareketiRecord = recordRepository.Find("kasa_hareketleri", findRequestKasaHareketi);

												if (!currentKasaHareketiRecord.IsNullOrEmpty())
												{
													var hareketTipiField = kasaHareketiModule.Fields.Single(x => x.Name == "hareket_tipi");
													var hareketTipleri = await picklistRepository.GetById(hareketTipiField.PicklistId.Value);
													var kasaHareketiObj = (JObject)currentKasaHareketiRecord.First();
													await recordRepository.Delete(kasaHareketiObj, kasaHareketiModule);

													//kasa hareketlerinin ve ana kasanın bakiyesini güncelleme
													decimal kasaBalance = await CalculateKasaBalance(record, hareketTipleri, appUser, kasaHareketiModule);
													var kasaRecord = new JObject();
													kasaRecord["id"] = record["kasa"];
													kasaRecord["guncel_bakiye"] = kasaBalance;
													await recordRepository.Update(kasaRecord, kasaModule);
												}
											}
											else if (!record["banka"].IsNullOrEmpty())
											{
												var bankaHareketiModule = await moduleRepository.GetByName("banka_hareketleri");
												var bankaModule = await moduleRepository.GetByName("bankalar");
												var findRequestBankaHareketi = new FindRequest { Filters = new List<Filter> { new Filter { Field = "ilgili_cari_hareket", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
												var currentBankaHareketiRecord = recordRepository.Find("kasa_hareketleri", findRequestBankaHareketi);

												if (!currentBankaHareketiRecord.IsNullOrEmpty())
												{
													var hareketTipiField = bankaHareketiModule.Fields.Single(x => x.Name == "hareket_tipi");
													var hareketTipleri = await picklistRepository.GetById(hareketTipiField.PicklistId.Value);
													var bankaHareketiObj = (JObject)currentBankaHareketiRecord.First();
													await recordRepository.Delete(bankaHareketiObj, bankaHareketiModule);

													//banka hareketlerinin ve ana kasanın bakiyesini güncelleme
													decimal bankaBalance = await CalculateBankaBalance(record, hareketTipleri, appUser, bankaHareketiModule);
													var bankaRecord = new JObject();
													bankaRecord["id"] = record["banka"];
													bankaRecord["guncel_bakiye"] = bankaBalance;
													await recordRepository.Update(bankaRecord, bankaModule);
												}
											}
										}

										break;
									case "kasa_hareketleri":
										var kasaHareketleriModule = await moduleRepository.GetByName("kasa_hareketleri");
										var moduleKasa = await moduleRepository.GetByName("kasalar");
										var kasaHareketTipiField = kasaHareketleriModule.Fields.Single(x => x.Name == "hareket_tipi");
										var kasaHareketTipleri = await picklistRepository.GetById(kasaHareketTipiField.PicklistId.Value);

										var recordKasa = new JObject();
										decimal kasaRecordBalance;

										kasaRecordBalance = await CalculateKasaBalance(record, kasaHareketTipleri, appUser, kasaHareketleriModule);
										recordKasa["guncel_bakiye"] = kasaRecordBalance;

										//bankanın balance'ını güncelleme
										recordKasa["id"] = record["kasa"];
										await recordRepository.Update(recordKasa, moduleKasa);

										break;
									case "banka_hareketleri":
										var bankaHareketleriModule = await moduleRepository.GetByName("banka_hareketleri");
										var bankModule = await moduleRepository.GetByName("bankalar");
										var bankaHareketTipiField = bankaHareketleriModule.Fields.Single(x => x.Name == "hareket_tipi");
										var bankaHareketTipleri = await picklistRepository.GetById(bankaHareketTipiField.PicklistId.Value);

										var bankRecord = new JObject();
										decimal bankaRecordBalance;

										bankaRecordBalance = await CalculateBankaBalance(record, bankaHareketTipleri, appUser, bankaHareketleriModule);
										bankRecord["guncel_bakiye"] = bankaRecordBalance;

										//bankanın balance'ını güncelleme
										bankRecord["id"] = record["banka"];
										await recordRepository.Update(bankRecord, bankModule);

										break;
									case "country_travel_tracking":
										var countryTravelTracking = await moduleRepository.GetByName("country_travel_tracking");
										int approverId = 3488;
										if (!record["shared_users_edit"].IsNullOrEmpty())
										{
											var sharedUsers = (JArray)record["shared_users_edit"];
											foreach (var item in sharedUsers.ToList())
											{
												if ((int)item != approverId)
												{
													sharedUsers.Add(approverId);
													record["shared_users_edit"] = sharedUsers;
												}
											}
										}
										else
										{
											var sharedUsers = new JArray();
											sharedUsers.Add(approverId);
											record["shared_users_edit"] = sharedUsers;
										}

										await recordRepository.Update(record, countryTravelTracking);
										break;
									case "referance_pool":
										var referancePoolModule = await moduleRepository.GetByName("referance_pool");
										var refObj = new JObject();
										refObj["id"] = record["id"];
                                        if (!record["total_budget"].IsNullOrEmpty() && !record["company_share"].IsNullOrEmpty())
                                        {
                                            refObj["proportion_carried_out"] = (int)record["total_budget"] * (decimal)record["company_share"] / 100;
                                        }

                                        if (!record["related_partner"].IsNullOrEmpty())
                                        {
                                            var relatedPartnerRequest = new FindRequest { Fields = new List<string> { "name" }, Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)record["related_partner"], No = 1 } }, Limit = 1 };
                                            var relatedPartner = recordRepository.Find("accounts", relatedPartnerRequest);
                                            refObj["related_partner_text"] = (string)relatedPartner.First()["name"];
                                        }
                                        await recordRepository.Update(refObj, referancePoolModule);
										break;
									case "procurement_requisition":
										var procurementRequisitionModule = await moduleRepository.GetByName("procurement_requisition");
										var stepPicklist = procurementRequisitionModule.Fields.Single(x => x.Name == "procurement_step");
										var stepPicklistItem = await picklistRepository.FindItemByLabel(stepPicklist.PicklistId.Value, (string)record["procurement_step"], appUser.TenantLanguage);
										if (stepPicklistItem.Value == "requisition")
										{
											var requestTypePicklist = procurementRequisitionModule.Fields.Single(x => x.Name == "request_type");
											var requestTypePicklistItem = await picklistRepository.FindItemByLabel(requestTypePicklist.PicklistId.Value, (string)record["request_type"], appUser.TenantLanguage);
											var sharedUsetFindRequest = new FindRequest();

											if (requestTypePicklistItem != null)
											{
												if (requestTypePicklistItem.Value == "it")
													sharedUsetFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = "gurkan.benekse@projectgroup.com.tr", No = 1 } }, Limit = 1 };
												else
													sharedUsetFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = "burcin.erkan@pf.com.tr", No = 1 } }, Limit = 1 };

												var sharedUser = recordRepository.Find("users", sharedUsetFindRequest);

												if (!record["shared_users_edit"].IsNullOrEmpty())
												{
													int id = (int)sharedUser.First()["id"];
													var sharedUsers = (JArray)record["shared_users_edit"];
													foreach (var item in sharedUsers.ToList())
													{
														if ((int)item != id)
														{
															sharedUsers.Add(sharedUser.First()["id"]);
															record["shared_users_edit"] = sharedUsers;
														}
													}
												}
												else
												{
													var sharedUsers = new JArray();
													sharedUsers.Add(sharedUser.First()["id"]);
													record["shared_users_edit"] = sharedUsers;
												}
											}
										}
										else if (stepPicklistItem.Value == "offer_selection")
										{
											if (!record["selected_offer"].IsNullOrEmpty())
											{
												record["shared_users_edit"] = null;
											}
										}

										if (!record["approver"].IsNullOrEmpty())
										{
											var approverPicklist = procurementRequisitionModule.Fields.Single(x => x.Name == "approver");
											var approverPicklistItem = await picklistRepository.FindItemByLabel(approverPicklist.PicklistId.Value, (string)record["approver"], appUser.TenantLanguage);
											var recordOwnerRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)record["owner"], No = 1 } }, Limit = 9999 };
											var recordOwner = recordRepository.Find("users", recordOwnerRequest);
											var recordOwnerObj = (JObject)recordOwner.First();
											if (approverPicklistItem.Value == "levent_ergen")
											{
												if ((string)recordOwnerObj["email"] != "levent.ergen@pf.com.tr")
												{
													record["custom_approver"] = "levent.ergen@pf.com.tr";
												}
												else
												{
													record["custom_approver"] = "deniz.tekeli@projectgroup.com.tr";
												}
											}
											else if (approverPicklistItem.Value == "deniz_tekeli")
											{
												if ((string)recordOwnerObj["email"] != "deniz.tekeli@projectgroup.com.tr")
												{
													record["custom_approver"] = "deniz.tekeli@projectgroup.com.tr";
												}
												else
												{
													record["custom_approver"] = "levent.ergen@pf.com.tr";
												}
											}
											else
											{
												var projectOverheadPicklist = procurementRequisitionModule.Fields.Single(x => x.Name == "projectoverhead");
												var projectOverheadPicklistItem = await picklistRepository.FindItemByLabel(projectOverheadPicklist.PicklistId.Value, (string)record["projectoverhead"], appUser.TenantLanguage);
												if (projectOverheadPicklistItem.Value == "project_expense")
												{
													var bdpmPicklist = procurementRequisitionModule.Fields.Single(x => x.Name == "bdpm");
													var bdpmPicklistItem = await picklistRepository.FindItemByLabel(bdpmPicklist.PicklistId.Value, (string)record["bdpm"], appUser.TenantLanguage);
													var approvalModule = await moduleRepository.GetByName("approval_workflow");
													var approvalPicklistId = approvalModule.Fields.Single(x => x.Name == "approval_type").PicklistId.Value;
													var approvalPicklist = await picklistRepository.GetById(approvalPicklistId);

													if (bdpmPicklistItem.Value == "bd_stage")
													{
														var approvalPicklistItem = approvalPicklist.Items.Single(x => x.SystemCode == "business");
														var findRequestApproval = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_project", Operator = Operator.Equals, Value = (int)record["project_code"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalPicklistItem.LabelTr : approvalPicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
														var approvalRecord = recordRepository.Find("approval_workflow", findRequestApproval);
														FindRequest approverRequest = null;
														JArray approverRecord = null;

														if (approverPicklistItem.Value == "project_officer")
														{
															approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalRecord.First()["project_officer_staff"], No = 1 } }, Limit = 9999 };
															approverRecord = recordRepository.Find("human_resources", approverRequest);
															var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
															var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

															if (!record["shared_users_edit"].IsNullOrEmpty())
															{
																int id = (int)sharedUser.First()["id"];
																var sharedUsers = (JArray)record["shared_users_edit"];
																foreach (var item in sharedUsers.ToList())
																{
																	if ((int)item != id)
																	{
																		sharedUsers.Add(sharedUser.First()["id"]);
																		record["shared_users_edit"] = sharedUsers;
																	}
																}
															}
															else
															{
																var sharedUsers = new JArray();
																sharedUsers.Add(sharedUser.First()["id"]);
																record["shared_users_edit"] = sharedUsers;
															}
															record["custom_approver"] = approverRecord.First()["e_mail1"];
														}
														else if (approverPicklistItem.Value == "first_approver")
														{
															approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
															approverRecord = recordRepository.Find("human_resources", approverRequest);
															var approverRecordObj = (JObject)approverRecord.First();

															if ((string)recordOwnerObj["email"] == (string)approverRecordObj["e_mail1"])
															{
																approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalRecord.First()["second_approver"], No = 1 } }, Limit = 9999 };
																approverRecord = recordRepository.Find("human_resources", approverRequest);
																var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
																var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

																if (!record["shared_users_edit"].IsNullOrEmpty())
																{
																	int id = (int)sharedUser.First()["id"];
																	var sharedUsers = (JArray)record["shared_users_edit"];
																	foreach (var item in sharedUsers.ToList())
																	{
																		if ((int)item != id)
																		{
																			sharedUsers.Add(sharedUser.First()["id"]);
																			record["shared_users_edit"] = sharedUsers;
																		}
																	}
																}
																else
																{
																	var sharedUsers = new JArray();
																	sharedUsers.Add(sharedUser.First()["id"]);
																	record["shared_users_edit"] = sharedUsers;
																}
																record["custom_approver"] = approverRecord.First()["e_mail1"];
															}
															else
															{
																var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
																var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

																if (!record["shared_users_edit"].IsNullOrEmpty())
																{
																	int id = (int)sharedUser.First()["id"];
																	var sharedUsers = (JArray)record["shared_users_edit"];
																	foreach (var item in sharedUsers.ToList())
																	{
																		if ((int)item != id)
																		{
																			sharedUsers.Add(sharedUser.First()["id"]);
																			record["shared_users_edit"] = sharedUsers;
																		}
																	}
																}
																else
																{
																	var sharedUsers = new JArray();
																	sharedUsers.Add(sharedUser.First()["id"]);
																	record["shared_users_edit"] = sharedUsers;
																}
																record["custom_approver"] = approverRecord.First()["e_mail1"];
															}
														}
														else if (approverPicklistItem.Value == "second_approver")
														{
															approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalRecord.First()["second_approver"], No = 1 } }, Limit = 9999 };
															approverRecord = recordRepository.Find("human_resources", approverRequest);
															var approverRecordObj = (JObject)approverRecord.First();

															if ((string)recordOwnerObj["email"] == (string)approverRecordObj["e_mail1"])
															{
																var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = "deniz.tekeli@projectgroup.com.tr", No = 1 } }, Limit = 1 };
																var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

																if (!record["shared_users_edit"].IsNullOrEmpty())
																{
																	int id = (int)sharedUser.First()["id"];
																	var sharedUsers = (JArray)record["shared_users_edit"];
																	foreach (var item in sharedUsers.ToList())
																	{
																		if ((int)item != id)
																		{
																			sharedUsers.Add(sharedUser.First()["id"]);
																			record["shared_users_edit"] = sharedUsers;
																		}
																	}
																}
																else
																{
																	var sharedUsers = new JArray();
																	sharedUsers.Add(sharedUser.First()["id"]);
																	record["shared_users_edit"] = sharedUsers;
																}
																record["custom_approver"] = "deniz.tekeli@projectgroup.com.tr";
															}
															else
															{
																var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
																var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

																if (!record["shared_users_edit"].IsNullOrEmpty())
																{
																	int id = (int)sharedUser.First()["id"];
																	var sharedUsers = (JArray)record["shared_users_edit"];
																	foreach (var item in sharedUsers.ToList())
																	{
																		if ((int)item != id)
																		{
																			sharedUsers.Add(sharedUser.First()["id"]);
																			record["shared_users_edit"] = sharedUsers;
																		}
																	}
																}
																else
																{
																	var sharedUsers = new JArray();
																	sharedUsers.Add(sharedUser.First()["id"]);
																	record["shared_users_edit"] = sharedUsers;
																}
																record["custom_approver"] = approverRecord.First()["e_mail1"];
															}
														}
													}
													else if (bdpmPicklistItem.Value == "pm_stage")
													{
														var approvalPicklistItem = approvalPicklist.Items.Single(x => x.SystemCode == "nonbillable");
														var findRequestApproval = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_project", Operator = Operator.Equals, Value = (int)record["project_code"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalPicklistItem.LabelTr : approvalPicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
														var approvalRecord = recordRepository.Find("approval_workflow", findRequestApproval);
														FindRequest approverRequest = null;
														JArray approverRecord = null;

														if (approverPicklistItem.Value == "project_officer")
														{
															approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalRecord.First()["project_officer_staff"], No = 1 } }, Limit = 9999 };
															approverRecord = recordRepository.Find("human_resources", approverRequest);
															var approverRecordObj = approverRecord.First();
															var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
															var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

															if (!record["shared_users_edit"].IsNullOrEmpty())
															{
																int id = (int)sharedUser.First()["id"];
																var sharedUsers = (JArray)record["shared_users_edit"];
																foreach (var item in sharedUsers.ToList())
																{
																	if ((int)item != id)
																	{
																		sharedUsers.Add(sharedUser.First()["id"]);
																		record["shared_users_edit"] = sharedUsers;
																	}
																}
															}
															else
															{
																var sharedUsers = new JArray();
																sharedUsers.Add(sharedUser.First()["id"]);
																record["shared_users_edit"] = sharedUsers;
															}
															record["custom_approver"] = approverRecord.First()["e_mail1"];
														}
														else if (approverPicklistItem.Value == "first_approver")
														{
															approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
															approverRecord = recordRepository.Find("human_resources", approverRequest);
															var approverRecordObj = approverRecord.First();

															if ((string)recordOwnerObj["email"] == (string)approverRecordObj["e_mail1"])
															{
																approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalRecord.First()["second_approver"], No = 1 } }, Limit = 9999 };
																approverRecord = recordRepository.Find("human_resources", approverRequest);
																var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
																var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

																if (!record["shared_users_edit"].IsNullOrEmpty())
																{
																	int id = (int)sharedUser.First()["id"];
																	var sharedUsers = (JArray)record["shared_users_edit"];
																	foreach (var item in sharedUsers.ToList())
																	{
																		if ((int)item != id)
																		{
																			sharedUsers.Add(sharedUser.First()["id"]);
																			record["shared_users_edit"] = sharedUsers;
																		}
																	}
																}
																else
																{
																	var sharedUsers = new JArray();
																	sharedUsers.Add(sharedUser.First()["id"]);
																	record["shared_users_edit"] = sharedUsers;
																}
																record["custom_approver"] = approverRecord.First()["e_mail1"];
															}
															else
															{
																var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
																var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

																if (!record["shared_users_edit"].IsNullOrEmpty())
																{
																	int id = (int)sharedUser.First()["id"];
																	var sharedUsers = (JArray)record["shared_users_edit"];
																	foreach (var item in sharedUsers.ToList())
																	{
																		if ((int)item != id)
																		{
																			sharedUsers.Add(sharedUser.First()["id"]);
																			record["shared_users_edit"] = sharedUsers;
																		}
																	}
																}
																else
																{
																	var sharedUsers = new JArray();
																	sharedUsers.Add(sharedUser.First()["id"]);
																	record["shared_users_edit"] = sharedUsers;
																}
																record["custom_approver"] = approverRecord.First()["e_mail1"];
															}
														}
														else if (approverPicklistItem.Value == "second_approver")
														{
															approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalRecord.First()["second_approver"], No = 1 } }, Limit = 9999 };
															approverRecord = recordRepository.Find("human_resources", approverRequest);
															var approverRecordObj = approverRecord.First();

															if ((string)recordOwnerObj["email"] == (string)approverRecordObj["e_mail1"])
															{
																var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = "levent.ergen@pf.com.tr", No = 1 } }, Limit = 1 };
																var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

																if (!record["shared_users_edit"].IsNullOrEmpty())
																{
																	int id = (int)sharedUser.First()["id"];
																	var sharedUsers = (JArray)record["shared_users_edit"];
																	foreach (var item in sharedUsers.ToList())
																	{
																		if ((int)item != id)
																		{
																			sharedUsers.Add(sharedUser.First()["id"]);
																			record["shared_users_edit"] = sharedUsers;
																		}
																	}
																}
																else
																{
																	var sharedUsers = new JArray();
																	sharedUsers.Add(sharedUser.First()["id"]);
																	record["shared_users_edit"] = sharedUsers;
																}
																record["custom_approver"] = "levent.ergen@pf.com.tr";
															}
															else
															{
																var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
																var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

																if (!record["shared_users_edit"].IsNullOrEmpty())
																{
																	int id = (int)sharedUser.First()["id"];
																	var sharedUsers = (JArray)record["shared_users_edit"];
																	foreach (var item in sharedUsers.ToList())
																	{
																		if ((int)item != id)
																		{
																			sharedUsers.Add(sharedUser.First()["id"]);
																			record["shared_users_edit"] = sharedUsers;
																		}
																	}
																}
																else
																{
																	var sharedUsers = new JArray();
																	sharedUsers.Add(sharedUser.First()["id"]);
																	record["shared_users_edit"] = sharedUsers;
																}
																record["custom_approver"] = approverRecord.First()["e_mail1"];
															}
														}
													}
												}
												else if (projectOverheadPicklistItem.Value == "overhead")
												{
													var findRequestHumanResources = new FindRequest { Filters = new List<Filter> { new Filter { Field = "e_mail1", Operator = Operator.Equals, Value = appUser.Email, No = 1 } }, Limit = 9999 };
													var humanResourcesRecord = recordRepository.Find("human_resources", findRequestHumanResources);
													var approvalModule = await moduleRepository.GetByName("approval_workflow");
													var approvalPicklistId = approvalModule.Fields.Single(x => x.Name == "approval_type").PicklistId.Value;
													var approvalPicklist = await picklistRepository.GetById(approvalPicklistId);
													var approvalTypePicklistItem = approvalPicklist.Items.Single(x => x.SystemCode == "management");
													var findRequestApprovalWorkflow = new FindRequest { Filters = new List<Filter> { new Filter { Field = "staff", Operator = Operator.Equals, Value = (int)humanResourcesRecord.First()["id"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalTypePicklistItem.LabelTr : approvalTypePicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
													var approvalWorkflowRecord = recordRepository.Find("approval_workflow", findRequestApprovalWorkflow);
													var findApproverRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
													var approverRecord = recordRepository.Find("human_resources", findApproverRecord);
													var sharedUserFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 1 };
													var sharedUser = recordRepository.Find("users", sharedUserFindRequest);

													if (!record["shared_users_edit"].IsNullOrEmpty())
													{
														int id = (int)sharedUser.First()["id"];
														var sharedUsers = (JArray)record["shared_users_edit"];
														foreach (var item in sharedUsers.ToList())
														{
															if ((int)item != id)
															{
																sharedUsers.Add(sharedUser.First()["id"]);
																record["shared_users_edit"] = sharedUsers;
															}
														}
													}
													else
													{
														var sharedUsers = new JArray();
														sharedUsers.Add(sharedUser.First()["id"]);
														record["shared_users_edit"] = sharedUsers;
													}
													record["custom_approver"] = approverRecord.First()["e_mail1"];
												}
											}
										}

										await recordRepository.Update(record, procurementRequisitionModule);
										break;
									case "petty_cash_requisition":
									case "expenditure":
										var pettyCashModule = await moduleRepository.GetByName("petty_cash");
										var pettyCashRecord = recordRepository.GetById(pettyCashModule, (int)record["related_petty_cash_2"], false, null, true);
										var pettyCashUpdateRecord = new JObject();
										if (module.Name == "petty_cash_requisition")
										{
											var pettyCashRequisitionModule = await moduleRepository.GetByName("petty_cash_requisition");
											var pettyCashRequisitionPicklist = pettyCashRequisitionModule.Fields.Single(x => x.Name == "status");
											var pettyCashRequisitionPicklistItem = await picklistRepository.FindItemByLabel(pettyCashRequisitionPicklist.PicklistId.Value, (string)record["status"], appUser.TenantLanguage);

											if (pettyCashRequisitionPicklistItem.Value == "paid" && !record["paid_amount"].IsNullOrEmpty() && !record["paid_by"].IsNullOrEmpty())
											{
												var findRequestPettyCashRequisition = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_petty_cash_2", Operator = Operator.Equals, Value = (int)record["related_petty_cash_2"], No = 1 } }, Limit = 9999 };
												var pettyCashRequisitionRecords = recordRepository.Find(module.Name, findRequestPettyCashRequisition);
												var currencyFieldRequisition = pettyCashRequisitionModule.Fields.Single(x => x.Name == "currency");
												var currencyPicklistRequisition = await picklistRepository.GetById(currencyFieldRequisition.PicklistId.Value);

												decimal totalIncomeTry = 0;
												decimal totalIncomeEur = 0;
												decimal totalIncomeUsd = 0;
												decimal totalIncomeGbp = 0;

												foreach (var requisitionRecordItem in pettyCashRequisitionRecords)
												{
													var amount = !requisitionRecordItem["paid_amount"].IsNullOrEmpty() ? (decimal)requisitionRecordItem["paid_amount"] : 0;
													var currency = currencyPicklistRequisition.Items.Single(x => x.LabelEn == (string)requisitionRecordItem["currency"]).SystemCode;

													switch (currency)
													{
														case "try":
															totalIncomeTry += amount;
															break;
														case "eur":
															totalIncomeEur += amount;
															break;
														case "usd":
															totalIncomeUsd += amount;
															break;
														case "gbp":
															totalIncomeGbp += amount;
															break;
													}
												}

												pettyCashUpdateRecord["id"] = (int)record["related_petty_cash_2"];
												pettyCashUpdateRecord["try_income"] = totalIncomeTry;
												pettyCashUpdateRecord["eur_income"] = totalIncomeEur;
												pettyCashUpdateRecord["usd_income"] = totalIncomeUsd;
												pettyCashUpdateRecord["gbp_income"] = totalIncomeGbp;
												pettyCashUpdateRecord["try_balance"] = totalIncomeTry - (pettyCashRecord["try_expenditure"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["try_expenditure"]);
												pettyCashUpdateRecord["eur_balance"] = totalIncomeEur - (pettyCashRecord["eur_expenditure"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["eur_expenditure"]);
												pettyCashUpdateRecord["usd_balance"] = totalIncomeUsd - (pettyCashRecord["usd_expenditure"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["usd_expenditure"]);
												pettyCashUpdateRecord["gbp_balance"] = totalIncomeGbp - (pettyCashRecord["gbp_expenditure"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["gbp_expenditure"]);
												pettyCashUpdateRecord["updated_by"] = (int)record["updated_by"];
											}
										}
										if (module.Name == "expenditure")
										{
											var expenditureModule = await moduleRepository.GetByName("expenditure");

											if (!record["amount"].IsNullOrEmpty())
											{
												var findRequestExpenditure = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_petty_cash_2", Operator = Operator.Equals, Value = (int)record["related_petty_cash_2"], No = 1 } }, Limit = 9999 };
												var expenditureRecords = recordRepository.Find(module.Name, findRequestExpenditure);
												var currencyFieldExpenditure = expenditureModule.Fields.Single(x => x.Name == "currency_c");
												var currencyPicklistExpenditure = await picklistRepository.GetById(currencyFieldExpenditure.PicklistId.Value);

												decimal totalExpenditureTry = 0;
												decimal totalExpenditureEur = 0;
												decimal totalExpenditureUsd = 0;
												decimal totalExpenditureGbp = 0;

												foreach (var expenditureRecordItem in expenditureRecords)
												{
													var amount = !expenditureRecordItem["amount"].IsNullOrEmpty() ? (decimal)expenditureRecordItem["amount"] : 0;
													var currency = currencyPicklistExpenditure.Items.Single(x => x.LabelEn == (string)expenditureRecordItem["currency_c"]).SystemCode;

													switch (currency)
													{
														case "try":
															totalExpenditureTry += amount;
															break;
														case "eur":
															totalExpenditureEur += amount;
															break;
														case "usd":
															totalExpenditureUsd += amount;
															break;
														case "gbp":
															totalExpenditureGbp += amount;
															break;
													}
												}


												pettyCashUpdateRecord["id"] = (int)record["related_petty_cash_2"];
												pettyCashUpdateRecord["try_expenditure"] = totalExpenditureTry;
												pettyCashUpdateRecord["eur_expenditure"] = totalExpenditureEur;
												pettyCashUpdateRecord["usd_expenditure"] = totalExpenditureUsd;
												pettyCashUpdateRecord["gbp_expenditure"] = totalExpenditureGbp;
												pettyCashUpdateRecord["try_balance"] = (pettyCashRecord["try_income"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["try_income"]) - totalExpenditureTry;
												pettyCashUpdateRecord["eur_balance"] = (pettyCashRecord["eur_income"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["eur_income"]) - totalExpenditureEur;
												pettyCashUpdateRecord["usd_balance"] = (pettyCashRecord["usd_income"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["usd_income"]) - totalExpenditureUsd;
												pettyCashUpdateRecord["gbp_balance"] = (pettyCashRecord["gbp_income"].IsNullOrEmpty() ? 0 : (decimal)pettyCashRecord["gbp_income"]) - totalExpenditureGbp;
												pettyCashUpdateRecord["updated_by"] = (int)record["updated_by"];
											}
										}
										await recordRepository.Update(pettyCashUpdateRecord, pettyCashModule);
										break;

									case "expense_sheet":
									case "invoices":

										var expenseModule = await moduleRepository.GetByName("expense_sheet");
										var expenseTypePicklist = expenseModule.Fields.Single(x => x.Name == "expense_type");
										var expenseTypePicklistItem = await picklistRepository.FindItemByLabel(expenseTypePicklist.PicklistId.Value, (string)record["expense_type"], appUser.TenantLanguage);

										var approvalWorkflowModule = await moduleRepository.GetByName("approval_workflow");
										var approvalTypePicklistId = approvalWorkflowModule.Fields.Single(x => x.Name == "approval_type").PicklistId.Value;
										var approvalTypePicklist = await picklistRepository.GetById(approvalTypePicklistId);


										if (module.Name == "expense_sheet")
										{
											JArray approverUserRecord = null;
											if (expenseTypePicklistItem.SystemCode == "project_expense")
											{
												var approvalTypePicklistItem = approvalTypePicklist.Items.Single(x => x.SystemCode == "nonbillable");
												var findRequestApprovalWorkflow = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_project", Operator = Operator.Equals, Value = (int)record["project_code"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalTypePicklistItem.LabelTr : approvalTypePicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
												var approvalWorkflowRecord = recordRepository.Find("approval_workflow", findRequestApprovalWorkflow);
												var findRequestHumanResources = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
												var humanResourcesRecord = recordRepository.Find("human_resources", findRequestHumanResources);
												var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = humanResourcesRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
												var recordOwnerRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)record["owner"], No = 1 } }, Limit = 9999 };
												var recordOwner = recordRepository.Find("users", recordOwnerRequest);
												var recordOwnerObj = recordOwner.First();
												var humanResourcesRecordObj = humanResourcesRecord.First();
												approverUserRecord = recordRepository.Find("users", findApproverUser);
												if (!humanResourcesRecord.IsNullOrEmpty())
												{
													if ((string)recordOwnerObj["email"] == (string)humanResourcesRecordObj["e_mail1"])
													{
														var approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["second_approver"], No = 1 } }, Limit = 9999 };
														var approverRecord = recordRepository.Find("human_resources", approverRequest);
														record["custom_approver"] = approverRecord.First()["e_mail1"];
													}
													else
													{
														record["custom_approver"] = humanResourcesRecord.First()["e_mail1"];
													}

													if (!record["shared_users_edit"].IsNullOrEmpty())
													{
														int id = (int)approverUserRecord.First()["id"];
														var sharedUsers = (JArray)record["shared_users_edit"];
														foreach (var item in sharedUsers.ToList())
														{
															if ((int)item != id)
															{
																sharedUsers.Add(approverUserRecord.First()["id"]);
																record["shared_users_edit"] = sharedUsers;
															}
														}
													}
													else
													{
														var sharedUsers = new JArray();
														sharedUsers.Add(approverUserRecord.First()["id"]);
														record["shared_users_edit"] = sharedUsers;
													}
												}
											}
											else if (expenseTypePicklistItem.SystemCode == "overhead")
											{
												var findRequestHumanResources = new FindRequest { Filters = new List<Filter> { new Filter { Field = "e_mail1", Operator = Operator.Equals, Value = appUser.Email, No = 1 } }, Limit = 9999 };
												var humanResourcesRecord = recordRepository.Find("human_resources", findRequestHumanResources);
												var approvalTypePicklistItem = approvalTypePicklist.Items.Single(x => x.SystemCode == "management");
												var findRequestApprovalWorkflow = new FindRequest { Filters = new List<Filter> { new Filter { Field = "staff", Operator = Operator.Equals, Value = (int)humanResourcesRecord.First()["id"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalTypePicklistItem.LabelTr : approvalTypePicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
												var approvalWorkflowRecord = recordRepository.Find("approval_workflow", findRequestApprovalWorkflow);
												var findApproverRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
												var approverRecord = recordRepository.Find("human_resources", findApproverRecord);
												var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
												approverUserRecord = recordRepository.Find("users", findApproverUser);
												if (!approverRecord.IsNullOrEmpty())
												{
													record["custom_approver"] = approverRecord.First()["e_mail1"];
													if (!record["shared_users_edit"].IsNullOrEmpty())
													{
														var sharedUsers = (JArray)record["shared_users_edit"];
														sharedUsers.Add(approverUserRecord.First()["id"]);
														record["shared_users_edit"] = sharedUsers;
													}
													else
													{
														var sharedUsers = new JArray();
														sharedUsers.Add(approverUserRecord.First()["id"]);
														record["shared_users_edit"] = sharedUsers;
													}
												}

											}

											if (!approverUserRecord.IsNullOrEmpty())
											{
												var expenseFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "expense_sheet", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
												var expenses = recordRepository.Find("expenses", expenseFindRequest);
												if (expenses.Count > 0)
												{
													var expModule = await moduleRepository.GetByName("expenses");
													foreach (JObject expense in expenses)
													{
														if (!expense["shared_users_edit"].IsNullOrEmpty())
														{
															var expenseSharedUsers = (JArray)expense["shared_users_edit"];
															expenseSharedUsers.Add(approverUserRecord.First()["id"]);
															expense["shared_users_edit"] = expenseSharedUsers;
														}
														else
														{
															var expenseSharedUsers = new JArray();
															expenseSharedUsers.Add(approverUserRecord.First()["id"]);
															expense["shared_users_edit"] = expenseSharedUsers;
														}

														await recordRepository.Update(expense, expModule);
													}
												}
											}

											using (var userGroupRepository = new UserGroupRepository(databaseContext, _configuration))
											{
												var financeUserGroup = await userGroupRepository.GetByName("finance-expense");

												if (financeUserGroup != null)
												{
													var sharedUserGroups = new JArray();

													if (!record["shared_user_groups_edit"].IsNullOrEmpty())
														sharedUserGroups = (JArray)record["shared_user_groups_edit"];

													if (!sharedUserGroups.Any(x => (int)x == financeUserGroup.Id))
														sharedUserGroups.Add(financeUserGroup.Id);

													record["shared_user_groups_edit"] = sharedUserGroups;

													var expenseFindRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "expense_sheet", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
													var expenses = recordRepository.Find("expenses", expenseFindRequest);
													if (expenses.Count > 0)
													{
														var expModule = await moduleRepository.GetByName("expenses");
														foreach (JObject expense in expenses)
														{
															if (!expense["shared_user_groups_edit"].IsNullOrEmpty())
															{
																var expenseSharedUsers = (JArray)expense["shared_user_groups_edit"];
																if (!expenseSharedUsers.Any(x => (int)x == financeUserGroup.Id))
																{
																	expenseSharedUsers.Add(financeUserGroup.Id);
																	expense["shared_user_groups_edit"] = expenseSharedUsers;
																}
															}
															else
															{
																var expenseSharedUsers = new JArray();
																expenseSharedUsers.Add(financeUserGroup.Id);
																expense["shared_user_groups_edit"] = expenseSharedUsers;
															}

															await recordRepository.Update(expense, expModule);
														}
													}
												}
											}

											await recordRepository.Update(record, expenseModule);
										}
										else if (module.Name == "invoices")
										{
											var invoiceModule = await moduleRepository.GetByName("invoices");
											var invoiceTypePicklist = invoiceModule.Fields.Single(x => x.Name == "invoice_type");
											var invoiceTypePicklistItem = await picklistRepository.FindItemByLabel(invoiceTypePicklist.PicklistId.Value, (string)record["invoice_type"], appUser.TenantLanguage);
											var invoiceApproverPicklist = invoiceModule.Fields.Single(x => x.Name == "approver");
											var invoiceApproverPicklistItem = await picklistRepository.FindItemByLabel(invoiceApproverPicklist.PicklistId.Value, (string)record["approver"], appUser.TenantLanguage);
											var recordOwnerRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)record["owner"], No = 1 } }, Limit = 9999 };
											var recordOwner = recordRepository.Find("users", recordOwnerRequest);
											var recordOwnerObj = recordOwner.First();
											if (invoiceTypePicklistItem.SystemCode == "project_expense")
											{
												var approvalTypePicklistItem = approvalTypePicklist.Items.Single(x => x.SystemCode == "nonbillable");
												var findRequestApprovalWorkflow = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_project", Operator = Operator.Equals, Value = (int)record["project"], No = 1 }, new Filter { Field = "approval_type", Operator = Operator.Equals, Value = appUser.TenantLanguage == "tr" ? approvalTypePicklistItem.LabelTr : approvalTypePicklistItem.LabelEn, No = 2 } }, Limit = 9999 };
												var approvalWorkflowRecord = recordRepository.Find("approval_workflow", findRequestApprovalWorkflow);

												if (invoiceApproverPicklistItem.SystemCode == "project_director")
												{
													var findApproverRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["first_approver"], No = 1 } }, Limit = 9999 };
													var approverRecord = recordRepository.Find("human_resources", findApproverRecord);
													var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
													var approverUserRecord = recordRepository.Find("users", findApproverUser);
													var approverRecordObj = approverRecord.First();

													if (!approverRecord.IsNullOrEmpty())
													{
														if ((string)recordOwnerObj["email"] == (string)approverRecordObj["e_mail1"])
														{
															var approverRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["second_approver"], No = 1 } }, Limit = 9999 };
															var approverHumanRecord = recordRepository.Find("human_resources", approverRequest);
															record["custom_approver"] = approverHumanRecord.First()["e_mail1"];
														}
														else
														{
															record["custom_approver"] = approverRecord.First()["e_mail1"];
														}
														if (!record["shared_users_edit"].IsNullOrEmpty())
														{
															var sharedUsers = (JArray)record["shared_users_edit"];
															sharedUsers.Add(approverUserRecord.First()["id"]);
															record["shared_users_edit"] = sharedUsers;
														}
														else
														{
															var sharedUsers = new JArray();
															sharedUsers.Add(approverUserRecord.First()["id"]);
															record["shared_users_edit"] = sharedUsers;
														}
													}
												}
												else if (invoiceApproverPicklistItem.SystemCode == "project_officer")
												{
													var findApproverRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)approvalWorkflowRecord.First()["project_officer_staff"], No = 1 } }, Limit = 9999 };
													var approverRecord = recordRepository.Find("human_resources", findApproverRecord);
													var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = approverRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
													var approverUserRecord = recordRepository.Find("users", findApproverUser);
													var approverRecordObj = approverRecord.First();

													if (!approverRecord.IsNullOrEmpty())
													{
														record["custom_approver"] = approverRecord.First()["e_mail1"];
														if (!record["shared_users_edit"].IsNullOrEmpty())
														{
															var sharedUsers = (JArray)record["shared_users_edit"];
															sharedUsers.Add(approverUserRecord.First()["id"]);
															record["shared_users_edit"] = sharedUsers;
														}
														else
														{
															var sharedUsers = new JArray();
															sharedUsers.Add(approverUserRecord.First()["id"]);
															record["shared_users_edit"] = sharedUsers;
														}
													}
												}

											}
											else if (invoiceTypePicklistItem.SystemCode == "overhead")
											{
												var approvalTypePicklistItem = approvalTypePicklist.Items.Single(x => x.SystemCode == "management");
												var findRequestHumanResources = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)record["related_staff"], No = 1 } }, Limit = 9999 };
												var humanResourcesRecord = recordRepository.Find("human_resources", findRequestHumanResources);
												var findApproverUser = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = humanResourcesRecord.First()["e_mail1"], No = 1 } }, Limit = 9999 };
												var approverUserRecord = recordRepository.Find("users", findApproverUser);

												if (!humanResourcesRecord.IsNullOrEmpty())
												{
													record["custom_approver"] = humanResourcesRecord.First()["e_mail1"];
													if (!record["shared_users_edit"].IsNullOrEmpty())
													{
														var sharedUsers = (JArray)record["shared_users_edit"];
														sharedUsers.Add(approverUserRecord.First()["id"]);
														record["shared_users_edit"] = sharedUsers;
													}
													else
													{
														var sharedUsers = new JArray();
														sharedUsers.Add(approverUserRecord.First()["id"]);
														record["shared_users_edit"] = sharedUsers;
													}
												}
											}

											using (var userGroupRepository = new UserGroupRepository(databaseContext, _configuration))
											{
												var financeUserGroup = await userGroupRepository.GetByName("finance-expense");

												if (financeUserGroup != null)
												{
													var sharedUserGroups = new JArray();

													if (!record["shared_user_groups_edit"].IsNullOrEmpty())
														sharedUserGroups = (JArray)record["shared_user_groups_edit"];

													if (!sharedUserGroups.Any(x => (int)x == financeUserGroup.Id))
														sharedUserGroups.Add(financeUserGroup.Id);

													record["shared_user_groups_edit"] = sharedUserGroups;
												}
											}

											await recordRepository.Update(record, invoiceModule);
										}

										break;
									case "sales_orders":
										var salesOrderModuleObj = await moduleRepository.GetByName("sales_orders");
										var prodModObj = await moduleRepository.GetByName("products");
										var salesOrderPicklist = salesOrderModuleObj.Fields.Single(x => x.Name == "order_stage");
										var stockModObj = await moduleRepository.GetByName("stock_transactions");
										var salesOrderModulePicklist = await picklistRepository.FindItemByLabel(salesOrderPicklist.PicklistId.Value, (string)record["order_stage"], appUser.TenantLanguage);
										var findRequestCurrentStockRecordObj = new FindRequest { Filters = new List<Filter> { new Filter { Field = "sales_order", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
										var currentStockRecordArr = recordRepository.Find("stock_transactions", findRequestCurrentStockRecordObj);
										if ((operationType == OperationType.delete && salesOrderModulePicklist.SystemCode != "converted_to_sales_invoice") || (salesOrderModulePicklist.SystemCode != "confirmed_purchase_order_stage" && salesOrderModulePicklist.SystemCode != "confirmed_order_stage" && salesOrderModulePicklist.SystemCode != "converted_to_sales_invoice"))
										{
											if (currentStockRecordArr.Count > 0)
											{
												foreach (JObject stockTransObj in currentStockRecordArr)
												{
													await recordRepository.Delete(stockTransObj, stockModObj);
													var prodItemObj = new JObject();
													decimal productStockQuantity = await CalculateStock(stockTransObj, appUser, stockModObj);
													prodItemObj["stock_quantity"] = productStockQuantity;
													prodItemObj["id"] = stockTransObj["product"];
													await recordRepository.Update(prodItemObj, prodModObj);
												}
											}
										}
										break;
									case "purchase_orders":
										var purchaseOrderModuleObj = await moduleRepository.GetByName("purchase_orders");
										var prodModObj2 = await moduleRepository.GetByName("products");
										var purchaseOrderPicklist = purchaseOrderModuleObj.Fields.Single(x => x.Name == "order_stage");
										var stockModObj2 = await moduleRepository.GetByName("stock_transactions");
										var purchaseOrderModulePicklist = await picklistRepository.FindItemByLabel(purchaseOrderPicklist.PicklistId.Value, (string)record["order_stage"], appUser.TenantLanguage);
										var findRequestCurrentStockRecordObj2 = new FindRequest { Filters = new List<Filter> { new Filter { Field = "purchase_order", Operator = Operator.Equals, Value = (int)record["id"], No = 1 } }, Limit = 9999 };
										var currentStockRecordArr2 = recordRepository.Find("stock_transactions", findRequestCurrentStockRecordObj2);
										if (operationType == OperationType.delete || (purchaseOrderModulePicklist.SystemCode != "confirmed_purchase_order_stage" && purchaseOrderModulePicklist.SystemCode != "confirmed_order_stage"))
										{
											if (currentStockRecordArr2.Count > 0)
											{
												foreach (JObject stockTransObj in currentStockRecordArr2)
												{
													await recordRepository.Delete(stockTransObj, stockModObj2);
													var prodItemObj = new JObject();
													decimal productStockQuantity = await CalculateStock(stockTransObj, appUser, stockModObj2);
													prodItemObj["stock_quantity"] = productStockQuantity;
													prodItemObj["id"] = stockTransObj["product"];
													await recordRepository.Update(prodItemObj, prodModObj2);
												}
											}
										}
										break;
									case "order_products":
									case "purchase_order_products":
										var prodMod = await moduleRepository.GetByName("products");
										var prodItem = recordRepository.GetById(prodMod, (int)record["product"], false);
										var currentModulePicklist = new PicklistItem();
										var findRequestCurrentStockRecord = new FindRequest();
										if (module.Name == "order_products")
										{
											findRequestCurrentStockRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "sales_order", Operator = Operator.Equals, Value = (int)record["sales_order"], No = 1 }, new Filter { Field = "product", Operator = Operator.Equals, Value = (int)record["product"], No = 2 } }, Limit = 9999 };
											var salesOrderModule = await moduleRepository.GetByName("sales_orders");
											var salesOrderItem = recordRepository.GetById(salesOrderModule, (int)record["sales_order"], false);
											var salesStagePicklist = salesOrderModule.Fields.Single(x => x.Name == "order_stage");
											currentModulePicklist = await picklistRepository.FindItemByLabel(salesStagePicklist.PicklistId.Value, (string)salesOrderItem["order_stage"], appUser.TenantLanguage);
										}
										else if (module.Name == "purchase_order_products")
										{
											findRequestCurrentStockRecord = new FindRequest { Filters = new List<Filter> { new Filter { Field = "purchase_order", Operator = Operator.Equals, Value = (int)record["purchase_order"], No = 1 }, new Filter { Field = "product", Operator = Operator.Equals, Value = (int)record["product"], No = 2 } }, Limit = 9999 };
											var purchaseOrderModule = await moduleRepository.GetByName("purchase_orders");
											var purchaseOrderItem = recordRepository.GetById(purchaseOrderModule, (int)record["purchase_order"], false);
											var purchaseStagePicklist = purchaseOrderModule.Fields.Single(x => x.Name == "order_stage");
											currentModulePicklist = await picklistRepository.FindItemByLabel(purchaseStagePicklist.PicklistId.Value, (string)purchaseOrderItem["order_stage"], appUser.TenantLanguage);

										}

										var currentStockRecord = recordRepository.Find("stock_transactions", findRequestCurrentStockRecord);
										var stockModule = await moduleRepository.GetByName("stock_transactions");

										if ((currentModulePicklist.SystemCode == "confirmed_purchase_order_stage" || currentModulePicklist.SystemCode == "confirmed_order_stage") && operationType != OperationType.delete)
										{
											var modelStateTransaction = new ModelStateDictionary();
											var transactionTypeField = stockModule.Fields.Single(x => x.Name == "stock_transaction_type");
											var transactionTypes = await picklistRepository.GetById(transactionTypeField.PicklistId.Value);

											var stock = new JObject();
											stock["owner"] = appUser.Id;
											stock["product"] = record["product"];
											stock["transaction_date"] = DateTime.UtcNow.Date;

											if (module.Name == "order_products")
											{
												stock["cikan_miktar"] = record["quantity"];
												stock["stock_transaction_type"] = transactionTypes.Items.Single(x => x.SystemCode == "stock_output").Id;
												stock["sales_order"] = (int)record["sales_order"];

											}
											else if (module.Name == "purchase_order_products")
											{
												stock["quantity"] = record["quantity"];
												stock["stock_transaction_type"] = transactionTypes.Items.Single(x => x.SystemCode == "stock_input").Id;
												stock["purchase_order"] = (int)record["purchase_order"];
											}

											var transactionBeforeCreate = await BeforeCreateUpdate(stockModule, stock, modelStateTransaction, appUser.TenantLanguage);
											if (transactionBeforeCreate < 0 && !modelStateTransaction.IsValid)
											{
												//ErrorLog.GetDefault(null).Log(new Error(new Exception("Stock transaction can not be created")));
												return;
											}
											if (currentStockRecord.Count > 0)
											{
												stock["id"] = currentStockRecord.First()["id"];
												await recordRepository.Update(stock, stockModule);
											}
											else

												await recordRepository.Create(stock, stockModule);

											if (prodItem["stock_quantity"].IsNullOrEmpty())
												prodItem["stock_quantity"] = 0;


											decimal productStockQuantity = await CalculateStock(record, appUser, stockModule);
											prodItem["stock_quantity"] = productStockQuantity;

											await recordRepository.Update(prodItem, prodMod);
										}

										break;

									case "stock_transactions":
										var productModuleObj = await moduleRepository.GetByName("products");
										var product = recordRepository.GetById(productModuleObj, (int)record["product"], false);

										if (product["stock_quantity"].IsNullOrEmpty())
											product["stock_quantity"] = 0;

										var stockModuleObj = await moduleRepository.GetByName("stock_transactions");
										var transactionTypePicklist = stockModuleObj.Fields.Single(x => x.Name == "stock_transaction_type");
										decimal stockQuantity = await CalculateStock(record, appUser, stockModuleObj);
										product["stock_quantity"] = stockQuantity;

										await recordRepository.Update(product, productModuleObj);
										break;

									//case "current_accounts":
									//    try
									//    {
									//        var currentTransactionType = (string)record["transaction_type_system"];
									//        var recordUpdate = new JObject();
									//        decimal balance;
									//        Module moduleUpdate;

									//        switch (currentTransactionType)
									//        {
									//            case "sales_invoice":
									//            case "collection":
									//                var customerId = (int)record["customer"];
									//                balance = recordRepository.CalculateBalance(currentTransactionType, customerId);
									//                moduleUpdate = await moduleRepository.GetByName("accounts");
									//                recordUpdate["id"] = customerId;
									//                recordUpdate["balance"] = balance;
									//                break;
									//            case "purchase_invoice":
									//            case "payment":
									//                var supplierId = (int)record["supplier"];
									//                balance = recordRepository.CalculateBalance(currentTransactionType, supplierId);
									//                moduleUpdate = await moduleRepository.GetByName("suppliers");
									//                recordUpdate["id"] = supplierId;
									//                recordUpdate["balance"] = balance;
									//                break;
									//            default:
									//                throw new Exception("Record transaction_type_system must be sales_invoice, collection, purchase_invoice or payment.");
									//        }

									//        recordUpdate["updated_by"] = (int)record["updated_by"];

									//        var resultUpdate = await recordRepository.Update(recordUpdate, moduleUpdate);

									//        //if (resultUpdate < 1)
									//        //ErrorLog.GetDefault(null).Log(new Error(new Exception("Balance cannot be updated! Object: " + recordUpdate)));
									//    }
									//    catch (Exception ex)
									//    {
									//        //ErrorLog.GetDefault(null).Log(new Error(ex));
									//    }
									//    break;
									case "project_indicators":
										var projectScopeModule = await moduleRepository.GetByName("project_scope");
										var projectScopeRecord = recordRepository.GetById(projectScopeModule, (int)record["related_result"]);
										var findRequestProjectIndicator = new FindRequest { Filters = new List<Filter> { new Filter { Field = "related_result", Operator = Operator.Equals, Value = (int)record["related_result"], No = 1 } }, Limit = 9999 };
										var projectIndicatorRecords = recordRepository.Find(module.Name, findRequestProjectIndicator);
										var modelState = new ModelStateDictionary();
										var weightScope = !projectScopeRecord["weight"].IsNullOrEmpty() ? (decimal)projectScopeRecord["weight"] : 0;
										decimal percentage = 0;
										decimal targetBudget = 0;
										decimal actualBudget = 0;

										//Update project scope percentage
										foreach (var projectIndicatorRecord in projectIndicatorRecords)
										{
											percentage += (!projectIndicatorRecord["weight2"].IsNullOrEmpty() ? (decimal)projectIndicatorRecord["weight2"] : 0) * (!projectIndicatorRecord["percentage"].IsNullOrEmpty() ? (decimal)projectIndicatorRecord["percentage"] : 0) / 100;
											targetBudget += !projectIndicatorRecord["target_budget"].IsNullOrEmpty() ? (decimal)projectIndicatorRecord["target_budget"] : 0;
											actualBudget += !projectIndicatorRecord["actual_budget"].IsNullOrEmpty() ? (decimal)projectIndicatorRecord["actual_budget"] : 0;
										}

										if (percentage > 100)
											percentage = 100;

										var projectScopeUpdateRecord = new JObject();
										projectScopeUpdateRecord["id"] = (int)projectScopeRecord["id"];
										projectScopeUpdateRecord["percentage"] = percentage;
										projectScopeUpdateRecord["effect"] = weightScope * percentage / 100;
										projectScopeUpdateRecord["budget"] = targetBudget;
										projectScopeUpdateRecord["actual_budget"] = actualBudget;
										projectScopeUpdateRecord["updated_by"] = (int)projectScopeRecord["updated_by"];

										var resultBeforeProjectScope = await BeforeCreateUpdate(projectScopeModule, projectScopeUpdateRecord, modelState, appUser.TenantLanguage);

										if (resultBeforeProjectScope < 0 && !modelState.IsValid)
										{
											//ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectScope cannot be updated! Object: " + projectScopeUpdateRecord + " ModelState: " + modelState.ToJsonString())));
											return;
										}

										try
										{
											var resultUpdateProjectScope = await recordRepository.Update(projectScopeUpdateRecord, projectScopeModule);

											if (resultUpdateProjectScope < 1)
											{
												//ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectScope cannot be updated! Object: " + projectScopeUpdateRecord)));
												return;
											}

											await Calculate((int)projectScopeUpdateRecord["id"], projectScopeModule, appUser, warehouse, operationType, BeforeCreateUpdate, GetAllFieldsForFindRequest);
										}
										catch (Exception ex)
										{
											//ErrorLog.GetDefault(null).Log(new Error(ex));
											return;
										}

										//Update project indicator effect
										if (operationType != OperationType.delete)
										{
											var weightIndicator = !record["weight2"].IsNullOrEmpty() ? (decimal)record["weight2"] : 0;
											var percentageIndicator = !record["percentage"].IsNullOrEmpty() ? (decimal)record["percentage"] : 0;
											var projectIndicatorUpdateRecord = new JObject();
											projectIndicatorUpdateRecord["id"] = (int)record["id"];
											projectIndicatorUpdateRecord["effect"] = ((weightIndicator / 100) * (percentageIndicator / 100)) * weightScope;
											projectIndicatorUpdateRecord["updated_by"] = (int)record["updated_by"];

											var indicatorStatusField = module.Fields.Single(x => x.Name == "status");
											var indicatorStatusPicklist = await picklistRepository.GetById(indicatorStatusField.PicklistId.Value);
											var completedStatusPicklistItem = indicatorStatusPicklist.Items.Single(x => x.Value == "completed");
											var ongoingStatusPicklistItem = indicatorStatusPicklist.Items.Single(x => x.Value == "ongoing");
											var notStartedStatusPicklistItem = indicatorStatusPicklist.Items.Single(x => x.Value == "not_started");

											if (percentageIndicator >= 100)
												projectIndicatorUpdateRecord["status"] = completedStatusPicklistItem.Id;

											if (percentageIndicator < 100)
												projectIndicatorUpdateRecord["status"] = ongoingStatusPicklistItem.Id;

											if (percentageIndicator <= 0)
												projectIndicatorUpdateRecord["status"] = notStartedStatusPicklistItem.Id;

											var resultBeforeProjectIndicator = await BeforeCreateUpdate(module, projectIndicatorUpdateRecord, modelState, appUser.TenantLanguage);

											if (resultBeforeProjectIndicator < 0 && !modelState.IsValid)
											{
												//ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectIndicator cannot be updated! Object: " + projectScopeUpdateRecord + " ModelState: " + modelState.ToJsonString())));
												return;
											}

											try
											{
												var resultUpdateProjectIndicator = await recordRepository.Update(projectIndicatorUpdateRecord, module);

												if (resultUpdateProjectIndicator < 1)
												{
													//ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectIndicator cannot be updated! Object: " + projectScopeUpdateRecord)));
												}
											}
											catch (Exception ex)
											{
												//ErrorLog.GetDefault(null).Log(new Error(ex));
											}
										}

										break;
									case "project_scope":
										var projectModule = await moduleRepository.GetByName("projects");
										var projectRecord = recordRepository.GetById(projectModule, (int)record["project"]);
										var findRequestProjectScope = new FindRequest { Filters = new List<Filter> { new Filter { Field = "project", Operator = Operator.Equals, Value = (int)record["project"], No = 1 } }, Limit = 9999 };
										var projectScopeRecords = recordRepository.Find(module.Name, findRequestProjectScope);
										var modelStateScope = new ModelStateDictionary();
										decimal scope = 0;

										foreach (var projectScopeRecordItem in projectScopeRecords)
										{
											scope += !projectScopeRecordItem["effect"].IsNullOrEmpty() ? (decimal)projectScopeRecordItem["effect"] : 0;
										}

										if (scope > 100)
											scope = 100;

										var projectUpdateRecord = new JObject();
										projectUpdateRecord["id"] = (int)projectRecord["id"];
										projectUpdateRecord["scope"] = scope;
										projectUpdateRecord["updated_by"] = (int)projectRecord["updated_by"];

										var resultBeforeProject = await BeforeCreateUpdate(projectModule, projectUpdateRecord, modelStateScope, appUser.TenantLanguage);

										if (resultBeforeProject < 0 && !modelStateScope.IsValid)
										{
											//ErrorLog.GetDefault(null).Log(new Error(new Exception("Project cannot be updated! Object: " + projectUpdateRecord + " ModelState: " + modelStateScope.ToJsonString())));
											return;
										}

										try
										{
											var resultUpdateProject = await recordRepository.Update(projectUpdateRecord, projectModule);

											if (resultUpdateProject < 1)
											{
												//ErrorLog.GetDefault(null).Log(new Error(new Exception("Project cannot be updated! Object: " + projectUpdateRecord)));
											}
										}
										catch (Exception ex)
										{
											//ErrorLog.GetDefault(null).Log(new Error(ex));
										}
										break;
									case "expenses":
										var expenseSheetModule = await moduleRepository.GetByName("expense_sheet");
										var expensesModule = await moduleRepository.GetByName("expenses");
										var expenseSheetRecord = recordRepository.GetById(expenseSheetModule, (int)record["expense_sheet"]);
										var findRequestExpense = new FindRequest { Filters = new List<Filter> { new Filter { Field = "expense_sheet", Operator = Operator.Equals, Value = (int)record["expense_sheet"], No = 1 } }, Limit = 9999 };
										var expenseRecords = recordRepository.Find(module.Name, findRequestExpense);
										var currencyField = expensesModule.Fields.Single(x => x.Name == "currency");
										var currencyPicklist = await picklistRepository.GetById(currencyField.PicklistId.Value);
										var modelStateExpenseSheet = new ModelStateDictionary();
										decimal totalAmountTry = 0;
										decimal totalAmountEur = 0;
										decimal totalAmountUsd = 0;
										decimal totalAmountGbp = 0;

										foreach (var expenseRecordItem in expenseRecords)
										{
											var amount = !expenseRecordItem["amount"].IsNullOrEmpty() ? (decimal)expenseRecordItem["amount"] : 0;
											var currency = currencyPicklist.Items.Single(x => x.LabelEn == (string)expenseRecordItem["currency"]).SystemCode;

											switch (currency)
											{
												case "try":
													totalAmountTry += amount;
													break;
												case "eur":
													totalAmountEur += amount;
													break;
												case "usd":
													totalAmountUsd += amount;
													break;
												case "gbp":
													totalAmountGbp += amount;
													break;
											}
										}

										var expenseSheetUpdateRecord = new JObject();
										expenseSheetUpdateRecord["id"] = (int)expenseSheetRecord["id"];
										expenseSheetUpdateRecord["total_amount_try"] = totalAmountTry;
										expenseSheetUpdateRecord["total_amount_eur"] = totalAmountEur;
										expenseSheetUpdateRecord["total_amount_usd"] = totalAmountUsd;
										expenseSheetUpdateRecord["total_amount_gbp"] = totalAmountGbp;
										expenseSheetUpdateRecord["updated_by"] = (int)expenseSheetRecord["updated_by"];

										var resultBeforeExpenseSheet = await BeforeCreateUpdate(expenseSheetModule, expenseSheetUpdateRecord, modelStateExpenseSheet, appUser.TenantLanguage);

										if (resultBeforeExpenseSheet < 0 && !modelStateExpenseSheet.IsValid)
										{
											//ErrorLog.GetDefault(null).Log(new Error(new Exception("ExpenseSheet cannot be updated! Object: " + expenseSheetUpdateRecord + " ModelState: " + modelStateExpenseSheet.ToJsonString())));
											return;
										}

										try
										{
											var resultUpdateExpenseSheet = await recordRepository.Update(expenseSheetUpdateRecord, expenseSheetModule);

											if (resultUpdateExpenseSheet < 1)
											{
												//ErrorLog.GetDefault(null).Log(new Error(new Exception("ExpenseSheet cannot be updated! Object: " + expenseSheetUpdateRecord)));
											}
										}
										catch (Exception ex)
										{
											//ErrorLog.GetDefault(null).Log(new Error(ex));
										}
										break;
									case "projects":
										/*//Disabled temporarily
                                        if (module.Fields.Any(x => x.Name == "eoi_coordinator_and_rationale_writer"))
                                        {
                                            if (!record["sector"].IsNullOrEmpty())
                                            {
                                                var projectsModule = await moduleRepository.GetByName("projects");
                                                var valueArray = new JArray();
                                                valueArray.Add((string)record["sector"]);
                                                var findRequestEoiCoordinator = new FindRequest { Filters = new List<Filter> { new Filter { Field = "eoi_coordinator_and_rationale_writer_sectors", Operator = Operator.Contains, Value = valueArray, No = 1 } }, Limit = 1 };
                                                var eoiCoordinatorRecords = recordRepository.Find("human_resources", findRequestEoiCoordinator);
                                                var findRequestTpManager = new FindRequest { Filters = new List<Filter> { new Filter { Field = "tp_manager_and_strategy_writer_sector", Operator = Operator.Contains, Value = valueArray, No = 1 } }, Limit = 1 };
                                                var tpManagerRecords = recordRepository.Find("human_resources", findRequestTpManager);
                                                var modelStateprojects = new ModelStateDictionary();
                                                var projectsUpdateRecord = new JObject();
                                                projectsUpdateRecord["id"] = (int)record["id"];
                                                projectsUpdateRecord["updated_by"] = (int)record["updated_by"];

                                                if (!eoiCoordinatorRecords.IsNullOrEmpty())
                                                    projectsUpdateRecord["eoi_coordinator_and_rationale"] = eoiCoordinatorRecords[0]["id"];

                                                if (!tpManagerRecords.IsNullOrEmpty())
                                                    projectsUpdateRecord["tp_manager_and_strategy"] = tpManagerRecords[0]["id"];

                                                var resultBeforeProjects = await RecordHelper.BeforeCreateUpdate(projectsModule, projectsUpdateRecord, modelStateprojects, appUser.TenantLanguage, moduleRepository, picklistRepository);

                                                if (resultBeforeProjects < 0 && !modelStateprojects.IsValid)
                                                {
                                                    ErrorLog.GetDefault(null).Log(new Error(new Exception("ExpenseSheet cannot be updated! Object: " + projectsUpdateRecord + " ModelState: " + modelStateprojects.ToJsonString())));
                                                    return;
                                                }

                                                try
                                                {
                                                    var resultUpdateProject = await recordRepository.Update(projectsUpdateRecord, projectsModule);

                                                    if (resultUpdateProject < 1)
                                                    {
                                                        ErrorLog.GetDefault(null).Log(new Error(new Exception("Project cannot be updated! Object: " + projectsUpdateRecord)));
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    ErrorLog.GetDefault(null).Log(new Error(ex));
                                                }
                                            }
                                        }
                                        */
										//Core merge
										//await YillikIzinHesaplama((int)record["id"], (int)izinlerCalisan["id"], recordRepository, moduleRepository);
										break;
									case "timesheet_item":
										var timesheetModule = await moduleRepository.GetByName("timesheet");
										var findRequestFields = await GetAllFieldsForFindRequest("timesheet_item");
										var findRequestTimesheetItems = new FindRequest { Fields = findRequestFields, Filters = new List<Filter> { new Filter { Field = "related_timesheet", Operator = Operator.Equals, Value = (int)record["related_timesheet"], No = 1 } }, Limit = 9999 };
										var timesheetItemsRecords = recordRepository.Find(module.Name, findRequestTimesheetItems, false);
										var statusField = timesheetModule.Fields.Single(x => x.Name == "status");
										var statusPicklist = await picklistRepository.GetById(statusField.PicklistId.Value);
										var approvedStatusPicklistItem = statusPicklist.Items.Single(x => x.Value == "approved_second");
										var approvedStatusPicklistItemLabel = appUser.TenantLanguage == "tr" ? approvedStatusPicklistItem.LabelTr : approvedStatusPicklistItem.LabelEn;
										var timesheetRecord = recordRepository.GetById(timesheetModule, (int)record["related_timesheet"]);
										var approved = true;

										if ((string)timesheetRecord["status"] == approvedStatusPicklistItemLabel)
											return;

										if (timesheetItemsRecords.Count == 0)
											return;

										if (!timesheetItemsRecords.IsNullOrEmpty() && timesheetItemsRecords.Count > 0)
										{
											foreach (var timesheetItemsRecord in timesheetItemsRecords)
											{
												if (!approved)
													continue;

												var statusRecord = timesheetItemsRecord["status"].ToString();
												var statusPicklistItem = statusPicklist.Items.Single(x => x.LabelEn == statusRecord);

												if (statusPicklistItem.Value != "approved_second")
												{
													approved = false;
													break;
												}
											}
										}

										if (approved)
										{
											var timesheetRecordUpdate = new JObject();
											timesheetRecordUpdate["id"] = (int)record["related_timesheet"];
											timesheetRecordUpdate["status"] = approvedStatusPicklistItem.Id;
											timesheetRecordUpdate["updated_by"] = (int)record["updated_by"];

											var modelStateTimesheet = new ModelStateDictionary();
											var resultBefore = await BeforeCreateUpdate(timesheetModule, timesheetRecordUpdate, modelStateTimesheet, appUser.TenantLanguage);

											if (resultBefore < 0 && !modelStateTimesheet.IsValid)
											{
												//ErrorLog.GetDefault(null).Log(new Error(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate + " ModelState: " + modelStateTimesheet.ToJsonString())));
												return;
											}

											try
											{
												var resultUpdate = await recordRepository.Update(timesheetRecordUpdate, timesheetModule);

												if (resultUpdate < 1)
												{
													//ErrorLog.GetDefault(null).Log(new Error(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate)));
													return;
												}
											}
											catch (Exception ex)
											{
												//ErrorLog.GetDefault(null).Log(new Error(ex));
												return;
											}

											using (var userRepository = new UserRepository(databaseContext, _configuration))
											{
												var timesheetOwner = await userRepository.GetById((int)record["owner"]);


												var timesheetInfo = timesheetRecord["year"] + "-" + timesheetRecord["term"];
												var timesheetMonth = int.Parse(timesheetRecord["term"].ToString()) - 1;
												var body = "<!DOCTYPE html> <html> <head> <meta name=\"viewport\" content=\"width=device-width\"> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <title></title> <style type=\"text/css\"> @media only screen and (max-width: 620px) { table[class= body] h1 { font-size: 28px !important; margin-bottom: 10px !important; } table[class=body] p, table[class=body] ul, table[class=body] ol, table[class=body] td, table[class=body] span, table[class=body] a { font-size: 16px !important; } table[class=body] .wrapper, table[class=body] .article { padding: 10px !important; } table[class=body] .content { padding: 0 !important; } table[class=body] .container { padding: 0 !important; width: 100% !important; } table[class=body] .main { border-left-width: 0 !important; border-radius: 0 !important; border-right-width: 0 !important; } table[class=body] .btn table { width: 100% !important; } table[class=body] .btn a { width: 100% !important; } table[class=body] .img-responsive { height: auto !important; max-width: 100% !important; width: auto !important; }} @media all { .ExternalClass { width: 100%; } .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } .apple-link a { color: inherit !important; font-family: inherit !important; font-size: inherit !important; font-weight: inherit !important; line-height: inherit !important; text-decoration: none !important; } .btn-primary table td:hover { background-color: #34495e !important; } .btn-primary a:hover { background - color: #34495e !important; border-color: #34495e !important; } } </style> </head> <body class=\"\" style=\"background-color:#f6f6f6;font-family:sans-serif;-webkit-font-smoothing:antialiased;font-size:14px;line-height:1.4;margin:0;padding:0;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"body\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background-color:#f6f6f6;width:100%;\"> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\">&nbsp;</td> <td class=\"container\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;display:block;max-width:580px;padding:10px;width:580px;Margin:0 auto !important;\"> <div class=\"content\" style=\"box-sizing:border-box;display:block;Margin:0 auto;max-width:580px;padding:10px;\"> <!-- START CENTERED WHITE CONTAINER --> <table class=\"main\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background:#fff;border-radius:3px;width:100%;\"> <!-- START MAIN CONTENT AREA --> <tr> <td class=\"wrapper\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;box-sizing:border-box;padding:20px;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;\"> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\"> Dear " + timesheetOwner.FullName + ", <br><br>Your timesheet (" + timesheetInfo + ") is approved. <br><br><br><br><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;box-sizing:border-box;width:100%;\"> <tbody> <tr> <td align=\"left\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;padding-bottom:15px;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;width:auto;\"> <tbody> <tr> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;background-color:#ffffff;border-radius:5px;text-align:center;background-color:#3498db;\"> <a href=\"https://crm.ofisim.com/#/app/timesheet?month=" + timesheetMonth + "\" target=\"_blank\" style=\"text-decoration:underline;background-color:#ffffff;border:solid 1px #3498db;border-radius:5px;box-sizing:border-box;color:#3498db;cursor:pointer;display:inline-block;font-size:14px;font-weight:bold;margin:0;padding:12px 25px;text-decoration:none;background-color:#3498db;border-color:#3498db;color:#ffffff;\">Go to Your Timesheet</a> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table></td> </tr> </table> </td> </tr> <!-- END MAIN CONTENT AREA --> </table> <!-- START FOOTER --> <div class=\"footer\" style=\"clear:both;padding-top:10px;text-align:center;width:100%;\"> <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;\"> <tr> <td class=\"content-block\" style=\"font-family:sans-serif;font-size:14px;vertical-align:top;color:#999999;font-size:12px;text-align:center;\"> <br><span class=\"apple-link\" style=\"color:#999999;font-size:12px;text-align:center;\">Ofisim.com</span> </td> </tr> </table> </div> <!-- END FOOTER --> <!-- END CENTERED WHITE CONTAINER --> </div> </td> <td style=\"font-family:sans-serif;font-size:14px;vertical-align:top;\">&nbsp;</td> </tr> </table> </body> </html>";
												var externalEmail = new Email("Timesheet (" + timesheetInfo + ") Approved", body, _configuration);
												externalEmail.AddRecipient(timesheetOwner.Email);
												externalEmail.AddToQueue(appUser: appUser);
											}


											await CalculateTimesheet(timesheetItemsRecords, appUser, module, timesheetModule);
										}
										break;
									case "human_resources":
										var findRequestIzinlerCalisanPG = new FindRequest
										{
											Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
											Limit = 99999,
											Offset = 0
										};

										var izinlerCalisanPG = recordRepository.Find("izin_turleri", findRequestIzinlerCalisanPG, false).First;
										await YillikIzinHesaplama((int)record["id"], (int)izinlerCalisanPG["id"]);
										break;
									case "calisanlar":

										//Yıllık izin ise calculationlar çalıştırılıyor.
										var findRequestIzinlerCalisan = new FindRequest
										{
											Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
											Limit = 99999,
											Offset = 0
										};

										var izinlerCalisan = recordRepository.Find("izin_turleri", findRequestIzinlerCalisan, false).First;



										var rehberModule = await moduleRepository.GetByName("rehber");
										var calisanModule = await moduleRepository.GetByName("calisanlar");
										var recordRehber = new JObject();

										if (operationType == OperationType.update || operationType == OperationType.delete)
										{
											var findRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "calisan_id", Operator = Operator.Equals, Value = (int)record["id"] } } };
											var recordsRehber = recordRepository.Find("rehber", findRequest);

											if (recordsRehber.IsNullOrEmpty())
											{
												if (operationType == OperationType.update)
													operationType = OperationType.insert;
												else
													return;
											}
											else
											{
												recordRehber = (JObject)recordsRehber[0];
											}
										}
										var calismaDurumuField = calisanModule.Fields.Single(x => x.Name == "calisma_durumu");
										var calismaDurumuPicklist = await picklistRepository.GetById(calismaDurumuField.PicklistId.Value);
										var calismaDurumuPicklistItem = calismaDurumuPicklist.Items.SingleOrDefault(x => x.Value == "active");
										var calismaDurumu = (string)record["calisma_durumu"];
										var isActive = !string.IsNullOrEmpty(calismaDurumu) && calismaDurumuPicklistItem != null && calismaDurumu == (appUser.TenantLanguage == "tr" ? calismaDurumuPicklistItem.LabelTr : calismaDurumuPicklistItem.LabelEn);

										if (operationType != OperationType.delete)
										{
											recordRehber["owner"] = record["owner"];
											recordRehber["ad"] = record["ad"];
											recordRehber["soyad"] = record["soyad"];
											recordRehber["ad_soyad"] = record["ad_soyad"];
											recordRehber["cep_telefonu"] = record["cep_telefonu"];
											recordRehber["e_posta"] = record["e_posta"];
											recordRehber["lokasyon"] = record["lokasyon"];
											recordRehber["sube"] = record["sube"];
											recordRehber["fotograf"] = record["fotograf"];
											recordRehber["departman"] = record["departman"];
											recordRehber["unvan"] = record["unvan"];

											if (!record["is_telefonu"].IsNullOrEmpty())
												recordRehber["is_telefonu"] = record["is_telefonu"];

											if (!record["ozel_cep_telefonu"].IsNullOrEmpty())
												recordRehber["ozel_cep_telefonu"] = record["ozel_cep_telefonu"];

											if (!record["yoneticisi"].IsNullOrEmpty())
											{
												var findRequestYonetici = new FindRequest { Filters = new List<Filter> { new Filter { Field = "calisan_id", Operator = Operator.Equals, Value = (int)record["yoneticisi"] } } };
												var recordsYonetici = recordRepository.Find("rehber", findRequestYonetici);

												if (!recordsYonetici.IsNullOrEmpty())
													recordRehber["yoneticisi"] = recordsYonetici[0]["id"];
											}

											var modelStateRehber = new ModelStateDictionary();
											var resultBefore = await BeforeCreateUpdate(rehberModule, recordRehber, modelStateRehber, appUser.TenantLanguage, convertPicklists: false);

											if (resultBefore < 0 && !modelStateRehber.IsValid)
											{
												//ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be created or updated! Object: " + recordRehber + " ModelState: " + modelStateRehber.ToJsonString())));
												return;
											}

											if (operationType == OperationType.insert && recordRehber["id"].IsNullOrEmpty() && isActive) //create
											{
												recordRehber["calisan_id"] = record["id"];

												//try
												//{
												//    var resultCreate = await recordRepository.Create(recordRehber, rehberModule);

												//    //if (resultCreate < 1)
												//    //    ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be created! Object: " + recordRehber)));
												//}
												//catch (Exception ex)
												//{
												//    //ErrorLog.GetDefault(null).Log(new Error(ex));
												//}
											}
											else //update
											{
												//Core merge
												//if (!isActive)
												//{
												//    if (!recordRehber["id"].IsNullOrEmpty())
												//    {
												//        try
												//        {
												//            var resultDelete = await recordRepository.Delete(recordRehber, rehberModule);

												//            if (resultDelete < 1)
												//                ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be deleted! Object: " + recordRehber)));
												//        }
												//        catch (Exception ex)
												//        {
												//            ErrorLog.GetDefault(null).Log(new Error(ex));
												//        }
												//    }
												//}
												//else
												//{
												//try
												//{
												//    var resultUpdate = await recordRepository.Update(recordRehber, rehberModule);

												//    //if (resultUpdate < 1)
												//    //    ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be updated! Object: " + recordRehber)));
												//}
												//catch (Exception ex)
												//{
												//    //ErrorLog.GetDefault(null).Log(new Error(ex));
												//}
											}
										}
										//if (!record["dogum_tarihi"].IsNullOrEmpty())
										//{
										//    var today = DateTime.Today;
										//    var birthDate = (DateTime)record["dogum_tarihi"];
										//    var age = today.Year - birthDate.Year;

										//    if (birthDate > today.AddYears(-age))
										//        age--;

										//    record["yasi"] = age;
										//}

										//if (!record["ise_baslama_tarihi"].IsNullOrEmpty())
										//{
										//    var timespan = DateTime.UtcNow.Subtract((DateTime)record["ise_baslama_tarihi"]);
										//    record["deneyim_yil"] = Math.Floor(timespan.TotalDays / 365);

										//    if ((int)record["deneyim_yil"] > 0)
										//    {
										//        record["deneyim_ay"] = Math.Floor(timespan.TotalDays / 30) - ((int)record["deneyim_yil"] * 12);
										//    }
										//    else
										//    {
										//        record["deneyim_ay"] = Math.Floor(timespan.TotalDays / 30);
										//    }

										//    var deneyimAyStr = (string)record["deneyim_ay"];

										//    record["toplam_deneyim_firma"] = record["deneyim_yil"] + "." + (deneyimAyStr.Length == 1 ? "0" + deneyimAyStr : deneyimAyStr);
										//    record["toplam_deneyim_firma_yazi"] = record["deneyim_yil"] + " yıl " + deneyimAyStr + " ay";

										//    if (record["onceki_deneyim_yil"].IsNullOrEmpty())
										//        record["onceki_deneyim_yil"] = 0;

										//    if (record["onceki_deneyim_ay"].IsNullOrEmpty())
										//        record["onceki_deneyim_ay"] = 0;

										//    var deneyimYil = (int)record["deneyim_yil"] + (int)record["onceki_deneyim_yil"];
										//    var deneyimAy = (int)record["deneyim_ay"] + (int)record["onceki_deneyim_ay"];

										//    if (deneyimAy > 12)
										//    {
										//        deneyimAy -= 12;
										//        deneyimYil += 1;
										//    }

										//    record["toplam_deneyim"] = deneyimYil + "." + (deneyimAy.ToString().Length == 1 ? "0" + deneyimAy : deneyimAy.ToString());
										//    record["toplam_deneyim_yazi"] = deneyimYil + " yıl " + deneyimAy + " ay";
										//}

										//if (!record["dogum_tarihi"].IsNullOrEmpty() || !record["ise_baslama_tarihi"].IsNullOrEmpty())
										//    await recordRepository.Update(record, calisanModule);
										else//delete
										{
											//try
											//{
											//    var resultDelete = await recordRepository.Delete(recordRehber, rehberModule);

											//    if (resultDelete < 1)
											//        ErrorLog.GetDefault(null).Log(new Error(new Exception("Rehber cannot be deleted! Object: " + recordRehber)));
											//}
											//catch (Exception ex)
											//{
											//    ErrorLog.GetDefault(null).Log(new Error(ex));
											//}
										}
										break;
									case "izinler":
										if (record["calisan"].IsNullOrEmpty())
											return;

										//Yıllık izin ise calculationlar çalıştırılıyor.
										var findRequestIzinler = new FindRequest
										{
											Filters = new List<Filter> { new Filter { Field = "yillik_izin", Operator = Operator.Equals, Value = true, No = 1 } },
											Limit = 99999,
											Offset = 0
										};

										var izinler = recordRepository.Find("izin_turleri", findRequestIzinler, false).First;

										//await YillikIzinHesaplama((int)record["calisan"], izinTuru, recordRepository, moduleRepository);
										if (record["process_status"] != null)
										{
											if ((bool)izinler["yillik_izin"] && operationType == OperationType.update && !record["process_status"].IsNullOrEmpty() && (int)record["process_status"] == 2)
												await YillikIzinHesaplama((int)record["calisan"], (int)izinler["id"]);
											else if ((bool)izinler["yillik_izin"] && operationType == OperationType.delete && !record["process_status"].IsNullOrEmpty() && (int)record["process_status"] == 2)
												await DeleteAnnualLeave((int)record["calisan"], (int)izinler["id"], record);
										}
										else
										{
											if (operationType == OperationType.delete)
											{
												await DeleteAnnualLeave((int)record["calisan"], (int)izinler["id"], record);
											}
											else
											{
												await YillikIzinHesaplama((int)record["calisan"], (int)izinler["id"]);
											}
										}
										break;
									case "masraf_kalemleri":
										try
										{
											var moduleUpdate = await moduleRepository.GetByName("masraflar");
											var masrafKalemiModule = await moduleRepository.GetByName("masraf_kalemleri");
											var masrafCalisanModule = await moduleRepository.GetByName("calisanlar");
											var masrafTuruPicklist = masrafKalemiModule.Fields.Single(x => x.Name == "masraf_turu");
											var odenecekTutarField = masrafKalemiModule.Fields.SingleOrDefault(x => x.Name == "odenecek_tutar");
											var masrafRecord = new JObject();
											bool masrafKalemiCalculate = false;
											if (odenecekTutarField != null)
											{
												masrafRecord = recordRepository.GetById(moduleUpdate, (int)record["masraf"], true, null, true);
											}
											if (odenecekTutarField != null && (masrafRecord["process_status"].IsNullOrEmpty() || (int)masrafRecord["process_status"] == 3))
											{
												masrafKalemiCalculate = true;
												var masrafTuruPicklistItem = await picklistRepository.FindItemByLabel(masrafTuruPicklist.PicklistId.Value, (string)record["masraf_turu"], appUser.TenantLanguage);
												if (masrafTuruPicklistItem.SystemCode == "yemek")
												{
													var yurtIcıDisiPicklist = masrafKalemiModule.Fields.Single(x => x.Name == "yurticiyurtdisi");
													var yurtIcıDisiPicklistItem = await picklistRepository.FindItemByLabel(yurtIcıDisiPicklist.PicklistId.Value, (string)record["yurticiyurtdisi"], appUser.TenantLanguage);
													if (yurtIcıDisiPicklistItem.SystemCode == "yurt_ici")
													{
														var findRequestMasrafCalisan = new FindRequest { Filters = new List<Filter> { new Filter { Field = "owner", Operator = Operator.Equals, Value = record["owner"], No = 1 } }, Limit = 1 };
														var recordsMasrafCalisan = recordRepository.Find("calisanlar", findRequestMasrafCalisan);
														var lokasyonPicklist = masrafCalisanModule.Fields.Single(x => x.Name == "lokasyon");
														var lokasyonPicklistItem = await picklistRepository.FindItemByLabel(lokasyonPicklist.PicklistId.Value, (string)recordsMasrafCalisan.First()["lokasyon"], appUser.TenantLanguage);
														var illerPicklist = masrafKalemiModule.Fields.Single(x => x.Name == "iller");
														var illerPicklistItem = await picklistRepository.FindItemByLabel(illerPicklist.PicklistId.Value, (string)record["iller"], appUser.TenantLanguage);
														if (lokasyonPicklistItem.Value == illerPicklistItem.SystemCode)
														{
															record["odenecek_tutar"] = 24;
														}
														else
														{
															record["odenecek_tutar"] = 30;
														}
													}
													else
													{
														record["odenecek_tutar"] = record["tutar"];
													}
												}
												else
												{
													record["odenecek_tutar"] = record["tutar"];
												}

												await recordRepository.Update(record, masrafKalemiModule);
											}
											var recordUpdate = new JObject();
											var masrafId = (int)record["masraf"];
											decimal totalAmount = 0;
											decimal totalOdenecekTutar = 0;

											var findRequestMasrafKalemi = new FindRequest { Filters = new List<Filter> { new Filter { Field = "masraf", Operator = Operator.Equals, Value = masrafId, No = 1 } }, Limit = 9999 };
											var recordsMasrafKalemi = recordRepository.Find(module.Name, findRequestMasrafKalemi);

											foreach (JObject recordMasrafKalemi in recordsMasrafKalemi)
											{
												decimal amount = 0;

												if (!recordMasrafKalemi["tutar"].IsNullOrEmpty())
													amount = (decimal)recordMasrafKalemi["tutar"];

												totalAmount += amount;

												if (!recordMasrafKalemi["odenecek_tutar"].IsNullOrEmpty())
												{
													decimal odenecekTutar = 0;
													odenecekTutar = (decimal)recordMasrafKalemi["odenecek_tutar"];
													totalOdenecekTutar += odenecekTutar;
												}
											}

											recordUpdate["id"] = masrafId;
											recordUpdate["toplam_tutar"] = totalAmount;
											recordUpdate["updated_by"] = (int)record["updated_by"];

											if (masrafKalemiCalculate)
											{
												recordUpdate["odenecek_toplam_tutar"] = totalOdenecekTutar;
											}

											var resultUpdate = await recordRepository.Update(recordUpdate, moduleUpdate);

											//if (resultUpdate < 1)
											//    ErrorLog.GetDefault(null).Log(new Error(new Exception("toplam_tutar cannot be updated! Object: " + recordUpdate)));
										}
										catch (Exception ex)
										{
											//ErrorLog.GetDefault(null).Log(new Error(ex));
										}
										break;
								}
							}
						}
					}
				}

			}
			catch (Exception ex)
			{
				//ErrorLog.GetDefault(null).Log(new Error(ex));
			}
		}

		public async Task<bool> YillikIzinHesaplama(int userId, int izinTuruId)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				{
					_moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;

					var calisanlarModule = await _moduleRepository.GetByName("calisanlar");
					if (calisanlarModule == null)
					{
						calisanlarModule = await _moduleRepository.GetByName("human_resources");
						if (calisanlarModule == null)
							return false;
					}

					var calisanId = userId;

					var calisan = _recordRepository.GetById(calisanlarModule, calisanId, false);

					if (calisan == null)
						return false;

					var izinTurleri = await _moduleRepository.GetByName("izin_turleri");
					var iseBaslamaTarihi = (string)calisan["ise_baslama_tarihi"];

					var bugun = DateTime.UtcNow;

					if (string.IsNullOrEmpty(iseBaslamaTarihi))
						return false;

					var calismayaBasladigiZaman = DateTime.ParseExact(iseBaslamaTarihi, "MM/dd/yyyy h:mm:ss", null);

					var dayDiff = bugun - calismayaBasladigiZaman;
					var dayDiffMonth = ((bugun.Year - calismayaBasladigiZaman.Year) * 12) + bugun.Month - calismayaBasladigiZaman.Month;
					var dayDiffYear = dayDiff.Days / 365;

					var izinKurali = _recordRepository.GetById(izinTurleri, izinTuruId, false);

					var ekIzin = 0.0;
					var toplamKalanIzin = 0.0;
					var hakedilenIzin = 0.0;

					var yasaGoreIzin = 0.0;
					var kidemeGoreIzin = 0.0;
					var kullanilanYillikIzin = 0.0;
					var devredenIzin = 0.0;

					if (!calisan["sabit_devreden_izin"].IsNullOrEmpty())
					{
						devredenIzin = (double)calisan["sabit_devreden_izin"];
					}

					if (dayDiffYear > 0)
					{
						#region Yıllık izine ek izin süresi tanımlanmışsa tanımlanan ek izin süresini çalışanın profiline tanımlıyoruz.
						if ((bool)izinKurali["yillik_izine_ek_izin_suresi_ekle"])
							ekIzin = (double)izinKurali["yillik_izine_ek_izin_suresi_gun"];
						#endregion

						#region Yaşa göre asgari izin kuralı.
						if ((double)izinKurali["yasa_gore_asgari_izin_gun"] != 0)
						{
							var dogumYili = (string)calisan["dogum_tarihi"];
							if (!String.IsNullOrEmpty(dogumYili))
							{
								var calisanYasi = (bugun - DateTime.ParseExact(dogumYili, "MM/dd/yyyy h:mm:ss", null)).Days / 365;
								if (calisanYasi < 18 || calisanYasi > 50)
								{
									yasaGoreIzin = (double)izinKurali["yasa_gore_asgari_izin_gun"];
								}
							}
						}
						#endregion

						#region Çalışanın çalıştığı yıl hesaplanarak kıdem izinleri hesaplanıyor.
						var findRequestKidemeGoreIzinler = new FindRequest
						{
							Filters = new List<Filter>
					{
						new Filter { Field = "turu", Operator = Operator.Equals, Value = 1, No = 1 },
						new Filter { Field = "calisilan_yil", Operator = Operator.LessEqual, Value = dayDiffYear}
					},
							Limit = 9999,
							SortDirection = SortDirection.Desc
						};

						var kidemIzni = _recordRepository.Find("kideme_gore_yillik_izin_artislari", findRequestKidemeGoreIzinler, false).First;


						if (kidemIzni != null)
							kidemeGoreIzin = (double)kidemIzni["ek_yillik_izin"];
						#endregion

						#region Kıdem ve yaş izinleri kıyaslanarak uygun olan setleniyor.

						if (kidemeGoreIzin + (double)izinKurali["yillik_izin_hakki_gun"] > yasaGoreIzin)
							hakedilenIzin = kidemeGoreIzin + (double)izinKurali["yillik_izin_hakki_gun"];
						else
							hakedilenIzin = yasaGoreIzin;
						#endregion
					}

					/*#region İlk izin kullanımı hakediş zamanı çalışanın işe giriş tarihinden büyük ise kullanıcının izin hakları sıfır olarak kaydediliyor.
					if ((double)izinKurali["ilk_izin_kullanimi_hakedis_zamani_ay"] != 0 && dayDiffMonth < (int)izinKurali["ilk_izin_kullanimi_hakedis_zamani_ay"])
						return false;
					#endregion*/

					#region Bu yıl kullandığı toplam izinler hesaplanıyor.
					var totalUsed = 0.0;

					var year = DateTime.UtcNow.Year;
					var ts = new TimeSpan(0, 0, 0);
					calismayaBasladigiZaman = calismayaBasladigiZaman.Date + ts;

					if (new DateTime(DateTime.UtcNow.Year, calismayaBasladigiZaman.Month, calismayaBasladigiZaman.Day, 0, 0, 0) > DateTime.UtcNow)
						year--;

					var findRequestIzinler = new FindRequest
					{
						Fields = new List<string> { "hesaplanan_alinacak_toplam_izin", "process.process_requests.process_status" },
						Filters = new List<Filter>
				{
					new Filter { Field = "calisan", Operator = Operator.Equals, Value = calisanId, No = 1 },
					new Filter { Field = "baslangic_tarihi", Operator = Operator.GreaterEqual, Value = new DateTime(year, calismayaBasladigiZaman.Month, calismayaBasladigiZaman.Day, 0, 0, 0).ToString("yyyy-MM-dd h:mm:ss"), No = 2 },
					new Filter { Field = "izin_turu", Operator = Operator.Equals, Value = izinTuruId, No = 3 },
					new Filter { Field = "deleted", Operator = Operator.Equals, Value = false, No = 4 },
					new Filter { Field = "process.process_requests.process_status", Operator = Operator.Equals, Value = 2, No = 5 }
				},
						Limit = 9999
					};

					var izinlerRecords = _recordRepository.Find("izinler", findRequestIzinler, false);

					foreach (var izinlerRecord in izinlerRecords)
					{
						if (!izinlerRecord["hesaplanan_alinacak_toplam_izin"].IsNullOrEmpty())
							totalUsed += (double)izinlerRecord["hesaplanan_alinacak_toplam_izin"];
					}

					kullanilanYillikIzin = totalUsed;
					#endregion

					#region Kullanılan izinler öncelikle devreden düşülüyor sonra kalan izninden düşülüyor.

					hakedilenIzin = hakedilenIzin + ekIzin;

					var devredenCounter = devredenIzin - kullanilanYillikIzin;

					devredenIzin = devredenCounter;

					if (devredenCounter < 0)
					{
						devredenIzin = 0;
						toplamKalanIzin = hakedilenIzin + devredenCounter;
					}
					else
					{
						toplamKalanIzin = hakedilenIzin + devredenIzin;
					}

					#endregion

					//var calisanlarModule = await moduleRepository.GetByName("calisanlar");
					try
					{
						var accountRecordUpdate = new JObject();
						accountRecordUpdate["id"] = calisanId;
						accountRecordUpdate["hakedilen_izin"] = hakedilenIzin;
						accountRecordUpdate["devreden_izin"] = devredenIzin;
						accountRecordUpdate["kalan_izin_hakki"] = toplamKalanIzin;


						accountRecordUpdate["updated_by"] = (int)calisan["updated_by"];
						var resultUpdate = await _recordRepository.Update(accountRecordUpdate, calisanlarModule);

						if (resultUpdate < 1)
						{
							//ErrorLog.GetDefault(null).Log(new Error(new Exception("Account (IK) cannot be updated! Object: " + accountRecordUpdate)));
							return false;
						}
					}
					catch (Exception ex)
					{
						//ErrorLog.GetDefault(null).Log(new Error(ex));
						return false;
					}
					return true;
				}
			}
		}

		public async Task<bool> DeleteAnnualLeave(int userId, int izinTuruId, JObject record)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				{
					_moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;

					var calisanlarModule = await _moduleRepository.GetByName("calisanlar");
					if (calisanlarModule == null)
					{
						calisanlarModule = await _moduleRepository.GetByName("human_resources");
						if (calisanlarModule == null)
							return false;
					}

					var calisanId = userId;
					var calisan = _recordRepository.GetById(calisanlarModule, calisanId, false);

					if (calisan == null)
						return false;

					try
					{
						var accountRecordUpdate = new JObject();
						accountRecordUpdate["id"] = calisanId;
						accountRecordUpdate["kalan_izin_hakki"] = (double)calisan["kalan_izin_hakki"] + (double)record["hesaplanan_alinacak_toplam_izin"];


						accountRecordUpdate["updated_by"] = (int)calisan["updated_by"];
						var resultUpdate = await _recordRepository.Update(accountRecordUpdate, calisanlarModule);

						if (resultUpdate < 1)
						{
							//ErrorLog.GetDefault(null).Log(new Error(new Exception("Account (IK) cannot be updated! Object: " + accountRecordUpdate)));
							return false;
						}
						return true;
					}
					catch (Exception ex)
					{
						//ErrorLog.GetDefault(null).Log(new Error(ex));
						return false;
					}
				}
			}
		}

		public async Task<bool> CalculateTimesheet(JArray timesheetItemsRecords, UserItem appUser, Module timesheetItemModule, Module timesheetModule)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
				{
					_moduleRepository.CurrentUser = _recordRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;

					var entryTypeField = timesheetItemModule.Fields.Single(x => x.Name == "entry_type");
					var entryTypePicklist = await _picklistRepository.GetById(entryTypeField.PicklistId.Value);
					var chargeTypeField = timesheetItemModule.Fields.Single(x => x.Name == "charge_type");
					var chargeTypePicklist = await _picklistRepository.GetById(chargeTypeField.PicklistId.Value);
					var placeOfPerformanceField = timesheetItemModule.Fields.Single(x => x.Name == "place_of_performance");
					var placeOfPerformancePicklist = await _picklistRepository.GetById(placeOfPerformanceField.PicklistId.Value);
					var timesheetId = 0;
					var timesheetOwnerEmail = "";
					var totalDaysWorked = 0.0;
					var totalDaysWorkedTurkey = 0.0;
					var totalDaysWorkedNonTurkey = 0.0;
					var totalDaysWorkedPerDiem = 0.0;
					var projectTotals = new Dictionary<int, double>();

					//Calculate timesheet total days
					foreach (var timesheetItemRecord in timesheetItemsRecords)
					{
						timesheetId = (int)timesheetItemRecord["related_timesheet.timesheet.timesheet_id"];
						timesheetOwnerEmail = (string)timesheetItemRecord["owner.users.email"];

						if (timesheetItemRecord["entry_type"].IsNullOrEmpty())
							continue;

						var entryTypePicklistItem = entryTypePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["entry_type"].ToString());

						if (entryTypePicklistItem == null)
							continue;

						var value = double.Parse(entryTypePicklistItem.Value);

						if (entryTypePicklistItem.SystemCode != "per_diem_only")
						{
							totalDaysWorked += value;

							if (!timesheetItemRecord["place_of_performance"].IsNullOrEmpty())
							{
								var placeOfPerformancePicklistItem = placeOfPerformancePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["place_of_performance"].ToString());

								if (placeOfPerformancePicklistItem.SystemCode == "other")
									totalDaysWorkedNonTurkey += value;
								else
									totalDaysWorkedTurkey += value;
							}

							if (!timesheetItemRecord["per_diem"].IsNullOrEmpty() && bool.Parse(timesheetItemRecord["per_diem"].ToString()))
								totalDaysWorkedPerDiem += value;
						}
						else
						{
							totalDaysWorkedPerDiem += value;
						}

						var chargeTypePicklistItem = chargeTypePicklist.Items.Single(x => x.LabelEn == timesheetItemRecord["charge_type"].ToString());

						if (chargeTypePicklistItem == null)
							continue;

						if (chargeTypePicklistItem.Value == "billable")
						{
							var projectId = (int)timesheetItemRecord["selected_project.projects.id"];

							if (projectTotals.ContainsKey(projectId))
								projectTotals[projectId] += value;
							else
								projectTotals.Add(projectId, value);
						}
					}

					//Update timesheet total days
					var timesheetRecordUpdate = new JObject();
					timesheetRecordUpdate["id"] = timesheetId;
					timesheetRecordUpdate["total_days"] = totalDaysWorked;
					timesheetRecordUpdate["total_days_worked_turkey"] = totalDaysWorkedTurkey;
					timesheetRecordUpdate["total_days_worked_non_turkey"] = totalDaysWorkedNonTurkey;
					timesheetRecordUpdate["total_days_perdiem"] = totalDaysWorkedPerDiem;

					try
					{
						var resultUpdate = await _recordRepository.Update(timesheetRecordUpdate, timesheetModule);

						if (resultUpdate < 1)
						{
							//ErrorLog.GetDefault(null).Log(new Error(new Exception("Timesheet cannot be updated! Object: " + timesheetRecordUpdate)));
							return false;
						}
					}
					catch (Exception ex)
					{
						//ErrorLog.GetDefault(null).Log(new Error(ex));
						return false;
					}

					//Update project teams billable timesheet total days
					var findRequestExpert = new FindRequest { Filters = new List<Filter> { new Filter { Field = "e_mail1", Operator = Operator.Is, Value = timesheetOwnerEmail, No = 1 } } };
					var expertRecords = _recordRepository.Find("experts", findRequestExpert, false);

					if (expertRecords.IsNullOrEmpty() || expertRecords.Count < 1)
					{
						//ErrorLog.GetDefault(null).Log(new Error(new Exception("Expert not found! FindRequest: " + findRequestExpert.ToJsonString())));
						return false;
					}

					var expertRecord = expertRecords[0];
					var projectTeamModule = await _moduleRepository.GetByName("project_team");

					foreach (var projectTotal in projectTotals)
					{
						var findRequestProjectTeam = new FindRequest
						{
							Filters = new List<Filter>
					{
						new Filter { Field = "expert", Operator = Operator.Equals, Value = (int)expertRecord["id"], No = 1 },
						new Filter { Field = "project", Operator = Operator.Equals, Value = projectTotal.Key, No = 2 }
					}
						};

						var projectTeamRecords = _recordRepository.Find("project_team", findRequestProjectTeam, false);

						if (projectTeamRecords.IsNullOrEmpty() || projectTeamRecords.Count < 1)
							continue;

						var projectTeamRecordUpdate = new JObject();
						projectTeamRecordUpdate["id"] = (int)projectTeamRecords[0]["id"];
						projectTeamRecordUpdate["timesheet_total_days"] = projectTotal.Value;

						try
						{
							var resultUpdate = await _recordRepository.Update(projectTeamRecordUpdate, projectTeamModule);

							if (resultUpdate < 1)
							{
								//ErrorLog.GetDefault(null).Log(new Error(new Exception("ProjectTeam cannot be updated! Object: " + projectTeamRecordUpdate)));
								return false;
							}
						}
						catch (Exception ex)
						{
							//ErrorLog.GetDefault(null).Log(new Error(ex));
							return false;
						}
					}

					return true;
				}
			}
		}

		public async Task<decimal> CalculateAccountBalance(JObject record, string currency, UserItem appUser, Module currentAccountModule, Picklist currencyPicklistSalesInvoice, Module module)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				{
					_recordRepository.CurrentUser = _currentUser;

					decimal balance = 0;
					switch (currency)
					{
						case "try":
							var accountCurrentAccountsRequestTry = new FindRequest { SortField = "date,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "customer", Operator = Operator.Equals, Value = module.Name == "sales_invoices" ? (int)record["account"] : (int)record["customer"], No = 1 }, new Filter { Field = "currency", Operator = Operator.Is, Value = appUser.TenantLanguage == "tr" ? currencyPicklistSalesInvoice.Items.Single(x => x.SystemCode == "try").LabelTr : currencyPicklistSalesInvoice.Items.Single(x => x.SystemCode == "try").LabelEn, No = 2 } }, Limit = 9999 };
							var accountCurrentAccountsTry = _recordRepository.Find("current_accounts", accountCurrentAccountsRequestTry);
							if (accountCurrentAccountsTry.Count > 0)
							{
								foreach (JObject accountCurrentAccountTry in accountCurrentAccountsTry)
								{
									if ((string)accountCurrentAccountTry["transaction_type_system"] == "sales_invoice")
										balance += (decimal)accountCurrentAccountTry["borc_tl"];
									else
										balance -= (decimal)accountCurrentAccountTry["alacak"];

									accountCurrentAccountTry["bakiye_tl"] = balance;
									await _recordRepository.Update(accountCurrentAccountTry, currentAccountModule);
								}

							}
							break;

						case "eur":
							var accountCurrentAccountsRequestEuro = new FindRequest { SortField = "date,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "customer", Operator = Operator.Equals, Value = module.Name == "sales_invoices" ? (int)record["account"] : (int)record["customer"], No = 1 }, new Filter { Field = "currency", Operator = Operator.Is, Value = appUser.TenantLanguage == "tr" ? currencyPicklistSalesInvoice.Items.Single(x => x.SystemCode == "eur").LabelTr : currencyPicklistSalesInvoice.Items.Single(x => x.SystemCode == "eur").LabelEn, No = 2 } }, Limit = 9999 };
							var accountCurrentAccountsEuro = _recordRepository.Find("current_accounts", accountCurrentAccountsRequestEuro);
							if (accountCurrentAccountsEuro.Count > 0)
							{
								foreach (JObject accountCurrentAccountEuro in accountCurrentAccountsEuro)
								{
									if ((string)accountCurrentAccountEuro["transaction_type_system"] == "sales_invoice")
										balance += (decimal)accountCurrentAccountEuro["borc_euro"];
									else
										balance -= (decimal)accountCurrentAccountEuro["alacak_euro"];

									accountCurrentAccountEuro["bakiye_euro"] = balance;
									await _recordRepository.Update(accountCurrentAccountEuro, currentAccountModule);
								}
							}
							break;

						case "usd":
							var accountCurrentAccountsRequestUsd = new FindRequest { SortField = "date,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "customer", Operator = Operator.Equals, Value = module.Name == "sales_invoices" ? (int)record["account"] : (int)record["customer"], No = 1 }, new Filter { Field = "currency", Operator = Operator.Is, Value = appUser.TenantLanguage == "tr" ? currencyPicklistSalesInvoice.Items.Single(x => x.SystemCode == "usd").LabelTr : currencyPicklistSalesInvoice.Items.Single(x => x.SystemCode == "usd").LabelEn, No = 2 } }, Limit = 9999 };
							var accountCurrentAccountsUsd = _recordRepository.Find("current_accounts", accountCurrentAccountsRequestUsd);
							if (accountCurrentAccountsUsd.Count > 0)
							{
								foreach (JObject accountCurrentAccountUsd in accountCurrentAccountsUsd)
								{
									if ((string)accountCurrentAccountUsd["transaction_type_system"] == "sales_invoice")
										balance += (decimal)accountCurrentAccountUsd["borc_usd"];
									else
										balance -= (decimal)accountCurrentAccountUsd["alacak_usd"];

									accountCurrentAccountUsd["bakiye_usd"] = balance;
									await _recordRepository.Update(accountCurrentAccountUsd, currentAccountModule);
								}
							}
							break;
					}

					return balance;
				}
			}

		}

		public async Task<decimal> CalculateSupplierBalance(JObject record, string currency, UserItem appUser, Module currentAccountModule, Picklist currencyPicklistPurchaseInvoice, Module module)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				{
					_recordRepository.CurrentUser = _currentUser;

					decimal balance = 0;
					switch (currency)
					{
						case "try":
							var accountCurrentAccountsRequestTry = new FindRequest { SortField = "date,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "supplier", Operator = Operator.Equals, Value = module.Name == "purchase_invoices" ? (int)record["tedarikci"] : (int)record["supplier"], No = 1 }, new Filter { Field = "currency", Operator = Operator.Is, Value = appUser.TenantLanguage == "tr" ? currencyPicklistPurchaseInvoice.Items.Single(x => x.SystemCode == "try").LabelTr : currencyPicklistPurchaseInvoice.Items.Single(x => x.SystemCode == "try").LabelEn, No = 2 } }, Limit = 9999 };
							var accountCurrentAccountsTry = _recordRepository.Find("current_accounts", accountCurrentAccountsRequestTry);
							if (accountCurrentAccountsTry.Count > 0)
							{
								foreach (JObject accountCurrentAccountTry in accountCurrentAccountsTry)
								{
									if ((string)accountCurrentAccountTry["transaction_type_system"] == "purchase_invoice")
										balance -= (decimal)accountCurrentAccountTry["alacak"];
									else
										balance += (decimal)accountCurrentAccountTry["borc_tl"];

									accountCurrentAccountTry["bakiye_tl"] = balance;
									await _recordRepository.Update(accountCurrentAccountTry, currentAccountModule);
								}

							}
							break;

						case "eur":
							var accountCurrentAccountsRequestEuro = new FindRequest { SortField = "date,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "supplier", Operator = Operator.Equals, Value = module.Name == "purchase_invoices" ? (int)record["tedarikci"] : (int)record["supplier"], No = 1 }, new Filter { Field = "currency", Operator = Operator.Is, Value = appUser.TenantLanguage == "tr" ? currencyPicklistPurchaseInvoice.Items.Single(x => x.SystemCode == "eur").LabelTr : currencyPicklistPurchaseInvoice.Items.Single(x => x.SystemCode == "eur").LabelEn, No = 2 } }, Limit = 9999 };
							var accountCurrentAccountsEuro = _recordRepository.Find("current_accounts", accountCurrentAccountsRequestEuro);
							if (accountCurrentAccountsEuro.Count > 0)
							{
								foreach (JObject accountCurrentAccountEuro in accountCurrentAccountsEuro)
								{
									if ((string)accountCurrentAccountEuro["transaction_type_system"] == "purchase_invoice")
										balance -= (decimal)accountCurrentAccountEuro["alacak_euro"];
									else
										balance += (decimal)accountCurrentAccountEuro["borc_euro"];

									accountCurrentAccountEuro["bakiye_euro"] = balance;
									await _recordRepository.Update(accountCurrentAccountEuro, currentAccountModule);
								}
							}
							break;

						case "usd":
							var accountCurrentAccountsRequestUsd = new FindRequest { SortField = "date,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "supplier", Operator = Operator.Equals, Value = module.Name == "purchase_invoices" ? (int)record["tedarikci"] : (int)record["supplier"], No = 1 }, new Filter { Field = "currency", Operator = Operator.Is, Value = appUser.TenantLanguage == "tr" ? currencyPicklistPurchaseInvoice.Items.Single(x => x.SystemCode == "usd").LabelTr : currencyPicklistPurchaseInvoice.Items.Single(x => x.SystemCode == "usd").LabelEn, No = 2 } }, Limit = 9999 };
							var accountCurrentAccountsUsd = _recordRepository.Find("current_accounts", accountCurrentAccountsRequestUsd);
							if (accountCurrentAccountsUsd.Count > 0)
							{
								foreach (JObject accountCurrentAccountUsd in accountCurrentAccountsUsd)
								{
									if ((string)accountCurrentAccountUsd["transaction_type_system"] == "purchase_invoice")
										balance -= (decimal)accountCurrentAccountUsd["alacak_usd"];
									else
										balance += (decimal)accountCurrentAccountUsd["borc_usd"];

									accountCurrentAccountUsd["bakiye_usd"] = balance;
									await _recordRepository.Update(accountCurrentAccountUsd, currentAccountModule);
								}
							}
							break;
					}

					return balance;
				}
			}
		}

		public async Task<decimal> CalculateKasaBalance(JObject record, Picklist hareketTipleri, UserItem appUser, Module kasaHareketiModule)
		{

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				{
					_recordRepository.CurrentUser = _currentUser;

					decimal balance = 0;
					var kasaHareketleriRequest = new FindRequest { SortField = "islem_tarihi,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "kasa", Operator = Operator.Equals, Value = (int)record["kasa"], No = 1 } }, Limit = 9999 };
					var kasaHareketleri = _recordRepository.Find("kasa_hareketleri", kasaHareketleriRequest);
					if (kasaHareketleri.Count > 0)
					{
						foreach (JObject kasaHareketi in kasaHareketleri)
						{
							var hareketTipi = hareketTipleri.Items.Single(x => appUser.TenantLanguage == "tr" ? x.LabelTr == (string)kasaHareketi["hareket_tipi"] : x.LabelEn == (string)kasaHareketi["hareket_tipi"]).SystemCode;

							if (hareketTipi == "para_cikisi")
								balance -= (decimal)kasaHareketi["alacak"];
							else
								balance += (decimal)kasaHareketi["borc"];

							kasaHareketi["bakiye"] = balance;
							await _recordRepository.Update(kasaHareketi, kasaHareketiModule);
						}

					}
					return balance;
				}
			}
		}

		public async Task<decimal> CalculateBankaBalance(JObject record, Picklist hareketTipleri, UserItem appUser, Module bankaHareketiModule)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				{
					_recordRepository.CurrentUser = _currentUser;

					decimal balance = 0;
					var bankaHareketleriRequest = new FindRequest { SortField = "islem_tarihi,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "banka", Operator = Operator.Equals, Value = (int)record["banka"], No = 1 } }, Limit = 9999 };
					var bankaHareketleri = _recordRepository.Find("banka_hareketleri", bankaHareketleriRequest);
					if (bankaHareketleri.Count > 0)
					{
						foreach (JObject bankaHareketi in bankaHareketleri)
						{
							var hareketTipi = hareketTipleri.Items.Single(x => appUser.TenantLanguage == "tr" ? x.LabelTr == (string)bankaHareketi["hareket_tipi"] : x.LabelEn == (string)bankaHareketi["hareket_tipi"]).SystemCode;

							if (hareketTipi == "para_cikisi")
								balance -= (decimal)bankaHareketi["alacak"];
							else
								balance += (decimal)bankaHareketi["borc"];

							bankaHareketi["bakiye"] = balance;
							await _recordRepository.Update(bankaHareketi, bankaHareketiModule);
						}
					}

					return balance;
				}
			}
		}

		public async Task<decimal> CalculateStock(JObject record, UserItem appUser, Module stockTransactionModule)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
				{
					_recordRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;

					decimal balance = 0;
					var stockTransactionRequest = new FindRequest { SortField = "transaction_date,id", SortDirection = SortDirection.Asc, Filters = new List<Filter> { new Filter { Field = "product", Operator = Operator.Equals, Value = (int)record["product"], No = 1 } }, Limit = 9999 };
					var stockTransactions = _recordRepository.Find("stock_transactions", stockTransactionRequest);
					if (stockTransactions.Count > 0)
					{
						var transactionTypePicklist = stockTransactionModule.Fields.Single(x => x.Name == "stock_transaction_type");
						var transactionsTypes = await _picklistRepository.GetById(transactionTypePicklist.PicklistId.Value);

						foreach (JObject stockTransaction in stockTransactions)
						{
							var transactionTypePicklistItem = await _picklistRepository.FindItemByLabel(transactionTypePicklist.PicklistId.Value, (string)stockTransaction["stock_transaction_type"], appUser.TenantLanguage);
							if (transactionTypePicklistItem.Value2 == "customer_return" || transactionTypePicklistItem.Value2 == "stock_input")
							{
								balance += (decimal)stockTransaction["quantity"];
							}
							else if (transactionTypePicklistItem.Value2 == "supplier_return" || transactionTypePicklistItem.Value2 == "stock_output")
							{
								balance -= (decimal)stockTransaction["cikan_miktar"];
							}

							stockTransaction["bakiye"] = balance;
							await _recordRepository.Update(stockTransaction, stockTransactionModule);
						}

					}

					return balance;
				}
			}	
		}
	}
}