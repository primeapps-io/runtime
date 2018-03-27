using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrimeApps.App.Helpers
{
    /// <summary>
    /// This utility class provides cryptography methods for entity layer.
    /// </summary>
    public static class Crypto
    {
        /// <summary>
        ///  Returns a string of N random chars.
        /// </summary>
        /// <param name="length">Length of random chars</param>
        /// <returns></returns>
        public static string GenerateRandomCode(int length)
        {
            //create a new random variable.
            Random random = new Random();

            //create string variable
            string s = "";

            //create a list of allowed chars.
            string allowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            //get length of allowed chars
            int len = allowedChars.Length;

            //get chars from array randomly and concatanate it.
            for (int i = 0; i < length; i++)
            {
                s = String.Concat(s, allowedChars.Substring(random.Next(len), 1));
            }

            return s;
        }

        //returns MD5 hash of a string

        /// <summary>
        /// Converts string to MD5
        /// </summary>
        /// <param name="input">String to make md5</param>
        /// <returns></returns>
        public static string MD5Hash(string input)
        {
            //Get md5 service provider.
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();

            //Get bytes of input.
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);

            //compute hash of bytes
            bs = x.ComputeHash(bs);

            //create a new instance of string builder
            System.Text.StringBuilder s = new System.Text.StringBuilder();

            foreach (byte b in bs)
            {
                //append computed hashes into string builder object.
                s.Append(b.ToString("x2").ToLower());
            }

            //return it.
            return s.ToString();
        }
    }
}