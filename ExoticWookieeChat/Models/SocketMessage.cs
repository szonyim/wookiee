using System;

namespace ExoticWookieeChat.Models
{
    /// <summary>
    /// Represent a socket message
    /// </summary>
    public class SocketMessage
    {
        public const String SENDER_CUSTOMER = "customer";
        public const String SENDER_SUPPORT = "support";
        public const String SENDER_SYSTEM = "system";

        public const String COMMAND_RELOAD_CONVERSTION_LIST = "reloadConversationList";
        public const String COMMAND_RELOAD_SUPPORT_INTREFACE = "reloadSupportInterface";
        public const String COMMAND_ALERT_NEW_CONVERSATION = "alertNewIncomingConversation";
        public const String COMMAND_ADD_NEW_CONVERSATION = "addNewConversation";

        /// <summary>
        /// Types of SocketMessage
        /// </summary>
        public enum Types { Normal = 0, System = 1, Command = 2 }

        /// <summary>
        /// Content of message
        /// </summary>
        public String Content { get; set; }

        /// <summary>
        /// Command that cusomter interface will be execute
        /// </summary>
        public String Command { get; set; }

        /// <summary>
        /// Message sender
        /// </summary>
        public String Sender { get; set; }

        /// <summary>
        /// Type of socket message
        /// </summary>
        public byte Type { get; set; }
    }
}