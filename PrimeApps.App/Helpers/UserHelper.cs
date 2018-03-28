using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ElmahCore;
using Hangfire;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Entities.Platform.Identity;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Jobs.QueueAttributes;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories;

namespace PrimeApps.App.Helpers
{
    public static class UserHelper
    {
        public static async Task<string> AddUser(AddUserBindingModel request, string culture, string currency, string picklistLanguage, int appId, string adminUserEmail, int tenantId, ApplicationUserManager userManager, IUserRepository userRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, Warehouse warehouse, PlatformUser applicationUser = null, string password = "")
        {
            if (applicationUser == null)
            {
                if (string.IsNullOrWhiteSpace(password))
                    password = Utils.GenerateRandomUnique(8);

                applicationUser = new PlatformUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.Phone,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Culture = culture,
                    Currency = currency,
                    CreatedAt = DateTime.Now,
                    AppId = appId
                };

                var result = await userManager.CreateAsync(applicationUser, password);

                if (!result.Succeeded)
                    throw new Exception();
            }

            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser.Id);
            var confirmResponse = await userManager.ConfirmEmailAsync(applicationUser.Id, confirmationToken);

            if (!confirmResponse.Succeeded)
                throw new Exception();

            var registerModel = new RegisterBindingModel();
            registerModel.Email = request.Email;
            registerModel.FirstName = request.FirstName;
            registerModel.LastName = request.LastName;
            registerModel.Password = password;
            registerModel.License = "F89E4FBF-A50F-40BA-BBEC-FE027F3F1524"; //Free license

            //HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.InsertUser(registerModel, warehouse));

            var user = await platformUserRepository.Get(applicationUser.Id);

