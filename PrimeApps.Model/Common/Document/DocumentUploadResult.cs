using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Document
{
    /// <summary>
    /// DocumentResult DTO object helps us to transfer document list from server to client.
    /// </summary>
    [DataContract]
    public class DocumentUploadResult
    {
        /// <summary>
        /// Unique name of the document. This is the id of the document in Windows Azure Storage. 
        /// </summary>
        [DataMember]
        public string UniqueName { get; set; }

        /// <summary>
        /// File name of the document.
        /// </summary>
        [DataMember]
        public string FileName { get; set; }

        /// <summary>
        /// ContentType of the document.
        /// </summary>
        [DataMember]
        public string ContentType { get; set; }

        /// <summary>
        /// Public URL of downloadable file, if provided.
        /// </summary>
        [DataMember]
        public string PublicURL { get; set; }

        /// <summary>
        /// Chunk size
        /// </summary>
        [DataMember]
        public int Chunks { get; set; }
    }
}