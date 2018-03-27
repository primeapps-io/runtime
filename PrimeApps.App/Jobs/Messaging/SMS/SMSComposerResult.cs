using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs.Messaging.SMS
{
    public class SMSComposerResult
    {
        public string ProviderResponse { get; set; }
        public IList<Message> Messages { get; set; }
        public JArray DetailedMessageStatusList { get; set; }
        public int Successful { get; set; }
        public int NotAllowed { get; set; }
        public int InvalidNumbers { get; set; }
        public int MissingNumbers { get; set; }
    }
}
