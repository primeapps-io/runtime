using System.Collections.Generic;
using PrimeApps.Model.Common;
using PrimeApps.Model.Enums;

namespace PrimeApps.Console.Models
{
    public class ViewViewModel
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string SystemCode { get; set; }
        public SystemType SystemType { get; set; }
        public string LabelEn { get; set; }
        public string LabelTr { get; set; }
        public bool Active { get; set; }
        public ViewSharingType SharingType { get; set; }
        public string FilterLogic { get; set; }
        public int CreatedBy { get; set; }
        public List<ViewFieldViewModel> Fields { get; set; }
        public List<ViewFilterViewModel> Filters { get; set; }
        public List<UserBasicViewModel> Shares { get; set; }
    }
}