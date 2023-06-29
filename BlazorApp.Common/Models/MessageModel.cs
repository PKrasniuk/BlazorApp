using System;

namespace BlazorApp.Common.Models;

public class MessageModel
{
    public MessageModel()
    {
    }

    public MessageModel(string id, string userName, string text, bool mine)
    {
        Id = id;
        UserName = userName;
        Text = text;
        Mine = mine;
    }

    public string Id { get; set; }

    public string UserName { get; set; }

    public string Text { get; set; }

    public DateTime When { get; set; }

    public string UserId { get; set; }

    public bool Mine { get; set; }

    /// <summary>
    ///     Determine CSS classes to use for message div
    /// </summary>
    public string CSS => Mine ? "sent" : "received";
}