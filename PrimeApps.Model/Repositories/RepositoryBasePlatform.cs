using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Security.Claims;
using System.Threading;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
    public abstract class RepositoryBasePlatform : IRepositoryBasePlatform, IDisposable
    {
        private PlatformDBContext _dbContext;
        private int? _userId;

        public int? UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public RepositoryBasePlatform(PlatformDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public PlatformDBContext DbContext
        {
            get
            {
                return _dbContext;
            }
        }

        public PlatformDBContext DbContextLazy
        {
            get
            {
                DbContext.Configuration.LazyLoadingEnabled = true;
                DbContext.Configuration.ProxyCreationEnabled = true;

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

                return currentUser;
            }
        }

        public void Dispose()
        {
        }
    }
}
