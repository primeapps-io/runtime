using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Auth.Helpers
{
	public interface IGiteaHelper
	{
		Task<string> GetToken(string email, string password);
	}

	public class GiteaHelper : IGiteaHelper
	{

		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IHttpContextAccessor _context;
		private IConfiguration _configuration;

		public GiteaHelper(IHttpContextAccessor context,
			IConfiguration configuration,
			IServiceScopeFactory serviceScopeFactory)
		{
			_context = context;
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public async Task<string> GetToken(string email, string password)
		{
			var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

			if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(enableGiteaIntegration))
			{
				try
				{
					using (var httpClient = new HttpClient())
					{
						var request = new JObject
						{
							["name"] = "primeapps"
						};

						SetHeaders(httpClient, email, password);
						var userName = GetUserName(email);
						var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
						if (!string.IsNullOrEmpty(giteaUrl))
						{
							var response = await httpClient.PostAsync(giteaUrl + "/api/v1/users/" + userName + "/tokens", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
							var resp = await response.Content.ReadAsStringAsync();
							var giteaResponse = JObject.Parse(resp);
							return giteaResponse["sha1"].ToString();
						}
						else
							return string.Empty;
					}
				}
				catch (Exception ex)
				{
					ErrorHandler.LogError(ex, "GiteaHelper Get Gitea Token. User:" + email);
					return string.Empty;
				}
			}
			else
				return string.Empty;
		}

		private void SetHeaders(HttpClient client, string email, string password)
		{
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetBasicAuthToken(email, password));
		}

		private string GetBasicAuthToken(string email, string password)
		{
			byte[] bytes = Encoding.GetEncoding(28591).GetBytes(email + ":" + password);
			return Convert.ToBase64String(bytes);
		}

		private string GetUserName(string email)
		{
			var query = email.Replace("@", string.Empty).Split(".");
			Array.Resize(ref query, query.Length - 1);
			return string.Join("", query);
		}
	}
}
