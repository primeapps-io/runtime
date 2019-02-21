using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPicklistRepository : IRepositoryBaseTenant
    {
        Task<Picklist> GetById(int id);
        Task<ICollection<Picklist>> GetAll();
        Task<Picklist> GetPicklistByLabelEn(string labelEn);
        Task<ICollection<Picklist>> Find(PaginationModel paginationModel);
        Task<ICollection<Picklist>> Find(List<int> ids);
        Task<Picklist> GetItemPage(int id, PaginationModel paginationModel);
        Task<int> Count();
        Task<int> CountItems(int id);
        Task<PicklistItem> GetItemById(int id);
        Task<PicklistItem> GetItemBySystemCode(string systemCode);
        Task<ICollection<PicklistItem>> GetItemsAll();
        Task<ICollection<PicklistItem>> FindItems(List<int> ids);
        Task<PicklistItem> FindItemByLabel(int picklistId, string label, string language);
        Task<int> Create(Picklist picklist);
        Task<int> Update(Picklist picklist);
        Task<int> DeleteSoft(Picklist picklist);
        Task<int> DeleteHard(Picklist picklist);
        Task<int> ItemDeleteSoft(PicklistItem picklistItem);
    }
}
