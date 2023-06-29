using System;

namespace BlazorApp.CommonUI.Hubs;

public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

public class MessageReceivedEventArgs : EventArgs
{
    public MessageReceivedEventArgs(string id, string username, string message)
    {
        Id = id;
        Username = username;
        Message = message;
    }

    public string Username { get; }

    public string Message { get; }

    public string Id { get; }
}