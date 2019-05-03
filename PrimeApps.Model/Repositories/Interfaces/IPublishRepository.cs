using Newtonsoft.Json.Linq;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPublishRepository : IRepositoryBaseTenant
    {
        JArray GetAllDynamicTables();
        bool CleanUp(JArray tableNames = null);
        bool CleanUpTables(JArray tableNames = null);
    }
}