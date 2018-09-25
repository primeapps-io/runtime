using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PrimeApps.Model.Entities.Application
{
    [Table("roles")]
    public class Role : BaseEntity
    {
        public Role()
        {
            Users = new List<TenantUser>();
            _ownersList = new List<string>();

        }

        [Column("label_en"), MaxLength(200), Required]
        public string LabelEn { get; set; }

        [Column("label_tr"), MaxLength(200), Required]
        public string LabelTr { get; set; }

        [Column("description_en"), MaxLength(500)]
        public string DescriptionEn { get; set; }

        [Column("description_tr"), MaxLength(500)]
        public string DescriptionTr { get; set; }

        [Column("master")]
        public bool Master { get; set; }

        [Column("owners")]
        public string Owners
        {
            get
            {
                return string.Join(",", _ownersList);
            }
            set
            {
                _ownersList = value?.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }
        }

        [Column("migration_id")]
        public string MigrationId { get; set; }

        [Column("share_data")]
        public bool ShareData { get; set; }

        [Column("reports_to_id"), ForeignKey("ReportsTo")]
        public int? ReportsToId { get; set; }

        [Column("system_code")]
        public string SystemCode { get; set; }

        public virtual Role ReportsTo { get; set; }

        [InverseProperty("Role")]
        public virtual ICollection<TenantUser> Users { get; set; }

        
        [NotMapped]
        private List<string> _ownersList { get; set; }

        [NotMapped]
        public List<string> OwnersList
        {
            get
            {
                return _ownersList;
            }
            set
            {
                _ownersList = value;
            }
        }
    }
}
