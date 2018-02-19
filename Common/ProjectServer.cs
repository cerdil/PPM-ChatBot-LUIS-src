﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using Common.Contracts;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;


namespace Common
{
    public class ProjectServer
    {
        private string _userName;
        private string _userPassword;

        private string _siteUri;



        public ProjectServer(string userName)
        {
            _userName = userName;

            _siteUri = ConfigurationManager.AppSettings["PPMServerURL"];
        }

        public ProjectServer(string userName , string password)
        {
            _userName = userName;
            _userPassword = password;

            _siteUri = ConfigurationManager.AppSettings["PPMServerURL"];
        }

        public string FindProjectByName(string searchTermName)
        {
            string projects = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                CamlQuery query = new CamlQuery();


                context.Load(context.Projects);
                context.ExecuteQuery();

                ProjectCollection projectDetails = context.Projects;
                projects = string.Join("<br>", projectDetails.Select(x => x.Name));
            }
            return projects;
        }

        public string GetAllProjects(bool showCompletion)
        {
            string projects = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPassword.ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);


                context.Load(context.Projects);
                //context.ExecuteQuery();

                ProjectCollection projectDetails = context.Projects;
                StringBuilder htmlTable = new StringBuilder();
                htmlTable.AppendLine("<table>");

                htmlTable.AppendLine("<tr>");
                htmlTable.AppendLine("<th>colum 1</th>");
                htmlTable.AppendLine("<th>colum 2</th>");
                htmlTable.AppendLine("<th>colum 3</th>");
                htmlTable.AppendLine("</tr>");


                htmlTable.AppendLine("<tr>");
                htmlTable.AppendLine("<td>colum 1 data</td>");
                htmlTable.AppendLine("<td>colum 2 data</td>");
                htmlTable.AppendLine("<td>colum 3 data</td>");
                htmlTable.AppendLine("</tr>");


                htmlTable.AppendLine("</table>");

                if (showCompletion == true)
                {
                    foreach (PublishedProject pro in projectDetails)
                    {
                        projects = projects + pro.Name + "," + pro.PercentComplete + "%" + "<br>";



                    }
                }

