using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum NotificationType
    {
        Sms = 1,
        Email = 2
    }
    public enum NotificationStatus
    {
        Queued = 0,
        Successful = 1,
        ConnectionFailed = 2,
        InvalidProvider = 3,
        InvalidUserNamePassword = 4,
        InsufficentCredits = 5,
        SystemError = 6,
        InvalidAlias = 7,
        InvalidXML = 8,
        OtherError = 9
    }
}
