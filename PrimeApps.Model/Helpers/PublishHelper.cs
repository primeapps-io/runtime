using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Helpers
{
    public class PublishHelper
    {
        public static string SetRecordsIsSample()
        {
            return "SELECT 'update \"' || cl.\"table_name\" || '\" set is_sample=true;' " +
                   "FROM information_schema.\"columns\" cl " +
                   "WHERE cl.\"column_name\" = 'is_sample' " +
                   "ORDER BY cl.\"table_name\" ";
        }

        public static string CleanUpSystemTables()
        {
            return "DELETE FROM analytic_shares; " +
                   "DELETE FROM audit_logs; " +
                   "DELETE FROM changelogs; " +
                   "DELETE FROM imports; " +
                   "DELETE FROM note_likes; " +
                   "DELETE FROM process_logs; " +
                   "DELETE FROM process_requests; " +
                   "DELETE FROM reminders; " +
                   "DELETE FROM report_shares; " +
                   "DELETE FROM template_shares; " +
                   "DELETE FROM user_custom_shares; " +
                   "DELETE FROM users_user_groups; " +
                   "DELETE FROM user_groups; " +
                   "DELETE FROM view_shares; " +
                   "DELETE FROM view_states; " +
                   "DELETE FROM workflow_logs; ";
        }

        public static string GetAllSystemTablesSql()
        {
            return "SELECT table_name FROM information_schema.tables WHERE table_type != 'VIEW' AND table_schema = 'public' AND table_name NOT LIKE '%_d'";
        }

        public static string GetAllDynamicTablesSql()
        {
            return "SELECT table_name FROM information_schema.tables WHERE table_type != 'VIEW' AND table_schema = 'public' AND table_name LIKE '%_d'";
        }

        public static string GetUserFKColumnsSql(string tableName)
        {
            return "SELECT " +
                   "kcu.column_name " +
                   "FROM " +
                   "information_schema.table_constraints AS tc " +
                   "JOIN information_schema.key_column_usage AS kcu " +
                   "ON tc.constraint_name = kcu.constraint_name " +
                   "AND tc.table_schema = kcu.table_schema " +
                   "JOIN information_schema.constraint_column_usage AS ccu " +
                   "ON ccu.constraint_name = tc.constraint_name " +
                   "AND ccu.table_schema = tc.table_schema " +
                   "WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_name='" + tableName + "' AND ccu.table_name = 'users';";
        }

        public static string GetAllRecordsWithColumns(string tableName, JArray columns)
        {
            var sql = new StringBuilder("SELECT id,");

            foreach (var column in columns)
            {
                var columnName = column["column_name"];
                sql.Append(columnName + ",");
            }

            sql = sql.Remove(sql.Length - 1, 1);
            sql.Append(" FROM " + tableName);

            return sql.ToString();
        }

        public static string UpdateRecordSql(JToken record, string tableName, JArray columns, int value)
        {
            var sql = new StringBuilder("UPDATE " + tableName);

            foreach (var column in columns)
            {
                var columnName = column["column_name"];

                if (string.IsNullOrEmpty(record[columnName.ToString()].ToString()))
                    continue;

                sql.Append(" SET " + columnName + " = " + value + " AND");
            }

            sql = sql.Remove(sql.Length - 3, 3);

            sql.Append("WHERE id = " + (int)record["id"] + ";");

            return sql.ToString();
        }
    }
}