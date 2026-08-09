// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Server.Messages;
using Serilog;

namespace Canopy.Server;

public class CanopyWebsocketServer
{
    private readonly WebsocketServer websocketServer = new WebsocketServer("/update", Canopy.CurrentConfig.Websocket.Url);
    private ISocketMessage? lastMessage;

    public void Initialize()
    {
        websocketServer.Start();

        websocketServer.ClientConnected.Subscribe(_ =>
        {
            if (lastMessage != null)
                Send(lastMessage);
        });
    }

    public void Send(ISocketMessage message)
    {
        lastMessage = message;
        Task.Run(() => websocketServer.Send(message.Encode()));
        Log.Information("(WebSocket) -> Sent message {message} to websocket", message.GetType().Name);
    }

    public void Stop()
    {
        websocketServer.Stop();
    }
}
