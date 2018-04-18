using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("team_users")]
	public class TeamUsers
	{
		[Column("user_id")]
		public int UserId { get; set; }
		public virtual PlatformUser PlatformUser { get; set; }

		[Column("team_id")]
		public int TeamId { get; set; }
		public virtual Team Team { get; set; }

		[Column("role")]
		public string Role { get; set; }
	}
}
