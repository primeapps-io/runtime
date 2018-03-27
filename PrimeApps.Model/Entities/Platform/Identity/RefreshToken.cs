using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform.Identity
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Column("id"), Key]
        public string Id { get; set; }

        [Column("subject"), Required, MaxLength(50)]//, Index]
        public string Subject { get; set; }

        [Column("client_id"), Required, MaxLength(50)]//, Index]
        public string ClientId { get; set; }

        [Column("issued_utc")]
        public DateTime IssuedUtc { get; set; }

        [Column("expires_utc")]
        public DateTime ExpiresUtc { get; set; }

        [Column("protected_ticket"), Required]
        public string ProtectedTicket { get; set; }
    }
}