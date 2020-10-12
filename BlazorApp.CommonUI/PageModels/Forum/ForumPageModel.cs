using BlazorApp.Common.Models;
using BlazorApp.CommonUI.Hubs;
using BlazorApp.CommonUI.Services.Implementations;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp.CommonUI.PageModels.Forum
{
    public class ForumPageModel : ComponentBase
    {
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject] public IJSRuntime JsRuntime { get; set; }

        [Inject] public IMatToaster MatToaster { get; set; }

        [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        private UserInfoModel UserInfo { get; set; } = new UserInfoModel();

        private ChatClient Client { get; set; }

        protected List<MessageModel> Messages = new List<MessageModel>();

        protected override async Task OnInitializedAsync()
        {
            UserInfo = await ((IdentityAuthenticationStateProvider)AuthStateProvider).GetUserInfo();

            await AuthenticationStateTask;
            await Chat();
        }

        private async Task Chat()
        {
            try
            {
                Messages.Clear();
                Client = new ChatClient(JsRuntime);
                Client.MessageReceived += MessageReceived;
                await Client.Start();
            }
            catch (Exception)
            {
                MatToaster.Add("Failed to start chat client", MatToastType.Danger, "Failed To Start Chat");
            }
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var isMine = false;
            if (!string.IsNullOrWhiteSpace(e.Username))
            {
                isMine = string.Equals(e.Username, UserInfo.UserName, StringComparison.CurrentCultureIgnoreCase);
            }

            var newMessage = new MessageModel(e.Id, e.Username, e.Message, isMine);
            Messages.Insert(0, newMessage);

            StateHasChanged();
        }

        public async Task Disconnect()
        {
            await Client.Stop();
            Client.Dispose();
            Client = null;
            MatToaster.Add("Chat Ended", MatToastType.Info, "Chat Ended");
        }

        public async Task Delete(MessageModel messageModel)
        {
            if (messageModel != null)
            {
                await Client.Delete(messageModel.Id);
                Messages.Remove(messageModel);
                StateHasChanged();
            }
        }

        public async Task Send(MessageModel messageModel)
        {
            if (!string.IsNullOrWhiteSpace(messageModel.Text))
            {
                await Client.Send(messageModel.Text);
                StateHasChanged();
            }
        }
    }
}