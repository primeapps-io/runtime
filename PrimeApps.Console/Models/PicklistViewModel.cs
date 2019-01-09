using System.Collections.Generic;

namespace PrimeApps.Console.Models.ViewModel.Picklist
{
    public class PicklistViewModel
    {
        public int Id { get; set; }
        public string LabelEn { get; set; }
        public string LabelTr { get; set; }
        public List<PicklistItemViewModel> Items { get; set; }
    }
}