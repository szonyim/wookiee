using System;
using System.Net.WebSockets;

namespace ExoticWookieeChat.Connections
{
    /// <summary>
    /// Represent a WebSocket connection with additional parameters
    /// </summary>
    public class WebSocketConnection
    {
        public enum ConnectionTypes : byte { Customer, Employee, Administrator };

        /// <summary>
        /// Guid of connection
        /// </summary>
        public Guid SocketGuid { get; set; }

        /// <summary>
        /// The Websocket
        /// </summary>
        public WebSocket WebSocket { get; set; }

        /// <summary>
        /// Type of connection
        /// </summary>
        public byte ConnectionType { get; set; }

        /// <summary>
        /// Guid of customer socket
        /// </summary>
        public Guid Channel { get; set; }
    }
}