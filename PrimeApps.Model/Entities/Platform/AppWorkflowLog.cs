using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("workflow_logs")]
    public class AppWorkflowLog : BaseEntity
    {
        [Column("workflow_id"), ForeignKey("AppWorkflow")]
        public int AppWorkflowId { get; set; }

        [Column("app_id"), Required]
        public int AppId { get; set; }

        public virtual AppWorkflow AppWorkflow { get; set; }
    }
}
