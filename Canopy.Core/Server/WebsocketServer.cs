// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net;
using System.Net.WebSockets;
using System.Text;
using Serilog;
using Synesthesia.Utils.Events;

namespace Canopy.Server;

public class WebsocketServer
{
    private readonly HttpListener listener = new();
    private readonly string path;
    private readonly HashSet<WebSocket> sockets = new();
    private readonly Lock @lock = new();
    private CancellationTokenSource? cts;

    public readonly EventDispatcher<WebSocket> ClientConnected = new EventDispatcher<WebSocket>();

    public WebsocketServer(string path, string url)
    {
        this.path = path;
        listener.Prefixes.Add(url.EndsWith('/') ? url : url + "/");
    }

    public void Start()
    {
        cts = new CancellationTokenSource();
        listener.Start();
        Log.Information("(Websocket) Server listening on {prefix}", string.Join(", ", listener.Prefixes));
        _ = Task.Run(() => acceptLoopAsync(cts.Token));
    }

    public void Stop()
    {
        cts?.Cancel();
        listener.Stop();
    }

    private async Task acceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await listener.GetContextAsync();
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            if (ctx.Request.Url?.AbsolutePath != path || !ctx.Request.IsWebSocketRequest)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.Close();
                continue;
            }

            _ = handleClientAsync(ctx, ct);
        }
    }

    private async Task handleClientAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        WebSocketContext wsCtx;
        try
        {
            wsCtx = await ctx.AcceptWebSocketAsync(null);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "(Websocket) WebSocket handshake failed");
            ctx.Response.StatusCode = 500;
            ctx.Response.Close();
            return;
        }

        var socket = wsCtx.WebSocket;
        lock (@lock) { sockets.Add(socket); }
        Log.Debug("(Websocket) <-> Client connected on {path}", path);
        ClientConnected.Dispatch(socket);

        var buffer = new byte[1024];
        try
        {
            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, ct);
                    break;
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException ex)
        {
            Log.Error(ex, "(Websocket) <-!-> Client disconnected due to error");
        }
        finally
        {
            lock (@lock) { sockets.Remove(socket); }
            socket.Dispose();
            Log.Debug("(Websocket) <-!-> Client disconnected safely");
        }
    }

    public async Task Send(string text)
    {
        WebSocket[] activeSockets;
        lock (@lock)
        {
            activeSockets = sockets.Where(s => s.State == WebSocketState.Open).ToArray();
        }

        if (activeSockets.Length == 0) return;

        var bytes = Encoding.UTF8.GetBytes(text);
        var sendTasks = activeSockets.Select(socket => Task.Run(async () =>
        {
            try
            {
                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                Log.Warning(ex, "(Websocket) -> Send failed, client likely disconnected");
            }
        }));

        await Task.WhenAll(sendTasks);
    }
}
