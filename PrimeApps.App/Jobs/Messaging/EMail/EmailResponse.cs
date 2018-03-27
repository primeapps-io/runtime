using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs.Messaging.EMail
{
    public class EMailResponse
    {
        public NotificationStatus Status { get; set; }
        public string Response { get; set; }
    }
}
