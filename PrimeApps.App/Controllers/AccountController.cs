using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.App.Providers;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using Npgsql;
using PrimeApps.App.Extensions;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers.QueryTranslation;
using ChallengeResult = PrimeApps.App.Results.ChallengeResult;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Hangfire;
using Microsoft.AspNetCore.WebUtilities;

namespace PrimeApps.App.Controllers
{
    [Route("api/account")]
    public class AccountController : BaseController
    {
        private const string LocalLoginProvider = "Local";
        private IRecordRepository _recordRepository;
        private IPlatformUserRepository _platformUserRepository;
        private ITenantRepository _tenantRepository;
        private Warehouse _warehouse;

        /*public AccountController(IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, Warehouse warehouse) : this(recordRepository, platformUserRepository, tenantRepository, warehouse)
        {
        }*/

        public AccountController(IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, Warehouse warehouse) : this()
        {
            _recordRepository = recordRepository;
            _warehouse = warehouse;
            _platformUserRepository = platformUserRepository;
            _tenantRepository = tenantRepository;

            //Set warehouse database name Ofisim to integration
            //_warehouse.DatabaseName = "Ofisim";
        }
        public AccountController() { }

        /*public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }*/

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET account/user_info
        /*[Route("user_info")]
        public UserInfoViewModel GetUserInfo()
        {
            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }*/

        // POST account/logout
        [Route("logout")]
        public IActionResult Logout()
        {
            //Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }
	}
}

