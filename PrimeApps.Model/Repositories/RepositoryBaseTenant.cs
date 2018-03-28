using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Context;
using PrimeApps.Model.Exceptions;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Security.Claims;
using System.Threading;

namespace PrimeApps.Model.Repositories
{
    public abstract class RepositoryBaseTenant : IRepositoryBaseTenant, IDisposable
    {
        private TenantDBContext _dbContext;
        private int? _tenantId;

        public int? TenantId
        {
            get { return _tenantId; }
            set { _tenantId = value; }
        }

        private int? _userId;

        public int? UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }


        public RepositoryBaseTenant(TenantDBContext dbContext)
        {
            _dbContext = dbContext;

            if (dbContext.TenantId.HasValue && !_tenantId.HasValue)
            {
                _tenantId = dbContext.TenantId;
            }

        }

        public RepositoryBaseTenant(TenantDBContext dbContext, int tenantId) : this(dbContext)
        {
            _tenantId = tenantId;
        }

        public TenantDBContext DbContext
        {
            get
            {
                if (_dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                {
                    if (_tenantId.HasValue)
                    {
                        _dbContext.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(_tenantId.Value);
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

        public TenantDBContext DbContextLazy
        {
            get
            {
                //TODO: Find out another method to configure this settings.
                //DbContext.Configuration.LazyLoadingEnabled = true;
                //DbContext.Configuration.ProxyCreationEnabled = true;

                return DbContext;
            }
        }

        public CurrentUser CurrentUser
        {
            get
            {
                var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
                ClaimsIdentity claimsIdentity = null;

                if (claimsPrincipal != null)
                    claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;

                var currentUser = new CurrentUser
                {
                    TenantId = -1,
                    UserId = -1
                };

                if (claimsIdentity != null && claimsIdentity.HasClaim(x => x.Type == "user_id"))
                {
                    currentUser.UserId = int.Parse(claimsIdentity.FindFirst(x => x.Type == "user_id").Value);
                }
                else if (_userId.HasValue)
                {
                    currentUser.UserId = _userId.Value;
                }

                if (claimsIdentity != null && claimsIdentity.HasClaim(x => x.Type == "tenant_id"))
                {
                    currentUser.TenantId = int.Parse(claimsIdentity.FindFirst(x => x.Type == "tenant_id").Value);
                }
                else if (_tenantId.HasValue)
                {
                    currentUser.TenantId = _tenantId.Value;
                }

                return currentUser;
            }
        }

        public void Dispose()
        {
        }
    }
}
