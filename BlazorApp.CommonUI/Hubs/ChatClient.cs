using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorApp.CommonUI.Hubs;

public class ChatClient : IDisposable
{
    private static readonly Dictionary<string, ChatClient> Clients = new();

    private readonly IJSRuntime _jsruntime;

    private readonly string _key;

    private readonly string _username;

    private bool _started;

    public ChatClient(IJSRuntime jsruntime, string username = "")
    {
        _jsruntime = jsruntime;
        _username = username;
        _key = Guid.NewGuid().ToString();
        Clients.Add(_key, this);
    }

    public void Dispose()
    {
        if (_started)
            Task.Run(async () => { await Stop(); }).Wait();

        if (Clients.ContainsKey(_key))
            Clients.Remove(_key);
    }

    [JSInvokable]
    public static void ReceiveMessage(string key, string method, string id, string username, string message)
    {
        if (Clients.ContainsKey(key))
        {
            var client = Clients[key];
            switch (method)
            {
                case "ReceiveMessage":
                    client.HandleReceiveMessage(id, username, message);
                    return;
                default:
                    throw new NotImplementedException(method);
            }
        }
    }

    public async Task Start()
    {
        if (!_started)
        {
            var _ = await _jsruntime.InvokeAsync<object>("ChatClient.Start", _key, "/chathub", "BlazorApp.CommonUI",
                "ReceiveMessage");
            _started = true;
            await _jsruntime.InvokeAsync<object>("ChatClient.Register", _key, _username);
        }
    }

    private void HandleReceiveMessage(string id, string username, string message)
    {
        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(id, username, message));
    }

    public event MessageReceivedEventHandler MessageReceived;

    public async Task Send(string message)
    {
        if (!_started)
            throw new InvalidOperationException("Client not started");
        await _jsruntime.InvokeAsync<object>("ChatClient.Send", _key, message);
    }

    public async Task Delete(string id)
    {
        if (!_started)
            throw new InvalidOperationException("Client not started");
        await _jsruntime.InvokeAsync<object>("ChatClient.Delete", _key, id);
    }

    public async Task Stop()
    {
        if (_started)
        {
            await _jsruntime.InvokeAsync<object>("ChatClient.Stop", _key);
            _started = false;
        }
    }
}