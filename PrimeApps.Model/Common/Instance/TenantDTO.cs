using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Instance
{
    /// <summary>
    /// This dto object transfers all required instance data to the client side.
    /// </summary>
    public class TenantDTO
    {
        /// <summary>
        /// Tenant Name/Title
        /// </summary>
        [DataMember]
        [Required(ErrorMessage = "required")]
        public string Title { get; set; }

        /// <summary>
        /// Tenant Currency
        /// </summary>
        [DataMember]
        public string Currency { get; set; }

        /// <summary>
        /// Tenant Language
        /// </summary>
        [DataMember]
        public string Language { get; set; }

        /// <summary>
        /// Has Logo
        /// </summary>
        [DataMember]
        public string Logo { get; set; }


        /// <summary>
        /// Tenant ID
        /// </summary>
        [DataMember]
        [Required(ErrorMessage = "required")]
        public int TenantId { get; set; }
    }
}