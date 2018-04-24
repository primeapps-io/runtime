using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("team_apps")]
	public class TeamApp
	{
		[Column("app_id")]
		public int AppId { get; set; }
		public virtual App App { get; set; }

		[Column("team_id")]
		public int TeamId { get; set; }
		public virtual Team Team { get; set; }
	}
}
