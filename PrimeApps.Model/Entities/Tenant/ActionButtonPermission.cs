using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("action_button_permissions")]
    public class ActionButtonPermission : BaseEntity
    {
        [JsonIgnore]
        [Column("action_button_id"), ForeignKey("ActionButton")]
        public int ActionButtonId { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required]
        public int ProfileId { get; set; }

        [Column("type"), Required]
        public ActionButtonPermissionType Type { get; set; }

        public virtual ActionButton ActionButton { get; set; }

        public virtual Profile Profile { get; set; }
    }
}