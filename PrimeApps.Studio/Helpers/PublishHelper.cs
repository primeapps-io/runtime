using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Studio.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Studio.Storage;

namespace PrimeApps.Studio.Helpers
{
    public interface IPublishHelper
    {
        Task Create(bool clearAllRecords, string dbName, int version, int deploymentId);
    }

    public class PublishHelper : IPublishHelper
    {
        private CurrentUser _currentUser;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;

        public PublishHelper(IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            IHttpContextAccessor context)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _context = context;
            _currentUser = UserHelper.GetCurrentUser(_context);
        }

        public async Task Create(bool clearAllRecords, string dbName, int version, int deploymentId)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();

                using (var deploymentRepository = new DeploymentRepository(databaseContext, _configuration))
                {
                    deploymentRepository.CurrentUser = _currentUser;

                    var result = Model.Helpers.PublishHelper.Create(clearAllRecords, dbName, version, deploymentId, _configuration);
                    var deployment = await deploymentRepository.Get(deploymentId);

                    if (deployment != null)
                    {
                        deployment.Status = result ? DeploymentStatus.Succeed : DeploymentStatus.Failed;
                        deployment.EndTime = DateTime.Now;

                        await deploymentRepository.Update(deployment);
                    }
                }
            }
        }
    }
}