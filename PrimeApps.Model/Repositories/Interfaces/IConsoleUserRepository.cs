using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IConsoleUserRepository : IRepositoryBaseConsole
    {
        Task<int> Create(ConsoleUser user);
        Task<int> Update(ConsoleUser user);
        Task<int> Delete(ConsoleUser user);
    }
}
