using PrimeApps.Model.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Console
{
    [Table("team_users")]
    public class TeamUser
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("team_id")]
        public int TeamId { get; set; }

        public virtual ConsoleUser ConsoleUser { get; set; }

        public virtual Team Team { get; set; }
        
        //public virtual Platform.PlatformUser UserInfo { get; set; }
    }
}
