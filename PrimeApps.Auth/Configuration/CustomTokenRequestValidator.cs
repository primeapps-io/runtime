using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PrimeApps.Auth.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.Auth
{
    public class CustomTokenRequestValidator : ICustomTokenRequestValidator
    {
        private readonly HttpContext _httpContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomTokenRequestValidator(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _httpContext = contextAccessor.HttpContext;
            _userManager = userManager;
        }
        public async Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            var sub = _httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var client = context.Result.ValidatedRequest.Client;

            var bodyStr = "";
            var req = _httpContext.Request;

            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                bodyStr = reader.ReadToEnd();
            }

            var emailObj = Array.Find(bodyStr.Split("&"), element => element.StartsWith("username", StringComparison.Ordinal));
            var passwordObj = Array.Find(bodyStr.Split("&"), element => element.StartsWith("password", StringComparison.Ordinal));

            if (emailObj == null || passwordObj == null)
            {
                context.Result.IsError = true;
                context.Result.Error = "User informations are not valid!";
                return;
            }

            var email = HttpUtility.UrlDecode(emailObj.Split("username=")[1]).ToString();
            var password = HttpUtility.UrlDecode(passwordObj.Split("password=")[1]).ToString();

            var user = await _userManager.FindByEmailAsync(email);
            var validUser = await _userManager.CheckPasswordAsync(user, password);

            if (!validUser)
            {
                context.Result.IsError = true;
                context.Result.Error = "User informations are not valid!";
                return;
            }

            context.Result.ValidatedRequest.ClientClaims.Add(new System.Security.Claims.Claim("email", user.Email));
            context.Result.ValidatedRequest.ClientClaims.Add(new System.Security.Claims.Claim("email_confirmed", user.EmailConfirmed.ToString()));
            context.Result.ValidatedRequest.ClientClaims.Add(new System.Security.Claims.Claim("external_login", "false"));


            // don't want it to be prefixed with "client_" ? we change it here (or from global settings)
            context.Result.ValidatedRequest.Client.ClientClaimsPrefix = "";

        }
    }
}
