using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform.Identity
{
    [Table("active_directory_cache")]
    public class ActiveDirectoryCache
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("unique_id")]//, Index]
        public string UniqueId { get; set; }

        [Column("cache_bits")]
        public byte[] CacheBits { get; set; }

        [Column("last_write")]
        public DateTime LastWrite { get; set; }
    }
}
