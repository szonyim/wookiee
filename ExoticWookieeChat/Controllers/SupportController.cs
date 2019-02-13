using ExoticWookieeChat.Constants;
using ExoticWookieeChat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ExoticWookieeChat.Controllers
{
    [Authorize(Roles = UserRoleConstants.ROLE_EMPLOYEE)]
    public class SupportController : Controller
    {
        private readonly DataContext _dataContext;

        public SupportController()
        {
            _dataContext = DataContext.CreateContext();
        }

        /// <summary>
        /// Support welcome page
        /// List conversations
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            String filter = Request.QueryString.Get("State") ?? ConversationConstants.STATE_NEW;

            List<Conversation> conversations;
            switch (filter)
            {
                case ConversationConstants.STATE_NEW:
                    conversations = _dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.New).ToList();
                    break;

                case ConversationConstants.STATE_IN_PROGRESS:
                    conversations = _dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.InProgress).ToList();
                    break;

                case ConversationConstants.STATE_CLOSED:
                    conversations = _dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.Closed).ToList();
                    break;

                default:
                    conversations = _dataContext.Conversations.ToList();
                    break;
            }

            ViewBag.Filter = filter;
            ViewBag.Title = "Conversation list";
            ViewBag.Conversations = conversations.Reverse<Conversation>();

            return View();
        }

        /// <summary>
        /// Return with one serialized Conversation object or an empty string
        /// </summary>
        /// <returns>Json string</returns>
        public string GetConversation()
        {
            int conversationId = Convert.ToInt32(Request.QueryString.Get("ConversationId"));
            Conversation conversation = _dataContext.Conversations.Find(conversationId);
            return conversation == null ? "" : Serialize(conversation);
        }

        /// <summary>
        /// Return with serialized conversation list
        /// </summary>
        /// <returns></returns>
        public string GetConversationList()
        {
            String filter = Request.QueryString.Get("State") ?? ConversationConstants.STATE_NEW;

            List<Conversation> conversations;
            switch (filter)
            {
                case ConversationConstants.STATE_NEW:
                    conversations = _dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.New).OrderByDescending(c => c.Id).ToList();
                    break;

                case ConversationConstants.STATE_IN_PROGRESS:
                    conversations = _dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.InProgress).OrderByDescending(c => c.Id).ToList();
                    break;

                case ConversationConstants.STATE_CLOSED:
                    conversations = _dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.Closed).OrderByDescending(c => c.Id).ToList();
                    break;

                default:
                    conversations = _dataContext.Conversations.ToList();
                    break;
            }

            return Serialize(conversations);
        }

        /// <summary>
        /// Remove one selected conversation from database
        /// </summary>
        /// <returns>Result of action</returns>
        [HttpGet]
        public string RemoveConversation()
        {
            IDictionary<string, string> response = new Dictionary<string, string>();

            try { 
                int conversationId = Convert.ToInt32(Request.QueryString.Get("ConversationId"));

                using(DataContext dataContext = DataContext.CreateContext())
                {
                    Conversation conversation = dataContext.Conversations.Find(conversationId);
                    if (conversation != null)
                    {
                        dataContext.Conversations.Remove(conversation);
                        dataContext.SaveChanges();
                        response.Add("result", "success");
                    }
                }
            } catch(Exception) {
                response.Add("result", "error");
            }

            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// Load selected conversation on support side
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public string LoadConversation()
        {
            int conversationId = Convert.ToInt32(Request.QueryString.Get("ConversationId"));
            using (var dataContext = DataContext.CreateContext())
            {
                Conversation conversation = dataContext.Conversations.Find(conversationId);
                if(conversation != null && conversation.Messages.Any())
                {
                    var responseData = new Dictionary<string, object>();
                    responseData.Add("conversation", conversation);
                    responseData.Add("messages", conversation.Messages);
                    
                    var serializedMessages = JsonConvert.SerializeObject(
                        responseData, 
                        Formatting.None,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }
                    );

                    return serializedMessages;
                }
            }

            return null;
        }

        /// <summary>
        /// Serialize an object without reference loop
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Serialized object</returns>
        private static string Serialize(object data)
        {
            var serializedData = JsonConvert.SerializeObject(
                data,
                Formatting.None,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }
            );

            return serializedData;
        }
    }
}