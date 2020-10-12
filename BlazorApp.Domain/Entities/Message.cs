using System;

namespace BlazorApp.Domain.Entities
{
    public class Message<T> : Entity<T> where T : struct, IEquatable<T>
    {
        public string UserName { get; set; }

        public string Text { get; set; }

        public DateTime When { get; set; }

        public T UserId { get; set; }

        public virtual ApplicationUser<T> Sender { get; set; }
    }
}