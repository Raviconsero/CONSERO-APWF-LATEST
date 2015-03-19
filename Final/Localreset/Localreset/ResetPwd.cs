using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Net;
using System.Security.Permissions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.UI.Dialog;
using FogCreek.FogBugz.UI.EditableTable;
using FogCreek.FogBugz.Database;
using FogCreek.FogBugz.Database.Entity;

using System.Collections;

namespace Consero.Plugins.ResetPwd
{
    public class ResetPwd : Plugin, IPluginRawPageDisplay, //IPluginBugJoin,
        IPluginBugDisplay 
    {
        protected const string PLUGIN_ID =
           "CGSresetpwd@conseroglobal.com";

        /* A constant for populating the "code name" input field for multiple case edit */
        protected const string VARIOUS_TEXT = "[various]";
        // private string sPrefixedTableName;

  
        public ResetPwd(CPluginApi api)
            : base(api)
        {
            // sPrefixedTableName = api.Database.PluginTableName("TestField");
        }

        #region IPluginBugDisplay Members

        public CBugDisplayDialogItem[] BugDisplayEditLeft(CBug[] rgbug,
                                                       BugEditMode nMode,
                                                       bool fPublic)
        {
            return null;
        }

        public CBugDisplayDialogItem[] BugDisplayEditTop(CBug[] rgbug,
                                                      BugEditMode nMode,
                                                      bool fPublic)
        {
            return null;
        }

        public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
        {
            /* If there was an error passed in the URL or the redirect from
            * the raw page, display it using the Notifications API */
            if ((api.Request[api.AddPluginPrefix("sError")] != null) &&
                (api.Request[api.AddPluginPrefix("actionToken")] != null) &&
                api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")].ToString()))
            {
                api.Notifications.AddError(api.Request[api.AddPluginPrefix("sError")].ToString());
            }

            //If only one bug is shown enable the make copies button
            if (rgbug.Length != 1)
            {

                return null;
            }

            /* if bug in the list is not editable by the current user, don't make copies */
            if (!rgbug[0].IsWritable)
            {

                return null;
            }

            if (rgbug[0].ixProject != 3)

            { return null; }

            // api.Notifications.AddMessage("calling CBugDisplayDialogItem");

            return new CBugDisplayDialogItem[] {
                    
                new CBugDisplayDialogItem("Reset Password", EditableTable(rgbug[0].ixBug).RenderHtml())
                  
                };
        }

        public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
        {
            // api.Notifications.AddMessage("calling CBugDisplayDialogItem_1");
            return null;
        }

        #endregion

        #region IPluginRawPageDisplay Members

        public string RawPageDisplay()
        {
            try
            {

                // api.Notifications.AddMessage("calling RawPageDisplay");
                // int ixBug = 0;
                //  int iCopies = 0;
                string sError = "";
                string cases = "";
                string emailid = "";

                /* If the request did not include a valid action token, do not
                    * edit any cases and redirect with an error message to display */
                if ((api.Request[api.AddPluginPrefix("actionToken")] == null) ||
                    !api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")].ToString()))
                {
                    api.Notifications.AddAdminNotification("action token failed", "action token failed");
                    sError = string.Format("{0}={1}", api.AddPluginPrefix("sError"),
                    HttpUtility.UrlEncode("ResetPassword: Emails are not been sent because action token was invalid or missing."));
                }
                else
                {

                    string Username = (api.Request[api.AddPluginPrefix("Username")].ToString());
                    string Password = (api.Request[api.AddPluginPrefix("Password")].ToString());
                    emailid = (api.Request[api.AddPluginPrefix("EmailId")].ToString());

                    //api.Notifications.AddAdminNotification("Username", Username.ToString());
                    //api.Notifications.AddAdminNotification("Password", Password.ToString());
                    //api.Notifications.AddAdminNotification("emailid", emailid.ToString());

                    string sub = "Welcome to Empower Workflow!";
                    string body = "Hi " + Username + ",";
                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                   // body += "Your access has been created in Consero AP workflow system. Below are the access details.";
                    body += "Welcome to Empower Workflow.";
                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                    body += "To get started click on the link below and enter the user name and password.";
                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                    body += " http://empower.conseroglobal.com .";
                    body += System.Environment.NewLine;
                    body += "If above link does not work copy and paste this URL onto your browser.";
                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                    body += "Here are the user credentials:";
                    body += System.Environment.NewLine;
                   // body += System.Environment.NewLine;
                    body += "User Name : " + emailid.ToString() +".";
                    body += System.Environment.NewLine;
                    body += "Password : " + Password;
                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                    body += "Please contact techsupport@conseroglobal.com if you have any questions.";         
                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                    body += "Regards,";
                    body += System.Environment.NewLine;
                    body += "Consero TechTeam.";
                    body += System.Environment.NewLine;
                //    api.Notifications.AddAdminNotification("emailid","sent1");
                    api.Mail.SendTextEmail(emailid, sub, body);
                    api.Mail.SendTextEmail("poornima.r@conseroglobal.com", sub, body);
                    //api.Notifications.AddAdminNotification("emailid", "sent2");
                   // cases = "";

                }
            }

            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error while sending Emails");
            }

            return string.Empty;
        }

        public PermissionLevel RawPageVisibility()
        {
            return PermissionLevel.Normal;
        }

        #endregion

        private string sTableId;

        protected CEditableTable EditableTable(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("New User In APWF");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            editableTable.Header.AddCell("Send Credentials to Users");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Send emails to approvers on reset Password"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId, "new", "sDataId", CommandUrl("new", ixBug), "Send Credentials"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("new", dlgTemplateNew);

            return editableTable;
        }

        protected CDialogTemplate DialogTemplateNew(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new user to " ;
            dlgTemplateNew.Template.sWidth = "300px";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixBug"),
                                                   ixBug.ToString()));

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Username"), ""),
                                "User Name");
            dlgTemplateNew.Template.Items.Add(itemEditId2);

            CDialogItem itemEditId1 =
               new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Password"), ""),
                               "Password");
            dlgTemplateNew.Template.Items.Add(itemEditId1);

            CDialogItem itemEditId3 =
               new CDialogItem(Forms.TextInput(api.AddPluginPrefix("EmailId"), ""),
                               "EmailId");
            dlgTemplateNew.Template.Items.Add(itemEditId3);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }

        protected string CommandUrl(string sCommand, int ixBug)
        {
            return string.Concat(api.Url.PluginPageUrl(),
                                 LinkParameter("sCommand", sCommand),
                                 LinkParameter("ixBug", ixBug.ToString()));
        }

        protected string LinkParameter(string sName, string sValue)
        {
            return string.Format("&{0}={1}", api.AddPluginPrefix(sName), sValue);
        }
    }
}
