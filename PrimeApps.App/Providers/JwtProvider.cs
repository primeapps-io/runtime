using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Platform.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace PrimeApps.App.Providers
{
    public class JwtProvider
    {
        private static string _tokenUri;

        public JwtProvider() { }

        public static JwtProvider Create(string tokenUri)
        {
            _tokenUri = tokenUri;
            return new JwtProvider();
        }

        public async Task<string> GetTokenAsync(string username, string password, string clientId, string deviceId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_tokenUri);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("username", username),
                        new KeyValuePair<string, string>("password", password),
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("device_id", deviceId),
                        new KeyValuePair<string, string>("client_id", clientId)
                });

                var response = await client.PostAsync(string.Empty, content);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return null;
            }
        }

        public JObject DecodePayload(string token)
        {
            var parts = token.Split('.');
            var payload = parts[1];

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            return JObject.Parse(payloadJson);
        }

        public ClaimsIdentity CreateIdentity(bool isAuthenticated, string userName, string clientId, PlatformUser user, JObject payload)
        {
            //decode the payload from token
            //in order to create a claim            
            string userId = (string)payload["nameid"];

            var jwtIdentity = new ClaimsIdentity(new JwtIdentity(isAuthenticated, userName, DefaultAuthenticationTypes.ApplicationCookie));

            //add user id
            jwtIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
            jwtIdentity.AddClaim(new Claim("client_id", clientId));
            jwtIdentity.AddClaim(new Claim("user_id", user.Id.ToString()));
            jwtIdentity.AddClaim(new Claim("tenant_id", user.TenantId.ToString()));

            return jwtIdentity;
        }

        private byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
    }

    public class JwtIdentity : IIdentity
    {
        private bool _isAuthenticated;
        private string _name;
        private string _authenticationType;

        public JwtIdentity() { }

        public JwtIdentity(bool isAuthenticated, string name, string authenticationType)
        {
            _isAuthenticated = isAuthenticated;
            _name = name;
            _authenticationType = authenticationType;
        }
        public string AuthenticationType
        {
            get
            {
                return _authenticationType;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _isAuthenticated;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}