﻿using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.PageModels.Account
{
    public class RegisterPageModel : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject] private IMatToaster MatToaster { get; set; }

        protected RegisterModel RegisterParameters { get; } = new RegisterModel();

        protected async Task RegisterUserAsync()
        {
            try
            {
                var response = await ((IdentityAuthenticationStateProvider)AuthStateProvider).Register(RegisterParameters);
                if (response.StatusCode == StatusCodes.Status200OK)
                {
                    MatToaster.Add($"New User Email Verification was sent to: {RegisterParameters.Email}", MatToastType.Success, "User Creation Successful");
                    NavigationManager.NavigateTo("");
                }
                else
                {
                    MatToaster.Add(response.Message, MatToastType.Danger, "User Creation Failed");
                }
            }
            catch (Exception ex)
            {
                MatToaster.Add(ex.Message, MatToastType.Danger, "User Creation Failed");
            }
        }
    }
}