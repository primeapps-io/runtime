using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using System.Collections.Generic;

namespace PrimeApps.Model.Helpers
{
    public class DataHelpers
    {
        public static JArray LoadDataAsJson(TenantDBContext ctx, string sqlSelect, params object[] sqlParameters)
        {
            var table = new List<Dictionary<string, object>>();
            var list = new JArray();
            if (ctx.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            {
                ctx.Database.GetDbConnection().Open();
            }
            using (var cmd = ctx.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = sqlSelect;
                if (sqlParameters != null && sqlParameters.Length > 0)
                {
                    foreach (var param in sqlParameters)
                        cmd.Parameters.Add(param);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        JObject jRow = new JObject();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader[i];
                            var val = JToken.FromObject(reader[i]);
                            jRow[reader.GetName(i)] = val;
                        }
                        if (jRow.Count > 0)
                        {
                            list.Add(jRow);
                        }

                    }

                }
            }

            return list;
        }
        public static List<Dictionary<string, object>> LoadData(TenantDBContext ctx, string sqlSelect, params object[] sqlParameters)
        {
            var table = new List<Dictionary<string, object>>();

            if (ctx.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            {
                ctx.Database.GetDbConnection().Open();
            }
            using (var cmd = ctx.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = sqlSelect;
                if (sqlParameters != null && sqlParameters.Length > 0)
                {
                    foreach (var param in sqlParameters)
                        cmd.Parameters.Add(param);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader[i];
                        table.Add(row);
                    }
                }
            }

            return table;
        }
    }
}
