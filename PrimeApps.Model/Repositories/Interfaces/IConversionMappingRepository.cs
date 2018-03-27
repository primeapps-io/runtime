using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Application;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IConversionMappingRepository : IRepositoryBaseTenant
    {
        Task<ICollection<ConversionMapping>> GetAll(int moduleId);
        Task<ICollection<ConversionSubModule>> GetSubConversions(int moduleId);
        Task<List<ConversionMapping>> GetMappingsByFieldId(int fieldId);
        Task<int> Create(ConversionMapping note);
        Task<ConversionMapping> GetByFields(ConversionMapping conversionMapping);
        Task<int> Update(ConversionMapping note);
        Task<int> DeleteSoft(ConversionMapping note);
        Task<int> DeleteHard(ConversionMapping note);
    }
}
