using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Repositories
{
    public class ReminderRepository : RepositoryBaseTenant, IReminderRepository
    {
        public ReminderRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }
         
        public async Task<Reminder> GetById(int id)
        {
            return await DbContext.Reminders.Include(x => x.CreatedBy).FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Reminder> Create(Reminder reminder)
        {
            var result = DbContext.Reminders.Add(reminder);
            await DbContext.SaveChangesAsync();
            return result.Entity;
        }
        public async Task<Reminder> Update(Reminder reminder)
        {
            DbContext.Entry(reminder).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return reminder;
        }
        public async Task Delete(int recordId, int moduleId)
        {
            var reminderRecord = await DbContext.Reminders.FirstOrDefaultAsync(x => x.RecordId == recordId && x.ModuleId == moduleId && !x.Deleted);
            reminderRecord.Deleted = true;
            await DbContext.SaveChangesAsync();
        }
        /// <summary>
        /// Get Reminder with record id of the reminderFor case. Reminder for can be module or other passage.
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="reminderFor"></param>
        /// <returns></returns>
        public async Task<Reminder> GetReminder(int recordId, string reminderFor = null, int moduleId = 0)
        {
            if (reminderFor != null)
            {
                return await DbContext.Reminders.FirstOrDefaultAsync(x => x.RecordId == recordId && x.ReminderScope == reminderFor && !x.Deleted);
            }
            return await DbContext.Reminders.FirstOrDefaultAsync(x => x.RecordId == recordId && x.ModuleId == moduleId && !x.Deleted);

        }
    }
}
