using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Messaging
{
    [DataContract]
    public class EMailRequest
    {
        [DataMember]
        [Required]
        public int ModuleId { get; set; }
        [DataMember]
        public string[] Ids { get; set; }
        [DataMember]
        public string Query { get; set; }

        [DataMember]
        public bool IsAllSelected { get; set; }
        [DataMember]
        [Required]
        public string Subject { get; set; }
        [DataMember]
        [Required]
        public string Message { get; set; }
        [DataMember]
        [Required]
        public string EMailField { get; set; }
        [DataMember]
        [Required]
        public string SenderAlias { get; set; }
        [DataMember]
        [Required]
        public AccessLevelEnum ProviderType { get; set; }
        [DataMember]
        [Required]
        public string SenderEMail { get; set; }
        [DataMember]
        public string AttachmentContainer { get; set; }
        [DataMember]
        public string AttachmentLink { get; set; }
        [DataMember]
        public string AttachmentName { get; set; }
        [DataMember]
        public string Cc { get; set; }
        [DataMember]
        public string Bcc { get; set; }
    }
}
