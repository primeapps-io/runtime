using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.Team
{
    public class TeamUserModel
    {
        [DataMember(Name = "user_id"), JsonProperty("user_id"), Required]
        public int UserId { get; set; }

    }
}
