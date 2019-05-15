using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class PublishCreateBindingModels
    {
        public bool ClearAllRecords { get; set; }
        public bool EnableRegistration { get; set; }
        public HostType Type { get; set; }
    }
}