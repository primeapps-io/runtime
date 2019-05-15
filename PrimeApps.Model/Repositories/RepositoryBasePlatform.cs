using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
    public abstract class RepositoryBasePlatform : IRepositoryBasePlatform, IDisposable
    {
        private PlatformDBContext _dbContext;
        private IConfiguration _configuration;

        public int? UserId { get; set; }

        public CurrentUser CurrentUser { get; set; }

        public ICacheHelper CacheHelper { get; set; }

        public RepositoryBasePlatform(PlatformDBContext dbContext, IConfiguration configuration)//, ICacheHelper cacheHelper)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            //CacheHelper = cacheHelper;
        }

        public PlatformDBContext DbContext
        {
            get
            {
                var dbConnection = _dbContext.Database.GetDbConnection();

                if (dbConnection.State != System.Data.ConnectionState.Open)
                    dbConnection.ConnectionString = _configuration.GetConnectionString("PlatformDBConnection");

                _dbContext.UserId = CurrentUser != null ? CurrentUser.UserId : 0;

                return _dbContext;
            }
        }

        public void Dispose()
        {
        }
    }
}
