﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TikTokLiveSharp.Client.Requests;

namespace TikTokLiveSharp.Client.Sockets
{
    /// <summary>
    /// WebSocket-Connection to TikTok-servers
    /// </summary>
    public sealed class TikTokWebSocket : ITikTokWebSocket
    {
        /// <summary>
        /// WebSocket-Client
        /// </summary>
        private ClientWebSocket clientWebSocket;
        /// <summary>
        /// Size for Messages
        /// <para>
        /// Should be large enough to be able to read a message to the end
        /// </para>
        /// </summary>
        private uint bufferSize;

        /// <summary>
        /// Creates a TikTok WebSocket instance
        /// </summary>
        /// <param name="cookieContainer">The cookie container to use</param>
        /// <param name="buffSize">Size for Message-Buffer</param>
        public TikTokWebSocket(TikTokCookieJar cookieContainer, uint buffSize = 500_000)
        {
            bufferSize = buffSize;
            clientWebSocket = new ClientWebSocket();
            clientWebSocket.Options.AddSubProtocol("echo-protocol");
            clientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            StringBuilder cookieHeader = new StringBuilder();
            foreach (string cookie in cookieContainer)
                cookieHeader.Append(cookie);
            clientWebSocket.Options.SetRequestHeader("Cookie", cookieHeader.ToString());
        }

        /// <summary>
        /// Connects to the websocket
        /// </summary>
        /// <param name="url">Websocket url</param>
        /// <returns>Task to await</returns>
        public async Task Connect(string url)
        {
            await clientWebSocket.ConnectAsync(new Uri(url), CancellationToken.None);
        }

        /// <summary>
        /// Disconnects from the websocket
        /// </summary>
        /// <returns>Task to await</returns>
        public async Task Disconnect()
        {
            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        /// <summary>
        /// Writes a message to the websocket
        /// </summary>
        /// <param name="arr">The bytes to write</param>
        /// <returns>Task to await</returns>
        public async Task WriteMessage(ArraySegment<byte> arr)
        {
            await clientWebSocket.SendAsync(arr, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>
        /// Recieves a message from websocket
        /// </summary>
        /// <returns></returns>
        public async Task<TikTokWebSocketResponse> ReceiveMessage()
        {
            var arr = new ArraySegment<byte>(new byte[bufferSize]);
            WebSocketReceiveResult response = await clientWebSocket.ReceiveAsync(arr, CancellationToken.None);
            if (response.MessageType == WebSocketMessageType.Binary)
                return new TikTokWebSocketResponse(arr.Array, response.Count);
            return null;
        }

        /// <summary>
        /// Is the websocket currently connected?
        /// </summary>
        public bool IsConnected => clientWebSocket.State == WebSocketState.Open;
    }
}