                else
                    projects = string.Join("<br>", projectDetails.Select(x => x.Name));
            }
            return projects;
        }


        public string GetProjectIssues(string pName)
        {
            string strissues = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri + "/Demo"))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName("Demo", context);

                context.Load(context.Web);
                context.ExecuteQuery();

                var issues = context.Web.Lists.GetByTitle("Issues");
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                ListItemCollection itemsIssue = issues.GetItems(query);

                context.Load(issues);
                context.Load(itemsIssue);
                context.ExecuteQuery();


                foreach (ListItem item in itemsIssue)
                {
                    string IssueName = (string)item["Title"];
                    string IssueStatus = (string)item["Status"];
                    string IssuePriority = (string)item["Priority"];


                    strissues = strissues + IssueName + "," + IssueStatus + "," + IssuePriority + "<br>";

                }

            }
            return strissues;
        }


        private static PublishedProject GetProjectByName(string name, ProjectContext context)
        {
            IEnumerable<PublishedProject> projs = context.LoadQuery(context.Projects.Where(p => p.Name == name));
            context.ExecuteQuery();

            if (!projs.Any())       // no project found
            {
                return null;
            }
            return projs.FirstOrDefault();
        }

        public string GetProjectTasks(string pName)
        {
            string strissues = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri + "/Demo"))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName("Demo", context);


                context.Load(project.Tasks);
                context.ExecuteQuery();
                PublishedTaskCollection tskcoll = project.Tasks;


                foreach (PublishedTask tsk in tskcoll)
                {
                    //tsk.ActualStart = Convert.ToDateTime("ActualStart");
                    //tsk.ActualFinish = Convert.ToDateTime("ActualFinish");
                    //tsk.Work = "Work";
                    //tsk.ActualWork = "ActualWork";
                    //tsk.PercentComplete = Convert.ToInt32("PercentComplete");


                    string TaskName = tsk.Name;
                    string TaskDuration = tsk.Duration;
                    string TaskPercentCompleted = tsk.PercentComplete.ToString();
                    string TaskStartDate = tsk.Start.ToString();
                    string TaskFinishDate = tsk.Finish.ToString();
                    strissues = strissues + TaskName + "," + TaskDuration + "," + TaskPercentCompleted + "," + TaskStartDate + "," + TaskFinishDate + "<br>";


                }



            }
            return strissues;
        }


        public string GetProjectInfo(string pName, bool optionalDate = false, bool optionalDuration = false, bool optionalCompletion = false)
        {
            string strissues = string.Empty;
            Token token = new Mongo().Get<Token>("ContextTokens", "UserName", this._userName);

            using (ClientContext contextah = TokenHelper.GetClientContextWithContextToken(_siteUri, token.ContextToken, _siteUri))
            {
                using (ProjectContext context = new ProjectContext(_siteUri))
                {
                    //SecureString passWord = new SecureString();
                    //foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                    //context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                    PublishedProject project = GetProjectByName(pName, context);

                    if (optionalDate == true)
                        strissues = strissues + project.StartDate + "," + project.FinishDate + ",";

                    if (optionalDuration == true)
                    {
                        TimeSpan duration = project.FinishDate - project.StartDate;
                        strissues = strissues + duration.Days + ",";
                    }

                    if (optionalCompletion == true)
                        strissues = strissues + project.PercentComplete + "%" + ",";



                }
            }
            return strissues;
        }

        public string GetProjectRiskResources(string pName)
        {
            string strissues = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri + "/Demo"))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName("Demo", context);

                context.Load(context.Web);
                context.ExecuteQuery();

                var issues = context.Web.Lists.GetByTitle("Risks");
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                ListItemCollection itemsIssue = issues.GetItems(query);

                context.Load(issues);
                context.Load(itemsIssue);
                context.ExecuteQuery();


                foreach (ListItem item in itemsIssue)
                {
                    string IssueName = (string)item["Title"];


                    FieldUserValue fuv = (FieldUserValue)item["AssignedTo"];



                    strissues = strissues + IssueName + "," + "," + fuv.LookupValue + "<br>";

                }

            }
            return strissues;
        }

        public string GetProjectRiskStatus(string pName)
        {
            string strissues = string.Empty;
            using (ProjectContext context = new ProjectContext(_siteUri + "/Demo"))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                PublishedProject project = GetProjectByName("Demo", context);

                context.Load(context.Web);
                context.ExecuteQuery();

                var issues = context.Web.Lists.GetByTitle("Risks");
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                ListItemCollection itemsIssue = issues.GetItems(query);

                context.Load(issues);
                context.Load(itemsIssue);
                context.ExecuteQuery();


                foreach (ListItem item in itemsIssue)
                {
                    string IssueName = (string)item["Title"];
                    string riskStatus = (string)item["Status"];
                    strissues = strissues + IssueName + "," + "," + riskStatus + "<br>";
                }

            }
            return strissues;
        }

        public string FilterProjectsByDate(string FilterType, string pStartDate, string PEndDate, string ProjectSEdateFlag)
        {
            string strissues = string.Empty;
            IEnumerable<PublishedProject> retrivedProjects = null; ;
            using (ProjectContext context = new ProjectContext(_siteUri))
            {
                SecureString passWord = new SecureString();
                foreach (char c in "Amman@123".ToCharArray()) passWord.AppendChar(c);
                context.Credentials = new SharePointOnlineCredentials(_userName, passWord);
                DateTime startdate = new DateTime();
                DateTime endate = new DateTime();

                if (!string.IsNullOrEmpty(pStartDate))
                    startdate = DateTime.Parse(pStartDate);

                if (!string.IsNullOrEmpty(PEndDate))
                    endate = DateTime.Parse(PEndDate);


                if (ProjectSEdateFlag == "START")
                {

                    if (FilterType.ToUpper() == "BEFORE" && pStartDate != "")
                    {
                        var pubProjects = context.Projects
                                  .Where(p => (p.StartDate <= startdate))
                                  .Select(p => p);
                        retrivedProjects = context.LoadQuery(pubProjects);


                    }
                    else if (FilterType.ToUpper() == "AFTER" && pStartDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.StartDate >= startdate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }

                    else if (FilterType.ToUpper() == "BETWEEN" && pStartDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.StartDate >= startdate && p.StartDate <= endate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }
                }
                else
                {

                    if (FilterType.ToUpper() == "BEFORE" && PEndDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.FinishDate <= endate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }

                    else if (FilterType.ToUpper() == "AFTER" && PEndDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.StartDate >= endate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }
                    else if (FilterType.ToUpper() == "BETWEEN" && PEndDate != "")
                    {
                        var pubProjects = context.Projects
                            .Where(p => p.IsEnterpriseProject == true
                            && p.FinishDate >= startdate && p.FinishDate <= endate);
                        retrivedProjects = context.LoadQuery(pubProjects);

                    }
                }

                context.ExecuteQuery();

            }
            if (!retrivedProjects.Any())       // no project found
            {
                return null;
            }
            else
            {
                foreach (var item in retrivedProjects)
                {
                    strissues = strissues + item.Name + "," + item.StartDate + "," + item.FinishDate + "," + item.DefaultFixedCostAccrual.ToString() + "<br>";

                }
            }

            return strissues;
        }



        //public bool GetUserPermissions()
        //{
        //    Token token = new Mongo().Get<Token>("ContextTokens", "UserName", this._userName);

        //    ClientContext context = new ClientContext(_siteUri);
        //    NetworkCredential nc = new NetworkCredential(token.UserName , token.ContextToken, "basesmc2008");
        //    context.Credentials = nc;
        //    BasePermissions bp = new BasePermissions();

        //    bp.Set(PermissionKind.ManageWeb);
        //    ClientResult<bool> manageWeb = context.Web.DoesUserHavePermissions(bp);
        //    context.ExecuteQuery();

        //    if (manageWeb.Value == true)
        //        return true;
        //    else
        //        return false;

        //}


    }
}
