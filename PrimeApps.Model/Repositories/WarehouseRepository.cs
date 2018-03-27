using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Warehouse;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class WarehouseRepository : RepositoryBaseTenant, IWarehouseRepository
    {
        private Warehouse _warehouse;

        public WarehouseRepository(TenantDBContext dbContext, Warehouse warehouse) : base(dbContext)
        {
            _warehouse = warehouse;
        }

        public async Task Create(WarehouseCreateRequest request, ICollection<Module> modules, string userEmail, string tenantLanguage)
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["WarehouseConnection"].ToString());

            using (connection)
            {
                connection.Open();

                var sql = $"CREATE DATABASE {request.DatabaseName} (EDITION = 'basic', MAXSIZE = 1 GB);\n" +
                          $"CREATE LOGIN {request.DatabaseUser} WITH password='{request.DatabasePassword}';";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 300;//5 minutes
                    command.ExecuteNonQuery();
                }
            }

            // Wait two minute for Sql Server database creation is completed
            await Task.Delay(120000);

            Model.Entities.Platform.PlatformWarehouse warehouse = Model.Entities.Platform.PlatformWarehouse.Create(request);
            await Sync(warehouse, modules, userEmail, tenantLanguage);
        }

        public async Task Sync(Model.Entities.Platform.PlatformWarehouse warehouseEntity, ICollection<Module> modules, string userEmail, string tenantLanguage)
        {
            if (warehouseEntity.Completed)
                throw new Exception("Sync already completed");

            _warehouse.CreateUser(warehouseEntity);
            _warehouse.CreateSchema(warehouseEntity, modules, tenantLanguage);
            _warehouse.SyncData(modules, warehouseEntity.DatabaseName, TenantId.Value, tenantLanguage);

            Model.Entities.Platform.PlatformWarehouse.SetCompleted(warehouseEntity, userEmail);
        }

        public void ChangePassword(string username, string password)
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["WarehouseConnection"].ToString());

            using (connection)
            {
                connection.Open();

                var sql = $"ALTER LOGIN {username} WITH password='{password}';";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        Task IWarehouseRepository.Create(PlatformWarehouse warehouse, ICollection<Module> modules, string userEmail, string tenantLanguage)
        {
            throw new NotImplementedException();
        }

        Task IWarehouseRepository.Sync(PlatformWarehouse warehouse, ICollection<Module> modules, string userEmail, string tenantLanguage)
        {
            throw new NotImplementedException();
        }

        void IWarehouseRepository.ChangePassword(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
