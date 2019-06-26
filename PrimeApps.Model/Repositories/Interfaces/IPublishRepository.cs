using System.Threading.Tasks;
using Microsoft.SqlServer.Management.SqlScriptPublish;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPublishRepository : IRepositoryBaseStudio
    {
        Task<Release> GetLastDeployment(int appId);
    }
}