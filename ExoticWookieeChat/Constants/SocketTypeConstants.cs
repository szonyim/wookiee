using System;

namespace ExoticWookieeChat.Constants
{
    /// <summary>
    /// Contains all socket types
    /// </summary>
    public class SocketTypeConstants
    {
        /// <summary>
        /// Message exchange type | Default
        /// </summary>
        public const String MESSAGE_EXCHANGE = "messageExchange";

        /// <summary>
        /// Command socket type. 
        /// </summary>
        public const String COMMAND_LISTENER = "commandListener";
    }
}