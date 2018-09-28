using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IMessagingRepository : IRepositoryBaseTenant
    {
        Task<Notification> Create(Notification notification);
        Task<Notification> Update(Notification notification);

    }
}
