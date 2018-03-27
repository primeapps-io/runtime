using PrimeApps.Model.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IReminderRepository : IRepositoryBaseTenant
    {
        Task<Reminder> GetById(int id);
        Task<Reminder> Create(Reminder reminder);
        Task<Reminder> Update(Reminder reminder);
        Task Delete(int recordId, int moduleId);
        Task<Reminder> GetReminder(int recordId, string reminderFor = null, int moduleId = 0);

    }
}
