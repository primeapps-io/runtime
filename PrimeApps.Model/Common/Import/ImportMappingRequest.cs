using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Common.Import
{
    public class ImportMappingRequest
    {
        public int Id { get; set; }

        public int ModuleId { get; set; }

        public string Name { get; set; }

        public bool Skip { get; set; }

        public string Mapping { get; set; }

    }
}
