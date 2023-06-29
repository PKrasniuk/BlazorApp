using Microsoft.AspNetCore.Components;

namespace BlazorApp.CommonUI.SharedModels.Components;

public class LoadingBackgroundModel : ComponentBase
{
    [Parameter] public bool ShowLogoBox { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }
}