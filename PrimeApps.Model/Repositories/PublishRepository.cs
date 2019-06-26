using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class PublishRepository : RepositoryBaseStudio, IPublishRepository
    {
        public PublishRepository(StudioDBContext dbContext, IConfiguration configuration) : base(dbContext,
            configuration)
        {
        }

        public async Task<Release> GetLastDeployment(int appId)
        {
            return await DbContext.Releases.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.AppId == appId);
        } }
}