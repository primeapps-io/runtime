using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Console.Models
{
    public class DocumentBindingModel
    {
        public int ModuleId { get; set; }

        public int RecordId { get; set; }

        public int UserId { get; set; }

        public int TenantId { get; set; }

        public int ContainerId { get; set; }
    }

    public class SecondLevel
    {
        public int RelationId { get; set; }
        public Module Module { get; set; }
        public Module SubModule { get; set; }
        public Relation SubRelation { get; set; }
    }
}