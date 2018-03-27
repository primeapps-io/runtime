using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs.Messaging.EMail
{
    public enum EMailStatusEnum
    {
        Successful,
        ConnectionFailed,
        InvalidProvider,
        InvalidUserNamePassword,
        InsufficentCredits,
        SystemError,
        OtherError
    }
}
