
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Common.Bpm
{
    public class BpmDataUpdate
    {
        public int? WorkflowId { get; set; }

        public Module Module { get; set; }

        public Field Field { get; set; }

        public string Value { get; set; }

    }
}
