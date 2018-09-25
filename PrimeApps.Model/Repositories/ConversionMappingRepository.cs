using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class ConversionMappingRepository : RepositoryBaseTenant, IConversionMappingRepository
    {
        public ConversionMappingRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<ICollection<ConversionMapping>> GetAll(int moduleId)
        {
            var conversionMappings = await DbContext.ConversionMappings
                .Include(x => x.MappingModule)
                .Where(x => !x.Deleted && x.ModuleId == moduleId)
                .ToListAsync();

            return conversionMappings;
        }
        public async Task<List<ConversionMapping>> GetMappingsByFieldId(int fieldId)
        {
            var mappings = await DbContext.ConversionMappings
                .Include(x => x.MappingModule)
                .Where(x => !x.Deleted && x.FieldId == fieldId)
                .ToListAsync();

            return mappings;
        }

        public async Task<int> Create(ConversionMapping conversionMapping)
        {
            DbContext.ConversionMappings.Add(conversionMapping);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<ConversionMapping> GetByFields(ConversionMapping conversionMapping)
        {
            //x.MappingFieldId == conversionMapping.MappingFieldId && -> check this out
            return await DbContext.ConversionMappings.FirstOrDefaultAsync(x => x.MappingModuleId == conversionMapping.MappingModuleId && x.ModuleId == conversionMapping.ModuleId && x.FieldId == conversionMapping.FieldId);
        }

        public async Task<int> Update(ConversionMapping conversionMapping)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(ConversionMapping conversionMapping)
        {
            conversionMapping.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(ConversionMapping conversionMapping)
        {
            DbContext.ConversionMappings.Remove(conversionMapping);

            return await DbContext.SaveChangesAsync();
        }

        //conversion sub modules methods
        public async Task<ICollection<ConversionSubModule>> GetSubConversions(int moduleId)
        {
            var conversionSubModules = await DbContext.ConversionSubModules
                .Include(x => x.MappingSubModule)
                .Where(x => !x.Deleted && x.ModuleId == moduleId)
                .ToListAsync();

            return conversionSubModules;
        }
    }
}

