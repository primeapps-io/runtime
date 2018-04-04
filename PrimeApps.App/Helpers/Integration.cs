using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;

namespace PrimeApps.App.Helpers
{
    public static class Integration
    {
        public static async void InsertSubscriber(RegisterBindingModel model, Warehouse warehouse)
        {
            try
            {
                warehouse.DatabaseName = "Ofisim";
                var appUser = GetAppUser();
                var integrationUserId = int.Parse(ConfigurationManager.AppSettings.Get("OfisimIntegrationUserId"));

                using (var databaseContext = new TenantDBContext(appUser.TenantId))
                {
                    using (var moduleRepository = new ModuleRepository(databaseContext))
                    {
                        using (var recordRepository = new RecordRepository(databaseContext, warehouse))
                        {
                            var moduleAccount = await moduleRepository.GetByName("accounts");
                            var findRequestContact = new FindRequest
                            {
                                Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = model.Email, No = 1 } },
                                Limit = 1
                            };

                            var contactsCurrent = recordRepository.Find("contacts", findRequestContact, false);
                            JObject contactCurrent = null;

                            if (contactsCurrent.Count > 0)
                                contactCurrent = (JObject)contactsCurrent[0];

                            JObject account;

                            if (contactCurrent.IsNullOrEmpty() || contactCurrent["account"].IsNullOrEmpty())
                            {
                                account = CreateAccountRecord(model, integrationUserId);
                                var modelStateAccount = new ModelStateDictionary();
                                var resultBeforeAccount = await RecordHelper.BeforeCreateUpdate(moduleAccount, account, modelStateAccount, appUser.TenantLanguage, moduleRepository, null, false);

                                if (resultBeforeAccount < 0 && !modelStateAccount.IsValid)
                                {
                                    //var error = new Error(new Exception("Ofisim subscriber insert failed. Account create-before has error! Email: " + model.Email + " ModelState: " + modelStateAccount.ToJsonString()), HttpContext.Current);
                                    //ErrorLog.GetDefault(null).Log(error);
                                    return;
                                }

                                try
                                {
                                    var resultCreate = await recordRepository.Create(account, moduleAccount);

                                    if (resultCreate < 1)
                                    {
                                        //var error = new Error(new Exception("Ofisim subscriber insert failed. Account create has error! Email: " + model.Email), HttpContext.Current);
                                        //ErrorLog.GetDefault(null).Log(error);
                                        return;
                                    }

                                    RecordHelper.AfterCreate(moduleAccount, account, appUser, warehouse);
                                }
                                catch (Exception ex)
                                {
                                    //ErrorLog.GetDefault(null).Log(new Error(ex));
                                    return;
                                }
                            }
                            else
                            {
                                var accountCurrent = recordRepository.GetById(moduleAccount, (int)contactCurrent["account"], false);
                                var currentAccountCurrent = accountCurrent;
                                accountCurrent["email_aktivasyonu_c"] = false;
                                accountCurrent["kampanya_kodu_c"] = model.CampaignCode;
                                accountCurrent["dil_c"] = model.Culture == "tr-TR" ? "Türkçe" : "İngilizce";
                                accountCurrent["tenant_id_c"] = 0;
                                accountCurrent["updated_by"] = integrationUserId;
                                accountCurrent["updated_at"] = DateTime.UtcNow;

                                try
                                {
                                    var resultUpdate = await recordRepository.Update(accountCurrent, moduleAccount);

                                    if (resultUpdate < 1)
                                    {
                                        //var error = new Error(new Exception("Ofisim subscriber insert failed. AccountCurrent update has error! Email: " + model.Email), HttpContext.Current);
                                        //ErrorLog.GetDefault(null).Log(error);
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //ErrorLog.GetDefault(null).Log(new Error(ex));
                                }

                                RecordHelper.AfterUpdate(moduleAccount, accountCurrent, currentAccountCurrent, appUser, warehouse);
                                account = accountCurrent;
                            }

                            var moduleContact = await moduleRepository.GetByName("contacts");

                            if (contactCurrent == null)
                            {
                                var contact = CreateContactRecord(model, integrationUserId, true, (int)account["id"]);
                                var modelStateContact = new ModelStateDictionary();
                                var resultBeforeContact = await RecordHelper.BeforeCreateUpdate(moduleContact, contact, modelStateContact, appUser.TenantLanguage, moduleRepository, null, false);

                                if (resultBeforeContact < 0 && !modelStateContact.IsValid)
                                {
                                    //var error = new Error(new Exception("Ofisim subscriber insert failed. Contact create-before has error! Email: " + model.Email + " ModelState: " + modelStateContact.ToJsonString()), HttpContext.Current);
                                    //ErrorLog.GetDefault(null).Log(error);
                                    return;
                                }

                                try
                                {
                                    var resultCreate = await recordRepository.Create(contact, moduleContact);

                                    if (resultCreate < 1)
                                    {
                                        //var error = new Error(new Exception("Ofisim subscriber insert failed. Contact create has error! Email: " + model.Email), HttpContext.Current);
                                        //ErrorLog.GetDefault(null).Log(error);
                                        return;
                                    }

                                    RecordHelper.AfterCreate(moduleContact, contact, appUser, warehouse);
                                }
                                catch (Exception ex)
                                {
                                    //ErrorLog.GetDefault(null).Log(new Error(ex));
                                    return;
                                }
                            }
                            else
                            {
                                var currentContactCurrent = contactCurrent;
                                contactCurrent["first_name"] = model.FirstName;
                                contactCurrent["last_name"] = model.LastName;
                                contactCurrent["full_name"] = model.FirstName + " " + model.LastName;
                                contactCurrent["mobile"] = "0" + model.Phone;
                                contactCurrent["account"] = (int)account["id"];
                                contactCurrent["email_aktivasyonu_c"] = false;
                                contactCurrent["ana_kullanici_c"] = true;
                                contactCurrent["updated_by"] = integrationUserId;
                                contactCurrent["updated_at"] = DateTime.UtcNow;

                                try
                                {
                                    var resultUpdate = await recordRepository.Update(contactCurrent, moduleContact);

                                    if (resultUpdate < 1)
                                    {
                                        //var error = new Error(new Exception("Ofisim subscriber insert failed. ContactCurrent update has error! Email: " + model.Email), HttpContext.Current);
                                        //ErrorLog.GetDefault(null).Log(error);
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //ErrorLog.GetDefault(null).Log(new Error(ex));
                                }

                                RecordHelper.AfterUpdate(moduleContact, contactCurrent, currentContactCurrent, appUser, warehouse);
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

        public static async void InsertUser(RegisterBindingModel model, Warehouse warehouse)
        {
            //warehouse.DatabaseName = "Ofisim";
            //var appUser = GetAppUser();
            //var integrationUserId = int.Parse(ConfigurationManager.AppSettings.Get("OfisimIntegrationUserId"));

            //using (var databaseContext = new TenantDBContext(appUser.TenantId))
            //{
            //    using (var moduleRepository = new ModuleRepository(databaseContext))
            //    {
            //        using (var recordRepository = new RecordRepository(databaseContext, warehouse))
            //        {
            //            var moduleAccount = await moduleRepository.GetByName("accounts");
            //            var findRequestContact = new FindRequest
            //            {
            //                Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = model.Email, No = 1 } },
            //                Limit = 1
            //            };

            //            var contactsCurrent = recordRepository.Find("contacts", findRequestContact, false);
            //            JObject contactCurrent = null;

            //            if (contactsCurrent.Count > 0)
            //                contactCurrent = (JObject)contactsCurrent[0];

            //            int? accountId = null;

            //            //TODO: Pending Share Request, INTEGRATION
            //                var shareRequests = session.QueryOver<crmPendingShareRequests>().Fetch(x => x.Instance).Eager.Where(x => x.Email == model.Email).List<crmPendingShareRequests>();
            //            var shareRequest = shareRequests.FirstOrDefault();

            //            if (shareRequests.Count != 1 || shareRequest == null)
            //            {
            //                var error = new Error(new Exception("Ofisim user insert failed (account). Share request not found or too many share request! Email: " + model.Email), HttpContext.Current);
            //                ErrorLog.GetDefault(null).Log(error);
            //                return;
            //            }

            //            var findRequestContactAdmin = new FindRequest
            //            {
            //                Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = shareRequest.Instance.Admin.email, No = 1 } },
            //                Limit = 1
            //            };

            //            var contactsAdmin = recordRepository.Find("contacts", findRequestContactAdmin, false);
            //            JObject contactAdmin;

            //            if (contactsAdmin.Count < 1)
            //            {
            //                var error = new Error(new Exception("Ofisim user insert failed. ContactAdmin not found! Email: " + model.Email), HttpContext.Current);
            //                ErrorLog.GetDefault(null).Log(error);
            //                return;
            //            }

            //            contactAdmin = (JObject)contactsAdmin[0];
            //            var account = recordRepository.GetById(moduleAccount, (int)contactAdmin["account"], false);

            //            var contactCurrentAccountId = (int?)contactCurrent?["account"];

            //            if (contactCurrentAccountId.HasValue)
            //                accountId = contactCurrentAccountId.Value;
            //            else
            //                accountId = (int)account["id"];


            //            var moduleContact = await moduleRepository.GetByName("contacts");

            //            if (contactCurrent == null)
            //            {
            //                var contact = CreateContactRecord(model, integrationUserId, true, accountId);
            //                var modelStateContact = new ModelStateDictionary();
            //                var resultBeforeContact = await RecordHelper.BeforeCreateUpdate(moduleContact, contact, modelStateContact, appUser.PicklistLanguage, moduleRepository, null, false);

            //                if (resultBeforeContact < 0 && !modelStateContact.IsValid)
            //                {
            //                    var error = new Error(new Exception("Ofisim user insert failed. Contact create-before has error! Email: " + model.Email + " ModelState: " + modelStateContact.ToJsonString()), HttpContext.Current);
            //                    ErrorLog.GetDefault(null).Log(error);
            //                    return;
            //                }

            //                try
            //                {
            //                    var resultCreate = await recordRepository.Create(contact, moduleContact);

            //                    if (resultCreate < 1)
            //                    {
            //                        var error = new Error(new Exception("Ofisim user insert failed. Contact create has error! Email: " + model.Email), HttpContext.Current);
            //                        ErrorLog.GetDefault(null).Log(error);
            //                        return;
            //                    }

            //                    RecordHelper.AfterCreate(moduleContact, contact, appUser, warehouse);
            //                }
            //                catch (Exception ex)
            //                {
            //                    ErrorLog.GetDefault(null).Log(new Error(ex));
            //                    return;
            //                }
            //            }
            //            else
            //            {
            //                var currentContactCurrent = contactCurrent;
            //                contactCurrent["first_name"] = model.FirstName;
            //                contactCurrent["last_name"] = model.LastName;
            //                contactCurrent["full_name"] = model.FirstName + " " + model.LastName;
            //                contactCurrent["mobile"] = "0" + model.Phone;
            //                contactCurrent["email_aktivasyonu_c"] = false;
            //                contactCurrent["ana_kullanici_c"] = false;
            //                contactCurrent["updated_by"] = integrationUserId;
            //                contactCurrent["updated_at"] = DateTime.UtcNow;

            //                try
            //                {
            //                    var resultUpdate = await recordRepository.Update(contactCurrent, moduleContact);

            //                    if (resultUpdate < 1)
            //                    {
            //                        var error = new Error(new Exception("Ofisim subscriber insert failed. ContactCurrent update has error! Email: " + model.Email), HttpContext.Current);
            //                        ErrorLog.GetDefault(null).Log(error);
            //                        return;
            //                    }
            //                }
            //                catch (Exception ex)
            //                {
            //                    ErrorLog.GetDefault(null).Log(new Error(ex));
            //                }

            //                RecordHelper.AfterUpdate(moduleContact, contactCurrent, currentContactCurrent, appUser, warehouse);
            //            }
            //        }
            //    }
            //}
        }

        public static async void UpdateSubscriber(string userEmail, int tenantId, Warehouse warehouse)
        {
            try
            {
                warehouse.DatabaseName = "Ofisim";
                var appUser = GetAppUser();
                var integrationUserId = int.Parse(ConfigurationManager.AppSettings.Get("OfisimIntegrationUserId"));

                using (var databaseContext = new TenantDBContext(appUser.TenantId))
                {
                    using (var moduleRepository = new ModuleRepository(databaseContext))
                    {
                        using (var recordRepository = new RecordRepository(databaseContext, warehouse))
                        {
                            var moduleContact = await moduleRepository.GetByName("contacts");
                            var findRequestContact = new FindRequest
                            {
                                Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = userEmail, No = 1 } },
                                Limit = 1
                            };

                            var contacts = recordRepository.Find("contacts", findRequestContact, false);

                            if (contacts.Count < 1)
                            {
                                //var error = new Error(new Exception("Ofisim subscriber update failed (contact). Contact not found! Email: " + userEmail), HttpContext.Current);
                                //ErrorLog.GetDefault(null).Log(error);
                                return;
                            }

                            if (contacts.Count > 1)
                            {
                                //var error = new Error(new Exception("Ofisim subscriber update failed (contact). Too many contacts found! Email: " + userEmail), HttpContext.Current);
                                //ErrorLog.GetDefault(null).Log(error);
                                return;
                            }

                            var contact = (JObject)contacts[0];
                            var contactCurrent = contact;
                            contact["email_aktivasyonu_c"] = true;
                            contact["updated_by"] = integrationUserId;
                            contact["updated_at"] = DateTime.UtcNow;

                            try
                            {
                                var resultUpdate = await recordRepository.Update(contact, moduleContact);

                                if (resultUpdate < 1)
                                {
                                    //var error = new Error(new Exception("Ofisim subscriber update failed. Contact update has error! Email: " + userEmail), HttpContext.Current);
                                    //ErrorLog.GetDefault(null).Log(error);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                //ErrorLog.GetDefault(null).Log(new Error(ex));
                                return;
                            }

                            RecordHelper.AfterUpdate(moduleContact, contact, contactCurrent, appUser, warehouse);

                            var moduleAccount = await moduleRepository.GetByName("accounts");
                            var findRequestAccount = new FindRequest
                            {
                                Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = userEmail, No = 1 } },
                                Limit = 1
                            };

                            var accounts = recordRepository.Find("accounts", findRequestAccount, false);

                            if (accounts.Count < 1)
                            {
                                //var error = new Error(new Exception("Ofisim subscriber update failed (account). Contact not found! Email: " + userEmail), HttpContext.Current);
                                //ErrorLog.GetDefault(null).Log(error);
                                return;
                            }

                            if (accounts.Count > 1)
                            {
                                //var error = new Error(new Exception("Ofisim subscriber update failed (account). Too many contacts found! Email: " + userEmail), HttpContext.Current);
                                //ErrorLog.GetDefault(null).Log(error);
                                return;
                            }

                            var account = (JObject)accounts[0];
                            var accountCurrent = account;
                            account["tenant_id_c"] = tenantId;
                            account["email_aktivasyonu_c"] = true;
                            account["updated_by"] = integrationUserId;
                            account["updated_at"] = DateTime.UtcNow;

                            try
                            {
                                var resultUpdate = await recordRepository.Update(account, moduleAccount);

                                if (resultUpdate < 1)
                                {
                                    //var error = new Error(new Exception("Ofisim subscriber update failed. Account update has error! Email: " + userEmail), HttpContext.Current);
                                    //ErrorLog.GetDefault(null).Log(error);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                //ErrorLog.GetDefault(null).Log(new Error(ex));
                                return;
                            }

                            RecordHelper.AfterUpdate(moduleAccount, account, accountCurrent, appUser, warehouse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //ErrorLog.GetDefault(null).Log(new Error(ex));
            }
        }

        public static async void UpdateUser(string userEmail, Warehouse warehouse)
        {
            try
            {
                warehouse.DatabaseName = "Ofisim";
                var appUser = GetAppUser();
                var integrationUserId = int.Parse(ConfigurationManager.AppSettings.Get("OfisimIntegrationUserId"));

                using (var databaseContext = new TenantDBContext(appUser.TenantId))
                {
                    using (var moduleRepository = new ModuleRepository(databaseContext))
                    {
                        using (var recordRepository = new RecordRepository(databaseContext, warehouse))
                        {
                            var moduleContact = await moduleRepository.GetByName("contacts");
                            var findRequestContact = new FindRequest
                            {
                                Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = userEmail, No = 1 } },
                                Limit = 1
                            };

                            var contacts = recordRepository.Find("contacts", findRequestContact, false);

                            if (contacts.Count < 1)
                            {
                                //var error = new Error(new Exception("Ofisim subscriber update failed (contact). Contact not found! Email: " + userEmail), HttpContext.Current);
                                //ErrorLog.GetDefault(null).Log(error);
                                return;
                            }

                            if (contacts.Count > 1)
                            {
                                //var error = new Error(new Exception("Ofisim subscriber update failed (contact). Too many contacts found! Email: " + userEmail), HttpContext.Current);
                                //ErrorLog.GetDefault(null).Log(error);
                                return;
                            }

                            var contact = (JObject)contacts[0];
                            var contactCurrent = contact;
                            contact["email_aktivasyonu_c"] = true;
                            contact["updated_by"] = integrationUserId;
                            contact["updated_at"] = DateTime.UtcNow;

                            try
                            {
                                var resultUpdate = await recordRepository.Update(contact, moduleContact);

                                if (resultUpdate < 1)
                                {
                                    //var error = new Error(new Exception("Ofisim subscriber update failed. Contact update has error! Email: " + userEmail), HttpContext.Current);
                                    //ErrorLog.GetDefault(null).Log(error);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                //ErrorLog.GetDefault(null).Log(new Error(ex));
                                return;
                            }

                            RecordHelper.AfterUpdate(moduleContact, contact, contactCurrent, appUser, warehouse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //ErrorLog.GetDefault(null).Log(new Error(ex));
            }
        }

        private static UserItem GetAppUser()
        {
            var userEmail = ConfigurationManager.AppSettings.Get("OfisimTenantEmail");
            var userId = AsyncHelpers.RunSync(() => Cache.ApplicationUser.GetId(userEmail));
            var user = AsyncHelpers.RunSync(() => Cache.User.Get(userId, true));

            return user;
        }

        private static JObject CreateAccountRecord(RegisterBindingModel model, int integrationUserId)
        {
            var account = new JObject();
            account["owner"] = integrationUserId;
            account["created_by"] = integrationUserId;
            account["name"] = "Belirsiz (" + model.FirstName + " " + model.LastName + ")";
            account["mobile"] = "0" + model.Phone;
            account["email"] = model.Email;
            account["email_aktivasyonu_c"] = false;
            account["kampanya_kodu_c"] = model.CampaignCode;
            account["dil_c"] = model.Culture == "tr-TR" ? "Türkçe" : "İngilizce";
            account["tenant_id_c"] = 0;
            account["hesap_turu_c"] = "Ücretsiz Deneme";
            account["reyting_c"] = "İletişime Geçilecek";
            account["arama_durumu_c"] = "1. Arama Yapılacak";

            switch (model.AppID)
            {
                case 1:
                    account["uygulama"] = "Ofisim CRM";
                    break;
                case 2:
                    account["uygulama"] = "Ofisim Kobi";
                    break;
                case 3:
                    account["uygulama"] = "Ofisim Asistan";
                    break;
                case 4:
                    account["uygulama"] = "Ofisim İK";
                    break;
                case 5:
                    account["uygulama"] = "Ofisim Çağrı";
                    break;
                default:
                    account["uygulama"] = "Diğer";
                    break;
            }

            return account;
        }

        private static JObject CreateContactRecord(RegisterBindingModel model, int integrationUserId, bool isMember, int? accountId)
        {
            var contact = new JObject();
            contact["owner"] = integrationUserId;
            contact["created_by"] = integrationUserId;
            contact["first_name"] = model.FirstName;
            contact["last_name"] = model.LastName;
            contact["account"] = accountId;
            contact["mobile"] = "0" + model.Phone;
            contact["email"] = model.Email;
            contact["email_aktivasyonu_c"] = false;
            contact["ana_kullanici_c"] = isMember;

            return contact;
        }
    }
}