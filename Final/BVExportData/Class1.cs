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



namespace Consero.Plugins.ExportData
{


    public class Act : Plugin, IPluginBugDisplay, IPluginRawPageDisplay
    {
        //  [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]

        protected const string PLUGIN_ID =
          "ExportData@conseroglobal.com";

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




            if (rgbug[0].ixProject != 6)

            { return null; }






            // api.Notifications.AddMessage("calling CBugDisplayDialogItem");

            return new CBugDisplayDialogItem[] {
                    
                new CBugDisplayDialogItem("ExportData", EditableTable(rgbug[0].ixBug).RenderHtml())
                  
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
                             
                string sError = "";
                //string cases = "";
                int ixBug = 0;
                /* If the request did not include a valid action token, do not
                    * edit any cases and redirect with an error message to display */
                
                if ((api.Request[api.AddPluginPrefix("actionToken")] == null) ||
                    !api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")].ToString()))
                {
                    api.Notifications.AddMessage("action token failed");

                    sError = string.Format("{0}={1}",
                                api.AddPluginPrefix("sError"),
                                HttpUtility.UrlEncode("ExportData: Data has not been exoprted because action token was invalid or missing.")
                                );
                }
                else
                {
                 
                  
                    /*DateTime fmdt = Convert.ToDateTime(api.Request[api.AddPluginPrefix("FromDate")].ToString());
           
                    string Format = "yyyy-MM-dd 00:00:00.00";
            
                    string fromdate1 = fmdt.ToString(Format);
                
                    DateTime todt = Convert.ToDateTime(api.Request[api.AddPluginPrefix("ToDate")].ToString());
                  
                    string todate1 = todt.ToString(Format);
                    string emailids = (api.Request[api.AddPluginPrefix("Emailid")].ToString());

                    DateTime dNow = DateTime.Now;

                    string sNow = dNow.ToString();

                    */

                                       
                  //  string hdr_mail_body ="";   // for header

                   // string hdr_mail_body2 = ""; // for line items

                   // string sInvoiceno = "";
                    
                   // CSelectQuery Bill_Header = api.Database.NewSelectQuery("Plugin_37_CustomBugData");
                   // Bill_Header.IgnorePermissions = true;
                    //Bill_Header.AddSelect("ixBug,customxformy81,vendorxnamei32,postingxperiodxxreqxd212,currencyv14,invoicexdatee36,datexinvoicexenteredb47,exchangexratea814,termsa39,duexdatey3a,invoicexnumberq55,memop3c");
                    //Bill_Header.AddWhere("convert(varchar, datexinvoicexenteredb47,101) between " + fromdate1 + " and " + todate1);
                  
                   // DataSet Ds_1 = Bill_Header.GetDataSet();
                    
                  
                    // for restricting the user to export data  maximum of 60 days
                   
                    /*
                    if (((todt.Date - fmdt.Date).Days) > 30)

                    {
                        api.Notifications.AddAdminNotification(sNow+" over 30 days", ((todt.Date - fmdt.Date).Days).ToString() + " Exported date range is Over 30"); 
                        return string.Empty; 
                    }
                    */


                    ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());
                    CBug bug = api.Bug.GetBug(ixBug);
                    bug.IgnorePermissions = true;
                    int iproj = bug.ixProject;
                    string user = bug.ixPersonAssignedTo.ToString();

                    DateTime insertdate = new DateTime();
                   string insdate =  insertdate.Date.ToString();

                   // if ((api.Request[api.AddPluginPrefix("fmdt")].ToString().Trim()) != null && (api.Request[api.AddPluginPrefix("todt")].ToString().Trim()) != null)
                    {


                        CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("Export_Trigger_Table"));
                            insert1.InsertInt("ixProject", iproj);
                           // insert1.InsertString("User", user);
                            insert1.InsertString("sETTfmDate", insdate);
                            insert1.Execute();
                        
                    }



                  //-------------------------
                    

                                            // line items for this case

                                            
                                        }
                                    }
                                
                            

//-------------------------------------------------------------------------------------------------------------
                 
               
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



