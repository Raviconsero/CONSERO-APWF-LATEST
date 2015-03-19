using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data;
using System.Reflection;

using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.UI.Dialog;
using FogCreek.FogBugz.Database.Entity;
using FogCreek.FogBugz.UI.EditableTable;

namespace Consero.Plugins.CopyBugs
{
    public class Act : Plugin, IPluginBugDisplay, IPluginRawPageDisplay
    {
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

           

            if (rgbug[0].ixStatus != 72)
            {
                
                    return null;
                
               
            }

            return new CBugDisplayDialogItem[] {
                new CBugDisplayDialogItem("CopyBug2.0", EditableTable(rgbug[0].ixBug).RenderHtml())
            };
        }

        public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
        {
            return null;
        }

        #endregion

        #region IPluginRawPageDisplay Members

        public string RawPageDisplay()
        {
            try
            {
                int ixBug = 0;
                int iCopies = 0;
                string sError = "";

                /* If the request did not include a valid action token, do not
                    * edit any cases and redirect with an error message to display */
                if ((api.Request[api.AddPluginPrefix("actionToken")] == null) ||
                    !api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")].ToString()))
                {
                    sError = string.Format("{0}={1}",
                                api.AddPluginPrefix("sError"),
                                HttpUtility.UrlEncode("Copy Bug: Cases not copied because action token was invalid or missing.")
                                );
                }
                else
                {
                    ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());
                    iCopies = Convert.ToInt32(api.Request[api.AddPluginPrefix("iCopies")].ToString());

                    CBug bug = api.Bug.GetBug(ixBug);
                    bug.IgnorePermissions = true;
                    CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                    attachmentQuery.AddWhere(" Bug.ixBug = " + ixBug.ToString());
                    attachmentQuery.IgnorePermissions = true;
                    attachmentQuery.ExcludeDeleted = true;
                    DataSet ds = attachmentQuery.GetDataSet();

                    for (int i = 0; i < iCopies; i++)
                    {
                        CBug newbug = api.Bug.NewBug();
                        newbug.IgnorePermissions = true;
                        newbug.ixProject = bug.ixProject;
                        newbug.ixArea = bug.ixArea;
                        newbug.sTitle = bug.sTitle + "- Copy " + (i + 1).ToString();
                        newbug.ixCategory = bug.ixCategory;
                        newbug.ixPersonAssignedTo = bug.ixPersonAssignedTo;
                        newbug.ixPriority = bug.ixPriority;
                        newbug.ixStatus = bug.ixStatus;
                        newbug.ixBugParent = bug.ixBug;
                        newbug.sCustomerEmail = bug.sCustomerEmail;
                        //newbug.sTitle += " - Attachment Count : ";
                        //newbug.sTitle += ds.Tables[0].Rows.Count.ToString();
                        List<CAttachment> attachments = new List<CAttachment>();
                        for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                        {
                            //newbug.sTitle += "Attachment No : ";
                            //newbug.sTitle += ds.Tables[0].Rows[j]["ixAttachment"];
                            CAttachment attachment = api.Attachment.GetAttachment(Convert.ToInt32(ds.Tables[0].Rows[j]["ixAttachment"]));
                            attachment.IgnorePermissions = true;
                            attachments.Add(CloneAttachment(attachment, "Copy_" + (i + 1).ToString() + "_Of_" + attachment.sFileName));
                        }
                        newbug.Commit("Is A Copy of Case " + ixBug.ToString(), attachments.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error while copying cases");
            }

            return string.Empty;
        }

        public PermissionLevel RawPageVisibility()
        {
            return PermissionLevel.Normal;
        }

        #endregion

        [System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert)]
        CAttachment CloneAttachment(CAttachment attachment, string sFileName)
        {
            // find the type for the internal CAttachment class    
            // (this is different from FogCreek.FogBugz.Plugins.Entity.CAttachment)   
            var ass = Assembly.Load("FogBugz");
            var tCAttachment = ass.GetType("FogCreek.FogBugz.CAttachment");
            if (tCAttachment == null)
                throw new Exception("Couldn't load 'FogCreek.FogBugz.CAttachment' type.");
            // find the constructor and create an instance    
            var ctor = tCAttachment.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                null, new Type[] { }, null);
            if (ctor == null)
                throw new Exception("Couldn't find default CAttachment constructor.");
            var entity = ctor.Invoke(null);
            // create a new attachment record in the DB    
            var storeAttachmentMethod = tCAttachment.GetMethod("StoreAttachmentInDB", BindingFlags.Instance | BindingFlags.NonPublic);
            if (storeAttachmentMethod == null)
                throw new Exception("Couldn't find StoreAttachmentInDB method.");
            var ixAttachment = (int)storeAttachmentMethod.Invoke(entity, new object[] { attachment.rgbData, sFileName });
            if (ixAttachment == 0)
                throw new Exception("Unable to clone attachment.");
            return api.Attachment.GetAttachment(ixAttachment);
        }

        private string sTableId;

        protected CEditableTable EditableTable(int ixBug)
        {
            CEditableTable editableTable = new CEditableTable("CopyBugsTable");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            editableTable.Header.AddCell("Split Case");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("If the attachment has more than one invoice"));
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
                                                    "Enter No Of Invoices"));

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
            dlgTemplateNew.Template.sTitle = "Make Copies Of Case " + ixBug.ToString();
            dlgTemplateNew.Template.sWidth = "200px";

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
                                "No Of Copies");
            dlgTemplateNew.Template.Items.Add(itemEditId);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

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

