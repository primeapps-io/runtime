using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Console
{
    [Table("organization_users")]
    public class OrganizationUser : BaseEntity
    {
        [Column("user_id")]
        public int UserId { get; set; }
        public virtual ConsoleUser ConsoleUser { get; set; }

        [Column("organization_id")]
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Column("role")]
        public OrganizationRole Role { get; set; }

    }
}
