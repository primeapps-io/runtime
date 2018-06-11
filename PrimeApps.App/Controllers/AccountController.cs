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
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Controllers
{
	[Route("api/account")]
	public class AccountController : Controller
	{
		private const string LocalLoginProvider = "Local";
		private IRecordRepository _recordRepository;
		private IPlatformRepository _platformRepository;
		private IPlatformUserRepository _platformUserRepository;
		private ITenantRepository _tenantRepository;
		private IProfileRepository _profileRepository;
		private IUserRepository _userRepository;
		private IRoleRepository _roleRepository;
		private Warehouse _warehouse;

		/*public AccountController(IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, Warehouse warehouse) : this(recordRepository, platformUserRepository, tenantRepository, warehouse)
        {
        }*/

		public AccountController(IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, IPlatformRepository platformRepository, IRoleRepository roleRepository, IProfileRepository profileRepository, IUserRepository userRepository, ITenantRepository tenantRepository, Warehouse warehouse)
		{
			_recordRepository = recordRepository;
			_warehouse = warehouse;
			_platformUserRepository = platformUserRepository;
			_tenantRepository = tenantRepository;
			_platformRepository = platformRepository;
			_profileRepository = profileRepository;
			_roleRepository = roleRepository;
			_userRepository = userRepository;

			//Set warehouse database name Ofisim to integration
			//_warehouse.DatabaseName = "Ofisim";
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			//SetContext(context);
			//SetCurrentUser(_recordRepository);

			base.OnActionExecuting(context);
		}

		// GET account/activate?userId=&token=&culture=

		
		[HttpGet]
		[AllowAnonymous]
		[Route("activate")]
		public async Task<IActionResult> Activate(
			[FromQuery(Name = "email")] string email, 
			[FromQuery(Name = "appId")] int appId = 0, 
			[FromQuery(Name = "culture")] string culture = "", 
			[FromQuery(Name = "firstName")] string firstName = "",
			[FromQuery(Name = "lastName")] string lastName = "",
			[FromQuery(Name = "token")] string token = null)
		{
			if (string.IsNullOrWhiteSpace(email) || appId == 0)
			{
				ModelState.AddModelError("", "appId and email are required");
				return BadRequest(ModelState);
			}

			var userExist = true;
			PlatformUser user = await _platformUserRepository.GetWithTenants(email);
			var app = _platformRepository.GetAppInfo(appId);

			if (user != null)
			{
				var appTenant = user.TenantsAsUser.FirstOrDefault(x => x.Tenant.AppId == appId);

				if (appTenant != null)
				{
					ModelState.AddModelError("", "User is already registered for this app.");
					return BadRequest(ModelState);
				}
			}
			else
			{
				userExist = false;
				user = new PlatformUser
				{
					Email = email,
					FirstName = firstName,
					LastName = lastName,
					Setting = new PlatformUserSetting()
				};

				if (!string.IsNullOrEmpty(culture))
				{
					user.Setting.Culture = culture;
					user.Setting.Language = culture.Substring(0, 2);
					//tenant.Setting.TimeZone = 
					user.Setting.Currency = culture;
				}
				else
				{
					user.Setting.Culture = app.Setting.Culture;
					user.Setting.Currency = app.Setting.Currency;
					user.Setting.Language = app.Setting.Language;
					user.Setting.TimeZone = app.Setting.TimeZone;

				}

				var result = _platformUserRepository.CreateUser(user).Result;

				if (result == 0)
				{
					ModelState.AddModelError("", "user not created");
					return BadRequest(ModelState);
				}

				user = await _platformUserRepository.GetWithTenants(email);
			}

			//var tenantId = 0;
			Tenant tenant = null;
			var tenantId = 2032;
			try
			{

				tenant = new Tenant
				{
					Id = tenantId,
					AppId = appId,
					Owner = user,
					UseUserSettings = true,
					GuidId = Guid.NewGuid(),
					License = new TenantLicense {
						UserLicenseCount = 5,
						ModuleLicenseCount = 2
					},
					Setting = new TenantSetting {
						Culture = app.Setting.Culture,
						Currency = app.Setting.Currency,
						Language = app.Setting.Language,
						TimeZone = app.Setting.TimeZone
					},
					CreatedBy = user
				};

				await _tenantRepository.CreateAsync(tenant);
				tenantId = tenant.Id;

				user.TenantsAsOwner.Add(tenant);
				await _platformUserRepository.UpdateAsync(user);

				var tenantUser = new TenantUser
				{
					Id = user.Id,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					FullName = $"{user.FirstName} {user.LastName}",
					IsActive = true,
					IsSubscriber = false,
					Culture = user.Setting.Culture,
					Currency = app.Setting.Currency,
					CreatedAt = user.CreatedAt,
					CreatedByEmail = user.Email
				};

				await Postgres.CreateDatabaseWithTemplate(tenantId, appId);

				_userRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = user.Id };
				_profileRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = user.Id };
				_roleRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = user.Id };
				_recordRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = user.Id };

				_profileRepository.TenantId = _roleRepository.TenantId = _userRepository.TenantId = _recordRepository.TenantId = tenantId;

				tenantUser.IsSubscriber = true;
				await _userRepository.CreateAsync(tenantUser);

				var userProfile = await _profileRepository.GetDefaultAdministratorProfileAsync();
				var userRole = await _roleRepository.GetByIdAsync(1);

				tenantUser.Profile = userProfile;
				tenantUser.Role = userRole;


				await _userRepository.UpdateAsync(tenantUser);
				await _recordRepository.UpdateSystemData(user.Id, DateTime.UtcNow, tenant.Setting.Language, appId);


				user.TenantsAsUser.Add(new UserTenant { Tenant = tenant, PlatformUser = user });

				//HostingEnvironment.QueueBackgroundWorkItem(clt => DocumentHelper.UploadSampleDocuments(user.Tenant.GuidId, user.AppId, tenant.Language));
				BackgroundJob.Enqueue(() => DocumentHelper.UploadSampleDocuments(tenant.GuidId, appId, tenant.Setting.Language));

				//user.TenantId = user.Id;
				//tenant.License.HasAnalyticsLicense = true;
				await _platformUserRepository.UpdateAsync(user);
				await _tenantRepository.UpdateAsync(tenant);

				await _recordRepository.UpdateSampleData(user);
				//await Cache.ApplicationUser.Add(user.Email, user.Id);
				//await Cache.User.Get(user.Id);

				if (token != null && !userExist)
				{
					var appTemplates = _platformRepository.GetAppTemplate(appId, AppTemplateType.Email, "email_confirm", culture.Substring(0, 2));
					var template = appTemplates.Templates.FirstOrDefault();

					template.Content.Replace("{{:FIRSTNAME}}", firstName);
					template.Content.Replace("{{:LASTNAME}}", lastName);
					template.Content.Replace("{{:EMAIL}}", email);
					template.Content.Replace("{:Url}", Request.IsLocal() ? "" : "https://" +  app.Setting.AuthDomain + "/user/activate?token=" + token);

					Email notification = new Email(template.Subject, template.Content);

					notification.AddRecipient(template.MailSenderEmail);
					notification.AddToQueue(template.MailSenderEmail, template.MailSenderName);

				}

				//HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.UpdateSubscriber(user.Email, user.TenantId.Value, _warehouse));
				BackgroundJob.Enqueue(() => Integration.UpdateSubscriber(user.Email, tenantId, _warehouse));

			}
			catch (Exception ex)
			{
				Postgres.DropDatabase(tenantId, true);

				await DeactivateUser(tenant);

				throw ex;
			}

			return Ok();
		}
		//return GetErrorResult(confirmResponse);
		//return BadRequestResult();

		private async Task DeactivateUser(Tenant tenant)
		{
			await _tenantRepository.DeleteAsync(tenant);
		}
	}

	// POST account/logout
	/*[Route("logout")]
	public IActionResult Logout()
	{
		//Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
		return Ok();
	}*/
}


