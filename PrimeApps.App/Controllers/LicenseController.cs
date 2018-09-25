using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.User;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.SqlServer.Management.Sdk.Differencing.SPI;

namespace PrimeApps.App.Controllers
{
    [Route("api/License")]
    public class LicenseController : ApiBaseController
    {
        private ISettingRepository _settingRepository;
        private IPlatformUserRepository _platformUserRepository;
        private ITenantRepository _tenantRepository;
        private IUserRepository _userRepository;
        public LicenseController(ISettingRepository settingRespository, IPlatformUserRepository platformUserRepository, IUserRepository userRepository, ITenantRepository tenantRepository)
        {
            _settingRepository = settingRespository;
            _userRepository = userRepository;
            _platformUserRepository = platformUserRepository;
            _tenantRepository = tenantRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_settingRepository);
            SetCurrentUser(_userRepository);

            base.OnActionExecuting(context);
        }

        ///// <summary>
        ///// Gets all license types.
        ///// </summary>
        ///// <returns>IList{DTO.LicenseDTO}.</returns>
        //[Route("GetAll")]
        //[ResponseType(typeof(LicenseUpgradeResult))]
        //[HttpPost]
        //public IActionResult GetAll()
        //{
        //    IList<LicenseDTO> paidLicenses = new List<LicenseDTO>();
        //    LicenseUpgradeResult result = null;
        //    using (ISession session = Provider.GetSession())
        //    {
        //        ///get all upgradeable paid licenses
        //        paidLicenses = crmLicenses.GetAll(AppUser.Currency, Provider.GetSession()).Where(x => x.Price > 0 && x.Title.Contains("Silver")).OrderBy(x => x.Price).ToList();

        //        result = crmLicenses.CalculateLicenseUpgradePrices(AppUser.GlobalId, paidLicenses, session);
        //    }

        //    return Ok(result);
        //}

        ///// <summary>
        ///// Upgrades and Downgrades license of the user if it is available.
        ///// </summary>
        ///// <param name="LicenseID">The license identifier.</param>
        //[Route("Upgrade")]
        //[ResponseType(typeof(void))]
        //[HttpPost]
        //public async Task<IActionResult> Upgrade(UpgradeRequest request)
        //{
        //    crmUser user = null;
        //    int currentWorkgroupCount = 0;
        //    LicenseItem currentLicense = null;
        //    LicenseDTO licenseToUpgrade = null;
        //    using (ISession session = Provider.GetSession())
        //    {
        //        using (ITransaction transaction = session.BeginTransaction())
        //        {
        //            /// get the license that user wants to upgrade.
        //            licenseToUpgrade = crmLicenses.GetDTOByID(request.LicenseID, AppUser.Currency, session).SingleOrDefault();

        //            ///safety check.
        //            if (licenseToUpgrade == null) return Ok();


        //            /// get workgroup count of user to check if it is applicable for current license.
        //            currentWorkgroupCount = Provider.GetSession().QueryOver<crmInstance>().Where(x => x.Admin.ID == AppUser.GlobalId).RowCount();

        //            /// get current license from users session with current usage
        //            currentLicense = await Cache.License.Get(AppUser.GlobalId);

        //            if (currentLicense.LicenseUsage.DownloadSize > licenseToUpgrade.DownloadLimit ||
        //                currentLicense.LicenseUsage.FileStorageSize + currentLicense.LicenseUsage.OtherStorageSize > licenseToUpgrade.StorageSize ||
        //                (currentWorkgroupCount > 1 && licenseToUpgrade.IsSingleWorkgroupLimited))
        //            {
        //                /// The chosen license is not applicable.
        //                return Ok();
        //            }

        //            ///get user.
        //            user = session.Get<crmUser>(AppUser.GlobalId);

        //            if (request.IsFreeLicense)
        //            {
        //                crmInstance inst = new crmInstance();

        //                inst = new crmInstance();
        //                inst.Title = string.Format(Resources.System.Instance.UsersInstance, user.firstName);
        //                inst.Admin = user;
        //                session.Save(inst);

        //                //assign new instance id's to user and update user.
        //                user.defaultInstanceID = inst.ID;
        //                user.lastInstanceID = inst.ID;
        //                session.Update(user);

        //                //create instance relation for the new user.
        //                crmInstanceRelation relation = new crmInstanceRelation();
        //                relation.Instance = inst;
        //                relation.User = user;
        //                session.Save(relation);

        //                /// Update additional license
        //                crmAddonLicenseType freeDaysAddonLicenseType = session.Get<crmAddonLicenseType>(Guid.Parse("D1C97898-441A-47DA-9A4D-0501CDE49782"));
        //                crmAddonLicense.UpdateUsersFreeAddonLicense(user.ID);

