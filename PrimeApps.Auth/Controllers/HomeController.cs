// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.UI
{
    [SecurityHeaders]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
		private IApplicationRepository _applicationRepository;

        public HomeController(IIdentityServerInteractionService interaction, IApplicationRepository applicationRepository)
        {
			_applicationRepository = applicationRepository;
            _interaction = interaction;
        }

        public IActionResult Index(string error, string success)
        {
            /*var appInfo = await _applicationRepository.GetWithAuth(Request.Host.Value);

            if(appInfo != null)
                return Redirect(Request.Scheme + "://" + appInfo.Setting.Domain);*/
            ViewBag.Error = error;
            ViewBag.Success = success;
            ViewBag.UserName = User.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
            ViewBag.Email = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
            return View();
        }

        /// <summary>
        /// Shows the error page
        /// </summary>
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;
            }

            return View("Error", vm);
        }
    }
}