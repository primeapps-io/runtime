using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs.Messaging.SMS
{
    public enum SMSStatusEnum
    {
        Successful,
        InvalidProvider,
        InvalidAlias,
        InvalidUserNamePassword,
        InsufficentCredits,
        InvalidXML,
        SystemError,
        OtherError
    }
}
