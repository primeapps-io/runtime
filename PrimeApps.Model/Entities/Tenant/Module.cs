using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("modules")]
    public class Module : BaseEntity
    {
        [Column("name"), MaxLength(50), Required]
        public string Name { get; set; }

        [Column("system_type"), Required]
        public SystemType SystemType { get; set; }

        [Column("order")]
        public short Order { get; set; }

        [Column("display")]
        public bool Display { get; set; }

        [Column("sharing"), Required]
        public Sharing Sharing { get; set; }

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

        [Column("location_enabled")]
        public bool LocationEnabled { get; set; }

        [Column("display_calendar")]
        public bool DisplayCalendar { get; set; }

        [Column("calendar_color_dark")]
        public string CalendarColorDark { get; set; }

        [Column("calendar_color_light")]
        public string CalendarColorLight { get; set; }

        [Column("detail_view_type")]
        public DetailViewType DetailViewType { get; set; }

        public virtual ICollection<Section> Sections { get; set; }

        public virtual ICollection<Field> Fields { get; set; }

        public virtual ICollection<Relation> Relations { get; set; }

        public virtual ICollection<Dependency> Dependencies { get; set; }

        public virtual ICollection<Calculation> Calculations { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }

        public virtual ICollection<Note> Notes { get; set; }

        public virtual ICollection<AuditLog> AuditLogs { get; set; }

        public virtual ICollection<Reminder> Reminders { get; set; }

        public virtual ICollection<Component> Components { get; set; }

    }
}
