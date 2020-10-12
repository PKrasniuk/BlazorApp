using System;

namespace BlazorApp.Domain.Entities
{
    public class ApiLogItem<T> : Entity<T> where T : struct, IEquatable<T>
    {
        public DateTime RequestTime { get; set; }

        public long ResponseMillis { get; set; }

        public int StatusCode { get; set; }

        public string Method { get; set; }

        public string Path { get; set; }

        public string QueryString { get; set; }

        public string RequestBody { get; set; }

        public string ResponseBody { get; set; }

        public string IpAddress { get; set; }

        public T? ApplicationUserId { get; set; }

        public virtual ApplicationUser<T> ApplicationUser { get; set; }
    }
}