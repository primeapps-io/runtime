using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Common.Bpm
{
    public class Reference
    {
        public int Id { get; set; }

        public string Culture { get; set; }

        public string TimeZone { get; set; }

        public string Language { get; set; }

        public int TenantId { get; set; }

        public int AppId { get; set; }

        public int ProfileId { get; set; }

    }
}
