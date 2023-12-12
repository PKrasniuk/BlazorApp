using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using MatBlazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace BlazorApp.CommonUI.PageModels.Admin;

[Authorize]
public class RolesPageModel : ComponentBase
{
    protected readonly List<PermissionSelection> PermissionsSelections = new();

    private bool _isInsertOperation;
    [Inject] private HttpClient Http { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    protected string CurrentRoleName { get; set; }

    protected bool IsCurrentRoleReadOnly { get; set; }

    protected bool IsUpsertRoleDialogOpen { get; set; }

    protected string LabelUpsertDialogTitle { get; set; }

    protected string LabelUpsertDialogOkButton { get; set; }

    protected bool IsDeleteRoleDialogOpen { get; set; }

    protected List<RoleModel> Roles { get; set; }

    private int PageSize { get; } = 10;

    private int CurrentPage { get; } = 0;

    protected override async Task OnInitializedAsync()
    {
        await InitializeRolesListAsync();
    }

    protected async Task OpenUpsertRoleDialogAsync(string roleName = "")
    {
        try
        {
            CurrentRoleName = roleName;
            _isInsertOperation = string.IsNullOrWhiteSpace(roleName);

            if (_isInsertOperation)
            {
                LabelUpsertDialogTitle = "Create Role";
                LabelUpsertDialogOkButton = "Create Role";
            }
            else
            {
                LabelUpsertDialogTitle = "Edit Role";
                LabelUpsertDialogOkButton = "Update Role";
            }

            RoleModel role = null;
            IsCurrentRoleReadOnly = !_isInsertOperation;

            if (IsCurrentRoleReadOnly)
            {
                var roleResponse = await Http.GetFromJsonAsync<ApiResponse>($"api/Admin/Role/{roleName}");
                role = (RoleModel)roleResponse.Result;
            }

            var response = await Http.GetFromJsonAsync<ApiResponse>("api/Admin/Permissions");
            PermissionsSelections.Clear();


            foreach (var name in JsonConvert.DeserializeObject<string[]>(response.Result.ToString() ?? string.Empty)
                         .ToList())
                PermissionsSelections.Add(new PermissionSelection
                {
                    Name = name,
                    IsSelected = role?.Permissions.Contains(name) ?? false
                });

            IsUpsertRoleDialogOpen = true;
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, $"{LabelUpsertDialogTitle} Error");
        }
    }

    protected async Task UpsertRoleAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CurrentRoleName))
            {
                MatToaster.Add("Role Creation Error", MatToastType.Danger, "Enter in a Role Name");
                return;
            }

            var request = new RoleModel
            {
                Name = CurrentRoleName,
                Permissions = new List<string>()
            };

            foreach (var status in PermissionsSelections.Where(status => status.IsSelected))
                request.Permissions.Add(status.Name);

            HttpResponseMessage apiResponse;

            if (_isInsertOperation)
                apiResponse = await Http.PostAsJsonAsync("api/Admin/Role", request);
            else
                apiResponse = await Http.PutAsJsonAsync("api/Admin/Role", request);

            if (apiResponse.StatusCode == HttpStatusCode.OK)
            {
                MatToaster.Add(_isInsertOperation ? "Role Created" : "Role Updated", MatToastType.Success);
                StateHasChanged();
            }
            else
            {
                MatToaster.Add(apiResponse.ReasonPhrase, MatToastType.Danger);
            }

            await OnInitializedAsync();
            IsUpsertRoleDialogOpen = false;
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "Role Operation Error");
        }
    }

    protected void OpenDeleteDialog(string roleName)
    {
        CurrentRoleName = roleName;
        IsDeleteRoleDialogOpen = true;
    }

    protected async Task DeleteRoleAsync()
    {
        try
        {
            var response = await Http.DeleteAsync($"api/Admin/Role/{CurrentRoleName}");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MatToaster.Add("Role Delete Failed", MatToastType.Danger);
                return;
            }

            MatToaster.Add("Role Deleted", MatToastType.Success);
            await OnInitializedAsync();
            IsDeleteRoleDialogOpen = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "Role Delete Error");
        }
    }

    private async Task InitializeRolesListAsync()
    {
        try
        {
            var apiResponse =
                await Http.GetFromJsonAsync<ApiResponse>(
                    $"api/Admin/Roles?pageSize={PageSize}&pageNumber={CurrentPage}");

            if (apiResponse.StatusCode == (int)HttpStatusCode.OK)
            {
                MatToaster.Add(apiResponse.Message, MatToastType.Success, "Roles Retrieved");
                Roles = JsonConvert.DeserializeObject<RoleModel[]>(apiResponse.Result.ToString()).ToList();
            }
            else
            {
                MatToaster.Add($"{apiResponse.Message} : {apiResponse.StatusCode}", MatToastType.Danger,
                    "Roles Retrieval Failed");
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.GetBaseException().Message, MatToastType.Danger, "Roles Retrieval Error");
        }
    }

    protected class PermissionSelection
    {
        public bool IsSelected { get; set; }

        public string Name { get; set; }
    }
}