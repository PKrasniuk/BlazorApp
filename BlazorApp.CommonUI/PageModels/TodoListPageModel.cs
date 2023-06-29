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
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BlazorApp.CommonUI.PageModels;

public class TodoListPageModel : ComponentBase
{
    [Inject] private HttpClient Http { get; set; }

    [Inject] private IMatToaster MatToaster { get; set; }

    protected bool DeleteDialogOpen { get; set; }

    protected bool DialogIsOpen { get; set; }

    protected List<TodoModel> TodoModelList { get; set; } = new();

    protected TodoModel Todo { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await ReadAsync();
    }

    private async Task ReadAsync()
    {
        var apiResponse = await Http.GetFromJsonAsync<ApiResponse>("api/todo");
        if (apiResponse.StatusCode == StatusCodes.Status200OK)
        {
            MatToaster.Add(apiResponse.Message, MatToastType.Success, "Todo List Retrieved");
            TodoModelList = JsonConvert
                .DeserializeObject<TodoModel[]>(apiResponse.Result.ToString() ??
                                                throw new InvalidOperationException()).ToList();
        }
        else
        {
            MatToaster.Add($"{apiResponse.Message} : {apiResponse.StatusCode}", MatToastType.Danger,
                "Todo List Retrieval Failed");
        }
    }

    protected async void UpdateAsync(TodoModel todo)
    {
        try
        {
            todo.IsCompleted = !todo.IsCompleted;
            var apiResponse = await Http.PostAsJsonAsync("api/todo", todo);

            if (apiResponse.IsSuccessStatusCode)
            {
                MatToaster.Add(apiResponse.ReasonPhrase, MatToastType.Success);
            }
            else
            {
                MatToaster.Add($"{apiResponse.ReasonPhrase}: {apiResponse.StatusCode}", MatToastType.Danger,
                    "Todo Save Failed");
                todo.IsCompleted = !todo.IsCompleted;
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.Message, MatToastType.Danger, "Todo Save Failed");
            todo.IsCompleted = !todo.IsCompleted;
        }
    }

    protected async Task DeleteAsync()
    {
        try
        {
            var response = await Http.DeleteAsync("api/todo/" + Todo.Id);
            if (response.StatusCode == (HttpStatusCode)StatusCodes.Status200OK)
            {
                MatToaster.Add("Todo Deleted", MatToastType.Success);
                TodoModelList.Remove(Todo);
            }
            else
            {
                MatToaster.Add($"Todo Delete Failed: {response.StatusCode}", MatToastType.Danger);
            }

            DeleteDialogOpen = false;
            Todo = new TodoModel();
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.Message, MatToastType.Danger, "Todo Save Failed");
        }
    }

    protected void OpenDialog()
    {
        DialogIsOpen = true;
    }

    protected void OpenDeleteDialog(string todoId)
    {
        Todo = TodoModelList.SingleOrDefault(x => x.Id == todoId);
        DeleteDialogOpen = true;
    }

    protected async Task CreateAsync()
    {
        DialogIsOpen = false;
        try
        {
            var apiResponse = await Http.PostAsJsonAsync("api/todo", Todo);
            if (apiResponse.IsSuccessStatusCode)
            {
                MatToaster.Add(apiResponse.ReasonPhrase, MatToastType.Success);
                Todo = JsonConvert.DeserializeObject<TodoModel>(await apiResponse.Content.ReadAsStringAsync());
                TodoModelList.Add(Todo);
                Todo = new TodoModel();
            }
            else
            {
                MatToaster.Add($"{apiResponse.ReasonPhrase} : {apiResponse.StatusCode}", MatToastType.Danger,
                    "Todo Creation Failed");
            }
        }
        catch (Exception ex)
        {
            MatToaster.Add(ex.Message, MatToastType.Danger, "Todo Creation Failed");
        }
    }
}