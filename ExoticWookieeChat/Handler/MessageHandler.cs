using System;
using System.Web;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.WebSockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

using ExoticWookieeChat.Connections;
using ExoticWookieeChat.Constants;
using ExoticWookieeChat.Models;

namespace ExoticWookieeChat.Handler
{
    /// <summary>
    /// Handle all incoming socket message
    /// </summary>
	public class MessageHandler : IHttpHandler
	{
        /// <summary>
        /// List of all WebSocket connection
        /// </summary>
        private static readonly IList<WebSocketConnection> Connections = new List<WebSocketConnection>();

        /// <summary>
        /// Locker instance
        /// </summary>
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        /// <summary>
        /// DataContext instance
        /// </summary>
        private DataContext dataContext;

        public MessageHandler()
        {
            this.dataContext = DataContext.CreateContext();
        }

        /// <summary>
        /// Process the request
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                context.AcceptWebSocketRequest(ProcessCommunication);
            }
        }

        /// <summary>
        /// Process the communication between customer and employee
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ProcessCommunication(AspNetWebSocketContext context)
        {
            WebSocketConnection connection;
            bool IsEmployeeConnection = context.User.Identity.IsAuthenticated;

            Guid socketGuid             = Guid.NewGuid();
            Conversation conversation   = null;
            WebSocket socket            = context.WebSocket;

            byte connectionType = IsEmployeeConnection ? (byte)WebSocketConnection.ConnectionTypes.Employee : (byte)WebSocketConnection.ConnectionTypes.Customer;
            String SocketType = context.QueryString.Get("SocketType") ?? SocketTypeConstants.MESSAGE_EXCHANGE;

            if(SocketType == SocketTypeConstants.COMMAND_LISTENER)
            {
                connectionType = (byte)WebSocketConnection.ConnectionTypes.Command;
            }

            // Create connection
            Locker.EnterWriteLock();
            try
            {
                Guid channel = Guid.Empty;
                if(context.QueryString.Get("customerWebSocketGuid") != null)
                {
                    channel = Guid.Parse(context.QueryString.Get("customerWebSocketGuid"));
                    conversation = dataContext.Conversations.FirstOrDefault(c => c.SocketGuid == channel);
                }

                connection = new WebSocketConnection()
                {
                    SocketGuid = socketGuid,
                    WebSocket = socket,
                    ConnectionType = connectionType,
                    Channel = channel
                };

                Connections.Add(connection);

                // Create Conversation if socket type is MESSAGE_EXCHANGE
                if (conversation == null && SocketType == SocketTypeConstants.MESSAGE_EXCHANGE)
                    conversation = CreateConversation(socketGuid);
            }
            finally
            {
                Locker.ExitWriteLock();
            }

            User employee = null;
            if (IsEmployeeConnection)
            {
                String employeeName = context.User.Identity.Name;
                employee = dataContext.Users.FirstOrDefault(e => e.UserName == employeeName);
            }

            bool isFirstMessage = true;
            
            // Listen socket
            for (;;)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (socket.State == WebSocketState.Open)
                {
                    String userMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    SocketMessage socketMessage = JsonConvert.DeserializeObject<SocketMessage>(userMessage);

                    userMessage = JsonConvert.SerializeObject(socketMessage);

                    if(conversation != null)
                    {
                        Message msg = new Message()
                        {
                            Content = socketMessage.Content,
                            Sender = socketMessage.Sender,
                            CreatedAt = DateTime.Now,
                            Conversation = conversation,
                            Employee = employee
                        };

                        dataContext.Messages.Add(msg);
                        await dataContext.SaveChangesAsync();
                    }

                    if (isFirstMessage)
                    {
                        // Notify employees and add new conversation to support interface at first message
                        if (SocketType == SocketTypeConstants.MESSAGE_EXCHANGE && socketMessage.Sender == SocketMessage.SENDER_CUSTOMER)
                        {
                            await NotfiyEmployees();
                            await AddNewConversationToEmployeeInterface(conversation.Id);
                        }
                    }

                    isFirstMessage = false;

                    buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(userMessage));

                    // Employee write to customer
                    if(connection.Channel != Guid.Empty)
                    {
                        foreach(var c in Connections)
                        {
                            if(c.SocketGuid == connection.Channel)
                            {
                                if(conversation.State == (byte)Conversation.States.New)
                                {
                                    conversation.State = (byte)Conversation.States.InProgress;
                                    await dataContext.SaveChangesAsync();
                                }
                                await c.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                    }

                    // Customer write for employee
                    foreach (var c in Connections)
                    {
                        if (connection.SocketGuid == c.Channel)
                        {
                            // Write to recipient
                            await c.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }

                    // Write back the message to the sender
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    // client is no longer available - delete from list
                    Locker.EnterWriteLock();
                    try
                    {
                        CloseConversation(socketGuid);
                        Connections.Remove(connection);
                    }
                    finally
                    {
                        Locker.ExitWriteLock();
                    }

                    break;
                }
            }
        }

        public bool IsReusable { get { return false; } }

        // TODO: implement: if last message time over 30 min, close socket
        public void StartIdleTimer()
        {
            throw new Exception("Not implemented yet!");
        }

        /// <summary>
        /// Notfiy all employees about new incoming conversation
        /// </summary>
        /// <returns></returns>
        public async Task NotfiyEmployees()
        {
            SocketMessage commandMessage = new SocketMessage()
            {
                Command = SocketMessage.COMMAND_ALERT_NEW_CONVERSATION,
                Type = (byte)SocketMessage.Types.Command
            };

            await SendCommandMessage(commandMessage);
        }

        /// <summary>
        /// Indicate user interface to add new conversation to their list
        /// </summary>
        /// <param name="ConversationId"></param>
        /// <returns></returns>
        public async Task AddNewConversationToEmployeeInterface(int ConversationId)
        {
            SocketMessage commandMessage = new SocketMessage()
            {
                Content = ConversationId.ToString(),
                Command = SocketMessage.COMMAND_ADD_NEW_CONVERSATION,
                Type = (byte)SocketMessage.Types.Command
            };

            await SendCommandMessage(commandMessage);
        }

        /// <summary>
        /// Send command message to employees
        /// </summary>
        /// <param name="commandMessage"></param>
        /// <returns></returns>
        private async Task SendCommandMessage(SocketMessage commandMessage)
        {
            if (Connections.Count > 0)
            {
                String commandMessageStr = JsonConvert.SerializeObject(commandMessage);
                ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(commandMessageStr));

                foreach (WebSocketConnection wsc in Connections)
                {
                    if (wsc.ConnectionType == (byte)WebSocketConnection.ConnectionTypes.Command)
                    {
                        await wsc.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }

        /// <summary>
        /// Create new conversation
        /// </summary>
        /// <param name="socketGuid"></param>
        /// <returns></returns>
        private Conversation CreateConversation(Guid socketGuid)
        {
            Conversation conversation = new Conversation()
            {
                SocketGuid = socketGuid,
                State = (byte)Conversation.States.New,
                CreatedAt = DateTime.Now
            };

            dataContext.Conversations.Add(conversation);
            dataContext.SaveChanges();

            return conversation;
        }

        /// <summary>
        /// Set conversation state to closed
        /// </summary>
        /// <param name="socketGuid"></param>
        /// <returns></returns>
        private int CloseConversation(Guid socketGuid)
        {
            int result = 0;
            Conversation conversation = dataContext.Conversations.FirstOrDefault(c => c.SocketGuid == socketGuid);
            if(conversation != null)
            {
                conversation.State = (byte)Conversation.States.Closed;
                result = dataContext.SaveChanges();
            }

            return result;
        }


    }
}