        //                /// create first invoice, set it's payment date same with the additional licenses end date.
        //                crmInvoice firstInvoice = new crmInvoice()
        //                {
        //                    User = user,
        //                    Amount = 0,
        //                    InvoiceDate = DateTime.UtcNow.AddDays(freeDaysAddonLicenseType.Value),
        //                    Status = PaymentStatusEnum.Unpaid,
        //                    Type = InvoiceTypeEnum.Periodic,
        //                    Frequency = user.PaymentMethod.Frequency
        //                };

        //                session.Save(firstInvoice);

        //                /// Update previous unpaid invoice
        //                var unpaidInvoice = crmInvoice.GetUsersUnpaidInvoice(user.ID);
        //                unpaidInvoice.Status = PaymentStatusEnum.Paid;
        //                session.Update(unpaidInvoice);

        //                /// assign first invoice as the next invoice of the user.
        //                user.NextInvoice = firstInvoice;
        //                session.Update(user);
        //            }

        //            IList<LicenseDTO> licenseToCalculate = new List<LicenseDTO>();
        //            licenseToCalculate.Add(licenseToUpgrade);

        //            LicenseUpgradeResult upgradeResult = crmLicenses.CalculateLicenseUpgradePrices(AppUser.GlobalId, licenseToCalculate, session);

        //            /// get the calculation results
        //            CalculatedLicense calculation = upgradeResult.AvailableLicenses.SingleOrDefault();

        //            /// this is not an upgradable
        //            if (!calculation.IsUpgradable) return Ok();


        //            if (calculation.Type == InvoiceTypeEnum.OneTimeOnly)
        //            {
        //                /// An invoice needs to be created because this user uses an annual payment method. Remaining balance is not enough and it is not the last month of next invoice.
        //                crmInvoice.InvoiceUpgradedLicense(calculation, currentLicense.License, user, session);
        //            }

        //            /// update users license in database.
        //            user.license = session.Get<crmLicenses>(request.LicenseID);
        //            session.Update(user);

        //            if (upgradeResult.IsFrequencyChangeAllowed && user.PaymentMethod.Frequency != request.Frequency && request.Frequency != FrequencyEnum.None)
        //            {
        //                /// update frequency of the
        //                user.PaymentMethod.Frequency = request.Frequency;
        //                user.NextInvoice.Frequency = request.Frequency;
        //                session.Update(user.NextInvoice);
        //            }

        //            /// campaign lost.
        //            user.PaymentMethod.CampaignCode = String.Empty;
        //            user.PaymentMethod.UsageAmountOfCampaign = 0;
        //            session.Update(user.PaymentMethod);

        //            //Update license membership type on Ofisim CRM account
        //            //HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.UpdateLicenseMembershipType(AppUser.Email, AppUser.GlobalId, request.Frequency));

        //            transaction.Commit();
        //        }
        //    }

        //    /// set new license to current license object, then update all roles and notify clients.
        //    currentLicense.License = licenseToUpgrade;
        //    await Cache.License.AddOrUpdate(AppUser.GlobalId, currentLicense);

        //    return Ok();
        //}

        [Route("GetUserLicenseStatus")]
        [HttpPost]
        public async Task<IActionResult> GetUserLicenseStatus()
        {
            UserLicenseDTO userLicense = new UserLicenseDTO();

            Model.Entities.Platform.Tenant tenant = await _tenantRepository.GetWithLicenseAsync(AppUser.TenantId);
            userLicense.Total = tenant.License.UserLicenseCount;
            int usedLicenseCount = await _userRepository.GetTenantUserCount();
            userLicense.Used = usedLicenseCount;

            return Ok(userLicense);
        }

        [Route("GetModuleLicenseCount")]
        [HttpPost]
        public async Task<int> GetModuleLicenseCount()
        {
            return await _platformUserRepository.GetTenantModuleLicenseCount(AppUser.TenantId);
        }

        [Route("SetUserLicenseCount"), HttpGet]

