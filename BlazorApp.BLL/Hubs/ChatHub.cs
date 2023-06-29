using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorApp.BLL.Interfaces;
using BlazorApp.Common.Models;
using BlazorApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.BLL.Hubs;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> UserLookup = new();

    private readonly IMessageManager _messageManager;
    private readonly UserManager<ApplicationUser<Guid>> _userManager;

    public ChatHub(IMessageManager messageManager, UserManager<ApplicationUser<Guid>> userManager)
    {
        _messageManager = messageManager;
        _userManager = userManager;
    }

    public async Task DeleteMessage(string id)
    {
        await _messageManager.DeleteMessageAsync(id);
    }

    public async Task SendMessage(string message)
    {
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        var messageModel = new MessageModel
        {
            Text = message,
            UserName = user.UserName,
            UserId = user.Id.ToString(),
            When = DateTime.UtcNow
        };
        await _messageManager.CreateMessageAsync(messageModel);

        await Clients.All.SendAsync("ReceiveMessage", messageModel.Id, messageModel.UserName, messageModel.Text);
    }

    public async Task Register(string username)
    {
        var currentId = Context.ConnectionId;

        if (!UserLookup.ContainsKey(currentId))
        {
            UserLookup.Add(currentId, username);

            await Clients.AllExcept(currentId)
                .SendAsync("ReceiveMessage", string.Empty, username, $"{username} joined the chat");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var apiResponse = await _messageManager.GetMessagesAsync();
        if (apiResponse.Result != null)
            foreach (var message in (IEnumerable<MessageModel>)apiResponse.Result)
                await Clients.Client(Context.ConnectionId)
                    .SendAsync("ReceiveMessage", message.Id, message.UserName, message.Text);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception e)
    {
        var id = Context.ConnectionId;
        if (UserLookup.TryGetValue(id, out var username))
        {
            UserLookup.Remove(id);
            await Clients.AllExcept(Context.ConnectionId)
                .SendAsync("ReceiveMessage", string.Empty, username, $"{username} has left the chat");
        }

        await base.OnDisconnectedAsync(e);
    }
}