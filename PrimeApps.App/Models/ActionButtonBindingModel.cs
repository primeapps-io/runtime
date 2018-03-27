using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;
using static PrimeApps.Model.Enums.ActionButtonEnum;

namespace PrimeApps.App.Models
{
    public class ActionButtonBindingModel
    {
        [Required, StringLength(500)]
        public string Name { get; set; }

        public ActionType Type { get; set; }

        public ActionTrigger Trigger { get; set; }

        public string Template { get; set; }

        public WebhhokHttpMethod MethodType { get; set; }

        public string Parameters { get; set; }

        public string Url { get; set; }

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