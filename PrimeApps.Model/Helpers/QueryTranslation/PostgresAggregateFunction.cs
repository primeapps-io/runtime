using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrimeApps.Model.Helpers.QueryTranslation
{
    public static class PostgresAggregateFunction
    {
        public enum PostgresAggregateEnum
        {
            NONE = 0,
            AVG = 1,
            MIN = 2,
            MAX = 3,
            SUM = 4,
            COUNT = 5
        }

        static Dictionary<PostgresAggregateEnum, Regex> aggregateFunctions = new Dictionary<PostgresAggregateEnum, Regex>()
        {
            {PostgresAggregateEnum.AVG  , new Regex(@"^avg\(([a-zA-z1-9._:]*)\)") },
            {PostgresAggregateEnum.COUNT, new Regex(@"^count\(([a-zA-z1-9._:]*)\)") },
            {PostgresAggregateEnum.MAX  , new Regex(@"^max\(([a-zA-z1-9._:]*)\)") },
            {PostgresAggregateEnum.MIN  , new Regex(@"^min\(([a-zA-z1-9._:]*)\)") },
            {PostgresAggregateEnum.SUM  , new Regex(@"^sum\(([a-zA-z1-9._:]*)\)") }
        };

        static Dictionary<PostgresAggregateEnum, string> aggregateFunctionTemplates = new Dictionary<PostgresAggregateEnum, string>()
        {
            {PostgresAggregateEnum.AVG,   @"avg({0})"},
            {PostgresAggregateEnum.COUNT, @"count({0})"},
            {PostgresAggregateEnum.MAX,   @"max({0})"},
            {PostgresAggregateEnum.MIN,   @"min({0})"},
            {PostgresAggregateEnum.SUM,   @"sum({0})"}
        };

        public static string Extract(string field)
        {
            Match match = null;
            foreach (var funct in aggregateFunctions)
            {
                match = funct.Value.Match(field);

                if (match.Success) break;
            }

            string fieldValue = match.Success && match.Groups.Count > 0 ? match.Groups[1].Value : field;

            // if the field definition includes a type cast, remove it in order to prevent wrong field name.
            if (fieldValue.Contains("::"))
            {
                fieldValue = fieldValue.Split(':').First();
            }

            return fieldValue;
        }

        public static string Parse(string field, string fieldPrefix = "", string fieldSuffix = "")
        {
            string output = "";
            PostgresAggregateEnum aggr = Contains(field);
            if (aggr != PostgresAggregateEnum.NONE)
            {
                output = aggregateFunctions[aggr].Match(field).Groups[1].Value;
                output = $"{fieldPrefix}{output}{fieldSuffix}";
                output = aggregateFunctionTemplates[aggr].Replace("{0}", output);
            }
            else
            {
                output = field;
                output = $"{fieldPrefix}{output}{fieldSuffix}";
            }
            return output;
        }


        public static PostgresAggregateEnum Contains(string field)
        {
            PostgresAggregateEnum result = PostgresAggregateEnum.NONE;
            foreach (var funct in aggregateFunctions)
            {
                if (funct.Value.IsMatch(field))
                {
                    result = funct.Key;
                    break;
                }
            }

            return result;
        }
    }
}
