using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IStudioUserRepository : IRepositoryBaseStudio
    {
        Task<int> Create(StudioUser user);
        Task<int> Update(StudioUser user);
        Task<int> Delete(StudioUser user);
    }
}
