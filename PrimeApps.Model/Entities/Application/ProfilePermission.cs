using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    /// <summary>
    /// Contains permission details for user profiles.
    /// </summary>
    [Table("profile_permissions")]
    public class ProfilePermission
    {

        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        [Column("type")]
        public EntityType Type { get; set; }

        [Column("read")]
        public bool Read { get; set; }

        [Column("write")]
        public bool Write { get; set; }

        [Column("modify")]
        public bool Modify { get; set; }

        [Column("remove")]
        public bool Remove { get; set; }

        [Column("profile_id"), ForeignKey("Profile")]
        public int ProfileId { get; set; }

        [Column("module_id"), ForeignKey("Module")]
        public int? ModuleId { get; set; }

        public virtual Profile Profile { get; set; }

        public virtual Module Module { get; set; }

    }
}
