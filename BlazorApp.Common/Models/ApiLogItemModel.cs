using System;

namespace BlazorApp.Common.Models
{
    public class ApiLogItemModel
    {
        public string Id { get; set; }

        public DateTime RequestTime { get; set; }

        public long ResponseMillis { get; set; }

        public int StatusCode { get; set; }

        public string Method { get; set; }

        public string Path { get; set; }

        public string QueryString { get; set; }

        public string RequestBody { get; set; }

        public string ResponseBody { get; set; }

        public string IpAddress { get; set; }

        public string ApplicationUserId { get; set; }
    }
}