using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITemplateRepository : IRepositoryBaseTenant
    {
        Task<Template> GetById(int id);
        Task<ICollection<Template>> GetByType(TemplateType templateType);
        Task<ICollection<Template>> GetAll(TemplateType templateType = TemplateType.NotSet, string moduleName = "");
        Task<ICollection<Template>> GetAllList(TemplateType templateType = TemplateType.NotSet, TemplateType excelTemplateType = TemplateType.NotSet, string moduleName = "");
        Task<int> Create(Template template);
        Task<int> CreateExcel(Template template);
        Task<int> Update(Template template);
        Task<int> DeleteSoft(Template template);
        Task<int> DeleteHard(Template template);
        int Count(TemplateType templateType);
        Task<ICollection<Template>> Find(PaginationModel paginationModel, TemplateType templateType);
    }
}
