using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Studio
{
    [Table("app_collaborators")]
    public class AppCollaborator : BaseEntity
    {
        [Column("app_id"), ForeignKey("AppDraft")]
        public int AppId { get; set; }

        [Column("user_id"), ForeignKey("StudioUser")]
        public int? UserId { get; set; }

        [Column("team_id"), ForeignKey("Team")]
        public int? TeamId { get; set; }

        [Column("profile")]
        public ProfileEnum Profile { get; set; }

        public virtual AppDraft AppDraft { get; set; }

        public virtual StudioUser StudioUser { get; set; }

        public virtual Team Team { get; set; }
    }
}
