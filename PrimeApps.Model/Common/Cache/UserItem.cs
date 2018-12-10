using System;

namespace PrimeApps.Model.Common.Cache
{
    public class UserItem
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Culture { get; set; }
        public string Currency { get; set; }
        public string TimeZone { get; set; }
        public string Language { get; set; }
        public int AppId { get; set; }
        public int TenantId { get; set; }
        public Guid TenantGuid { get; set; }
        public string TenantLanguage { get; set; }
        public int RoleId { get; set; }
        public int ProfileId { get; set; }
        public bool HasAdminProfile { get; set; }
        public string WarehouseDatabaseName { get; set; }
        public int OrganizationId { get; set; }
    }
}