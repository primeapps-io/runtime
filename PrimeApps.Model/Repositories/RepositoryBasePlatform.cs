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

        public void Dispose()
        {
        }
    }
}
