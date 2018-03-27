using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IRepositoryBaseTenant
    {
        CurrentUser CurrentUser { get; }
        TenantDBContext DbContext { get; }
        TenantDBContext DbContextLazy { get; }
        int? TenantId { get; set; }
    }
}