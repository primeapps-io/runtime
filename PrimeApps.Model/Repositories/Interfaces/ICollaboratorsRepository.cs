using PrimeApps.Model.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ICollaboratorsRepository : IRepositoryBaseStudio
    {
        Task<List<AppCollaborator>> GetByAppId(int appId);
        Task<List<AppCollaborator>> GetByUserId(int userId);
        Task<int> AppCollaboratorAdd(AppCollaborator appCollaborator);
        Task<int> Delete(AppCollaborator appCollaborator);
        Task<AppCollaborator> GetById(int id);
        Task<int> Update(AppCollaborator appCollaborator);
    }
}
