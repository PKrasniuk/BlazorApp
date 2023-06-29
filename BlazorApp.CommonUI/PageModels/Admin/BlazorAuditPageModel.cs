using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using BlazorApp.Common.Wrappers;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BlazorApp.CommonUI.PageModels.Admin;

public class BlazorAuditPageModel : ComponentBase
{
    [Inject] private HttpClient Http { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    protected List<ApiLogItemModel> ApiLogItems { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    protected async Task LoadDataAsync()
    {
        var apiResponse = await Http.GetFromJsonAsync<ApiResponse>("api/apilog");

        if (apiResponse.StatusCode == StatusCodes.Status200OK)
        {
            MatToaster.Add(apiResponse.Message, MatToastType.Success, "Api Log Items Retrieved");
            ApiLogItems = JsonConvert.DeserializeObject<ApiLogItemModel[]>(apiResponse.Result.ToString()).ToList();
        }
        else
        {
            MatToaster.Add(apiResponse.Message, MatToastType.Danger, "Api Log Items Retrieval Failed");
        }
    }
}