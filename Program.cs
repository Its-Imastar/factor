using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[Route("api/proxy")]
public class ProxyController : ControllerBase
{
    private readonly string _targetUrl = "wss://example.com/socket"; // Replace with your game's WebSocket server URL

    [HttpGet("websocket")]
    public async Task WebSocketProxy(CancellationToken cancellationToken)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        // Accept WebSocket connection
        using (WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            using (var clientSocket = new ClientWebSocket())
            {
                // Connect to the target WebSocket server
                await clientSocket.ConnectAsync(new Uri(_targetUrl), cancellationToken);

                // Start receiving messages from the client
                var buffer = new byte[1024 * 4];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                        break;
                    }

                    // Forward the client message to the server
                    await clientSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, cancellationToken);

                    // Read messages from the server and forward to client
                    var serverBuffer = new byte[1024 * 4];
                    var serverResult = await clientSocket.ReceiveAsync(new ArraySegment<byte>(serverBuffer), cancellationToken);
                    await webSocket.SendAsync(new ArraySegment<byte>(serverBuffer, 0, serverResult.Count), serverResult.MessageType, serverResult.EndOfMessage, cancellationToken);
                }
            }
        }
    }
}
