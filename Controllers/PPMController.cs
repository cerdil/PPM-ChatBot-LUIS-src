﻿using System;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.Generic;
using System.Web;
using Common.Contracts;
using Common;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using LuisBot.Forms;
using Microsoft.Bot.Builder.FormFlow;

namespace Microsoft.Bot.Sample.LuisBot
{
    [BotAuthentication]
    public class PPMController : ApiController
    {

        protected string prompt { get; }
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            
            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                if (activity.From.Name.ToLower() != "User".ToLower())
                {
                    await Conversation.SendAsync(activity, () => new PPMDialog(activity));
                }
                else
                {
                    await Conversation.SendAsync(activity, MakeForm);
                }
                
            }
            else
            {
                HandleSystemMessage(activity);
            }


          

            var response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            return response;
        }

        internal static IDialog<LoginForm> MakeForm()
        {
            return Chain.From(() => FormDialog.FromForm(LoginForm.BuildForm))
                
                .Do(async (context, order) =>
                {
                    try
                    {
                        var completed = await order;
                        if (TokenHelper.checkAuthorizedUser(completed.Name) == true)
                        {
                            context.UserData.SetValue("UserName", completed.Name);
                            new Mongo().Insert("ContextTokens", new Token(completed.Name));
                            await context.PostAsync("You are registerd. Have a happy time with us.");
                        }
                        else
                        {
                            string reply = $"Sorry, Your User Name Is wrong or you don't have permission. Please try again.";
                            await context.PostAsync(reply);
                        }
                    }
                    catch (FormCanceledException<LoginForm> e)
                    {
                        string reply = null == e.InnerException ?
                        $"Hey, you quit the registration. Dont miss out the party!" :
                        "Sorry, Could not register you. Please try again.";
                        await context.PostAsync(reply);
                    }
                });
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}