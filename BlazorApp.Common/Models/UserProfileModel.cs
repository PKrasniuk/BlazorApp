using System;

namespace BlazorApp.Common.Models
{
    public class UserProfileModel
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string LastPageVisited { get; set; }

        public bool IsNavOpen { get; set; }

        public bool IsNavMinified { get; set; }

        public int Count { get; set; }

        public DateTime LastUpdatedDate { get; set; }
    }
}