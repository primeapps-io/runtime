using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class MessagingRepository : RepositoryBaseTenant, IMessagingRepository
    {
        public MessagingRepository(TenantDBContext dbContext) : base(dbContext)
        {
        }
        public async Task<Notification> Create(Notification notification)
        {
            var newNotification = DbContext.Notifications.Add(notification);
            await DbContext.SaveChangesAsync();
            return newNotification;
        }
        public async Task<Notification> Update(Notification notification)
        {
            DbContext.Entry(notification).State = System.Data.Entity.EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return notification;
        }
    }
}
