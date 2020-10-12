using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BlazorApp.Common.Models.Security
{
    public static class ApplicationPermissions
    {
        public static readonly ReadOnlyCollection<ApplicationPermission> AllPermissions;

        static ApplicationPermissions()
        {
            var allPermissions = new List<ApplicationPermission>();

            IEnumerable<object> permissionClasses = typeof(Permissions)
                .GetNestedTypes(BindingFlags.Static | BindingFlags.Public).Cast<TypeInfo>();
            foreach (TypeInfo permissionClass in permissionClasses)
            {
                var permissions = permissionClass.DeclaredFields.Where(f => f.IsLiteral);
                foreach (var permission in permissions)
                {
                    var applicationPermission = new ApplicationPermission
                    {
                        Value = permission.GetValue(null).ToString(),
                        Name = permission.GetValue(null).ToString().Replace('.', ' '),
                        GroupName = permissionClass.Name
                    };

                    var attributes =
                        (DescriptionAttribute[])permission.GetCustomAttributes(typeof(DescriptionAttribute), false);

                    applicationPermission.Description =
                        attributes.Length > 0 ? attributes[0].Description : applicationPermission.Name;

                    allPermissions.Add(applicationPermission);
                }
            }

            AllPermissions = allPermissions.AsReadOnly();
        }

        public static ApplicationPermission GetPermissionByName(string permissionName)
        {
            return AllPermissions.FirstOrDefault(p => p.Name == permissionName);
        }

        public static ApplicationPermission GetPermissionByValue(string permissionValue)
        {
            return AllPermissions.FirstOrDefault(p => p.Value == permissionValue);
        }

        public static string[] GetAllPermissionValues()
        {
            return AllPermissions.Select(p => p.Value).ToArray();
        }

        public static string[] GetAllPermissionNames()
        {
            return AllPermissions.Select(p => p.Name).ToArray();
        }

        public static string[] GetAdministrativePermissionValues()
        {
            return GetAllPermissionNames();
        }
    }
}