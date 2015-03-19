using System.Data;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Net;
using System.Security.Permissions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System;
using System.Web;

namespace Solarbridge
{
    public class Solarbridge : Plugin, IPluginBugJoin,IPluginRawPageDisplay,
        IPluginBugDisplay, IPluginBugCommit, IPluginDatabase,  IPluginGridColumn
    {
        protected const string PLUGIN_ID =
         "Solarbridge@conseroglobal.com";

        /* A constant for populating the "code name" input field for multiple case edit */
        protected const string VARIOUS_TEXT = "[various]";
        private string sPrefixedTableName;


        /* Constructor: We'll just initialize the inherited Plugin class, which 
         * takes the passed instance of CPluginApi and sets its "api" member variable. */
        public Solarbridge(CPluginApi api)
            : base(api)
        {
            sPrefixedTableName = api.Database.PluginTableName("SLRHeader");
        }

        #region IPluginBugJoin Members

        public string[] BugJoinTables()
        {
            /* All tables specified here must have an integer ixBug column so FogBugz can
            * perform the necessary join. */

            return new string[] { "SLRHeader" };
        }

        #endregion

        #region IPluginBugDisplay Members

        public CBugDisplayDialogItem[]
            BugDisplayEditLeft(CBug[] rgbug, BugEditMode nMode, bool fPublic)
        {
            //if (fPublic)
            return null;

            // /* We're returning 1 dialog item, an input text box allowing the user to 
            //* enter a code name */
        }

        public CBugDisplayDialogItem[] BugDisplayEditTop(CBug[] rgbug,
            BugEditMode nMode, bool fPublic)
        {
            /* don't show non-logged in users the invoice details */
            if (fPublic)
                return null;

            /* don't show multi-edits the invoice details */
            if (nMode != BugEditMode.Edit)
            {
                if (rgbug.Length != 1)
                    return null;
            }

            CBug bug = rgbug[0];

                     

            if (nMode == BugEditMode.Edit)
            {
               
                if (bug.ixProject == 20)
                {
                         return new CBugDisplayDialogItem[] 
                  {
                    
                       new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                     
                        CreateListField(rgbug, "CustomForm", "Header Desc", "SLRCustomForm", "SLRCustomForm", true),
                          CreateListField(rgbug, "Vendor", "Vendor Name", "SLRVendor", "SLRVendor", true),
                          CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "SInvoiceNumber"),
                           CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "SInvoiceDate"),
                           CreateTextInputField(rgbug, "Invoice Amount", "Invoice Amount", "SInvoiceAmount"),
                            CreateTextInputField(rgbug, "PO Number", "PO Number", "SPONumber"),
                              CreateListField(rgbug, "Type", "Type", "SLRType", "SLRType", true),
                              CreateTextInputField(rgbug, "Memo", "Memo", "SMemo"),
                          
                     };
                    
                  
                }

                else if (bug.ixProject == 3)
                {
                    return new CBugDisplayDialogItem[] 
                  {
                    
                      
                        CreateListField(rgbug, "BPM/Projects", "BPM/Projects", "SLRCustomForm", "SLRCustomForm", true),
                        CreateListField(rgbug, "LO", "LO", "SLRType", "SLRType", true),
                     //   CreateListField(rgbug, "Client", "Client", "SLRVendor", "SLRVendor", true),
                                                   
                     };


                }
            

                else
                {

                     return null;
                      //new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   };

        }



            if (nMode == BugEditMode.Resolve)
            {
                if (bug.ixProject == 20)
                {
                    return new CBugDisplayDialogItem[]
                    {
                         
                          new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                            CreateText(rgbug, "CustomForm", "Header Desc", "SLRCustomForm", true,"SLRCustomForm"),
                          CreateText(rgbug, "Vendor", "Vendor Name", "SLRVendor", true ,"SLRVendor"),
                          CreateText(rgbug, "InvoiceNumber", "Invoice Number", "SInvoiceNumber"),
                           CreateText(rgbug, "InvoiceDate", "Invoice Date", "SInvoiceDate"),
                           CreateText(rgbug, "Invoice Amount", "Invoice Amount", "SInvoiceAmount"),
                            CreateText(rgbug, "PO Number", "PO Number", "SPONumber"),
                            CreateText(rgbug, "Type", "Type", "SLRType", true,"SLRType"),
                             CreateText(rgbug, "Memo", "Memo", "SMemo"),
                   };
                   
                }



                else if (bug.ixProject == 3)
                {
                    return new CBugDisplayDialogItem[]
                    {
                         
                        CreateText(rgbug, "BPM/Projects", "BPM/Projects", "SLRCustomForm", true,"SLRCustomForm"),
                          CreateText(rgbug, "LO", "LO", "SLRType", true,"SLRType"),
                          // CreateText(rgbug, "Client", "Client", "SLRVendor", true,"SLRVendor"),
                                                   
                   };

                }

                else
                {
                    return null;
                }

                               
            }
            return null;
            }
        

