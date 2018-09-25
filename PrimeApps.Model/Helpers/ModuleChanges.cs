using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Helpers
{
    public class ModuleChanges
    {
        public ModuleChanges()
        {
            SectionsAdded = new List<Section>();
            SectionsDeleted = new List<Section>();
            FieldsAdded = new List<Field>();
            FieldsDeleted = new List<Field>();
            RelationsAdded = new List<Relation>();
            RelationsDeleted = new List<Relation>();
            ValidationsChanged = new Dictionary<string, string>();
        }

        public List<Section> SectionsAdded { get; set; }
        public List<Section> SectionsDeleted { get; set; }
        public List<Field> FieldsAdded { get; set; }
        public List<Field> FieldsDeleted { get; set; }
        public List<Relation> RelationsAdded { get; set; }
        public List<Relation> RelationsDeleted { get; set; }
        public Dictionary<string, string> ValidationsChanged { get; set; }
    }
}
