using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IRepositoryBasePlatform
    {
        PlatformDBContext DbContext { get; }
    }
}