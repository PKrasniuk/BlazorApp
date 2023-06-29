using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Domain.Entities;

public class ApplicationUser<T> : IdentityUser<T> where T : struct, IEquatable<T>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName { get; set; }

    public virtual ICollection<ApiLogItem<T>> ApiLogItems { get; set; }

    public virtual UserProfile<T> Profile { get; set; }

    public virtual ICollection<Message<T>> Messages { get; set; }
}