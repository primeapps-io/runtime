using PrimeApps.Model.Entities.Platform.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("tenants")]
    public class Tenant
    {
        /// <summary>
        /// Tenant ID
        /// </summary>
        [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.None), Key, Required]
        public int Id { get; set; }

        [Column("guid_id"), Index]
        public Guid GuidId { get; set; }

        /// <summary>
        /// Instance Title
        /// </summary>
        [Column("title")]
        public string Title { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [Column("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        [Column("language"), Index]
        public string Language { get; set; }

        /// <summary>
        /// Has Logo
        /// </summary>
        [Column("logo")]
        public string Logo { get; set; }

        /// <summary>
        /// Has Sample Data
        /// </summary>
        [Column("has_sample_data"), Index]
        public bool? HasSampleData { get; set; }

        /// <summary>
        /// Has Business Intelligence
        /// </summary>
        [Column("has_analytics"), Index]
        public bool? HasAnalytics { get; set; }

        /// <summary>
        /// Has Ofisim Phone
        /// </summary>
        [Column("has_phone"), Index]
        public bool? HasPhone { get; set; }

        /// <summary>
        /// Custom Domain
        /// </summary>
        [Column("custom_domain"), Index]
        public string CustomDomain { get; set; }

        /// <summary>
        /// Mail Sender Name
        /// </summary>
        [Column("mail_sender_name")]
        public string MailSenderName { get; set; }

        /// <summary>
        /// Mail Sender Email
        /// </summary>
        [Column("mail_sender_email"), Index]
        public string MailSenderEmail { get; set; }

        /// <summary>
        /// Custom Title
        /// </summary>
        [Column("custom_title"), Index]
        public string CustomTitle { get; set; }

        /// <summary>
        /// Custom Login Title
        /// </summary>
        [Column("custom_description")]
        public string CustomDescription { get; set; }

        /// <summary>
        /// Custom Favicon
        /// </summary>
        [Column("custom_favicon")]
        public string CustomFavicon { get; set; }

        /// <summary>
        /// Custom Color
        /// </summary>
        [Column("custom_color")]
        public string CustomColor { get; set; }

        /// <summary>
        /// Custom Image
        /// </summary>
        [Column("custom_image")]
        public string CustomImage { get; set; }

        [Column("user_license_count"), Index]
        public int UserLicenseCount { get; set; }

        [Column("module_license_count"), Index]
        public int ModuleLicenseCount { get; set; }

        [Column("has_analytics_license"), Index]
        public bool HasAnalyticsLicense { get; set; }

        [Column("is_paid_customer"), Index]
        public bool IsPaidCustomer { get; set; }

        [Column("is_deactivated"), Index]
        public bool IsDeactivated { get; set; }

        [Column("is_suspended"), Index]
        public bool IsSuspended { get; set; }

        [Column("deactivated_at"), Index]
        public DateTime? DeactivatedAt { get; set; }

        [Column("suspended_at"), Index]
        public DateTime? SuspendedAt { get; set; }

        /// <summary>
        /// Owner of the instance.
        /// </summary>
        [Column("owner"), ForeignKey("Owner"), Index]
        public int OwnerId { get; set; }

        public virtual PlatformUser Owner { get; set; }

        /// <summary>
        /// Platform users that belongs to this tenant.
        /// </summary>
        [InverseProperty("Tenant")]
        public virtual IList<PlatformUser> Users { get; set; }

    }
}
