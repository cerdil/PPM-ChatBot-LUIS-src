﻿using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net.Http;
using Microsoft.Bot.Builder.FormFlow;
using LuisBot.Forms;

namespace Microsoft.Bot.Sample.LuisBot
{
    [Serializable]
    public class PPMDialog : LuisDialog<object>
    {
        private string userName;
        private string password;

        private DateTime msgReceivedDate;
        public PPMDialog(Activity activity) : base(new LuisService(new LuisModelAttribute(


            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            userName = activity.From.Name;
            msgReceivedDate = DateTime.Now;// activity.Timestamp ? ? DateTime.Now;
        }
        [LuisIntent("")]
        [LuisIntent("none")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult luisResult)
        {
            string response = string.Empty;
            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }


        [LuisIntent("Greet.Welcome")]
        public async Task GreetWelcome(IDialogContext context, LuisResult luisResult)
        {
            StringBuilder response = new StringBuilder();

            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {

                if (this.msgReceivedDate.ToString("tt") == "AM")
                {
                    response.Append($"Good morning, {userName}.. :)");
                }
                else
                {
                    response.Append($"Hey {userName}.. :)");
                }

                await context.PostAsync(response.ToString());
                context.Wait(this.MessageReceived);
            }
            else
            {
                var form = new FormDialog<LoginForm>(new LoginForm(), LoginForm.BuildForm);
                context.Call(form, SignUpComplete);
            }
        }

       

        [LuisIntent("Greet.Farewell")]
        public async Task GreetFarewell(IDialogContext context, LuisResult luisResult)
        {
            context.UserData.Clear();
            //.SetValue("UserName", string.Empty);
            //context.UserData.SetValue("Password", string.Empty);

            string response = string.Empty;
            if (this.msgReceivedDate.ToString("tt") == "AM")
            {
                response = $"Good bye, {userName}.. Have a nice day. :)";
            }
            else
            {
                response = $"b'bye {userName}, Take care.";
            }

            

            await context.PostAsync(response);
            context.Wait(this.MessageReceived);
        }



        [LuisIntent("GetAllProjectsData")]
        public async Task GetAllProjectsData(IDialogContext context, LuisResult luisResult)
        {
            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {
                EntityRecommendation projectSDate, projectEDate, projectDuration, projectCompletion, projectDate;

                bool showCompletion = false;
                bool Pdate = false;
                bool pDuration = false;

                if (luisResult.TryFindEntity("Project.Completion", out projectCompletion))
                    showCompletion = true;

                if (luisResult.TryFindEntity("Project.SDate", out projectSDate) || luisResult.TryFindEntity("Project.EDate", out projectEDate) || luisResult.TryFindEntity("Project.Date", out projectDate))
                    Pdate = true;

                if (luisResult.TryFindEntity("Project.Duration", out projectDuration))
                    pDuration = true;

                await context.PostAsync(new Common.ProjectServer(userName, password).GetAllProjects(showCompletion , Pdate , pDuration));
                context.Wait(this.MessageReceived);
            }
            else
            {
                var form = new FormDialog<LoginForm>(new LoginForm(), LoginForm.BuildForm);
                context.Call(form, SignUpComplete);
            }
        }

        [LuisIntent("GetProjectInfo")]
        public async Task GetProjectInfo(IDialogContext context, LuisResult luisResult)
        {
            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {
                EntityRecommendation projectname;
                EntityRecommendation projectIssues;
                EntityRecommendation projectTasks;
                EntityRecommendation projectRisks;


                string searchTerm_ProjectName = string.Empty;
                string ListName = string.Empty;


                if (luisResult.TryFindEntity("Project.name", out projectname))
                {
                    searchTerm_ProjectName = projectname.Entity;
                }

                if (luisResult.TryFindEntity("Project.Issues", out projectIssues))
                {
                    ListName = Common.Enums.ListName.Issues.ToString();
                }

                if (luisResult.TryFindEntity("Project.Tasks", out projectTasks))
                {
                    ListName = Common.Enums.ListName.Tasks.ToString();
                }
                if (luisResult.TryFindEntity("Project.Risks", out projectRisks))
                {
                    ListName = Common.Enums.ListName.Risks.ToString();
                }

                if (string.IsNullOrWhiteSpace(searchTerm_ProjectName))
                {
                    await context.PostAsync($"Unable to get search term.");
                }
                else
                {
                    if (ListName != string.Empty)
                    {
                        await context.PostAsync(new Common.ProjectServer(userName, password).GetProjectSubItems(searchTerm_ProjectName, ListName));
                        context.Wait(this.MessageReceived);
                    }
                    else
                    {
                        EntityRecommendation  projectSDate, projectEDate, projectDuration, projectCompletion , projectDate;


                        bool Pdate = false;
                        bool pDuration = false;
                        bool PCompletion = false;

                        if (luisResult.TryFindEntity("Project.SDate", out projectSDate) || luisResult.TryFindEntity("Project.EDate", out projectEDate) || luisResult.TryFindEntity("Project.Date", out projectDate))
                            Pdate = true;
                        if (luisResult.TryFindEntity("Project.Duration", out projectDuration))
                            pDuration = true;
                        if (luisResult.TryFindEntity("Project.Completion", out projectCompletion))
                            PCompletion = true;

                        await context.PostAsync(new Common.ProjectServer(userName , password).GetProjectInfo(searchTerm_ProjectName, Pdate, pDuration, PCompletion));
                        context.Wait(this.MessageReceived);
                    }
                }
            }
            else
            {
                var form = new FormDialog<LoginForm>(new LoginForm(), LoginForm.BuildForm);
                context.Call(form, SignUpComplete);
            }
        }

        [LuisIntent("FilterProjectsByDate")]
        public async Task FilterProjectsByDate(IDialogContext context, LuisResult luisResult)
        {
            if (context.UserData.TryGetValue<string>("UserName", out userName) && (context.UserData.TryGetValue<string>("Password", out password)))
            {
                string FilterType = string.Empty;
                string ProjectSEdateFlag = "START";
                string ProjectED = string.Empty;


                string ProjectSDate = string.Empty;
                string ProjectEDate = string.Empty;

                var filterDate = (object)null;

                EntityRecommendation dateTimeEntity, dateRangeEntity, ProjectS, ProjectE;


                if (luisResult.TryFindEntity("builtin.datetimeV2.daterange", out dateRangeEntity))
                {
                    filterDate = dateRangeEntity.Resolution.Values.Select(x => x).OfType<List<object>>().SelectMany(i => i).FirstOrDefault();
                    if (Common.TokenHelper.Datevalues(filterDate, "Mod") != "")
                    {
                        FilterType = Common.TokenHelper.Datevalues(filterDate, "Mod");
                        ProjectSDate = Common.TokenHelper.Datevalues(filterDate, "timex");

                    }
                    else
                    {
                        FilterType = "Between";

                        ProjectSDate =Common.TokenHelper.Datevalues(filterDate, "start");
                    }
                    ProjectEDate = Common.TokenHelper.Datevalues(filterDate, "end");

                }
                else if (luisResult.TryFindEntity("Project.Start", out ProjectS))
                {
                    ProjectSEdateFlag = "START";
                }
                else if (luisResult.TryFindEntity("Project.Finish", out ProjectE))
                {
                    ProjectSEdateFlag = "Finish";
                }


                if (filterDate != null)
                    await context.PostAsync(new Common.ProjectServer(this.userName, password).FilterProjectsByDate(FilterType, ProjectSDate, ProjectEDate, ProjectSEdateFlag));




                context.Wait(this.MessageReceived);
            }
            else
            {
                var form = new FormDialog<LoginForm>(new LoginForm(), LoginForm.BuildForm);
                context.Call(form, SignUpComplete);
            }
        }



       

    

        private async Task SignUpComplete(IDialogContext context, IAwaitable<LoginForm> result)
        {
            LoginForm form = null;
            try
            {
                form = await result;
            }
            catch (OperationCanceledException)
            {
            }

            if (form == null)
            {
                await context.PostAsync("You canceled the form.");
            }
            else
            {
                // Here is where we could call our signup service here to complete the sign-up

                var message = $"You are currently Logged In. Please Enjoy Using our App. **{form.Name}**.";
                await context.PostAsync(message);
            }

            context.Wait(MessageReceived);
        }
        //public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    var msg = await argument;
        //    if (msg.ChannelId == "ppmDL")
        //    {
        //        string botChatSecret = ConfigurationManager.AppSettings["BotChatDirectLineSecret"];
        //        var request = new HttpRequestMessage(HttpMethod.Get, "https://webchat.botframework.com/api/tokens");
        //        request.Headers.Add("Authorization", "BOTCONNECTOR " + botChatSecret);
        //        using (HttpResponseMessage response = await new HttpClient().SendAsync(request))
        //        {
        //            string token = await response.Content.ReadAsStringAsync();
        //            Token = token.Replace("\"", "");
        //        }

        //    }
        //    else if (msg.ChannelId == "facebook")
        //    {
        //        var reply = context.MakeMessage();
        //        reply.ChannelData = new FacebookMessage
        //        (
        //            text: "Please share your location with me.",
        //            quickReplies: new List<FacebookQuickReply>
        //            {
        //                // If content_type is location, title and payload are not used
        //                // see https://developers.facebook.com/docs/messenger-platform/send-api-reference/quick-replies#fields
        //                // for more information.
        //                new FacebookQuickReply(
        //                    contentType: FacebookQuickReply.ContentTypes.Location,
        //                    title: default(string),
        //                    payload: default(string)
        //                )
        //            }
        //        );
        //        await context.PostAsync(reply);
        //      //  context.Wait(LocationReceivedAsync);
        //    }
        //    else if (msg.ChannelId == "email")
        //    { }
        //    else if (msg.ChannelId == "skype")
        //    { }
        //    else if (msg.ChannelId == "slack")
        //    { }
        //    else
        //    {
        //        context.Done(default(Place));
        //    }
        //}

        //public virtual async Task LocationReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    var msg = await argument;
        //    var location = msg.Entities?.Where(t => t.Type == "Place").Select(t => t.GetAs<Place>()).FirstOrDefault();
        //    context.Done(location);
        //}



    }

}


