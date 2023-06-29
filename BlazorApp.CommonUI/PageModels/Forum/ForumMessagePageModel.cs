using System;
using System.Threading.Tasks;
using BlazorApp.Common.Models;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.CommonUI.PageModels.Forum;

public class ForumMessagePageModel : ComponentBase
{
    [Parameter] public Func<MessageModel, Task> Delete { get; set; }

    [Parameter] public MessageModel Message { get; set; }

    protected MessageModel MessageModel { get; set; }

    protected override void OnParametersSet()
    {
        MessageModel = Message;
    }

    protected async Task DeleteMessage()
    {
        await Delete(MessageModel);
    }
}