using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("workflows")]
    public class AppWorkflow : BaseEntity
    {
        [Column("name"), MaxLength(200), Required]
        public string Name { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("frequency"), Required]
        public WorkflowFrequency Frequency { get; set; }

        [Column("operations"), MaxLength(50), Required]
        public string Operations { get; set; }

        public AppWorkflowWebhook WebHook { get; set; }

        public virtual ICollection<AppWorkflowLog> Logs { get; set; }

        [Column("app_id"), ForeignKey("App")]
        public int AppId { get; set; }

        public virtual App App { get; set; }

        [NotMapped]
        public string[] OperationsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Operations))
                    return null;

                return Operations.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    Operations = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }
    }
}