        public async Task<IActionResult> SetUserLicenseCount(int tenantId, int count)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);
            var subscriberTenant = await _tenantRepository.GetAsync(tenantId);
            subscriberTenant.License.UserLicenseCount = count;
            await _tenantRepository.UpdateAsync(subscriberTenant);

            return Ok();
        }

        [Route("SetModuleLicenseCount")]
        [HttpGet]
        public async Task<IActionResult> SetModuleLicenseCount([FromQuery(Name = "tenantId")]int tenantId, [FromQuery(Name = "count")]int count)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var subscriberTenant = await _tenantRepository.GetAsync(tenantId);

            subscriberTenant.License.ModuleLicenseCount = count;

            await _tenantRepository.UpdateAsync(subscriberTenant);

            return Ok();
        }

        [Route("SetSoftPhoneLicenseCount")]
        [HttpGet]
        public async Task<IActionResult> SetSoftPhoneLicenseCount([FromQuery(Name = "tenantId")]int tenantId, [FromQuery(Name = "count")]int count)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var subscriberTenant = await _tenantRepository.GetAsync(tenantId);

            subscriberTenant.License.SipLicenseCount = count;

            await _tenantRepository.UpdateAsync(subscriberTenant);


            return Ok();
        }

        [Route("SetUserAnalyticsLicense")]
        [HttpGet]
        public async Task<IActionResult> SetUserAnalyticsLicense([FromQuery(Name = "email")]string email, [FromQuery(Name = "active")]bool active)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var subscriberUser = await _platformUserRepository.Get(email);
            var user = await _userRepository.GetByEmail(email);
            var licenseSetting = await _settingRepository.GetByKeyAsync("HasAnalyticsLicense", subscriberUser.Id);
            if (licenseSetting == null && active)
            {
                var setting = new Setting
                {
                    Type = SettingType.Custom,
                    Key = "HasAnalyticsLicense",
                    Value = active.ToString(),
                    CreatedById = subscriberUser.Id,
                    UserId = user.Id
                };
                var result = await _settingRepository.Create(setting);

                if (result < 1)
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            }
            else
            {
                licenseSetting.Value = active.ToString();
                await _settingRepository.Update(licenseSetting);
            }

            return Ok();
        }

        /// <summary>
        /// sets ispaidcustomer value in crmusers table
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [Route("SetPaidCustomer")]
        [HttpGet]
        public async Task<IActionResult> SetPaidCustomer([FromQuery(Name = "tenantId")]int tenantId)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var tenant = await _tenantRepository.GetAsync(tenantId);

            tenant.License.IsPaidCustomer = !tenant.License.IsPaidCustomer;
            await _tenantRepository.UpdateAsync(tenant);


            return Ok();
        }

        /// <summary>
        /// deactivates customer and users
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [Route("DeactivateAccount")]
        [HttpGet]
        public async Task<IActionResult> DeactivateAccount([FromQuery(Name = "tenantId")]int tenantId)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var users = await _platformUserRepository.GetAllByTenant(tenantId);
            var tenant = await _tenantRepository.GetAsync(tenantId);
            _userRepository.DbContext.TenantId = tenantId;

            tenant.License.IsDeactivated = true;
            tenant.License.DeactivatedAt = DateTime.UtcNow;

            foreach (var user in users)
            {
                await _platformUserRepository.UpdateAsync(user);
                var userTenant = await _userRepository.GetById(tenantId);

                if (userTenant != null)
                {
                    userTenant.IsActive = false;
                    await _userRepository.UpdateAsync(userTenant);
                }
            }

            await _tenantRepository.UpdateAsync(tenant);

            return Ok();
        }

        /// <summary>
        /// activates customer and users
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [Route("ActivateAccount")]
        [HttpGet]
        public async Task<IActionResult> ActivateAccount([FromQuery(Name = "tenantId")]int tenantId)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var users = await _platformUserRepository.GetAllByTenant(tenantId);
            var tenant = await _tenantRepository.GetAsync(tenantId);

            _userRepository.DbContext.TenantId = tenantId;
            foreach (var user in users)
            {
                await _platformUserRepository.UpdateAsync(user);

                _userRepository.DbContext.TenantId = tenantId;
                var userTenant = await _userRepository.GetById(user.Id);

                if (userTenant != null)
                {
                    userTenant.IsActive = true;
                    await _userRepository.UpdateAsync(userTenant);
                }
            }

            tenant.License.IsDeactivated = false;
            tenant.License.IsSuspended = false;
            tenant.License.IsPaidCustomer = true;
            await _tenantRepository.UpdateAsync(tenant);

            return Ok();
        }
        /// <summary>
        /// Suspends customer and users
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [Route("SuspendAccount")]
        [HttpGet]
        public async Task<IActionResult> SuspendAccount([FromQuery(Name = "tenantId")]int tenantId)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var tenant = await _tenantRepository.GetAsync(tenantId);

            tenant.License.IsSuspended = true;
            tenant.License.SuspendedAt = DateTime.UtcNow;

            _userRepository.DbContext.TenantId = tenantId;
            var tenantUsers = await _userRepository.GetAllAsync();

            foreach (var tenantUser in tenantUsers)
            {
                tenantUser.IsActive = false;
                await _userRepository.UpdateAsync(tenantUser);
            }

            return Ok();
        }
    }
}