        #region IPluginDatabase Members

        public CTable[] DatabaseSchema()
        {
            /* for this plugin, we'll need a bug-to-code name table to allow for a join. */

            CTable Export_Table = api.Database.NewTable(api.Database.PluginTableName("Export_Table"));
            Export_Table.sDesc = "Caputures Export trigger parameters Parameters";
            Export_Table.AddAutoIncrementPrimaryKey("ixET");
            Export_Table.AddIntColumn("ixProject", true, 1);
            Export_Table.AddVarcharColumn("sETTfmDate", 200, false);
            Export_Table.AddDateColumn("sETTtoDate", false);
            Export_Table.AddVarcharColumn("User", 200, false);
            Export_Table.AddVarcharColumn("sETTextra2", 200, false);
                       
           
            return new CTable[] {Export_Table};
        }

        public int DatabaseSchemaVersion()
        {
            return 3;
        }

        public void DatabaseUpgradeAfter(int ixVersionFrom, int ixVersionTo,
            CDatabaseUpgradeApi apiUpgrade)
        {
        }

        public void DatabaseUpgradeBefore(int ixVersionFrom, int ixVersionTo,
            CDatabaseUpgradeApi apiUpgrade)
        {
        }

        #endregion


        private string sTableId;



        protected CEditableTable EditableTable(int ixBug)
        {
                        
            // api.Notifications.AddMessage("calling editable table");

          
            CEditableTable editableTable = new CEditableTable("ExportData");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            editableTable.Header.AddCell("Export Bills Data");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Exports Bills data to email"));
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
                                                    "Export Data"));

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
            dlgTemplateNew.Template.sTitle = "Exporting Bills detail";
            dlgTemplateNew.Template.sWidth = "375px";

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
           // CDialogItem itemEditId =
             //   new CDialogItem(Forms.TextInput(api.AddPluginPrefix("iCopies"), ""),
                              //  " ");

            // dlgTemplateNew.Template.Items.Add(itemEditId);

            // CDialogItem itemEditId1 =
            //  new CDialogItem(Forms.DateInputCTZ(api.AddPluginPrefix("FmDate"), "FromDate"),"From Date ");

             /* the DateInputOptions class allows you to control the behavior of the FogBugz
              * date picker widget */
             DateInputOptions dtDateOptions = new DateInputOptions();
             dtDateOptions.fAllowFuture = false;
             /* Create a standard Fogbugz date text field and calendar picker pair */
             CDialogItem itemfrmDate = new CDialogItem(
                 Forms.DateInputCTZ("FromDate",
                                    api.AddPluginPrefix("FromDate"),
                                    DateTime.Parse(DateTime.Now.ToString()),
                                    dtDateOptions),
                 "From Date",
                 " ");


           //  dlgTemplateNew.Template.Items.Add(itemfrmDate);

           //  CDialogItem itemEditId2 =
            // new CDialogItem(Forms.DateInputCTZ(api.AddPluginPrefix("ToDate"), "ToDate"),"ToDate");

             DateInputOptions dtDateOptions1 = new DateInputOptions();
             dtDateOptions1.fAllowFuture = false;
             /* Create a standard Fogbugz date text field and calendar picker pair */
             CDialogItem itemtoDate = new CDialogItem(
                 Forms.DateInputCTZ("ToDate",
                                    api.AddPluginPrefix("ToDate"),
                                    DateTime.Parse(DateTime.Now.ToString()),
                                    dtDateOptions1),
                 "To Date",
                 "Date range should not be more than 30 days");


            // dlgTemplateNew.Template.Items.Add(itemtoDate);

            // dlgTemplateNew.Template.Items.Add(itemEditId2);

             CDialogItem itemEditId3 =
             new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Emailid"), "For multiple Ids separate them by a comma"),
            "Email Id(s)");

           //  dlgTemplateNew.Template.Items.Add(itemEditId3);

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

