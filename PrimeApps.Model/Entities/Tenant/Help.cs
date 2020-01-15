using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("helps")]
    public class Help : BaseEntity
    {
        [Column("name"), Required]
        public string Name { get; set; }

        [Column("template"), Required]
        public string Template { get; set; }

        [Column("module_id"), ForeignKey("Module")]
        public int? ModuleId { get; set; }

        [Column("modal_type")]
        public ModalType ModalType { get; set; }

        [Column("show_type")]
        public ShowType ShowType { get; set; }

        [Column("module_type")]
        public ModuleType ModuleType { get; set; }

        [Column("route_url")]
        public string RouteUrl { get; set; }
        
        [Column("first_screen")]
        public bool FirstScreen { get; set; }

        [Column("custom_help")]
        public bool CustomHelp { get; set; }
        
        [Column("language"), Required]
        public LanguageType Language { get; set; }

        [JsonIgnore]
        public virtual Module Module { get; set; }

    }
}
