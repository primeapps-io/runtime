using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("team_users")]
    public class TeamUser
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("team_id")]
        public int TeamId { get; set; }

        [Column("role")]
        public string Role { get; set; }

        public virtual PlatformUser PlatformUser { get; set; }

        public virtual Team Team { get; set; }
    }
}
