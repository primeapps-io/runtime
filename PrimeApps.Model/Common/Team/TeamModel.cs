using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.Team
{
    public class TeamModel
    {
        [DataMember(Name = "id"), JsonProperty("id")]
        public int Id { get; set; }

        [DataMember(Name = "name"), JsonProperty("name"), Required]
        public string Name { get; set; }

        [DataMember(Name = "icon"), JsonProperty("icon")]
        public string Icon { get; set; }

        [DataMember(Name = "team_users"), JsonProperty("team_users")]
        public List<TeamUserModel> TeamUsers { get; set; }
        
        [DataMember(Name = "app_id"), JsonProperty("app_id")]
        public List<int> AppIds { get; set; }

        [DataMember(Name = "deleted"), JsonProperty("deleted")]
        public bool Deleted { get; set; }

    }
}
