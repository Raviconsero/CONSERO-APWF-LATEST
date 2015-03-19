using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using System.Reflection;
using System.Net;
using System.Security.Permissions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Mail;



using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.UI.Dialog;
using FogCreek.FogBugz.Database.Entity;
using FogCreek.FogBugz.UI.EditableTable;
using FogCreek.FogBugz.Database;



namespace Consero.Plugins.BVRemainderEmails
{
    

    public class Act : Plugin, IPluginBugDisplay, IPluginRawPageDisplay
    {
      //  [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]

        public Act(CPluginApi api)
            : base(api)
        {
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




            if (rgbug[0].ixProject != 9)

            { return null; }
            

            
              
           

                // api.Notifications.AddMessage("calling CBugDisplayDialogItem");
         
                return new CBugDisplayDialogItem[] {
                    
                new CBugDisplayDialogItem("BVReminderEmails", EditableTable(rgbug[0].ixBug).RenderHtml())
                  
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
                string emailid="";

                /* If the request did not include a valid action token, do not
                    * edit any cases and redirect with an error message to display */
                if ((api.Request[api.AddPluginPrefix("actionToken")] == null) ||
                    !api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")].ToString()))
                {
                    api.Notifications.AddMessage("action token failed");

                    sError = string.Format("{0}={1}",
                                api.AddPluginPrefix("sError"),
                                HttpUtility.UrlEncode("BVReminderEmails: Emails are not been sent because action token was invalid or missing.")
                                );
                }
                else
                {
                  
                    CSelectQuery Group_person = api.Database.NewSelectQuery("PermissionGroupMember");
                    Group_person.IgnorePermissions = true;
                    Group_person.AddSelect("ixPerson");
                    Group_person.AddWhere("PermissionGroupMember.ixPermissionGroup = 10");
                    DataSet Ds_1 = Group_person.GetDataSet();
                   
                    if (null != Ds_1.Tables && Ds_1.Tables.Count == 1 && Ds_1.Tables[0].Rows.Count >= 1)
                    {
                        

                        for (int j = 0; j < Ds_1.Tables[0].Rows.Count; j++)
                        {
                            
                            string person = Ds_1.Tables[0].Rows[j]["ixPerson"].ToString();


                            CSelectQuery Person_mailid = api.Database.NewSelectQuery("Person");
                            Person_mailid.IgnorePermissions = true;
                            Person_mailid.AddSelect("sEmail");
                            Person_mailid.AddWhere("Person.ixPerson = " + person);
                            DataSet Ds_2 = Person_mailid.GetDataSet();
                            if (null != Ds_2.Tables && Ds_2.Tables.Count == 1 && Ds_2.Tables[0].Rows.Count == 1)
                            {

                               

                                emailid = Ds_2.Tables[0].Rows[0]["sEmail"].ToString();

                                CSelectQuery Pending_Cases = api.Database.NewSelectQuery("Bug");
                                Pending_Cases.IgnorePermissions = true;
                                Pending_Cases.AddSelect("ixBug");
                                Pending_Cases.AddWhere("Bug.ixPersonAssignedTo = " + person);
                                Pending_Cases.AddWhere("Bug.ixStatus = 102");
                                DataSet Ds_3 = Pending_Cases.GetDataSet();

                                if (null != Ds_3.Tables && Ds_3.Tables.Count == 1 && Ds_3.Tables[0].Rows.Count >= 1)
                                //cases ="";
                                {
                                  
                                    for (int i = 0; i < Ds_3.Tables[0].Rows.Count; i++)
                                    {

                                        string vendor = "";
                                        string invoice_num = "";

                                        string cas = Ds_3.Tables[0].Rows[i]["ixBug"].ToString();

                                        CSelectQuery Bill_detail = api.Database.NewSelectQuery("Plugin_89_CGSInvoice_MLA");
                                        Bill_detail.AddSelect("CWFVendor,sInvoiceNumber");
                                        Bill_detail.AddWhere(("Plugin_89_CGSInvoice_MLA") + ".ixBug = " + cas);
                                        DataSet Ds_4 = Bill_detail.GetDataSet();

                                        if (null != Ds_4.Tables && Ds_4.Tables.Count == 1 && Ds_4.Tables[0].Rows.Count == 1)
                                        {

                                            vendor = Ds_4.Tables[0].Rows[0]["CWFVendor"].ToString();
                                            invoice_num = Ds_4.Tables[0].Rows[0]["sInvoiceNumber"].ToString();
                                        }

                                        cases += System.Environment.NewLine;
                                        cases += cas + "  ||  " + vendor + "  ||  " + invoice_num;
                                        
                                    }

                                    
                                    string sub = "Reminder for bills pending for your approval";
                                    string body = "Dear Approver,";
                                    body += System.Environment.NewLine;
                                    body += System.Environment.NewLine;
                                    body += "The following transactions are still pending for your approval.";
                                    body += System.Environment.NewLine;
                                    body += System.Environment.NewLine;
                                    body += "Transaction Id || Vendor Name  || Invoice No.";
                                    body += System.Environment.NewLine;
                                    body += cases;
                                    body += System.Environment.NewLine;
                                    body += System.Environment.NewLine;
                                    body += "You can login to AP-Workflow system here http://192.168.1.212:81/ and Approve/Reject the same.";
                                    body += System.Environment.NewLine;
                                    body += System.Environment.NewLine;
                                    body += "Regards,";
                                    body += System.Environment.NewLine;
                                    body += "AP Team.";
                                    body += System.Environment.NewLine;
                                    
                                    api.Mail.SendTextEmail(emailid, sub, body);

                                    cases = "";
                                }


                            }

                        }
                        
                    }

                   // api.Notifications.AddMessage("action token passed");

                   


                          /*
                        
                        for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                        {
                            //newbug.sTitle += "Attachment No : ";
                            //newbug.sTitle += ds.Tables[0].Rows[j]["ixAttachment"];
                            CAttachment attachment = api.Attachment.GetAttachment(Convert.ToInt32(ds.Tables[0].Rows[j]["ixAttachment"]));
                            attachment.IgnorePermissions = true;
                            attachments.Add(CloneAttachment(attachment, "Copy_" + (i + 1).ToString() + "_Of_" + attachment.sFileName));
                        }
                        */
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

        //[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert)]
       
        private string sTableId;

        protected CEditableTable EditableTable(int ixBug)
        {
            

           // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("BVReminderEmails");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            editableTable.Header.AddCell("Reminder Emails");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Send mails to approvers on pending approval cases"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                    sTableId,
                                                    "new",
                                                    "sDataId",
                                                    CommandUrl("new", ixBug),
                                                    "Send Reminder mail"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("new", dlgTemplateNew);
        
            return editableTable;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew(int ixBug)
        {
            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();
            dlgTemplateNew.Template.sTitle = "Are you sure to send Reminder Emails";
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
            CDialogItem itemEditId =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("iCopies"), ""),
                                " ");
          
           // dlgTemplateNew.Template.Items.Add(itemEditId);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));
          
          //  api.Notifications.AddMessage("dlgTemplateNew returned");
            
            return dlgTemplateNew;
           
        }

        /* these two methods are used to construc the Urls which a user would
         * follow if javascript is disabled (preventing the use of the Dialogs */
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

