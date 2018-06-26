using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IRepositoryBasePlatform
    {
        CurrentUser CurrentUser { get; set; }
        PlatformDBContext DbContext { get; }
    }
}