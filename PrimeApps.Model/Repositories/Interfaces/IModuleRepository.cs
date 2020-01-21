using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;
using PrimeApps.Model.Common;
using PrimeApps.Model.Helpers;
using System.Linq;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IModuleRepository : IRepositoryBaseTenant
    {
        Task<Module> GetById(int id);
        Task<Module> GetByName(string name);
        Task<Module> GetBasicByName(string name);
        Task<Module> GetByLabel(string name);
        Task<Module> GetByIdBasic(int id);
        Task<Module> GetByNameBasic(string name);
        Task<ICollection<Module>> GetByNamesBasic(List<string> names);
        Task<Module> GetByNameWithDependencies(string name);
        Task<ICollection<Module>> GetAll();
        Task<ICollection<Module>> GetAllDeleted();
        Task<ICollection<Module>> GetAllBasic();
        Task<int> Create(Module module);
        Task<int> CreateTable(Module module, string language);
        Task<int> CreateIndexes(Module module);
        Task<int> Update(Module module);
        Task<int> AlterTable(Module module, ModuleChanges moduleChanges, string language);
        Task<int> DeleteSoft(Module module);
        Task<int> DeleteHard(Module module);
        Task<int> DeleteTable(Module module);
        Task<Relation> GetRelation(int id);
        Task<int> CreateRelation(Relation relation);
        Task<int> CreateJunctionTable(Module module, Relation relation, ICollection<Relation> currentRelations);
        Task<int> UpdateRelation(Relation relation);
        Task<Field> GetField(int FieldId);
        Task<int> UpdateField(Field field);
        Task<int> DeleteRelationSoft(Relation relation);
        Task<int> DeleteRelationHard(Relation relation);
        Task<Dependency> GetDependency(int id);
        Task<int> CreateDependency(Dependency dependency);
        Task<int> UpdateDependency(Dependency dependency);
        Task<int> DeleteDependencySoft(Dependency dependency);
        Task<int> DeleteDependencyHard(Dependency dependency);
        Task<ICollection<Component>> GetComponents();
        Task<Field> GetFieldByName(string fieldName);
        Task<int> Count();
        IQueryable<Module> Find();
        Task<ICollection<Field>> GetModuleFieldByName(string moduleName);
        Task<Module> GetByIdFullModule(int id); 
        Task<Module> GetByNameFullModule(string name);
    }
}