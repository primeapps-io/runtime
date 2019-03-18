using System.Collections.Generic;

namespace PrimeApps.Studio.Models
{
    public class PicklistViewModel
    {
        public int Id { get; set; }
        public string LabelEn { get; set; }
        public string LabelTr { get; set; } 
        public string SystemCode { get; set; }
        public List<PicklistItemViewModel> Items { get; set; }
    }
}