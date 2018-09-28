using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Repositories
{
    public class MessagingRepository : RepositoryBaseTenant, IMessagingRepository
    {
        public MessagingRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<Notification> Create(Notification notification)
        {
            var newNotification = DbContext.Notifications.Add(notification);
            await DbContext.SaveChangesAsync();
            return newNotification.Entity;
        }
        public async Task<Notification> Update(Notification notification)
        {
            DbContext.Entry(notification).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return notification;
        }
    }
}
