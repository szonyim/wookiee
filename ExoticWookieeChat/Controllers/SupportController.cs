using ExoticWookieeChat.Constants;
using ExoticWookieeChat.Models;
using ExoticWookieeChat.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExoticWookieeChat.Controllers
{
    [Authorize(Roles = UserRoleConstants.ROLE_EMPLOYEE)]
    public class SupportController : Controller
    {
        private readonly DataContext dataContext;

        public SupportController()
        {
            dataContext = DataContext.CreateContext();
        }

        /// <summary>
        /// Support welcome page
        /// List conversations
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            String filter = Request.QueryString.Get("State") ?? ConversationConstants.STATE_NEW;

            List<Conversation> Conversations = null;
            switch (filter)
            {
                case ConversationConstants.STATE_NEW:
                    Conversations = dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.New).ToList<Conversation>();
                    break;

                case ConversationConstants.STATE_IN_PROGRESS:
                    Conversations = dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.InProgress).ToList<Conversation>();
                    break;

                case ConversationConstants.STATE_CLOSED:
                    Conversations = dataContext.Conversations.Where(c => c.State == (byte)Conversation.States.Closed).ToList<Conversation>();
                    break;

                default:
                    Conversations = dataContext.Conversations.ToList<Conversation>();
                    break;
            }

            ViewBag.Filter = filter;
            ViewBag.Title = "Conversation list";
            ViewBag.Conversations = Conversations;

            return View();
        }

        [HttpGet]
        public String RemoveConversation()
        {
            IDictionary<string, string> response = new Dictionary<string, string>();

            try { 
                int conversationId = Convert.ToInt32(Request.QueryString.Get("ConversationId"));

                using(DataContext dataContext = DataContext.CreateContext())
                {
                    Conversation conversation = dataContext.Conversations.Find(conversationId);
                    dataContext.Conversations.Remove(conversation);
                    dataContext.SaveChanges();
                    response.Add("result", "success");
                }
            } catch(Exception e) {
                response.Add("result", "error");
            }

            return JsonConvert.SerializeObject(response);
           
        }

        /// <summary>
        /// Load selected conversation on support side
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public String LoadConversation()
        {
            int conversationId = Convert.ToInt32(Request.QueryString.Get("ConversationId"));
            using (var dataContext = DataContext.CreateContext())
            {
                Conversation conversation = dataContext.Conversations.Find(conversationId);
                if(conversation != null && conversation.Messages.Count() > 0)
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

        
    }
}