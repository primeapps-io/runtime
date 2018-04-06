using System.Collections.Generic;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.ActionButton
{
    public class ActionButtonViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ActionButtonEnum.ActionType ActionType { get; set; }
        public string Template { get; set; }
        public int ModuleId { get; set; }
        public string Icon { get; set; }
        public string CssClass { get; set; }
        public string Url { get; set; }
        public string DependentField { get; set; }
        public string Dependent { get; set; }
        public ActionButtonEnum.WebhhookHttpMethod MethodType { get; set; }
        public string Parameters { get; set; }
        public ActionButtonEnum.ActionTrigger Trigger { get; set; }
        public List<ActionButtonPermissionViewModel> Permissions { get; set; }
    }

    public class ActionButtonPermissionViewModel
    {
        public int? Id { get; set; }

        public int ProfileId { get; set; }

        public ActionButtonPermissionType Type { get; set; }
    }
}
