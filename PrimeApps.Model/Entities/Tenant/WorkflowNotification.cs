using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("workflow_notifications")]
    public class WorkflowNotification
    {
        [JsonIgnore]
        [Column("workflow_id"), Key]
        public int WorkflowId { get; set; }

        [Column("subject"), MaxLength(200), Required]
        public string Subject { get; set; }

        [Column("message"), Required]
        public string Message { get; set; }

        [Column("recipients"), MaxLength(4000), Required]
        public string Recipients { get; set; }

        [Column("cc"), MaxLength(4000)]
        public string CC { get; set; }

        [Column("bcc"), MaxLength(4000)]
        public string Bcc { get; set; }

        [Column("schedule")]
        public int? Schedule { get; set; }

        public virtual Workflow Workflow { get; set; }

        [NotMapped]
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

        [NotMapped]
        public ICollection<UserBasic> RecipientList { get; set; }


        [NotMapped]
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

        [NotMapped]
        public ICollection<UserBasic> CCList { get; set; }


        [NotMapped]
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

        [NotMapped]
        public ICollection<UserBasic> BccList { get; set; }
    }
}
