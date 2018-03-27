using System;

namespace PrimeApps.Model.Common.Document
{
    public class DocumentRequest
    {
        public int ModuleId { get; set; }
        public int RecordId { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        /// <summary>
        /// Old InstanceId, currently must be set with Tenant.GuidId
        /// </summary>
        public Guid ContainerId { get; set; }

    }
}
