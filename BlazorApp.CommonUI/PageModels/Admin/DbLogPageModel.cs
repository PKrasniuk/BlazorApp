﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Models.Security;
using BlazorApp.Common.Wrappers;
using MatBlazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace BlazorApp.CommonUI.PageModels.Admin;

[Authorize(Policy = Policies.IsAdmin)]
public class DbLogPageModel : ComponentBase
{
    protected List<DbLogModel> DbLogItems = new();

    protected string MatfabAnimateClass = string.Empty;
    [Inject] private HttpClient Http { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    protected ApiResponse ApiResponse { get; set; }

    protected int PageSize { get; set; } = 10;

    private int PageIndex { get; set; }

    protected int LogCountTotal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    protected async Task OnPage(MatPaginatorPageEvent e)
    {
        PageSize = e.PageSize;
        PageIndex = e.PageIndex;

        await LoadData();
    }

    protected async Task LoadData()
    {
        MatfabAnimateClass = string.IsNullOrWhiteSpace(MatfabAnimateClass) ? "mat-fab-animate" : string.Empty;
        ApiResponse =
            await Http.GetFromJsonAsync<ApiResponse>($"api/DbLog?page={PageIndex}&pageSize={PageSize}");

        switch (ApiResponse.StatusCode)
        {
            case (int)HttpStatusCode.OK:
            {
                var nextPage = JsonConvert
                    .DeserializeObject<DbLogModel[]>(ApiResponse.Result.ToString()).ToList();
                LogCountTotal = ApiResponse.PaginationDetails.CollectionSize ?? LogCountTotal;
                DbLogItems = nextPage;
                break;
            }
            case (int)HttpStatusCode.NoContent:
                MatToaster.Add(string.Empty, MatToastType.Info, "No more logs to fetch");
                break;
            default:
                MatToaster.Add(ApiResponse.Message, MatToastType.Danger, "DB Log Items Retrieval Failed");
                break;
        }

        await InvokeAsync(StateHasChanged);
    }
}