using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
	//TODO Removed
    [Authorize]
    public class ActiveDirectoryController : Controller
    {
        private IPlatformUserRepository _platformUserRepository;
        public ActiveDirectoryController(IPlatformUserRepository platformUserRepository)
        {
            _platformUserRepository = platformUserRepository;
        }
		
        public async Task<ActionResult> SignUp()
        {
            /*UserItem result = null;
            var userId = await Cache.ApplicationUser.GetId(User.Identity.Name);
            // try to get user object only if user id exists in session cache.
            if (userId != 0)
                result = await Cache.User.Get(userId);


            var adTenant = new ActiveDirectoryTenant();
            adTenant.Name = result.UserName;
            adTenant.Issuer = Guid.NewGuid().ToString();
            adTenant.CreatedAt = DateTime.UtcNow;
            adTenant.AdminConsented = true;
            adTenant.TenantId = result.TenantId;
            adTenant.Confirmed = false;

            using (var dbContext = new PlatformDBContext())
            {
                dbContext.ActiveDirectoryTenants.Add(adTenant);
                dbContext.SaveChanges();
            }*/

            var authorizationRequest = string.Format(
                "https://login.microsoftonline.com/common/oauth2/authorize?response_type=code&client_id={0}&resource={1}&redirect_uri={2}&state={3}",
                Uri.EscapeDataString("7697cae4-0291-4449-8046-7b1cae642982"),
				Uri.EscapeDataString("https://graph.windows.net"),
                Uri.EscapeDataString(new Uri(Request.GetDisplayUrl()).GetLeftPart(UriPartial.Authority) + "/ActiveDirectory/ProcessCode"),
                Uri.EscapeDataString(Guid.NewGuid().ToString())
            );

            authorizationRequest += string.Format("&prompt={0}", Uri.EscapeDataString("admin_consent"));

            return new RedirectResult(authorizationRequest);
        }

        [HttpGet]
        public async Task<ActionResult> ProcessCode(string code, string error, string error_description, string resource, string state)
        {

			if (error == "access_denied")
			{
				ViewBag.Error = "accessDenied";
			}
			return View();
			/*
			var langCookie = Request.Cookies["_lang"];
            var language = langCookie != null ? langCookie : "tr";
            Thread.CurrentThread.CurrentUICulture = language == "tr" ? new CultureInfo("tr-TR") : new CultureInfo("en-GB");
            using (var dbContext = new PlatformDBContext())
            {
                UserItem userItem = null;
                var adTenant = dbContext.ActiveDirectoryTenants.FirstOrDefault(a => a.Issuer == state);
                var userId = await Cache.ApplicationUser.GetId(User.Identity.Name);

                var user = await _platformUserRepository.Get(userId);

                // try to get user object only if user id exists in session cache.
                //if (userId != Guid.Empty)
                    //userItem = AsyncHelpers.RunSync(() => Cache.User.Get(userId));
                if (error == "access_denied")
                {
                    ViewBag.Error = "accessDenied";
                    return View();
                }

                if (adTenant == null)
                    return View("Error");

                var credential = new ClientCredential("7697cae4-0291-4449-8046-7b1cae642982", "J2YHu8tqkM8YJh8zgSj8XP0eJpZlFKgshTehIe5ITvU=");
                var authContext = new AuthenticationContext("https://login.microsoftonline.com/common/");
                var result = await authContext.AcquireTokenByAuthorizationCodeAsync(code, new Uri(new Uri(Request.GetDisplayUrl()).GetLeftPart(UriPartial.Path)), credential);

                var isEmailAvailable = await _platformUserRepository.IsEmailAvailable(result.UserInfo.DisplayableId, 1);
                var crmUserInfo = await _platformUserRepository.Get(userId);

                if (crmUserInfo.Email != result.UserInfo.DisplayableId && !isEmailAvailable)
                {
                    ViewBag.Error = "userExist";
                    return View();
                }


                var issuer = $"https://sts.windows.net/{result.TenantId}/";
                adTenant.Issuer = issuer;
                adTenant.Confirmed = true;

                var tenMinsAgo = DateTime.UtcNow.Subtract(new TimeSpan(0, 10, 0));
                var adTenantsOld = dbContext.ActiveDirectoryTenants.Where(a => !a.Issuer.StartsWith("https") && a.CreatedAt < tenMinsAgo);

                foreach (var adTenantOld in adTenantsOld)
                    dbContext.ActiveDirectoryTenants.Remove(adTenantOld);

                if (!string.IsNullOrWhiteSpace(crmUserInfo.ActiveDirectoryEmail) && crmUserInfo.ActiveDirectoryEmail != result.UserInfo.DisplayableId)
                {
                    ViewBag.Error = "emailNotMatch";
                    return View();
                }

                user.ActiveDirectoryTenantId = adTenant.Id;
                user.ActiveDirectoryEmail = result.UserInfo.DisplayableId;
                await _platformUserRepository.UpdateAsync(user);

                var accountOwner = await _platformUserRepository.Get(user.TenantId.Value);

                if (user.Id != user.TenantId && accountOwner.ActiveDirectoryTenantId == 0)
                {
                    accountOwner.ActiveDirectoryTenantId = adTenant.Id;
                    await _platformUserRepository.UpdateAsync(accountOwner);
                }

                dbContext.SaveChanges();

                return View();
            }*/
		}
    }
}