        private CBugDisplayDialogItem CreateTextInputField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            System.Collections.IDictionary dictionary = new System.Collections.Specialized.ListDictionary();
            dictionary.Add("required", "true");
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = Forms.TextInput(api.PluginPrefix + fieldName, GetText(rgbug, fieldName), dictionary);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateTextInputField_memo(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            System.Collections.IDictionary dictionary = new System.Collections.Specialized.ListDictionary();
            dictionary.Add("required", "true");
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = Forms.TextareaInput(api.PluginPrefix + fieldName, GetText(rgbug, fieldName), 10, dictionary);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateDateInputField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            try
            {
                DialogItem.sContent = Forms.DateInputCTZ(api.PluginPrefix + fieldName, api.PluginPrefix + fieldName, Convert.ToDateTime(GetText(rgbug, fieldName)));
            }
            catch
            {
                DialogItem.sContent = Forms.DateInputCTZ(api.PluginPrefix + fieldName, api.PluginPrefix + fieldName, DateTime.Now);
            }
            return DialogItem;
        }

        
        private CBugDisplayDialogItem CreateListField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
           
            DialogItem.sContent = GetSelects(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
             
            
            return DialogItem;
        }


        public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
        {
          
            //Newly Added code by Alok
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

            if (rgbug[0].ixProject != 20)
            {
                return null;
            }
            if (rgbug[0].ixProject == 20)
            {
                return new CBugDisplayDialogItem[] {
                new CBugDisplayDialogItem("Vendor_1", EditableTable_Vend(rgbug[0].ixBug).RenderHtml())
             };
            }

            else  
            {
                return new CBugDisplayDialogItem[] 
                  {
                  
                     CreateListField(rgbug, "Client", "Client", "SLRVendor", "SLRVendor", true),
                                                   
                    };
            }

        

        }

        public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
        {
            /* don't show non-logged in users the invoice details */
            if (fPublic)
                return null;

            /* don't show multi-edits the invoice details */
            if (rgbug.Length != 1)
                return null;

            CBug bug = rgbug[0];

         
            if (bug.ixProject == 20)
            {
                return new CBugDisplayDialogItem[] 
                   {
          
                        new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                          CreateText(rgbug, "CustomForm", "Header Desc", "SLRCustomForm", true,"SLRCustomForm"),
                          CreateText(rgbug, "Vendor", "Vendor Name", "SLRVendor", true ,"SLRVendor"),
                          CreateText(rgbug, "InvoiceNumber", "Invoice Number", "SInvoiceNumber"),
                           CreateText(rgbug, "InvoiceDate", "Invoice Date", "SInvoiceDate"),
                           CreateText(rgbug, "Invoice Amount", "Invoice Amount", "SInvoiceAmount"),
                            CreateText(rgbug, "PO Number", "PO Number", "SPONumber"),
                            CreateText(rgbug, "Type", "Type", "SLRType", true,"SLRType"),
                              CreateText(rgbug, "Memo", "Memo", "SMemo"),
                            
                   };
            }



            else if (bug.ixProject == 3)
            {
                return new CBugDisplayDialogItem[]
                    {
                         
                        CreateText(rgbug, "BPM/Projects", "BPM/Projects", "SLRCustomForm", true,"SLRCustomForm"),
                          CreateText(rgbug, "LO", "LO", "SLRType", true,"SLRType"),
                         // CreateText(rgbug, "Client", "Client", "SLRVendor", true,"SLRVendor"),
                   };

            }
            
             else
            {
                return null;
            }
        }

   
        

        private CBugDisplayDialogItem CreateText(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            return CreateText(rgbug, itemName, fielddisplay, fieldName, false, null);
        }



        private CBugDisplayDialogItem CreateText(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
            // if (bLookup)
            // {
            //   sValue = QueryDbForValue(sTableName, sValue);
            // }
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
            return DialogItem;

        }


        #endregion

        #region IPluginBugCommit Members

        public static void CreateDirectory(DirectoryInfo directory)
        {
            if (!directory.Parent.Exists)
                CreateDirectory(directory.Parent);
            directory.Create();
        }

        public void BugCommitAfter(CBug bug, BugAction nBugAction, CBugEvent bugevent,
            bool fPublic)
        {
            if (bugevent.EventType == BugEventType.Edited || bugevent.EventType == BugEventType.Assigned)
            {

                if (bug.ixProject != 20)
                {
                    if (bug.ixProject != 3)
                    {
                        return;
                    }
                }

                if (bug.ixStatus == 131)
                {
                    //string Vendor_Name = "";
                   // api.Notifications.AddMessage("calling rename");
                    RenameFile(bug, bugevent);
                  //  api.Notifications.AddMessage("calling rename1");

                }

            }
        }

