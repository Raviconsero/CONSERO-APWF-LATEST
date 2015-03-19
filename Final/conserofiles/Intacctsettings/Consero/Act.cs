using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Data;

using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.UI.Dialog;
using FogCreek.FogBugz.UI.EditableTable;
using FogCreek.FogBugz.Database;
using System.Collections;
using System.Xml;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace Consero.Plugins.Custom
{
    /* Class Declaration: Inherit from Plugin, expose IPluginProjectJoin, IPluginProjectDisplay,
     * IPluginProjectCommit */
    public class Act : Plugin, IPluginProjectJoin,
        IPluginProjectDisplay, IPluginProjectCommit, IPluginDatabase, IPluginRawPageDisplay
    {
        protected const string sPluginId = "IntacctSettings@conseroglobal.com";
        protected const string sIntacctUrl = "sIntacctUrl";
        protected const string sIntacctSenderId = "sIntacctSenderId";
        protected const string sIntacctSenderPassword = "sIntacctSenderPassword";
        protected const string sIntacctUserId = "sIntacctUserId";
        protected const string sIntacctUserPassword = "sIntacctUserPassword";
        protected const string sIntacctCompanyId = "sIntacctCompanyId";
        protected const string sIntacctLocationId = "sIntacctLocationId";
        protected const string sDailyMailAddress = "sDailyMailAddress";
        protected const string sBackupLocation = "sBackupLocation";
        protected const string sEnableIntacct = "sEnableIntacct";
        protected const string sNoOfClientApprovers = "sNumberOfClientApprovers";

        protected string preCommitIntacctUrl = string.Empty;
        protected string preCommitIntacctSenderId = string.Empty;
        protected string preCommitIntacctSenderPassword = string.Empty;
        protected string preCommitIntacctUserId = string.Empty;
        protected string preCommitIntacctUserPassword = string.Empty;
        protected string preCommitIntacctCompanyId = string.Empty;
        protected string preCommitIntacctLocationId = string.Empty;
        protected string preCommitDailyMailAddress = string.Empty;
        protected string preCommitEnableIntacct = string.Empty;

        /* Constructor: We'll just initialize the inherited Plugin class, which 
         * takes the passed instance of CPluginApi and sets its "api" member variable. */
        public Act(CPluginApi api)
            : base(api)
        {
        }

        #region IPluginProjectJoin Members

        public string[] ProjectJoinTables()
        {
            return new string[] { "IntacctSettings"};
        }

        #endregion

        #region IPluginProjectDisplay Members

        public string GetText(CProject project, String fieldId)
        {
            if (project == null)
                return "";

            return Convert.ToString(project.GetPluginField(sPluginId, fieldId));
        }

        public CDialogItem[] ProjectDisplayEdit(CProject project)
        {   
            List<CDialogItem> dialogItems = new List<CDialogItem>();
            dialogItems.Add(CreateDropDownInput(project, "Enable Intacct", sEnableIntacct, "Enable Intacct"));
            dialogItems.Add(CreateTextInput(project, "Daily Mail Should Be Sent To", sDailyMailAddress, "Daily Mail Address"));
            dialogItems.Add(CreateNumericInput(project, "Number of Client Approvers", sNoOfClientApprovers, "No. Of Client Approvers"));
            dialogItems.Add(CreateTextInput(project, "Intacct Url", sIntacctUrl, "Intacct API Access URL"));
            dialogItems.Add(CreateTextInput(project, "Intacct Sender Id", sIntacctSenderId, "This most likely is an ID allocated for Consero"));
            dialogItems.Add(CreateTextInput(project, "Intacct Sender Password", sIntacctSenderPassword, "The password for the sender (ie) Consero"));
            dialogItems.Add(CreateTextInput(project, "Intacct User Id", sIntacctUserId, "Make sure this user id can list all types of stuff in Intacct. Used For Lookup"));
            dialogItems.Add(CreateTextInput(project, "Intacct User Password", sIntacctUserPassword, "Password for above said user id"));
            dialogItems.Add(CreateTextInput(project, "Intacct Company Id", sIntacctCompanyId, "Company Name"));
            dialogItems.Add(CreateTextInput(project, "Intacct Client Id", sIntacctLocationId, "Location Name"));
            if (project.fLoaded)
            {
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlVendor").RenderHtml(), "GL Vendors"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlNetTerm").RenderHtml(), "GL Net Terms"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlTrxCurrency").RenderHtml(), "Gl Trx Currency"));
               //dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlExchratetype").RenderHtml(), "Gl Exch Rate Type"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlAccount").RenderHtml(), "GL Accounts"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlDepartment").RenderHtml(), "GL Departments"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlLocation").RenderHtml(), "GL Locations"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlProject").RenderHtml(), "GL Projects"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlItem").RenderHtml(), "GL Items"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlClass").RenderHtml(), "GL Classes"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlBankAccount").RenderHtml(), "GL Bank Accounts"));
               dialogItems.Add(new CDialogItem(EditableTable(project.ixProject, "GlPaymentMethod").RenderHtml(), "GL Payment Methods"));
            }

            return dialogItems.ToArray();
        }

        private CDialogItem CreateTextInput(CProject project, string sDisplay, string sId, string sInstructions)
        {
            return new CDialogItem(Forms.TextInput(api.PluginPrefix + sId, GetText(project, sId)), sDisplay, sInstructions);
        }

        private CDialogItem CreateDropDownInput(CProject project, string sDisplay, string sId, string sInstructions)
        {
            return new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix(sId),
                                               Forms.SelectOptions(new string[] { "No", "Yes" },
                                                                   GetText(project, sId),
                                                                   new string[] { "0", "1" })), sDisplay, sInstructions);
        }

        private CDialogItem CreateNumericInput(CProject project, string sDisplay, string sId, string sInstructions)
        {
            return new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix(sId),
                                               Forms.SelectOptions(new string[] { "2", "3", "4" },
                                                                   GetText(project, sId),
                                                                   new string[] { "2", "3" , "4"})), sDisplay, sInstructions);
        }
        

        public string[] ProjectDisplayListFields(CProject project)
        {
            return null;
            //return new string[] { 
            //    ExtractValue(project, sIntacctCompanyId)
            //};
        }

        private static string ExtractValue(CProject project, string id)
        {
            return HttpUtility.HtmlEncode(
                Convert.ToString(
                    project.GetPluginField(sPluginId, id)
                )
            );
        }

        public string[] ProjectDisplayListHeaders()
        {
            return null;
            //return new string[] { 
            //    "Intacct Company Id"
            //};
        }

        #endregion

        #region IPluginProjectCommit Members

        public void ProjectCommitAfter(CProject Project)
        {
           
        }

        public bool ProjectCommitBefore(CProject Project)
        {
            preCommitEnableIntacct = SetValue(Project, sEnableIntacct);
            preCommitIntacctUrl = SetValue(Project, sIntacctUrl);
            preCommitIntacctSenderId = SetValue(Project, sIntacctSenderId);
            preCommitIntacctSenderPassword = SetValue(Project, sIntacctSenderPassword);
            preCommitIntacctUserId = SetValue(Project, sIntacctUserId);
            preCommitIntacctUserPassword = SetValue(Project, sIntacctUserPassword);
            preCommitIntacctCompanyId = SetValue(Project, sIntacctCompanyId);
            preCommitIntacctLocationId = SetValue(Project, sIntacctLocationId);
            preCommitDailyMailAddress = SetValue(Project, sDailyMailAddress);
            SetValue(Project, sNoOfClientApprovers);
            Project.SetPluginField(sPluginId, sBackupLocation, "d:\\files\\consero");

            if ("1".Equals(preCommitEnableIntacct))
            {
                bool hasAllIntacctDetails = true;
                if (string.IsNullOrEmpty(preCommitIntacctUrl))
                {
                    api.Notifications.AddMessage("Intacct Url is Required.");
                    hasAllIntacctDetails = false;
                }
                if (string.IsNullOrEmpty(preCommitIntacctSenderId))
                {
                    api.Notifications.AddMessage("Intacct Sender Id is Required.");
                    hasAllIntacctDetails = false;
                }
                if (string.IsNullOrEmpty(preCommitIntacctSenderPassword))
                {
                    api.Notifications.AddMessage("Intacct Sender Password is Required.");
                    hasAllIntacctDetails = false;
                }
                if (string.IsNullOrEmpty(preCommitIntacctCompanyId))
                {
                    api.Notifications.AddMessage("Intacct Company Id is Required.");
                    hasAllIntacctDetails = false;
                }
                if (string.IsNullOrEmpty(preCommitIntacctUserId))
                {
                    api.Notifications.AddMessage("Intacct User Id is Required.");
                    hasAllIntacctDetails = false;
                }
                if (string.IsNullOrEmpty(preCommitIntacctUserPassword))
                {
                    api.Notifications.AddMessage("Intacct User Password is Required");
                    hasAllIntacctDetails = false;
                }

             //   if (string.IsNullOrEmpty(preCommitIntacctLocationId))
              //  {
              //      api.Notifications.AddMessage("Intacct Client Id is Required.");
              //      hasAllIntacctDetails = false;
              //  }
                if (hasAllIntacctDetails)
                {
                    api.Notifications.AddMessage("Location ID is." + preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "vendor", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "apterm", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "trxcurrencies", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                   // PostXMLTransaction(Project, "exchratetype", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                     //   preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                         PostXMLTransaction(Project, "glaccount", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "department", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "location", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "project", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "class", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "icitem", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);
                    PostXMLTransaction(Project, "bankaccount", preCommitIntacctUrl, preCommitIntacctSenderId, preCommitIntacctSenderPassword,
                        preCommitIntacctCompanyId, preCommitIntacctUserId, preCommitIntacctUserPassword, preCommitIntacctLocationId);

                    CreatePaymentMethods(Project.ixProject);
                }
            }

            return true;
        }

        private string SetValue(CProject Project, string fieldId)
        {
            string currentValue = string.Empty;

            if (api.Request[api.AddPluginPrefix(fieldId)] != null)
            {
                //string previousValue = Convert.ToString(Project.GetPluginField(sPluginId,fieldId));

                currentValue = Convert.ToString(api.Request[api.AddPluginPrefix(fieldId)]);

                Project.SetPluginField(sPluginId, fieldId,
                        Convert.ToString(api.Request[api.AddPluginPrefix(fieldId)]));
            }

            return currentValue;
        }

        public void ProjectCommitRollback(CProject Project)
        {
            //don't do anything
        }

        #endregion

        #region IPluginDatabase Members

        public CTable[] DatabaseSchema()
        {
            CTable ProjectIntacctSettings = api.Database.NewTable(api.Database.PluginTableName("IntacctSettings"));
            ProjectIntacctSettings.sDesc = "Assigns Intacct Settings Info to Clients";
            ProjectIntacctSettings.AddAutoIncrementPrimaryKey("ixIntacctSettings");
            ProjectIntacctSettings.AddIntColumn("ixProject", true, 1);
            ProjectIntacctSettings.AddVarcharColumn(sEnableIntacct, 1, true, "Enable Intacct", "Whether to Enable Intacct For This Client");
            ProjectIntacctSettings.AddVarcharColumn(sIntacctUrl, 255, true, "API Url", "The Intacct Sender Id - Integrator's Id");
            ProjectIntacctSettings.AddVarcharColumn(sIntacctSenderId, 30, true, "Sender Id", "The Intacct Sender Id - Integrator's Id");
            ProjectIntacctSettings.AddVarcharColumn(sIntacctSenderPassword, 30, true, "Sender Password", "The Intacct Sender Password - Integrator's Passwd");
            ProjectIntacctSettings.AddVarcharColumn(sIntacctUserId, 30, true, "User Id", "The Intacct user Id For The Client");
            ProjectIntacctSettings.AddVarcharColumn(sIntacctUserPassword, 30, true, "User Password", "The Intacct user passwd For The Client");
            ProjectIntacctSettings.AddVarcharColumn(sIntacctCompanyId, 30, true, "Company Id", "The Intacct Company Id For The Client");
            ProjectIntacctSettings.AddVarcharColumn(sIntacctLocationId, 30, true, "Client Id", "The Intacct Client Id For The Client");
            ProjectIntacctSettings.AddVarcharColumn(sDailyMailAddress, 100, true, "Daily Mail Address", "The Daily Mail Address For The Client");
            ProjectIntacctSettings.AddVarcharColumn(sBackupLocation, 100, true, "Backup Location", "The Backup Location For The Client");
            ProjectIntacctSettings.AddIntColumn(sNoOfClientApprovers, true, 2, "Number of Client Approvers");
            
            return new CTable[] { ProjectIntacctSettings, 
                CreateTable("GlVendor"),
                CreateTable("GlNetTerm"), 
                CreateTable("GlTrxCurrency"),
                CreateTable("GlExchratetype"),
                CreateTable("GlAccount"),
                CreateTable("GlDepartment"),
                CreateTable("GlLocation"),
                CreateTable("GlProject"),
                CreateTable("GlItem"),
                CreateTable("GlClass"),
                CreateTable("GlBankAccount"),
                CreateTable("GlPaymentMethod")
            };
        }

        private CTable CreateTable(string sType)
        {
            CTable table = api.Database.NewTable(api.Database.PluginTableName(sType));
            table.sDesc = sType + "s For Clients";
            table.AddAutoIncrementPrimaryKey("ix" + sType);
            table.AddIntColumn("ixProject", true, 1);
            table.AddVarcharColumn("s" + sType + "Id", 255, true, sType + " Id", sType + " Id");
            table.AddVarcharColumn("s" + sType + "Name", 255, true, sType + " Name", sType + " Name");
            return table;
        }

        public int DatabaseSchemaVersion()
        {
            return 1;
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

        /* Methods to created, read, update and delete entries in the database, using
         * fields in the plugin api request object */
        #region CRUD

        protected void Insert(string sType)
        {
            CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName(sType));
            insert.InsertString("s" + sType + "Id", api.Request[api.AddPluginPrefix("s" + sType + "Id")].ToString());
            insert.InsertString("s" + sType + "Name", api.Request[api.AddPluginPrefix("s" + sType + "Name")].ToString());
            insert.InsertInt("ixProject", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")]));
            insert.Execute();
        }

        protected void Insert(string sType, string sId, string sValue, int ixProject)
        {
            CSelectQuery select = api.Database.NewSelectQuery(api.Database.PluginTableName(sType));
            select.IgnorePermissions = true;
            select.AddWhere(api.Database.PluginTableName(sType) + ".ixProject = " + ixProject.ToString() + " and "
                + api.Database.PluginTableName(sType) + ".s" + sType + "Id = '" + sId + "'");
            select.AddSelect("*");
            DataSet ds = select.GetDataSet();
            if (null == ds.Tables || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
              //  api.Notifications.AddMessage("Success", "Adding " + sType + " " + sId + ":" + sValue);
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName(sType));
                insert.InsertString("s" + sType + "Id", sId);
                insert.InsertString("s" + sType + "Name", sValue);
                insert.InsertInt("ixProject", ixProject);
                insert.Execute();
            }
            else 
            {
               // api.Notifications.AddMessage("Skipped", "Skipped Adding " + sType + " " + sId + ":" + sValue);
            }
        }


        protected void InsertCurrency(string sType, string sId, string sValue, int ixProject)
        {
            CSelectQuery select = api.Database.NewSelectQuery(api.Database.PluginTableName(sType));
            select.IgnorePermissions = true;
            select.AddWhere(api.Database.PluginTableName(sType) + ".ixProject = " + ixProject.ToString() + " and "
                + api.Database.PluginTableName(sType) + ".s" + sType + "Id = '" + sId + "'");
            select.AddSelect("*");
            DataSet ds = select.GetDataSet();

            api.Notifications.AddMessage("row Count "+(ds.Tables[0].Rows.Count == 0).ToString());

            if (null == ds.Tables || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                api.Notifications.AddMessage("Success", "Adding " + sType + " " + sId + ":" + sValue);
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName(sType));
                insert.InsertString("s" + sType + "Id", sId);
             //   insert.InsertString("s" + sType + "Name", sValue);
                insert.InsertInt("ixProject", ixProject);
                insert.Execute();
            }
            else
            {
                // api.Notifications.AddMessage("Skipped", "Skipped Adding " + sType + " " + sId + ":" + sValue);
            }
        }

        protected void InsertExchratetype(string sType, string sId, string sValue, int ixProject)
        {
            CSelectQuery select = api.Database.NewSelectQuery(api.Database.PluginTableName(sType));
            select.IgnorePermissions = true;
            select.AddWhere(api.Database.PluginTableName(sType) + ".ixProject = " + ixProject.ToString() + " and "
                + api.Database.PluginTableName(sType) + ".s" + sType + "Id = '" + sId + "'");
            select.AddSelect("*");
            DataSet ds = select.GetDataSet();

            api.Notifications.AddMessage("row Count " + (ds.Tables[0].Rows.Count == 0).ToString());

            if (null == ds.Tables || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                api.Notifications.AddMessage("Success", "Adding " + sType + " " + sId + ":" + sValue);
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName(sType));
                insert.InsertString("s" + sType + "Id", sId);
                //   insert.InsertString("s" + sType + "Name", sValue);
                insert.InsertInt("ixProject", ixProject);
                insert.Execute();
            }
            else
            {
                // api.Notifications.AddMessage("Skipped", "Skipped Adding " + sType + " " + sId + ":" + sValue);
            }
        }
        protected DataSet Fetch(string sType, int ixProject)
        {
            CSelectQuery select = api.Database.NewSelectQuery(api.Database.PluginTableName(sType));
            select.IgnorePermissions = true;
            select.AddWhere(api.Database.PluginTableName(sType) + ".ixProject = " + ixProject.ToString());
            select.AddSelect("*");
            return select.GetDataSet();
        }

        protected void Update(string sType)
        {
            CUpdateQuery update = api.Database.NewUpdateQuery(api.Database.PluginTableName(sType));

            if (api.Request[api.AddPluginPrefix("s" + sType + "Name")] != null)
            {
                update.UpdateString("s" + sType + "Name", api.Request[api.AddPluginPrefix("s" + sType + "Name")]);
            }

            if (api.Request[api.AddPluginPrefix("s" + sType + "Id")] != null)
            {
                update.UpdateString("s" + sType + "Id", api.Request[api.AddPluginPrefix("s" + sType + "Id")]);
            }

            update.AddWhere("ix" + sType + "= @ix" + sType);
            update.SetParamInt("@ix" + sType, Convert.ToInt32(api.Request[api.AddPluginPrefix("ix" + sType)]));
            update.Execute();
        }

        protected void Delete(string sType)
        {
            CDeleteQuery delete =
                api.Database.NewDeleteQuery(api.Database.PluginTableName(sType));

            if (api.Request[api.AddPluginPrefix("ix" + sType)] != null)
            {
                delete.AddWhere("ix" + sType + " = @ix" + sType);
                delete.SetParamInt("@ix" + sType, Convert.ToInt32(api.Request[api.AddPluginPrefix("ix" + sType)]));
                delete.Execute();
            }
        }

        #endregion

        private string sTableId;

        protected CEditableTable EditableTable(int ixProject, string sType)
        {
            return EditableTable(ixProject, sType, false);        
        }

        private CMiniTable MiniTable(int ixProject, string sType)
        {
            CMiniTable miniTable = new CMiniTable();
            miniTable.sId = sType + "Table";
            miniTable.AddRow(new string[]{sType + " Id ", sType + "Name"});
            /* setup a DataSet and fetch the entries from the database */
            DataSet dsData = Fetch(sType, ixProject);
            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */
            if (dsData.Tables[0] != null && dsData.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                {
                    miniTable.AddRow(new string[] { 
                    dsData.Tables[0].Rows[i]["s" + sType + "Id"].ToString(),
                    dsData.Tables[0].Rows[i]["s" + sType + "Name"].ToString()});
                }
            }
            dsData.Dispose();
            return miniTable;
        }

        protected CEditableTable EditableTable(int ixProject, string sType, bool bDisplayAddNewRow)
        {
            CEditableTable editableTable = new CEditableTable(sType + "Table");
            sTableId = editableTable.sId;
            //editableTable.sWidth = "640px";
            /* Define the header row of the table */
            //editableTable.Header.AddCell("Edit");
            //editableTable.Header.AddCell("Delete");
            editableTable.Header.AddCell(sType + " Id");
            editableTable.Header.AddCell(sType + " Name");
            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTable.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */
            //CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sType);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = null;

            if (bDisplayAddNewRow)
            {
                dlgTemplateNew = DialogTemplateNew(ixProject, sType);
            }

            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            //CDialogTemplate dlgTemplateDelete = DialogTemplateDelete(sType);

            /* setup a DataSet and fetch the entries from the database */
            DataSet dsData = Fetch(sType, ixProject);
            string sEntryName = "none";
            string sEntryId = "none";
            int ixTypeValue = -1;;
            
            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */
            if (dsData.Tables[0] != null && dsData.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                {
                    ixTypeValue = Convert.ToInt32(dsData.Tables[0].Rows[i]["ix" + sType]);
                    sEntryName = dsData.Tables[0].Rows[i]["s" + sType + "Name"].ToString();
                    sEntryId = dsData.Tables[0].Rows[i]["s" + sType + "Id"].ToString();
                   
                    /* create a new table row and set the row id to the unique ixtype */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = ixTypeValue.ToString();
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    //row.AddCell(CEditableTable.LinkShowDialogEditIcon(
                    //                sTableId,
                    //                "edit",
                    //                row.sRowId,
                    //                CommandUrl("edit", sType, ixTypeValue, ixProject)));
                    //row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                    //                sTableId,
                    //                "delete",
                    //                row.sRowId,
                    //                CommandUrl("delete", sType, ixTypeValue, ixProject)));
                    /* make sure to run HtmlEncode on any user data before displaying it! */
                    row.AddCell(HttpUtility.HtmlEncode(sEntryId));
                    row.AddCell(HttpUtility.HtmlEncode(sEntryName));
                    editableTable.Body.AddRow(row);

                    /* Now that the row is populated for display, put the data in a hash table
                     * to be used in populating the pop-up add, edit and delete dialogs. */
                    Hashtable hashData = new Hashtable();
                    hashData.Add("ix" + sType, ixTypeValue.ToString());
                    hashData.Add("ixProject", ixProject.ToString());
                    hashData.Add("s" + sType + "Id", HttpUtility.HtmlEncode(sEntryId));
                    hashData.Add("s" + sType + "Name", HttpUtility.HtmlEncode(sEntryName));

                    /* add the hash table as data to the edit template */
                    //dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                    /* add the data to the delete template as well */
                    //dlgTemplateDelete.AddTemplateData(row.sRowId, hashData);
                }
            }
            else
            {
                /* If there are no entries, just display a note in a full-width cell */
                CEditableTableRow row = new CEditableTableRow();
                row.sRowId = "none";
                row.AddCellWithColspan("No " + sType + "s Yet...", nColCount);
                editableTable.Body.AddRow(row);
            }
            dsData.Dispose();

            if (bDisplayAddNewRow)
            {
                /* Add a footer row with icon and text links to the add new dialog */
                editableTable.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", sType, ixTypeValue, ixProject)));
                editableTable.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", sType, ixTypeValue, ixProject),
                                                        "Add New " + sType),
                                                        nColCount - 1);

                /* Associate the dialog templates with the table by name */
                editableTable.AddDialogTemplate("new", dlgTemplateNew);
                //editableTable.AddDialogTemplate("edit", dlgTemplateEdit);
                //editableTable.AddDialogTemplate("delete", dlgTemplateDelete);
            }

            return editableTable;
        }

        protected CDialogTemplate DialogTemplateNew(int ixProject, string sType)
        {
            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();
            dlgTemplateNew.Template.sTitle = "Add New " + sType;

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
                                                   api.AddPluginPrefix("ixProject"),
                                                   ixProject.ToString()));
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("sType"),
                                                   sType.ToString()));

            CDialogItem itemEditId =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("s" + sType +"Id"),""),
                                sType + " Id");
            dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemNewName =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("s"+ sType +"Name"), ""),
                                sType + " Name");
            dlgTemplateNew.Template.Items.Add(itemNewName);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateNew;
        }

        protected CDialogTemplate DialogTemplateEdit(string sType)
        {
            CDialogTemplate dlgTemplateEdit = new CDialogTemplate();
            dlgTemplateEdit.Template = new CDoubleColumnDialog();
            /* names in curly braces are replaced with the otuput of the ToString()
             * method for the corresponding value in the template's data hashtable */
            dlgTemplateEdit.Template.sTitle = "Edit " + sType + " id {ix" + sType + "}: \"{s" + sType + "Name}\"";
            CDialogItem itemEditHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateEdit.Template.Items.Add(itemEditHiddenUrl);
            CDialogItem itemEditHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
            dlgTemplateEdit.Template.Items.Add(itemEditHiddenAction);
            dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
                                                    api.AddPluginPrefix("ix" + sType),
                                                    "{ix" + sType + "}"));
            dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixProject"),
                                                   "{ixProject}"));
            dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("sType"),
                                                   sType.ToString()));

            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateEdit.Template.Items.Add(itemActionToken);

            CDialogItem itemEditId =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("s" + sType + "Id"),
                                                "{s" + sType + "Id}"),
                                sType + " Id");
            dlgTemplateEdit.Template.Items.Add(itemEditId);

            CDialogItem itemEditName =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("s" + sType + "Name"),
                                                "{s" + sType + "Name}"),
                                sType + " Name");
            dlgTemplateEdit.Template.Items.Add(itemEditName);

            /* Standard ok and cancel buttons */
            dlgTemplateEdit.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateEdit;
        }

        protected CDialogTemplate DialogTemplateDelete(string sType)
        {
            CDialogTemplate dlgTemplateDelete = new CDialogTemplate();
            dlgTemplateDelete.Template = new CSingleColumnDialog();
            dlgTemplateDelete.Template.sTitle = "Delete " + sType + " ID {ix" + sType + "}: \"{s" + sType + "Name}\"";
            dlgTemplateDelete.Template.Items.Add(
                CDialogItem.HiddenInput(api.AddPluginPrefix("ix" + sType), "{ix" + sType + "}"));
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixProject"),
                                                   "{ixProject}"));
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("sType"),
                                                   sType.ToString()));

            CDialogItem itemDeleteHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateDelete.Template.Items.Add(itemDeleteHiddenUrl);
            CDialogItem itemDeleteHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "delete");
            dlgTemplateDelete.Template.Items.Add(itemDeleteHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateDelete.Template.Items.Add(itemActionToken);

            /* DialogItems don't have to be form elements, they can also be simple html */
            //dlgTemplateDelete.Template.Items.Add(
            //    new CDialogItem("Do you want to delete " + sType +" {ix" + sType + "} '{s" + sType + "Name}'?")
            //);

            dlgTemplateDelete.Template.Items.Add(
                new CDialogItem("Deletes should be done on Intacct.")
            );

            /* Standard ok and cancel buttons */
            dlgTemplateDelete.Template.Items.Add(CEditableTable.DialogItemDismissDialog(sTableId));

            return dlgTemplateDelete;
        }

        /* these two methods are used to construc the Urls which a user would
         * follow if javascript is disabled (preventing the use of the Dialogs */

        protected string CommandUrl(string sCommand, string sType, int typeValue, int ixProject)
        {
            return string.Concat(api.Url.PluginPageUrl(),
                                 LinkParameter("sCommand", sCommand),
                                 LinkParameter("sType", sType),
                                 LinkParameter("ix" + sType, typeValue.ToString()),
                                 LinkParameter("ixProject", ixProject.ToString()));
        }

        protected string LinkParameter(string sName, string sValue)
        {
            return string.Format("&{0}={1}", api.AddPluginPrefix(sName), sValue);
        }

        #region IPluginRawPageDisplay Members

        public string RawPageDisplay()
        {
            /* check for a valid action token in the request before processing to
             * prevent cross site request forgery */
            if ((api.Request[api.AddPluginPrefix("sAction")] != null) &&
                (api.Request[api.AddPluginPrefix("actionToken")] != null) &&
                api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")]))
            {
                string sType = api.Request[api.AddPluginPrefix("sType")].ToString();
                switch (api.Request[api.AddPluginPrefix("sAction")].ToString())
                {
                    case "new":
                        Insert(sType);
                        break;
                    case "edit":
                        Update(sType);
                        break;
                    case "delete":
                        Delete(sType);
                        break;
                    default:
                        return string.Empty;
                }
                /* you should really handle errors but we're keeping it simple */
                /* return the updated table as xml so FogBugz can update the page */
                api.Response.ContentType = "text/xml";
                return EditableTable(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), sType).RenderXml();
            }
            else
                return string.Empty;
        }

        public PermissionLevel RawPageVisibility()
        {
            return PermissionLevel.Normal;
        }

        #endregion

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert)]
        public XmlDocument PostXMLTransaction(CProject project, string sType, 
            string sIntacctUrl, string sSenderId, string sSenderPassword,
            string sCompanyId, string sUserId, string sUserPassword, string sLocationId
            )
        {
            //Company ID: demo58970749
            //User ID: guest
            //Password: 46dbd2c4

            //A production XML gateway license has been created for you.  Following are instructions for posting to the production XML gateway:

            //URL: https://www.intacct.com/ia/xml/xmlgw.phtml
            //Your Sender ID is: consero
            //Your Password is: HubeJAJu$e
           // api.Notifications.AddMessage("The credentials "+project + "," + sType + "," + sSenderId + "," + sSenderPassword + "," + sCompanyId + "," + sUserId + "," + sUserPassword + "," + sLocationId);
            
            
            MemoryStream ms = Xml(project, sType, sSenderId, sSenderPassword, sCompanyId, sUserId, sUserPassword, sLocationId);
            
            //Declare XMLResponse document
            XmlDocument xmlResponse = null;
           
            //Declare an HTTP-specific implementation of the WebRequest class.
            HttpWebRequest objHttpWebRequest;
           
            //Declare an HTTP-specific implementation of the WebResponse class
            HttpWebResponse objHttpWebResponse = null;
          
            //Declare a generic view of a sequence of bytes
            Stream objRequestStream = null;
            Stream objResponseStream = null;

            //Declare XMLReader
            XmlTextReader objXMLReader;

            //Creates an HttpWebRequest for the specified URL.
            objHttpWebRequest = (HttpWebRequest)WebRequest.Create(sIntacctUrl);

            try
            {
                //---------- Start HttpRequest 

            

                //string xmlToPost = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                //xmlToPost = HttpUtility.HtmlEncode(xmlToPost);
                //api.Notifications.AddAdminNotification("XML to Post", "\"" + xmlToPost.ToString() + "\"");

                //Set HttpWebRequest properties
                objHttpWebRequest.Method = "POST";
    
                objHttpWebRequest.ContentLength = ms.Length;
                
                objHttpWebRequest.ContentType = "x-intacct-xml-request";
              //  api.Notifications.AddMessage("2e3");



               // ServicePointManager.ServerCertificateValidationCallback =  CertChecker;
              //  api.Notifications.AddMessage("2f");
                try
                {
                    //Get Stream object 
                  
                    objRequestStream = objHttpWebRequest.GetRequestStream();

                    ms.WriteTo(objRequestStream);

                    ms.Close();
                  
                }
                catch (Exception ex)
                {
                    api.Notifications.AddAdminNotification("error while posting xml", ex.ToString());
                }
                finally
                {
                    //Close stream
                    objRequestStream.Close();
                }

                //---------- End HttpRequest

                //Sends the HttpWebRequest, and waits for a response.
                objHttpWebResponse = (HttpWebResponse)objHttpWebRequest.GetResponse();

                //---------- Start HttpResponse
              
                if (objHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    //Get response stream 
                    objResponseStream = objHttpWebResponse.GetResponseStream();

                    //Load response stream into XMLReader
                    objXMLReader = new XmlTextReader(objResponseStream);

                    //Declare XMLDocument
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(objXMLReader);

                    //Set XMLResponse object returned from XMLReader
                    xmlResponse = xmldoc;

                    //Close XMLReader
                    objXMLReader.Close();
                }

                //Close HttpWebResponse
                objHttpWebResponse.Close();
            }
            catch (WebException we)
            {
                //TODO: Add custom exception handling
                api.Notifications.AddAdminNotification("Error while posting to Intacct", we.ToString());
            }
            catch (Exception ex)
            {
                api.Notifications.AddAdminNotification("Error while posting to Intacct", ex.ToString());
            }
            finally
            {

                //Close connections
                objRequestStream.Close();
                objResponseStream.Close();
                objHttpWebResponse.Close();

                //Release objects
                objXMLReader = null;
                objRequestStream = null;
                objResponseStream = null;
                objHttpWebResponse = null;
                objHttpWebRequest = null;
            }

            //if (null != XMLResponse)
            //{
            //    api.Notifications.AddAdminNotification("XML Response to Intacct", "\"" + XMLResponse.InnerXml + "\"");
            //}
          
            switch (sType) {
                case "vendor": CreateVendors(xmlResponse, project.ixProject); break;
                case "apterm": CreateNetTerms(xmlResponse, project.ixProject); break;
                case "trxcurrencies": CreateTrxCurrencies(xmlResponse, project.ixProject); break;
                //case "exchratetype": CreateExchratetype(xmlResponse, project.ixProject); break;
                case "glaccount": CreateAccounts(xmlResponse, project.ixProject); break;
                case "department": CreateDepartments(xmlResponse, project.ixProject); break;
                case "project": CreateProjects(xmlResponse, project.ixProject); break;
                case "location": CreateLocations(xmlResponse, project.ixProject); break;
                case "class": CreateClasses(xmlResponse, project.ixProject); break;
                case "icitem": CreateItems(xmlResponse, project.ixProject); break;
                case "bankaccount": CreateBankAccounts(xmlResponse, project.ixProject); break;
                
                    
            }
            //Return
            return xmlResponse;
        }

        private void CreateVendors(XmlDocument xmlResponse, int ixProject ) 
        {
           
            string sVendorId = "";
            string sVendorName = "--NONE--";
            string sType = "GlVendor";

            //Insert a Dummy Row
            //Insert(sType, sVendorId, sVendorName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/vendor");
            api.Notifications.AddMessage("Vendor Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors) 
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "vendorid": sVendorId = childNode.InnerText; break;
                            case "name": sVendorName = childNode.InnerText; break;
                        }
                    }

                    if (!string.IsNullOrEmpty(sVendorId) && !string.IsNullOrEmpty(sVendorName))
                    {
                        Insert(sType, sVendorId, sVendorName, ixProject);
                    }
                }
                else 
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreateNetTerms(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "--NONE--";
            string sType = "GlNetTerm";

            //Insert a Dummy Row
            //Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/apterm");
            api.Notifications.AddMessage("Net Term Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "due": sId = childNode.ChildNodes[0].InnerText; break;
                            case "name": sName = childNode.InnerText; break;

                                
                        }
                    }

                    if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    {
                        Insert(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }
     
        // code added by Ravi For SB on 29/05/2012

        private void CreateTrxCurrencies(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "";
            string sType = "GlTrxCurrency";

            //Insert a Dummy Row
            //Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/trxcurrencies");
            api.Notifications.AddMessage("Transaction currency Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "code": sId = childNode.ChildNodes[0].InnerText; break;
                           // case "name": sName = childNode.InnerText; break;

                                
                        }
                    }

                   // api.Notifications.AddMessage("curr code " + sId);
                   // api.Notifications.AddMessage("stype " + sType);
                   // api.Notifications.AddMessage("sname " + sName);
                   // api.Notifications.AddMessage("ixProject " + ixProject);

                   // if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    if (!string.IsNullOrEmpty(sId))
                    {
                        InsertCurrency(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }
        private void CreateExchratetype(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "";
            string sType = "GlExchratetype";

            //Insert a Dummy Row
            //Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/exchratetype");
            api.Notifications.AddMessage("exchratetype Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "code": sId = childNode.ChildNodes[0].InnerText; break;
                            // case "name": sName = childNode.InnerText; break;


                        }
                    }

                    // api.Notifications.AddMessage("curr code " + sId);
                    // api.Notifications.AddMessage("stype " + sType);
                    // api.Notifications.AddMessage("sname " + sName);
                    // api.Notifications.AddMessage("ixProject " + ixProject);

                    // if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    if (!string.IsNullOrEmpty(sId))
                    {
                        InsertExchratetype(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreateAccounts(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "--NONE--";
            string sType = "GlAccount";

            //Insert a Dummy Row
            //Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/glaccount");
            api.Notifications.AddMessage("Account Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "glaccountno": sId = childNode.ChildNodes[0].InnerText; break;
                            case "title": sName = childNode.InnerText; break;
                        }
                    }

                    if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    {
                        Insert(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreateDepartments(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "--NONE--";
            string sType = "GlDepartment";

            //Insert a Dummy Row
            //Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/department");
            api.Notifications.AddMessage("Department Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "departmentid": sId = childNode.ChildNodes[0].InnerText; break;
                            case "title": sName = childNode.InnerText; break;
                        }
                    }

                    if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    {
                        Insert(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreateLocations(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "--NONE--";
            string sType = "GlLocation";

            //Insert a Dummy Row
            //Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/location");
            api.Notifications.AddMessage("Location Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "locationid": sId = childNode.ChildNodes[0].InnerText; break;
                            case "name": sName = childNode.InnerText; break;
                        }
                    }

                    if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    {
                        Insert(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreateProjects(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "--NONE--";
            string sType = "GlProject";

            //Insert a Dummy Row
            Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/project");
            api.Notifications.AddMessage("Project Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "key": sId = childNode.ChildNodes[0].InnerText; break;
                            case "name": sName = childNode.InnerText; break;
                        }
                    }

                    if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    {
                        Insert(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreateClasses(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "--NONE--";
            string sType = "GlClass";

            //Insert a Dummy Row
            Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/class");
            api.Notifications.AddMessage("Class Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "key": sId = childNode.ChildNodes[0].InnerText; break;
                            case "name": sName = childNode.InnerText; break;
                        }
                    }

                    if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    {
                        Insert(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreateItems(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "--NONE--";
            string sType = "GlItem";

            //Insert a Dummy Row
            Insert(sType, sId, sName, ixProject);

            XmlNodeList vendors = xmlResponse.SelectNodes("/response/operation/result/data/icitem");
            api.Notifications.AddMessage("Item Count is " + vendors.Count.ToString());
            foreach (XmlNode vendor in vendors)
            {
                if (vendor.HasChildNodes)
                {
                    foreach (XmlNode childNode in vendor.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "itemid": sId = childNode.ChildNodes[0].InnerText; break;
                            case "itemname": sName = childNode.InnerText; break;
                        }
                    }

                    if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    {
                        Insert(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreateBankAccounts(XmlDocument xmlResponse, int ixProject)
        {
            string sId = "";
            string sName = "--NONE--";
            string sType = "GlBankAccount";

            XmlNodeList nodes = xmlResponse.SelectNodes("/response/operation/result/data/bankaccount");
            api.Notifications.AddMessage("Bank Count is " + nodes.Count.ToString());
            foreach (XmlNode node in nodes)
            {
                if (node.HasChildNodes)
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        switch (childNode.Name)
                        {
                            case "bankaccountid": sId = childNode.ChildNodes[0].InnerText; break;
                            case "bankname": sName = childNode.InnerText; break;
                        }
                    }

                    if (!string.IsNullOrEmpty(sId) && !string.IsNullOrEmpty(sName))
                    {
                        Insert(sType, sId, sName, ixProject);
                    }
                }
                else
                {
                    api.Notifications.AddMessage("Whoops a Daisy. No Child Nodes");
                }
            }
        }

        private void CreatePaymentMethods(int ixProject)
        {
            //<!-- ENUMS: "Printed Check", "Online", "Cash", "EFT", "Credit Card", "Online Charge Card", "WF Check", "WF Domestic ACH", "WF USD Wire" -->
            string sType = "GlPaymentMethod";
            Insert(sType, "Printed Check", "Printed Check", ixProject);
            Insert(sType, "Online", "Online", ixProject);
            Insert(sType, "Cash", "Cash", ixProject);
            Insert(sType, "EFT", "EFT", ixProject);
            Insert(sType, "Credit Card", "Credit Card", ixProject);
            Insert(sType, "Online Charge Card", "Online Charge Card", ixProject);
            Insert(sType, "WF Check", "WF Check", ixProject);
            Insert(sType, "WF Domestic ACH", "WF Domestic ACH", ixProject);
            Insert(sType, "WF USD Wire", "WF USD Wire", ixProject);
        }

        private static MemoryStream Xml(CProject project, string sType,
            string sSenderId, string sSenderPwd, string sCompanyId, string sUserId, string sUserPwd, string sLocationId)
        {
           // api.get.Notifications.AddMessage("Intacct Client Id is Required.");
          

            XmlWriterSettings wSettings = new XmlWriterSettings();
            MemoryStream ms = new MemoryStream();
            XmlWriter xw = XmlWriter.Create(ms, wSettings);
            // Write Declaration
            xw.WriteStartDocument();
            xw.WriteDocType("request", null, "intacct_request.v2.1.dtd", null);

            // Write the root node
            xw.WriteStartElement("request");

            // Write the control and the control elements
            xw.WriteStartElement("control");
            xw.WriteStartElement("senderid");
            xw.WriteString(sSenderId);
            xw.WriteEndElement();
            xw.WriteStartElement("password");
            xw.WriteString(sSenderPwd);
            xw.WriteEndElement();
            xw.WriteStartElement("controlid");
            xw.WriteString("controlid");
            xw.WriteEndElement();
            xw.WriteStartElement("uniqueid");
            xw.WriteString("false");
            xw.WriteEndElement();
            xw.WriteStartElement("dtdversion");
            xw.WriteString("2.1");
            xw.WriteEndElement();
            xw.WriteEndElement();
            // Write the operation and the operation elements
            xw.WriteStartElement("operation");
            xw.WriteStartElement("authentication");
            xw.WriteStartElement("login");
            xw.WriteStartElement("userid");
            xw.WriteString(sUserId);
            xw.WriteEndElement();
            xw.WriteStartElement("companyid");
            xw.WriteString(sCompanyId);
            xw.WriteEndElement();
            xw.WriteStartElement("password");
            xw.WriteString(sUserPwd);
            xw.WriteEndElement();
            xw.WriteStartElement("locationid");
            xw.WriteString(sLocationId);
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteStartElement("content");
            xw.WriteStartElement("function");
            xw.WriteStartAttribute("controlid");
            xw.WriteString(project.ixProject.ToString());
            xw.WriteEndAttribute();
            xw.WriteStartElement("get_list");
            xw.WriteStartAttribute("object");
            xw.WriteString(sType);
            xw.WriteEndAttribute();
            //xw.WriteStartElement("fields");
            //xw.WriteStartElement("field");
            //xw.WriteString("vendorid");
            //xw.WriteEndElement();
            //xw.WriteStartElement("field");
            //xw.WriteString("name");
            //xw.WriteEndElement();
            //xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();

            //close the root element
            xw.WriteEndElement();
            // Close the document
            xw.WriteEndDocument();
            // Flush the write
            xw.Flush();
            return ms;
        }

        static bool CertChecker (object sender, X509Certificate certificate,
                         X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

    }
}