using System;

namespace BlazorApp.Domain.Entities
{
    public class UserProfile<T> : Entity<T> where T : struct, IEquatable<T>
    {
        public T UserId { get; set; }

        public virtual ApplicationUser<T> ApplicationUser { get; set; }

        public string LastPageVisited { get; set; }

        public bool? IsNavOpen { get; set; }

        public bool IsNavMinified { get; set; }

        public int Count { get; set; }

        public DateTime LastUpdatedDate { get; set; }
    }
}