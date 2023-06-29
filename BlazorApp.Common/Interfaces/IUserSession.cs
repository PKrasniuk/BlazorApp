using System.Collections.Generic;

namespace BlazorApp.Common.Interfaces;

public interface IUserSession<T>
{
    T UserId { get; set; }

    List<string> Roles { get; set; }

    string UserName { get; set; }
}