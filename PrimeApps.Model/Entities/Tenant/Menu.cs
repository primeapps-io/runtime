using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("menu")]
    public class Menu : BaseEntity
    {
        public Menu()
        {
            _profileList = new List<string>();
        }

        [Column("name"), Required]
        public string Name { get; set; }

        [Column("profiles")]
        public string Profiles
        {
            get
            {
                return string.Join(",", _profileList);
            }
            set
            {
                _profileList = value?.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }
        }

        [Column("default")]
        public bool Default { get; set; }

        [Column("description"), MaxLength(500)]
        public string Description { get; set; }
         
        [NotMapped]
        private List<string> _profileList { get; set; }

        [NotMapped]
        public List<string> ProfileList
        {
            get
            {
                return _profileList;
            }
            set
            {
                _profileList = value;
            }
        }
    }
}
