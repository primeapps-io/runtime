using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public static class Utils
    {
        /// <summary>
        /// If string has numeric only
        /// </summary>
        public static bool IsNumeric(string checkString)
        {
            if (string.IsNullOrEmpty(checkString))
            {
                return false;
            }
            var ca = checkString.ToCharArray();
            for (var i = 0; i < ca.Length; i++)
            {
                if (!char.IsNumber(ca[i]))
                    return false;
            }
            return true;
        }

        public static double BytesToKilobytes(this int bytes)
        {
            return bytes / 1024d;
        }

        public static double BytesToKilobytes(this long bytes)
        {
            return bytes / 1024d;
        }

        public static double BytesToMegabytes(this long bytes)
        {
            return bytes / 1024d / 1024d;
        }

        public static double KilobytesToBytes(this double kilobytes)
        {
            return kilobytes * 1024d;
        }

        /// <summary>
        ///    Checks if string is valid email 
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string emailAddress)
        {
            var patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
                                + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                                + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                                + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                                + @"[a-zA-Z]{2,}))$";
            var reStrict = new Regex(patternStrict);
            var isStrictMatch = reStrict.IsMatch(emailAddress);
            return isStrictMatch;
        }

        /// <summary>
        ///     Created and MD5 from string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Md5Hash(string str)
        {
            var md5Hash = MD5.Create();
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        ///   Checks the list for the requested string in
        /// </summary>
        /// <param name="stringTolook"></param>
        /// <param name="listToLookIn"></param>
        /// <returns></returns>
        public static bool CheckTheListString(string stringTolook, List<string> listToLookIn)
        {
            var exists = listToLookIn.Any(c => c == stringTolook);
            if (exists)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///    Convert BYTE Size
        /// </summary>
        /// <param name="bytesize"></param>
        /// <returns></returns>
        public static string ConvertByteSize(double bytesize)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            var len = bytesize;
            var order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }
            var result = string.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

        /// <summary>
        ///     Creates Random String with given length (default = 10)
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string CreateRandomString(int length = 10)
        {
            var arrPossibleChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
            var intPasswordLength = length;
            string stringPassword = null;
            var rand = new Random();
            int i;
            for (i = 0; i <= intPasswordLength; i++)
            {
                var intRandom = rand.Next(arrPossibleChars.Length);
                stringPassword = stringPassword + arrPossibleChars[intRandom];
            }
            return stringPassword;
        }

        public static string CreateGuidString()
        {
            var getKey = Guid.NewGuid().ToString().Substring(0, 25).ToUpper();
            return Convert.ToString(Regex.Replace(getKey, ".{4}", "$0-"));
        }

        public static string GetStringBetween(string str, string firstString, string lastString)
        {
            var pos1 = str.IndexOf(firstString, StringComparison.Ordinal) + firstString.Length;
            var pos2 = str.IndexOf(lastString, StringComparison.Ordinal);
            var finalString = str.Substring(pos1, pos2 - pos1);
            return finalString;
        }

        /// <summary>
        ///     Convert comma string to array
        /// </summary>
        /// <param name="convertString"></param>
        /// <returns></returns>
        public static string[] ConvertCommaStringToArray(string convertString)
        {
            return convertString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        ///    Convert comma int to list int
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<int> ConvertStringToIntList(string str)
        {
            if (string.IsNullOrEmpty(str))
                yield break;
            foreach (var s in str.Split(','))
            {
                int num;
                if (int.TryParse(s, out num))
                    yield return num;
            }
        }

        public static async Task<bool> IsComplexPassword(string password)
        {
            var passwordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            var passwordValidateResult = await passwordValidator.ValidateAsync(password);

            return passwordValidateResult.Succeeded;
        }

        public static string GenerateRandomUnique(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            var unique = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return unique;
        }
    }
}