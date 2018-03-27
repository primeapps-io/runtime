using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Helpers.QueryTranslation
{
    /// <summary>
    /// PostgreSQL Error Codes
    /// https://www.postgresql.org/docs/current/static/errcodes-appendix.html
    /// </summary>
    public static class PostgreSqlStateCodes
    {
        public const string UndefinedTable = "42P01";
        public const string UndefinedColumn = "42703";
        public const string UniqueViolation = "23505";
        public const string ForeignKeyViolation = "23503";
        public const string InvalidInput = "22P02";
        public const string DatabaseDoesNotExist = "3D000";
    }
}
