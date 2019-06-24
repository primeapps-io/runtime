using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace PrimeApps.CLI
{
    [Command(Description = "Logs in to PrimeApps Studio account."), HelpOption]
    public class Login
    {
        private static readonly HttpClient client = new HttpClient();
        [Option(Description = "Remain logged in.")]
        public bool RemainSignedIn { get; }

        private async Task<int> OnExecute(IConsole console)
        {
            string username = Prompt.GetString("Your PrimeApps Email Address?",
            promptColor: ConsoleColor.Blue);

            string password = Prompt.GetPassword("Your Password?",
                promptColor: ConsoleColor.Red);

            client.DefaultRequestHeaders.Accept.Clear();
            // granttype : password
            // username : your@localuser.com
            // password : 123456
            // clientid : primeapps_studio
            // clientsecret : secret

            IList<KeyValuePair<string, string>> nameValueCollection = new List<KeyValuePair<string, string>> {
    { new KeyValuePair<string, string>("granttype", "password") },
    { new KeyValuePair<string, string>("username", username) },
    { new KeyValuePair<string, string>("password", password) },
    { new KeyValuePair<string, string>("clientid", "primeapps_studio") },
    { new KeyValuePair<string, string>("clientsecret", "secret")
    }
};
            console.WriteLine("Logging in...");

            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/user/token", new FormUrlEncodedContent(nameValueCollection));
            string token = await response.Content.ReadAsStringAsync();

            JObject parsed = null;
            try
            {
                parsed = JObject.Parse(token);

            }
            catch (JsonReaderException ex)
            {
                console.WriteLine("An Error Occured: {0}", ex.Message);
                return 1;
            }

            JToken access_token;
            if (!parsed.TryGetValue("access_token", out access_token))
            {
                console.WriteLine("Not Authorized, please try again!");
                return 1;
            }

            if (String.IsNullOrEmpty(access_token.Value<string>()))
            {
                console.WriteLine("Not Authorized, please try again!");
                return 1;
            }

            string config_folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);

            if (!Directory.Exists(Path.Combine(config_folder, "primeapps")))
            {
                Directory.CreateDirectory(Path.Combine(config_folder, "primeapps"));
            }

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(config_folder, "primeapps", "token"), false))
            {
                outputFile.WriteLine(access_token.Value<string>());
            }
            console.ForegroundColor = ConsoleColor.Green;
            console.WriteLine("Successfully logged in as {0}!", username);
            console.ResetColor();
            return 0;

        }
    }
}