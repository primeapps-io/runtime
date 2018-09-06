using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrimeApps.Model.Helpers.QueryTranslation
{
    /// <summary>
    /// Helps translating dynamic date functions into postgres format. 
    /// </summary>
    public static class PostgresDynamicDate
    {

        static Dictionary<DynamicDateFunction, Regex> dateFunctionRules = new Dictionary<DynamicDateFunction, Regex>()
        {
            {DynamicDateFunction.NOW  , new Regex(@"^now\(((-?\d)+([smhDYMW]))*\)") },
            {DynamicDateFunction.THIS_WEEK, new Regex(@"^this_week\(((-?\d+)+([smhDYMW]))*\)") },
            {DynamicDateFunction.THIS_MONTH , new Regex(@"^this_month\(((-?\d+)+([smhDYMW]))*\)") },
            {DynamicDateFunction.THIS_YEAR  , new Regex(@"^this_year\(((-?\d+)+([smhDYMW]))*\)") },
            {DynamicDateFunction.TODAY  , new Regex(@"^today\(((-?\d+)+([smhDYMW]))*\)") },
            {DynamicDateFunction.DAY  , new Regex(@"^day\(((-?\d+)+([smhDYMW]))*\)") },
            {DynamicDateFunction.MONTH  , new Regex(@"^month\(((-?\d+)+([smhDYMW]))*\)") },
            {DynamicDateFunction.YEAR  , new Regex(@"^year\(((-?\d+)+([smhDYMW]))*\)") }
        };

        static Dictionary<DynamicDateFunction, string> dateFunctions = new Dictionary<DynamicDateFunction, string>()
        {
            {DynamicDateFunction.NOW  , @"now() AT TIME ZONE '{0}' + interval '{1} {2}'" },
            {DynamicDateFunction.THIS_WEEK, @"date_trunc('week', now() AT TIME ZONE '{0}') + interval '{1} {2}'" },
            {DynamicDateFunction.THIS_MONTH , @"date_trunc('month', now() AT TIME ZONE '{0}') + interval '{1} {2}'" },
            {DynamicDateFunction.THIS_YEAR  , @"date_trunc('year', now() AT TIME ZONE '{0}') + interval '{1} {2}'" },
            {DynamicDateFunction.TODAY  , @"date_trunc('day', now() AT TIME ZONE '{0}') + interval '{1} {2}'" },
            {DynamicDateFunction.DAY  , @"EXTRACT(DAY FROM now() AT TIME ZONE '{0}')" },
            {DynamicDateFunction.MONTH  , @"EXTRACT(MONTH FROM now() AT TIME ZONE '{0}')" },
            {DynamicDateFunction.YEAR  , @"EXTRACT(YEAR FROM now() AT TIME ZONE '{0}')" }

        };

        static Dictionary<string, string> intervalTypes = new Dictionary<string, string>()
        {
            {"s","second" },
            {"m","minute" },
            {"h","hour" },
            {"D","day" },
            {"Y","year" },
            {"M","month" },
            {"W","week"}
        };


        public enum DynamicDateFunction
        {
            NONE = 0,
            NOW = 1,
            THIS_WEEK = 2,
            THIS_MONTH = 3,
            THIS_YEAR = 4,
            TODAY = 5,
            DAY=7,
            MONTH=8,
            YEAR=9
        }
        /// <summary>
        /// Checks string values if it contains a date function, defined in date function rules.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DynamicDateFunction Contains(string value)
        {
            DynamicDateFunction df = DynamicDateFunction.NONE;
            foreach (var funct in dateFunctionRules)
            {
                if (funct.Value.IsMatch(value))
                {
                    df = funct.Key;
                    break;
                }
            }
            return df;
        }
        /// <summary>
        /// Parses string values if they contain date function.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="timezoneOffset"></param>
        /// <returns></returns>
        public static string Parse(string value, int timezoneOffset = 180)
        {
            Match match = null;
            int interval = 0;
            string intervalType = null;
            string postgresFunction = value;

            foreach (var funct in dateFunctionRules)
            {
                match = funct.Value.Match(value);
                if (match.Success)
                {
                    //SQL için eklendi.
                    timezoneOffset = 0;

                    if (match.Groups.Count > 2)
                    {
                        int.TryParse(match.Groups[2].Value, out interval);
                        intervalType = intervalTypes.SingleOrDefault(x => x.Key == match.Groups[3].Value).Value;

                    }

                    postgresFunction = Translate(funct.Key, GetTimeZone(timezoneOffset), interval, intervalType);
                    break;
                }
            }


            return postgresFunction;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionType"></param>
        /// <param name="timeZoneString"></param>
        /// <param name="minutes"></param>
        /// <param name="intervalType"></param>
        /// <returns></returns>
        public static string Translate(DynamicDateFunction functionType, string timeZoneString, int minutes = 0, string intervalType = "s")
        {
            return string.Format(dateFunctions[functionType], timeZoneString, minutes, intervalType);
        }
        /// <summary>
        /// Gets Postgresql compatible time zone format from minutes.
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static string GetTimeZone(int minutes)
        {
            TimeSpan tz = TimeSpan.FromMinutes(minutes);
            return tz.ToString(@"hh\:mm\:ss");
        }


    }
}
