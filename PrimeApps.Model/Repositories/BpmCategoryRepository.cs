using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class BpmCategoryRepository : RepositoryBaseTenant, IBpmCategoryRepository
    {
        public BpmCategoryRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<BpmCategory> Get(int id)
        {
            var bpmCategory = await DbContext.BpmCategories.Where(q => q.Id == id && !q.Deleted).FirstOrDefaultAsync();

            return bpmCategory;
        }

        public async Task<ICollection<BpmCategory>> GetAll()
        {
            var bpmCategories = await DbContext.BpmCategories.Where(q => !q.Deleted).ToListAsync();

            return bpmCategories;
        }

        public async Task<int> Count(BpmFindRequest request)
        {
            //TODO
            var count = await DbContext.BpmCategories.Where(q => !q.Deleted).CountAsync();

            return count;
        }

        public async Task<int> Create(BpmCategory BpmCategory)
        {
            DbContext.BpmCategories.Add(BpmCategory);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(BpmCategory BpmCategory)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(BpmCategory BpmCategory)
        {
            BpmCategory.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(BpmCategory BpmCategory)
        {
            DbContext.BpmCategories.Remove(BpmCategory);

            return await DbContext.SaveChangesAsync();
        }
    }
}
