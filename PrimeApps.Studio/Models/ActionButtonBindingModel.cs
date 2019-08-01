using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class ActionButtonBindingModel
    {
        [Required, StringLength(500)]
        public string ActionButtonName { get; set; }

        public ActionButtonEnum.ActionType Type { get; set; }

        public ActionButtonEnum.ActionTrigger Trigger { get; set; }

        public string Template { get; set; }

        public ActionButtonEnum.WebhhookHttpMethod MethodType { get; set; }

        public string Parameters { get; set; }

        public string Headers { get; set; }

        public string ActionButtonUrl { get; set; }

        public string CssClass { get; set; }

        public int ModuleId { get; set; }

        public List<ActionButtonPermissionBindingModel> Permissions { get; set; }
    }

    public class ActionButtonPermissionBindingModel
    {
        public int? Id { get; set; }

        public int ProfileId { get; set; }

        public ActionButtonPermissionType Type { get; set; }
    }
}