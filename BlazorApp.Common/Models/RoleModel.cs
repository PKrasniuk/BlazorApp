using System.Collections.Generic;

namespace BlazorApp.Common.Models
{
    public class RoleModel
    {
        public string Name { get; set; }

        public List<string> Permissions { get; set; }

        public string FormattedPermissions => string.Join(", ", Permissions.ToArray());
    }
}