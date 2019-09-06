using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class FunctionRepository : RepositoryBaseTenant, IFunctionRepository
    {
        public FunctionRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<int> Count()
        {
            return await DbContext.Functions
               .Where(x => !x.Deleted)
               .CountAsync();
        }

        public async Task<bool> IsFunctionNameAvailable(string name)
        {
            return await DbContext.Functions
                .Where(x => x.Name == name)
                .FirstOrDefaultAsync() == null;
        }

        public async Task<Function> Get(int id)
        {
            return await DbContext.Functions
               .Where(x => !x.Deleted && x.Id == id)
               .FirstOrDefaultAsync();
        }

        public async Task<Function> Get(string name)
        {
            return await DbContext.Functions
               .Where(x => !x.Deleted && x.Name == name)
               .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Function>> Find(PaginationModel paginationModel)
        {
            var functions = await DbContext.Functions
                .Where(x => !x.Deleted)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit)
                .ToListAsync();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Module).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    functions = functions.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    functions = functions.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }

            return functions;
        }

        public async Task<int> Create(Function function)
        {
            DbContext.Functions.Add(function);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Function organization)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(Function organization)
        {
            organization.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }
    }
}
