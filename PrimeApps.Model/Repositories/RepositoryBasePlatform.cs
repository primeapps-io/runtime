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

        public RepositoryBasePlatform(PlatformDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public PlatformDBContext DbContext
        {
            get
            {
                var dbConnection = _dbContext.Database.GetDbConnection();
                dbConnection.ConnectionString = _configuration.GetConnectionString("PlatformDBConnection");

                _dbContext.UserId = CurrentUser.UserId;

                return _dbContext;
            }
        }

        public void Dispose()
        {
        }
    }
}
