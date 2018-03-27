using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("api_logs")]
    public class ApiLog
    {
        // The (database) ID for the API log entry.
        [Column("id")]
        public long Id { get; set; }
        // The user that made the request.
        [Column("user")]
        //[Index]
        public string User { get; set; }
        [Column("tenant_id")]
        //[Index]
        // Tenant id of the user that made the request
        public int TenantId { get; set; }
        // Id of the user that made the request.
        [Column("user_id")]
        //[Index]
        public int UserId { get; set; }
        // The machine that made the request.
        [Column("machine")]
        public string Machine { get; set; }
        // The IP address that made the request.
        [Column("request_ip_address")]
        public string RequestIpAddress { get; set; }
        // The request content type.
        [Column("request_content_type")]
        public string RequestContentType { get; set; }
        // The request content body.
        [Column("request_content_body")]
        public string RequestContentBody { get; set; }
        // The request URI.
        [Column("request_uri")]
        public string RequestUri { get; set; }
        // The request method (GET, POST, etc).
        [Column("request_method")]
        public string RequestMethod { get; set; }
        // The request route template.
        [Column("request_route")]
        //[Index]
        public string RequestRoute { get; set; }
        // The request headers.
        [Column("request_headers")]
        public string RequestHeaders { get; set; }
        // The request timestamp.
        [Column("request_timestamp")]
        public DateTime? RequestTimestamp { get; set; }
        // The response content type.
        [Column("response_content_type")]
        public string ResponseContentType { get; set; }
        // The response content body.
        [Column("response_content_body")]
        public string ResponseContentBody { get; set; }
        // The response status code.
        [Column("response_status_code")]
        public int? ResponseStatusCode { get; set; }
        // The response headers.
        [Column("response_id")]
        public string ResponseHeaders { get; set; }
        // The response timestamp.
        [Column("response_timestamp")]
        public DateTime? ResponseTimestamp { get; set; }
    }
}