using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    /// <summary>
    /// This is the table where we keep the information about storage objects we uploaded to Windows Azure Storage.
    /// It is like a link between client and Windows Azure Storage. In every record we have the unique name of the file in azure storage.
    /// By this property we can get files directly from azure storage and return them to the clients by their real properties.
    /// </summary>
    [Table("documents")]
    public class Document : BaseEntity
    {

        /// <summary>
        /// Name of the document.
        /// </summary>
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// Record ID of document.
        /// </summary>
        [Column("record_id")]
        public int RecordId { get; set; }

        /// <summary>
        /// Which module is associated with this document.
        /// </summary>
        [Column("module_id"), ForeignKey("Module")]//, Index]
        public int ModuleId { get; set; }

        /// <summary>
        /// Unique name of the document. This is the id of the document in Windows Azure Storage.
        /// </summary>
        [Column("unique_name")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Description of the document.
        /// </summary>
        [Column("description")]
        public string Description { get; set; }

        /// <summary>
        /// Type of the document. (MimeType)
        /// </summary>
        [Column("type")]
        public string Type { get; set; }

        /// <summary>
        /// Document size in byte(s).
        /// </summary>
        [Column("file_size")]
        public long FileSize { get; set; }

        public virtual Module Module { get; set; }

    }
}