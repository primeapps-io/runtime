using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PrimeApps.Model.Common.Bpm
{
    public class BpmNotification
    {

        public int WorkflowId { get; set; }

        [MaxLength(200)]
        public string Subject { get; set; }
        
        public string Message { get; set; }

        [ MaxLength(4000)]
        public string Recipients { get; set; }

        [MaxLength(4000)]
        public string CC { get; set; }

        [MaxLength(4000)]
        public string Bcc { get; set; }
        
        public int? Schedule { get; set; }

        public virtual BpmWorkflow BpmWorkflow { get; set; }
        
        public string[] RecipientsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Recipients))
                    return null;

                return Recipients.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    Recipients = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }
        
        public ICollection<UserBasic> RecipientList { get; set; }

        
        public string[] CCArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CC))
                    return null;

                return CC.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    CC = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }
        
        public ICollection<UserBasic> CCList { get; set; }
        
        public string[] BccArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Bcc))
                    return null;

                return Bcc.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    Bcc = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }

        public ICollection<UserBasic> BccList { get; set; }
    }
}
