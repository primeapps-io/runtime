using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPublishRepository : IRepositoryBaseStudio
    {
        Task<Deployment> GetLastDeployment(int appId);
    }
}