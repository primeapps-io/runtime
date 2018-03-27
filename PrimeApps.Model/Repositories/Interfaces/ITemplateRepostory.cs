using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITemplateRepostory : IRepositoryBaseTenant
    {
        Task<Template> GetById(int id);
        Task<ICollection<Template>> GetAll(TemplateType templateType = TemplateType.NotSet, string moduleName = "");
        Task<int> Create(Template template);
        Task<int> Update(Template template);
        Task<int> DeleteSoft(Template template);
        Task<int> DeleteHard(Template template);
    }
}
