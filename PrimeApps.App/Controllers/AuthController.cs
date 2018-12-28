using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    public class AuthController : Controller
    {
        //private ApplicationSignInManager _signInManager;
        private readonly IStringLocalizer<AuthController> _localizer;
        private IPlatformRepository _platformRepository;
        private IConfiguration _configuration;
        private ICacheHelper _cacheHelper;

        public AuthController(IStringLocalizer<AuthController> localizer, IPlatformRepository platformRepository, IConfiguration configuration, ICacheHelper cacheHelper)
        {
            _localizer = localizer;
            _platformRepository = platformRepository;
            _configuration = configuration;
            _cacheHelper = cacheHelper;
        }

		/*public async Task<ActionResult> Authorize()
        {
            var claims = new ClaimsPrincipal(User).Claims.ToArray();
            var identity = new ClaimsIdentity(claims, "Bearer");
            //Authentication.SignIn(identity);
            var token = await HttpContext.GetTokenAsync("access_token");
            return Redirect(Request.Scheme + "://" + Request.Host.Value + "#access_token=" + token);
        }*/

		public async Task<ActionResult> Login(string returnUrl, string language = null, string error = null, string success = "")
		{
			var lang = GetLanguage();
			if (language != null)
			{
				lang = language;
				SetLanguae(lang);
			}
			ViewBag.Success = success;
			ViewBag.Lang = lang;
			ViewBag.Error = error;
			ViewBag.ReturnUrl = returnUrl;

			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);

			if (!string.IsNullOrWhiteSpace(ViewBag.AppInfo["language"].Value))
			{
				SetLanguae(ViewBag.AppInfo["language"].Value);
				ViewBag.Lang = ViewBag.AppInfo["language"].Value;
			}

			return View();
		}

		[Route("login"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginBindingModel model, string returnUrl)
		{
			var url = new Uri(Request.GetDisplayUrl()).Host;
			var lang = GetLanguage();
			ViewBag.Lang = lang;
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);
			model.Email = model.Email.Replace(@" ", "");

			//TODO: Remove this when remember me feature developed
			model.RememberMe = true;

            PlatformUser user;
            int appId;
            //SignInStatus result;
            var result = false;
            using (var platformDBContext = new PlatformDBContext(_configuration))
            using (var platformUserRepository = new PlatformUserRepository(platformDBContext, _configuration, _cacheHelper))
            {
                user = await platformUserRepository.Get(model.Email);
                appId = GetAppId(url);
                //result = SignInStatus.Failure;

				if (user != null)
				{
					//TODO Removed
					if (url.Contains("localhost") || url.Contains("mirror.ofisim.com") || url.Contains("staging.ofisim.com") /*|| user.AppId == appId*/) { }
					//result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
					else
					{

						var app = platformDBContext.UserTenants.FirstOrDefault(x => x.UserId == user.Id && x.Tenant.AppId == appId);

						if (app != null)
						{
							//result = await SignInManager.PasswordSignInAsync(app.Email, model.Password, model.RememberMe, shouldLockout: false);
						}
						//TODO Removed
						/*else if (user.AppId != appId)
						{
							if (user.AppId == 1)
								ViewData["appName"] = "CRM";
							else
								ViewData["appName"] = "İK";

							//result = SignInStatus.LockedOut;
						}*/

					}
				}
			}

			switch (result)
			{
				//case SignInStatus.Success:
				case true:
					return RedirectToLocal(returnUrl);
				//case SignInStatus.LockedOut:
				//     ViewBag.Error = "isNotValidApp";
				//     return View(model);
				//case SignInStatus.RequiresVerification:
				//    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
				//case SignInStatus.Failure:
				default:
					ModelState.AddModelError("", "Invalid login attempt.");
					ViewBag.Error = "wrongInfo";
					return View(model);
			}
		}

		[Route("register"), AllowAnonymous]
		public async Task<ActionResult> Register(string returnUrl, string language = null, string error = null, string name = null, string lastname = null, string email = null, bool officeSignIn = false, string appId = null)
		{
			RegisterBindingModel registerBindingModel = null;

			if (name != null || lastname != null || email != null)
			{
				registerBindingModel = new RegisterBindingModel()
				{
					Email = email,
					FirstName = HttpUtility.UrlDecode(name),
					LastName = HttpUtility.UrlDecode(lastname)
				};

				if (email != null)
					ViewBag.ReadOnly = true;
			}
			var lang = GetLanguage();
			if (language != null)
			{
				lang = language;
				SetLanguae(lang);
			}

			ViewBag.OfficeSignIn = officeSignIn;
			ViewBag.Lang = lang;
			ViewBag.error = error;
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);
			ViewBag.AppId = appId;

			if (!string.IsNullOrWhiteSpace(ViewBag.AppInfo["language"].Value))
			{
				SetLanguae(ViewBag.AppInfo["language"].Value);
				ViewBag.Lang = ViewBag.AppInfo["language"].Value;
			}

			return View(registerBindingModel);
		}

		[Route("register"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<ActionResult> Register(RegisterBindingModel registerBindingModel, string returnUrl, string campaignCode = null, bool officeSignIn = false, string appId = null)
		{
			var lang = GetLanguage();
			var phone = registerBindingModel.Phone;
			registerBindingModel.Phone = phone.Replace(@"(", "").Replace(@")", "").Replace(@"-", "").Replace(@" ", "");
			registerBindingModel.License = "7673E999-18FB-497F-A958-84DCA43031CC";
			registerBindingModel.Culture = lang == "tr" ? "tr-TR" : "en-US";
			registerBindingModel.Currency = lang == "tr" ? "TRY" : "USD";
			registerBindingModel.CampaignCode = campaignCode;
			registerBindingModel.Email = registerBindingModel.Email.Replace(@" ", "");
			registerBindingModel.OfficeSignIn = officeSignIn;
			registerBindingModel.AppID = appId != null ? Convert.ToInt32(appId) : GetAppId(Request.Host.ToString());
			if (officeSignIn)
				registerBindingModel.Password = Utils.GenerateRandomUnique(8);

			ViewBag.Lang = lang;
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);
			ViewBag.OfficeSignIn = officeSignIn;

			if (!string.IsNullOrWhiteSpace(ViewBag.AppInfo["language"].Value))
			{
				SetLanguae(ViewBag.AppInfo["language"].Value);
				ViewBag.Lang = ViewBag.AppInfo["language"].Value;
			}

			var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
			var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/register";

			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(apiUrl);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(
					new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

				var dataAsString = JsonConvert.SerializeObject(registerBindingModel);
				var content = new StringContent(dataAsString);
				var response = await client.PostAsync(apiUrl, content);
				//var response = await client.PostAsJsonAsync(apiUrl, registerBindingModel); 
				if (response.IsSuccessStatusCode)
				{
					if (!officeSignIn)
						return RedirectToAction("Verify", "Auth",
							new { ReturnUrl = ViewBag.ReturnUrl, Email = registerBindingModel.Email });

					var data = response.Content.ReadAsStringAsync().Result;
					JObject automaticAccountActivationModel = JObject.Parse(data);
					var token = (string)automaticAccountActivationModel["Token"];
					var guid = (Guid)automaticAccountActivationModel["GuId"];

                    return RedirectToAction("Activation", "Auth",
                        new { Token = token, Uid = guid, OfficeSignIn = true });
                }
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    using (var platformDBContext = new PlatformDBContext(_configuration))
                    using (var platformUserRepository = new PlatformUserRepository(platformDBContext, _configuration, _cacheHelper))
                    {
                        var user = await platformUserRepository.Get(registerBindingModel.Email);
                        if (user != null)
                        {
                            //TODO Removed
                            /*if (user.AppId == 1)
								ViewData["appName"] = "CRM";
							else*/
							ViewData["appName"] = "İK";

							ViewBag.Error = "UserExist";
						}
					}
				}
				else
				{
					throw new Exception("Unexcepted error");
				}

				registerBindingModel.Phone = phone;
			}
			return View(registerBindingModel);
		}

		[Route("ResetPassword"), AllowAnonymous]
		public async Task<ActionResult> ResetPassword(string returnUrl, string token, Guid uid, string error = null)
		{
			var lang = GetLanguage();
			ViewBag.Lang = lang;
			ViewBag.Error = error;
			ViewBag.Token = WebUtility.UrlEncode(token);
			ViewBag.Uid = uid;
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);
			return View();
		}

		[Route("ResetPassword"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(ResetPasswordBindingModel resetPasswordBindingModel, string token, int uid)
		{
			var lang = GetLanguage();
			resetPasswordBindingModel.UserId = uid;
			resetPasswordBindingModel.Token = WebUtility.UrlDecode(token);

			ViewBag.Lang = lang;
			ViewBag.ReturnUrl = "/";
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);

			var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
			var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/reset_password";

			using (HttpClient client = new HttpClient())
			{
				client.BaseAddress = new Uri(apiUrl);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

				var dataAsString = JsonConvert.SerializeObject(resetPasswordBindingModel);
				var contentResetPasswordBindingModel = new StringContent(dataAsString);
				var response = await client.PostAsync(apiUrl, contentResetPasswordBindingModel);

				//var response = await client.PostAsJsonAsync(apiUrl, resetPasswordBindingModel);
				var res = "";

				if (response.IsSuccessStatusCode)
				{
					return RedirectToAction("Login", "Auth", new { ReturnUrl = ViewBag.ReturnUrl, Success = "passwordChanged" });
				}

				using (var content = response.Content)
				{
					res = content.ReadAsStringAsync().Result;
				}

				var result = JObject.Parse(res);
				var modelState = result["ModelState"];

				ViewBag.Error = modelState[""] != null && modelState[""].HasValues ? "invalidToken" : "notFound";
			}
			return View(resetPasswordBindingModel);
		}

		[Route("ForgotPassword"), AllowAnonymous]
		public async Task<ActionResult> ForgotPassword(string email = null, string language = null, string error = null, string info = null)
		{
			var lang = GetLanguage();
			if (language != null)
			{
				lang = language;
				SetLanguae(lang);
			}
			ViewBag.Lang = lang;
			ViewBag.error = error;
			ViewBag.Info = info;
			ViewBag.ReturnUrl = "/";
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);

			if (!string.IsNullOrWhiteSpace(ViewBag.AppInfo["language"].Value))
			{
				SetLanguae(ViewBag.AppInfo["language"].Value);
				ViewBag.Lang = ViewBag.AppInfo["language"].Value;
			}

			return View();
		}

		[Route("ForgotPassword"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword(string email)
		{
			var lang = GetLanguage();
			var culture = lang == "tr" ? "tr-TR" : "en-US";
			ViewBag.Error = null;

			ViewBag.Lang = lang;
			ViewBag.ReturnUrl = "/";
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);

			var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
			var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/forgot_password?email=" + email.Replace(@" ", "") + "&culture=" + culture;


			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(apiUrl);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

				var response = await client.GetAsync(apiUrl);
				var res = "";

				if (response.IsSuccessStatusCode)
				{
					ViewBag.Info = "Success";
					return View();
				}

				using (var content = response.Content)
				{
					res = content.ReadAsStringAsync().Result;
				}

				var result = JObject.Parse(res);
				var modelState = result["ModelState"];

				if (modelState["not_found"] != null && modelState["not_found"].HasValues)
				{
					ViewBag.Error = "notFound";
				}
				else if (modelState["not_activated"] != null && modelState["not_activated"].HasValues)
				{
					ViewBag.Error = "notActivated";
				}
			}
			return View();
		}

		[HttpPost]
		[Route("send_activation")]
		public async Task<IActionResult> SendActivation([FromBody] SendActivationBindingModels sendActivationBindingModel)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var templates = await _platformRepository.GetAppTemplate(sendActivationBindingModel.AppId, AppTemplateType.Email, sendActivationBindingModel.Culture, "verification");

			foreach (var template in templates)
			{
				template.Content.Replace("{{:FirstName}}", sendActivationBindingModel.FirstName);
				template.Content.Replace("{{:LastName}}", sendActivationBindingModel.LastName);
				template.Content.Replace("{{:Email}}", sendActivationBindingModel.Email);
				template.Content.Replace("{{:URL}}", sendActivationBindingModel.Email);

				Email notification = new Email(template.Subject, template.Content, _configuration);

				var req = JsonConvert.DeserializeObject<JObject>(template.Settings);
				if (req != null)
				{
					notification.AddRecipient((string)req["MailSenderEmail"]);
					notification.AddToQueue((string)req["MailSenderEmail"], (string)req["MailSenderName"]);
				}

			}
			return Ok();
		}

		[Route("Activation"), HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
		public async Task<ActionResult> Activation(string token = "", string uid = null, bool officeSignIn = false)
		{
			var lang = GetLanguage();
			var culture = lang == "tr" ? "tr-TR" : "en-US";

			var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
			var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/activate?userId=" + uid + "&token=" + WebUtility.UrlEncode(token) + "&culture=" + culture + "&officeSignIn=" + officeSignIn;
			ViewBag.Lang = lang;
			ViewBag.ReturnUrl = "/";
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);

			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(apiUrl);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

				var response = await client.GetAsync(apiUrl);
				var res = "";

				if (response.IsSuccessStatusCode)
				{
					if (officeSignIn)
						return RedirectToAction("SignInAd", "Auth");

					return RedirectToAction("Login", "Auth", new { ReturnUrl = ViewBag.ReturnUrl, Success = "accountActivated" });
				}

				using (var content = response.Content)
				{
					res = content.ReadAsStringAsync().Result;
				}

				var result = JObject.Parse(res);
				var modelState = result["ModelState"];

				if (modelState == null || result["Message"] != null && result["Message"].HasValues)
				{
					ViewBag.Error = "anErrorOccured";
				}
				else if (modelState[""] != null && modelState[""].HasValues)
				{
					ViewBag.Error = "invalidToken";
				}
			}
			ViewBag.PostBack = "PostBack";
			return View();
		}

		[Route("Activation"), AllowAnonymous]
		public async Task<ActionResult> Activation(string token = "", string uid = null, string app = null, bool officeSignIn = false)
		{
			var lang = GetLanguage();
			ViewBag.Lang = lang;
			ViewBag.ReturnUrl = "/";
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);
			ViewBag.Token = token;
			ViewBag.Uid = uid;
			ViewBag.PostBack = "";
			ViewBag.OfficeSignIn = officeSignIn;
			return View();
		}

		[Route("Verify"), AllowAnonymous]
		public async Task<ActionResult> Verify(string returnUrl, string email, bool resend = false)
		{
			var lang = GetLanguage();
			ViewBag.Lang = lang;
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.Email = email.Replace(@" ", "");
			ViewBag.Resend = resend;
			ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, lang, _configuration);

			if (!resend) return View();

			var culture = lang == "tr" ? "tr-TR" : "en-US";
			var index = new Uri(Request.GetDisplayUrl()).OriginalString.IndexOf(new Uri(Request.GetDisplayUrl()).PathAndQuery);
			var apiUrl = new Uri(Request.GetDisplayUrl()).OriginalString.Remove(index) + "/api/account/resend_activation?email=" + email + "&culture=" + culture;
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(apiUrl);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

				var response = await client.GetAsync(apiUrl);
				var res = "";

				if (response.IsSuccessStatusCode)
				{
					return View();
				}

				using (var content = response.Content)
				{
					res = content.ReadAsStringAsync().Result;
				}

				var result = JObject.Parse(res);
				foreach (var keyValuePair in result)
				{
					if (keyValuePair.Value.Type == JTokenType.Object &&
						keyValuePair.Value[""][0].ToString() == "User has already activated")
						ViewBag.Error = "alreadyActivated";
					else if (keyValuePair.Value.Type == JTokenType.Object &&
							 keyValuePair.Value[""][0].ToString() == "User not found")
					{
						ViewBag.Error = "userNotFound";
					}
					else if (keyValuePair.Value.Type == JTokenType.Object &&
							 keyValuePair.Value[""][0].ToString() == "Email are required")
					{
						ViewBag.Error = "emailRequired";
					}
					else
					{
						ViewBag.Error = "error";
					}
				}
			}

			return View();
		}

		[Route("logout"), HttpPost, ValidateAntiForgeryToken]
		public ActionResult Logout()
		{
			//Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			return RedirectToAction("Login", "Auth");
		}

		/*public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }*/

		/*private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }*/

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction("Index", "Home");
		}

		[Route("Auth"), AllowAnonymous]
		public void SignInAd()
		{
			/*if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }*/
		}

		// sign out triggered from the Sign Out gesture in the UI
		// after sign out, it redirects to Post_Logout_Redirect_Uri (as set in Startup.Auth.cs)
		public async Task<ActionResult> SignOut()
		{
			await HttpContext.SignOutAsync("Cookies");
			await HttpContext.SignOutAsync("oidc");
			//Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			return RedirectToAction("Index", "Home");
		}

		/*public void EndSession()
        {
            // If AAD sends a single sign-out message to the app, end the user's session, but don't redirect to AAD for sign out.
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        }*/

		private void SetLanguae(string lang)
		{
			if (string.IsNullOrWhiteSpace(lang))
				lang = "tr";

			HttpContext.Response.Cookies.Append("_lang", lang, new CookieOptions { Expires = DateTime.Now.AddYears(20) });
			//Response.Cookies.Add(cookieVisitor);
			Response.Cookies.Append("_lang", lang, new CookieOptions { Expires = DateTime.Now.AddYears(20) });
		}

		private string GetLanguage()
		{
			var lang = Request.Cookies["_lang"];
			if (lang != null)
			{
				return lang;
			}

			SetLanguae("tr");
			return "tr";
		}

		public int GetAppId(string url)
		{
			if (url.Contains("kobi.ofisim.com") || url.Contains("kobi-test.ofisim.com"))
			{
				return 2;
			}
			if (url.Contains("asistan.ofisim.com") || url.Contains("asistan-test.ofisim.com"))
			{
				return 3;
			}
			if (url.Contains("ik.ofisim.com") || url.Equals("ik-test.ofisim.com") || url.Contains("ik-dev.ofisim.com"))
			{
				return 4;
			}
			if (url.Contains("cagri.ofisim.com") || url.Contains("cagri-test.ofisim.com"))
			{
				return 5;
			}
			if (url.Contains("crm.ofisim.com") || url.Contains("test.ofisim.com") || url.Contains("dev.ofisim.com"))
			{
				return 1;
			}
			if (url.Contains("crm.livasmart.com"))
			{
				return 6;
			}
			if (url.Contains("crm.appsila.com"))
			{
				return 7;
			}
			if (url.Contains("hr.ofisim.com") || url.Equals("hr-test.ofisim.com") || url.Contains("hr-dev.ofisim.com"))
			{
				return 8;
			}
			return 1;
		}
	}
}