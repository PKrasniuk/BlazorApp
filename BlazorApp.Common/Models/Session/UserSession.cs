using BlazorApp.Common.Interfaces;
using BlazorApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace BlazorApp.Common.Models.Session
{
    public class UserSession<T> : Entity<T>, IUserSession<T> where T : struct, IEquatable<T>
    {
        public UserSession()
        {
        }

        public UserSession(IdentityUser<T> user)
        {
            UserId = user.Id;
            UserName = user.UserName;
        }

        public bool IsAuthenticated { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<KeyValuePair<string, string>> ExposedClaims { get; set; }

        public T UserId { get; set; }

        public string UserName { get; set; }

        public List<string> Roles { get; set; }
    }
}