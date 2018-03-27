using System;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.AuditLog
{
    public class AuditLogRequest
    {
        public RecordActionType RecordActionType { get; set; }

        public SetupActionType SetupActionType { get; set; }

        public int? UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }
    }
}
