using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
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

        public async Task<Notification> GetNotification(int notificationId)
        {
            var emailNotification = DbContext.Notifications.Include(x => x.CreatedBy).FirstOrDefault(r => r.NotificationType == NotificationType.Email && r.Id == notificationId && r.Deleted == false);

            return emailNotification;
        }

        public async Task<List<Setting>> GetSetting(MessageDTO emailQueueItem, int notificationId)
        {
            var emailNotification = await GetNotification(notificationId);
            var emailSet = DbContext.Settings
                .Include(x => x.CreatedBy)
                .Where(r => r.Type == SettingType.Email && r.Deleted == false);

            if (emailQueueItem.AccessLevel == AccessLevelEnum.Personal)
                emailSet = emailSet.Where(r => r.UserId == emailNotification.CreatedById);
            else
                emailSet = emailSet.Where(r => !r.UserId.HasValue);

            return emailSet.ToList();
        }
    }
}

