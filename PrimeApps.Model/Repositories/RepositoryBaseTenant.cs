using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Context;
using PrimeApps.Model.Exceptions;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;

namespace PrimeApps.Model.Repositories
{
    public abstract class RepositoryBaseTenant : IRepositoryBaseTenant, IDisposable
    {
        private TenantDBContext _dbContext;
        private IConfiguration _configuration;

        public int? TenantId { get; set; }

        public int? UserId { get; set; }

        public CurrentUser CurrentUser { get; set; }

        public RepositoryBaseTenant(TenantDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
           
            if (dbContext.TenantId.HasValue && !TenantId.HasValue)
            {
                TenantId = dbContext.TenantId;
            }
        }

        public TenantDBContext DbContext
        {
            get
            {
                var dbConnection = _dbContext.Database.GetDbConnection();
                var connectionString = _configuration.GetConnectionString("TenantDBConnection");

                if (dbConnection.State != System.Data.ConnectionState.Open)
                {
                    if (TenantId.HasValue)
                    {
                        dbConnection.ConnectionString = Postgres.GetConnectionString(connectionString, TenantId.Value, CurrentUser.PreviewMode);
                    }
                    else if (CurrentUser.TenantId != -1)
                    {
                        dbConnection.ConnectionString = Postgres.GetConnectionString(connectionString, CurrentUser.TenantId, CurrentUser.PreviewMode);
                    }
                    else
                    {
                        throw new TenantNotFoundException("No valid Tenant Database information found for the repository.");
                    }
                }

                _dbContext.UserId = CurrentUser.UserId;

                return _dbContext;
            }
        }

        public void Dispose()
        {
        }
    }
}
