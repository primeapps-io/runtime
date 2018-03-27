using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Document
{
    /// <summary>
    /// Document DTO object to transfer documents between client and web service.
    /// </summary>
    [DataContract]
    public class DocumentDTO
    {
        /// <summary>
        /// ID of the document.
        /// </summary>
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Tenant ID required!")]
        public int TenantId { get; set; }

        /// <summary>
        /// Unique name of the document. This is the id of the document in Windows Azure Storage. 
        /// </summary>
        [DataMember]
        public string UniqueFileName { get; set; }

        /// <summary>
        /// Name of the document.
        /// </summary>
        [DataMember]
        public string FileName { get; set; }

        /// <summary>
        /// Chunk size of the document.
        /// </summary>
        [DataMember]
        public int ChunkSize { get; set; }

        /// <summary>
        /// Document size in byte(s).
        /// </summary>
        [DataMember]
        public long FileSize { get; set; }

        /// <summary>
        /// Type of the document.
        /// </summary>
        [DataMember]
        public string MimeType { get; set; }

        /// <summary>
        /// Description of the document.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Parent entity id of the document.
        /// </summary>
        [DataMember]
        public Guid ParentEntityID { get; set; }

        /// <summary>
        /// Parent entity type of the document.
        /// </summary>
        [DataMember]
        public Guid ParentEntityType { get; set; }

        /// <summary>
        /// Which record is associated with this document.
        /// </summary>

        [DataMember]
        public int RecordId { get; set; }

        /// <summary>
        /// Which module is associated with this document.
        /// </summary>
        [DataMember]
        public int ModuleId { get; set; }
    }
}