using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Helpers.QueryTranslation;
using System.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Helpers
{
    public static class RecordHelper
    {
        public static string GenerateGetSql(Module module, ICollection<Module> lookupModules, int recordId, string owners = null, int userId = 0, string userGroups = null, bool deleted = false)
        {
            var moduleName = module.Name;
            var tableName = moduleName != "users" ? moduleName + "_d" : "users";
            var fieldsSql = "";
            var joinSql = "";

            if (moduleName != "users")
            {
                foreach (var systemField in ModuleHelper.SystemFields)
                {
                    fieldsSql += $"\"{moduleName}\".\"{systemField}\", ";
                }

                foreach (var moduleSpecificField in ModuleHelper.ModuleSpecificFields(module))
                {
                    fieldsSql += $"\"{moduleName}\".\"{moduleSpecificField}\", ";
                }
            }
            else
            {
                fieldsSql += "\"users\".\"id\", \"users\".\"deleted\", ";

                foreach (var standardField in ModuleHelper.StandardFields)
                {
                    fieldsSql += $"\"users\".\"{standardField}\", ";
                }
            }

            //Custom module fields
            switch (moduleName)
            {
                case "activities":
                    fieldsSql += $"\"{moduleName}\".\"activity_type_system\", ";
                    break;
                case "current_accounts":
                    fieldsSql += $"\"{moduleName}\".\"transaction_type_system\", ";
                    break;
            }

            foreach (var field in module.Fields)
            {
                if (field.DataType == DataType.Lookup && lookupModules != null && lookupModules.Count > 0 && field.LookupType != "relation")
                {
                    var lookupTableName = field.LookupType != "users" ? field.LookupType + "_d" : "users";

                    var alias = field.Name + "_" + field.LookupType;
                    var lookupModule = lookupModules.SingleOrDefault(x => x.Name == field.LookupType);

                    if (lookupModule == null)
                        continue;

                    fieldsSql += $"\"{alias}\".\"id\" AS \"{field.Name}.id\", ";

                    var lookupModuleFields = lookupModule.Fields.Where(x => !x.Deleted);

                    foreach (var lookupField in lookupModuleFields)
                    {
                        if (lookupModule.Name == "users" && lookupField.Name != "id" && lookupField.Name != "email" && lookupField.Name != "full_name")
                            continue;

                        fieldsSql += $"\"{alias}\".\"{lookupField.Name}\" AS \"{field.Name}.{lookupField.Name}\", ";
                    }

                    joinSql += $"LEFT OUTER JOIN {lookupTableName} AS \"{alias}\" ON \"{alias}\".\"id\" = \"{moduleName}\".\"{field.Name}\" AND \"{alias}\".\"deleted\" IS NOT TRUE\n";
                }
                else
                {
                    fieldsSql += $"\"{moduleName}\".\"{field.Name}\", ";
                }
            }

            fieldsSql = fieldsSql.Trim().TrimEnd(',');

            var sql = "PREPARE SelectQuery AS\n" +
                      $"SELECT {fieldsSql}\n" +
                      ", pr.process_id, pr.process_status, pr.operation_type, pr.process_status_order\n" +//Approval Processes
                      $"FROM {tableName} AS \"{moduleName}\"\n" +
                      $"LEFT OUTER JOIN process_requests AS pr ON pr.record_id = \"{moduleName}\".\"id\" AND pr.\"module\" = '{moduleName}'" +//Approval Processes
                      $"{joinSql}";

            if (!deleted)
                sql += $"WHERE \"{moduleName}\".\"deleted\" = FALSE\n";
            else
                sql += $"WHERE 1 = 1\n";

            if (!string.IsNullOrEmpty(owners))
            {
                sql += $"AND (\n\t\"{moduleName}\".\"owner\" = ANY(ARRAY[{owners}]) \n\tOR {userId} = ANY(\"{moduleName}\".\"shared_users\") \n\tOR {userId} = ANY(\"{moduleName}\".\"shared_users_edit\")";

                if (!string.IsNullOrEmpty(userGroups))
                    sql += $"\n\tOR \"{moduleName}\".\"shared_user_groups\" <@ ARRAY[{userGroups}]\n\tOR \"{moduleName}\".\"shared_user_groups_edit\" <@ ARRAY[{userGroups}]";

                sql += "\n)\n";
            }

            sql += $"AND \"{moduleName}\".\"id\" = $1;\n" +
                   $"EXECUTE SelectQuery" +
                   $" ({recordId});\n" +
                   "DEALLOCATE SelectQuery;";

            return sql;
        }

        /// <summary>
        /// Searches dynamic module records with advanced filters, relations and aggregation support.
        /// </summary>
        /// <param name="moduleName">Module Name</param>
        /// <param name="findRequest">Request parameters</param>
        /// <param name="owners">Record owners</param>
        /// <param name="userId">Id of User who created the record</param>
        /// <param name="userGroups">Groups that can see searched records</param>
        /// <returns></returns>
        public static string GenerateFindSql(string moduleName, FindRequest findRequest, string owners = null, int userId = 0, string userGroups = null, int timezoneOffset = 180)
        {
            var joins = new Dictionary<string, string>();
            var tableName = moduleName != "users" ? moduleName + "_d" : "users";
            var fieldsSql = "\"" + tableName + "\".*";
            var aggregateFieldsSql = "";
            var idColumn = "id";
            var parsedField = "";
            // if this is true aggregated query options will be activated and the main query will be regarded as a subquery.
            bool isAggregate = false;

            // contains fields that are used in projection part of the query. used in order to prevent duplicated column names. 
            IList<string> queryFields = new List<string>();

            // check if projection parameters contains any aggregation function.
            isAggregate = findRequest.Fields != null ? findRequest.Fields.Any(x => PostgresAggregateFunction.Contains(x) != PostgresAggregateFunction.PostgresAggregateEnum.NONE) : false;

            if (!string.IsNullOrWhiteSpace(findRequest.ManyToMany))
            {
                //Check this relation created by user or dynamically. And if relation type is many to many. Use same table.
                if (findRequest.TwoWay)
                    tableName = moduleName + "_" + findRequest.ManyToMany + "_d";
                else
                    tableName = findRequest.ManyToMany + "_" + moduleName + "_d";

                idColumn = moduleName + "_id";

                if (findRequest.ManyToMany.Contains("|"))
                {
                    var manyToManyParts = findRequest.ManyToMany.Split('|');
                    tableName = manyToManyParts[0] + "_" + moduleName + "_" + manyToManyParts[1] + "_d";
                }
            }

            if (isAggregate)
            {
                // this is an aggregated query, so generate projection part for the aggregated query.
                foreach (var field in findRequest.Fields)
                {
                    var fieldParse = field;

                    if (field.Contains("count("))
                        fieldParse = "count(id)";

                    var fieldSuffix = "\"";

                    if (field.ToLower().Contains("sum(") || field.ToLower().Contains("avg("))
                        fieldSuffix = "\"::numeric";

                    // parse aggregate function if it exist, otherwise just return the input string.
                    aggregateFieldsSql += $"{PostgresAggregateFunction.Parse(fieldParse, "sub.\"", fieldSuffix)}, ";
                }

                aggregateFieldsSql = aggregateFieldsSql.Trim().TrimEnd(',');
            }

            if (findRequest.Fields != null && findRequest.Fields.Count > 0)
            {
                fieldsSql = $"\"{tableName}\".\"{idColumn}\", ";
                var joinIdsAdded = new List<string>();

                foreach (var field in findRequest.Fields)
                {
                    if (field == idColumn)
                        continue;

                    if (!field.Contains("."))
                    {
                        if (field != "total_count()")
                        {
                            parsedField = PostgresAggregateFunction.Extract(field);
                            if (!queryFields.Contains(parsedField))
                            {
                                fieldsSql += $"\"{tableName}\".\"{parsedField}\", ";
                                queryFields.Add(parsedField);
                            }
                        }
                        else
                        {
                            fieldsSql += $"COUNT(\"{tableName}\".\"{idColumn}\") OVER() AS \"total_count\", ";
                        }
                    }
                    else
                    {
                        parsedField = PostgresAggregateFunction.Extract(field);
                        var fieldParts = parsedField.Split('.');
                        if (fieldParts[2] == "id")
                            continue;

                        //Approval Processes
                        if (fieldParts[1] == "process_requests")
                            continue;

                        if (!joinIdsAdded.Contains(fieldParts[0]))
                        {
                            fieldsSql += $"\"{fieldParts[1]}_{fieldParts[0]}\".\"id\" AS \"{fieldParts[0] + "." + fieldParts[1] + ".id"}\", ";
                            joinIdsAdded.Add(fieldParts[0]);
                        }

                        fieldsSql += $"\"{fieldParts[1]}_{fieldParts[0]}\".\"{fieldParts[2]}\" AS \"{field}\", ";

                        if (!joins.ContainsKey(fieldParts[0]))
                            joins.Add(fieldParts[0], fieldParts[1]);
                    }
                }

                fieldsSql = fieldsSql.Trim().TrimEnd(',');

                //Approval Processes
                if (string.IsNullOrEmpty(findRequest.ManyToMany) && moduleName != "quote_products" && moduleName != "order_products" && moduleName != "purchase_order_products")
                    fieldsSql += ", \"process_requests_process\".process_id AS \"process.process_requests.process_id\", process_requests_process.process_status AS \"process.process_requests.process_status\", process_requests_process.operation_type AS \"process.process_requests.operation_type\", process_requests_process.process_status_order AS \"process.process_requests.process_status_order\"";
            }

            var filtersSql = $"\"{tableName}\".\"deleted\" IS NOT TRUE";

            if (moduleName == "users")
                filtersSql = $"\"{tableName}\".\"is_active\" IS NOT FALSE";


            if (!string.IsNullOrWhiteSpace(findRequest.ManyToMany))
                filtersSql = "";

            if (findRequest.Filters != null && findRequest.Filters.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(findRequest.FilterLogic))
                {
                    var filtersHasValue = findRequest.Filters.Where(x => x.Operator != Operator.Empty && x.Operator != Operator.NotEmpty).OrderBy(x => x.No).ToList();
                    var filtersHasNotValue = findRequest.Filters.Where(x => x.Operator == Operator.Empty || x.Operator == Operator.NotEmpty).OrderBy(x => x.No).ToList();
                    var filters = new List<string>();
                    var filterIndexExt = filtersHasValue.Count(x => x.Operator != Operator.NotIn);

                    for (var i = 0; i < filtersHasValue.Count; i++)
                    {
                        if (filtersHasValue[i].Value.ToString().Contains("I") || filtersHasValue[i].Value.ToString().Contains("ı"))//"Turkish i problem" fix
                            filterIndexExt++;

                        var filter = filtersHasValue[i];
                        filters.Add(GetFilterItemSql(filter, tableName, i + 1, filterIndexExt));
                    }

                    foreach (var filterHasNotValue in filtersHasNotValue)
                    {
                        filters.Add(GetFilterItemSql(filterHasNotValue, tableName, 0, 0));
                    }

                    var logicType = "AND";

                    if (findRequest.LogicType == LogicType.Or)
                        logicType = "OR";

                    filtersSql = string.Join(" " + logicType + " ", filters);
                }
                else
                {
                    filtersSql = findRequest.FilterLogic;
                    var filterLogicDigits = findRequest.FilterLogic.Where(Char.IsDigit).ToList();
                    var filterIndex = 0;

                    //Operators can be Empty or NotEmpty
                    var filterCount = (from filter in findRequest.Filters
                                       where filter.Value.ToString() == "-"
                                       select filter).Count();

                    var filterIndexExt = filterLogicDigits.Count - filterCount;

                    for (var i = 1; i < filterLogicDigits.Count + 1; i++)
                    {
                        filtersSql = filtersSql.Replace(i.ToString(), "::" + i + "::");
                    }

                    foreach (var filterLogicDigit in filterLogicDigits)
                    {
                        var filter = findRequest.Filters.FirstOrDefault(x => x.No == Byte.Parse(filterLogicDigit.ToString()));
                        if (filter.Operator != Operator.Empty && filter.Operator != Operator.NotEmpty)
                        {
                            if (filter.Value.ToString().Contains("I") || filter.Value.ToString().Contains("ı"))//"Turkish i problem" fix
                                filterIndexExt++;

                            filtersSql = filtersSql.Replace("::" + filterLogicDigit + "::", GetFilterItemSql(filter, tableName, filterIndex + 1, filterIndexExt));
                            filterIndex++;
                        }
                        else
                        {
                            filtersSql = filtersSql.Replace("::" + filterLogicDigit + "::", GetFilterItemSql(filter, tableName, 0, 0));
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(findRequest.ManyToMany))
                    filtersSql = $"\"{tableName}\".\"deleted\" IS NOT TRUE \nAND ({filtersSql})";
            }

            if (!string.IsNullOrEmpty(owners) && string.IsNullOrWhiteSpace(findRequest.ManyToMany) && moduleName != "quote_products" && moduleName != "order_products" && moduleName != "purchase_order_products")
            {
                filtersSql += $"\nAND (\n\"{tableName}\".\"owner\" = ANY(ARRAY[{owners}]) \nOR {userId} = ANY(\"{tableName}\".\"shared_users\") \nOR {userId} = ANY(\"{tableName}\".\"shared_users_edit\")";

                if (!string.IsNullOrEmpty(userGroups))
                    filtersSql += $"\nOR \"{tableName}\".\"shared_user_groups\" <@ ARRAY[{userGroups}] \nOR \"{tableName}\".\"shared_user_groups_edit\" <@ ARRAY[{userGroups}]";

                filtersSql += "\n)";
            }

            var sortSql = $"\"{tableName}\".\"{idColumn}\" DESC";

            if (!string.IsNullOrWhiteSpace(findRequest.SortField))
            {
                var sortDirection = "DESC";

                if (findRequest.SortDirection != SortDirection.NotSet)
                    sortDirection = findRequest.SortDirection.ToString().ToUpper();

                if (!findRequest.SortField.Contains("."))
                {
                    sortSql = $"\"{tableName}\".\"{findRequest.SortField}\" {sortDirection}";
                }
                else
                {
                    var sortFieldParts = findRequest.SortField.Split('.');
                    var alias = sortFieldParts[1] + "_" + sortFieldParts[0];
                    sortSql = $"\"{alias}\".\"{sortFieldParts[2]}\"  {sortDirection}";
                }
            }

            var limit = findRequest.Limit > 0 ? findRequest.Limit : 10;

            var sql = "PREPARE SelectQuery AS\n";

            if (!isAggregate)
            {
                sql += $"SELECT {fieldsSql}\n" +
                      $"FROM {tableName}\n";
            }
            else
            {
                sql += $"SELECT {aggregateFieldsSql} \nFROM \n(\nSELECT {fieldsSql}\n" +
                    $"FROM {tableName} ";
            }

            if (string.IsNullOrEmpty(findRequest.ManyToMany) && moduleName != "quote_products" && moduleName != "order_products" && moduleName != "purchase_order_products")//Approval Processes
            {
                sql += $"LEFT OUTER JOIN process_requests AS process_requests_process ON process_requests_process.\"record_id\" = \"{tableName}\".\"id\" AND process_requests_process.\"module\" = '{moduleName}' ";

                if (filtersSql.Contains("process_approvers"))
                    sql += "\nJOIN process_approvers AS process_approvers_process ON process_approvers_process.\"process_id\" = process_requests_process.\"process_id\" AND process_approvers_process.\"order\" = process_requests_process.\"process_status_order\" ";
            }

            foreach (var jn in joins)
            {
                var joinTableName = jn.Value != "users" ? jn.Value + "_d" : "users";
                var alias = jn.Value + "_" + jn.Key;

                if (string.IsNullOrWhiteSpace(findRequest.ManyToMany))
                    sql += "LEFT OUTER ";

                sql += $"JOIN {joinTableName} AS \"{alias}\" ON \"{alias}\".\"id\" = {tableName}.\"{jn.Key}\"";

                if (moduleName != "quote_products" && moduleName != "order_products" && moduleName != "purchase_order_products")
                    sql += $"AND \"{alias}\".\"deleted\" IS NOT TRUE";

                if (!string.IsNullOrEmpty(owners) && !string.IsNullOrWhiteSpace(findRequest.ManyToMany))
                    sql += $" AND \"{alias}\".\"owner\" = ANY(ARRAY[{owners}])\n";
                else
                    sql += "\n";
            }

            if (!string.IsNullOrEmpty(filtersSql))
                sql += $"WHERE {filtersSql}\n";

            if (!isAggregate)
            {
                sql += $"ORDER BY {sortSql} NULLS LAST\n" +
                   $"LIMIT {limit}\n" +
                   $"OFFSET {findRequest.Offset};\n" +
                   "EXECUTE SelectQuery";
            }
            else
            {
                sql += $"ORDER BY {sortSql} NULLS LAST\n" +
                    ") sub\n";

                if (!string.IsNullOrWhiteSpace(findRequest.GroupBy))
                {
                    sql += $"GROUP BY \"{findRequest.GroupBy}\"";
                }

                sql += ";\nEXECUTE SelectQuery";
            }

            var selectQuery = "";

            if (findRequest.Filters != null && findRequest.Filters.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(findRequest.FilterLogic))
                {
                    var filtersHasValue = findRequest.Filters.Where(x => x.Operator != Operator.Empty && x.Operator != Operator.NotEmpty).OrderBy(x => x.No).ToList();

                    foreach (var filter in filtersHasValue)
                    {
                        if (filter.Operator != Operator.NotIn)
                            selectQuery += GetQueryParameterValue(filter, timezoneOffset) + ", ";
                    }

                    //"Turkish i problem" fix
                    foreach (var filter in filtersHasValue)
                    {
                        // do not replace values in dynamic date functions.
                        if (filter.Value.ToString().Contains("date_trunc(") || filter.Value.ToString().Contains("now("))
                            continue;

                        if (filter.Value.ToString().Contains("ı") && filter.Value.ToString().Contains("I"))
                        {
                            filter.Value = filter.Value.ToString().ToLower();
                        }

                        if (filter.Value.ToString().Contains("ı"))
                        {
                            filter.Value = filter.Value.ToString().Replace("ı", "i");
                            selectQuery += GetQueryParameterValue(filter) + ", ";
                        }

                        if (filter.Value.ToString().Contains("I"))
                        {
                            filter.Value = filter.Value.ToString().Replace("I", "ı");
                            selectQuery += GetQueryParameterValue(filter, timezoneOffset) + ", ";
                        }
                    }
                }
                else
                {
                    var filterLogicDigits = findRequest.FilterLogic.Where(Char.IsDigit).ToList();

                    foreach (var filterLogicDigit in filterLogicDigits)
                    {
                        var filter = findRequest.Filters.FirstOrDefault(x => x.No == Byte.Parse(filterLogicDigit.ToString()));

                        if (filter.Operator != Operator.Empty && filter.Operator != Operator.NotEmpty && filter.Operator != Operator.NotIn)
                            selectQuery += GetQueryParameterValue(filter, timezoneOffset) + ", ";
                    }

                    //"Turkish i problem" fix
                    foreach (var filterLogicDigit in filterLogicDigits)
                    {
                        // do not replace values in dynamic date functions.
                        var filter = findRequest.Filters.FirstOrDefault(x => x.No == Byte.Parse(filterLogicDigit.ToString()));

                        // do not replace values in dynamic date functions.
                        if (filter.Value.ToString().Contains("date_trunc(") || filter.Value.ToString().Contains("now(")) continue;

                        if (filter.Value.ToString().Contains("ı"))
                        {
                            filter.Value = filter.Value.ToString().Replace("ı", "i");
                            selectQuery += GetQueryParameterValue(filter, timezoneOffset) + ", ";
                        }

                        if (filter.Value.ToString().Contains("I"))
                        {
                            filter.Value = filter.Value.ToString().Replace("I", "ı");
                            selectQuery += GetQueryParameterValue(filter, timezoneOffset) + ", ";
                        }
                    }
                }

                selectQuery = selectQuery.Trim().TrimEnd(',');
            }

            if (!string.IsNullOrEmpty(selectQuery))
                sql += "(" + selectQuery + ")";

            sql += ";\nDEALLOCATE SelectQuery;";

            return sql;
        }

        public static string GenerateRoleBasedSql(string moduleName, int userId)
        {
            var sql = "SELECT m.\"sharing\", r.\"owners\", p.\"has_admin_rights\", cr.\"owners\" AS \"custom_owners\", cs.\"modules\", cs.\"shared_user_id\"\n" +
                      "FROM users u\n" +
                      "JOIN roles r ON r.\"id\" = u.\"role_id\"\n" +
                      "JOIN profiles p ON p.\"id\" = u.\"profile_id\"\n" +
                      $"JOIN modules m ON m.\"name\" = '{moduleName}'\n" +
                      "LEFT JOIN user_custom_shares cs ON cs.\"user_id\" = u.\"id\" AND cs.\"deleted\" = FALSE\n" +
                      "LEFT JOIN users cu ON cu.\"id\" = cs.\"shared_user_id\"\n" +
                      "LEFT JOIN roles cr ON cr.\"id\" = cu.\"role_id\"\n" +
                      $"WHERE u.\"id\" = {userId}";

            return sql;
        }

        public static string GenerateUserGroupSql(int userId)
        {
            var sql = "SELECT users_user_groups.\"group_id\"\n" +
                      "FROM users_user_groups\n" +
                      $"WHERE users_user_groups.\"user_id\" = {userId}";

            return sql;
        }

        public static string GenerateGetAllByIdSql(string moduleName, List<int> recordIds, string owners = null, int userId = 0, string userGroups = null)
        {
            var tableName = moduleName != "users" ? moduleName + "_d" : "users";
            var fieldsSql = "\"" + tableName + "\".*";
            var filtersSql = $"\"{tableName}\".\"deleted\" IS NOT TRUE";
            filtersSql += $"\nAND \"{tableName}\".\"id\" IN('{string.Join("', '", recordIds)}')";

            if (!string.IsNullOrEmpty(owners))
            {
                filtersSql += $"\nAND (\n\"{tableName}\".\"owner\" = ANY(ARRAY[{owners}]) \nOR {userId} = ANY(\"{tableName}\".\"shared_users\") \nOR {userId} = ANY(\"{tableName}\".\"shared_users_edit\")";

                if (!string.IsNullOrEmpty(userGroups))
                    filtersSql += $"\nOR \"{tableName}\".\"shared_user_groups\" <@ ARRAY[{userGroups}] \nOR \"{tableName}\".\"shared_user_groups_edit\" <@ ARRAY[{userGroups}]";

                filtersSql += "\n)";
            }

            var sql = "PREPARE SelectQuery AS\n" +
                      $"SELECT {fieldsSql}\n" +
                      $"FROM {tableName}\n";

            if (!string.IsNullOrEmpty(filtersSql))
                sql += $"WHERE {filtersSql};";

            sql += "\nEXECUTE SelectQuery;\nDEALLOCATE SelectQuery;";

            return sql;
        }

        private static string GetQueryParameterValue(Filter filter, int timeZoneOffset = 180)
        {
            bool dynamicParameterValue = false;

            if (filter.Value.GetType() == typeof(JArray))
            {
                if (filter.Operator == Operator.NotIn)
                    return "";

                var values = (JArray)filter.Value;
                var valuesLower = new List<string>();

                foreach (var value in values)
                {
                    valuesLower.Add("LOWER('" + value.ToString().Replace("'", "''") + "')");
                }

                return $"ARRAY[{string.Join(",", valuesLower)}]";
            }

            var valueString = filter.Value.ToString();
            filter.Value = PostgresDynamicDate.Parse(valueString, timeZoneOffset);

            if (valueString != filter.Value.ToString())
                return filter.Value.ToString();

            switch (filter.Operator)
            {
                case Operator.Is:
                case Operator.IsNot:
                    return "LOWER('" + filter.Value.ToString().Replace("'", "''") + "')";
                case Operator.Contains:
                case Operator.NotContain:
                    return "LOWER('%" + filter.Value.ToString().Replace("'", "''").Replace("%", "\\%") + "%')";
                case Operator.StartsWith:
                    return "LOWER('" + filter.Value.ToString().Replace("'", "''").Replace("%", "\\%") + "%')";
                case Operator.EndsWith:
                    return "LOWER('%" + filter.Value.ToString().Replace("'", "''").Replace("%", "\\%") + "')";
                default:
                    DateTime valueDate;
                    double isDouble;

                    if (!double.TryParse(valueString, out isDouble))
                    {
                        if (DateTime.TryParse(valueString, out valueDate))
                        {
                            if (valueString.Length > 10)
                                valueString = valueDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                            else
                                valueString = valueDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                        }
                    }

                    return "'" + valueString.Replace("'", "''") + "'";
            }
        }

        public static void AddCommandParameters(NpgsqlCommand command, JObject record, Module module)
        {
            foreach (var property in record)
            {
                if (ModuleHelper.SystemFieldsExtended.Contains(property.Key))
                    continue;

                if (ModuleHelper.ModuleSpecificFields(module).Contains(property.Key))
                    continue;

                var key = property.Key;
                var value = property.Value.ToString();
                var field = module.Fields.FirstOrDefault(x => x.Name == key);

                if (field == null)
                    continue;

                if (record["id"].IsNullOrEmpty() && string.IsNullOrWhiteSpace(value))
                    continue;

                switch (field.DataType)
                {
                    case DataType.TextSingle:
                    case DataType.TextMulti:
                    case DataType.Email:
                    case DataType.Picklist:
                    case DataType.Document:
                    case DataType.Location:
                    case DataType.Url:
                    case DataType.Image:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = value, NpgsqlDbType = NpgsqlDbType.Varchar });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Varchar });
                        break;
                    case DataType.Multiselect:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = value.Split('|'), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Varchar });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Varchar });
                        break;
                    case DataType.Number:
                    case DataType.NumberAuto:
                    case DataType.NumberDecimal:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = Decimal.Parse(value), NpgsqlDbType = NpgsqlDbType.Numeric });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Numeric });
                        break;
                    case DataType.Currency:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = Decimal.Parse(value), NpgsqlDbType = NpgsqlDbType.Money });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Money });
                        break;
                    case DataType.Date:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DateTime.Parse(value), NpgsqlDbType = NpgsqlDbType.Date });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Date });
                        break;
                    case DataType.DateTime:
                    case DataType.Time:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DateTime.Parse(value).ToUniversalTime(), NpgsqlDbType = NpgsqlDbType.Timestamp });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Timestamp });
                        break;
                    case DataType.Lookup:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = Int32.Parse(value), NpgsqlDbType = NpgsqlDbType.Integer });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });
                        break;
                    case DataType.Checkbox:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = Boolean.Parse(value), NpgsqlDbType = NpgsqlDbType.Boolean });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Boolean });
                        break;
                    case DataType.Rating:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = Decimal.Parse(value), NpgsqlDbType = NpgsqlDbType.Numeric });
                        else
                            command.Parameters.Add(new NpgsqlParameter { ParameterName = key, NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Numeric });
                        break;
                }
            }
        }

        public static void AddCommandStandardParametersCreate(NpgsqlCommand command, JObject record, Module module, List<string> columns, List<string> values, int currentUserId, DateTime now)
        {
            var createdByValue = currentUserId;
            var updatedByValue = currentUserId;
            var createdAtValue = now;
            var updatedAtValue = now;

            if (!record["created_by"].IsNullOrEmpty())
                createdByValue = (int)record["created_by"];

            if (!record["updated_by"].IsNullOrEmpty())
                updatedByValue = (int)record["updated_by"];

            if (!record["created_at"].IsNullOrEmpty())
                createdAtValue = (DateTime)record["created_at"];

            if (!record["updated_at"].IsNullOrEmpty())
                updatedAtValue = (DateTime)record["updated_at"];

            command.Parameters.Add(new NpgsqlParameter { ParameterName = "created_by", NpgsqlValue = createdByValue, NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter { ParameterName = "updated_by", NpgsqlValue = updatedByValue, NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter { ParameterName = "created_at", NpgsqlValue = createdAtValue, NpgsqlDbType = NpgsqlDbType.Timestamp });
            command.Parameters.Add(new NpgsqlParameter { ParameterName = "updated_at", NpgsqlValue = updatedAtValue, NpgsqlDbType = NpgsqlDbType.Timestamp });
            columns.Add("\"created_by\"");
            columns.Add("\"updated_by\"");
            columns.Add("\"created_at\"");
            columns.Add("\"updated_at\"");
            values.Add("@created_by");
            values.Add("@updated_by");
            values.Add("@created_at");
            values.Add("@updated_at");

            if (!record["master_id"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "master_id", NpgsqlValue = (int)record["master_id"], NpgsqlDbType = NpgsqlDbType.Integer });
                columns.Add("\"master_id\"");
                values.Add("@master_id");
            }

            if (!record["migration_id"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "migration_id", NpgsqlValue = (string)record["migration_id"], NpgsqlDbType = NpgsqlDbType.Varchar });
                columns.Add("\"migration_id\"");
                values.Add("@migration_id");
            }

            if (!record["shared_users"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_users", NpgsqlValue = record["shared_users"].ToObject<int[]>(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                columns.Add("\"shared_users\"");
                values.Add("@shared_users");
            }

            if (!record["shared_user_groups"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_user_groups", NpgsqlValue = record["shared_user_groups"].ToObject<int[]>(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                columns.Add("\"shared_user_groups\"");
                values.Add("@shared_user_groups");
            }

            if (!record["shared_users_edit"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_users_edit", NpgsqlValue = record["shared_users_edit"].ToObject<int[]>(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                columns.Add("\"shared_users_edit\"");
                values.Add("@shared_users_edit");
            }

            if (!record["shared_user_groups_edit"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_user_groups_edit", NpgsqlValue = record["shared_user_groups_edit"].ToObject<int[]>(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                columns.Add("\"shared_user_groups_edit\"");
                values.Add("@shared_user_groups_edit");
            }

            //Module specific fields
            switch (module.Name)
            {
                case "activities":
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "activity_type_system", NpgsqlValue = (string)record["activity_type_system"], NpgsqlDbType = NpgsqlDbType.Varchar });
                    columns.Add("\"activity_type_system\"");
                    values.Add("@activity_type_system");
                    break;
                case "opportunities":
                    if (!record["forecast_type"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_type", NpgsqlValue = (string)record["forecast_type"], NpgsqlDbType = NpgsqlDbType.Varchar });
                        columns.Add("\"forecast_type\"");
                        values.Add("@forecast_type");
                    }

                    if (!record["forecast_category"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_category", NpgsqlValue = (string)record["forecast_category"], NpgsqlDbType = NpgsqlDbType.Varchar });
                        columns.Add("\"forecast_category\"");
                        values.Add("@forecast_category");
                    }

                    if (!record["forecast_year"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_year", NpgsqlValue = (int)record["forecast_year"], NpgsqlDbType = NpgsqlDbType.Integer });
                        columns.Add("\"forecast_year\"");
                        values.Add("@forecast_year");
                    }

                    if (!record["forecast_month"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_month", NpgsqlValue = (int)record["forecast_month"], NpgsqlDbType = NpgsqlDbType.Integer });
                        columns.Add("\"forecast_month\"");
                        values.Add("@forecast_month");
                    }

                    if (!record["forecast_quarter"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_quarter", NpgsqlValue = (int)record["forecast_quarter"], NpgsqlDbType = NpgsqlDbType.Integer });
                        columns.Add("\"forecast_quarter\"");
                        values.Add("@forecast_quarter");
                    }
                    break;
                case "current_accounts":
                    if (!record["transaction_type_system"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "transaction_type_system", NpgsqlValue = (string)record["transaction_type_system"], NpgsqlDbType = NpgsqlDbType.Varchar });
                        columns.Add("\"transaction_type_system\"");
                        values.Add("@transaction_type_system");
                    }
                    break;
                case "quotes":
                case "sales_orders":
                case "purchase_orders":
                    if (!record["exchange_rate_try_usd"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_try_usd", NpgsqlValue = (decimal)record["exchange_rate_try_usd"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        columns.Add("\"exchange_rate_try_usd\"");
                        values.Add("@exchange_rate_try_usd");
                    }

                    if (!record["exchange_rate_try_eur"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_try_eur", NpgsqlValue = (decimal)record["exchange_rate_try_eur"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        columns.Add("\"exchange_rate_try_eur\"");
                        values.Add("@exchange_rate_try_eur");
                    }

                    if (!record["exchange_rate_usd_try"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_usd_try", NpgsqlValue = (decimal)record["exchange_rate_usd_try"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        columns.Add("\"exchange_rate_usd_try\"");
                        values.Add("@exchange_rate_usd_try");
                    }

                    if (!record["exchange_rate_usd_eur"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_usd_eur", NpgsqlValue = (decimal)record["exchange_rate_usd_eur"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        columns.Add("\"exchange_rate_usd_eur\"");
                        values.Add("@exchange_rate_usd_eur");
                    }

                    if (!record["exchange_rate_eur_try"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_eur_try", NpgsqlValue = (decimal)record["exchange_rate_eur_try"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        columns.Add("\"exchange_rate_eur_try\"");
                        values.Add("@exchange_rate_eur_try");
                    }

                    if (!record["exchange_rate_eur_usd"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_eur_usd", NpgsqlValue = (decimal)record["exchange_rate_eur_usd"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        columns.Add("\"exchange_rate_eur_usd\"");
                        values.Add("@exchange_rate_eur_usd");
                    }
                    break;
            }
        }

        public static void AddCommandStandardParametersUpdate(NpgsqlCommand command, JObject record, Module module, List<string> sets, int currentUserId, DateTime now, bool delete)
        {
            var updatedByValue = currentUserId;
            var updatedAtValue = now;

            if (!record["updated_by"].IsNullOrEmpty())
                updatedByValue = (int)record["updated_by"];

            if (!record["updated_at"].IsNullOrEmpty())
                updatedAtValue = (DateTime)record["updated_at"];

            command.Parameters.Add(new NpgsqlParameter { ParameterName = "updated_by", NpgsqlValue = updatedByValue, NpgsqlDbType = NpgsqlDbType.Integer });
            command.Parameters.Add(new NpgsqlParameter { ParameterName = "updated_at", NpgsqlValue = updatedAtValue, NpgsqlDbType = NpgsqlDbType.Timestamp });

            sets.Add("\"updated_by\" = @updated_by");
            sets.Add("\"updated_at\" = @updated_at");

            if (delete)
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "deleted", NpgsqlValue = true, NpgsqlDbType = NpgsqlDbType.Boolean });
                sets.Add("\"deleted\" = @deleted");
            }

            if (!record["is_sample"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "is_sample", NpgsqlValue = (int)record["is_sample"], NpgsqlDbType = NpgsqlDbType.Boolean });
                sets.Add("\"is_sample\" = @is_sample");
            }

            if (!record["is_converted"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "is_converted", NpgsqlValue = (int)record["is_converted"], NpgsqlDbType = NpgsqlDbType.Boolean });
                sets.Add("\"is_converted\" = @is_converted");
            }

            if (record["master_id"] != null)
            {
                if (!string.IsNullOrWhiteSpace(record["master_id"].ToString()))
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "master_id", NpgsqlValue = (int)record["master_id"], NpgsqlDbType = NpgsqlDbType.Integer });
                else
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "master_id", NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer });

                sets.Add("\"master_id\" = @master_id");
            }

            if (record["shared_users"] != null)
            {
                if (!string.IsNullOrWhiteSpace(record["shared_users"].ToString()))
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_users", NpgsqlValue = record["shared_users"].ToObject<int[]>(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                else
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_users", NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });

                sets.Add("\"shared_users\" = @shared_users");
            }

            if (record["shared_user_groups"] != null)
            {
                if (!string.IsNullOrWhiteSpace(record["shared_user_groups"].ToString()))
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_user_groups", NpgsqlValue = record["shared_user_groups"].ToObject<int[]>(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                else
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_user_groups", NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });

                sets.Add("\"shared_user_groups\" = @shared_user_groups");
            }

            if (record["shared_users_edit"] != null)
            {
                if (!string.IsNullOrWhiteSpace(record["shared_users_edit"].ToString()))
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_users_edit", NpgsqlValue = record["shared_users_edit"].ToObject<int[]>(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                else
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_users_edit", NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });

                sets.Add("\"shared_users_edit\" = @shared_users_edit");
            }

            if (record["shared_user_groups_edit"] != null)
            {
                if (!string.IsNullOrWhiteSpace(record["shared_user_groups_edit"].ToString()))
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_user_groups_edit", NpgsqlValue = record["shared_user_groups_edit"].ToObject<int[]>(), NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });
                else
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = "shared_user_groups_edit", NpgsqlValue = DBNull.Value, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer });

                sets.Add("\"shared_user_groups_edit\" = @shared_user_groups_edit");
            }

            //Module specific fields
            switch (module.Name)
            {
                case "activities":
                    if (!record["activity_type_system"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "activity_type_system", NpgsqlValue = (string)record["activity_type_system"], NpgsqlDbType = NpgsqlDbType.Varchar });
                        sets.Add("\"activity_type_system\" = @activity_type_system");
                    }
                    break;
                case "opportunities":
                    if (!record["forecast_type"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_type", NpgsqlValue = (string)record["forecast_type"], NpgsqlDbType = NpgsqlDbType.Varchar });
                        sets.Add("\"forecast_type\" = @forecast_type");
                    }

                    if (!record["forecast_category"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_category", NpgsqlValue = (string)record["forecast_category"], NpgsqlDbType = NpgsqlDbType.Varchar });
                        sets.Add("\"forecast_category\" = @forecast_category");
                    }

                    if (!record["forecast_year"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_year", NpgsqlValue = (int)record["forecast_year"], NpgsqlDbType = NpgsqlDbType.Integer });
                        sets.Add("\"forecast_year\" = @forecast_year");
                    }

                    if (!record["forecast_month"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_month", NpgsqlValue = (int)record["forecast_month"], NpgsqlDbType = NpgsqlDbType.Integer });
                        sets.Add("\"forecast_month\" = @forecast_month");
                    }

                    if (!record["forecast_quarter"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "forecast_quarter", NpgsqlValue = (int)record["forecast_quarter"], NpgsqlDbType = NpgsqlDbType.Integer });
                        sets.Add("\"forecast_quarter\" = @forecast_quarter");
                    }
                    break;
                case "current_accounts":
                    if (!record["transaction_type_system"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "transaction_type_system", NpgsqlValue = (string)record["transaction_type_system"], NpgsqlDbType = NpgsqlDbType.Varchar });
                        sets.Add("\"transaction_type_system\" = @transaction_type_system");
                    }
                    break;
                case "quotes":
                case "sales_orders":
                case "purchase_orders":
                    if (!record["exchange_rate_try_usd"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_try_usd", NpgsqlValue = (decimal)record["exchange_rate_try_usd"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        sets.Add("\"exchange_rate_try_usd\" = @exchange_rate_try_usd");
                    }

                    if (!record["exchange_rate_try_eur"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_try_eur", NpgsqlValue = (decimal)record["exchange_rate_try_eur"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        sets.Add("\"exchange_rate_try_eur\" = @exchange_rate_try_eur");
                    }

                    if (!record["exchange_rate_usd_try"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_usd_try", NpgsqlValue = (decimal)record["exchange_rate_usd_try"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        sets.Add("\"exchange_rate_usd_try\" = @exchange_rate_usd_try");
                    }

                    if (!record["exchange_rate_usd_eur"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_usd_eur", NpgsqlValue = (decimal)record["exchange_rate_usd_eur"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        sets.Add("\"exchange_rate_usd_eur\" = @exchange_rate_usd_eur");
                    }

                    if (!record["exchange_rate_eur_try"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_eur_try", NpgsqlValue = (decimal)record["exchange_rate_eur_try"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        sets.Add("\"exchange_rate_eur_try\" = @exchange_rate_eur_try");
                    }

                    if (!record["exchange_rate_eur_usd"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new NpgsqlParameter { ParameterName = "exchange_rate_eur_usd", NpgsqlValue = (decimal)record["exchange_rate_eur_usd"], NpgsqlDbType = NpgsqlDbType.Numeric });
                        sets.Add("\"exchange_rate_eur_usd\" = @exchange_rate_eur_usd");
                    }
                    break;
            }
        }

        public static string GenerateSystemDataUpdateSql(int createdBy, DateTime createdAt)
        {
            var sql = $"UPDATE profiles SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE roles SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE picklists SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE picklist_items SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE modules SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE sections SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE fields SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE relations SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE dependencies SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE calculations SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE views SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE view_fields SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE view_filters SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE templates SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE conversion_mappings SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE reports SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE charts SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE widgets SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';\n" +
                      $"UPDATE dashlets SET created_by = '{createdBy}', created_at = '{createdAt:yyyy-M-dd hh:mm:ss}';";

            return sql;
        }

        public static string GenerateSystemDashletUpdateSql(int appId)
        {
            var sql = "";

            switch (appId)
            {
                case 1:
                    sql = "UPDATE reports SET \"name\" = 'Accounts by Industry' WHERE \"id\" = 1;" +
                          "UPDATE reports SET \"name\" = 'Sales Pipeline' WHERE \"id\" = 2;" +
                          "UPDATE reports SET \"name\" = 'Current Sales Orders' WHERE \"id\" = 3;" +
                          "UPDATE reports SET \"name\" = 'Total Lead Count' WHERE \"id\" = 4;" +
                          "UPDATE reports SET \"name\" = 'Total Account Count' WHERE \"id\" = 5;" +
                          "UPDATE reports SET \"name\" = 'Total Contact Count' WHERE \"id\" = 6;" +
                          "UPDATE reports SET \"name\" = 'Total Opportunity Count' WHERE \"id\" = 7;" +
                          "UPDATE reports SET \"name\" = 'Leads by Status' WHERE \"id\" = 8;" +
                          "UPDATE reports SET \"name\" = 'Quotes by Valid Date' WHERE \"id\" = 9;" +
                          "UPDATE widgets SET \"name\" = 'Total Lead Count' WHERE \"id\" = 1;" +
                          "UPDATE widgets SET \"name\" = 'Total Account Count' WHERE \"id\" = 2;" +
                          "UPDATE widgets SET \"name\" = 'Total Contact Count' WHERE \"id\" = 4;" +
                          "UPDATE widgets SET \"name\" = 'Total Opportunity Count' WHERE \"id\" = 3;" +
                          "UPDATE charts SET \"caption\" = 'Accounts by Industry', x_axis_name='Industry', y_axis_name='Account Count' WHERE \"id\" = 1;" +
                          "UPDATE charts SET \"caption\" = 'Opportunity by Stage', x_axis_name='Stage', y_axis_name='Opportunity Count' WHERE \"id\" = 2;" +
                          "UPDATE charts SET \"caption\" = 'Current Sales Orders', x_axis_name='Type', y_axis_name='Sales Order Count' WHERE \"id\" = 3;" +
                          "UPDATE charts SET \"caption\" = 'Leads by Status', x_axis_name='Status', y_axis_name='Lead Count' WHERE \"id\" = 4;" +
                          "UPDATE charts SET \"caption\" = 'Quotes by Valid Date', x_axis_name='Valid Date', y_axis_name='Quote Count' WHERE \"id\" = 5;";
                    break;
                case 2:
                    sql = "UPDATE reports SET \"name\" = 'Accounts by Industry' WHERE \"id\" = 1;" +
                          "UPDATE reports SET \"name\" = 'Current Accounts by Transaction Type' WHERE \"id\" = 2;" +
                          "UPDATE reports SET \"name\" = 'Current Sales Orders' WHERE \"id\" = 3;" +
                          "UPDATE reports SET \"name\" = 'Total Account Count' WHERE \"id\" = 4;" +
                          "UPDATE reports SET \"name\" = 'Total Supplier Count' WHERE \"id\" = 5;" +
                          "UPDATE reports SET \"name\" = 'Total Quote Count' WHERE \"id\" = 6;" +
                          "UPDATE reports SET \"name\" = 'Total Sales Order Count' WHERE \"id\" = 7;" +
                          "UPDATE reports SET \"name\" = 'Quotes by Stages' WHERE \"id\" = 8;" +
                          "UPDATE reports SET \"name\" = 'Quotes by Valid Date' WHERE \"id\" = 9;" +
                          "UPDATE widgets SET \"name\" = 'Total Account Count' WHERE \"id\" = 1;" +
                          "UPDATE widgets SET \"name\" = 'Total Supplier Count' WHERE \"id\" = 2;" +
                          "UPDATE widgets SET \"name\" = 'Total Quote Count' WHERE \"id\" = 4;" +
                          "UPDATE widgets SET \"name\" = 'Total Sales Count' WHERE \"id\" = 3;" +
                          "UPDATE charts SET \"caption\" = 'Accounts by Industry', x_axis_name='Industry', y_axis_name='Account Count' WHERE \"id\" = 1;" +
                          "UPDATE charts SET \"caption\" = 'Current Accounts by Transaction Type', x_axis_name='Stage', y_axis_name='Opportunity Count' WHERE \"id\" = 2;" +
                          "UPDATE charts SET \"caption\" = 'Current Sales Orders', x_axis_name='Type', y_axis_name='Sales Order Count' WHERE \"id\" = 3;" +
                          "UPDATE charts SET \"caption\" = 'Quotes by Stages', x_axis_name='Status', y_axis_name='Lead Count' WHERE \"id\" = 4;" +
                          "UPDATE charts SET \"caption\" = 'Quotes by Valid Date', x_axis_name='Valid Date', y_axis_name='Quote Count' WHERE \"id\" = 5;";
                    break;
            }


            return sql;
        }

        public static string GenerateSampleDataSql(int tenantId, string tenantLanguage, int appId)
        {
            var now = DateTime.UtcNow;
            var date1 = now.AddDays(1);
            var date2 = now.AddDays(2);
            var tenantValues = "'" + tenantId + "', '" + tenantId + "', '" + tenantId + "', '" + now.ToString("yyyy-M-dd hh:mm:ss") + "', '" + now.ToString("yyyy-M-dd hh:mm:ss") + "'";
            var tenantValues2 = "'" + tenantId + "', '" + tenantId + "', '" + now.ToString("yyyy-M-dd hh:mm:ss") + "', '" + now.ToString("yyyy-M-dd hh:mm:ss") + "'";
            var eventStartDate1 = new DateTime(date1.Year, date1.Month, date1.Day, 6, 0, 0).ToString("yyyy-M-dd hh:mm:ss");
            var eventEndDate1 = new DateTime(date1.Year, date1.Month, date1.Day, 9, 0, 0).ToString("yyyy-M-dd hh:mm:ss");
            var eventStartDate2 = new DateTime(date2.Year, date2.Month, date2.Day, 11, 0, 0).ToString("yyyy-M-dd hh:mm:ss");
            var eventEndDate2 = new DateTime(date2.Year, date2.Month, date2.Day, 14, 0, 0).ToString("yyyy-M-dd hh:mm:ss");
            var sql = "";

            switch (appId)
            {
                case 1:
                    if (tenantLanguage == "tr")
                    {
                        sql = $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}1', 'TOLNAK SAN. VE TİC. A.Ş.', 'Müşteri', 'Satış ve Pazarlama', '$32,000,000.00', '51', '02164344640', '05322715962', '02164293704', 'info@tolnak.com.tr', 'http://www.tolnak.com.tr', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', '2014 yılından beri çalışmıyoruz, tekrar canlandırmak gerekiyor.', 'true', {tenantValues}, '-10000');\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}2', 'DELSAN TURİZM GIDA PAZARLAMA LTD.', 'Diğer', 'Satış ve Pazarlama', '$38,000,000.00', '60', '02423557148', '05074171158', '02424552321', 'info@delsan.com.tr', 'http://www.delsan.com.tr', 'Yeni Mh. Ateş Cd. No:147/114', 'Antalya', 'Antalya', '07586', 'Türkiye', 'Yeni Mh. Ateş Cd. No:147/114', 'Antalya', 'Antalya', '07586', 'Türkiye', 'Yeni çalışmaya başladık; sağlam referanslara sahip.', 'true', {tenantValues}, '2000');\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}3', 'BEYAZ KİMYA TİC. LTD. ŞTİ.', 'İş Ortağı', 'Kimya, Petrol, Lastik ve Plastik', '$12,000,000.00', '25', '02162842912', '05444611935', '02164355689', 'info@beyazkim.com.tr', 'http://www.beyazkim.com.tr', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', '2014 yılından beri çalışmıyoruz, tekrar canlandırmak gerekiyor.', 'true', {tenantValues}, '-2000');\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}4', 'PARK REKLAMCILIK LTD. ŞTİ.', 'Müşteri', 'Satış ve Pazarlama', '$32,000,000.00', '120', '02629749408', '05079547169', '02629384121', 'info@parkrek.com.tr', 'http://www.parkrek.com.tr', 'Berat Mh. Gelincik Cd. No:36/120', 'İzmit', 'Kocaeli', '41688', 'Türkiye', 'Berat Mh. Gelincik Cd. No:36/120', 'İzmit', 'Kocaeli', '41688', 'Türkiye', 'Yeni çalışmaya başladık; sağlam referanslara sahip.', 'true', {tenantValues}, '10000');\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}5', 'ULTRAS ÜRETİM TİC. SAN. A.Ş.', 'Müşteri', 'Üretim', '$25,000,000.00', '138', '03124527131', '05072873352', '03122639126', 'info@ultra.com.tr', 'http://www.ultra.com.tr', 'İstiklal Mh. Gül Cd. No:57/115', 'Etimesgut', 'Ankara', '06756', 'Türkiye', 'İstiklal Mh. Gül Cd. No:57/115', 'Etimesgut', 'Ankara', '06756', 'Türkiye', 'Güvenilir bir firma, iyi referansları var; 2015 yılında birlikte birkaç proje gerçekleştirdik.', 'true', {tenantValues}, '-1000');\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'AYŞE', 'DORUK', 'AYŞE DORUK', 'Bayan', 'Yönetici', '1974-10-16', 'Fuar', '02423557148', '05074256209', 'ayse.doruk@delsan.com.tr', 'http://www.delsan.com.tr', 'Yeni Mh. Ateş Cd. No:147/114', 'Antalya', 'Antalya', '07586', 'Türkiye', 'Fuar', '{tenantId}1', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'RÜSTEM', 'GÜZEL', 'RÜSTEM GÜZEL', 'Bay', 'Genel Müdür', '1973-10-18', 'Reklam', '02623749408', '05323538796', 'rustem.guzel@park.com.tr', 'http://www.park.com.tr', 'Berat Mh. Gelincik Cd. No:36/120', 'İzmit', 'Kocaeli', '41688', 'Türkiye', 'Reklam', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'İHSAN', 'ÖZDEMİR', 'İHSAN ÖZDEMİR', 'Bay', 'Yönetici', '1970-03-20', 'Fuar', '02423557148', '05333413975', 'ihsan.ozdemir@delsan.com.tr', 'http://www.delsan.com.tr', 'Yeni Mh. Ateş Cd. No:147/114', 'Antalya', 'Antalya', '07586', 'Türkiye', 'Fuar', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'TÜRKER', 'EKİN', 'TÜRKER EKİN', 'Bayan', 'Uzman', '1975-10-12', 'Eposta', '02162842912', '05352122722', 'turker.ekin@beyazkim.com.tr', 'http://www.beyazkim.com.tr', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', 'Eposta', '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'ESEN', 'KARABIYIK', 'ESEN KARABIYIK', 'Bayan', 'Yönetici', '1982-11-12', 'Reklam', '02164344640', '05355024445', 'esen.karabiyik@tolnak.com.tr', 'http://www.tolnak.com.tr', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', 'Reklam', '{tenantId}5', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'HAKAN', 'DOĞRUER', 'HAKAN DOĞRUER', 'Bay', 'HAKAN TEKSTİL', 'Firma Sahibi', 'Çalışan Referansı', 'İletişim Bekliyor', 'Tekstil, Hazır Giyim, Deri', '$30,000,000.00', '120', '02125943321', '05324829193', '02125943322', 'hdogruer@hakanteks.com.tr', 'http://www.hakanteks.com.tr', 'Erenler Mh. Çağdaş Sk. No:122/93', 'Bahçelievler', 'İstanbul', '34691', 'Türkiye', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'AHMET', 'DAKİK', 'AHMET DAKİK', 'Bay', 'DAKİK TURİZM LTD. ŞTİ.', 'Genel Müdür', 'Fuar', 'İletişime Geçildi', 'Turizm, Konaklama', '$1,200,000.00', '40', '02124956644', '05324859911', '02124956644', 'adakik@dakiktur.com.tr', 'http://www.dakiktur.com.tr', 'Hacışerif Mh. Konak Sk. No:136/1', 'Sarıyer', 'İstanbul', '34500', 'Türkiye', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'ERKAN', 'DOĞRU', 'ERKAN DOĞRU', 'Bay', 'DOĞRU OTOMOTİV', 'Genel Müdür', 'Fuar', 'İlgileniyor', 'Otomotiv', '$9,000,000.00', '12', '0224405944', '0532483119', '0224405944', 'erkandogru@dogruoto.com.tr', 'http://www.dogruoto.com.tr', 'Bursa OSB, 3.Cad. No:542', 'Nilüfer', 'Bursa', '16511', 'Türkiye', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'BETÜL', 'POYRAZ', 'BETÜL POYRAZ', 'Bayan', 'POYRAZ ALÜMİNYUM', 'Firma Sahibi', 'Diğer', 'İlgileniyor', 'Üretim', '$55,000,000.00', '45', '02129554455', '05435543322', '02129554452', 'betul@poyrazaluminyum.com', 'http://www.poyrazaluminyum.com', 'İstikbal Mh. Camii sk. No:67/8', 'Bağcılar', 'İstanbul', '34532', 'Türkiye', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'ERMAN', 'SARIER', 'ERMAN SARIER', 'Bay', 'SARIER HUKUK', 'Avukat', 'Diğer', 'İletişime Geçildi', 'Hukuk Firmaları', '$1,000,000.00', '7', '02126948833', '05324881928', '02126948831', 'erman@sarierhukuk.av.tr', 'http://www.sarierhukuk.av.tr', 'Ortaklar Cd. No:55 Mecidiyeköy', 'Şişli', 'İstanbul', '34549', 'Türkiye', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Mobilya Değişimi', 'Yeni', '$65,000.00', '2016-05-17', 'Analiz Gerekiyor', '20', '$13,000.00', 'open', 'pipeline', '2016', '5', '2', '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Tolnak Ofis Revizyon', 'Yeni', '$60,000.00', '2016-02-08', 'Kazanıldı', '100', '$60,000.00', 'closed_won', 'closed', '2016', '2', '1', '{tenantId}1', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Ofis Kaplama', 'Mevcut', '$25,000.00', '2016-02-17', 'Rekabet Nedeniyle Kaybedildi', '100', NULL, 'omitted', 'omitted', '2016', '2', '1', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Dekor Yenileme', 'Mevcut', '$20,000.00', '2016-05-03', 'Fiyat Teklifi', '75', '$15,000.00', 'open', 'pipeline', '2016', '5', '2', '{tenantId}5', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Yeni Ofis Mobilyası Prj', 'Yeni', '$40,000.00', '2016-05-01', 'Analiz Gerekiyor', '20', '$8,000.00', 'open', 'pipeline', '2016', '5', '2', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Etkinlik', 'İş ortaklarıyla birlikte yemek', 'Firma', '{tenantId}5', NULL, NULL, NULL, NULL, 'Ankara', '{eventStartDate1}', '{eventEndDate1}', 'Hayır', NULL, NULL, NULL, NULL, NULL, NULL, 'event', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Arama', 'Ayşe Hanım destek talebi araması', 'Kişi', '{tenantId}1', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Gelen Arama', 'Destek', 'Tamamlanmış Arama', '2016-12-16 12:16:53', '45', NULL, 'call', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Arama', 'Mali tablolar ile ilgili sorulara cevap alalım', 'Kişi', '{tenantId}3', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Giden Arama', 'Yönetimsel', 'Yeni Arama', '2016-12-16 12:16:53', NULL, 'Proje kapatılacak', 'call', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Görev', 'Park Reklamcılık eksik evrakların tamamlanması', 'Firma', '{tenantId}4', '{eventStartDate1}', 'Başlanmadı', 'Normal', 'Hayır', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'task', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Etkinlik', 'Beyaz Kimya Proje Demosu', 'Firma', '{tenantId}3', NULL, NULL, NULL, NULL, 'Sancaktepe', '{eventStartDate2}', '{eventEndDate2}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'event', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\",\"purchase_price\", \"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Siyah Toplantı Masası + 6 Koltuk', 'STM13049', 'Adet', '$7,000.00','$3,000.00', '18', '5', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\",\"purchase_price\", \"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Siyah Deri Ofis Mobilya Takımı', 'SYO16301', 'Adet', '$12,000.00','$9,000.00', '18', '10', '1 ofis masası, 1 müdür koltuğu, 1 sehpa ve 2 adet misafir koltuğundan oluşan siyah deri ofis mobilya takımı', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\",\"purchase_price\", \"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Ofis Dekor Yenileme Hizmeti', 'ODY20441', 'Adet', '$2,200.00','$1,900.00', '18', NULL, 'Ofis dekoru revizyonu için kumaş ve yenileme hizmetleri', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"purchase_price\",\"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Kahve Kumaş Ofis Mobilya Takımı', 'KKO16493', 'Adet', '$9,000.00','$5,000.00', '18', '8', '1 ofis masası, 1 müdür koltuğu, 1 sehpa ve 2 adet misafir koltuğundan oluşan kahve kumaş ofis mobilya takımı', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"purchase_price\",\"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Dekorasyon ve Kurulum Hizmeti', 'DKH50341', 'Adet', '$2,000.00','$2,900.00', '18', NULL, 'Verilen dekorasyon ürünleri için kurulum bedeli', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Tolnak Yönetim Ofisleri Revizyonu', 'Onaylandı', '2016-05-30', '$50,000.00', '$9,000.00', '$59,000.00', 'Ödeme vadesi 15 gündür. Teslimat, firma merkez adresine yapılacaktır.', '18;9000', 'percent', '{tenantId}1', '{tenantId}5', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Beyaz Kimya Ofis Mobilya Kaplamaları', 'Reddedildi', '2016-05-08', '$34,000.00', '$6,120.00', '$40,120.00', 'Ödeme vadesi 15 gündür. Teslimat, firma merkez adresine yapılacaktır.', '18;6120', 'percent', '{tenantId}3', '{tenantId}4', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Park Reklam Mobilya Değişimi', 'Taslak', '2016-05-30', '$31,800.00', '$5,724.00', '$37,524.00', 'Ödeme vadesi 15 gündür. Teslimat, firma merkez adresine yapılacaktır.', '18;5724', 'percent', '{tenantId}4', '{tenantId}2', '{tenantId}1', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Delsan Turizm Ofis Yenileme', 'Taslak', '2016-06-02', '$48,000.00', '$8,640.00', '$56,640.00', 'Ödeme vadesi 15 gündür. Teslimat adresi firma merkezidir.', '18;8640', 'percent', '{tenantId}2', '{tenantId}3', '{tenantId}5', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Ultra Üretim Dekor Yenileme', 'Gönderildi', '2016-06-03', '$12,000.00', '$2,160.00', '$14,160.00', '- Ödeme vadesi 15 gündür. Teslimat, firma merkez adresine yapılacaktır.', '18;2160', 'percent', '{tenantId}5', NULL, '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Beyaz Kimya Ofis Mobilyası Siparişi', 'İşlemde', '2016-06-28', '$3,231.00', '$21,181.00', '$17,950.00', '18;3231', 'percent', '{tenantId}3', '{tenantId}4', NULL, NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Tolnak Mobilya Siparişi', 'Onaylandı', '2016-05-30', '$5,976.00', '$39,176.00', '$33,200.00', '18;5976', 'percent', '{tenantId}1', '{tenantId}5', '{tenantId}2', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Park Reklam Mobilya Değişimi', 'Beklemede', '2016-06-05', '$6,102.00', '$40,002.00', '$33,900.00', '18;6102', 'percent', '{tenantId}4', '{tenantId}2', NULL, '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', '4', 'Adet', '$9,000.00', '$36,000.00', 'percent', '{tenantId}2', '{tenantId}4', '{tenantId}4', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', '1', 'Adet', '$7,000.00', '$7,000.00', 'percent', '{tenantId}2', '{tenantId}2', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', '2', 'Adet', '$7,000.00', '$14,000.00', 'percent', '{tenantId}2', '{tenantId}1', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', '10', 'Adet', '$1,200.00', '$12,000.00', 'percent', '{tenantId}1', '{tenantId}5', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', '2', 'Adet', '$12,000.00', '$24,000.00', 'percent', '{tenantId}1', '{tenantId}2', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}6', '3', 'Adet', '$9,000.00', '$27,000.00', 'percent', '{tenantId}1', '{tenantId}3', '{tenantId}4', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}7', '3', 'Adet', '$12,000.00', '$36,000.00', 'percent', '{tenantId}1', '{tenantId}1', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}8', '10', 'Adet', '$1,200.00', '$12,000.00', 'percent', '{tenantId}1', '{tenantId}4', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}9', '3', 'Adet', '$1,000.00', '$3,000.00', 'percent', '{tenantId}3', '{tenantId}2', '{tenantId}5', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}10', '4', 'Adet', '$1,200.00', '$4,800.00', 'percent', '{tenantId}2', '{tenantId}3', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', '1', 'Adet', '$1,000.00', '15', 'percent', '$850.00', '{tenantId}2', '{tenantId}1', '{tenantId}5', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', '3', 'Adet', '$9,000.00', '10', 'percent', '$24,300.00', '{tenantId}1', '{tenantId}3', '{tenantId}4', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', '1', 'Adet', '$1,000.00', '20', 'percent', '$800.00', '{tenantId}2', '{tenantId}2', '{tenantId}5', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', '3', 'Adet', '$12,000.00', '10', 'percent', '$32,400.00', '{tenantId}1', '{tenantId}2', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', '2', 'Adet', '$9,000.00', '5', 'percent', '$17,100.00', '{tenantId}1', '{tenantId}1', '{tenantId}4', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}6', '10', 'Adet', '$1,200.00', '20', 'percent', '$9,600.00', '{tenantId}2', '{tenantId}3', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"suppliers_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}3', 'MAVİBEYAZ BİLGİSAYAR LTD.', 'Bilişim Teknolojileri', '$5,000,000.00', '25', '02164344640', '05322715962', '02164293704', 'info@mavibeyazbilgisayar.com', 'www.mavibeyazbilgisayar.com', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', '2010 yılından bu yana çalışıyoruz', NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, '20000');" +
                              $"INSERT INTO \"public\".\"suppliers_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}4', 'MOBİLYA ÜRETİM TİC. SAN. A.Ş.', 'Ağaç İşleri, Kağıt ve Kağıt Ürünleri', '$20,000,000.00', '60', '03124527131', '05072873352', '03122639126', 'uretim@mobilyauretimas.com.tr', 'www.mobilyauretimas.com.tr', 'İstiklal Mh. Gül Cd. No:57/115', 'Etimesgut', 'Ankara', '06756', 'Türkiye', 'İstiklal Mh. Gül Cd. No:57/115', 'Etimesgut', 'Ankara', '06756', 'Türkiye', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, '0');" +
                              $"INSERT INTO \"public\".\"suppliers_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}5', 'KİMYANET TİC. LTD. ŞTİ.','Kimya, Petrol, Lastik ve Plastik', '$2,000,000.00', '15', '02162842912', '05444611935', '02164355689', 'bilgi@kimyanettic.com', 'www.kimyanettic.com', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, '-3000');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}46', '2017-05-10', '12000', NULL, 'YTU5454AC', 'Alış Faturası', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}5', 'purchase_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}47', '2017-06-28', '5000', NULL, 'GRS4545TR', 'Alış Faturası', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}5', 'purchase_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}48', '2017-07-08', '14000', 'Nakit', NULL, 'Ödeme', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}5', 'payment');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}49', '2017-06-23', '50000', NULL, 'CDF3434HJN', 'Alış Faturası', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}4', 'purchase_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}50', '2017-07-06', '50000', 'Çek', NULL, 'Ödeme', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', '2017-07-07', '{tenantId}4', 'payment');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}51', '2017-07-04', '120000', NULL, 'NBM335B45', 'Alış Faturası', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}3', 'purchase_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}52', '2017-07-08', '120000', 'Banka Transferi', NULL, 'Ödeme', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}3', 'payment');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}53', '2017-07-09', '20000', 'Banka Transferi', NULL, 'Ödeme', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}3', 'payment');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}54', '2017-06-07', '2000', 'Nakit', NULL, 'Tahsilat', NULL, '{tenantId}5', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}55', '2017-07-07', '2000', 'Banka Transferi', NULL, 'Tahsilat', NULL, '{tenantId}5', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}56', '2017-07-09', '5000', NULL, 'VBD4545H56', 'Satış Faturası', NULL, '{tenantId}5', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}57', '2017-06-06', '18000', 'Banka Transferi', NULL, 'Tahsilat', NULL, '{tenantId}4', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}58', '2017-07-04', '8000', NULL, 'VDF24H456', 'Satış Faturası', NULL, '{tenantId}4', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}59', '2017-07-06', '5000', NULL, 'VBF4546GH78', 'Satış Faturası', NULL, '{tenantId}2', '{tenantId}1', NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}60', '2017-07-06', '2000', NULL, 'BVG345FG454', 'Satış Faturası', NULL, '{tenantId}3', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}61', '2017-07-10', '3000', NULL, 'VBG657G67', 'Satış Faturası', NULL, '{tenantId}2', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}62', '2017-07-10', '80000', NULL, 'VBF345VB3', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}63', '2017-07-11', '20000', NULL, 'VBF6764F33', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}64', '2017-07-09', '30000', NULL, 'VBG45436N8', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}65', '2017-06-28', '10000', NULL, 'VBH567HY78', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}66', '2017-07-12', '3000', NULL, 'VBG45656GB', 'Satış Faturası', NULL, '{tenantId}3', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}67', '2017-07-06', '1000', NULL, 'VBF546546Y78', 'Satış Faturası', NULL, '{tenantId}3', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}68', '2017-07-09', '4000', 'Nakit', NULL, 'Tahsilat', NULL, '{tenantId}3', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}69', '2017-07-21', '10000', NULL, 'HYF4546GF', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}70', '2017-07-10', '120000', 'Çek', NULL, 'Tahsilat', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', '2017-07-12', NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}71', '2017-07-10', '20000', 'Nakit', NULL, 'Tahsilat', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}72', '2017-07-10', '10000', 'Banka Transferi', NULL, 'Tahsilat', NULL, '{tenantId}2', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"templates\" (\"id\", \"template_type\", \"name\", \"content\", \"language\", \"active\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"module\", \"subject\", \"sharing_type\") VALUES ('1', '3', 'Fiyat Teklifi', 'quote-template-tr.docx', '2', 't', '{tenantId}', NULL, '2017-02-16 16:48:31.814936', NULL, false, 'quotes', NULL, '0');\n";
                    }
                    else
                    {
                        sql = $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Yadel', 'Other', 'Energy', '$75,000,000.00', '19', '742 220 0483', '116 437 3315', '880 333 3789', 'krogersd0@tumblr.com', 'http://www.yadel.com', '369 Towne Crossing', 'Kansas', 'Shawnee Mission', '22650', 'United States', '369 Towne Crossing', 'Kansas', 'Shawnee Mission', '22650', 'United States', 'non velit nec nisi vulputate nonummy maecenas tincidunt lacus at velit vivamus vel nulla eget eros elementum', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Jetwire', 'Business Partner', 'Government', '$41,000,000.00', '57', '388 608 0694', '000 301 6203', '364 269 9153', 'tturnerct@bloglines.com', 'http://www.jetwire.com', '3 Hoffman Terrace', 'North Carolina', 'Winston Salem', '42136', 'United States', '3 Hoffman Terrace', 'North Carolina', 'Winston Salem', '42136', 'United States', 'neque aenean auctor gravida sem praesent id massa id nisl venenatis lacinia aenean sit amet justo morbi ut', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Fanoodle', 'Other', 'Energy', '$82,000,000.00', '50', '430 629 0030', '278 649 7701', '213 028 4958', 'kcarpenterf67@histats.com', 'http://www.fanoodle.com', '7926 Golf Course Avenue', 'Colorado', 'Colorado Springs', '60169', 'United States', '7926 Golf Course Avenue', 'Colorado', 'Colorado Springs', '60169', 'United States', 'neque aenean auctor gravida sem praesent id massa id nisl', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Jazzy', 'Other', 'Automotive', '$43,000,000.00', '68', '617 715 8170', '096 345 6427', '897 540 6174', 'jadams9u@pbs.org', 'http://www.jazzy.com', '0451 Schiller Terrace', 'Maryland', 'Annapolis', '84163', 'United States', '0451 Schiller Terrace', 'Maryland', 'Annapolis', '84163', 'United States', 'justo maecenas rhoncus aliquam lacus morbi quis tortor id nulla ultrices aliquet', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"account_type\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Dabfeed Automotive', 'Other', 'Automotive', '$9,000,000.00', '28', '746 852 2410', '910 388 0034', '845 208 5403', 'efrazierbw@stumbleupon.com', 'http://www.dabfeed.com', '57293 Maryland Drive', 'Louisiana', 'Baton Rouge', '74660', 'United States', '57293 Maryland Drive', 'Louisiana', 'Baton Rouge', '74660', 'United States', 'justo nec condimentum neque sapien placerat ante nulla justo aliquam quis turpis eget elit sodales scelerisque', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Cynthia', 'Dunn', 'Cynthia Dunn', 'Mrs. / Ms.', 'Computer Systems Analyst II', 'HR', '1978-11-11', 'Web Site', '542 341 4862', '940 377 9525', 'cdunn6p@cmu.edu', '34 Summer Ridge Place', 'Washington', 'Seattle', '35761', 'United States', 'tempus semper est quam pharetra magna ac consequat metus sapien ut nunc vestibulum ante', '{tenantId}1', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Annie', 'Hunter', 'Annie Hunter', 'Mrs. / Ms.', 'Geologist I', 'HR', '1974-07-20', 'Email', '706 794 2148', '355 729 8714', 'ahunterbi@guardian.co.uk', '80 2nd Parkway', 'Michigan', 'Detroit', '97678', 'United States', 'at dolor quis odio consequat varius integer ac leo pellentesque ultrices mattis', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Stephen', 'Hughes', 'Stephen Hughes', 'Mrs. / Ms.', 'Technical Writer', 'HR', '1976-09-06', 'Partner', '957 241 3173', '993 166 7697', 'shughes69@sohu.com', '94 Little Fleur Hill', 'Arizona', 'Phoenix', '27', 'United States', 'ipsum aliquam non mauris morbi non lectus aliquam sit amet', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Lisa', 'Barnes', 'Lisa Barnes', 'Mrs. / Ms.', 'Graphic Designer', 'HR', '1974-05-14', 'Web Site', '956 101 0714', '505 897 7570', 'lbarnes22@ft.com', '4527 Ludington Avenue', 'Texas', 'Amarillo', '5579', 'United States', 'pretium nisl ut volutpat sapien arcu sed augue aliquam erat volutpat in', '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Bruce', 'Stephens', 'Bruce Stephens', 'Dr.', 'Accountant I', 'HR', '1978-03-29', 'Web Site', '776 488 9673', '184 191 2638', 'bstephensdl@arstechnica.com', '6 Rowland Pass', 'Wisconsin', 'Milwaukee', '13043', 'United States', 'rhoncus dui vel sem sed sagittis nam congue risus semper porta volutpat quam pede lobortis', '{tenantId}5', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Kelly', 'Gray', 'Kelly Gray', 'Mrs. / Ms.', 'Thoughtmix', 'Human Resources Assistant I', 'Email', 'Contact in Future', 'Electronics', '$56,000,000.00', '88', '+66 510 233 8149', '+43 179 071 6728', '+22 456 881 5856', 'kgray3p@zimbio.com', 'www.thoughtmix.com', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Sara', 'Daniels', 'Sara Daniels', 'Dr.', 'Feedfish', 'Technical Writer', 'Seminar', 'Lost Lead', 'Education', '$14,000,000.00', '14', '+38 178 367 6319', '+82 194 493 7391', '+83 330 697 8945', 'sdanielst@devhub.com', 'www.feedfish.com', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Keith', 'Bailey', 'Keith Bailey', 'Dr.', 'Izio', 'Senior Cost Accountant', 'Web Site', 'Contact in Future', 'Automotive', '$79,000,000.00', '8', '+89 753 461 7214', '+79 831 392 8697', '+98 618 240 0558', 'kbailey9y@godaddy.com', 'www.izio.com', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Gary', 'Wagner', 'Gary Wagner', 'Mr.', 'Pixoboo', 'Statistician I', 'Web Site', 'Contact in Future', 'Automotive', '$42,000,000.00', '19', '+32 809 940 6193', '+99 138 753 7508', '+85 151 602 0547', 'gwagner16@shutterfly.com', 'www.pixoboo.com', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"leads_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"company\", \"job_title\", \"lead_source\", \"lead_status\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Ralph', 'Cook', 'Ralph Cook', 'Mr.', 'Quimba', 'Clinical Specialist', 'Web Site', 'Lost Lead', 'Automotive', '$27,000,000.00', '42', '+29 117 792 0562', '+51 059 286 8685', '+16 423 795 4251', 'rcook6o@blogger.com', 'www.quimba.com', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"description\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"next_step\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Construction Project', 'New', '$740,000.00', '2016-02-11', 'Needs Analysis', '20', '$148,000.00', 'iaculis diam erat fermentum justo nec condimentum neque sapien placerat ante nulla justo aliquam quis turpis', 'open', 'pipeline', '2016', '2', '1', '{tenantId}3', 'Presentation', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"description\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"next_step\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Management Consulting', 'Existing', '$400,000.00', '2016-03-24', 'Negotiation/Review', '50', '$200,000.00', 'sed augue aliquam erat volutpat in congue etiam justo etiam pretium iaculis justo in hac habitasse platea dictumst', 'open', 'pipeline', '2016', '3', '1', '{tenantId}2', 'Follow up', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"description\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"next_step\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Construction Project', 'New', '$220,000.00', '2016-08-01', 'Negotiation/Review', '40', '$88,000.00', 'aliquam erat volutpat in congue etiam justo etiam pretium iaculis justo in hac habitasse platea dictumst etiam', 'open', 'pipeline', '2016', '8', '3', '{tenantId}4', 'Follow up', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"description\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"next_step\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Management Consulting', 'New', '$360,000.00', '2016-01-10', 'Negotiation/Review', '10', '$36,000.00', 'semper est quam pharetra magna ac consequat metus sapien ut nunc vestibulum ante ipsum primis in faucibu', 'open', 'pipeline', '2016', '1', '1', '{tenantId}1', 'Proposal', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"opportunities_d\" (\"id\", \"name\", \"opportunity_type\", \"amount\", \"closing_date\", \"stage\", \"probability\", \"expected_revenue\", \"description\", \"forecast_type\", \"forecast_category\", \"forecast_year\", \"forecast_month\", \"forecast_quarter\", \"account\", \"next_step\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Operator Training', 'Existing', '$230,000.00', '2016-01-11', 'Qualification', '75', '$172,500.00', 'risus auctor sed tristique in tempus sit amet sem fusce consequat nulla nisl nunc nisl duis', 'open', 'pipeline', '2016', '1', '1', '{tenantId}5', 'Follow up', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Event', 'Dinner with Lisa & Team', 'Contact', '4', NULL, NULL, NULL, 'John''s Coffee', '{eventStartDate1}', '{eventEndDate1}', 'No', NULL, NULL, NULL, NULL, NULL, 'event', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Call', 'Call Stephen for meeting', 'Contact', '3', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Outbound', 'Demo', 'Scheduled Call', '2016-12-23 13:18:02', NULL, 'call', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Event', 'Meet Ralph for Demo', 'Lead', '5', NULL, NULL, NULL, 'Foster''s Co.', '{eventStartDate2}', '{eventEndDate2}', 'Yes', NULL, NULL, NULL, NULL, NULL, 'event', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Task', 'Follow-up Keith', 'Lead', '3', '2016-12-23', 'Not Started', 'Low', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'task', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Call', 'Call Gary for quote', 'Lead', '4', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Outbound', 'Prospecting', 'Current Call', '2016-12-02 13:18:02', 'Ask Gary if he''s confirmed to get our latest quote', 'call', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"purchase_price\", \"vat_percent\", \"stock_quantity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Consulting Service', 'CS10268', 'Quantity', '$800.00', '$500.00','10', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"purchase_price\", \"vat_percent\", \"stock_quantity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Training Service', 'TS10214', 'Quantity', '$700.00', '$400.00','10', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"purchase_price\", \"vat_percent\", \"stock_quantity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Construction Starter Kit', 'CSK10239', 'Quantity', '$6,000.00','$5,000.00', '10', '12', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Yadel Management Consulting', 'Draft', '2016-05-25', '$226,000.00', '$22,600.00', '$248,600.00', 'Payment due on delivery. All payments must be made in US currency', '10;22600', 'percent', '{tenantId}1', '{tenantId}3', '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Jazzy Construction Project', 'Draft', '2016-06-03', '$190,000.00', '$19,000.00', '$209,000.00', 'Payment due on delivery. All payments must be made in US currency', '10;19000', 'percent', '{tenantId}4', '{tenantId}4', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Dabfeed Training Service', 'Negotiation', '2016-05-31', '$187,000.00', '$18,700.00', '$205,700.00', 'Payment due on delivery. All payments must be made in US currency', '10;18700', 'percent', '{tenantId}5', '{tenantId}1', '{tenantId}5', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Dabfeed Training Service', 'In Progress', NULL, '$18,700.00', '$205,700.00', '$187,000.00', '10;18700', 'percent', '{tenantId}5', '{tenantId}1', '{tenantId}5', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Jazzy Construction Project', 'On Hold', '2016-05-17', '$17,730.00', '$195,030.00', '$177,300.00', '10;17730', 'percent', '{tenantId}4', '{tenantId}4', '{tenantId}3', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"opportunity\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Fanoodle New Starter Kit Sales Order', 'In Progress', '2016-06-16', '$6,840.00', '$75,240.00', '$68,400.00', '10;6840', 'percent', '{tenantId}3', '{tenantId}2', NULL, NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', '20', 'Quantity', '$800.00', '$16,000.00', 'percent', '{tenantId}1', '{tenantId}1', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', '35', 'Quantity', '$6,000.00', '$210,000.00', 'percent', '{tenantId}2', '{tenantId}1', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', '21', 'Quantity', '$6,000.00', '$126,000.00', 'percent', '{tenantId}1', '{tenantId}2', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', '23', 'Quantity', '$6,000.00', '$138,000.00', 'percent', '{tenantId}2', '{tenantId}3', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', '80', 'Quantity', '$800.00', '$64,000.00', 'percent', '{tenantId}2', '{tenantId}2', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}6', '70', 'Quantity', '$700.00', '$49,000.00', 'percent', '{tenantId}1', '{tenantId}3', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', '12', 'Quantity', '$6,000.00', '5', 'percent', '$68,400.00', '{tenantId}1', '{tenantId}3', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', '21', 'Quantity', '$6,000.00', '5', 'percent', '$119,700.00', '{tenantId}1', '{tenantId}2', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', '80', 'Quantity', '$800.00', '10', 'percent', '$57,600.00', '{tenantId}2', '{tenantId}2', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', '70', 'Quantity', '$700.00', NULL, 'percent', '$49,000.00', '{tenantId}1', '{tenantId}1', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', '23', 'Quantity', '$6,000.00', NULL, 'percent', '$138,000.00', '{tenantId}2', '{tenantId}1', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}6', '70', 'Quantity', '$700.00', NULL, 'percent', '$49,000.00', '{tenantId}1', '{tenantId}1', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}7', '23', 'Quantity', '$6,000.00', NULL, 'percent', '$138,000.00', '{tenantId}2', '{tenantId}1', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"templates\" (\"id\", \"template_type\", \"name\", \"content\", \"language\", \"active\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"module\", \"subject\", \"sharing_type\") VALUES ('1', '3', 'Quote Template', 'quote-template-en.docx', '1', 't', '{tenantId}', NULL, '2017-02-16 16:48:31.814936', NULL, false, 'quotes', NULL, '0');\n";
                    }

                    sql += "SELECT SETVAL('public.accounts_d_id_seq', COALESCE(MAX(id), 1)) FROM accounts_d;\n" +
                           "SELECT SETVAL('public.contacts_d_id_seq', COALESCE(MAX(id), 1)) FROM contacts_d;\n" +
                           "SELECT SETVAL('public.leads_d_id_seq', COALESCE(MAX(id), 1)) FROM leads_d;\n" +
                           "SELECT SETVAL('public.opportunities_d_id_seq', COALESCE(MAX(id), 1)) FROM opportunities_d;\n" +
                           "SELECT SETVAL('public.activities_d_id_seq', COALESCE(MAX(id), 1)) FROM activities_d;\n" +
                           "SELECT SETVAL('public.products_d_id_seq', COALESCE(MAX(id), 1)) FROM products_d;\n" +
                           "SELECT SETVAL('public.quotes_d_id_seq', COALESCE(MAX(id), 1)) FROM quotes_d;\n" +
                           "SELECT SETVAL('public.sales_orders_d_id_seq', COALESCE(MAX(id), 1)) FROM sales_orders_d;\n" +
                           "SELECT SETVAL('public.quote_products_d_id_seq', COALESCE(MAX(id), 1)) FROM quote_products_d;\n" +
                           "SELECT SETVAL('public.order_products_d_id_seq', COALESCE(MAX(id), 1)) FROM order_products_d;\n" +
                           "SELECT SETVAL('public.templates_id_seq', COALESCE(MAX(id), 1)) FROM templates;";
                    break;
                case 2:
                    if (tenantLanguage == "tr")
                    {
                        sql = $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}1', 'TOLNAK SAN. VE TİC. A.Ş.', 'Satış ve Pazarlama', '$32,000,000.00', '51', '02164344640', '05322715962', '02164293704', 'info@tolnak.com.tr', 'http://www.tolnak.com.tr', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', '2014 yılından beri çalışmıyoruz, tekrar canlandırmak gerekiyor.', 'true', {tenantValues}, '-10000');\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}2', 'DELSAN TURİZM GIDA PAZARLAMA LTD.', 'Satış ve Pazarlama', '$38,000,000.00', '60', '02423557148', '05074171158', '02424552321', 'info@delsan.com.tr', 'http://www.delsan.com.tr', 'Yeni Mh. Ateş Cd. No:147/114', 'Antalya', 'Antalya', '07586', 'Türkiye', 'Yeni Mh. Ateş Cd. No:147/114', 'Antalya', 'Antalya', '07586', 'Türkiye', 'Yeni çalışmaya başladık; sağlam referanslara sahip.', 'true', {tenantValues}, '2000');\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}3', 'BEYAZ KİMYA TİC. LTD. ŞTİ.', 'Kimya, Petrol, Lastik ve Plastik', '$12,000,000.00', '25', '02162842912', '05444611935', '02164355689', 'info@beyazkim.com.tr', 'http://www.beyazkim.com.tr', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', '2014 yılından beri çalışmıyoruz, tekrar canlandırmak gerekiyor.', 'true', {tenantValues}, '-2000');\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}4', 'PARK REKLAMCILIK LTD. ŞTİ.', 'Satış ve Pazarlama', '$32,000,000.00', '120', '02629749408', '05079547169', '02629384121', 'info@parkrek.com.tr', 'http://www.parkrek.com.tr', 'Berat Mh. Gelincik Cd. No:36/120', 'İzmit', 'Kocaeli', '41688', 'Türkiye', 'Berat Mh. Gelincik Cd. No:36/120', 'İzmit', 'Kocaeli', '41688', 'Türkiye', 'Yeni çalışmaya başladık; sağlam referanslara sahip.', 'true', {tenantValues}, '10000');\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}5', 'ULTRAS ÜRETİM TİC. SAN. A.Ş.', 'Üretim', '$25,000,000.00', '138', '03124527131', '05072873352', '03122639126', 'info@ultra.com.tr', 'http://www.ultra.com.tr', 'İstiklal Mh. Gül Cd. No:57/115', 'Etimesgut', 'Ankara', '06756', 'Türkiye', 'İstiklal Mh. Gül Cd. No:57/115', 'Etimesgut', 'Ankara', '06756', 'Türkiye', 'Güvenilir bir firma, iyi referansları var; 2015 yılında birlikte birkaç proje gerçekleştirdik.', 'true', {tenantValues}, '-1000');\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'AYŞE', 'DORUK', 'AYŞE DORUK', 'Bayan', 'Yönetici', '1974-10-16', 'Fuar', '02423557148', '05074256209', 'ayse.doruk@delsan.com.tr', 'http://www.delsan.com.tr', 'Yeni Mh. Ateş Cd. No:147/114', 'Antalya', 'Antalya', '07586', 'Türkiye', 'Fuar', '{tenantId}1', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'RÜSTEM', 'GÜZEL', 'RÜSTEM GÜZEL', 'Bay', 'Genel Müdür', '1973-10-18', 'Reklam', '02623749408', '05323538796', 'rustem.guzel@park.com.tr', 'http://www.park.com.tr', 'Berat Mh. Gelincik Cd. No:36/120', 'İzmit', 'Kocaeli', '41688', 'Türkiye', 'Reklam', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'İHSAN', 'ÖZDEMİR', 'İHSAN ÖZDEMİR', 'Bay', 'Yönetici', '1970-03-20', 'Fuar', '02423557148', '05333413975', 'ihsan.ozdemir@delsan.com.tr', 'http://www.delsan.com.tr', 'Yeni Mh. Ateş Cd. No:147/114', 'Antalya', 'Antalya', '07586', 'Türkiye', 'Fuar', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'TÜRKER', 'EKİN', 'TÜRKER EKİN', 'Bayan', 'Uzman', '1975-10-12', 'Eposta', '02162842912', '05352122722', 'turker.ekin@beyazkim.com.tr', 'http://www.beyazkim.com.tr', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', 'Eposta', '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"web\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'ESEN', 'KARABIYIK', 'ESEN KARABIYIK', 'Bayan', 'Yönetici', '1982-11-12', 'Reklam', '02164344640', '05355024445', 'esen.karabiyik@tolnak.com.tr', 'http://www.tolnak.com.tr', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', 'Reklam', '{tenantId}5', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Etkinlik', 'İş ortaklarıyla birlikte yemek', 'Firma', '{tenantId}5', NULL, NULL, NULL, NULL, 'Ankara', '{eventStartDate1}', '{eventEndDate1}', 'Hayır', NULL, NULL, NULL, NULL, NULL, NULL, 'event', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Arama', 'Ayşe Hanım destek talebi araması', 'Kişi', '{tenantId}1', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Gelen Arama', 'Destek', 'Tamamlanmış Arama', '2016-12-16 12:16:53', '45', NULL, 'call', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Arama', 'Mali tablolar ile ilgili sorulara cevap alalım', 'Kişi', '{tenantId}3', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Giden Arama', 'Yönetimsel', 'Yeni Arama', '2016-12-16 12:16:53', NULL, 'Proje kapatılacak', 'call', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Görev', 'Park Reklamcılık eksik evrakların tamamlanması', 'Firma', '{tenantId}4', '{eventStartDate1}', 'Başlanmadı', 'Normal', 'Hayır', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'task', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"task_notification\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"call_duration_minute\", \"call_result\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Etkinlik', 'Beyaz Kimya Proje Demosu', 'Firma', '{tenantId}3', NULL, NULL, NULL, NULL, 'Sancaktepe', '{eventStartDate2}', '{eventEndDate2}', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'event', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Siyah Toplantı Masası + 6 Koltuk', 'STM13049', 'Adet', '$7,000.00', '18', '5', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Siyah Deri Ofis Mobilya Takımı', 'SYO16301', 'Adet', '$12,000.00', '18', '10', '1 ofis masası, 1 müdür koltuğu, 1 sehpa ve 2 adet misafir koltuğundan oluşan siyah deri ofis mobilya takımı', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Ofis Dekor Yenileme Hizmeti', 'ODY20441', 'Adet', '$1,200.00', '18', NULL, 'Ofis dekoru revizyonu için kumaş ve yenileme hizmetleri', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Kahve Kumaş Ofis Mobilya Takımı', 'KKO16493', 'Adet', '$9,000.00', '18', '8', '1 ofis masası, 1 müdür koltuğu, 1 sehpa ve 2 adet misafir koltuğundan oluşan kahve kumaş ofis mobilya takımı', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"vat_percent\", \"stock_quantity\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Dekorasyon ve Kurulum Hizmeti', 'DKH50341', 'Adet', '$1,000.00', '18', NULL, 'Verilen dekorasyon ürünleri için kurulum bedeli', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Tolnak Yönetim Ofisleri Revizyonu', 'Onaylandı', '2016-05-30', '$50,000.00', '$9,000.00', '$59,000.00', 'Ödeme vadesi 15 gündür. Teslimat, firma merkez adresine yapılacaktır.', '18;9000', 'percent', '{tenantId}1', '{tenantId}5', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Beyaz Kimya Ofis Mobilya Kaplamaları', 'Reddedildi', '2016-05-08', '$34,000.00', '$6,120.00', '$40,120.00', 'Ödeme vadesi 15 gündür. Teslimat, firma merkez adresine yapılacaktır.', '18;6120', 'percent', '{tenantId}3', '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Park Reklam Mobilya Değişimi', 'Taslak', '2016-05-30', '$31,800.00', '$5,724.00', '$37,524.00', 'Ödeme vadesi 15 gündür. Teslimat, firma merkez adresine yapılacaktır.', '18;5724', 'percent', '{tenantId}4', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Delsan Turizm Ofis Yenileme', 'Taslak', '2016-06-02', '$48,000.00', '$8,640.00', '$56,640.00', 'Ödeme vadesi 15 gündür. Teslimat adresi firma merkezidir.', '18;8640', 'percent', '{tenantId}2', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Ultra Üretim Dekor Yenileme', 'Gönderildi', '2016-06-03', '$12,000.00', '$2,160.00', '$14,160.00', '- Ödeme vadesi 15 gündür. Teslimat, firma merkez adresine yapılacaktır.', '18;2160', 'percent', '{tenantId}5', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Beyaz Kimya Ofis Mobilyası Siparişi', 'İşlemde', '2016-06-28', '$3,231.00', '$21,181.00', '$17,950.00', '18;3231', 'percent', '{tenantId}3', '{tenantId}4', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Tolnak Mobilya Siparişi', 'Onaylandı', '2016-05-30', '$5,976.00', '$39,176.00', '$33,200.00', '18;5976', 'percent', '{tenantId}1', '{tenantId}5', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Park Reklam Mobilya Değişimi', 'Beklemede', '2016-06-05', '$6,102.00', '$40,002.00', '$33,900.00', '18;6102', 'percent', '{tenantId}4', '{tenantId}2', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', '4', 'Adet', '$9,000.00', '$36,000.00', 'percent', '{tenantId}2', '{tenantId}4', '{tenantId}4', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', '1', 'Adet', '$7,000.00', '$7,000.00', 'percent', '{tenantId}2', '{tenantId}2', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', '2', 'Adet', '$7,000.00', '$14,000.00', 'percent', '{tenantId}2', '{tenantId}1', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', '10', 'Adet', '$1,200.00', '$12,000.00', 'percent', '{tenantId}1', '{tenantId}5', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', '2', 'Adet', '$12,000.00', '$24,000.00', 'percent', '{tenantId}1', '{tenantId}2', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}6', '3', 'Adet', '$9,000.00', '$27,000.00', 'percent', '{tenantId}1', '{tenantId}3', '{tenantId}4', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}7', '3', 'Adet', '$12,000.00', '$36,000.00', 'percent', '{tenantId}1', '{tenantId}1', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}8', '10', 'Adet', '$1,200.00', '$12,000.00', 'percent', '{tenantId}1', '{tenantId}4', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}9', '3', 'Adet', '$1,000.00', '$3,000.00', 'percent', '{tenantId}3', '{tenantId}2', '{tenantId}5', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}10', '4', 'Adet', '$1,200.00', '$4,800.00', 'percent', '{tenantId}2', '{tenantId}3', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', '1', 'Adet', '$1,000.00', '15', 'percent', '$850.00', '{tenantId}2', '{tenantId}1', '{tenantId}5', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', '3', 'Adet', '$9,000.00', '10', 'percent', '$24,300.00', '{tenantId}1', '{tenantId}3', '{tenantId}4', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', '1', 'Adet', '$1,000.00', '20', 'percent', '$800.00', '{tenantId}2', '{tenantId}2', '{tenantId}5', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', '3', 'Adet', '$12,000.00', '10', 'percent', '$32,400.00', '{tenantId}1', '{tenantId}2', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', '2', 'Adet', '$9,000.00', '5', 'percent', '$17,100.00', '{tenantId}1', '{tenantId}1', '{tenantId}4', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}6', '10', 'Adet', '$1,200.00', '20', 'percent', '$9,600.00', '{tenantId}2', '{tenantId}3', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"suppliers_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}3', 'MAVİBEYAZ BİLGİSAYAR LTD.', 'Bilişim Teknolojileri', '$5,000,000.00', '25', '02164344640', '05322715962', '02164293704', 'info@mavibeyazbilgisayar.com', 'www.mavibeyazbilgisayar.com', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', 'İstiklal Mh. Tursan Cd. No:5/121', 'Kadıköy', 'İstanbul', '34405', 'Türkiye', '2010 yılından bu yana çalışıyoruz', NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, '20000');" +
                              $"INSERT INTO \"public\".\"suppliers_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}4', 'MOBİLYA ÜRETİM TİC. SAN. A.Ş.', 'Ağaç İşleri, Kağıt ve Kağıt Ürünleri', '$20,000,000.00', '60', '03124527131', '05072873352', '03122639126', 'uretim@mobilyauretimas.com.tr', 'www.mobilyauretimas.com.tr', 'İstiklal Mh. Gül Cd. No:57/115', 'Etimesgut', 'Ankara', '06756', 'Türkiye', 'İstiklal Mh. Gül Cd. No:57/115', 'Etimesgut', 'Ankara', '06756', 'Türkiye', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, '0');" +
                              $"INSERT INTO \"public\".\"suppliers_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"balance\") VALUES ('{tenantId}5', 'KİMYANET TİC. LTD. ŞTİ.','Kimya, Petrol, Lastik ve Plastik', '$2,000,000.00', '15', '02162842912', '05444611935', '02164355689', 'bilgi@kimyanettic.com', 'www.kimyanettic.com', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', 'Atatürk Mh. Ateş Cd. No:97/154', 'Ümraniye', 'İstanbul', '34783', 'Türkiye', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, '-3000');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}46', '2017-05-10', '12000', NULL, 'YTU5454AC', 'Alış Faturası', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}5', 'purchase_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}47', '2017-06-28', '5000', NULL, 'GRS4545TR', 'Alış Faturası', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}5', 'purchase_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}48', '2017-07-08', '14000', 'Nakit', NULL, 'Ödeme', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}5', 'payment');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}49', '2017-06-23', '50000', NULL, 'CDF3434HJN', 'Alış Faturası', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}4', 'purchase_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}50', '2017-07-06', '50000', 'Çek', NULL, 'Ödeme', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', '2017-07-07', '{tenantId}4', 'payment');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}51', '2017-07-04', '120000', NULL, 'NBM335B45', 'Alış Faturası', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}3', 'purchase_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}52', '2017-07-08', '120000', 'Banka Transferi', NULL, 'Ödeme', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}3', 'payment');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}53', '2017-07-09', '20000', 'Banka Transferi', NULL, 'Ödeme', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, '{tenantId}3', 'payment');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}54', '2017-06-07', '2000', 'Nakit', NULL, 'Tahsilat', NULL, '{tenantId}5', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}55', '2017-07-07', '2000', 'Banka Transferi', NULL, 'Tahsilat', NULL, '{tenantId}5', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}56', '2017-07-09', '5000', NULL, 'VBD4545H56', 'Satış Faturası', NULL, '{tenantId}5', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}57', '2017-06-06', '18000', 'Banka Transferi', NULL, 'Tahsilat', NULL, '{tenantId}4', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}58', '2017-07-04', '8000', NULL, 'VDF24H456', 'Satış Faturası', NULL, '{tenantId}4', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}59', '2017-07-06', '5000', NULL, 'VBF4546GH78', 'Satış Faturası', NULL, '{tenantId}2', '{tenantId}1', NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}60', '2017-07-06', '2000', NULL, 'BVG345FG454', 'Satış Faturası', NULL, '{tenantId}3', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}61', '2017-07-10', '3000', NULL, 'VBG657G67', 'Satış Faturası', NULL, '{tenantId}2', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}62', '2017-07-10', '80000', NULL, 'VBF345VB3', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}63', '2017-07-11', '20000', NULL, 'VBF6764F33', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}64', '2017-07-09', '30000', NULL, 'VBG45436N8', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}65', '2017-06-28', '10000', NULL, 'VBH567HY78', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}66', '2017-07-12', '3000', NULL, 'VBG45656GB', 'Satış Faturası', NULL, '{tenantId}3', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}67', '2017-07-06', '1000', NULL, 'VBF546546Y78', 'Satış Faturası', NULL, '{tenantId}3', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}68', '2017-07-09', '4000', 'Nakit', NULL, 'Tahsilat', NULL, '{tenantId}3', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}69', '2017-07-21', '10000', NULL, 'HYF4546GF', 'Satış Faturası', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'sales_invoice');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}70', '2017-07-10', '120000', 'Çek', NULL, 'Tahsilat', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', '2017-07-12', NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}71', '2017-07-10', '20000', 'Nakit', NULL, 'Tahsilat', NULL, '{tenantId}1', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"current_accounts_d\" (\"id\", \"date\", \"amount\", \"payment_method\", \"invoice_no\", \"transaction_type\", \"description\", \"customer\", \"sales_order\", \"shared_users\", \"shared_user_groups\", \"shared_users_edit\", \"shared_user_groups_edit\", \"is_sample\", \"is_converted\", \"master_id\", \"migration_id\", \"import_id\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"payment_due\", \"supplier\", \"transaction_type_system\") VALUES ('{tenantId}72', '2017-07-10', '10000', 'Banka Transferi', NULL, 'Tahsilat', NULL, '{tenantId}2', NULL, NULL, NULL, NULL, NULL, 'true', 'false', NULL, NULL, NULL, {tenantValues}, 'false', NULL, NULL, 'collection');" +
                              $"INSERT INTO \"public\".\"templates\" (\"id\", \"template_type\", \"name\", \"content\", \"language\", \"active\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"module\", \"subject\", \"sharing_type\") VALUES ('1', '3', 'Fiyat Teklifi', 'quote-template-tr.docx', '2', 't', '{tenantId}', NULL, '2017-02-16 16:48:31.814936', NULL, false, 'quotes', NULL, '0');\n";
                    }
                    else
                    {
                        sql = $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Yadel', 'Energy', '$75,000,000.00', '19', '742 220 0483', '116 437 3315', '880 333 3789', 'krogersd0@tumblr.com', 'http://www.yadel.com', '369 Towne Crossing', 'Kansas', 'Shawnee Mission', '22650', 'United States', '369 Towne Crossing', 'Kansas', 'Shawnee Mission', '22650', 'United States', 'non velit nec nisi vulputate nonummy maecenas tincidunt lacus at velit vivamus vel nulla eget eros elementum', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Jetwire', 'Government', '$41,000,000.00', '57', '388 608 0694', '000 301 6203', '364 269 9153', 'tturnerct@bloglines.com', 'http://www.jetwire.com', '3 Hoffman Terrace', 'North Carolina', 'Winston Salem', '42136', 'United States', '3 Hoffman Terrace', 'North Carolina', 'Winston Salem', '42136', 'United States', 'neque aenean auctor gravida sem praesent id massa id nisl venenatis lacinia aenean sit amet justo morbi ut', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Fanoodle', 'Energy', '$82,000,000.00', '50', '430 629 0030', '278 649 7701', '213 028 4958', 'kcarpenterf67@histats.com', 'http://www.fanoodle.com', '7926 Golf Course Avenue', 'Colorado', 'Colorado Springs', '60169', 'United States', '7926 Golf Course Avenue', 'Colorado', 'Colorado Springs', '60169', 'United States', 'neque aenean auctor gravida sem praesent id massa id nisl', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Jazzy', 'Automotive', '$43,000,000.00', '68', '617 715 8170', '096 345 6427', '897 540 6174', 'jadams9u@pbs.org', 'http://www.jazzy.com', '0451 Schiller Terrace', 'Maryland', 'Annapolis', '84163', 'United States', '0451 Schiller Terrace', 'Maryland', 'Annapolis', '84163', 'United States', 'justo maecenas rhoncus aliquam lacus morbi quis tortor id nulla ultrices aliquet', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"accounts_d\" (\"id\", \"name\", \"industry\", \"annual_revenue\", \"no_of_employees\", \"phone\", \"mobile\", \"fax\", \"email\", \"web\", \"street_billing\", \"district_billing\", \"city_billing\", \"post_code_billing\", \"country_billing\", \"street_shipping\", \"district_shipping\", \"city_shipping\", \"post_code_shipping\", \"country_shipping\", \"description\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Dabfeed Automotive', 'Automotive', '$9,000,000.00', '28', '746 852 2410', '910 388 0034', '845 208 5403', 'efrazierbw@stumbleupon.com', 'http://www.dabfeed.com', '57293 Maryland Drive', 'Louisiana', 'Baton Rouge', '74660', 'United States', '57293 Maryland Drive', 'Louisiana', 'Baton Rouge', '74660', 'United States', 'justo nec condimentum neque sapien placerat ante nulla justo aliquam quis turpis eget elit sodales scelerisque', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Cynthia', 'Dunn', 'Cynthia Dunn', 'Mrs. / Ms.', 'Computer Systems Analyst II', 'HR', '1978-11-11', 'Web Site', '542 341 4862', '940 377 9525', 'cdunn6p@cmu.edu', '34 Summer Ridge Place', 'Washington', 'Seattle', '35761', 'United States', 'tempus semper est quam pharetra magna ac consequat metus sapien ut nunc vestibulum ante', '{tenantId}1', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Annie', 'Hunter', 'Annie Hunter', 'Mrs. / Ms.', 'Geologist I', 'HR', '1974-07-20', 'Email', '706 794 2148', '355 729 8714', 'ahunterbi@guardian.co.uk', '80 2nd Parkway', 'Michigan', 'Detroit', '97678', 'United States', 'at dolor quis odio consequat varius integer ac leo pellentesque ultrices mattis', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Stephen', 'Hughes', 'Stephen Hughes', 'Mrs. / Ms.', 'Technical Writer', 'HR', '1976-09-06', 'Partner', '957 241 3173', '993 166 7697', 'shughes69@sohu.com', '94 Little Fleur Hill', 'Arizona', 'Phoenix', '27', 'United States', 'ipsum aliquam non mauris morbi non lectus aliquam sit amet', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Lisa', 'Barnes', 'Lisa Barnes', 'Mrs. / Ms.', 'Graphic Designer', 'HR', '1974-05-14', 'Web Site', '956 101 0714', '505 897 7570', 'lbarnes22@ft.com', '4527 Ludington Avenue', 'Texas', 'Amarillo', '5579', 'United States', 'pretium nisl ut volutpat sapien arcu sed augue aliquam erat volutpat in', '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"contacts_d\" (\"id\", \"first_name\", \"last_name\", \"full_name\", \"title\", \"job_title\", \"department\", \"date_of_birth\", \"lead_source\", \"phone\", \"mobile\", \"email\", \"street\", \"district\", \"city\", \"post_code\", \"country\", \"description\", \"account\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Bruce', 'Stephens', 'Bruce Stephens', 'Dr.', 'Accountant I', 'HR', '1978-03-29', 'Web Site', '776 488 9673', '184 191 2638', 'bstephensdl@arstechnica.com', '6 Rowland Pass', 'Wisconsin', 'Milwaukee', '13043', 'United States', 'rhoncus dui vel sem sed sagittis nam congue risus semper porta volutpat quam pede lobortis', '{tenantId}5', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Event', 'Dinner with Lisa & Team', 'Contact', '4', NULL, NULL, NULL, 'John''s Coffee', '{eventStartDate1}', '{eventEndDate1}', 'No', NULL, NULL, NULL, NULL, NULL, 'event', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Call', 'Call Stephen for meeting', 'Contact', '3', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Outbound', 'Demo', 'Scheduled Call', '2016-12-23 13:18:02', NULL, 'call', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Event', 'Meet Ralph for Demo', 'Lead', '5', NULL, NULL, NULL, 'Foster''s Co.', '{eventStartDate2}', '{eventEndDate2}', 'Yes', NULL, NULL, NULL, NULL, NULL, 'event', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', 'Task', 'Follow-up Keith', 'Lead', '3', '2016-12-23', 'Not Started', 'Low', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'task', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"activities_d\" (\"id\", \"activity_type\", \"subject\", \"related_module\", \"related_to\", \"task_due_date\", \"task_status\", \"task_priority\", \"event_location\", \"event_start_date\", \"event_end_date\", \"all_day_event\", \"call_type\", \"call_purpose\", \"call_category\", \"call_time\", \"description\", \"activity_type_system\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', 'Call', 'Call Gary for quote', 'Lead', '4', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Outbound', 'Prospecting', 'Current Call', '2016-12-02 13:18:02', 'Ask Gary if he''s confirmed to get our latest quote', 'call', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"vat_percent\", \"stock_quantity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Consulting Service', 'CS10268', 'Quantity', '$800.00', '10', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"vat_percent\", \"stock_quantity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Training Service', 'TS10214', 'Quantity', '$700.00', '10', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"products_d\" (\"id\", \"name\", \"product_code\", \"usage_unit\", \"unit_price\", \"vat_percent\", \"stock_quantity\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Construction Starter Kit', 'CSK10239', 'Quantity', '$6,000.00', '10', '12', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Yadel Management Consulting', 'Draft', '2016-05-25', '$226,000.00', '$22,600.00', '$248,600.00', 'Payment due on delivery. All payments must be made in US currency', '10;22600', 'percent', '{tenantId}1', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Jazzy Construction Project', 'Draft', '2016-06-03', '$190,000.00', '$19,000.00', '$209,000.00', 'Payment due on delivery. All payments must be made in US currency', '10;19000', 'percent', '{tenantId}4', '{tenantId}4', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quotes_d\" (\"id\", \"subject\", \"quote_stage\", \"valid_till\", \"total\", \"vat_total\", \"grand_total\", \"terms_and_conditions\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Dabfeed Training Service', 'Negotiation', '2016-05-31', '$187,000.00', '$18,700.00', '$205,700.00', 'Payment due on delivery. All payments must be made in US currency', '10;18700', 'percent', '{tenantId}5', '{tenantId}1', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', 'Dabfeed Training Service', 'In Progress', NULL, '$18,700.00', '$205,700.00', '$187,000.00', '10;18700', 'percent', '{tenantId}5', '{tenantId}1', '{tenantId}3', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', 'Jazzy Construction Project', 'On Hold', '2016-05-17', '$17,730.00', '$195,030.00', '$177,300.00', '10;17730', 'percent', '{tenantId}4', '{tenantId}4', '{tenantId}2', 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"sales_orders_d\" (\"id\", \"subject\", \"order_stage\", \"due_date\", \"vat_total\", \"grand_total\", \"total\", \"vat_list\", \"discount_type\", \"account\", \"contact\", \"quote\", \"is_sample\", \"owner\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', 'Fanoodle New Starter Kit Sales Order', 'In Progress', '2016-06-16', '$6,840.00', '$75,240.00', '$68,400.00', '10;6840', 'percent', '{tenantId}3', '{tenantId}2', NULL, 'true', {tenantValues});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', '20', 'Quantity', '$800.00', '$16,000.00', 'percent', '{tenantId}1', '{tenantId}1', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', '35', 'Quantity', '$6,000.00', '$210,000.00', 'percent', '{tenantId}2', '{tenantId}1', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', '21', 'Quantity', '$6,000.00', '$126,000.00', 'percent', '{tenantId}1', '{tenantId}2', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', '23', 'Quantity', '$6,000.00', '$138,000.00', 'percent', '{tenantId}2', '{tenantId}3', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', '80', 'Quantity', '$800.00', '$64,000.00', 'percent', '{tenantId}2', '{tenantId}2', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"quote_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"amount\", \"discount_type\", \"order\", \"quote\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}6', '70', 'Quantity', '$700.00', '$49,000.00', 'percent', '{tenantId}1', '{tenantId}3', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}2', '12', 'Quantity', '$6,000.00', '5', 'percent', '$68,400.00', '{tenantId}1', '{tenantId}3', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}3', '21', 'Quantity', '$6,000.00', '5', 'percent', '$119,700.00', '{tenantId}1', '{tenantId}2', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}4', '80', 'Quantity', '$800.00', '10', 'percent', '$57,600.00', '{tenantId}2', '{tenantId}2', '{tenantId}1', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}5', '70', 'Quantity', '$700.00', NULL, 'percent', '$49,000.00', '{tenantId}1', '{tenantId}1', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}1', '23', 'Quantity', '$6,000.00', NULL, 'percent', '$138,000.00', '{tenantId}2', '{tenantId}1', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}6', '70', 'Quantity', '$700.00', NULL, 'percent', '$49,000.00', '{tenantId}1', '{tenantId}1', '{tenantId}2', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"order_products_d\" (\"id\", \"quantity\", \"usage_unit\", \"unit_price\", \"discount_percent\", \"discount_type\", \"amount\", \"order\", \"sales_order\", \"product\", \"is_sample\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\") VALUES ('{tenantId}7', '23', 'Quantity', '$6,000.00', NULL, 'percent', '$138,000.00', '{tenantId}2', '{tenantId}1', '{tenantId}3', 'true', {tenantValues2});\n" +
                              $"INSERT INTO \"public\".\"templates\" (\"id\", \"template_type\", \"name\", \"content\", \"language\", \"active\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"module\", \"subject\", \"sharing_type\") VALUES ('1', '3', 'Quote Template', 'quote-template-en.docx', '1', 't', '{tenantId}', NULL, '2017-02-16 16:48:31.814936', NULL, false, 'quotes', NULL, '0');\n";
                    }

                    sql += "SELECT SETVAL('public.accounts_d_id_seq', COALESCE(MAX(id), 1)) FROM accounts_d;\n" +
                           "SELECT SETVAL('public.contacts_d_id_seq', COALESCE(MAX(id), 1)) FROM contacts_d;\n" +
                           "SELECT SETVAL('public.activities_d_id_seq', COALESCE(MAX(id), 1)) FROM activities_d;\n" +
                           "SELECT SETVAL('public.products_d_id_seq', COALESCE(MAX(id), 1)) FROM products_d;\n" +
                           "SELECT SETVAL('public.quotes_d_id_seq', COALESCE(MAX(id), 1)) FROM quotes_d;\n" +
                           "SELECT SETVAL('public.sales_orders_d_id_seq', COALESCE(MAX(id), 1)) FROM sales_orders_d;\n" +
                           "SELECT SETVAL('public.quote_products_d_id_seq', COALESCE(MAX(id), 1)) FROM quote_products_d;\n" +
                           "SELECT SETVAL('public.order_products_d_id_seq', COALESCE(MAX(id), 1)) FROM order_products_d;\n" +
                           "SELECT SETVAL('public.suppliers_d_id_seq', COALESCE(MAX(id), 1)) FROM suppliers_d;\n" +
                           "SELECT SETVAL('public.current_accounts_d_id_seq', COALESCE(MAX(id), 1)) FROM current_accounts_d;\n" +
                           "SELECT SETVAL('public.templates_id_seq', COALESCE(MAX(id), 1)) FROM templates;";
                    break;
            }

            return sql;
        }

        public static string GenerateSampleDataUpdateSql(List<Module> modules, PlatformUser user)
        {
            var sql = "";

            modules.Where(x => x.Order > 0 && x.Name != "holidays" && x.Name != "izin_turleri").ToList()
                .ForEach(x => { sql += $"UPDATE {x.Name}_d SET is_sample=TRUE, owner={user.Id}, created_by={user.Id}, updated_by={user.Id};\n"; });

            sql += $"UPDATE reports SET user_id={user.Id} WHERE user_id IS NOT NULL;\n";
            sql += $"UPDATE process_approvers SET user_id={user.Id};\n";
			
            if (user.AppID == 1)
                sql += "UPDATE sales_orders_d SET onay_tarihi=now() WHERE id=19281;\n";

            if (user.AppId == 4 && !string.IsNullOrEmpty(user.Email))
                sql += $"UPDATE calisanlar_d SET ad='{user.FirstName}', soyad='{user.LastName}', ad_soyad='{user.FirstName + " " + user.LastName}', e_posta='{user.Email}', cep_telefonu={user.PhoneNumber}, yoneticisi=1 WHERE id=1;\n";

            return sql;
        }

        public static string GenerateSampleDataDeleteSql(List<Module> modules)
        {
            var sql = "";

            modules.ForEach(x => { sql += $"UPDATE {x.Name}_d SET deleted=TRUE WHERE is_sample=TRUE;\n"; });

            return sql;
        }

        public static string GenerateGetPicklistItemsSql(Module module, JObject record, string picklistLanguage)
        {
            var labelField = picklistLanguage == "tr" ? "label_tr" : "label_en";
            var queries = new List<string>();

            foreach (var field in module.Fields)
            {
                if (record[field.Name].IsNullOrEmpty())
                    continue;

                if (field.DataType == DataType.Picklist)
                {
                    queries.Add($"(\"picklist_id\" = {field.PicklistId.Value} AND LOWER(\"{labelField}\") = LOWER('{(string)record[field.Name]}'))");
                }

                if (field.DataType == DataType.Multiselect)
                {
                    var multiPicklistItems = (JArray)record[field.Name];

                    foreach (string multiPicklistItem in multiPicklistItems)
                    {
                        queries.Add($"(\"picklist_id\" = {field.PicklistId.Value} AND LOWER(\"{labelField}\") = LOWER('{multiPicklistItem}'))");
                    }
                }
            }

            if (queries.Count < 1)
                return null;

            var sql = "SELECT *\n" +
                      "FROM picklist_items\n" +
                      "WHERE \"deleted\" = FALSE\n" +
                      $"AND (\n{string.Join(" OR\n", queries)}\n);";

            return sql;
        }

        public static void SetPicklistItems(Module module, JObject record, string picklistLanguage, JArray picklistItems)
        {
            if (picklistItems.IsNullOrEmpty())
                return;

            var labelField = picklistLanguage == "tr" ? "label_tr" : "label_en";
            var culture = new CultureInfo(picklistLanguage == "tr" ? "tr-TR" : "en-US");

            foreach (var field in module.Fields)
            {
                if (record[field.Name].IsNullOrEmpty())
                    continue;

                if (field.DataType == DataType.Picklist)
                {
                    var picklistItem = picklistItems.FirstOrDefault(x => x[labelField].ToString().ToLower(culture) == record[field.Name].ToString().ToLower(culture));

                    if (!picklistItem.IsNullOrEmpty())
                        record[field.Name] = (JObject)picklistItem;
                }

                if (field.DataType == DataType.Multiselect)
                {
                    var multiPicklistItems = (JArray)record[field.Name];

                    foreach (string multiPicklistItem in multiPicklistItems)
                    {
                        var picklistItem = picklistItems.FirstOrDefault(x => x[labelField].ToString().ToLower(culture) == multiPicklistItem.ToLower(culture));

                        if (!picklistItem.IsNullOrEmpty())
                            record[field.Name] = (JObject)picklistItem;
                    }
                }
            }
        }

        public static void MultiselectsToString(Module module, JObject record)
        {
            foreach (var field in module.Fields)
            {
                if (field.DataType != DataType.Multiselect || record[field.Name].IsNullOrEmpty())
                    continue;

                var picklistLabels = ((JArray)record[field.Name]).ToObject<string[]>();
                record[field.Name] = string.Join("|", picklistLabels);
            }
        }

        public static string GenerateGetLookupIdsSql(string lookupType, string field, JArray lookupValues)
        {
            var tableName = lookupType + (lookupType != "users" ? "_d" : "");

            var sql = "PREPARE SelectQuery AS\n" +
                      $"SELECT \"id\", \"{field}\" AS \"value\"\n" +
                      $"FROM {tableName}\n" +
                      $"WHERE {tableName}.\"deleted\" = FALSE\n" +
                      "AND (";

            var values = new List<string>();

            for (int i = 0; i < lookupValues.Count; i++)
            {
                sql += $"{tableName}.\"{field}\" = ${i + 1} OR\n";
                string str = (string)lookupValues[i];

                if (str.Contains("'"))
                    lookupValues[i] = str.Replace("'", "''");

                values.Add("'" + (string)lookupValues[i] + "'");
            }

            sql = sql.Trim().Remove(sql.Length - 3);
            sql += ");\n";
            sql += $"EXECUTE SelectQuery ({string.Join(",", values)});\n" +
                   "DEALLOCATE SelectQuery;";

            return sql;
        }

        public static string GenerateBulkInsertSql(JArray records, Module module, int currentUserId, DateTime now)
        {
            var fields = GetFieldsDictionary(module);
            var columns = new List<string>();
            var values = new List<string>();

            foreach (var field in fields)
            {
                columns.Add("\"" + field.Value + "\"");
                values.Add("$" + (field.Key + 1));
            }

            var sql = "PREPARE InsertQuery AS\n" +
                      $"INSERT INTO \"{module.Name}_d\" ({string.Join(", ", columns)})\n" +
                      $"VALUES ({string.Join(", ", values)});\n";

            foreach (JObject record in records)
            {
                var recordValues = new List<string>();

                foreach (var field in fields)
                {
                    if (!record[field.Value].IsNullOrEmpty())
                        recordValues.Add("'" + record[field.Value].ToString().Trim().Replace("'", "''") + "'");
                    else
                    {
                        if (field.Value == "created_by" || field.Value == "updated_by")
                        {
                            recordValues.Add("'" + currentUserId + "'");
                        }
                        else if (field.Value == "created_at" || field.Value == "updated_at")
                        {
                            recordValues.Add("'" + now.ToString("yyyy-M-dd hh:mm:ss") + "'");
                        }
                        else
                        {
                            var moduleField = module.Fields.SingleOrDefault(x => x.Name == field.Value);

                            if (moduleField == null)
                            {
                                recordValues.Add("NULL");
                            }
                            else
                            {
                                if (moduleField.DataType == DataType.Checkbox)
                                {
                                    recordValues.Add("false");
                                }
                                else
                                {
                                    if (moduleField.Combination != null)
                                    {
                                        var combinationValue = "";

                                        if (!(record[moduleField.Combination.Field1].IsNullOrEmpty() && record[moduleField.Combination.Field2].IsNullOrEmpty()))
                                            combinationValue = record[moduleField.Combination.Field1] + " " + record[moduleField.Combination.Field2];

                                        if (!string.IsNullOrWhiteSpace(combinationValue))
                                            recordValues.Add("'" + combinationValue.Trim().Replace("'", "''") + "'");
                                        else
                                            recordValues.Add("NULL");
                                    }
                                    else
                                    {
                                        recordValues.Add("NULL");
                                    }
                                }
                            }
                        }
                    }
                }

                sql += $"EXECUTE InsertQuery ({string.Join(", ", recordValues)});\n";
            }

            sql += "DEALLOCATE InsertQuery;";

            return sql;
        }

        public static string GenerateRevertSql(string moduleName, int importId)
        {
            var sql = $"UPDATE \"{moduleName}_d\" SET \"deleted\" = true WHERE \"import_id\" = '{importId}'";

            return sql;
        }

        public static string GenerateLookupUserSql(int moduleId, string searchTerm, bool isReadonly, int userId)
        {
            var sql = "PREPARE SelectQuery AS\n" +
                      "	SELECT *\n" +
                      "	FROM(\n" +
                      "		SELECT\n" +
                      "			DISTINCT us.\"id\" AS \"id\",\n" +
                      "			us.\"full_name\" AS \"name\",\n" +
                      "			us.\"email\" AS \"description\",\n" +
                      "			'user' AS \"type\"\n" +
                      "		FROM users us\n" +
                      "		JOIN profiles pr ON pr.\"id\" = us.\"profile_id\" AND pr.\"deleted\" = FALSE\n" +
                      "		JOIN profile_permissions pp ON pp.\"profile_id\" = pr.\"id\"\n" +
                      "		WHERE\n" +
                      "			us.\"deleted\" = FALSE\n" +
                      "			AND us.\"is_active\" = TRUE\n" +
                      "			AND pp.\"module_id\" = $1\n" +
                      "			AND pp.\"read\" = TRUE\n" +
                      "			AND ($2 = FALSE OR pp.\"modify\" = TRUE)\n" +
                      "			AND LOWER(us.\"full_name\") LIKE LOWER($3)\n" +
                      "			AND us.\"id\" <> $4\n" +
                      "		ORDER BY\n" +
                      "			us.\"full_name\"\n" +
                      "	) AS usrs\n\n" +
                      "	UNION\n\n" +
                      "	SELECT *\n" +
                      "	FROM(\n" +
                      "		SELECT DISTINCT\n" +
                      "			ug.\"id\" AS \"id\",\n" +
                      "			ug.\"name\" AS \"name\",\n" +
                      "			'User Group' AS \"description\",\n" +
                      "			'group' AS \"type\"\n" +
                      "		FROM users_user_groups uug\n" +
                      "		JOIN user_groups ug ON ug.\"id\" = uug.\"group_id\"\n" +
                      "		JOIN users us ON us.\"id\" = uug.\"user_id\" AND us.\"deleted\" = FALSE\n" +
                      "		JOIN modules mo ON mo.\"id\" = $1\n" +
                      "		WHERE\n" +
                      "			ug.\"deleted\" = FALSE\n" +
                      "			AND us.\"is_active\" = TRUE\n" +
                      "			AND LOWER(ug.\"name\") LIKE LOWER($3)\n" +
                      "			AND ug.\"id\" NOT IN(\n" +
                      "				SELECT DISTINCT uug2.\"group_id\"\n" +
                      "				FROM profile_permissions pp\n" +
                      "				JOIN profiles pr ON pr.\"id\" = pp.\"profile_id\"\n" +
                      "				JOIN users us2 ON us2.\"profile_id\" = pr.\"id\" AND us2.\"deleted\" = FALSE\n" +
                      "				JOIN users_user_groups uug2 ON uug2.\"user_id\" = us2.\"id\"\n" +
                      "				JOIN user_groups ug2 ON ug2.\"id\" = uug2.\"group_id\" AND ug2.\"deleted\" = FALSE\n" +
                      "				WHERE pr.\"deleted\" = FALSE\n" +
                      "				AND pp.\"module_id\" = $1\n" +
                      "				AND (pp.\"read\" = FALSE OR ($2 = TRUE AND pp.\"modify\" = FALSE))\n" +
                      "			)\n" +
                      "		ORDER BY\n" +
                      "			ug.\"name\"\n" +
                      "	) AS grps;\n" +
                      $"EXECUTE SelectQuery({moduleId}, {isReadonly}, '{searchTerm.Replace("'", "''").Replace("%", "\\%")}%', '{userId}');\n" +
                      "DEALLOCATE SelectQuery;";

            return sql;
        }

        public static string GenerateSharedSql(string userIds, string userGroupIds)
        {
            var sql = "PREPARE SelectQuery AS" +
                      "	SELECT *" +
                      "	FROM(" +
                      "		SELECT" +
                      "			DISTINCT us.\"id\" AS \"id\"," +
                      "			us.\"full_name\" AS \"name\"," +
                      "			us.\"email\" AS \"description\"," +
                      "			'user' AS \"type\"" +
                      "		FROM users us" +
                      "		WHERE" +
                      "			us.\"deleted\" = FALSE" +
                      "			AND us.\"is_active\" = TRUE" +
                      $"			AND us.\"id\" = ANY(ARRAY[{userIds}])" +
                      "		ORDER BY" +
                      "			us.\"full_name\"" +
                      "	) AS usrs" +
                      "	UNION" +
                      "	SELECT *" +
                      "	FROM(" +
                      "		SELECT" +
                      "			DISTINCT ug.\"id\" AS \"id\"," +
                      "			ug.\"name\" AS \"name\"," +
                      "			'User Group' AS \"description\"," +
                      "			'group' AS \"type\"" +
                      "		FROM user_groups ug" +
                      "		WHERE" +
                      "			ug.\"deleted\" = FALSE" +
                      $"			AND ug.\"id\" = ANY(ARRAY[{userGroupIds}])" +
                      "		ORDER BY" +
                      "			ug.\"name\"" +
                      "	) AS grps;" +
                      "EXECUTE SelectQuery;" +
                      "DEALLOCATE SelectQuery;";

            return sql;
        }

        public static async Task<JObject> FormatRecordValues(Module module, JObject record, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, string picklistLanguage, string currentCulture, int timezoneMinutesFromUtc = 180, ICollection<Module> lookupModules = null, bool convertImage = false)
        {
            var recordNew = new JObject();

            if (string.IsNullOrEmpty(currentCulture))
                currentCulture = picklistLanguage == "tr" ? "tr-TR" : "en-US";

            var culture = CultureInfo.CreateSpecificCulture(currentCulture);
            Module moduleQuote = null;
            Module moduleSalesOrder = null;
            Module modulePurchaseOrder = null;

            foreach (var property in record)
            {
                var fieldName = property.Key;
                var fieldModule = module;

                if (property.Value.IsNullOrEmpty())
                {
                    recordNew[property.Key] = "";
                    continue;
                }

                if (fieldName.Contains("."))
                {
                    if (lookupModules == null)
                    {
                        recordNew[property.Key] = property.Value;
                        continue;
                    }

                    var fieldNameParts = fieldName.Split('.');

                    if (fieldNameParts.Length == 2)
                    {
                        fieldName = fieldNameParts[1];
                        var lookupType = module.Fields.Single(x => x.Name == fieldNameParts[0]).LookupType;
                        fieldModule = lookupModules.Single(x => x.Name == lookupType);
                    }
                    else if (fieldNameParts.Length == 3)
                    {
                        fieldName = fieldNameParts[2];
                        fieldModule = lookupModules.Single(x => x.Name == fieldNameParts[1]);
                    }
                }

                var field = fieldModule.Fields.SingleOrDefault(x => x.Name == fieldName);

                if (field == null)
                {
                    if (!fieldName.Contains("."))
                        recordNew[property.Key] = property.Value;

                    continue;
                }

                switch (field.DataType)
                {
                    case DataType.NumberDecimal:
                        var formatDecimal = "f" + (field.DecimalPlaces > 0 ? field.DecimalPlaces.ToString() : "2");
                        recordNew[property.Key] = ((decimal)property.Value).ToString(formatDecimal, culture);
                        break;
                    case DataType.NumberAuto:
                        recordNew[property.Key] = property.Value.ToString();

                        if (!string.IsNullOrEmpty(field.AutoNumberPrefix))
                            recordNew[property.Key] = field.AutoNumberPrefix + recordNew[property.Key];

                        if (!string.IsNullOrEmpty(field.AutoNumberSuffix))
                            recordNew[property.Key] = recordNew[property.Key] + field.AutoNumberSuffix;
                        break;
                    case DataType.Currency:
                        if (!string.IsNullOrEmpty(field.CurrencySymbol))
                            culture.NumberFormat.CurrencySymbol = field.CurrencySymbol;

                        if (!record["currency"].IsNullOrEmpty())
                        {
                            var currencyField = module.Fields.FirstOrDefault(x => x.Name == "currency");

                            if (currencyField == null)
                            {
                                switch (module.Name)
                                {
                                    case "quote_products":
                                        if (moduleQuote == null)
                                            moduleQuote = await moduleRepository.GetByName("quotes");

                                        currencyField = moduleQuote.Fields.FirstOrDefault(x => x.Name == "currency");
                                        break;
                                    case "order_products":
                                        if (moduleSalesOrder == null)
                                            moduleSalesOrder = await moduleRepository.GetByName("sales_orders");

                                        currencyField = moduleSalesOrder.Fields.FirstOrDefault(x => x.Name == "currency");
                                        break;
                                    case "purchase_order_products":
                                        if (modulePurchaseOrder == null)
                                            modulePurchaseOrder = await moduleRepository.GetByName("purchase_orders");

                                        currencyField = modulePurchaseOrder.Fields.FirstOrDefault(x => x.Name == "currency");
                                        break;
                                }
                            }

                            if (currencyField != null)
                            {
                                var currencyPicklistItem = await picklistRepository.FindItemByLabel(currencyField.PicklistId.Value, (string)record["currency"], picklistLanguage);

                                if (currencyPicklistItem != null)
                                    culture.NumberFormat.CurrencySymbol = currencyPicklistItem.Value;
                            }
                        }

                        var formatCurrency = "c" + (field.DecimalPlaces > 0 ? field.DecimalPlaces.ToString() : "2");
                        recordNew[property.Key] = ((decimal)property.Value).ToString(formatCurrency, culture);
                        break;
                    case DataType.Date:
                        var formatDate = currentCulture == "tr-TR" ? "dd.MM.yyyy" : "M/d/yyyy";
                        recordNew[property.Key] = ((DateTime)property.Value).ToString(formatDate);
                        break;
                    case DataType.DateTime:
                        var formatDateTime = currentCulture == "tr-TR" ? "dd.MM.yyyy HH:mm" : "M/d/yyyy h:mm a";
                        recordNew[property.Key] = ((DateTime)property.Value).AddMinutes(timezoneMinutesFromUtc).ToString(formatDateTime);
                        break;
                    case DataType.Time:
                        var formatTime = currentCulture == "tr-TR" ? "HH:mm" : "h:mm a";
                        recordNew[property.Key] = ((DateTime)property.Value).AddMinutes(timezoneMinutesFromUtc).ToString(formatTime);
                        break;
                    case DataType.Multiselect:
                        var valueArray = (JArray)property.Value;

                        foreach (var value in valueArray)
                        {
                            recordNew[property.Key] += value + "; ";
                        }
                        break;
                    case DataType.Checkbox:
                        var yesValue = picklistLanguage == "tr" ? "Evet" : "Yes";
                        var noValue = picklistLanguage == "tr" ? "Hayır" : "No";

                        recordNew[property.Key] = (bool)property.Value ? yesValue : noValue;
                        break;
                    case DataType.Image:
                        if (convertImage)
                        {
                            PlatformUser user;
                            using (PlatformDBContext platformDbContext = new PlatformDBContext())
                            {
                                using (PlatformUserRepository puRepo = new PlatformUserRepository(platformDbContext))
                                {
                                    user = await puRepo.Get(moduleRepository.CurrentUser.TenantId);


                                    var url = ConfigurationManager.AppSettings.Get("BlobUrl") + "/record-detail-" + user.Tenant.GuidId + "/" + property.Value;
                                    var img = "<img src=\"" + url + "\" width=\"100%\">";
                                    recordNew[property.Key] = img;
                                }
                            }
                        }
                        else
                            recordNew[property.Key] = property.Value;

                        break;
                    default:
                        recordNew[property.Key] = property.Value.ToString();
                        break;
                }
            }

            return recordNew;
        }

        public static JObject NormalizeRecordValues(JObject record, bool oneLevel = false)
        {
            var newRecord = new JObject();

            foreach (var pair in record)
            {
                if (pair.Value.IsNullOrEmpty())
                    continue;

                if (pair.Key.Contains('.'))
                {
                    var keyParts = pair.Key.Split('.');

                    if (!oneLevel && (newRecord[keyParts[0]].IsNullOrEmpty() || newRecord[keyParts[0]].Type != JTokenType.Object))
                        newRecord[keyParts[0]] = new JObject();

                    switch (keyParts.Length)
                    {
                        case 2:
                            if (!oneLevel)
                                newRecord[keyParts[0]][keyParts[1]] = record[pair.Key];
                            else
                                newRecord[keyParts[1]] = record[pair.Key];
                            break;
                        case 3:
                            if (!oneLevel)
                                newRecord[keyParts[0]][keyParts[2]] = record[pair.Key];
                            else
                                newRecord[keyParts[2]] = record[pair.Key];
                            break;
                    }
                }
                else
                {
                    if (!newRecord[pair.Key].IsNullOrEmpty())
                        continue;

                    newRecord[pair.Key] = record[pair.Key];
                }
            }

            return newRecord;
        }

        public static string GenerateBalanceSql(string type, int id, string transactionType1, string transactionType2)
        {
            var sql = "PREPARE SelectQuery AS\n" +
                      "SELECT\n" +
                      "(COALESCE(CAST(result1.total_amount as money), '')  - COALESCE(CAST(result2.total_amount as money), '')) AS balance\n" +
                      "FROM\n" +
                      "(\n" +
                      "SELECT SUM(ca.\"amount\") AS total_amount\n" +
                      "FROM current_accounts_d ca\n" +
                      "WHERE ca.\"deleted\" = FALSE\n" +
                      $"AND ca.\"{type}\" = $1\n" +
                      "AND ca.\"transaction_type_system\" = $2\n" +
                      ") AS result1\n" +
                      "CROSS JOIN\n" +
                      "(\n" +
                      "SELECT SUM(ca.\"amount\") AS total_amount\n" +
                      "FROM current_accounts_d ca\n" +
                      "WHERE ca.\"deleted\" = FALSE\n" +
                      $"AND ca.\"{type}\" = $1\n" +
                      "AND ca.\"transaction_type_system\" = $3\n" +
                      ") AS result2;\n" +
                      $"EXECUTE SelectQuery('{id}', '{transactionType1}', '{transactionType2}');\n" +
                      "DEALLOCATE SelectQuery;";

            return sql;
        }

        private static string GetFilterItemSql(Filter filter, string tableName, int filterIndex, int filterIndexExt)
        {
            string field;

            if (!filter.Field.Contains('.'))
            {
                field = $"{tableName}.\"{filter.Field}\"";
            }
            else
            {
                var fieldParts = filter.Field.Split('.');
                field = $"{fieldParts[1]}_{fieldParts[0]}.\"{fieldParts[2]}\"";
            }

            if (filter.Value.ToString() == "month()" || filter.Value.ToString() == "year()" || filter.Value.ToString() == "day()")
            {
                var time = filter.Value.ToString() == "month()" ? "MONTH" : filter.Value.ToString() == "day()" ? "DAY" : "YEAR";
                switch (filter.Operator)
                {
                    case Operator.Equals:
                        return $"EXTRACT({time} FROM {field}) = ${filterIndex}";
                    case Operator.NotEqual:
                        return $"EXTRACT({time} FROM {field})  <> ${filterIndex}";
                    case Operator.Greater:
                        return $"EXTRACT({time} FROM {field})  > ${filterIndex}";
                    case Operator.GreaterEqual:
                        return $"EXTRACT({time} FROM {field})  >= ${filterIndex}";
                    case Operator.Less:
                        return $"EXTRACT({time} FROM {field}) < ${filterIndex}";
                    case Operator.LessEqual:
                        return $"EXTRACT({time} FROM {field})  <= ${filterIndex}";
                }
            }

            // If value is array
            if (filter.Value.GetType() == typeof(JArray))
            {
                var values = (JArray)filter.Value;
                var isString = values[0].Type == JTokenType.String;

                switch (filter.Operator)
                {
                    case Operator.Is:
                        return isString ? $"ARRAY_LOWERCASE({field}) = ${filterIndex}" : $"{field} = ${filterIndex}";
                    case Operator.IsNot:
                        return isString ? $"ARRAY_LOWERCASE({field}) <> ${filterIndex}" : $"{field} <> ${filterIndex}";
                    case Operator.Contains:
                        return isString ? $"ARRAY_LOWERCASE({field}) @> ${filterIndex}" : $"{field} @> ${filterIndex}";
                    case Operator.NotContain:
                        return isString ? $"NOT(ARRAY_LOWERCASE({field}) @> ${filterIndex})" : $"NOT({field} @> ${filterIndex})";
                    case Operator.NotIn:
                        var ids = new List<int>();
                        foreach (var value in values)
                        {
                            ids.Add((int)value);
                        }
                        return $"{field} NOT IN ({String.Join(",", ids)})";
                }
            }

            //if value is date
            DateTime valueDate;
            double isDouble;
            string stringValue = filter.Value.ToString();
            // added this because datetime parses double values as date, so if the value is a double we skip parsing date.
            if (!double.TryParse(stringValue, out isDouble))
            {
                if (DateTime.TryParse(stringValue, out valueDate))
                {
                    switch (filter.Operator)
                    {
                        case Operator.Equals:
                            return $"date_trunc('minute', {field})::timestamp(0) = ${filterIndex}";
                        case Operator.NotEqual:
                            return $"date_trunc('minute', {field})::timestamp(0) <> ${filterIndex}";
                        case Operator.Greater:
                            return $"date_trunc('minute', {field})::timestamp(0) > ${filterIndex}";
                        case Operator.GreaterEqual:
                            return $"date_trunc('minute', {field})::timestamp(0) >= ${filterIndex}";
                        case Operator.Less:
                            return $"date_trunc('minute', {field})::timestamp(0) < ${filterIndex}";
                        case Operator.LessEqual:
                            return $"date_trunc('minute', {field})::timestamp(0) <= ${filterIndex}";
                    }
                }
            }

            switch (filter.Operator)
            {
                case Operator.Is:
                    if (filter.Value.ToString().Contains("I") || filter.Value.ToString().Contains("ı"))//"Turkish i problem" fix
                        return $"(LOWER({field}) = ${filterIndex} OR LOWER({field}) = ${filterIndexExt})";

                    return $"LOWER({field}) = ${filterIndex}";
                case Operator.IsNot:
                    if (filter.Value.ToString().Contains("I") || filter.Value.ToString().Contains("ı"))//"Turkish i problem" fix
                        return $"(LOWER({field}) <> ${filterIndex} AND LOWER({field}) <> ${filterIndexExt})";

                    return $"LOWER({field}) <> ${filterIndex}";
                case Operator.Equals:
                    return $"{field} = ${filterIndex}";
                case Operator.NotEqual:
                    return $"{field} <> ${filterIndex}";
                case Operator.Contains:
                case Operator.StartsWith:
                case Operator.EndsWith:
                    if (filter.Value.ToString().Contains("I") || filter.Value.ToString().Contains("ı"))//"Turkish i problem" fix
                        return $"(LOWER({field}) LIKE ${filterIndex} OR LOWER({field}) LIKE ${filterIndexExt})";

                    return $"LOWER({field}) LIKE ${filterIndex}";
                case Operator.NotContain:
                    if (filter.Value.ToString().Contains("I") || filter.Value.ToString().Contains("ı"))//"Turkish i problem" fix
                        return $"(LOWER({field}) NOT LIKE ${filterIndex} AND LOWER({field}) NOT LIKE ${filterIndexExt})";

                    return $"LOWER({field}) NOT LIKE ${filterIndex}";
                case Operator.Empty:
                    return $"{field} IS NULL";
                case Operator.NotEmpty:
                    return $"{field} IS NOT NULL";
                case Operator.Greater:
                    return $"{field} > ${filterIndex}";
                case Operator.GreaterEqual:
                    return $"{field} >= ${filterIndex}";
                case Operator.Less:
                    return $"{field} < ${filterIndex}";
                case Operator.LessEqual:
                    return $"{field} <= ${filterIndex}";
                default:
                    return string.Empty;
            }


        }

        private static Dictionary<int, string> GetFieldsDictionary(Module module)
        {
            var fieldsDictionary = new Dictionary<int, string>();
            var standardFields = new List<string> { "shared_users", "shared_user_groups", "shared_users_edit", "shared_user_groups_edit", "master_id", "migration_id", "import_id" };

            for (var i = 0; i < standardFields.Count; i++)
            {
                fieldsDictionary.Add(i, standardFields[i]);
            }

            var moduleFields = module.Fields.Where(x => !x.Deleted && x.DataType != DataType.NumberAuto).ToList();

            for (var i = 0; i < moduleFields.Count; i++)
            {
                var field = moduleFields[i];

                fieldsDictionary.Add(i + standardFields.Count, field.Name);
            }

            return fieldsDictionary;
        }

        public static async Task<ICollection<Module>> GetLookupModules(Module module, IModuleRepository moduleRepository, string additionalModule = "")
        {
            var lookupModuleNames = new List<string>();

            foreach (var field in module.Fields)
            {
                if (field.Deleted || lookupModuleNames.Contains(field.LookupType))
                    continue;

                if (field.DataType == DataType.Lookup && field.LookupType != "users" && field.LookupType != "relation" && !lookupModuleNames.Contains(field.LookupType))
                    lookupModuleNames.Add(field.LookupType);
            }

            if (!string.IsNullOrEmpty(additionalModule))
                lookupModuleNames.Add(additionalModule);

            ICollection<Module> lookupModules = new List<Module>();

            if (lookupModuleNames.Count > 0)
            {
                lookupModules = await moduleRepository.GetByNamesBasic(lookupModuleNames);

                if (lookupModules.Count < lookupModuleNames.Count)
                    throw new Exception("Some lookup modules not found!");
            }

            lookupModules.Add(ModuleHelper.GetFakeUserModule());

            return lookupModules;
        }
    }
}
