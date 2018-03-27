using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace PrimeApps.App.Helpers
{
    public class Security
    {


        /// <summary>
        /// Hashes the string with hmac md5 algorithm.
        /// </summary>
        /// <param name="hashString">The hash string.</param>
        /// <returns></returns>
        public static string HashStringWithHMAC(string hashString)
        {
            byte[] binaryHash = new HMACMD5(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings.Get("PrimeApps.WebClient.PayU.SignatureKey")))
                .ComputeHash(Encoding.UTF8.GetBytes(hashString.ToString()));

            var hash = BitConverter.ToString(binaryHash)
            .Replace("-", string.Empty)
                .ToLowerInvariant();

            return hash;
        }
    }
}