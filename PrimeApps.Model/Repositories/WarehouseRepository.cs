using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Warehouse;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class WarehouseRepository : RepositoryBaseTenant, IWarehouseRepository
    {
        private Warehouse _warehouse;
        private IPlatformWarehouseRepository _platformWarehouseRepository;
        private IConfiguration _configuration;

        public WarehouseRepository(TenantDBContext dbContext, Warehouse warehouse, IPlatformWarehouseRepository platformWarehouseRepository, IConfiguration configuration) : base(dbContext, configuration)
        {
            _warehouse = warehouse;
            _platformWarehouseRepository = platformWarehouseRepository;
            _configuration = configuration;
        }

        public async Task Create(WarehouseCreateRequest request, ICollection<Module> modules, string userEmail, string tenantLanguage)
        {
            var connection = new SqlConnection(_configuration.GetConnectionString("WarehouseConnection"));

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
            var warehouse = _platformWarehouseRepository.Create(new PlatformWarehouse()
            {
                Completed = false,
                DatabaseName = request.DatabaseName,
                DatabaseUser = request.DatabaseUser,
                PowerbiWorkspaceId = request.PowerBiWorkspaceId,
                TenantId = request.TenantId

            });
            //Model.Entities.Platform.PlatformWarehouse warehouse = Model.Entities.Platform.PlatformWarehouse.Create(request);
            await Sync(warehouse, modules, userEmail, tenantLanguage);
        }

        public async Task Sync(Model.Entities.Platform.PlatformWarehouse warehouseEntity, ICollection<Module> modules, string userEmail, string tenantLanguage)
        {
            if (warehouseEntity.Completed)
                throw new Exception("Sync already completed");

            _warehouse.CreateUser(warehouseEntity);
            _warehouse.CreateSchema(warehouseEntity, modules, tenantLanguage, _configuration.GetConnectionString("WarehouseConnection"));
            _warehouse.SyncData(modules, warehouseEntity.DatabaseName, CurrentUser, tenantLanguage);

            _platformWarehouseRepository.SetCompleted(warehouseEntity, userEmail);
        }

        public void ChangePassword(string username, string password)
        {
            var connection = new SqlConnection(_configuration.GetConnectionString("WarehouseConnection"));

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
