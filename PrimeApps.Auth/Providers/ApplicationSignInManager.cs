using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.UI;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.Auth.Providers
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser>
    {
        private IHttpContextAccessor _context;
        private UserManager<ApplicationUser> _userManager;

        public ApplicationSignInManager(UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor = null, ILogger<SignInManager<ApplicationUser>> logger = null, IAuthenticationSchemeProvider schema = null)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schema)
        {
            _context = contextAccessor;
            _userManager = userManager;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string email, string password, bool isPersistent, bool shouldLockout)
        {
            var clientId = AuthHelper.GetQueryValue(HttpUtility.ParseQueryString(_context.HttpContext.Request.QueryString.Value).Get("returnUrl"), "client_id");
            var applicationRepository = (IApplicationRepository)_context.HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
            var application = await applicationRepository.GetByNameAsync(clientId);
            var externalLogin = application.Setting.ExternalAuth != null ? JObject.Parse(application.Setting.ExternalAuth) : null;

            if (externalLogin != null)
            {
                var actions = (JArray)externalLogin["actions"];
                var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "login").FirstOrDefault();

                if (action == null)
                    return await base.PasswordSignInAsync(email, password, isPersistent, shouldLockout);

                var obj = new JObject
                {
                    ["email"] = email,
                    ["password"] = password
                };

                var result = await ExternalAuthHelper.Login(externalLogin, action, obj);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    await base.SignInAsync(user, isPersistent);
                }

                return await Task.FromResult(result);
            }

            return await base.PasswordSignInAsync(email, password, isPersistent, shouldLockout);
        }
    }
}