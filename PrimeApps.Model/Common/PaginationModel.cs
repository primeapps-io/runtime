using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace PrimeApps.Model.Common
{
    public class PaginationModel
    {
        [JsonProperty("limit"), DataMember(Name = "limit")]
        public int Limit { get; set; }
        [JsonProperty("offset"), DataMember(Name = "offset")]
        public int Offset { get; set; }
        [JsonProperty("order_column"), DataMember(Name = "order_column")]
        public string OrderColumn { get; set; }
        [JsonProperty("order_type"), DataMember(Name = "order_type")]
        public string OrderType { get; set; }
    }
}
