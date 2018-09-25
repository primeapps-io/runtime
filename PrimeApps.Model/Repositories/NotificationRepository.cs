using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common.Messaging;

namespace PrimeApps.Model.Repositories
{
    public class NotificationRepository : RepositoryBaseTenant, INotificationRepository
    {
        public NotificationRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<Notification> GetById(int id)
        {
            var notification = DbContext.Notifications
                .Include(x => x.CreatedBy)
                .FirstOrDefault(r => r.Id == id && r.Deleted == false);

            return notification;
        }

        public async Task<List<Setting>> GetSetting(MessageDTO queueItem, int notificationId)
        {
            var notification = await GetById(notificationId);
            var settings = DbContext.Settings
                .Include(x => x.CreatedBy)
                .Where(r => r.Type == SettingType.Email && r.Deleted == false);

            if (queueItem.AccessLevel == AccessLevelEnum.Personal)
                settings = settings.Where(r => r.UserId == notification.CreatedById);
            else
                settings = settings.Where(r => !r.UserId.HasValue);

            return settings.ToList();
        }
    }
}

