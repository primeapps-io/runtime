using Microsoft.Extensions.Configuration;
using System;

namespace PrimeApps.Model.Common.Document
{
    /// <summary>
    /// DocumentResult DTO object helps us to transfer document list from server to client.
    /// </summary>
    public class DocumentResult
    {
        private IConfiguration _configuration;
        public DocumentResult(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// ID of the document.
        /// </summary>
        public virtual int ID { get; set; }

        /// <summary>
        /// Name of the document.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Unique name of the document. This is the id of the document in Windows Azure Storage. 
        /// </summary>
        public virtual string UniqueName { get; set; }

        /// <summary>
        /// Description of the document.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Type of the document.
        /// </summary>
        public virtual string Type { get; set; }

        /// <summary>
        /// Owner User id of the document.
        /// </summary>
        public virtual int CreatedBy { get; set; }

        /// <summary>
        /// Creation date of the document
        /// </summary>
        public virtual DateTime? CreatedTime { get; set; }

        /// <summary>
        /// GuidId of the tenant which is being used as container Id, the old InstanceId
        /// </summary>
        public virtual Guid ContainerId { get; set; }

        /// <summary>
        /// Document size in byte(s).
        /// </summary>
        public virtual double FileSize { get; set; }

        /// <summary>
        /// CreatedBy user fullname.
        /// </summary>
        public virtual string CreatedByName { get; set; }

        /// <summary>
        /// Document full URL.
        /// </summary>
        public virtual string FileUrl{ get; set; }
        
        /// <summary>
        /// Document nonentity record id
        /// </summary>
        public virtual int RecordId { get; set; }
        /// <summary>
        /// Document associated module id
        /// </summary>
        public virtual int ModuleId { get; set; }
    }
}