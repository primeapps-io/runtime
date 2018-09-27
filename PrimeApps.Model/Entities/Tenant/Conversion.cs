using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("conversion_mappings")]
    public class ConversionMapping : BaseEntity
    {
        [Column("module_id"), ForeignKey("Module")]//, Index]
        public int ModuleId { get; set; }

        [Column("mapping_module_id"), ForeignKey("MappingModule")]
        public int MappingModuleId { get; set; }

        [Column("field_id")]
        public int FieldId { get; set; }

        [Column("mapping_field_id")]
        public int MappingFieldId { get; set; }

        public virtual Module Module { get; set; }

        public virtual Module MappingModule { get; set; }
    }
}
