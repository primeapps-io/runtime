using Newtonsoft.Json.Linq;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IWorkflowCoreRepository : IRepositoryBasePlatform
    {
        JArray GetWorkflowInstances(string code);
        JArray GetExecutionPointers(string code);
    }
}
