using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform.Identity
{
    [Table("clients")]
    public class Client
    {
        [Column("id"), Key]
        public string Id { get; set; }

        [Column("secret"), Required]//, Index]
        public string Secret { get; set; }

        [Column("name"), Required, MaxLength(100)]//, Index]
        public string Name { get; set; }

        [Column("application_type")]
        public ApplicationTypesEnum ApplicationType { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("refresh_token_life_time")]
        public int RefreshTokenLifeTime { get; set; }

        [Column("allowed_origin"), MaxLength(200)]
        public string AllowedOrigin { get; set; }
    }
}