using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Application
{
    [Table("notes")]
    public class Note : BaseEntity
    {
        [Column("text"), Required]
        public string Text { get; set; }

        [Column("module_id"), ForeignKey("Module")]//, Index]
        public int? ModuleId { get; set; }

        [Column("record_id")]//, Index]
        public int? RecordId { get; set; }

        [Column("note_id")]
        public int? NoteId { get; set; }

        public virtual Note Parent { get; set; }

        [JsonIgnore]
        public virtual Module Module { get; set; }

        public virtual ICollection<Note> Notes { get; set; }

        public virtual ICollection<NoteLikes> NoteLikes { get; set; }

        [NotMapped]
        public ICollection<TenantUser> Likes { get; set; }

        [NotMapped]
        public string RecordPrimaryValue { get; set; }
    }
}
