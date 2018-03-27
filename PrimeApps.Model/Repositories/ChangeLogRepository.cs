using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class ChangeLogRepository : RepositoryBaseTenant, IChangeLogRepository
    {
        public ChangeLogRepository(TenantDBContext dbContext) : base(dbContext)
        {

        }

        public async Task<int> Create(ChangeLog changeLog)
        {
            DbContext.ChangeLogs.Add(changeLog);
            return await DbContext.SaveChangesAsync();
        }
    }
}
