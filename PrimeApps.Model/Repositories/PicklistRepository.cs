using System.Collections.Generic;
using System.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Repositories
{
    public class PicklistRepository : RepositoryBaseTenant, IPicklistRepository
    {
        public PicklistRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<Picklist> GetById(int id)
        {
            var picklist = await DbContext.Picklists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            picklist.Items = picklist.Items.OrderBy(x => x.Order).ToList();

            return picklist;
        }

        public async Task<ICollection<Picklist>> GetAll()
        {
            var picklists = DbContext.Picklists
                .Where(x => !x.Deleted && x.LabelEn != "Module")
                .ToListAsync();

            return await picklists;
        }

        public async Task<ICollection<Picklist>> Find(List<int> ids = null)
        {
            var picklistsQuery = DbContext.Picklists
                .Include(x => x.Items)
                .Where(x => !x.Deleted);

            if (ids != null && ids.Count > 0)
            {
                picklistsQuery = picklistsQuery
                    .Where(x => ids.Contains(x.Id));
            }

            var picklists = await picklistsQuery.ToListAsync();

            return picklists;
        }

        public Task<PicklistItem> GetItemById(int id)
        {
            var picklistItem = DbContext.PicklistItems
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return picklistItem;
        }

        public Task<PicklistItem> GetItemBySystemCode(string systemCode)
        {
            var picklistItem = DbContext.PicklistItems
                .FirstOrDefaultAsync(x => !x.Deleted && x.SystemCode == systemCode);

            return picklistItem;
        }

        public async Task<ICollection<PicklistItem>> GetItemsAll()
        {
            var picklistItems = DbContext.PicklistItems
                .Where(x => !x.Deleted)
                .ToListAsync();

            return await picklistItems;
        }

        public async Task<ICollection<PicklistItem>> FindItems(List<int> ids)
        {
            if (ids == null || ids.Count < 1)
                return null;

            var picklistItems = DbContext.PicklistItems
                .Where(x => !x.Deleted)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            return await picklistItems;
        }

        public async Task<PicklistItem> FindItemByLabel(int picklistId, string label, string language)
        {
            var picklistItem = DbContext.PicklistItems
                .Where(x => !x.Deleted)
                .Where(x => x.PicklistId == picklistId);

            if (language == "tr")
                picklistItem = picklistItem.Where(x => x.LabelTr == label);
            else
                picklistItem = picklistItem.Where(x => x.LabelEn == label);

            return await picklistItem.FirstOrDefaultAsync();
        }

        public async Task<int> Create(Picklist picklist)
        {
            DbContext.Picklists.Add(picklist);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Picklist picklist)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Picklist picklist)
        {
            picklist.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Picklist picklist)
        {
            DbContext.Picklists.Remove(picklist);

            return await DbContext.SaveChangesAsync();
        }
        public Task<Picklist> GetPicklistByLabelEn(string labelEn)
        {
            var picklist = DbContext.Picklists
                .FirstOrDefaultAsync(x => !x.Deleted && x.LabelEn == labelEn);

            return picklist;
        }
    }
}