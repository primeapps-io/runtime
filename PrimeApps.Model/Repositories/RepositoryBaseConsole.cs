using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
    public abstract class RepositoryBaseConsole : IRepositoryBaseConsole, IDisposable
    {
        private StudioDBContext _dbContext;
        private IConfiguration _configuration;

        public int? UserId { get; set; }

        public CurrentUser CurrentUser { get; set; }

        public RepositoryBaseConsole(StudioDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public StudioDBContext DbContext
        {
            get
            {
                var dbConnection = _dbContext.Database.GetDbConnection();

                if (dbConnection.State != System.Data.ConnectionState.Open)
                    dbConnection.ConnectionString = _configuration.GetConnectionString("StudioDBConnection");

                _dbContext.UserId = CurrentUser != null ? CurrentUser.UserId : 0;

                return _dbContext;
            }
        }

        public void Dispose()
        {
        }
    }
}
