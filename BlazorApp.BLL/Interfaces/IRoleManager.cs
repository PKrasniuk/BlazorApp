using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using System.Threading.Tasks;

namespace BlazorApp.BLL.Interfaces
{
    public interface IRoleManager
    {
        Task<ApiResponse> GetPermissionsAsync();

        Task<ApiResponse> GetRolesAsync(int pageSize = 10, int pageNumber = 0);

        Task<ApiResponse> GetRoleAsync(string roleName);

        Task<ApiResponse> CreateRoleAsync(RoleModel roleModel);

        Task<ApiResponse> UpdateRoleAsync(RoleModel roleModel);

        Task<ApiResponse> DeleteRoleAsync(string roleName);
    }
}