using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Helpers
{
    public static class ExternalAuthHelper
    {
        public static async Task<SignInResult> Login(JObject externalLogin, JToken action, JObject data)
        {
            using (var client = new HttpClient())
            {
                var request = GenerateClient(client, externalLogin, action, data);
                var response = await client.SendAsync(request);

                return response.IsSuccessStatusCode ? SignInResult.Success : SignInResult.Failed;
            }
        }

        public static async Task<HttpResponseMessage> ChangePassword(JObject externalLogin, JToken action, JObject data)
        {
            using (var client = new HttpClient())
            {
                var request = GenerateClient(client, externalLogin, action, data);
                var response = await client.SendAsync(request);

                return response.IsSuccessStatusCode ?
                    new HttpResponseMessage(HttpStatusCode.OK) :
                    new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> VerifyUser(JObject externalLogin, JToken action, JObject data)
        {
            using (var client = new HttpClient())
            {
                var request = GenerateClient(client, externalLogin, action, data);
                var response = await client.SendAsync(request);

                return response.IsSuccessStatusCode ?
                    new HttpResponseMessage(HttpStatusCode.OK) :
                    new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> CheckUser(JObject externalLogin, JToken action, JObject data)
        {
            using (var client = new HttpClient())
            {
                var request = GenerateClient(client, externalLogin, action, data);
                var response = await client.SendAsync(request);

                return response.IsSuccessStatusCode ?
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response.Content.ReadAsStringAsync().Result) } :
                    new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> Register(JObject externalLogin, JToken action, JObject data)
        {
            using (var client = new HttpClient())
            {
                var request = GenerateClient(client, externalLogin, action, data);
                var response = await client.SendAsync(request);

                return response.IsSuccessStatusCode ?
                    new HttpResponseMessage(HttpStatusCode.OK) :
                    new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> ForgotPassword(JObject externalLogin, JToken action, JObject data)
        {
            using (var client = new HttpClient())
            {
                var request = GenerateClient(client, externalLogin, action, data);
                var response = await client.SendAsync(request);

                var content = await response.Content.ReadAsAsync<JObject>();

                return response.IsSuccessStatusCode ?
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content["Result"].ToString()) } :
                    new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static async Task<HttpResponseMessage> ResetPassword(JObject externalLogin, JToken action, JObject data)
        {
            using (var client = new HttpClient())
            {
                var request = GenerateClient(client, externalLogin, action, data);
                var response = await client.SendAsync(request);

                return response.IsSuccessStatusCode ? 
                    new HttpResponseMessage(HttpStatusCode.OK) :
                    new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public static HttpRequestMessage GenerateClient(HttpClient client, JObject externalLogin, JToken action, JObject data)
        {
            var url = action["url"].ToString().Contains("http") ? action["url"].ToString() : externalLogin["base_url"].ToString() + action["url"].ToString();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            if (!externalLogin["default_headers"].IsNullOrEmpty())
            {
                foreach (var header in externalLogin["default_headers"])
                {
                    client.DefaultRequestHeaders.Add(((JProperty)header).Name.ToString(), ((JProperty)header).Value.ToString());
                }
            }

            if (!action["headers"].IsNullOrEmpty())
            {
                foreach (var header in action["headers"])
                {
                    client.DefaultRequestHeaders.Add(((JProperty)header).Name.ToString(), ((JProperty)header).Value.ToString());
                }
            }

            if (!action["query_string"].IsNullOrEmpty())
            {
                object[] parameters;

                var queryDatas = action["query_string"].ToString().Split(",");

                parameters = new object[queryDatas.Length];
                var i = 0;
                foreach (var queryData in queryDatas)
                {
                    if (queryData.Contains("eq::"))
                    {
                        parameters[i] = data[queryData.Split("eq::")[1]];
                    }
                    else
                    {
                        parameters[i] = queryData.ToString();
                    }

                    i++;
                }

                url = String.Format(url, parameters);
            }

            JObject body = new JObject();

            if (action["method"].ToString() != "get" && !action["body"].IsNullOrEmpty())
            {
                var i = 0;
                foreach (var bodyParameter in action["body"])
                {
                    if (((JProperty)bodyParameter).Value.ToString().Contains("eq::"))
                    {
                        body[((JProperty)bodyParameter).Name.ToString()] = data[((JProperty)bodyParameter).Value.ToString().Split("eq::")[1]]; //Burası 0 olabilir 1 değil.
                    }
                    else
                    {
                        body[((JProperty)bodyParameter).Name.ToString()] = ((JProperty)bodyParameter).Value.ToString();
                    }

                    if (!action["data_options"].IsNullOrEmpty() && !action["data_options"][((JProperty)bodyParameter).Name.ToString()].IsNullOrEmpty())
                    {
                        body[((JProperty)bodyParameter).Name.ToString()] = action["data_options"][((JProperty)bodyParameter).Name.ToString()][body[((JProperty)bodyParameter).Name.ToString()].ToString()];
                    }

                    i++;
                }
            }

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = new HttpMethod(action["method"].ToString().ToUpper()),
                RequestUri = new Uri(url)
            };

            if (body.HasValues)
                request.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");

            return request;
        }
    }
}
