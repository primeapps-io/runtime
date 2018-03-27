using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PrimeApps.App.Helpers
{
    public static class UnixDate
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// This is a helper method for convert javascript timestamp to DateTime
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static DateTime GetDate(long ticks)
        {
            return UnixEpoch.AddMilliseconds(ticks);
        }

        /// <summary>
        /// This is a helper method to get javascript time (DateTime.UtcNow)
        /// </summary>
        /// <returns></returns>
        public static long GetTime()
        {
            var timestamp = DateTime.Now.ToUniversalTime() - UnixEpoch;
            var time = (long)timestamp.TotalMilliseconds;

            return time;
        }

        /// <summary>
        /// This is a helper method to get javascript time
        /// </summary>
        /// <returns></returns>
        public static long GetTime(DateTime date)
        {
            var timestamp = date.ToUniversalTime() - UnixEpoch;
            var time = (long)timestamp.TotalMilliseconds;

            return time;
        }

        /// <summary>
        /// This extention method returns financial quarter of given date
        /// </summary>
        /// <returns></returns>
        public static int ToQuarter(this DateTime date)
        {
            return (date.Month + 2) / 3;
        }

        /// <summary>
        /// This method returns a random javascript time
        /// </summary>
        /// <returns></returns>
        public static long RandomTime(int dayCountAfter, long? startTime = null)
        {
            var random = new Random();
            var startDate = DateTime.UtcNow;

            if (startTime.HasValue)
                startDate = GetDate(startTime.Value);

            var randomDate = startDate.AddDays(random.Next(dayCountAfter));

            return GetTime(randomDate);
        }
    }
}