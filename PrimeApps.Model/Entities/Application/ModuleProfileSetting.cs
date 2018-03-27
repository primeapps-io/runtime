using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Application
{
    [Table("module_profile_settings")]
    public class ModuleProfileSetting : BaseEntity
    {
        public ModuleProfileSetting()
        {
            _profileList = new List<string>();
        }

        [Column("module_id"), ForeignKey("Module"), Index]
        public int ModuleId { get; set; }

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

        [Column("label_en_singular"), MaxLength(50), Required]
        public string LabelEnSingular { get; set; }

        [Column("label_en_plural"), MaxLength(50), Required]
        public string LabelEnPlural { get; set; }

        [Column("label_tr_singular"), MaxLength(50), Required]
        public string LabelTrSingular { get; set; }

        [Column("label_tr_plural"), MaxLength(50), Required]
        public string LabelTrPlural { get; set; }

        [Column("menu_icon"), MaxLength(100)]
        public string MenuIcon { get; set; }

        [Column("display")]
        public bool Display { get; set; }

        public virtual Module Module { get; set; }

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
