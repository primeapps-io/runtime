using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Npgsql;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;

namespace PrimeApps.Model.Helpers
{
    public static class Extentions
    {
        public static T GetValueOrDefault<T>(this IDataRecord record, int ordinal)
        {
            return (T)(record.IsDBNull(ordinal) ? default(T) : record.GetValue(ordinal));
        }

        public static T GetValueOrDefault<T>(this IDataRecord record, int ordinal, T defa)
        {
            return (T)(record.IsDBNull(ordinal) ? defa : record.GetValue(ordinal));
        }

        public static JArray RecordAsJArray(this IDataRecord record)
        {
            var data = new object[record.FieldCount];
            var columns = record.GetValues(data);
            var result = new JArray();

            for (var j = 0; j < columns; j++)
            {
                result.Add(new JObject(
                                        new JProperty("Name", record.GetName(j)),
                                        new JProperty("Value", data[j])));
            }

            return result;
        }

        public static JObject RecordAsJObject(this IDataRecord record)
        {
            var data = new object[record.FieldCount];
            var columns = record.GetValues(data);
            var result = new JObject();

            for (var j = 0; j < columns; j++)
            {
                if (result[record.GetName(j)] != null)
                    continue;

                result.Add(new JProperty(record.GetName(j), data[j]));
            }

            return result;
        }

        public static string RecordAsJson(this IDataRecord record)
        {
            return RecordAsJObject(record).ToString();
        }

        public static JArray MultiResultToJArray(this DbDataReader reader)
        {
            var result = new JArray();

            while (true)
            {
                while (reader.Read())
                {
                    result.Add(reader.RecordAsJObject());
                }
                if (!reader.NextResult())
                {
                    break;
                }
            }

            return result;
        }

        public static JObject MultiResultToJObject(this DbDataReader reader, params string[] collectionNames)
        {
            Contract.Requires<ArgumentNullException>(reader != null);

            var result = new JObject();
            var res = 0;

            while (true)
            {
                var name = collectionNames != null && collectionNames.Length > res ? collectionNames[res++] : string.Concat("unamed_collection_", res++);

                result.Add(new JProperty(name, ResultToJArray(reader)));

                if (!reader.NextResult())
                {
                    break;
                }
            }

            return result;
        }

        public static JArray ResultToJArray(this DbDataReader reader)
        {
            var result = new JArray();

            while (reader.Read())
            {
                result.Add(reader.RecordAsJObject());
            }

            return result;
        }

        public static JObject ResultToJObject(this DbDataReader reader, string collectioName)
        {
            Contract.Requires<ArgumentNullException>(reader != null);
            Contract.Requires<ArgumentNullException>(collectioName != null);

            return new JObject(new JProperty(collectioName, ResultToJArray(reader)));
        }

        public static string ToJson(this DbDataReader reader)
        {
            return MultiResultToJArray(reader).ToString();
        }

        public static string ToJson(this DbDataReader reader, string rootName)
        {
            return MultiResultToJObject(reader, rootName).ToString();
        }

        public static JArray SqlQueryDynamic(this DatabaseFacade database, string sql)
        {
            using (var command = (NpgsqlCommand)database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;

                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();

                using (var dataReader = command.ExecuteReader())
                {
                    var rows = dataReader.MultiResultToJArray();

                    return rows;
                }
            }
        }

        public static JArray FunctionQueryDynamic(this DatabaseFacade database, string functionName, List<NpgsqlParameter> parameters = null)
        {
            using (var command = (NpgsqlCommand)database.GetDbConnection().CreateCommand())
            {
                command.CommandText = functionName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();

                using (var dataReader = command.ExecuteReader())
                {
                    var rows = dataReader.MultiResultToJArray();

                    return rows;
                }
            }
        }

        /// <summary>
        /// This is an extension method to check a JToken is numeric
        /// </summary>
        /// <param name="token">token</param>
        /// <returns></returns>
        public static bool IsNumeric(this JToken token)
        {
            if (token == null)
                return false;

            double number;
            return double.TryParse(Convert.ToString(token, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number);
        }

        /// <summary>
        /// This is an extension method to check a JToken is null or empty
        /// </summary>
        /// <param name="token">token</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == string.Empty) ||
                   (token.Type == JTokenType.Null);
        }

        public static string ToJsonString(this object source)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(source, Formatting.Indented, jsonSerializerSettings);
        }

        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);

            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static T To<T>(this JToken token)
        {
            try
            {
                var type = typeof(T);
                object newToken;

                if (token.IsNullOrEmpty())
                {
                    if (type.Name.ToString() == "String")
                    {
                        newToken = "";
                        return (T)newToken;
                    }

                    return default(T);
                }

                newToken = Convert.ChangeType(token, type);

                if (newToken == null)
                    return default(T);
                else
                    return (T)newToken;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
    }
}