            var tenantUser = new TenantUser
            {
                Id = user.Id,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.LastName}",
                Picture = "",
                IsActive = true,
                IsSubscriber = false,
                Culture = culture,
                Currency = currency,
                CreatedAt = DateTime.UtcNow,
                CreatedByEmail = adminUserEmail
            };

            await userRepository.CreateAsync(tenantUser);
            await profileRepository.AddUserAsync(user.Id, request.ProfileId);
            await roleRepository.AddUserAsync(user.Id, request.RoleId);

            user.TenantId = tenantId;

            await platformUserRepository.UpdateAsync(user);
            await Cache.Tenant.UpdateRoles(tenantId);
            await Cache.Tenant.UpdateProfiles(tenantId);
            await Cache.ApplicationUser.Add(user.Email, user.Id);
            await Cache.User.Get(user.Id);

            return password;
        }

        public static async Task<string> AddUserToNewTenant(AddUserBindingModel request, string culture, string currency, string picklistLanguage, int appId, string adminUserEmail, int tenantId, ApplicationUserManager userManager, IPlatformUserRepository platformUserRepository, IUserRepository userRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, Warehouse warehouse, PlatformUser platformUser = null, string password = "")
        {
            if (platformUser == null)
            {
                if (string.IsNullOrWhiteSpace(password))
                    password = Utils.GenerateRandomUnique(8);

                platformUser = new PlatformUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.Phone,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Culture = culture,
                    Currency = currency,
                    CreatedAt = DateTime.Now,
                    AppId = appId
                };

                var result = await userManager.CreateAsync(platformUser, password);

                if (!result.Succeeded)
                    throw new Exception();
            }

            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(platformUser.Id);
            var confirmResponse = await userManager.ConfirmEmailAsync(platformUser.Id, confirmationToken);

            if (!confirmResponse.Succeeded)
                throw new Exception();

            var registerModel = new RegisterBindingModel();
            registerModel.Email = request.Email;
            registerModel.FirstName = request.FirstName;
            registerModel.LastName = request.LastName;
            registerModel.Password = password;
            registerModel.License = "F89E4FBF-A50F-40BA-BBEC-FE027F3F1524"; //Free licenseuserRepository

            //HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.InsertUser(registerModel, warehouse));

            var user = await platformUserRepository.Get(platformUser.Id);


            var tenantUser = new TenantUser
            {
                Id = user.Id,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.LastName}",
                Picture = "",
                IsActive = true,
                IsSubscriber = false,
                Culture = culture,
                Currency = currency,
                CreatedAt = DateTime.UtcNow,
                CreatedByEmail = adminUserEmail
            };

            await userRepository.CreateAsync(tenantUser);


            await profileRepository.AddUserAsync(user.Id, request.ProfileId);
            await roleRepository.AddUserAsync(user.Id, request.RoleId);

            user.TenantId = tenantId;
            user.EmailConfirmed = true;
            await platformUserRepository.UpdateAsync(user);
            //await Cache.Workgroup.UpdateRoles(instanceId);
            //await Cache.Workgroup.UpdateProfiles(instanceId);
            await Cache.ApplicationUser.Add(user.Email, user.Id);
            await Cache.User.Get(user.Id);

            return password;
        }

        [CommonQueue, AutomaticRetry(Attempts = 0), DisableConcurrentExecution(360)]
        public static async Task AddUserBulk(int tenantId, string apiUrl, string accessToken, bool activeDirectory = false)
        {
            var errorList = new JArray();
            var successList = new JArray();
            using (var databaseContext = new TenantDBContext(tenantId))
            using (var platformDbContext = new PlatformDBContext())
            using (var platformWarehouseRepository = new PlatformWarehouseRepository(platformDbContext))
            using (var analyticRepository = new AnalyticRepository(databaseContext))
            {
                var warehouse = new Model.Helpers.Warehouse(analyticRepository);

                var warehouseEntity = await platformWarehouseRepository.GetByTenantId(tenantId);

                if (warehouseEntity != null)
                    warehouse.DatabaseName = warehouseEntity.DatabaseName;
                else
                    warehouse.DatabaseName = "0";

                using (var moduleRepository = new ModuleRepository(databaseContext))
                {
                    using (var recordRepository = new RecordRepository(databaseContext, warehouse))
                    {

                        var module = await moduleRepository.GetByName("calisanlar");

                        if (module == null)
                            return;

                        var findRequestCalisan = new FindRequest
                        {
                            Filters = new List<Filter>
                            {
                                new Filter {Field = "deleted", Operator = Operator.Equals, Value = false, No = 1}
                            },
                            Limit = 99999,
                            Offset = 0
                        };

                        var calisanlar = recordRepository.Find(module.Name, findRequestCalisan, false);
                        foreach (var calisan in calisanlar)
                        {
                            if (calisan["e_posta"].IsNullOrEmpty())
                            {
                                errorList.Add((string)calisan["id"]);
                                continue;
                            }

                            using (var client = new HttpClient())
                            {
                                client.BaseAddress = new Uri(apiUrl);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                                var content = new JObject();
                                content["email"] = (string)calisan["e_posta"];
                                content["firstName"] = (string)calisan["ad"];
                                content["lastName"] = (string)calisan["soyad"];
                                content["profileId"] = 1;
                                content["roleId"] = 1;
                                content["isOfficeUser"] = activeDirectory;
                                content["notCheckIsAdmin"] = true;

                                var stringContent = new StringContent(content.ToJsonString(), Encoding.UTF8, "application/json");

                                var clientResponse = await client.PostAsync(apiUrl, stringContent);

                                if (clientResponse.IsSuccessStatusCode)
                                {
                                    successList.Add((string)calisan["id"]);
                                }
                                else
                                {
                                    errorList.Add((string)calisan["id"]);
                                }
                            }

                        }
                    }
                }
            }
            var response = new JArray {
                errorList,
                successList
            };

            if (errorList.Count > 0)
                ErrorLog.GetDefault(null).Log(new Error(new Exception(errorList.ToJsonString())));
        }
    }
}