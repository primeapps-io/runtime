using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("conversion_sub_modules")]
    public class ConversionSubModule : BaseEntity
    {
        [Column("module_id"), ForeignKey("Module")]//, Index]
        public int ModuleId { get; set; }

        [Column("sub_module_id"), ForeignKey("MappingSubModule")]
        public int MappingModuleId { get; set; }

        [Column("submodule_source_field")]
        public string SubModuleSourceField { get; set; }

        [Column("submodule_destination_field")]
        public string SubModuleDestinationField { get; set; }

        public virtual Module Module { get; set; }

        public virtual Module MappingSubModule { get; set; }
    }
}
