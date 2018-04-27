using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IRepositoryBaseTenant
    {
        CurrentUser CurrentUser { get; set; }
        TenantDBContext DbContext { get; }
        int? TenantId { get; set; }
    }
}