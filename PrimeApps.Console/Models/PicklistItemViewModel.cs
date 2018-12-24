namespace PrimeApps.Console.Models.ViewModel.Crm.Picklist
{
    public class PicklistItemViewModel
    {
        public int Id { get; set; }
        public string LabelEn { get; set; }
        public string LabelTr { get; set; }
        public string Value { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string SystemCode { get; set; }
        public short Order { get; set; }
        public bool Inactive { get; set; }
    }
}