using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Studio
{
    [Table("team_users")]
    public class TeamUser
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("team_id")]
        public int TeamId { get; set; }

        public virtual StudioUser StudioUser { get; set; }

        public virtual Team Team { get; set; }
        
        //public virtual Platform.PlatformUser UserInfo { get; set; }
    }
}
