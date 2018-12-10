using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static PrimeApps.Model.Enums.ActionButtonEnum;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("action_buttons")]

    public class ActionButton : BaseEntity
    {
        [Column("name"), Required, MaxLength(50)]
        public string Name { get; set; }

        [Column("template"), Required]
        public string Template { get; set; }

        [Column("url"), Required]
        public string Url { get; set; }

        [Column("icon")]
        public string Icon { get; set; }

        [Column("css_class")]
        public string CssClass { get; set; }

        [Column("dependent_field")]
        public string DependentField { get; set; }

        [Column("dependent")]
        public string Dependent { get; set; }

        [Column("method_type"), Required, DefaultValue(WorkflowHttpMethod.Post)]
        public WebhhookHttpMethod MethodType { get; set; }

        [Column("parameters")]
        public string Parameters { get; set; }

        [Column("type")]
        public ActionType Type { get; set; }

        [Column("trigger")]
        public ActionTrigger Trigger { get; set; }

        [Column("module_id"), ForeignKey("Module")]
        public int ModuleId { get; set; }

        public virtual Module Module { get; set; }

        public virtual ICollection<ActionButtonPermission> Permissions { get; set; }
       
    }
}
