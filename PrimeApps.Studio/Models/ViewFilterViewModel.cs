using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class ViewFilterViewModel
    {
        public string Field { get; set; }
        public Operator Operator { get; set; }
        public object Value { get; set; }
        public int No { get; set; }
    }
}