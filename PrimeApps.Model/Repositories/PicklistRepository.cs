using System.Collections.Generic;
using System.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using System.Net;
using System;
using Npgsql;

namespace PrimeApps.Model.Repositories
{
    public class PicklistRepository : RepositoryBaseTenant, IPicklistRepository
    {
        public PicklistRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<Picklist> GetById(int id)
        {
            var picklist = await DbContext.Picklists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            picklist.Items = picklist.Items.Where(q => !q.Deleted).OrderBy(x => x.Order).ToList();

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

        public async Task<ICollection<Picklist>> Find(PaginationModel paginationModel)
        {
            var picklist = DbContext.Picklists
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.Id)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Picklist).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    picklist = picklist.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    picklist = picklist.OrderByDescending(x => propertyInfo.GetValue(x, null));
                }
            }

            return await picklist.ToListAsync();
        }

        public async Task<Picklist> GetItemPage(int id, PaginationModel paginationModel)
        {
            var picklist = await DbContext.Picklists.Where(x => !x.Deleted && x.Id == id)
                .Include(x => x.Items).FirstOrDefaultAsync();

            picklist.Items = picklist.Items
                .Where(x => !x.Deleted)
                .OrderByDescending(q => q.Id)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit).ToList();

            //var picklist = await DbContext.Picklists.Where(x => !x.Deleted && x.Id == id).FirstAsync();

            //var items = await DbContext.PicklistItems.Where(x => !x.Deleted && x.PicklistId == id)
            //    .OrderByDescending(q => q.Id)
            //    .Skip(paginationModel.Offset * paginationModel.Limit)
            //    .Take(paginationModel.Limit).ToListAsync();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(PicklistItem).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    picklist.Items = picklist.Items.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    picklist.Items = picklist.Items.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }
            }

            //picklist.Items = items;

            return picklist;
        }

        public async Task<int> Count()
        {
            var count = await DbContext.Picklists.Where(x => !x.Deleted).CountAsync();

            return count;
        }

        public async Task<int> CountItems(int id)
        {
            var count = await DbContext.PicklistItems.CountAsync(x => !x.Deleted && x.PicklistId == id);

            return count;
        }

        public async Task<PicklistItem> GetItemById(int id)
        {
            var picklistItem = await DbContext.PicklistItems
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return picklistItem;
        }

        public async Task<PicklistItem> GetItemBySystemCode(string systemCode)
        {
            var picklistItem = await DbContext.PicklistItems
                .FirstOrDefaultAsync(x => !x.Deleted && x.SystemCode == systemCode);

            return picklistItem;
        }

        public async Task<bool> GetItemUniqueBySystemCode(string systemCode)
        {
            var result = await DbContext.PicklistItems
                .AnyAsync(x => x.SystemCode == systemCode);

            return result;
        }

        public async Task<ICollection<PicklistItem>> GetItemsAll()
        {
            var picklistItems = await DbContext.PicklistItems
                .Where(x => !x.Deleted)
                .ToListAsync();

            return picklistItems;
        }

        public async Task<ICollection<PicklistItem>> FindItems(List<int> ids)
        {
            if (ids == null || ids.Count < 1)
                return null;

            var picklistItems = await DbContext.PicklistItems
                .Where(x => !x.Deleted)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            return picklistItems;
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

            try
            {
                return await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var message = ex.InnerException;
                if (message.ToString().Contains("system_code"))
                    return 2;
            }

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> AddItem(PicklistItem item)
        {
            DbContext.PicklistItems.Add(item);

            return await DbContext.SaveChangesAsync();
        }


        public async Task<int> Update(Picklist picklist)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(PicklistItem item)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Picklist picklist)
        {
            picklist.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> ItemDeleteSoft(PicklistItem picklistItem)
        {
            picklistItem.Deleted = true;
            picklistItem.Inactive = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Picklist picklist)
        {
            DbContext.Picklists.Remove(picklist);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<Picklist> GetPicklistByLabelEn(string labelEn)
        {
            var picklist = await DbContext.Picklists
                .FirstOrDefaultAsync(x => !x.Deleted && x.LabelEn == labelEn);

            return picklist;
        }

        public async Task<bool> isUniqueCheck(string systemCode)
        {
            return await DbContext.Picklists.AnyAsync(x => x.SystemCode == systemCode && !x.Deleted);
        }

        public async Task<Picklist> GetBySystemCode(string systemcode)
        {
            var picklist = await DbContext.Picklists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => !x.Deleted && x.SystemCode == systemcode);

            picklist.Items = picklist.Items.Where(q => !q.Deleted).OrderBy(x => x.Order).ToList();

            return picklist;
        }
    }
}