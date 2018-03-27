using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("user_apps")]
    public class UserApp
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("user_id")]//]//, Index]
        public int UserId { get; set; }

        [Column("tenant_id")]//]//, Index]
        public int TenantId { get; set; }

        [Column("main_tenant_id")]//]//, Index]
        public int MainTenantId { get; set; }

        [Column("email")]//]//, Index]
        public string Email { get; set; }

        [Column("active")]//]//, Index]
        public bool Active { get; set; }

        [Column("app_id")]//]//, Index]
        public int AppId { get; set; }
    }
}
