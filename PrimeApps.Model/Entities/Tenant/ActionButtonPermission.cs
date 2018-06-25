using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Application
{
    [Table("action_button_permissions")]
    public class ActionButtonPermission : BaseEntity
    {
        [JsonIgnore]
        [Column("action_button_id"), ForeignKey("ActionButton"), /*Index("action_button_permissions_IX_action_button_id_profile_id", 1, IsUnique = true)*/]
        public int ActionButtonId { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required, /*Index("action_button_permissions_IX_action_button_id_profile_id", 2, IsUnique = true)*/]
        public int ProfileId { get; set; }

        [Column("type"), Required]
        public ActionButtonPermissionType Type { get; set; }

        public virtual ActionButton ActionButton { get; set; }

        public virtual Profile Profile { get; set; }
    }
}