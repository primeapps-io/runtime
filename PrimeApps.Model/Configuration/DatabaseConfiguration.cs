using PrimeApps.Model.Context;
using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace PrimeApps.Model.Configuration
{
    public class DatabaseConfiguration : DbConfiguration
    {
        public DatabaseConfiguration()
        {

            SetExecutionStrategy("Npgsql", () => new SqlAzureExecutionStrategy(3, TimeSpan.FromMilliseconds(3000)));
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy(3, TimeSpan.FromMilliseconds(3000)));

            SetHistoryContext("Npgsql",(connection, defaultSchema) => new PostgreHistoryContext(connection, defaultSchema));
        }
    }
}
