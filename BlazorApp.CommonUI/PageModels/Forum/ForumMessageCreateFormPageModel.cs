using System;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.CommonUI.PageModels.Forum;

public class ForumMessageCreateFormPageModel : ComponentBase
{
    [Parameter] public Func<MessageModel, Task> Send { get; set; }

    protected MessageModel MessageModel { get; set; } = new();

    protected bool Creating { get; set; }

    protected async Task CreateMessage()
    {
        Creating = true;
        StateHasChanged();
        await Send(MessageModel);
        MessageModel.Text = "";
        Creating = false;
        StateHasChanged();
    }
}