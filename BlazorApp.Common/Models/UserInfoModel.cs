using System.Collections.Generic;

namespace BlazorApp.Common.Models;

public class UserInfoModel
{
    public bool IsAuthenticated { get; set; }

    public string UserId { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public List<string> Roles { get; set; }

    public List<KeyValuePair<string, string>> ExposedClaims { get; set; }
}