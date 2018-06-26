using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Helpers.QueryTranslation;
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
                      // ", pr.process_id, pr.process_status, pr.operation_type, pr.process_status_order\n" +//Approval Processes
                      ", pr.process_id, pr.process_status, pr.operation_type, pr.updated_by AS \"process_request_updated_by\", pr.updated_at AS \"process_request_updated_at\", pr.process_status_order\n" +//Approval Processes
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
                    fieldsSql += ", \"process_requests_process\".process_id AS \"process.process_requests.process_id\", process_requests_process.process_status AS \"process.process_requests.process_status\", process_requests_process.operation_type AS \"process.process_requests.operation_type\", process_requests_process.updated_by AS \"process.process_requests.updated_by\", process_requests_process.updated_at AS \"process.process_requests.updated_at\", process_requests_process.process_status_order AS \"process.process_requests.process_status_order\"";
                //fieldsSql += ", \"process_requests_process\".process_id AS \"process.process_requests.process_id\", process_requests_process.process_status AS \"process.process_requests.process_status\", process_requests_process.operation_type AS \"process.process_requests.operation_type\", process_requests_process.process_status_order AS \"process.process_requests.process_status_order\"";
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

                if (!findRequest.SortField.Contains(".") && !findRequest.SortField.Contains(","))
                {
                    sortSql = $"\"{tableName}\".\"{findRequest.SortField}\" {sortDirection}";
                }
                else if (findRequest.SortField.Contains(",") && !findRequest.SortField.Contains("."))
                {
                    var sortFieldParts = findRequest.SortField.Split(',');
                    sortSql = $"\"{tableName}\".\"{sortFieldParts[0]}\" {sortDirection}," + $"\"{tableName}\".\"{sortFieldParts[1]}\" {sortDirection}";
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
                    case DataType.Tag:
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
                case "sales_invoices":
                case "purchase_invoices":
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
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "is_sample", NpgsqlValue = (bool)record["is_sample"], NpgsqlDbType = NpgsqlDbType.Boolean });
                sets.Add("\"is_sample\" = @is_sample");
            }

            if (!record["is_converted"].IsNullOrEmpty())
            {
                command.Parameters.Add(new NpgsqlParameter { ParameterName = "is_converted", NpgsqlValue = (bool)record["is_converted"], NpgsqlDbType = NpgsqlDbType.Boolean });
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
                case "sales_invoices":
                case "purchase_invoices":
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

        public static string GenerateSampleDataUpdateSql(List<Module> modules, PlatformUser user)
        {
            var sql = "";

            modules.Where(x => x.Order > 0 && x.Name != "holidays" && x.Name != "izin_turleri").ToList()
                .ForEach(x => { sql += $"UPDATE {x.Name}_d SET is_sample=TRUE, owner={user.Id}, created_by={user.Id}, updated_by={user.Id};\n"; });

            sql += $"UPDATE reports SET user_id={user.Id} WHERE user_id IS NOT NULL;\n";
            sql += $"UPDATE process_approvers SET user_id={user.Id};\n";

            var tenant = user.TenantsAsUser.Single();

            if (tenant.Tenant.AppId == 1)
            {
                //sql += "UPDATE sales_orders_d SET onay_tarihi=now() WHERE id=19281;\n";
                sql += "UPDATE sales_orders_d SET onay_tarihi=now(), created_at=now(), updated_at=now() WHERE id=19281;\n";
                sql += "UPDATE quotes_d SET created_at=now(), updated_at=now()";
            }


            if (tenant.Tenant.AppId == 4 && !string.IsNullOrEmpty(user.Email))
            {
                sql += $"UPDATE calisanlar_d SET ad='{user.FirstName}', soyad='{user.LastName}', ad_soyad='{user.FirstName + " " + user.LastName}', e_posta='{user.Email}', cep_telefonu={user.Setting.Phone}, yoneticisi=1 WHERE id=1;\n";
                sql += $"UPDATE rehber_d SET ad='{user.FirstName}', soyad='{user.LastName}', ad_soyad='{user.FirstName + " " + user.LastName}', e_posta='{user.Email}', cep_telefonu={user.Setting.Phone} WHERE id=20;\n";
            }

            return sql;
        }

        public static string GenerateSampleDataDeleteSql(List<Module> modules)
        {
            var sql = "";

            modules.ForEach(x => { sql += $"UPDATE {x.Name}_d SET deleted=TRUE WHERE is_sample=TRUE;\n"; });
            sql += $"UPDATE notes SET deleted=TRUE WHERE id=2;\n";

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
                if (field.DataType != DataType.Multiselect || field.DataType != DataType.Tag || record[field.Name].IsNullOrEmpty())
                    continue;

                var picklistLabels = ((JArray)record[field.Name]).ToObject<string[]>();
                record[field.Name] = string.Join("|", picklistLabels);
            }
        }
        public static void TagToString(Module module, JObject record)
        {
            foreach (var field in module.Fields)
            {
                if (field.DataType != DataType.Tag || record[field.Name].IsNullOrEmpty())
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

        public static async Task<JObject> FormatRecordValues(Module module, JObject record, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IConfiguration configuration, string picklistLanguage, string currentCulture, int timezoneMinutesFromUtc = 180, ICollection<Module> lookupModules = null, bool convertImage = false)
        {
            var recordNew = new JObject();

            if (string.IsNullOrEmpty(currentCulture))
                currentCulture = picklistLanguage == "tr" ? "tr-TR" : "en-US";

            var culture = CultureInfo.CreateSpecificCulture(currentCulture);
            Module moduleQuote = null;
            Module moduleSalesOrder = null;
            Module modulePurchaseOrder = null;
            Module modulePurchaseInvoice = null;
            Module moduleSalesInvoice = null;

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
                                    case "purchase_invoices_products":
                                        if (modulePurchaseInvoice == null)
                                            modulePurchaseInvoice = await moduleRepository.GetByName("purchase_invoices");

                                        currencyField = modulePurchaseInvoice.Fields.FirstOrDefault(x => x.Name == "currency");
                                        break;
                                    case "sales_invoices_products":
                                        if (moduleSalesInvoice == null)
                                            moduleSalesInvoice = await moduleRepository.GetByName("sales_invoices");

                                        currencyField = moduleSalesInvoice.Fields.FirstOrDefault(x => x.Name == "currency");
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
                    case DataType.Tag:
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
                            using (var platformDbContext = new PlatformDBContext())
                            {
                                using (var tenantRepository = new TenantRepository(platformDbContext))
                                {
                                    var tenant = tenantRepository.Get(moduleRepository.CurrentUser.TenantId);

                                    var url = configuration.GetSection("AppSettings")["BlobUrl"] + "/record-detail-" + tenant.GuidId + "/" + property.Value;
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
                        //return isString ? $"ARRAY_LOWERCASE({field}) @> ${filterIndex}" : $"{field} @> ${filterIndex}";
                        return isString ? $"ARRAY_LOWERCASE({field}) && ${filterIndex}" : $"{field} && ${filterIndex}";
                    case Operator.NotContain:
                        //return isString ? $"NOT(ARRAY_LOWERCASE({field}) @> ${filterIndex})" : $"NOT({field} @> ${filterIndex})";
                        return isString ? $"NOT(ARRAY_LOWERCASE({field}) && ${filterIndex})" : $"NOT({field} && ${filterIndex})";
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
                {
                    var lookupModule = await moduleRepository.GetByNameBasic(field.LookupType);
                    if (lookupModule != null)
                        lookupModuleNames.Add(field.LookupType);
                }
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
