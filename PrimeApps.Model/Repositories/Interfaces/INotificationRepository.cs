using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.ActionButton;
using PrimeApps.Model.Common.Messaging;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface INotificationRepository  :IRepositoryBaseTenant
    {
        Task<Notification> GetById(int id);

        Task<List<Setting>> GetSetting(MessageDTO queueItem, int notificationId);
    }
}
