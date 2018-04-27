using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Context;
using PrimeApps.Model.Exceptions;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;

namespace PrimeApps.Model.Repositories
{
    public abstract class RepositoryBaseTenant : IRepositoryBaseTenant, IDisposable
    {
        private TenantDBContext _dbContext;

        public int? TenantId { get; set; }

        public int? UserId { get; set; }

        public CurrentUser CurrentUser { get; set; }

        public RepositoryBaseTenant(TenantDBContext dbContext)
        {
            _dbContext = dbContext;

            if (dbContext.TenantId.HasValue && !TenantId.HasValue)
            {
                TenantId = dbContext.TenantId;
            }
        }

        public TenantDBContext DbContext
        {
            get
            {
                if (_dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                {
                    if (TenantId.HasValue)
                    {
                        _dbContext.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(TenantId.Value);
                    }
                    else if (CurrentUser.TenantId != -1)
                    {
                        _dbContext.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(CurrentUser.TenantId);
                    }
                    else
                    {
                        throw new TenantNotFoundException("No valid Tenant Database information found for the repository.");
                    }
                }
                return _dbContext;
            }
        }


        public void Dispose()
        {
        }
    }
}
