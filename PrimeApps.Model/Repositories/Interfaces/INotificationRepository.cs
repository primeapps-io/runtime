using PrimeApps.Model.Entities.Application;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.ActionButton;
using PrimeApps.Model.Common.Messaging;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface INotificationRepository  :IRepositoryBaseTenant
    {
        Task<Notification> GetNotification(int notificationId);

        Task<List<Setting>> GetSetting(MessageDTO emailQueueItem, int notificationId);
    }
}
