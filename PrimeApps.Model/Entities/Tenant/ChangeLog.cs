using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    /// <summary>
    /// Keeps changes for records by directly saving them as a json string with a relation of record id, updated user id and date.
    /// </summary>
    [Table("changelogs")]
    public class ChangeLog
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }
        [Column("record_id")]
        public int RecordId { get; set; }
        [Column("record"), JsonIgnore]
        public string RecordJson { get; set; }
        [Column("updated_by"), ForeignKey("UpdatedBy")]
        public int UpdatedById { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        public virtual TenantUser UpdatedBy { get; set; }
        /// <summary>
        /// Serializes and deserializes RecordJson property. Use this to prevent extra casting.
        /// </summary>
        [NotMapped]
        public JObject Record
        {
            get { return JsonConvert.DeserializeObject<JObject>(RecordJson); }
            set { RecordJson = JsonConvert.SerializeObject(value); }
        }
    }

}
