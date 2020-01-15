using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static System.String;

namespace PrimeApps.App.Helpers
{
    /// <summary>
    /// This class contains miscellaneous extension methods.
    /// </summary>
    public static class Extensions
    {
        private const string NullIpAddress = "::1";

        /// <summary>
        /// This is an extension method for byte search.
        /// </summary>
        /// <param name="searchWithin"></param>
        /// <param name="serachFor"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int IndexOf(byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        index++;
                        if (index == serachFor.Length)
                        {
                            return startPos;
                        }
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1)
                        {
                            return -1;
                        }

                        index = 0;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// This is an extension method for streams, it converts streams to byte array.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// This is an helper method to generate slug
        /// </summary>
        /// <returns></returns>
        public static string ToSlug(string value, string seperator = null)
        {
            //First to lower case 
            value = value.ToLowerInvariant();

            //Remove all accents
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);

            value = Encoding.ASCII.GetString(bytes);

            //Replace spaces 
            if (IsNullOrEmpty(seperator))
                seperator = "_";

            value = Regex.Replace(value, @"\s", seperator, RegexOptions.Compiled);

            //Remove invalid chars 
            value = Regex.Replace(value, @"[^\w\s\p{Pd}]", "", RegexOptions.Compiled);

            //Trim dashes from end 
            value = value.Trim('-', '_');

            //Replace double occurences of - or \_ 
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return value.ToLower(CultureInfo.CreateSpecificCulture("en-US"));
        }

        public static string ToCamelCase(this string source)
        {
            var parts = source.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            return parts.First().ToLower() + Join("", parts.Skip(1).Select(ToCapital));
        }

        public static string ToCapital(this string source)
        {
            return $"{char.ToUpper(source[0])}{source.Substring(1).ToLower()}";
        }

        public static byte[] ReadToEnd(this System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }

                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        /// <summary>
        /// This is an extension method to truncate string with ellipsis
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="maxLength">maxLength</param>
        /// <param name="ellipsis">ellipsis</param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxLength, string ellipsis = "...")
        {
            return value?.Length <= maxLength ? value : value?.Substring(0, maxLength - ellipsis.Length) + ellipsis;
        }

        public static IEnumerable<byte[]> Split(this byte[] value, int bufferLength)
        {
            var countOfArray = value.Length / bufferLength;

            if (value.Length % bufferLength > 0)
                countOfArray++;

            for (var i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();
            }
        }

        /// <summary>
        /// Creates a SHA256 hash of the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A hash</returns>
        public static string ToSha256(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);

                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Creates a SHA256 hash of the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A hash.</returns>
        public static byte[] ToSha256(this byte[] input)
        {
            if (input == null)
            {
                return null;
            }

            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(input);
            }
        }

        /// <summary>
        /// Creates a SHA512 hash of the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A hash</returns>
        public static string ToSha512(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            using (var sha = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);

                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Retrieve the raw body as a string from the Request.Body stream
        /// </summary>
        /// <param name="request">Request instance to apply to</param>
        /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
        /// <returns></returns>
        public static async Task<Stream> ReadAsStreamAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (StreamReader reader = new StreamReader(request.Body, encoding))
            {
                var result = await reader.ReadToEndAsync();
                // convert string to stream
                byte[] byteArray = Encoding.UTF8.GetBytes(result);
                //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
                return new MemoryStream(byteArray);
            }
        }

        public static bool IsLocal(this HttpRequest req)
        {
            var connection = req.HttpContext.Connection;

            if (connection.RemoteIpAddress.IsSet())
            {
                //We have a remote address set up
                return connection.LocalIpAddress.IsSet()
                    //Is local is same as remote, then we are local
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    //else we are remote if the remote IP address is not a loopback address
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;
        }

        private static bool IsSet(this IPAddress address)
        {
            return address != null && address.ToString() != NullIpAddress;
        }
    }
}