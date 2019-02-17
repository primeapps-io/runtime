using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IRepositoryBaseConsole
    {
        CurrentUser CurrentUser { get; set; }
        StudioDBContext DbContext { get; }
    }
}