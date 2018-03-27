using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Report
{
    public class ReportFilterViewModel
    {
        public string Field { get; set; }
        public Operator Operator { get; set; }
        public object Value { get; set; }
        public int No { get; set; }
    }
}