        public void BugCommitBefore(CBug bug, BugAction nBugAction, CBugEvent bugevent,
            bool fPublic)
        {

            CProject project = api.Project.GetProject(bug.ixProject);
            {
               // api.Notifications.AddMessage("Vendor" + (bug.GetPluginField(PLUGIN_ID, "SVendor")).ToString().Trim());
               ExtractValue(bug, bugevent, "SLRCustomForm", "Header Desc");
               ExtractValue(bug, bugevent, "SLRVendor", "Vendor Name");
                ExtractValue(bug, bugevent, "SInvoiceNumber", "Invoice Number");
                ExtractValue(bug, bugevent, "SInvoiceDate", "Invoice Date");
               ExtractValue(bug, bugevent, "SInvoiceAmount", "Invoice Amount");
               ExtractValue(bug, bugevent, "SPONumber", "PO Number");
               ExtractValue(bug, bugevent, "SMemo", "sMemo");
              ExtractValue(bug, bugevent, "SLRType", "Type");
                

               {

                   int i = 0;

                    //string Vendor_Name = "-";
                    string mailsub = "", mailbody = "";
                    int iperson = 0;
                    // string Invoice_no = "-";
                    int old_inv_bug = 0;
                    //int dupe = 0;
                    try
                    {
                        string vendor_1 = (bug.GetPluginField(PLUGIN_ID, "SLRVendor")).ToString().Trim();
                        string InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "SInvoiceNumber")).ToString().Trim();

                        vendor_1 = vendor_1.Replace("'", "''");
                        if (vendor_1.Trim() == "-" || InvNo_1.Trim() == "-")
                        {
                            return;
                        }
                        if (vendor_1.Trim() != "-")
                        {
                            if (InvNo_1.Trim() != string.Empty || InvNo_1.Trim() != "")
                            {
                                {
                                    //this.api.Notifications.AddMessage("1");


                                    CSelectQuery Dupcheck2 = api.Database.NewSelectQuery(api.Database.PluginTableName("Solarbridge@conseroglobal.com", "SLRHeader"));
                                    Dupcheck2.AddSelect("ixbug");
                                    Dupcheck2.AddWhere("SLRVendor = " + "'" + vendor_1 + "'");
                                    Dupcheck2.AddWhere("SInvoiceNumber = " + "'" + InvNo_1 + "'");
                                    Dupcheck2.AddWhere("ixbug > " + bug.ixBug.ToString() + " OR ixbug < " + bug.ixBug.ToString());
                                    // this.api.Notifications.AddMessage("DUPLICATE");
                                    DataSet d_1 = Dupcheck2.GetDataSet();

                                    if (null != d_1.Tables && d_1.Tables.Count == 1 && d_1.Tables[0].Rows.Count > 0)
                                    {
                                        //Vendor_Name = Convert.ToString(d_1.Tables[0].Rows[0]["CWFVendor"]);
                                        // Invoice_no = Convert.ToString(d_1.Tables[0].Rows[0]["sInvoiceNumber"]);
                                        old_inv_bug = Convert.ToInt32(d_1.Tables[0].Rows[0]["ixbug"]);

                                        this.api.Notifications.AddError("--------------------------------------------------------------------------");
                                        this.api.Notifications.AddError("***DUPLICATE BILL****");
                                        this.api.Notifications.AddMessage("It seems An Invoice is already existing for the same vendor with ( case Id " + old_inv_bug + " )");
                                        this.api.Notifications.AddMessage("Please verify again");
                                        this.api.Notifications.AddError("-------------------------------------------------------------------------");

                                        mailsub = "Duplicate Invoice for SolarBridge in AP Workflow";
                                        mailbody = "It seems same invoice number " + InvNo_1.Trim() + " is already existing for the vendor " + vendor_1.Trim();
                                        iperson = bug.ixPersonAssignedTo;
                                     //   mailsender("vinaykumar.p@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        mailsender("lokesha.s@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        mailsender("ravichandra.b@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        mailsender("poornima.r@conseroglobal.com", bug, mailsub, mailbody, iperson);

                                        i = 1;
                                    }
                                }


                            }
                        }
                    }
                    catch
                    { 
                    }
                }



            }
        }

        private bool ExtractValue(CBug bug, CBugEvent bugevent, string fieldName, string fieldDisplay)
        {

            //api.Notifications.AddMessage("fieldName:" + fieldName + "    fieldDisplay:" + fieldDisplay);
            bool valueChanged = false;
            //api.Notifications.AddMessage("prefix:" + Convert.ToString(api.Request[api.AddPluginPrefix("SType")]));

            string sNewValue = Convert.ToString(api.Request[api.AddPluginPrefix(fieldName)]);
           // string sNewValue1 = Convert.ToString(bug.GetPluginField("Solarbridge@conseroglobal.com", "SType"));
            
            //string sNewValue = "PO";
            //api.Notifications.AddMessage("fieldName:" + fieldName + "    sNewValue:" + sNewValue);
           

            if (string.IsNullOrEmpty(sNewValue))
            {

            }
            else
            {
                string preCommitValue = Convert.ToString(bug.GetPluginField(PLUGIN_ID, fieldName));
                /* if the field changed, set the plugin field and record it in the BugEvent */
                if (sNewValue != preCommitValue)
                {
                    //api.Notifications.AddMessage("fieldName:" + fieldName + "    sNewValue:" + sNewValue);
                    //api.Notifications.AddMessage("fieldName:" + fieldName + "    preCommitValue:" + preCommitValue);
                    bug.SetPluginField(PLUGIN_ID, fieldName, sNewValue);

                    valueChanged = true;

                    if (string.IsNullOrEmpty(preCommitValue))
                    {

                        bugevent.AppendChangeLine(
                        string.Format("{0} set to'{1}'",
                                      fieldDisplay,
                                      sNewValue));

                    }
                    else
                    {
                        bugevent.AppendChangeLine(
                            string.Format("{0} changed from '{1}' to '{2}'",
                                          fieldDisplay,
                                          preCommitValue,
                                          sNewValue));
                    }
                }
            }

            return valueChanged;
        }
 
        public void BugCommitRollback(CBug bug, BugAction nBugAction, bool fPublic)
        {
        }
        #endregion

        protected bool PluginFieldVaries(CBug[] rgBug, string sKey)
        {
            if (rgBug == null || rgBug.Length == 0) return false;

            object start = rgBug[0].GetPluginField(PLUGIN_ID, sKey);

            /* If we find a different value from the first, return true. */

            if (start != null)
            {
                for (int i = 1; i < rgBug.Length; i++)
                    if (rgBug[i].GetPluginField(PLUGIN_ID, sKey) == null ||
                        !rgBug[i].GetPluginField(PLUGIN_ID, sKey).Equals(start))
                        return true;
            }
            else
            {
                for (int i = 1; i < rgBug.Length; i++)
                    if (rgBug[i].GetPluginField(PLUGIN_ID, sKey) != null)
                        return true;
            }

            return false;
        }

        public string GetText(CBug[] rgbug, string fieldName)
        {


            if (rgbug == null || rgbug.Length == 0)
            {
                return "";
            }
            if (PluginFieldVaries(rgbug, fieldName))
            {

                return VARIOUS_TEXT;
            }
            else
                //api.Notifications.AddMessage(fieldName + "-5A3 "+(Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName))));
               // api.Notifications.AddMessage("PliginId  " + PLUGIN_ID + "  fieldName" + fieldName);
                return Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
        }


        #region IPluginDatabase Members

        public CTable[] DatabaseSchema()
        {


            CTable Invoiceheader = api.Database.NewTable(api.Database.PluginTableName("SLRHeader"));
            Invoiceheader.sDesc = "Caputures Invoice Header Parameters with SLR";
            Invoiceheader.AddAutoIncrementPrimaryKey("ixInvoiceNumber");
            Invoiceheader.AddIntColumn("ixBug", true, 1);
            Invoiceheader.AddVarcharColumn("SLRCustomForm", 200, false);
            Invoiceheader.AddVarcharColumn("SLRVendor", 200, false);
            Invoiceheader.AddVarcharColumn("SInvoiceNumber", 200, false);
            Invoiceheader.AddDateColumn("SInvoiceDate", false);
            Invoiceheader.AddFloatColumn("SInvoiceAmount", false);
            Invoiceheader.AddVarcharColumn("SPONumber", 200, false);
            Invoiceheader.AddVarcharColumn("SMemo", 250, false);
            Invoiceheader.AddVarcharColumn("SLRType", 250, false);
            Invoiceheader.AddVarcharColumn("SCustomVal1", 200, false);
            Invoiceheader.AddVarcharColumn("SCustomVal2", 200, false);
            Invoiceheader.AddVarcharColumn("SCustomVal3", 200, false);

            CTable SlrCustomeform = api.Database.NewTable(api.Database.PluginTableName("SLRCustomForm"));
            SlrCustomeform.sDesc = "Caputures Invoice Header Parameters with SLR";
            SlrCustomeform.AddAutoIncrementPrimaryKey("ixSLRCustomForm");
            SlrCustomeform.AddIntColumn("ixProject", true, 1);
            SlrCustomeform.AddVarcharColumn("sSLRCustomFormValue", 200, false);

            CTable SlrVendor = api.Database.NewTable(api.Database.PluginTableName("SLRVendor"));
            SlrVendor.sDesc = "Caputures Invoice Header Parameters with SLR";
            SlrVendor.AddAutoIncrementPrimaryKey("ixSLRVendor");
            SlrVendor.AddIntColumn("ixProject", true, 1);
            SlrVendor.AddVarcharColumn("sSLRVendorValue", 200, false);

            CTable SlrType = api.Database.NewTable(api.Database.PluginTableName("SLRType"));
            SlrType.sDesc = "Caputures Invoice Header Parameters with SLR";
            SlrType.AddAutoIncrementPrimaryKey("ixSLRType");
            SlrType.AddIntColumn("ixProject", true, 1);
            SlrType.AddVarcharColumn("sSLRTypeValue", 200, false);

            return new CTable[] { Invoiceheader, SlrCustomeform, SlrVendor, SlrType };
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


        #region IPluginGridColumn Members

        public CGridColumn[] GridColumns()
        {
            CGridColumn gridCol1 = api.Grid.CreateGridColumn();
            /* the name displayed in the filter drop-down */
            gridCol1.sName = "SInvoiceNumber";
            /* the column title in grid view */
            gridCol1.sTitle = "SInvoice Number";
            /* every column you create needs to have a unique iType */
            gridCol1.iType = 0;

            CGridColumn gridCol2 = api.Grid.CreateGridColumn();
            gridCol2.sName = "SVendor Name";
            gridCol2.sTitle = "SVendor Name";
            /* every column you create needs to have a unique iType */
            gridCol2.iType = 1;

           
            CGridColumn gridCol3 = api.Grid.CreateGridColumn();
            gridCol3.sName = "SType";
            gridCol3.sTitle = "SType";
            /* every column you create needs to have a unique iType */
            gridCol3.iType = 2;

            CGridColumn gridCol4 = api.Grid.CreateGridColumn();
            gridCol4.sName = "SMemo";
            gridCol4.sTitle = "SMemo";
            /* every column you create needs to have a unique iType */
            gridCol4.iType = 3;

            CGridColumn gridCol5 = api.Grid.CreateGridColumn();
            gridCol5.sName = "SAmount";
            gridCol5.sTitle = "SAmount";
            /* every column you create needs to have a unique iType */
            gridCol5.iType = 4;

            CGridColumn gridCol6 = api.Grid.CreateGridColumn();
            gridCol6.sName = "SPO Number";
            gridCol6.sTitle = "SPO Number";
            /* every column you create needs to have a unique iType */
            gridCol6.iType = 5;

            CGridColumn gridCol7 = api.Grid.CreateGridColumn();
            gridCol7.sName = "Project/BPM";
            gridCol7.sTitle = "Project/BPM";
            /* every column you create needs to have a unique iType */
            gridCol7.iType = 6;

            return new CGridColumn[] { gridCol1, gridCol2, gridCol3, gridCol4, gridCol5, gridCol6,gridCol7 }; 
        }

        public CBugQuery GridColumnQuery(CGridColumn col)
        {
            /* Return a CBugQuery with the data you need joined
             * in. If your table is already joined to bug in
             * IPluginBugJoin, FogBugz does the work for you. */
            return api.Bug.NewBugQuery();
        }

        public string[] GridColumnDisplay(CGridColumn col,
                                          CBug[] rgBug,
                                          bool fPlainText)
        {
            string sTableColumn = "SInvoiceNumber";
            switch (col.iType)
            {
                case 0:
                    sTableColumn = "SInvoiceNumber";
                    break;
                case 1:
                    sTableColumn = "SLRVendor";
                    break;
                case 2:
                    sTableColumn = "SLRType";
                    break;
                case 3:
                    sTableColumn = "SMemo";
                    break;
                case 4:
                    sTableColumn = "SInvoiceAmount";
                    break;
                case 5:
                    sTableColumn = "SPONumber";
                    break;
                case 6:
                    sTableColumn = "SLRCustomForm";
                    break;
               
                    
            }
            string[] sValues = new string[rgBug.Length];

            for (int i = 0; i < rgBug.Length; i++)
            {
                /* For tables joined in IPluginBugJoin, use
                 * GetPluginField to fetch the values you need
                 * for the GridColumn. */
                object pluginField = rgBug[i].GetPluginField(PLUGIN_ID, string.Format("{0}", sTableColumn));
                sValues[i] = (pluginField == null) ?
                             "" :
                             HttpUtility.HtmlEncode(pluginField.ToString());
            }
            return sValues;
        }

        public CBugQuery GridColumnSortQuery(CGridColumn col, bool fDescending,
                                                bool fIncludeSelect)
        {
            string sTableColumn = "SInvoiceNumber";
            switch (col.iType)
            {
                case 0:
                    sTableColumn = "SInvoiceNumber";
                    break;
                case 1:
                    sTableColumn = "SLRVendor";
                    break;
                case 2:
                    sTableColumn = "SLRType";
                    break;
                case 3:
                    sTableColumn = "SMemo";
                    break;
                case 4:
                    sTableColumn = "SInvoiceAmount";
                    break;
                case 5:
                    sTableColumn = "SPONumber";
                    break;
                case 6:
                    sTableColumn = "SLRCustomForm";
                    break;
            
            }
            /* Return a CBugQuery with the data you need joined
             * in and sorted appropriately. Include an explicit
             * select if fIncludeSelect is true. If your table is
             * already joined to bug in IPluginBugJoin, FogBugz
             * does the work for you, ignore fIncludeSelect. */
            CBugQuery bugQuery = api.Bug.NewBugQuery();
            bugQuery.AddOrderBy(string.Format("{0}.{1} {2}",
                                            sPrefixedTableName,
                                            sTableColumn,
                                            (fDescending ? "DESC" : "ASC")
                                            )
                             );
            return bugQuery;
        }

        #endregion

        /* Methods to created, read, update and delete items in the database, using
         * fields in the plugin api request object */


        /* use the PluginRawPage to handle AJAX requests when the user adds, edits or
         * deletes Items. */
      


        #region beautifier
        protected CEditableTable Headings_1(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            CEditableTable Heading_1 = new CEditableTable("Headings");
            string sTableId = Heading_1.sId;
            Heading_1.Header.AddCell("Invoice Approval Status Information");
            return (Heading_1);
        }

        #endregion
        
        

        #region Utility Methods

       
       

        protected string CommandUrl1(string sCommand, int ixBug)
        {
            return string.Concat(api.Url.PluginPageUrl(),
                                 LinkParameter("sCommand", sCommand),
                                 LinkParameter("ixBug", ixBug.ToString()));
        }

        protected string LinkParameter(string sName, string sValue)
        {
            return string.Format("&{0}={1}", api.AddPluginPrefix(sName), sValue);
        }

        #endregion


        //not using this procedure
        private string QueryDbForValue(string sTableName, string sValue)
        {
            string sName = "";
            if (!string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sValue))
            {
                CSelectQuery sq = api.Database.NewSelectQuery(
                    api.Database.PluginTableName("Solarbridge@conseroglobal.com", sTableName));
                sq.AddSelect("s" + sTableName + "Name");
                sq.AddWhere(api.Database.PluginTableName("Solarbridge@conseroglobal.com", sTableName) + ".ix" + sTableName + " = " + sValue);
                DataSet ds = sq.GetDataSet();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                {
                    sName = ds.Tables[0].Rows[0]["s" + sTableName + "Name"].ToString();
                }
                ds.Dispose();
            }
            return sName;
        }

        #region Dropdown Selects

        protected string GetSelects(string sType, int ixProject)
        {
            return GetSelects(null, sType, ixProject, false);
        }

        protected string GetSelects(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("Solarbridge@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("Solarbridge@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("Solarbridge@conseroglobal.com", sType),
                                            "s" + sType + "Value",
                                            "ASC"));

            DataSet ds = sq.GetDataSet();
            int nGlCount;

            if (ds.Tables[0] != null && (nGlCount = ds.Tables[0].Rows.Count) > 0)
            {
                names = new string[nGlCount];
                ixs = new string[nGlCount];



                for (int i = 0; i < nGlCount; i++)
                {
                    

                    if (bDisplayId)
                    {

                        //names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        // + ":" + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Value"].ToString());

                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Value"].ToString());

                    }
                    else
                    {

                        //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        // names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        // + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Value"].ToString());
                    }
                    // ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                }

                ds.Dispose();

                //  return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                //                              Forms.SelectOptions(names,
                //                                            sSelected,
                //                                          ixs));

                //api.AddPluginPrefix(sType)

                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names,
                                                     sSelected,
                                                     names)));


            }
            ds.Dispose();
            return String.Empty;
        }
            
       
        #endregion

        #region Program for sending email

        // Method for sendig emails
        public void mailsender(string sMailAdderss, CBug bug, String mailsub, string mailbody, int Iperson)
        {

            if (sMailAdderss != null)
            {

                string body = "Dear User,";
                body += System.Environment.NewLine;
                body += System.Environment.NewLine;


                body += mailbody;//"The case" + bug.ixBug + " is pending for action";
                body += System.Environment.NewLine;
                body += System.Environment.NewLine;
                body += "Regards,";
                body += System.Environment.NewLine;

                body += "AP Team.";
                body += System.Environment.NewLine;

                api.Mail.SendTextEmail(sMailAdderss, mailsub, body);

                //  api.Mail.SendTextEmail(sMailAdderss, "Fogbugz (case " + bug.ixBug + " ) " + sProject + " " + bug.sTitle, body);
            }
        }

        # endregion

        public void RenameFile(CBug bug, CBugEvent bugevent)
        {
            //date_vendor_invoicenumber_casenumber.pdf
            string sFileName = "";
            string sInvoiceNumber = "";//Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber"));
            // string sVendorId = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlVendor"));
            string sVendorName = "";
            DateTime sINVDate;
            string sAmount = "";
            string sINVDate2 = "";
            string sFolderdate = "";
            //  string sMonth = "";
            // string sDate = "";

            /*  if (!string.IsNullOrEmpty(sVendorId))
              {
                  sVendorName = QueryDbForValue("GlVendor", sVendorId);
              }
             */
            DateTime bugdate = bug.dtOpened;
            sFolderdate = bugdate.ToString("MM.dd.yy");


            //querying Custom bugfields for invoice and vendor name to attch with mail subject start

            {

                //   String tname = "Plugin_40_CustomBugData";


                CSelectQuery File_det = api.Database.NewSelectQuery(api.Database.PluginTableName("Solarbridge@conseroglobal.com", "SLRHeader"));
                File_det.AddSelect("SLRVendor,SInvoiceNumber,SInvoiceDate,SInvoiceAmount");
                File_det.AddWhere("ixBug = " + bug.ixBug.ToString());


                DataSet Dcust = File_det.GetDataSet();

                if (Dcust.Tables.Count > 0 && Dcust.Tables[0] != null && Dcust.Tables[0].Rows.Count > 0)
                {
                    sInvoiceNumber = Convert.ToString(Dcust.Tables[0].Rows[0]["SInvoiceNumber"]);
                    sVendorName = Convert.ToString(Dcust.Tables[0].Rows[0]["SLRVendor"]);
                    try
                    {
                        sINVDate = Convert.ToDateTime(Dcust.Tables[0].Rows[0]["SInvoiceDate"]);
                    }

                    catch
                    {
                        return;
                    }
                    sAmount = Convert.ToString(Dcust.Tables[0].Rows[0]["SInvoiceAmount"]);
                    //a sInvoice will be havinf format HH:MM:SS we use below code for formating
                    DateTime dt = sINVDate;
                    sINVDate2 = dt.ToString("MM.dd.yy");

                }

            }

            string sDate = "";
            string sCaseNumber = bug.ixBug.ToString();

            if (!string.IsNullOrEmpty(sInvoiceNumber) &&
                !string.IsNullOrEmpty(sVendorName))
            {
                DateTime now = DateTime.Now;
                sDate = now.Year.ToString() + "_" + now.Month.ToString() + "_" + now.Day.ToString();
                //sFileName = sInvoiceNumber + "" + sVendorName + "_" + sDate + "_" + sCaseNumber;
                sFileName = sVendorName + "," + sINVDate2 + "," + sInvoiceNumber + "," + sAmount;
            }

            string attach1 = sFileName;
            // api.Notifications.AddMessage("attach1" + attach1);
            if (!string.IsNullOrEmpty(sFileName))
            {

                string fileBackupPath = "";
                CProject project = api.Project.GetProject(bug.ixProject);
                //string backUpLocation = "D:";//Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sBackupLocation"));
                //string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Default Sync Folder"
              //  string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Solarbridge\\" + sINVDate2;
                //string backUpLocation = "C:\\Users\\Administrator\\Documents\\My Box Files\\Default Sync Folder\\" + sINVDate2;
                string backUpLocation = "C:\\Users\\Administrator\\Documents\\My Box Files\\SolarBridge\\" + sVendorName + "\\" + sINVDate2;
                CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                attachmentQuery.AddWhere("Bug.ixBug = " + bug.ixBug.ToString());
                attachmentQuery.IgnorePermissions = true;
                attachmentQuery.ExcludeDeleted = true;
                DataSet ds = attachmentQuery.GetDataSet();
                List<CAttachment> attachments = new List<CAttachment>();
                //if (null != ds.Tables[0] && ds.Tables[0].Rows.Count == 1)

                int ixAttachment = 0;
                string sFilename2 = "";
                // string[] attachName = new string[10];
                string sExtn = "";
                int[] attachid = new int[10];
                int icount = 0;
                if (null != ds.Tables[0] && ds.Tables[0].Rows.Count > 0)
                {
                    string[] attachName1 = new string[ds.Tables[0].Rows.Count];
                    int[] attachid1 = new int[ds.Tables[0].Rows.Count];
                    int[] attachid2 = new int[ds.Tables[0].Rows.Count];
                    int[] attachid3 = new int[ds.Tables[0].Rows.Count];
                    //loop to check multiple attachments  

                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                    {

                        sFilename2 = sFileName;
                        //int ixAttachment = Convert.ToInt32(ds.Tables[0].Rows[0]["ixAttachment"]);
                        ixAttachment = Convert.ToInt32(ds.Tables[0].Rows[j]["ixAttachment"]);

                        CAttachment attachment = api.Attachment.GetAttachment(ixAttachment);
                        string[] fileNameDetails = attachment.sFileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        //checking for duplicate in attachment

                        // api.Notifications.AddMessage("arryvalues", attachName[k].ToString() + "||" + sFilename2 + " ||" + attachment.sFileName);
                        bool Exsit = Array.Exists(attachName1, element => element == attachment.sFileName); //Array.Find(attachName[],"ads");
                        //api.Notifications.AddMessage("cnt" + "x");
                        //  api.Notifications.AddMessage("j and filname", j.ToString()+"||" +  attachName[j].ToString() +" ||"+ attachment.sFileName);
                        if (Exsit == false)
                        {


                            //api.Notifications.AddMessage("j and filname", ixAttachment.ToString());
                            attachName1[j] = attachment.sFileName;
                            attachid1[j] = ixAttachment;

                        }
                        if (Exsit == true)
                        {
                            bug.DeleteAttachment(ixAttachment);
                        }

                    }//  attachName[j] = attachment.sFileName;
                    //api.Notifications.AddMessage("cnt" + 2);
                    //int cntindex = 0;
                    foreach (int i in attachid1)
                    {
                        //api.Notifications.AddMessage("cnt" + i);
                        //api.Notifications.AddMessage("cnt" + 3);
                        // api.Notifications.AddMessage("cnt" + attachid1[i].ToString());
                        int p = Convert.ToInt32(i);
                        CAttachment attachment = api.Attachment.GetAttachment(p);
                        string[] fileNameDetails = attachment.sFileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        //api.Notifications.AddMessage("j and filname",+Convert.ToInt32(p));
                        //foreach (string CurrentItem in fileNameDetails)
                        //{
                        //api.Notifications.AddMessage("cnt" + 4);
                        //}
                        //  api.Notifications.AddMessage("att" + att);
                        // api.Notifications.AddAdminNotification("att", fileNameDetails[fileNameDetails.Length - 1]);
                        if (fileNameDetails.Length > 1)
                        {
                            string fileExtension = fileNameDetails[fileNameDetails.Length - 1];
                            // api.Notifications.AddMessage("cnt" + 5);


                            //      sFileName += ".";
                            //    sFileName += fileExtension;

                            sExtn = fileExtension.ToLower();

                        }
                        // for checking extension

                        if (sExtn == "doc" || sExtn == "pdf" || sExtn == "bmp" || sExtn == "jpg" || sExtn == "jpeg" || sExtn == "xls" || sExtn == "xlsx" || sExtn == "docx" || sExtn == "gif" || sExtn == "tif")// || sExtn == "png")
                        {
                            // api.Notifications.AddMessage("cnt" + 6);
                            //  int sAttachmentold = ixAttachment;
                            int sAttachmentold = p;
                            // api.Notifications.AddMessage("sAttachmentold", sAttachmentold.ToString());
                            if (icount > 0)
                            {

                                sFilename2 += "_" + icount;

                                // icount = icount + 1;
                            }

                            icount = icount + 1;
                            sFilename2 += ".";
                            sFilename2 += sExtn;

                            // api.Notifications.AddMessage("attachment", attachment.sFileName);
                            // api.Notifications.AddMessage("sFilename2", sFilename2);

                            if (attachment.sFileName != sFilename2)
                            {
                                //api.Notifications.AddMessage("cnt" + 7);
                                CAttachment clonedAttachment = CloneAttachment(attachment, sFilename2);
                                attachments.Add(clonedAttachment);
                                //api.Notifications.AddMessage("cnt" + 8);
                                bugevent.CommitAttachmentAssociation(attachments.ToArray());

                                if (!string.IsNullOrEmpty(backUpLocation))
                                {

                                    // api.Notifications.AddMessage("cnt" + 10);
                                    // fileBackupPath = backUpLocation + "\\" + project.sProject + "\\" + sVendorName + "\\" + sDate + "\\" + sFileName;
                                    fileBackupPath = backUpLocation + "\\" + sFilename2;
                                    CreateDirectory(new DirectoryInfo(Path.GetDirectoryName(fileBackupPath)));
                                    // api.Notifications.AddMessage("File has been backed up as " + Path.GetFullPath(fileBackupPath));
                                    // api.Notifications.AddMessage("Invoice has been backed up succsessfuly");

                                    FileStream fileStream = new FileStream(Path.GetFullPath(fileBackupPath), FileMode.Create, FileAccess.Write);
                                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                                    binaryWriter.Write(attachment.rgbData);
                                    binaryWriter.Close();
                                    fileStream.Close();
                                    fileStream.Dispose();

                                    bug.DeleteAttachment(sAttachmentold);
                                }

                                // bug.DeleteAttachment(ixAttachment);
                                //  bug.DeleteAttachment(sAttachmentold);

                                // attachid2[cntindex] = sAttachmentold;
                                // cntindex++;
                            }
                        }

                    }

                    ds.Dispose();
                }

            }
        }

        [ReflectionPermission(SecurityAction.Assert)]
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

        ///// adding vendor
        #region IPluginRawPageDisplay Members

        public string RawPageDisplay()
        {
           // api.Notifications.AddAdminNotification("1", "1");
            try
            {

                int ixBug = 0;
                //  int iCopies = 0;
                string sError = "";

                /* If the request did not include a valid action token, do not
                    * edit any cases and redirect with an error message to display */
                if ((api.Request[api.AddPluginPrefix("actionToken")] == null) ||
                    !api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")].ToString()))
                {
                    api.Notifications.AddMessage("action token failed");

                    sError = string.Format("{0}={1}",
                                api.AddPluginPrefix("sError"),
                                HttpUtility.UrlEncode("action token was invalid or missing.")
                                );
                }

                else
                {
                  //  api.Notifications.AddAdminNotification("2", "2");
                    ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());
                    CBug bug = api.Bug.GetBug(ixBug);
                    bug.IgnorePermissions = true;
                    int iproj = bug.ixProject;
                    // adding new Vendor 
                    try
                    {
                        //  string vendorid = (api.Request[api.AddPluginPrefix("Vendorid")].ToString());
                        string vendorname = (api.Request[api.AddPluginPrefix("Vendorname_1")].ToString());

                      //  api.Notifications.AddAdminNotification("3", "3");
                        if ((api.Request[api.AddPluginPrefix("Vendorname_1")].ToString().Trim()) != null)
                        {
                            if ((api.Request[api.AddPluginPrefix("Vendorname_1")].ToString()).Trim() != "")
                            {
                                //api.Notifications.AddAdminNotification("project 4", sProj);

                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("SLRVendor"));
                             //   api.Notifications.AddAdminNotification("4", "4");

                                insert1.InsertInt("ixProject", iproj);
                                insert1.InsertString("sSLRVendorValue", vendorname);
                                insert1.Execute();
                            }


                        }




                    }


                    catch
                    {
                        //dont do anything
                    }



                }
            }
            catch
            {
                //return null;
            }
            return null;
        }

        public PermissionLevel RawPageVisibility()
        {
            return PermissionLevel.Normal;
        }

        #endregion

        private string sTableId;

        protected CEditableTable EditableTable_Vend(int ixBug)
        {
          //  api.Notifications.AddMessage("6");
            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Vendor_1");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Add New"));
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
                                                    "  Vendor  "));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("new", dlgTemplateNew);

            api.Notifications.AddMessage("7");
            return editableTable;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new vendor to ";
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
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Vendorid"), ""),
                                "Vendor ID ");

            //dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Vendorname_1"), ""),
                                "Vendor Name");
            dlgTemplateNew.Template.Items.Add(itemEditId2);

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

     
    }
}
