using System;
using System.IO;

namespace PrimeApps.CLI.Utils
{
    static class Auth
    {
        
        public static string GetStudioToken()
        {
            string config_folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);

            if (!Directory.Exists(Path.Combine(config_folder, "primeapps")))
            {
                Console.WriteLine($"You need to login to your studio account by using 'primeapps login' command.");
                return null;
            }

            string token;
            using (StreamReader outputFile = new StreamReader(Path.Combine(config_folder, "primeapps", "token")))
            {
                token = outputFile.ReadToEnd();
            }
            
            return token;
        }

    }

}