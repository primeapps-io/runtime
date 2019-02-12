using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IAppProfileRepository : IRepositoryBaseConsole
    {
        Task<int> Create(AppProfile app);
        Task<int> Update(AppProfile app);
        Task<int> Delete(AppProfile app);
        Task<AppProfile> GetByAppId(int id);
    }
}
