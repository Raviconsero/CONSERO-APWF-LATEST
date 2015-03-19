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

namespace Consero.Plugins.CGSInvoiceDetails_MLA
{
    public class Act : Plugin, IPluginBugJoin,
        IPluginBugDisplay, IPluginBugCommit, IPluginDatabase, IPluginRawPageDisplay, IPluginGridColumn, IPluginJS
    {

        protected const string PLUGIN_ID =
           "CGSInvoiceDetails_MLA@conseroglobal.com";

        /* A constant for populating the "code name" input field for multiple case edit */
        protected const string VARIOUS_TEXT = "[various]";
        private string sPrefixedTableName;

        string sAccount_P = "";
        string sTaxtype_P = "";
        string sDepartment_P = "";
        string sBillable_P = "";
        int flag = 0;
        string MailSub = "", MailBody = "";
        int ixperson = 0;

        int Rename = 0;

        

        /* Constructor: We'll just initialize the inherited Plugin class, which 
         * takes the passed instance of CPluginApi and sets its "api" member variable. */
        public Act(CPluginApi api)
            : base(api)
        {
            sPrefixedTableName = api.Database.PluginTableName("CGSInvoice_MLA");
        }

        #region IPluginBugJoin Members

        public string[] BugJoinTables()
        {
            /* All tables specified here must have an integer ixBug column so FogBugz can
            * perform the necessary join. */

            return new string[] { "CGSInvoice_MLA" };
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

            //if (bug.ixCategory != 3)
            // {
            //     return null;
            //   }

            //if (bug.ixProject != 9)
          //  { return null; }

            CProject project = api.Project.GetProject(bug.ixProject);
            string enableCGSWorkflowSettings = Convert.ToString(project.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            api.Notifications.AddMessage("settting message " + enableCGSWorkflowSettings);
            if (string.IsNullOrEmpty(enableCGSWorkflowSettings) )
            {
                return null;
            }
                else if("0".Equals(enableCGSWorkflowSettings))
                {

                    return null;
                }
                        else if ("1".Equals(enableCGSWorkflowSettings))
                        {

                //don't do any intacct calls
                return null;
            }

            

            if (nMode == BugEditMode.Edit)
            {
               
                if (bug.ixProject == 14)
                {
                    if (rgbug.Length != 1)
                    {
                        return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                   };
                    }
                    else
                    {
                        return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                          CreateListField(rgbug, "CustomForm", "CustomForm", "CWFCustomform", "CWFCustomform", true),
                          CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateListField_Posting(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", "CWFPostingperiod", true),
                          CreateListField(rgbug, "Country", "Country", "CWFCountry", "CWFCountry", true),
                          CreateListField(rgbug, "Currency", "Currency", "CWFCurrency", "CWFCurrency", true),
                          CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                         
                          CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateListField(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", "CWFSubsidiary", true),
                          CreateDateInputField(rgbug, "InvoiceEnteredDate", "Date Invoice Entered", "sInvoiceEnteredDate"),

                          CreateTextInputField(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          CreateListField(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                          CreateDateInputField(rgbug, "InvoiceDueDate", "Due Date", "sInvoiceDueDate"),

                          CreateTextInputField(rgbug, "NetAmount", "Net Amount", "Netamount"),
                          CreateTextInputField(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),
                          CreateTextInputField(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),

                          CreateTextInputField(rgbug, "AccountDesc", "Account Desc", "AccountDesc"),
                           CreateListField1(rgbug, "Key Areas", "KeyAreas", "CWFLocation", "CWFLocation",true),
                           CreateListField1(rgbug, "Accrual", "Accrual", "CWFDept", "CWFDept",true),
                        //  new CBugDisplayDialogItem("Invhead_7", null, null, 2),
                          //CreateListField(rgbug, "Type", "Type", "Type", "Type", true),
                          CreateTextInputField_memo(rgbug, "Memo", "Memo", "sMemo"),
                         
                
                       new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   };
                    }
                  
                }
                else if (bug.ixProject == 19)
                {
                    if (rgbug.Length != 1)
                    {
                        
                       // return new CBugDisplayDialogItem[] 
                   { 
                       //new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                       //   CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                       //   CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                       //   CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                       //   CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                   };
                    }

                    else
                    {

                        return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                          new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                         // CreateTextInputField(rgbug, "ExchangeRate", "Header Desc", "sExchangeRate"),
                          CreateListField_Cambridge(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateListField(rgbug, "Currency", "Currency", "CWFCurrency", "CWFCurrency", true),
                          CreateTextInputField(rgbug, "InvoiceNumber", "Doc Number", "sInvoiceNumber"),
                          CreateDateInputField(rgbug, "InvoiceDate", "Doc Date", "sInvoiceDate"),
                          CreateTextInputField(rgbug, "TotalAmount", " Purchases", "TotalAmount"),
                          CreateTextInputField(rgbug, "Memo", "Memo", "sMemo"),
                  
                       new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   };
                    }

                }
                else if (bug.ixProject == 22)
                {
                    if (rgbug.Length != 1)
                    {
                        return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "Notice Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                                     
                      
                   };
                    }
                    else
                    {
                        return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "Notice Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead_1", null, "Notice Header Information", 3),
                          CreateListField(rgbug, "Payroll/Non payroll", "Payroll/Non payroll", "CWFCustomform", "CWFCustomform", true),

                          CreateListField1(rgbug, "Entity", "Entity", "CWFVendor", "CWFVendor", true),
                          CreateListField(rgbug, "IRS/State", "IRS/State", "CWFTerms", "CWFTerms", true),
                          CreateListField(rgbug, "State", "State", "CWFCountry", "CWFCountry", true),
                          CreateListField(rgbug, "Triage", "Triage", "CWFCurrency", "CWFCurrency", true),
                                                 
                          CreateTextInputField(rgbug, "Date of notice", "Date of notice", "sAddInfo"),
                          CreateListField(rgbug, "Activity Type", "Activity Type", "CWFSubsidiary", "CWFSubsidiary", true),
                          CreateTextInputField(rgbug, "Period", "Period", "sExchangeRate"),
                          
                          CreateDateInputField(rgbug, "Department timeline", "Department timeline", "sInvoiceDueDate"),
                          CreateTextInputField(rgbug, "Amount", "Total Amount", "TotalAmount"),
                          CreateDateInputField(rgbug, "InternalTimeLine", "Internal TimeLine", "sInvoiceEnteredDate"),
                          CreateListField(rgbug, "ClientAction", "Client Action", "CWFPostingperiod", "CWFPostingperiod", true),
                          CreateTextInputField_memo(rgbug, "Concern", "Concern", "AccountDesc"),
                        
                        // new CBugDisplayDialogItem("Invhead_7", null, null, 1),
                        
                         CreateTextInputField_memo(rgbug, "To-do", "To-do", "sMemo"),
                         
                   };
                    }


                }


               else if (bug.ixProject == 25)  //edit mode fileds for symerg
                {
                   string POnum = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "Add_Fld1"));
                   bug.SetPluginField(PLUGIN_ID, "ixproject", bug.ixProject.ToString());
                 //  api.Notifications.AddAdminNotification("Ponume1", POnum.ToString());

                   if ((bug.ixStatus == 180) || (bug.ixStatus == 184))
                   {
                       if (POnum != "")
                       {
                           if (POnum != null)
                           {
                               return new CBugDisplayDialogItem[] 
                   { 
                         
                       //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         //CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         //CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         //CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                      CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "Old PO Header Information", 3),
                                               
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine1", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                           CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                           CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"), 
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),                   
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateText(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "Add_Fld1",false,null),
                        CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                        CreateTextInputField(rgbug, "PO Amount", "PO Amount", "POAmt"),
                          CreateTextInputField(rgbug, "PO Balance Amount", "PO Balance Amount", "POBalanceAmt"),
                     new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3) ,     
                 //    new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                 new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  
                        
                   };
                           }
                       }
                       else
                       {
                           return new CBugDisplayDialogItem[] 
                   { 
                       //CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                     
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "PO Header Information", 3),
                        //  CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                         CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine1", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                           CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                           CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"), 
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),                   
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),
                          CreateTextInputField(rgbug, "PO Amount", "PO Amount", "POAmt"),
                          CreateText(rgbug, "PO Balance Amount", "PO Balance Amount", "POBalanceAmt"),

                     new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3) ,     
                     new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  
                        
                   };
                       }
                   }

                   else if (POnum != "")
                   {
                       if (POnum != null)
                       {
                           return new CBugDisplayDialogItem[] 
                   { 
                         
                       //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead_1", null, "Old PO Header Information", 3),
                                               
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine1", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                           CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                           CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"), 
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),                   
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateDateInputField(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "Add_Fld1",false,null),
                       // CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                        CreateDateInputField(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                     new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)      
                 //    new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                        
                   };
                       }
                   }



                   else
                   {
                       if (rgbug.Length != 1)
                       {
                           return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                   };
                       }
                       else
                       {
                           return new CBugDisplayDialogItem[] 
                   { 
                         
                      //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                      

                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead_1", null, "PO Header Information", 3),

                     //  CreateListField(rgbug, "Entity ID", "EntityID", "CWFCountry", "CWFCountry", true),
                       CreateListField1_SE(rgbug, "Location ID", "LocationID", "CWFLocation", "CWFLocation", true),
                       CreateListField1_SE(rgbug, "Department", "Department", "CWFDept", "CWFDept", true),
                        
                          //CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateTextInputField(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateTextInputField(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateTextInputField(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateTextInputField(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateDateInputField(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateListField_NotSort(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                          CreateListField_NotSort(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", "CWFPostingperiod", true),
                        
                        CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                         CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                         //CreateText(rgbug, "Expires", "Expires", "DateString2"),
                         CreateDateInputField(rgbug, "Expires", "Expires", "DateString2"),
                       //  new CBugDisplayDialogItem("Invheadnew1", null, null, 1), 
                         CreateTextInputField_memo(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                         new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   
                   };
                       }
                   }

                }

                else if (bug.ixProject == 26)
                {
                    string POnum = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "PONumberArt_A"));
                //    bug.SetPluginField(PLUGIN_ID, "ixproject", bug.ixProject.ToString());
                    //  api.Notifications.AddAdminNotification("Ponume1", POnum.ToString());

                    if ((bug.ixStatus == 180) || (bug.ixStatus == 184))
                    {
                        if (POnum != "")
                        {
                            if (POnum != null)
                            {
                                return new CBugDisplayDialogItem[] 
                   { 
                         
                       //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         //CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         //CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         //CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                      CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "Old PO Header Information", 3),
                                               
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine1", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                           CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                           CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"), 
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),                   
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateText(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "PONumberArt_A",false,null),
                        CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                        CreateTextInputField(rgbug, "PO Amount", "PO Amount", "POAmt"),
                          CreateTextInputField(rgbug, "PO Balance Amount", "PO Balance Amount", "POBalanceAmt"),
                     new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)  ,
                     new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  ,
                 //    new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                        
                   };
                            }
                        }
                        else
                        {
                            return new CBugDisplayDialogItem[] 
                   { 
                       //CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                     
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "PO Header Information", 3),
                        //  CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                         CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine1", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                           CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                           CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"), 
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),                   
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),
                          CreateTextInputField(rgbug, "PO Amount", "PO Amount", "POAmt"),
                          CreateText(rgbug, "PO Balance Amount", "PO Balance Amount", "POBalanceAmt"),

                     new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3),
                     new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  ,
                        
                   };
                        }
                    }

                    else if (POnum != "")
                    {
                        if (POnum != null)
                        {
                            return new CBugDisplayDialogItem[] 
                   { 
                         
                       //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                       
                     //  new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                    //   new CBugDisplayDialogItem("Invhead_2", null, "Old PO Header Information", 3),
                                               
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine1", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                           CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                           CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"), 
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),                   
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateDateInputField(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "PONumberArt_A",false,null),
                        //CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                         CreateDateInputField(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                    new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)      
                 //    new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                        
                   };
                        }
                    }



                    else
                    {
                        if (rgbug.Length != 1)
                        {
                            return new CBugDisplayDialogItem[] 
                   { 
                     //  new CBugDisplayDialogItem("ApprInfo_3", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                   };
                        }
                        else
                        {
                            return new CBugDisplayDialogItem[] 
                   { 
                         
                      //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                      

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                     //  CreateListField(rgbug, "Entity ID", "EntityID", "CWFCountry", "CWFCountry", true),
                       CreateListField1_SE(rgbug, "Location ID", "LocationID", "CWFLocation", "CWFLocation", true),
                       CreateListField1_SE(rgbug, "Department", "Department", "CWFDept", "CWFDept", true),
                        
                          //CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateTextInputField(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateTextInputField(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateTextInputField(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateTextInputField(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateDateInputField(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateListField_NotSort(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                          CreateListField_NotSort(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", "CWFPostingperiod", true),
                        
                        CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                         CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                        // CreateText(rgbug, "Expires", "Expires", "DateString1"),

                        CreateDateInputField(rgbug, "Expires", "Expires", "DateString2"),
                       //  new CBugDisplayDialogItem("Invheadnew1", null, null, 1), 
                         CreateTextInputField_memo(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                         new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   
                   };
                        }
                   }

                }


                else if (bug.ixProject == 23)
                {
                    // api.Notifications.AddAdminNotification("Cambridge", "Cambridge");
                    // api.Notifications.AddMessage("Cambridge", "Cambridge");
                    if (rgbug.Length == 1)
                    {

                        // api.Notifications.AddAdminNotification("Cambridge1", "Cambridge1");


                        return new CBugDisplayDialogItem[] 
                   { 

                        
                       //new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                       //   CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                       //   CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                       //   CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                       //   CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                        
                          new CBugDisplayDialogItem("Invhead_1", null, "T&E Header Information", 3),
                          CreateDateInputField(rgbug, "StatementDate", "Statement Date", "sInvoiceDate"),
                          CreateTextInputField(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),
                          CreateTextInputField(rgbug, "Memo", "Memo", "sMemo"),
                  
                       new CBugDisplayDialogItem("item", ItemTable_TE(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   };
                    }

                }

                else if (bug.ixProject == 27)
                {
                    string type = (bug.GetPluginField("customfields@fogcreek.com", "typea718")).ToString();
                    string BPO = "";

                    CSelectQuery BlanketPO_NUM = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    BlanketPO_NUM.AddSelect("B_PO_ref");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    BlanketPO_NUM.AddWhere(sWhere2);
                    object NewPo = BlanketPO_NUM.GetScalarValue();
                    BPO = Convert.ToString(NewPo);

                    string B_AddenPOnum = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "B_PO_Adden"));

                    if ((bug.ixStatus == 192) || (bug.ixStatus == 193))
                    {


                        if (type == "General PO")
                        {
                            //api.Notifications.AddAdminNotification("BPO1", BPO.ToString());


                            if (B_AddenPOnum != "")
                            {
                                if (B_AddenPOnum != null)
                                {

                                    if (BPO == "" || BPO == null)
                                    {
                                        //api.Notifications.AddAdminNotification("BPO2", BPO.ToString());
                                        return new CBugDisplayDialogItem[] 
                    { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateDateInputField(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "B_PO_Adden",false,null),
                        //CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                         CreateDateInputField(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),

                          //new CBugDisplayDialogItem("Indetails_2", null, "Invoice Information", 3),
                          //CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                          //CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          //CreateTextInputField(rgbug, "InvoiceTotalAmount", "Invoice Total Amount", "POBalanceAmt"),


                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,

                         new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  ,

                       
                   };
                                    }
                                    else
                                    {
                                        return new CBugDisplayDialogItem[] 
             { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),

                        //   new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,

                           new CBugDisplayDialogItem("blanketpo_5", null, "Blanket PO Details", 3),
                             CreateText_B_PO(rgbug, "BlanketPO Number", "Blanket PO Number", "B_PO_ref",false,null),
                        CreateText_2(rgbug, "BlanketTotalAmount", "Blanket Total Amount", "POAmt",false,null),
                         CreateText_BPO(rgbug, "BalanceAmount", "Balance Amount", "PO_BalanceAmt",false,null),

                       new CBugDisplayDialogItem("Invhead_5", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                         CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                              new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateDateInputField(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "B_PO_Adden",false,null),
                        //CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                         CreateDateInputField(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),

                          //new CBugDisplayDialogItem("Indetails_2", null, "Invoice Information", 3),
                          //CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                          //CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          //CreateTextInputField(rgbug, "InvoiceTotalAmount", "Invoice Total Amount", "POBalanceAmt"),

                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,

                          new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  ,
                       
                   };
                                    }
                                }
                            }
                            else
                            {
                                if (BPO == "" || BPO == null)
                                {
                                   // api.Notifications.AddAdminNotification("BPO2", BPO.ToString());
                                    return new CBugDisplayDialogItem[] 
                    { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          //new CBugDisplayDialogItem("Indetails_2", null, "Invoice Information", 3),
                          //CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                          //CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          //CreateTextInputField(rgbug, "InvoiceTotalAmount", "Invoice Total Amount", "POBalanceAmt"),


                         new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,

                             new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  ,

                       
                   };
                                }
                                else
                                {
                                    return new CBugDisplayDialogItem[] 
             { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),

                        //   new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,

                           new CBugDisplayDialogItem("blanketpo_5", null, "Blanket PO Details", 3),
                             CreateText_B_PO(rgbug, "BlanketPO Number", "Blanket PO Number", "B_PO_ref",false,null),
                        CreateText_2(rgbug, "BlanketTotalAmount", "Blanket Total Amount", "POAmt",false,null),
                         CreateText_BPO(rgbug, "BalanceAmount", "Balance Amount", "PO_BalanceAmt",false,null),

                       new CBugDisplayDialogItem("Invhead_5", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                         CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          //new CBugDisplayDialogItem("Indetails_2", null, "Invoice Information", 3),
                          //CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                          //CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          //CreateTextInputField(rgbug, "InvoiceTotalAmount", "Invoice Total Amount", "POBalanceAmt"),

                        new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,

                      new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  ,
                       
                   };
                                }
                            }
                        }


                        else
                        {
                           // api.Notifications.AddAdminNotification("BPO3", BPO.ToString());
                            return new CBugDisplayDialogItem[] 
                   { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Blanket PO Amount", "Add_Fld2",false,null),
                          CreateText_B_PO(rgbug, "Blanket_PO Number", "Blanket PO Number", "B_PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,
                          // new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  
                       
                   };
                        }
                    }
                    else
                    {
                        if (type == "General PO")
                        {
                            if (B_AddenPOnum != "")
                            {
                                if (B_AddenPOnum != null)
                                {
                                    if (BPO == "" || BPO == null)
                                    {
                                        // if ((bug.ixStatus == 192) || (bug.ixStatus == 193))
                                        //  {
                                        return new CBugDisplayDialogItem[] 
                             { 
                         
                      //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                      

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                     //  CreateListField(rgbug, "Entity ID", "EntityID", "CWFCountry", "CWFCountry", true),
                     //  CreateListField1_SE(rgbug, "Location ID", "LocationID", "CWFLocation", "CWFLocation", true),
                       CreateListField1_SE(rgbug, "Department", "Department", "CWFDept", "CWFDept", true),
                        
                          //CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateTextInputField(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateTextInputField(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateTextInputField(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateTextInputField(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateDateInputField(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateListField_NotSort(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                         // CreateListField_NotSort(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", "CWFPostingperiod", true),
                        
                        CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                         CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                        // CreateText(rgbug, "Expires", "Expires", "DateString1"),

                        CreateDateInputField(rgbug, "Expires", "Expires", "DateString2"),
                        CreateTextInputField(rgbug, "PO Notes", "PO Notes", "sMemo"),
                       //  new CBugDisplayDialogItem("Invheadnew1", null, null, 1), 
                         //CreateTextInputField_memo(rgbug, "PO Notes", "PO Notes", "sMemo"),
                         CreateCheckbox(rgbug,"Force"),

                         new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateDateInputField(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "B_PO_Adden",false,null),
                        //CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                         CreateDateInputField(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),



                        new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   
                   };

                                        // }
                                    }
                                    else
                                    {

                                        return new CBugDisplayDialogItem[] 
                   { 
                         
                      //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                      

                       new CBugDisplayDialogItem("ApprInfo_5", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),

                       new CBugDisplayDialogItem("blanketpo_5", null, "Blanket PO Details", 3),

                       CreateText_B_PO(rgbug, "BlanketPO Number", "Blanket PO Number", "B_PO_ref",false,null),
                        CreateText_2(rgbug, "BlanketTotalAmount", "Blanket Total Amount", "POAmt",false,null),
                         CreateText_BPO(rgbug, "BalanceAmount", "Balance Amount", "PO_BalanceAmt",false,null),

                       new CBugDisplayDialogItem("bpo_2", null, "PO Header Information", 3),

                     //  CreateListField(rgbug, "Entity ID", "EntityID", "CWFCountry", "CWFCountry", true),
                     //  CreateListField1_SE(rgbug, "Location ID", "LocationID", "CWFLocation", "CWFLocation", true),
                       CreateListField1_SE(rgbug, "Department", "Department", "CWFDept", "CWFDept", true),
                        
                          //CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateTextInputField(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateTextInputField(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateTextInputField(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateTextInputField(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateDateInputField(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateListField_NotSort(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                         // CreateListField_NotSort(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", "CWFPostingperiod", true),
                        
                        CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                         CreateText_B_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                        // CreateText(rgbug, "Expires", "Expires", "DateString1"),

                        CreateDateInputField(rgbug, "Expires", "Expires", "DateString2"),
                       //  new CBugDisplayDialogItem("Invheadnew1", null, null, 1), 
                         //CreateTextInputField_memo(rgbug, "PO Notes", "PO Notes", "sMemo"),
                          CreateTextInputField(rgbug, "PO Notes", "PO Notes", "sMemo"),
                         CreateCheckbox(rgbug,"Force"),

                        new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   
                   };

                                    }
                                }
                            }
                            else

                            {
                                if (BPO == "" || BPO == null)
                                {
                                    // if ((bug.ixStatus == 192) || (bug.ixStatus == 193))
                                    //  {
                                    api.Notifications.AddError("Please select Blanket PO");
                                    api.Notifications.AddError("Ensure that you must select the Blanket PO for general PO's");
                                    return new CBugDisplayDialogItem[] 
                             { 
                         
                      //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                      

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                     //  CreateListField(rgbug, "Entity ID", "EntityID", "CWFCountry", "CWFCountry", true),
                     //  CreateListField1_SE(rgbug, "Location ID", "LocationID", "CWFLocation", "CWFLocation", true),
                       CreateListField1_SE(rgbug, "Department", "Department", "CWFDept", "CWFDept", true),
                        
                          //CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateTextInputField(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateTextInputField(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateTextInputField(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateTextInputField(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateDateInputField(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateListField_NotSort(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                         // CreateListField_NotSort(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", "CWFPostingperiod", true),
                        
                        CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                         CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                        // CreateText(rgbug, "Expires", "Expires", "DateString1"),

                        CreateDateInputField(rgbug, "Expires", "Expires", "DateString2"),
                        CreateTextInputField(rgbug, "PO Notes", "PO Notes", "sMemo"),
                       //  new CBugDisplayDialogItem("Invheadnew1", null, null, 1), 
                         //CreateTextInputField_memo(rgbug, "PO Notes", "PO Notes", "sMemo"),
                         CreateCheckbox(rgbug,"Force"),



                        new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   
                   };

                                    // }
                                }
                                else
                                {

                                    return new CBugDisplayDialogItem[] 
                   { 
                         
                      //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                      

                       new CBugDisplayDialogItem("ApprInfo_5", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),

                       new CBugDisplayDialogItem("blanketpo_5", null, "Blanket PO Details", 3),

                       CreateText_B_PO(rgbug, "BlanketPO Number", "Blanket PO Number", "B_PO_ref",false,null),
                        CreateText_2(rgbug, "BlanketTotalAmount", "Blanket Total Amount", "POAmt",false,null),
                         CreateText_BPO(rgbug, "BalanceAmount", "Balance Amount", "PO_BalanceAmt",false,null),

                       new CBugDisplayDialogItem("bpo_2", null, "PO Header Information", 3),

                     //  CreateListField(rgbug, "Entity ID", "EntityID", "CWFCountry", "CWFCountry", true),
                     //  CreateListField1_SE(rgbug, "Location ID", "LocationID", "CWFLocation", "CWFLocation", true),
                       CreateListField1_SE(rgbug, "Department", "Department", "CWFDept", "CWFDept", true),
                        
                          //CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateTextInputField(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateTextInputField(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateTextInputField(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateTextInputField(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateDateInputField(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateListField_NotSort(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                         // CreateListField_NotSort(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", "CWFPostingperiod", true),
                        
                        CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                         CreateText_B_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                        // CreateText(rgbug, "Expires", "Expires", "DateString1"),

                        CreateDateInputField(rgbug, "Expires", "Expires", "DateString2"),
                       //  new CBugDisplayDialogItem("Invheadnew1", null, null, 1), 
                         //CreateTextInputField_memo(rgbug, "PO Notes", "PO Notes", "sMemo"),
                          CreateTextInputField(rgbug, "PO Notes", "PO Notes", "sMemo"),
                         CreateCheckbox(rgbug,"Force"),

                        new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   
                   };

                                }
                            }


                        }


                        else
                        {

                            return new CBugDisplayDialogItem[] 
                   { 
                         
                      //  new CBugDisplayDialogItem("CWFUsercate", null, "Category", 1),
                         CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                         CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                      

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                          CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                          CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                     //  CreateListField(rgbug, "Entity ID", "EntityID", "CWFCountry", "CWFCountry", true),
                     //  CreateListField1_SE(rgbug, "Location ID", "LocationID", "CWFLocation", "CWFLocation", true),
                       CreateListField1_SE(rgbug, "Department", "Department", "CWFDept", "CWFDept", true),
                        
                          //CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateTextInputField(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateTextInputField(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateTextInputField(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateTextInputField(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateDateInputField(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateListField_NotSort(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                         // CreateListField_NotSort(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", "CWFPostingperiod", true),
                        
                        CreateText_2(rgbug, "TotalAmount", "Blanket PO Amount", "Add_Fld2",false,null),
                         CreateText_B_PO(rgbug, "Blanket_PO Number", "Blanket PO Number", "B_PO_Number",false,null),
                        // CreateText(rgbug, "Expires", "Expires", "DateString1"),

                        CreateDateInputField(rgbug, "Expires", "Expires", "DateString2"),
                       //  new CBugDisplayDialogItem("Invheadnew1", null, null, 1), 
                         CreateTextInputField(rgbug, "PO Notes", "PO Notes", "sMemo"),
                         //CreateCheckbox(rgbug,"Force"),


                        new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   
                   };

                        }
                    }
                }
               
                else
                {

                    return new CBugDisplayDialogItem[] 
                   { 
                     
                     //return null;
                      
                   };

                }
            }

            if (nMode == BugEditMode.Resolve)
            {
                if (bug.ixProject == 14)
                {
                    return new CBugDisplayDialogItem[]
                    {
                          new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
            CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
            CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
            CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                          new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                          CreateText(rgbug, "CustomForm", "CustomForm", "CWFCustomform", true,"CWFCustomform"),
                          CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor",true, "CWFVendor"),
                          CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod",true, "CWFPostingperiod"),
                          
                          CreateText(rgbug, "Country", "Country", "CWFCountry",true, "CWFCountry"),
                          CreateText(rgbug, "Currency", "Currency", "CWFCurrency",true, "CWFCurrency"),
                          CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                         
                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true,"CWFSubsidiary"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Date Invoice Entered", "sInvoiceEnteredDate"),

                           CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                           CreateText(rgbug, "Terms", "Terms", "CWFTerms",true, "CWFTerms"),
                           CreateText(rgbug, "InvoiceDueDate", "Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "NetAmount", "Net Amount", "Netamount"),
                          CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),
                          CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),

                         CreateText(rgbug, "AccountDesc", "Account Desc", "AccountDesc"),
                         CreateText(rgbug, "Key Areas", "KeyAreas", "CWFLocation", true,"CWFLocation"),
                         CreateText(rgbug, "Accrual", "Accrual", "CWFDept",true, "CWFDept"),
                          //new CBugDisplayDialogItem("Invhead_6", null, null, 2),
                         CreateText(rgbug, "Memo", "Memo", "sMemo"),
                        new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
                   
                }

                else if (bug.ixProject == 19)
                {
                    return new CBugDisplayDialogItem[] 
                   {
              new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence", 3),
            CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl2"),
            CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
            CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
            CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
            //CreateText(rgbug, "ExchangeRate", "Header Desc", "sExchangeRate"),
            CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
             CreateText(rgbug, "Currency", "Currency", "CWFCurrency",true, "CWFCurrency"),
            CreateText(rgbug, "InvoiceNumber", "Doc Number", "sInvoiceNumber"),
            CreateText(rgbug, "InvoiceDate", "Doc Date", "sInvoiceDate"),
              CreateText(rgbug, "TotalAmount", "Purchases", "TotalAmount"),
            CreateText(rgbug, "Memo", "Memo", "sMemo"),

            new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
                }

                else if (bug.ixProject == 22)
                {
                    return new CBugDisplayDialogItem[]
                    {
                          new CBugDisplayDialogItem("ApprInfo_1", null, "Notice Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                          new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                        
                          CreateText(rgbug, "Payroll/Non payroll", "Payroll/Non payroll", "CWFCustomform", true,"CWFCustomform"),
                          CreateText(rgbug, "Entity", "Entity", "CWFVendor",true ,"CWFVendor"),
                          CreateText(rgbug, "IRS/State", "IRS/State", "CWFTerms",true ,"CWFTerms"),
                          CreateText(rgbug, "State", "State", "CWFCountry", true,"CWFCountry"),
                          CreateText(rgbug, "Triage", "Triage", "CWFCurrency",true ,"CWFCurrency" ),
                          CreateText(rgbug, "Date of notice", "Date of notice", "sAddInfo"),
                          CreateText(rgbug, "Activity Type", "Activity Type", "CWFSubsidiary",true ,"CWFSubsidiary"),
                          CreateText(rgbug, "Period", "Period", "sExchangeRate"),
                          
                          CreateText(rgbug, "Department timeline", "Department timeline", "sInvoiceDueDate"),
                          CreateText(rgbug, "Amount", "Total Amount", "TotalAmount"),
                          CreateText(rgbug, "InternalTimeLine", "Internal TimeLine", "sInvoiceEnteredDate"),
                           CreateText(rgbug, "ClientAction", "Client Action", "CWFPostingperiod",true, "CWFPostingperiod"),
                          CreateText(rgbug, "Concern", "Concern", "AccountDesc"),
                          
                         // new CBugDisplayDialogItem("Invhead_7", null, null, 2),
                        
                          CreateText(rgbug, "To-do", "To-do", "sMemo"),

                       // new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };

                }


                else if (bug.ixProject == 25)
                {
                    string A_POnum = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "Add_Fld1"));

                    if (rgbug.Length != 1)
                    {
                        return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                   };
                    }

                    else if (A_POnum != "")
                    {
                     //   if (POnum != null)
                       // {
                            return new CBugDisplayDialogItem[] 
                   { 

                        CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                         //CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                         //CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         //CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                          CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true,"CWFApproverl1"),
                          CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true,"CWFApproverl2"),
                          CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3",true ,"CWFApproverl3"),
                          CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true,"CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "Old PO Header Information", 3),
                                               
                        //  CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
                         
                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString1"),
      
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),              
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateText(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "NewPOAmount", "New PO Total", "Add_Fld3",false,null),
                       CreateText_PO(rgbug, "NewPONumber", "New PO Number", "Add_Fld1",false,null),
                        CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"), 
                     new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                 //    new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                        
                   };
                      //  }
                    }
                    else
                    {
                        return new CBugDisplayDialogItem[] 
                   { 
                       CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                       
                      //    CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "PO Header Information", 3),

                        
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
                          
                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                         
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),     
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),               
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                     new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                        
                   };
                    }

                }

                else if (bug.ixProject == 26)
                {
                    string A_POnum = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "Add_Fld1"));

                    if (rgbug.Length != 1)
                    {
                        return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                   };
                    }

                    else if (A_POnum != "")
                    {
                        //   if (POnum != null)
                        // {
                        return new CBugDisplayDialogItem[] 
                   { 

                        CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                         //CreateListField1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                         //CreateListField1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         //CreateListField1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                          CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true,"CWFApproverl1"),
                          CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true,"CWFApproverl2"),
                          CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3",true ,"CWFApproverl3"),
                          CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true,"CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "Old PO Header Information", 3),
                                               
                        //  CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
                         
                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "sInvoiceDate"),
      
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),              
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateText(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "NewPOAmount", "New PO Total", "Add_Fld3",false,null),
                       CreateText_PO(rgbug, "NewPONumber", "New PO Number", "PONumberArt_A",false,null),
                        CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"), 
                  //   new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                 //    new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                        
                   };
                        //  }
                    }
                    else
                    {
                        return new CBugDisplayDialogItem[] 
                   { 
                       CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                       
                      //    CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                        
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
                          
                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                         
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),     
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),               
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                    // new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                        
                   };
                    }

                }

                else if (bug.ixProject == 23)
                {
                    return new CBugDisplayDialogItem[] 
                   {
              new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence", 3),
                          CreateText(rgbug, "StatementDate", "Statement Date", "sInvoiceDate"),
                          CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),
                          CreateText(rgbug, "Memo", "Memo", "sMemo"),

            new CBugDisplayDialogItem("item", ItemTable_TE(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
                }

                else if (bug.ixProject == 27)
                {
                    string type = (bug.GetPluginField("customfields@fogcreek.com", "typea718")).ToString();

                   // string type = (bug.GetPluginField("customfields@fogcreek.com", "typea718")).ToString();
                    string BPO = "";

                    CSelectQuery BlanketPO_NUM = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    BlanketPO_NUM.AddSelect("B_PO_ref");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    BlanketPO_NUM.AddWhere(sWhere2);
                    object NewPo = BlanketPO_NUM.GetScalarValue();
                    BPO = Convert.ToString(NewPo);

                    if (type == "General PO")
                    {
                        if (BPO == "" || BPO == null)
                        {
                            return new CBugDisplayDialogItem[] 
                     { 
                       CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                       
                      //    CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                        
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                        //  CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
                          
                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         CreateText_2(rgbug, "TotalAmount", "Total Amount", "Add_Fld2",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                         
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),     
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),               
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                     new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                        
                   };
                        }
                        else
                        {
                            return new CBugDisplayDialogItem[] 
              { 
                       CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                       
                      //    CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),

                          new CBugDisplayDialogItem("blanketpo_5", null, "Blanket PO Details", 3),

                              CreateText_B_PO(rgbug, "BlanketPO Number", "Blanket PO Number", "B_PO_ref",false,null),
                        CreateText_2(rgbug, "BlanketTotalAmount", "Blanket Total Amount", "POAmt",false,null),
                         CreateText_BPO(rgbug, "BalanceAmount", "Balance Amount", "PO_BalanceAmt",false,null),
                        

                       new CBugDisplayDialogItem("bpo_2", null, "PO Header Information", 3),

                        
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                        //  CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
                          
                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                         
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),     
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),               
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                     new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                        
                   };
                        }
                    }

                    else
                    {
                        return new CBugDisplayDialogItem[] 
                     { 
                       CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                       
                      //    CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                        
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                        //  CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
                          
                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         CreateText_2(rgbug, "TotalAmount", "Blanket Amount", "Add_Fld2",false,null),
                          CreateText_B_PO(rgbug, "Blanket_PO Number", "Blanket PO Number", "B_PO_Number",false,null),
                         
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),     
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),               
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                     new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                        
                   };
                    }
                }


                else
                {
                    return new CBugDisplayDialogItem[] 
                   {
                     //  return null;
                   };
                }
            }
            if (bug.ixProject == 14)
            {
                return new CBugDisplayDialogItem[]
                    {
                          new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                          CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                          CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                          CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                          CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                          new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                          CreateText(rgbug, "CustomForm", "CustomForm", "CWFCustomform", true,"CWFCustomform"),
                          CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor",true, "CWFVendor" ),
                          CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod",true, "CWFPostingperiod"),
                          
                          CreateText(rgbug, "Country", "Country", "CWFCountry",true, "CWFCountry"),
                          CreateText(rgbug, "Currency", "Currency", "CWFCurrency",true, "CWFCurrency"),
                          CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                         
                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true,"CWFSubsidiary"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Date Invoice Entered", "sInvoiceEnteredDate"),

                           CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                           CreateText(rgbug, "Terms", "Terms", "CWFTerms",true, "CWFTerms"),
                           CreateText(rgbug, "InvoiceDueDate", "Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "NetAmount", "Net Amount", "Netamount"),
                          CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),
                          CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),

                         CreateText(rgbug, "AccountDesc", "Account Desc", "AccountDesc"),
                         CreateText(rgbug, "Key Areas", "KeyAreas", "CWFLocation", true,"CWFLocation"),
                         CreateText(rgbug, "Accrual", "Accrual", "CWFDept",true, "CWFDept"),
                       //  new CBugDisplayDialogItem("Invhead_5", null, "Invoice5", 2),
                       //  CreateListField(rgbug, "Type", "Type", "Type", "Type", true),
                        
                         CreateText(rgbug, "Memo", "Memo", "sMemo"),
             new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
            }

            else if (bug.ixProject == 19)
            {
                return new CBugDisplayDialogItem[] 
                   {
               new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence", 3),
            CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl2"),
            CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
            CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
            CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
            //CreateText(rgbug, "ExchangeRate", "Header Desc", "sExchangeRate"),
            CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
             CreateText(rgbug, "Currency", "Currency", "CWFCurrency",true, "CWFCurrency"),
            CreateText(rgbug, "InvoiceNumber", "Doc Number", "sInvoiceNumber"),
            CreateText(rgbug, "InvoiceDate", "Doc Date", "sInvoiceDate"),
              CreateText(rgbug, "TotalAmount", "Purchases", "TotalAmount"),
            CreateText(rgbug, "Memo", "Memo", "sMemo"),
            new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
            }

            else if (bug.ixProject == 22)
            {
                return new CBugDisplayDialogItem[]
                    {
                          new CBugDisplayDialogItem("ApprInfo_1", null, "Notice Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                          new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                        
                          CreateText(rgbug, "Payroll/Non payroll", "Payroll/Non payroll", "CWFCustomform", true,"CWFCustomform"),
                          CreateText(rgbug, "Entity", "Entity", "CWFVendor",true ,"CWFVendor"),
                          CreateText(rgbug, "IRS/State", "IRS/State", "CWFTerms",true ,"CWFTerms"),
                          CreateText(rgbug, "State", "State", "CWFCountry", true,"CWFCountry"),
                          CreateText(rgbug, "Triage", "Triage", "CWFCurrency",true ,"CWFCurrency" ),
                          CreateText(rgbug, "Date of notice", "Date of notice", "sAddInfo"),
                          CreateText(rgbug, "Activity Type", "Activity Type", "sAddInfo",true ,"CWFSubsidiary"),
                          CreateText(rgbug, "Period", "Period", "sExchangeRate"),
                          
                          CreateText(rgbug, "Department timeline", "Department timeline", "sInvoiceDueDate"),
                          CreateText(rgbug, "Amount", "Total Amount", "TotalAmount"),
                           CreateText(rgbug, "InternalTimeLine", "Internal TimeLine", "sInvoiceEnteredDate"),
                            CreateText(rgbug, "ClientAction", "Client Action", "CWFPostingperiod",true, "CWFPostingperiod"),
                         CreateText(rgbug, "Concern", "Concern", "AccountDesc"),
                        
                        // new CBugDisplayDialogItem("Invhead_7", null, null, 2),
                       
                         CreateText(rgbug, "To-do", "To-do", "sMemo"),

                       // new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };

            }

            else if (bug.ixProject == 25)
            {
                if (rgbug.Length != 1)
                {
                    return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                   };
                }
                
                else
                {
                    return new CBugDisplayDialogItem[] 
                   { 
                       CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                  //   CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                     //     CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                    //      CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"), 

                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "PO Header Information", 3),

                          
                        //  CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
   
                          CreateText(rgbug, "Address Line1", "AddressLine1", "CWFCustomVal2"),
                          CreateText(rgbug, "Address Line2", "AddressLine2", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                           CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "Po Number", "PoNumber", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),                                             
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                      
                          new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
                }

            }

            else if (bug.ixProject == 26)
            {
                if (rgbug.Length != 1)
                {
                    return new CBugDisplayDialogItem[] 
                   { 
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                   };
                }

                else
                {
                    return new CBugDisplayDialogItem[] 
                   { 
                       CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                  //   CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                     //     CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                    //      CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"), 

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                          
                        //  CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
   
                          CreateText(rgbug, "Address Line1", "AddressLine1", "CWFCustomVal2"),
                          CreateText(rgbug, "Address Line2", "AddressLine2", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                           CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "Po Number", "PoNumber", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),                                             
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                      
                          new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
                }

            }

            else if (bug.ixProject == 27)
            {
                //CSelectQuery QryBVlinesumAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                //QryBVlinesumAmt.AddSelect("SUM(fAmount) as BVSumAcc,SUM(fTax) as BVSumTax");
                //string sWhere1 = (api.Database.PluginTableName("CGSInvoiceItems_MLA")) + ".ixBug = " + bug.ixBug.ToString();

                
                return new CBugDisplayDialogItem[] 
                   { 
                       CreateListField1(rgbug, "Status", "Status", "CWFUserResolve", "CWFUserResolve", true),
                       
                      //    CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                        
                          //CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                        //  CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),
                          
                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                         
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),     
                         // new CBugDisplayDialogItem("Invheadnew1", null, null, 1),               
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                     new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                        
                   };
            }

            else if (bug.ixProject == 23)
            {
                return new CBugDisplayDialogItem[] 
                   {
              new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence", 3),
                          CreateText(rgbug, "StatementDate", "Statement Date", "sInvoiceDate"),
                          CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),
                          CreateText(rgbug, "Memo", "Memo", "sMemo"),

            new CBugDisplayDialogItem("item", ItemTable_TE(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
            }

            else
            {
               return new CBugDisplayDialogItem[] 
                   {
                       //return null;
                   };
            }
            
            }

        private CBugDisplayDialogItem CreateCheckbox(CBug[] rgbug, string itemName)
        {

            System.Collections.IDictionary dictionary = new System.Collections.Specialized.ListDictionary();
            dictionary.Add("required", "true");
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            //DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = Forms.CheckboxInputString(api.AddPluginPrefix("CheckBox"), "true", "CheckedAttribute", "Force");
            return DialogItem;
            //CDialogItem itemEditId2 =
            //       new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute", "Header"));
            //   dlgTemplateNew.Template.Items.Add(itemEditId2);
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

        private CBugDisplayDialogItem CreateDateInputstringField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            try
            {
                DialogItem.sContent = Forms.DateInputString(api.PluginPrefix + fieldName, api.PluginPrefix + fieldName, Convert.ToString(GetText(rgbug, fieldName)), null);
            }
            catch
            {
                DialogItem.sContent = Forms.DateInputString(api.PluginPrefix + fieldName, api.PluginPrefix + fieldName, Convert.ToString(DateTime.Now),null);
            }
            return DialogItem;
        }
        
        private CBugDisplayDialogItem CreateCheckBoxField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = Forms.CheckboxInput(api.PluginPrefix + fieldName, GetText(rgbug, fieldName), true);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateListField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelects(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateListField_NotSort(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelects_Terms_Syner(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateListField_Posting(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelectDate(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateListField1(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelects1(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            return DialogItem;
        }
        
        private CBugDisplayDialogItem CreateListField1_SE(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelects_SE(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateListField_Cambridge(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelects_Cambridge(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
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

            CProject project1 = api.Project.GetProject(rgbug[0].ixProject);
            string enabledCGSWorkflowSettings = Convert.ToString(project1.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            //if (string.IsNullOrEmpty(enabledCGSWorkflowSettings) || "0".Equals(enabledCGSWorkflowSettings) || "1".Equals(enabledCGSWorkflowSettings))
            //{
            //    //don't do any intacct calls
            //    return null;
            //}
            if (string.IsNullOrEmpty(enabledCGSWorkflowSettings))
            {
                return null;
            }
            else if ("0".Equals(enabledCGSWorkflowSettings))
            {

                return null;
            }
            else if ("1".Equals(enabledCGSWorkflowSettings))
            {

                //don't do any intacct calls
                return null;
            }

           
            if (rgbug[0].ixProject == 14)
            {
                return new CBugDisplayDialogItem[] {
               new CBugDisplayDialogItem("Copy Case Details", EditableTable(rgbug[0].ixBug).RenderHtml()),
               //new CBugDisplayDialogItem("Copy Case Details", EditableTable_1(rgbug).RenderHtml()),
                               
                };
            }

            else if (rgbug[0].ixProject == 19)
            {
                return new CBugDisplayDialogItem[] {
             //  new CBugDisplayDialogItem("Copy Case Details", EditableTable(rgbug[0].ixBug).RenderHtml()),
               new CBugDisplayDialogItem("Copy Case Details", EditableTable_1(rgbug).RenderHtml()),
                               
                };
            }

            else if (rgbug[0].ixProject == 25 || rgbug[0].ixProject == 26)
            {
                return new CBugDisplayDialogItem[] {
             //  new CBugDisplayDialogItem("Copy Case Details", EditableTable(rgbug[0].ixBug).RenderHtml()),
               new CBugDisplayDialogItem("Copy Case Details", EditableTable_Synergis(rgbug).RenderHtml()),
                new CBugDisplayDialogItem("Addendum Details", EditableTable_Synergis_addendum(rgbug).RenderHtml()),
                               
                };
            }

            else if (rgbug[0].ixProject == 27)
            {
                string type = (rgbug[0].GetPluginField("customfields@fogcreek.com", "typea718")).ToString();


                return new CBugDisplayDialogItem[] {
             //  new CBugDisplayDialogItem("Copy Case Details", EditableTable(rgbug[0].ixBug).RenderHtml()),
               new CBugDisplayDialogItem("Copy PO Details", EditableTable_Spreadfast(rgbug).RenderHtml()),
               new CBugDisplayDialogItem("Addendum Details", EditableTable_Spreadfast_addendum(rgbug).RenderHtml()),
               new CBugDisplayDialogItem("Blanket_PO Details", EditableTable_Blanket_PO(rgbug).RenderHtml()),
                               
                };
            }
            //else if (rgbug[0].ixProject == 25)
            //{
            //    return new CBugDisplayDialogItem[] {
            // //  new CBugDisplayDialogItem("Copy Case Details", EditableTable(rgbug[0].ixBug).RenderHtml()),
            //   new CBugDisplayDialogItem("Addendum Details", EditableTable_Synergis_addendum(rgbug).RenderHtml()),
                               
            //    };
            //}
            else
            {
                return null;
            }

             //   return null;
                //End code addition by Alok
            
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

            // if (bug.ixCategory != 3 || !bug.fOpen)
            // {
            //     return null;
            // }

           // if (bug.ixProject != 9)
            //{return null;}

            CProject project = api.Project.GetProject(bug.ixProject);
            string enableCGSWorkflowSettings = Convert.ToString(project.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            if (string.IsNullOrEmpty(enableCGSWorkflowSettings))
            {
                return null;
            }
            else if ("0".Equals(enableCGSWorkflowSettings))
            {

                return null;
            }
            else if ("1".Equals(enableCGSWorkflowSettings))
            {

                //don't do any intacct calls
                return null;
            }
          
            if (bug.ixProject == 14)
            {
                return new CBugDisplayDialogItem[] 
                   {
                       new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                          CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1",true ,"CWFApproverl1"),
                          CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true,"CWFApproverl2"),
                          CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true,"CWFApproverl3"),
                          CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true,"CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                          CreateText(rgbug, "CustomForm", "CustomForm", "CWFCustomform",true ,"CWFCustomform"),
                          CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true,"CWFVendor"),
                          CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true,"CWFPostingperiod"),
                          CreateText(rgbug, "Country", "Country", "CWFCountry", true,"CWFCountry" ),
                          CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true,"CWFCurrency" ),
                          CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                         
                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true,"CWFSubsidiary"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Date Invoice Entered", "sInvoiceEnteredDate"),

                          CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true ,"CWFTerms"),
                          CreateText(rgbug, "InvoiceDueDate", "Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "NetAmount", "Net Amount", "Netamount"),
                          CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),
                          CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),

                          CreateText(rgbug, "AccountDesc", "Account Desc", "AccountDesc"),
                          CreateText(rgbug, "Key Areas", "KeyAreas", "CWFLocation", true,"CWFLocation"),
                          CreateText(rgbug, "Accrual", "Accrual", "CWFDept",true, "CWFDept"),
                         // new CBugDisplayDialogItem("Invheadnew", null, null, 2),
                          CreateText(rgbug, "Memo", "Memo", "sMemo"),
                      
             new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Invoice Items Detail", 3)  
             
                   };
            }
            else if (bug.ixProject == 19)
            {

                return new CBugDisplayDialogItem[] 
                   {
                       new CBugDisplayDialogItem("ApprInfo", null, "Invoice Approval Status", 3), 
                       CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl2"),
                       CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                       CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                       CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                     new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                       CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
                        CreateText(rgbug, "Currency", "Currency", "CWFCurrency",true, "CWFCurrency"),
                       CreateText(rgbug, "InvoiceNumber", "Doc Number", "sInvoiceNumber"),
                       CreateText(rgbug, "InvoiceDate", "Doc Date", "sInvoiceDate"),
                       CreateText(rgbug, "TotalAmount", "Purchases", "TotalAmount"),
                       CreateText(rgbug, "Memo", "Memo", "sMemo"),
             new CBugDisplayDialogItem("item", ItemTable_Cambridge(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Invoice Items Detail", 3)  
             
                   };
            }

            else if (bug.ixProject == 22)
            {
                return new CBugDisplayDialogItem[]
                    {
                          new CBugDisplayDialogItem("ApprInfo_1", null, "Notice Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                        
                           new CBugDisplayDialogItem("Invhead_1", null, "Notice Header Information", 3),
                          CreateText(rgbug, "Payroll/Non payroll", "Payroll/Non payroll", "CWFCustomform", true,"CWFCustomform"),
                          CreateText(rgbug, "Entity", "Entity", "CWFVendor",true ,"CWFVendor"),
                          CreateText(rgbug, "IRS/State", "IRS/State", "CWFTerms",true ,"CWFTerms"),
                          CreateText(rgbug, "State", "State", "CWFCountry", true,"CWFCountry"),
                          CreateText(rgbug, "Triage", "Triage", "CWFCurrency",true ,"CWFCurrency" ),
                          CreateText(rgbug, "Date of notice", "Date of notice", "sAddInfo"),
                          CreateText(rgbug, "Activity Type", "Activity Type", "CWFSubsidiary",true ,"CWFSubsidiary"),
                          CreateText(rgbug, "Period", "Period", "sExchangeRate"),
                          
                          CreateText(rgbug, "Department timeline", "Department timeline", "sInvoiceDueDate"),
                          CreateText(rgbug, "Amount", "Total Amount", "TotalAmount"),
                          CreateText(rgbug, "InternalTimeLine", "Internal TimeLine", "sInvoiceEnteredDate"),
                          CreateText(rgbug, "ClientAction", "Client Action", "CWFPostingperiod",true, "CWFPostingperiod"),
                         CreateText(rgbug, "Concern", "Concern", "AccountDesc"),
                          
                         //new CBugDisplayDialogItem("Invhead_7", null, null, 1),
                        
                         CreateText(rgbug, "To-do", "To-do", "sMemo"),
                       // new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };

            }

            else if (bug.ixProject == 25)
            {
                string POnum = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "Add_Fld1"));
                
                if ((bug.ixStatus == 180) || (bug.ixStatus == 184))
                {
                    if (POnum != "")
                    {
                        //return new CBugDisplayDialogItem[] 
                        //{}
                        // api.Notifications.AddAdminNotification("PON", "PON");
                        //  if (POnum != null)
                        //  {
                        return new CBugDisplayDialogItem[] 
                   { 
                         //CreateText_1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                         //CreateText_1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         //CreateText_1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "Old PO Header Information", 3),
                                               
                      //    CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         // CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                          //new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateText(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "Add_Fld1",false,null),
                        CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                           CreateText_2(rgbug, "PO Amount", "PO Amount", "POAmt",false,null),
                          CreateText_2(rgbug, "PO Balance Amount", "PO Balance Amount", "POBalanceAmt",false,null),
                     new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3),
                     new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "PO Invoice Details", 3)  
                 
                        
                   };
                        //   }
                    }

                    else
                    {
                        // api.Notifications.AddAdminNotification("PON-Null", "PON-NUll");
                        return new CBugDisplayDialogItem[] 
                   { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                                     CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),
                          CreateText_2(rgbug, "PO Amount", "PO Amount", "POAmt",false,null),
                          CreateText_2(rgbug, "PO Balance Amount", "PO Balance Amount", "POBalanceAmt",false,null),

                          new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3) ,
                          new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "PO Invoice Details", 3)  
                       
                   };
                    }
                }

                else
                {
                    if (POnum != "")
                    {
                        //return new CBugDisplayDialogItem[] 
                        //{}
                        // api.Notifications.AddAdminNotification("PON", "PON");
                        //  if (POnum != null)
                        //  {
                        return new CBugDisplayDialogItem[] 
                   { 
                         //CreateText_1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                         //CreateText_1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         //CreateText_1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "Old PO Header Information", 3),
                                               
                      //    CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         // CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                          //new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateText(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "Add_Fld1",false,null),
                        CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                     new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                 
                        
                   };
                        //   }
                    }

                    else
                    {
                        // api.Notifications.AddAdminNotification("PON-Null", "PON-NUll");
                        return new CBugDisplayDialogItem[] 
                   { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                                     CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumber",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                          new CBugDisplayDialogItem("item", ItemTable_synergs(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  
                       
                   };
                    }
                }

            }

            else if (bug.ixProject == 26)
            {
                string POnum = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "PONumberArt_A"));

                if ((bug.ixStatus == 180) || (bug.ixStatus == 184))
                {
                    if (POnum != "")
                    {
                        //return new CBugDisplayDialogItem[] 
                        //{}
                        // api.Notifications.AddAdminNotification("PON", "PON");
                        //  if (POnum != null)
                        //  {
                        return new CBugDisplayDialogItem[] 
                   { 
                         //CreateText_1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                         //CreateText_1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         //CreateText_1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "Old PO Header Information", 3),
                                               
                        CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         // CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                          //new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateText(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "PONumberArt_A",false,null),
                        CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                     
                           CreateText_2(rgbug, "PO Amount", "PO Amount", "POAmt",false,null),
                          CreateText_2(rgbug, "PO Balance Amount", "PO Balance Amount", "POBalanceAmt",false,null),
                     new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3) ,
                     new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "PO Invoice Details", 3)  
                 
                        
                   };
                        //   }
                    }

                    else
                    {
                        // api.Notifications.AddAdminNotification("PON-Null", "PON-NUll");
                        return new CBugDisplayDialogItem[] 
                   { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_1", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_1", null, "PO Header Information", 3),

                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),
                          CreateText_2(rgbug, "PO Amount", "PO Amount", "POAmt",false,null),
                          CreateText_2(rgbug, "PO Balance Amount", "PO Balance Amount", "POBalanceAmt",false,null),
                                                   
                          new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3),
                          new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "PO Invoice Details", 3)  
                       
                   };
                    }
                }
                else
                {
                    if (POnum != "")
                    {
                        //return new CBugDisplayDialogItem[] 
                        //{}
                        // api.Notifications.AddAdminNotification("PON", "PON");
                        //  if (POnum != null)
                        //  {
                        return new CBugDisplayDialogItem[] 
                   { 
                         //CreateText_1(rgbug, "Category", "Category", "CWFUsercate", "CWFUsercate", true),
                         //CreateText_1(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", "CWFUserAssign", true),
                         //CreateText_1(rgbug, "Status", "Status", "CWFUserStatus", "CWFUserStatus", true),
                       
                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "Old PO Header Information", 3),
                                               
                      //    CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                         // CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                          //new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                        new CBugDisplayDialogItem("New_Invhead_2", null, "New PO Header Information", 3),
                         CreateText(rgbug, "New PO Date", "New PO Date", "Add_Fld5"),
                        CreateText_2(rgbug, "Amount", "New PO Total", "Add_Fld3",false,null),
                        CreateText_PO(rgbug, "NewPONumber", "New PO Number", "PONumberArt_A",false,null),
                        CreateText(rgbug, "NewPOExpires", "New PO Expires", "Add_Fld6"),
                     new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3) ,
                     // new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  
                 
                        
                   };
                        //   }
                    }
                    else
                    {
                        // api.Notifications.AddAdminNotification("PON-Null", "PON-NUll");
                        return new CBugDisplayDialogItem[] 
                   { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                                     CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                          CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total Amount", "TotalAmount",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PONumberArt",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "Vendor PO Notes", "Vendor PO Notes", "sMemo"),

                          new CBugDisplayDialogItem("item", ItemTable_synergs_Artium(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3),
                         //  new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "PO Invoice Details", 3)  ,
                       
                   };
                    }
                }

            }
            else if (bug.ixProject == 27)
            {
                
                string type = (bug.GetPluginField("customfields@fogcreek.com", "typea718")).ToString();

                string BPO = "";

                CSelectQuery BlanketPO_NUM = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                BlanketPO_NUM.AddSelect("B_PO_ref");
                string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                BlanketPO_NUM.AddWhere(sWhere2);
                object NewPo = BlanketPO_NUM.GetScalarValue();
                BPO = Convert.ToString(NewPo);

             

                if ((bug.ixStatus == 192) || (bug.ixStatus == 193))
                {
                    
                    if (type == "General PO")
                    {
                       // api.Notifications.AddAdminNotification("BPO1", BPO.ToString());
                        if (BPO == "" || BPO == null)
                        {
                           // api.Notifications.AddAdminNotification("BPO2", BPO.ToString());
                            return new CBugDisplayDialogItem[] 
                    { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          //new CBugDisplayDialogItem("Indetails_2", null, "Invoice Information", 3),
                          //CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                          //CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          //CreateText_2(rgbug, "InvoiceTotalAmount", "Invoice Total Amount", "POBalanceAmt",false,null),

                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3) ,
                           //new CBugDisplayDialogItem("items", ItemTable_Spreadfast_Inv(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items1", 3) 
                           new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "PO Invoice Details", 3)  
                       
                   };
                        }
                        else
                        {
                            return new CBugDisplayDialogItem[] 
             { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),

                        //   new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,

                           new CBugDisplayDialogItem("blanketpo_5", null, "Blanket PO Details", 3),
                             CreateText_B_PO(rgbug, "BlanketPO Number", "Blanket PO Number", "B_PO_ref",false,null),
                        CreateText_2(rgbug, "BlanketTotalAmount", "Blanket Total Amount", "POAmt",false,null),
                         CreateText_BPO(rgbug, "BalanceAmount", "Balance Amount", "PO_BalanceAmt",false,null),

                       new CBugDisplayDialogItem("Invhead_5", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                         CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          //new CBugDisplayDialogItem("Indetails_2", null, "Invoice Information", 3),
                          //CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                          //CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          //CreateText_2(rgbug, "InvoiceTotalAmount", "Invoice Total Amount", "POBalanceAmt",false,null),

                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,
                          // new CBugDisplayDialogItem("items", ItemTable_Spreadfast_Inv(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items1", 3) 
                          new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "PO Invoice Details", 3)  
                       
                   };
                        }
                    }


                    else
                    {
                      //  api.Notifications.AddAdminNotification("BPO3", BPO.ToString());
                        return new CBugDisplayDialogItem[] 
                   { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Blanket PO Amount", "Add_Fld2",false,null),
                          CreateText_B_PO(rgbug, "Blanket_PO Number", "Blanket PO Number", "B_PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  
                       
                   };
                    }
                }
                else
                {
                    if (type == "General PO")
                    {
                        //if (BPO == "" || BPO == null)
                       // {
                           
                        //}
                       // api.Notifications.AddAdminNotification("BPO1", BPO.ToString());
                        if (BPO == "" || BPO == null)
                        {
                            api.Notifications.AddError("Please select Blanket PO");
                            api.Notifications.AddError("Ensure that you must select the Blanket PO for general PO's");

                         //   api.Notifications.AddAdminNotification("BPO2", BPO.ToString());
                            return new CBugDisplayDialogItem[] 
                    { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                          CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3) , 
                         // new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "PO Invoice Details", 3)  
                       
                   };
                        }
                        else
                        {
                            return new CBugDisplayDialogItem[] 
                          { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),

                        //   new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,

                           new CBugDisplayDialogItem("blanketpo_5", null, "Blanket PO Details", 3),
                         CreateText_B_PO(rgbug, "BlanketPO Number", "Blanket PO Number", "B_PO_ref",false,null),
                         CreateText_2(rgbug, "BlanketTotalAmount", "Blanket Total Amount", "POAmt",false,null),
                         CreateText_BPO(rgbug, "BalanceAmount", "Balance Amount", "PO_BalanceAmt",false,null),

                       new CBugDisplayDialogItem("Invhead_5", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Total PO Amount", "Add_Fld2",false,null),
                         CreateText_PO(rgbug, "PO Number", "PO Number", "PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  ,
                        //  new CBugDisplayDialogItem("itemPO", ItemTable_POInvoiceDetails(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "PO Invoice Details", 3)  
                       
                   };
                        }
                    }


                    else
                    {
                       // api.Notifications.AddAdminNotification("BPO3", BPO.ToString());
                        return new CBugDisplayDialogItem[] 
                   { 
                        // CreateText(rgbug, "Category", "Category", "CWFUsercate", true,"CWFUsercate"),
                       //   CreateText(rgbug, "Assisgned To", "Assisgned To", "CWFUserAssign", true,"CWFUserAssign"),
                       //   CreateText(rgbug, "Status", "Status", "CWFUserStatus", true,"CWFUserStatus"),

                       new CBugDisplayDialogItem("ApprInfo_2", null, "PO Approval Sequence Setup", 3),
                           CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl1"),
                           CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                           CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                           CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                       new CBugDisplayDialogItem("Invhead_2", null, "PO Header Information", 3),

                         // CreateText(rgbug, "Entity ID", "EntityID", "CWFCountry", true, "CWFCountry"),
                           //          CreateText(rgbug, "Universities", "LocationID", "CWFLocation", true, "CWFLocation"),
                          CreateText(rgbug, "Department", "Department", "CWFDept", true, "CWFDept"),
                          CreateText(rgbug, "Vendor", "Vendor", "CWFVendor"),

                          CreateText(rgbug, "Address Line", "AddressLine", "CWFCustomVal2"),
                          CreateText(rgbug, "State/Zipcode", "City/State/Zip Code", "CWFCustomVal3"),
                          CreateText(rgbug, "Phone Number", "Phone Number", "Remarks"),

                          CreateText(rgbug, "PO Date", "PO Date", "DateString1"),
                          CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),
                         // CreateText(rgbug, "Budgeted", "Budgeted", "CWFPostingperiod", true, "CWFPostingperiod"),
                          
                          CreateText_2(rgbug, "TotalAmount", "Blanket PO Amount", "Add_Fld2",false,null),
                          CreateText_B_PO(rgbug, "Blanket_PO Number", "Blanket PO Number", "B_PO_Number",false,null),
                          CreateText(rgbug, "Expires", "Expires", "DateString2"),   
                        //  new CBugDisplayDialogItem("Invheadnew", null, null, 1),                
                          CreateText(rgbug, "PO Notes", "PO Notes", "sMemo"),

                          new CBugDisplayDialogItem("item", ItemTable_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  
                       
                   };
                    }
                }
            }


            else if (bug.ixProject == 23)
            {
                return new CBugDisplayDialogItem[] 
                   {
           // new CBugDisplayDialogItem("ApprInfo", null, "Invoice Approval Status", 3), 
            //CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl2"),
            //CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
            //CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
            //CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            new CBugDisplayDialogItem("Invhead", null, "Invoice Header Information", 3), 
             CreateText(rgbug, "StatementDate", "Statement Date", "sInvoiceDate"),
                          CreateText(rgbug, "TotalAmount", "Total Amount", "TotalAmount"),
                          CreateText(rgbug, "Memo", "Memo", "sMemo"),
             new CBugDisplayDialogItem("item", ItemTable_TE(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Invoice Items Detail", 1)  
             
                   };
            }

            else
            {
                return new CBugDisplayDialogItem[] 
                   {
                      // return null;
                   };
            }
        }

        private CBugDisplayDialogItem CreateText(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            return CreateText(rgbug, itemName, fielddisplay, fieldName, false, null);
        }

        private CBugDisplayDialogItem CreateText(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
            return DialogItem;
        }
 

        private CBugDisplayDialogItem CreateText_1(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName)).Trim();
            
            int atlevel = Convert.ToInt32(rgbug[0].GetPluginField(PLUGIN_ID, "ixAtlevel"));

            if (rgbug[0].ixProject == 14)  //for Bazaarvoice
            {

                if (atlevel == 1)
                {
                    if (rgbug[0].ixStatus == 73 && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }
                }

                if (atlevel == 2)
                {
                    if (sValue != "-" && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }

                    else if (rgbug[0].ixStatus == 73 && fieldName == "CWFApproverl2")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }
                else if (atlevel == 3)
                {
                    if (sValue != "-" && (fieldName == "CWFApproverl1" || fieldName == "CWFApproverl2"))
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;
                    }

                    else if (rgbug[0].ixStatus == 73 && fieldName == "CWFApproverl3")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if (atlevel == 4)
                {
                    if (sValue != "-" && fieldName != "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }
                    else if (rgbug[0].ixStatus == 73 && fieldName == "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if ((sValue != "-") && (atlevel == 5))
                {
                    //api.Notifications.AddMessage("atlevel  ||" + atlevel + " sValue||" + sValue);

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                    return DialogItem;

                }

                else
                {

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                    return DialogItem;

                }
            }
            else if (rgbug[0].ixProject == 22)  // for Trilogy
            {
                if (atlevel == 1)
                {
                    if (rgbug[0].ixStatus == 157 && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }
                }

                if (atlevel == 2)
                {
                    if (sValue != "-" && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }

                    else if (rgbug[0].ixStatus == 157 && fieldName == "CWFApproverl2")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }
                else if (atlevel == 3)
                {
                    if (sValue != "-" && (fieldName == "CWFApproverl1" || fieldName == "CWFApproverl2"))
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;
                    }

                    else if (rgbug[0].ixStatus == 157 && fieldName == "CWFApproverl3")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if (atlevel == 4)
                {
                    if (sValue != "-" && fieldName != "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }
                    else if (rgbug[0].ixStatus == 157 && fieldName == "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if ((sValue != "-") && (atlevel == 5))
                {
                    //api.Notifications.AddMessage("atlevel  ||" + atlevel + " sValue||" + sValue);

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                    return DialogItem;

                }

                else
                {

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                    return DialogItem;

                }
            }

            else if (rgbug[0].ixProject == 19)  // for cambrige soft
            {
                if (atlevel == 1)
                {
                    if (rgbug[0].ixStatus == 141 && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }
                }

                if (atlevel == 2)
                {
                    if (sValue != "-" && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }

                    else if (rgbug[0].ixStatus == 141 && fieldName == "CWFApproverl2")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }
                else if (atlevel == 3)
                {
                    if (sValue != "-" && (fieldName == "CWFApproverl1" || fieldName == "CWFApproverl2"))
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;
                    }

                    else if (rgbug[0].ixStatus == 141 && fieldName == "CWFApproverl3")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if (atlevel == 4)
                {
                    if (sValue != "-" && fieldName != "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }
                    else if (rgbug[0].ixStatus == 141 && fieldName == "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if ((sValue != "-") && (atlevel == 5))
                {
                    //api.Notifications.AddMessage("atlevel  ||" + atlevel + " sValue||" + sValue);

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                    return DialogItem;

                }

                else
                {

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                    return DialogItem;

                }
            }

           else if (rgbug[0].ixProject == 25)  //for Synergis
            {

                if (atlevel == 1)
                {
                    if (rgbug[0].ixStatus == 184 && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }
                }

                if (atlevel == 2)
                {
                    if (sValue != "-" && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }

                    else if (rgbug[0].ixStatus == 184 && fieldName == "CWFApproverl2")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }
                else if (atlevel == 3)
                {
                    if (sValue != "-" && (fieldName == "CWFApproverl1" || fieldName == "CWFApproverl2"))
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;
                    }

                    else if (rgbug[0].ixStatus == 184 && fieldName == "CWFApproverl3")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if (atlevel == 4)
                {
                    if (sValue != "-" && fieldName != "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }
                    else if (rgbug[0].ixStatus == 184 && fieldName == "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if ((sValue != "-") && (atlevel == 5))
                {
                    //api.Notifications.AddMessage("atlevel  ||" + atlevel + " sValue||" + sValue);

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                    return DialogItem;

                }

                else
                {

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                    return DialogItem;

                }
            }

            else if (rgbug[0].ixProject == 26)  //for Artium
            {

                if (atlevel == 1)
                {
                    if (rgbug[0].ixStatus == 184 && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }
                }

                if (atlevel == 2)
                {
                    if (sValue != "-" && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }

                    else if (rgbug[0].ixStatus == 184 && fieldName == "CWFApproverl2")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }
                else if (atlevel == 3)
                {
                    if (sValue != "-" && (fieldName == "CWFApproverl1" || fieldName == "CWFApproverl2"))
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;
                    }

                    else if (rgbug[0].ixStatus == 184 && fieldName == "CWFApproverl3")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if (atlevel == 4)
                {
                    if (sValue != "-" && fieldName != "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }
                    else if (rgbug[0].ixStatus == 184 && fieldName == "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if ((sValue != "-") && (atlevel == 5))
                {
                    //api.Notifications.AddMessage("atlevel  ||" + atlevel + " sValue||" + sValue);

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                    return DialogItem;

                }

                else
                {

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                    return DialogItem;

                }
            }

            else if (rgbug[0].ixProject == 27)  //for Spreadfast
            {

                if (atlevel == 1)
                {
                    if (rgbug[0].ixStatus == 193 && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }
                }

                if (atlevel == 2)
                {
                    if (sValue != "-" && fieldName == "CWFApproverl1")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }

                    else if (rgbug[0].ixStatus == 193 && fieldName == "CWFApproverl2")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }
                else if (atlevel == 3)
                {
                    if (sValue != "-" && (fieldName == "CWFApproverl1" || fieldName == "CWFApproverl2"))
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;
                    }

                    else if (rgbug[0].ixStatus == 193 && fieldName == "CWFApproverl3")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if (atlevel == 4)
                {
                    if (sValue != "-" && fieldName != "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                        return DialogItem;

                    }
                    else if (rgbug[0].ixStatus == 193 && fieldName == "CWFApproverl4")
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Rejected)");
                        return DialogItem;
                    }

                    else
                    {
                        CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                        DialogItem.sLabel = fielddisplay;
                        DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                        return DialogItem;
                    }

                }

                else if ((sValue != "-") && (atlevel == 5))
                {
                    //api.Notifications.AddMessage("atlevel  ||" + atlevel + " sValue||" + sValue);

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue + " (Approved)");
                    return DialogItem;

                }

                else
                {

                    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
                    DialogItem.sLabel = fielddisplay;
                    DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
                    return DialogItem;

                }
            }

            else
            {
                return null;
            }
           

        }
        
        private CBugDisplayDialogItem CreateText_2(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string value = "";
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            if (sValue == null || sValue == "")
            {
            }
            else
            {
                decimal val = Convert.ToDecimal(sValue);
                value = val.ToString("C");
                // sValue = "PO-1-0" + sValue.ToString();
               // sValue = "$" + sValue;
            }
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = HttpUtility.HtmlEncode(value);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateText_PO(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            if (sValue == null || sValue == "")
            {
            }
            else if (rgbug[0].ixProject == 25)
            {
                sValue = "PO-1-0" + sValue.ToString();
            }
            else if (rgbug[0].ixProject == 27)
            {
                sValue = "PO-0" + sValue.ToString();
            }
            else
            {
                sValue = "PO-2-0" + sValue.ToString();
            }

            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateText_B_PO(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            if (sValue == null || sValue == "")
            {
            }
            else if (rgbug[0].ixProject == 27)
            {
                sValue = "B-PO-0" + sValue.ToString();
            }
            //else
            //{
            //    sValue = "PO-2-0" + sValue.ToString();
            //}
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateText_BPO(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string value = "";
            string sValue = Convert.ToString(Convert.ToDouble(rgbug[0].GetPluginField(PLUGIN_ID, "POAmt")) - (func_BPOBalanceAMount((rgbug[0].GetPluginField(PLUGIN_ID, "B_PO_ref")).ToString())));// Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
             decimal val = Convert.ToDecimal(sValue);
                value = val.ToString("C");

            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = HttpUtility.HtmlEncode(value);
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
            if (flag == 1)
            {
               
                       if ((bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")) != null)
                        {
                            if (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013").ToString() != "")
                            {

                                string CCemail = (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")).ToString();
                                mailsender(CCemail.Trim(), bug, MailSub, MailBody, ixperson);
                                this.api.Notifications.AddMessage("A CCEmail has been sent Successfully");
                            }
                        }
                    
                
            }
         
        }

        public void BugCommitBefore(CBug bug, BugAction nBugAction, CBugEvent bugevent,
            bool fPublic)
        {

            CProject project = api.Project.GetProject(bug.ixProject);
            string enableCGSWorkflowSettings = Convert.ToString(project.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            if (string.IsNullOrEmpty(enableCGSWorkflowSettings) || "0".Equals(enableCGSWorkflowSettings) || "1".Equals(enableCGSWorkflowSettings))
            {
                //don't do anything
                return;
            }

            {
                ExtractValue(bug, bugevent, "sInvoiceNumber", "Invoice Number");
                ExtractValue(bug, bugevent, "CWFApproverl1", "Level-2 Approver");
                ExtractValue(bug, bugevent, "CWFApproverl2", "Level-2 Approver");
                ExtractValue(bug, bugevent, "CWFApproverl3", "Level-3 Approver");
                ExtractValue(bug, bugevent, "CWFApproverl4", "Level-4 Approver");
                ExtractValue(bug, bugevent, "CWFCustomform", "Document Type");
                ExtractValue(bug, bugevent, "CWFVendor", "Vendor");
                ExtractValue(bug, bugevent, "CWFCountry", "Country");
                ExtractValue(bug, bugevent, "CWFCurrency", "Currency");
                ExtractValue(bug, bugevent, "CWFPostingperiod", "GL Posting Date");
                ExtractValue(bug, bugevent, "CWFSubsidiary", "Subsidiary");
                ExtractValue(bug, bugevent, "CWFTerms", "Terms");
                ExtractValue(bug, bugevent, "sInvoiceDate", "Invoice Date");
                ExtractValue(bug, bugevent, "sInvoiceEnteredDate", "Invoice Entered Date");
                ExtractValue(bug, bugevent, "sExchangeRate", "Exchange Rate");
                ExtractValue(bug, bugevent, "sInvoiceAmount", "Invoice Amount");
                ExtractValue(bug, bugevent, "sTaxAmount", "Tax Amount");
                ExtractValue(bug, bugevent, "sInvoiceDueDate", "Invoice DueDate");
                ExtractValue(bug, bugevent, "sMemo", "sMemo");
                ExtractValue(bug, bugevent, "sAddInfo", "AddInfo");
                ExtractValue(bug, bugevent, "Netamount", "NetAmount");
                ExtractValue(bug, bugevent, "TotalAmount", "Total Amount");
                ExtractValue(bug, bugevent, "AccountDesc", "Account Desc");

                ExtractValue(bug, bugevent, "CWFCustomVal2", "AddressLine1");
                ExtractValue(bug, bugevent, "CWFCustomVal3", "AddressLine2");

                ExtractValue(bug, bugevent, "CWFUsercate", "Category");
                ExtractValue(bug, bugevent, "CWFUserAssign", "UserAssign");
                ExtractValue(bug, bugevent, "CWFUserStatus", "UserStatus");
                ExtractValue(bug, bugevent, "Add_Fld4", "Add_Fld4");
                ExtractValue(bug, bugevent, "CWFUserResolve", "UserResolve");

                ExtractValue(bug, bugevent, "Add_Fld1", "Add_Fld1");
                ExtractValue(bug, bugevent, "Add_Fld2", "Add_Fld2");
                ExtractValue(bug, bugevent, "Add_Fld3", "Add_Fld3");
                ExtractValue(bug, bugevent, "Add_Fld5", "Add_Fld5");
                ExtractValue(bug, bugevent, "Add_Fld6", "Add_Fld6");

                ExtractValue(bug, bugevent, "Remarks", "Remarks");

                ExtractValue(bug, bugevent, "CWFLocation", "LocationId");
                ExtractValue(bug, bugevent, "CWFDept", "Dept");
                ExtractValue(bug, bugevent, "DateString1", "DateString1");
                ExtractValue(bug, bugevent, "DateString2", "DateString2");

                ExtractValue(bug, bugevent, "PONumber", "PONumber");
                ExtractValue(bug, bugevent, "PO_Number", "PO_Number");
                ExtractValue(bug, bugevent, "B_PO_Number", "B_PO_Number");
                
                ExtractValue(bug, bugevent, "PONumberArt", "PONumberArt");
                ExtractValue(bug, bugevent, "PONumberArt_A", "PONumberArt_A");

                ExtractValue(bug, bugevent, "POBalanceAmt", "POBalanceAmt");
                ExtractValue(bug, bugevent, "POAmt", "POAmt");
                ExtractValue(bug, bugevent, "ixproject", "ixproject");
                ExtractValue(bug, bugevent, "B_PO_ref", "B_PO_ref");

                ExtractValue(bug, bugevent, "B_PO_Adden", "B_PO_Adden");
                

                //  bug.SetPluginField(PLUGIN_ID, "ixproject", bug.ixProject.ToString());

                if (bug.ixProject == 14)
                {

                    double dBVLineAmount = 0;
                    double dBVLineTax = 0;

                    CSelectQuery QryBVlinesumAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    QryBVlinesumAmt.AddSelect("SUM(fAmount) as BVSumAcc,SUM(fTax) as BVSumTax");
                    string sWhere1 = (api.Database.PluginTableName("CGSInvoiceItems_MLA")) + ".ixBug = " + bug.ixBug.ToString();

                    sWhere1 += " and iDeleted = 0";
                    QryBVlinesumAmt.AddWhere(sWhere1);
                    DataSet ds1 = QryBVlinesumAmt.GetDataSet();

                    if (null != ds1.Tables && ds1.Tables.Count == 1 && ds1.Tables[0].Rows.Count == 1)
                    {
                        try
                        {
                            dBVLineAmount = Convert.ToDouble(Convert.ToString(ds1.Tables[0].Rows[0]["BVSumAcc"]));
                            dBVLineTax = Convert.ToDouble(Convert.ToString(ds1.Tables[0].Rows[0]["BVSumTax"]));
                        }

                        catch
                        {
                            dBVLineAmount = 0d;
                            dBVLineTax = 0d;
                        }

                    }

                    bug.SetPluginField(PLUGIN_ID, "Netamount", dBVLineAmount);
                    bug.SetPluginField(PLUGIN_ID, "sTaxAmount", dBVLineTax);
                    string totalamt = Convert.ToString(dBVLineAmount + dBVLineTax);
                    bug.SetPluginField(PLUGIN_ID, "TotalAmount", totalamt);

                    DataSet dsItems = FetchItems(bug.ixBug, true);
                    double dAmount = 0d;
                    if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                        {
                            try
                            {
                                dAmount += Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                            }
                            catch
                            {
                                dAmount += 0d;
                            }
                        }
                    }

                    double invoiceAmount = 0d;
                    try
                    {
                        invoiceAmount = Convert.ToDouble(bug.GetPluginField(PLUGIN_ID, "sInvoiceAmount"));
                    }
                    catch
                    {
                        bug.SetPluginField(PLUGIN_ID, "sInvoiceAmount", "0");
                        api.Notifications.AddMessage("Invoice Amount value is invalid. Reset to 0.");
                        invoiceAmount = 0d;
                    }


                    double TaxAmount = 0d;
                    try
                    {
                        TaxAmount = Convert.ToDouble(bug.GetPluginField(PLUGIN_ID, "sTaxAmount"));
                    }
                    catch
                    {
                        bug.SetPluginField(PLUGIN_ID, "sTaxAmount", "0");
                        api.Notifications.AddMessage("Tax Amount value is invalid. Reset to 0.");
                        invoiceAmount = 0d;
                    }

                }
            }

            if (bug.ixProject == 19)
            {
                // api.Notifications.AddAdminNotification("tamt", "123");
                CSelectQuery QryCBsumAmt = api.Database.NewSelectQuery("Plugin_67_CGSInvoiceItems_MLA");
                QryCBsumAmt.AddSelect("fAmount as CSAmount");
                string sWhere1 = ("Plugin_67_CGSInvoiceItems_MLA") + ".ixBug = " + bug.ixBug;
                sWhere1 += " and sExtra2 = ' Credit'";
                sWhere1 += " and iDeleted = 0";
                QryCBsumAmt.AddWhere(sWhere1);

                DataSet dsCS = QryCBsumAmt.GetDataSet();
                double CSAmount = 0d;
                if (null != dsCS.Tables && dsCS.Tables.Count == 1 && dsCS.Tables[0].Rows.Count == 1)
                {
                    CSAmount = Convert.ToDouble(dsCS.Tables[0].Rows[0]["CSAmount"].ToString());
                    bug.SetPluginField(PLUGIN_ID, "TotalAmount", CSAmount);
                }
            }

            //if (bug.ixProject == 25)
            //{

            //    double dBVLineAmount = 0;

            //    CSelectQuery QrySynlinesumAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
            //    QrySynlinesumAmt.AddSelect("SUM(fAmount * fTax) as SynSumAcc");
            //    string sWhere1 = (api.Database.PluginTableName("CGSInvoiceItems_MLA")) + ".ixBug = " + bug.ixBug.ToString();

            //    sWhere1 += " and iDeleted = 0";
            //    QrySynlinesumAmt.AddWhere(sWhere1);
            //    DataSet Synds = QrySynlinesumAmt.GetDataSet();

            //    if (null != Synds.Tables && Synds.Tables.Count == 1 && Synds.Tables[0].Rows.Count == 1)
            //    {
            //        try
            //        {
            //            dBVLineAmount = Convert.ToDouble(Convert.ToString(Synds.Tables[0].Rows[0]["SynSumAcc"]));
            //            bug.SetPluginField(PLUGIN_ID, "TotalAmount", dBVLineAmount);
            //        }

            //        catch
            //        {
            //            dBVLineAmount = 0d;

            //        }

            //    }
            //}

            // Automating Total amount in header

            if (bug.ixProject == 25 || bug.ixProject == 26 || bug.ixProject == 27)
            {
                bug.SetPluginField(PLUGIN_ID, "ixproject", bug.ixProject.ToString());
               // api.Notifications.AddAdminNotification("ixproject_SF", bug.ixProject.ToString());
                string newponum = "";

                double dBVLineAmount = 0;

                CSelectQuery QrySynlinesumAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                QrySynlinesumAmt.AddSelect("SUM(fAmount * fTax) as SynSumAcc");
                string sWhere1 = (api.Database.PluginTableName("CGSInvoiceItems_MLA")) + ".ixBug = " + bug.ixBug.ToString();

                sWhere1 += " and iDeleted = 0";
                QrySynlinesumAmt.AddWhere(sWhere1);

                if (bug.ixProject == 25)
                {
                    CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    Qrynewpon.AddSelect("Add_Fld1");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    Qrynewpon.AddWhere(sWhere2);
                    object NewPo = Qrynewpon.GetScalarValue();
                    newponum = Convert.ToString(NewPo);
                }
                else if (bug.ixProject == 27)
                {
                    CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    Qrynewpon.AddSelect("Add_Fld1");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    Qrynewpon.AddWhere(sWhere2);
                    object NewPo = Qrynewpon.GetScalarValue();
                    newponum = Convert.ToString(NewPo);
                }
                else
                {
                    CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    Qrynewpon.AddSelect("PONumberArt_A");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    Qrynewpon.AddWhere(sWhere2);
                    object NewPo = Qrynewpon.GetScalarValue();
                    newponum = Convert.ToString(NewPo);
                }

                //  api.Notifications.AddAdminNotification("newponum1", newponum.ToString());

                DataSet Synds = QrySynlinesumAmt.GetDataSet();

                if (null != Synds.Tables && Synds.Tables.Count == 1 && Synds.Tables[0].Rows.Count == 1)
                {
                    //  newponum = (bug.GetPluginField(PLUGIN_ID, "Add_Fld1")).ToString();
                    //   api.Notifications.AddAdminNotification("newponum2", newponum.ToString());

                    if (newponum == null || newponum == "")
                    {
                        //  if (newponum != "")
                        {
                            try
                            {

                                dBVLineAmount = Convert.ToDouble(Convert.ToString(Synds.Tables[0].Rows[0]["SynSumAcc"]));
                                bug.SetPluginField(PLUGIN_ID, "TotalAmount", dBVLineAmount.ToString());

                            }

                            catch
                            {
                                dBVLineAmount = 0d;

                            }
                        }

                    }
                    else
                    {
                        try
                        {
                            dBVLineAmount = Convert.ToDouble(Convert.ToString(Synds.Tables[0].Rows[0]["SynSumAcc"]));
                            bug.SetPluginField(PLUGIN_ID, "Add_Fld3", dBVLineAmount.ToString());

                        }

                        catch
                        {
                            dBVLineAmount = 0d;

                        }
                    }

                }
            }

            ///////////////////// Total Amount for spreafast ///////////////////////////////////////

            if (bug.ixProject == 27)
            {
                bug.SetPluginField(PLUGIN_ID, "ixproject", bug.ixProject.ToString());
               // api.Notifications.AddAdminNotification("ixproject_SF", bug.ixProject.ToString());
                string newponum = "";

                double dBVLineAmount = 0;

                CSelectQuery QrySynlinesumAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                QrySynlinesumAmt.AddSelect("SUM(fAmount * fTax) as SynSumAcc");
                string sWhere1 = (api.Database.PluginTableName("CGSInvoiceItems_MLA")) + ".ixBug = " + bug.ixBug.ToString();

                sWhere1 += " and iDeleted = 0";
                QrySynlinesumAmt.AddWhere(sWhere1);


                if (bug.ixProject == 27)
                {
                    CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    Qrynewpon.AddSelect("Add_Fld1");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    Qrynewpon.AddWhere(sWhere2);
                    object NewPo = Qrynewpon.GetScalarValue();
                    newponum = Convert.ToString(NewPo);
                }
               
                //  api.Notifications.AddAdminNotification("newponum1", newponum.ToString());

                DataSet Synds = QrySynlinesumAmt.GetDataSet();

                if (null != Synds.Tables && Synds.Tables.Count == 1 && Synds.Tables[0].Rows.Count == 1)
                {
                    //  newponum = (bug.GetPluginField(PLUGIN_ID, "Add_Fld1")).ToString();
                    //   api.Notifications.AddAdminNotification("newponum2", newponum.ToString());

                    if (newponum == null || newponum == "")
                    {
                        //  if (newponum != "")
                        {
                            try
                            {

                                dBVLineAmount = Convert.ToDouble(Convert.ToString(Synds.Tables[0].Rows[0]["SynSumAcc"]));
                                bug.SetPluginField(PLUGIN_ID, "Add_Fld2", dBVLineAmount.ToString());

                            }

                            catch
                            {
                                dBVLineAmount = 0d;

                            }
                        }

                    }
                    else
                    {
                        try
                        {
                            dBVLineAmount = Convert.ToDouble(Convert.ToString(Synds.Tables[0].Rows[0]["SynSumAcc"]));
                            bug.SetPluginField(PLUGIN_ID, "Add_Fld3", dBVLineAmount.ToString());

                        }

                        catch
                        {
                            dBVLineAmount = 0d;

                        }
                    }

                }
            }

           //////////////////////////////end//////////////////////////////////////////////////////////



            //if (bug.ixProject == 25 || bug.ixProject == 26)
            //{
            //    string newponum = "";

            //    double dBVLineAmount = 0;

            //     if (bug.ixProject == 25)
            //    {
            //        CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
            //        Qrynewpon.AddSelect("Add_Fld1");
            //        string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
            //        Qrynewpon.AddWhere(sWhere2);
            //        object NewPo = Qrynewpon.GetScalarValue();
            //        newponum = Convert.ToString(NewPo);
            //    }
            //    else
            //    {
            //        CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
            //        Qrynewpon.AddSelect("PONumberArt_A");
            //        string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
            //        Qrynewpon.AddWhere(sWhere2);
            //        object NewPo = Qrynewpon.GetScalarValue();
            //        newponum = Convert.ToString(NewPo);
            //    }

            //    if ((bug.ixStatus == 180) || (bug.ixStatus == 184))
            //    {
            //        double balanceamt = 0D;
            //        double Totalamt = 0D;
            //        double POEnteredamt = 0D;
            //        double balanceamt1 = 0D;

            //        if (newponum == null || newponum == "")
            //        {
            //            Totalamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "TotalAmount")).ToString());
            //            POEnteredamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "POAmt")).ToString());
            //            balanceamt = (Totalamt) - (POEnteredamt);
            //            bug.SetPluginField(PLUGIN_ID, "POBalanceAmt", balanceamt.ToString());
            //        }
            //        else
            //        {
            //            //  api.Notifications.AddAdminNotification("Add_Fld3", Totalamt.ToString());
            //            Totalamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "Add_Fld3")).ToString());
            //            api.Notifications.AddAdminNotification("Add_Fld3", Totalamt.ToString());
            //            POEnteredamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "POAmt")).ToString());
            //            balanceamt1 = (Totalamt) - (POEnteredamt);
            //            api.Notifications.AddAdminNotification("balanceamt1", balanceamt1.ToString());
            //            bug.SetPluginField(PLUGIN_ID, "POBalanceAmt", balanceamt1.ToString());
            //        }
            //    }
            //}


            //if ((bug.ixStatus == 180) || (bug.ixStatus == 184))
            //{
            //      double Totalamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "TotalAmount")).ToString());
            //      double POEnteredamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "POAmt")).ToString());
            //      double balanceamt = 0D;
            //      balanceamt = (Totalamt) - (POEnteredamt);
            //      bug.SetPluginField(PLUGIN_ID, "POBalanceAmt", balanceamt.ToString());
            //}




            if (bug.ixProject == 25 || bug.ixProject == 26)
            {
                if (bug.ixStatus == 180)   // 180 approved
                {
                    bug.ixStatus = 180;
                }
                else if (bug.ixStatus == 184)  // 184 reject
                {
                    bug.ixStatus = 184;
                }
                else
                {
                    string assignto = (bug.GetPluginField(PLUGIN_ID, "CWFUserAssign")).ToString().Trim();
                    //  api.Notifications.AddAdminNotification("assig", assignto.ToString());

                    string cate = (bug.GetPluginField(PLUGIN_ID, "CWFUsercate")).ToString().Trim();
                    //  api.Notifications.AddAdminNotification("cate", cate.ToString());

                    string UserStatus = (bug.GetPluginField(PLUGIN_ID, "CWFUserStatus")).ToString().Trim();
                    //   api.Notifications.AddAdminNotification("CWFUserStatus", UserStatus.ToString());
                    int pid = 0;
                    int status = 0;

                    CPersonQuery appr1 = api.Person.NewPersonQuery();
                    appr1.IgnorePermissions = true;
                    appr1.AddSelect("Person.ixPerson");
                    appr1.AddWhere(" Person.sFullName = " + "'" + assignto + "'"); ;

                    DataSet Dpers1 = appr1.GetDataSet();

                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                    {
                        pid = Convert.ToInt32(Dpers1.Tables[0].Rows[0]["ixPerson"]);
                        //  api.Notifications.AddAdminNotification("pid", pid.ToString());
                        bug.ixPersonAssignedTo = pid;
                    }

                    CStatusQuery status1 = api.Status.NewStatusQuery();
                    status1.IgnorePermissions = true;
                    status1.AddSelect("Status.ixStatus");
                    status1.AddWhere("Status.ixCategory = 23");
                    status1.AddWhere(" Status.sStatus = " + "'" + UserStatus + "'"); ;

                    DataSet Dstatus1 = status1.GetDataSet();

                    if (Dstatus1.Tables.Count > 0 && Dstatus1.Tables[0] != null && Dstatus1.Tables[0].Rows.Count > 0)
                    {
                        status = Convert.ToInt32(Dstatus1.Tables[0].Rows[0]["ixStatus"]);
                        //  api.Notifications.AddAdminNotification("status", status.ToString());
                        bug.ixStatus = status;
                        bug.ixCategory = 23;
                    }

                    //api.Notifications.AddAdminNotification("status2", bug.ixStatus.ToString());
                }


            }

            if (bug.ixProject == 27)
            {
                if (bug.ixStatus == 192)   // 180 approved
                {
                    bug.ixStatus = 192;
                }
                else if (bug.ixStatus == 193)  // 184 reject
                {
                    bug.ixStatus = 193;
                }
                else
                {
                    string assignto = (bug.GetPluginField(PLUGIN_ID, "CWFUserAssign")).ToString().Trim();
                    //  api.Notifications.AddAdminNotification("assig", assignto.ToString());

                    string cate = (bug.GetPluginField(PLUGIN_ID, "CWFUsercate")).ToString().Trim();
                    //  api.Notifications.AddAdminNotification("cate", cate.ToString());

                    string UserStatus = (bug.GetPluginField(PLUGIN_ID, "CWFUserStatus")).ToString().Trim();
                    //   api.Notifications.AddAdminNotification("CWFUserStatus", UserStatus.ToString());
                    int pid = 0;
                    int status = 0;

                    CPersonQuery appr1 = api.Person.NewPersonQuery();
                    appr1.IgnorePermissions = true;
                    appr1.AddSelect("Person.ixPerson");
                    appr1.AddWhere(" Person.sFullName = " + "'" + assignto + "'"); ;

                    DataSet Dpers1 = appr1.GetDataSet();

                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                    {
                        pid = Convert.ToInt32(Dpers1.Tables[0].Rows[0]["ixPerson"]);
                        //  api.Notifications.AddAdminNotification("pid", pid.ToString());
                        bug.ixPersonAssignedTo = pid;
                    }

                    CStatusQuery status1 = api.Status.NewStatusQuery();
                    status1.IgnorePermissions = true;
                    status1.AddSelect("Status.ixStatus");
                    status1.AddWhere("Status.ixCategory = 24");
                    status1.AddWhere(" Status.sStatus = " + "'" + UserStatus + "'"); ;

                    DataSet Dstatus1 = status1.GetDataSet();

                    if (Dstatus1.Tables.Count > 0 && Dstatus1.Tables[0] != null && Dstatus1.Tables[0].Rows.Count > 0)
                    {
                        status = Convert.ToInt32(Dstatus1.Tables[0].Rows[0]["ixStatus"]);
                        //  api.Notifications.AddAdminNotification("status", status.ToString());
                        bug.ixStatus = status;
                        bug.ixCategory = 24;
                    }

                    //api.Notifications.AddAdminNotification("status2", bug.ixStatus.ToString());
                }


            }



            //string Vendor_Name = "-";
            string mailsub = "", mailbody = "";
            int iperson = 0;
            // string Invoice_no = "-";
            int old_inv_bug = 0;
            string vendor_1 = "";
            string InvNo_1 = "";

            if (bug.ixProject == 14 || bug.ixProject == 19)
            {
                int i = 0;
                try
                {
                    vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                    InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();
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
                                CSelectQuery Dupcheck2 = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                                Dupcheck2.AddSelect("ixbug");
                                Dupcheck2.AddWhere("CWFVendor = " + "'" + vendor_1 + "'");
                                Dupcheck2.AddWhere("sInvoiceNumber = " + "'" + InvNo_1 + "'");
                                Dupcheck2.AddWhere("ixbug > " + bug.ixBug.ToString() + " OR ixbug < " + bug.ixBug.ToString());
                                DataSet d_1 = Dupcheck2.GetDataSet();

                                if (bug.ixProject == 14)
                                {
                                    if (null != d_1.Tables && d_1.Tables.Count == 1 && d_1.Tables[0].Rows.Count > 0)
                                    {
                                        old_inv_bug = Convert.ToInt32(d_1.Tables[0].Rows[0]["ixbug"]);

                                        this.api.Notifications.AddError("--------------------------------------------------------------------------");
                                        this.api.Notifications.AddError("***DUPLICATE BILL****");
                                        this.api.Notifications.AddMessage("It seems An Invoice is already existing for the same vendor with ( case Id " + old_inv_bug + " )");
                                        this.api.Notifications.AddMessage("Please verify again");
                                        this.api.Notifications.AddError("-------------------------------------------------------------------------");

                                        mailsub = "Duplicate Invoice for Bazaarvoice in AP Workflow";
                                        mailbody = "It seems same invoice number " + InvNo_1.Trim() + " is already existing for the vendor " + vendor_1.Trim();
                                        iperson = bug.ixPersonAssignedTo;
                                        mailsender("sham.m@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        mailsender("poornima.r@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        mailsender("hemalatha.m@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        // mailsender("sripad.k@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        i = 1;
                                    }


                                    {
                                        CSelectQuery Dupcheck3 = api.Database.NewSelectQuery("Plugin_37_CustomBugData");
                                        Dupcheck3.AddSelect("ixbug");
                                        Dupcheck3.AddWhere("vendorxnamey32 = " + "'" + vendor_1 + "'");
                                        Dupcheck3.AddWhere("invoicexnumbert26 = " + "'" + InvNo_1 + "'");
                                        Dupcheck3.AddWhere("ixbug > " + bug.ixBug.ToString() + " OR ixbug < " + bug.ixBug.ToString());
                                        DataSet d_3 = Dupcheck3.GetDataSet();

                                        if (null != d_3.Tables && d_3.Tables.Count == 1 && d_3.Tables[0].Rows.Count > 0)
                                        {
                                            old_inv_bug = Convert.ToInt32(d_3.Tables[0].Rows[0]["ixbug"]);

                                            this.api.Notifications.AddError("--------------------------------------------------------------------------");
                                            this.api.Notifications.AddError("***DUPLICATE BILL****");
                                            this.api.Notifications.AddMessage("It seems An Invoice is already existing for the same vendor with ( case Id " + old_inv_bug + " )");
                                            this.api.Notifications.AddMessage("Please verify again");
                                            this.api.Notifications.AddError("-------------------------------------------------------------------------");

                                            mailsub = "Duplicate Invoice for Bazaarvoice in AP Workflow";
                                            mailbody = "It seems same invoice number " + InvNo_1.Trim() + " is already existing for the vendor " + vendor_1.Trim();
                                            iperson = bug.ixPersonAssignedTo;
                                            mailsender("sham.m@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                            mailsender("poornima.r@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                            mailsender("hemalatha.m@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                            //  mailsender("sripad.k@conseroglobal.com", bug, mailsub, mailbody, iperson);

                                            i = 1;
                                        }
                                    }

                                }
                                else
                                {
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

                                        mailsub = "Duplicate Invoice for Cambridge in AP Workflow";
                                        mailbody = "It seems same invoice number " + InvNo_1 + " is already existing for the vendor " + vendor_1;
                                        iperson = bug.ixPersonAssignedTo;
                                        mailsender("Giridhara.Vedanthachar@PERKINELMER.COM", bug, mailsub, mailbody, iperson);
                                        // mailsender("sumangali.k@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        mailsender("poornima.r@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                        i = 1;

                                    }
                                }
                            }
                        }
                    }
                }

                catch
                { }
            }


            if (bug.ixProject == 14 || bug.ixProject == 19)
            {
                if (bug.ixProject == 14)
                {
                    // Terms
                    string InvoiceDate = (bug.GetPluginField(PLUGIN_ID, "sInvoiceDate")).ToString().Trim();
                    DateTime InvoiceDate1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sInvoiceDate"));
                    string Terms = (bug.GetPluginField(PLUGIN_ID, "CWFTerms")).ToString();

                    if (Terms.Trim() != "-")
                    {
                        if (Terms.Trim() != "Due On Receipt")
                        {
                            Terms = Terms.Substring(4);
                            DateTime duedate = InvoiceDate1.AddDays(Convert.ToInt32(Terms));
                            bug.SetPluginField(PLUGIN_ID, "sInvoiceDueDate", duedate);
                        }

                        else
                        {
                            bug.SetPluginField(PLUGIN_ID, "sInvoiceDueDate", InvoiceDate1);
                        }

                    }

                    //Acctdesc

                    CSelectQuery SqlAccDesc;
                    int Id = 0;
                    string AccntDescp = "";
                    try
                    {
                        AccntDescp = (bug.GetPluginField(PLUGIN_ID, "AccountDesc")).ToString();
                    }
                    catch
                    {
                        if (String.IsNullOrEmpty(AccntDescp))
                        {
                            SqlAccDesc = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                            SqlAccDesc.AddSelect("min(ixBugLineItem)");
                            SqlAccDesc.AddWhere("ixbug = " + bug.ixBug.ToString() + " AND iDeleted = 0");
                            object ItemID = SqlAccDesc.GetScalarValue();
                            Id = Convert.ToInt32(ItemID);

                            CSelectQuery AcctDept = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));
                            AcctDept.AddSelect("sAccount,sDepartment");
                            AcctDept.AddWhere("ixBugLineItem = " + Id.ToString());
                            DataSet dsDeptAcct = AcctDept.GetDataSet();
                            if (null != dsDeptAcct.Tables && dsDeptAcct.Tables.Count == 1 && dsDeptAcct.Tables[0].Rows.Count == 1)
                            {
                                string Acct = (Convert.ToString(dsDeptAcct.Tables[0].Rows[0]["sAccount"])).Trim();
                                string Dept = (Convert.ToString(dsDeptAcct.Tables[0].Rows[0]["sDepartment"])).Trim();
                                string ItemAcctDept = Acct + "," + Dept;
                                bug.SetPluginField(PLUGIN_ID, "AccountDesc", ItemAcctDept);
                            }
                        }
                    }
                }
            }

            //Expires



            if (bug.ixProject == 25)
            {

                string newponum = "";
               // if (bug.ixProject == 25)
              //  {
                    CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    Qrynewpon.AddSelect("Add_Fld1");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    Qrynewpon.AddWhere(sWhere2);
                    object NewPo = Qrynewpon.GetScalarValue();
                    newponum = Convert.ToString(NewPo);
   
               //  api.Notifications.AddAdminNotification("newponum2", newponum.ToString());
                DateTime today = System.DateTime.Today.Date;
              //  api.Notifications.AddAdminNotification("today", today.ToString());

                if (newponum == null || newponum == "")
                {
                  //  string InvoiceDate = (bug.GetPluginField(PLUGIN_ID, "DateString1")).ToString();
//api.Notifications.AddAdminNotification("InvoiceDate", InvoiceDate.ToString());
                   // string InvoiceDate = (bug.GetPluginField(PLUGIN_ID, "DateString1")).ToString().Trim();
                    DateTime InvoiceDate1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "DateString1"));
                   // api.Notifications.AddAdminNotification("InvoiceDate1", InvoiceDate1.ToString());
                    DateTime expire = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "DateString2"));
                   // api.Notifications.AddAdminNotification("expire", expire.ToString());
                    

                    if (expire == today)
                    {
                        int i = 90;
                        DateTime duedate = InvoiceDate1.AddDays(Convert.ToInt32(i));
                        // InvoiceDate = Convert.ToString(duedate);
                      //  api.Notifications.AddAdminNotification("duedate", duedate.ToString());
                        bug.SetPluginField(PLUGIN_ID, "DateString2", duedate.ToString("MM/dd/yyyy"));
                    }

                    else
                    {
                        //do nothing
                    }
                }
                else
                {
                    string InvoiceDate = (bug.GetPluginField(PLUGIN_ID, "Add_Fld5")).ToString().Trim();
                    DateTime InvoiceDate1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "Add_Fld5"));
                  //  api.Notifications.AddAdminNotification("InvoiceDate2", InvoiceDate.ToString());
                    DateTime expire1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "Add_Fld6"));
                   // api.Notifications.AddAdminNotification("expire1", expire1.ToString());


                    if (expire1 == today)
                    {
                        int i = 90;
                        DateTime duedate = InvoiceDate1.AddDays(Convert.ToInt32(i));
                     //   api.Notifications.AddAdminNotification("duedate", duedate.ToString());
                        // InvoiceDate = Convert.ToString(duedate);
                        bug.SetPluginField(PLUGIN_ID, "DateString2", duedate.ToString("MM/dd/yyyy"));
                    }

                    else
                    {
                        //do nothing
                    }
                }

            }

            if (bug.ixProject == 26)
            {
              //  api.Notifications.AddAdminNotification("ixProject", bug.ixProject.ToString());
                string newponum = "";
                
                    CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    Qrynewpon.AddSelect("PONumberArt_A");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    Qrynewpon.AddWhere(sWhere2);
                    object NewPo = Qrynewpon.GetScalarValue();
                    newponum = Convert.ToString(NewPo);
              

                //  api.Notifications.AddAdminNotification("newponum2", newponum.ToString());
                DateTime today = System.DateTime.Today.Date;
              //  api.Notifications.AddAdminNotification("today", today.ToString());

                if (newponum == null || newponum == "")
                {
                    //string InvoiceDate = (bug.GetPluginField(PLUGIN_ID, "DateString1")).ToString();
                  //  api.Notifications.AddAdminNotification("InvoiceDate", InvoiceDate.ToString());
                    // string InvoiceDate = (bug.GetPluginField(PLUGIN_ID, "DateString1")).ToString().Trim();
                    DateTime InvoiceDate1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "DateString1"));
                  //  api.Notifications.AddAdminNotification("InvoiceDate1", InvoiceDate1.ToString());
                    DateTime expire = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "DateString2"));
                 //   api.Notifications.AddAdminNotification("expire", expire.ToString());


                    if (expire == today)
                    {
                        int i = 90;
                        DateTime duedate = InvoiceDate1.AddDays(Convert.ToInt32(i));
                        // InvoiceDate = Convert.ToString(duedate);
                   //     api.Notifications.AddAdminNotification("duedate", duedate.ToString());
                        bug.SetPluginField(PLUGIN_ID, "DateString2", duedate.ToString("MM/dd/yyyy"));
                    }

                    else
                    {
                        //do nothing
                    }
                }
                else
                {
                    string InvoiceDate = (bug.GetPluginField(PLUGIN_ID, "Add_Fld5")).ToString().Trim();
                    DateTime InvoiceDate1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "Add_Fld5"));
                  //  api.Notifications.AddAdminNotification("InvoiceDate2", InvoiceDate.ToString());
                    DateTime expire1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "Add_Fld6"));
                  //  api.Notifications.AddAdminNotification("expire1", expire1.ToString());


                    if (expire1 == today)
                    {
                        int i = 90;
                        DateTime duedate = InvoiceDate1.AddDays(Convert.ToInt32(i));
                        //api.Notifications.AddAdminNotification("duedate", duedate.ToString());
                        // InvoiceDate = Convert.ToString(duedate);
                        bug.SetPluginField(PLUGIN_ID, "DateString2", duedate.ToString("MM/dd/yyyy"));
                    }

                    else
                    {
                        //do nothing
                    }
                }

            }

            if (bug.ixProject == 27)
            {
               // api.Notifications.AddAdminNotification("ixProject", bug.ixProject.ToString());
                string newponum = "";

                CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                Qrynewpon.AddSelect("Add_Fld1");
                string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                Qrynewpon.AddWhere(sWhere2);
                object NewPo = Qrynewpon.GetScalarValue();
                newponum = Convert.ToString(NewPo);


                //  api.Notifications.AddAdminNotification("newponum2", newponum.ToString());
                DateTime today = System.DateTime.Today.Date;
               // api.Notifications.AddAdminNotification("today", today.ToString());

                if (newponum == null || newponum == "")
                {

                    DateTime InvoiceDate1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "DateString1"));
                    //api.Notifications.AddAdminNotification("InvoiceDate1", InvoiceDate1.ToString());
                    DateTime expire = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "DateString2"));
                    //api.Notifications.AddAdminNotification("expire", expire.ToString());


                    if (expire == today)
                    {
                        int i = 90;
                        DateTime duedate = InvoiceDate1.AddDays(Convert.ToInt32(i));
                        // InvoiceDate = Convert.ToString(duedate);
                       // api.Notifications.AddAdminNotification("duedate", duedate.ToString());
                        bug.SetPluginField(PLUGIN_ID, "DateString2", duedate.ToString("MM/dd/yyyy"));
                    }

                    else
                    {
                        //do nothing
                    }
                }
                else
                {
                    string InvoiceDate = (bug.GetPluginField(PLUGIN_ID, "Add_Fld5")).ToString().Trim();
                    DateTime InvoiceDate1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "Add_Fld5"));
                   // api.Notifications.AddAdminNotification("InvoiceDate2", InvoiceDate.ToString());
                    DateTime expire1 = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "Add_Fld6"));
                   // api.Notifications.AddAdminNotification("expire1", expire1.ToString());


                    if (expire1 == today)
                    {
                        int i = 90;
                        DateTime duedate = InvoiceDate1.AddDays(Convert.ToInt32(i));
                      //  api.Notifications.AddAdminNotification("duedate", duedate.ToString());
                        // InvoiceDate = Convert.ToString(duedate);
                        bug.SetPluginField(PLUGIN_ID, "DateString2", duedate.ToString("MM/dd/yyyy"));
                    }

                    else
                    {
                        //do nothing
                    }
                }

            }

            string UStatus = "";

            if (bug.ixProject == 25)
            {
                string pon = "";
                CSelectQuery Qrynewpon1 = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                Qrynewpon1.AddSelect("PONumber");
                string sWhere1 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                Qrynewpon1.AddWhere(sWhere1);
                object NewPo1 = Qrynewpon1.GetScalarValue();
                pon = Convert.ToString(NewPo1);

                if (UStatus == "Rejected")
                {
                    bug.ixStatus = 184;
                }
                if (bug.ixStatus == 180 && pon !="")
                {
                    string newponum = "";

                    //double dBVLineAmount = 0;

                    CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                    Qrynewpon.AddSelect("Add_Fld1");
                    string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                    Qrynewpon.AddWhere(sWhere2);
                    object NewPo = Qrynewpon.GetScalarValue();
                    newponum = Convert.ToString(NewPo);


                    double balanceamt = 0D;
                    double Totalamt = 0D;
                    double POEnteredamt = 0D;
                    double balanceamt1 = 0D;

                    if (newponum == null || newponum == "")
                    {
                        Totalamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "TotalAmount")).ToString());
                        POEnteredamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "POAmt")).ToString());
                        balanceamt = (Totalamt) - (POEnteredamt);
                        bug.SetPluginField(PLUGIN_ID, "POBalanceAmt", balanceamt.ToString());
                    }
                    else
                    {
                        //  api.Notifications.AddAdminNotification("Add_Fld3", Totalamt.ToString());
                        Totalamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "Add_Fld3")).ToString());
                       // api.Notifications.AddAdminNotification("Add_Fld3", Totalamt.ToString());
                        POEnteredamt = Convert.ToDouble((bug.GetPluginField(PLUGIN_ID, "POAmt")).ToString());
                        balanceamt1 = (Totalamt) - (POEnteredamt);
                       // api.Notifications.AddAdminNotification("balanceamt1", balanceamt1.ToString());
                        bug.SetPluginField(PLUGIN_ID, "POBalanceAmt", balanceamt1.ToString());
                    }
                }
            }
        

            //validation

            //if (bug.ixProject == 25)
            //{
            //    string Location = "";
            //    string Dept = "";
            //    string vendor = "";
            //    string add = "";
            //    string state = "";

            //    Location = (bug.GetPluginField(PLUGIN_ID, "CWFLocation")).ToString().Trim();

            //    if (Location == "-")
            //    {
            //        this.api.Notifications.AddMessage("Please select the LocationId");
            //        bug.ixStatus = 185;
            //    }
            //    Dept = (bug.GetPluginField(PLUGIN_ID, "CWFDept")).ToString().Trim();
            //    if (Dept == "-")
            //    {
            //        this.api.Notifications.AddMessage("Please select the Deparment");
            //        bug.ixStatus = 185;
            //    }
            //    vendor = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
            //    if (vendor == null || vendor=="")
            //    {
            //        this.api.Notifications.AddMessage("Please enter the Vendor Name");
            //        bug.ixStatus = 185;
            //    }
            //    add = (bug.GetPluginField(PLUGIN_ID, "CWFCustomVal2")).ToString().Trim();
            //    if (add == null || add == "")
            //    {
            //        this.api.Notifications.AddMessage("Please enter the Address");
            //        bug.ixStatus = 185;
            //    }
            //    state = (bug.GetPluginField(PLUGIN_ID, "CWFCustomVal3")).ToString().Trim();
            //    if (state == null || state == "")
            //    {
            //        this.api.Notifications.AddMessage("Please enter the State");
            //        bug.ixStatus = 185;
            //    }
            //}

            //------------------------------------------------Ford Direct Workflow begin----------------------------------
            # region Bazaarvoice for MLA workflow begin


            {

                //capturing  and storing atlevel and levels

                if (bug.ixProject == 14)
                {

                    if (nBugAction == BugAction.Edit || nBugAction == BugAction.Assign)
                    {

                        if (bug.ixStatus == 72)
                        {

                            // this.api.Notifications.AddMessage("BCE-1");

                            string sL1e = "-";
                            string sL2e = "-";
                            string sL3e = "-";
                            string sL4e = "-";
                            int atlevel = 0;


                            string Assignedto = "";

                            CSelectQuery approvers = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                            approvers.AddSelect("CWFApproverl1,CWFApproverl2,CWFApproverl3,CWFApproverl4");
                            approvers.AddWhere("ixBug = " + bug.ixBug.ToString());

                            DataSet ds_1 = approvers.GetDataSet();

                            if (null != ds_1.Tables && ds_1.Tables.Count == 1 && ds_1.Tables[0].Rows.Count == 1)
                            {

                                sL1e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                                sL2e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                                sL3e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                                sL4e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                                //  this.api.Notifications.AddMessage(sL1e_1 + "||" + sL1e );

                            }

                            //checking Approval sequence

                            if (sL1e == "-" && sL2e == "-" && sL3e == "-" && sL4e == "-")
                            {


                                this.api.Notifications.AddMessage(sL1e + "|" + sL2e + "|" + sL3e + "|" + sL4e);
                                this.api.Notifications.AddMessage("Please set atleast one approver in the approavl sequence ");
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                //   bug.ixStatus = 102;
                                //change by poornima
                                // bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                bug.ixStatus = 126;
                                return;
                            }


                            if ((sL4e != "-" && sL3e == "-") || (sL3e != "-" && sL2e == "-") || (sL2e != "-" && sL1e == "-"))
                            {


                                this.api.Notifications.AddMessage("Please set the approval sequence properly ");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 126;


                                return;
                            }


                            if ((sL1e != "-" && (sL1e == sL2e || sL1e == sL3e || sL1e == sL4e)) ||
                                (sL2e != "-" && (sL2e == sL3e || sL1e == sL4e)) ||
                                (sL3e != "-" && (sL3e == sL4e)))
                            {


                                this.api.Notifications.AddMessage("Improper approval sequence- make sure no approvers are repeated in the sequence");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 126;
                                return;
                            }



                            // finding assigned to person
                            {

                                CPersonQuery appr1 = api.Person.NewPersonQuery();
                                appr1.IgnorePermissions = true;
                                appr1.AddSelect("Person.sFullName");
                                appr1.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo);

                                DataSet Dpers1 = appr1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    Assignedto = Convert.ToString(Dpers1.Tables[0].Rows[0]["sFullName"]);
                                }

                            }

                            //at levels
                            {

                                if (sL1e == Assignedto)
                                {
                                    atlevel = 1;
                                }

                                else if (sL2e == Assignedto)
                                {
                                    atlevel = 2;
                                }

                                else if (sL3e == Assignedto)
                                {
                                    atlevel = 3;
                                }

                                else if (sL4e == Assignedto)
                                {
                                    atlevel = 4;
                                }

                                else if ((sL1e != Assignedto) || (sL2e != Assignedto) || (sL3e != Assignedto) || (sL4e != Assignedto))
                                {
                                    this.api.Notifications.AddMessage(" The'Assigned to' person must be one of in approval sequesnce ");
                                    bug.ixStatus = 126;
                                    return;
                                }


                                // updating atlevel
                                string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", atlevel);

                            } //updating atlevel ends here


                            // Duplicate invoice checks
                            {
                                int i = 0;

                                ////string Vendor_Name = "-";
                                //string mailsub = "", mailbody = "";
                                //int iperson = 0;
                                //// string Invoice_no = "-";
                                //int old_inv_bug = 0;


                                // dates validation

                                {
                                    try
                                    {
                                        string Invdate = (bug.GetPluginField(PLUGIN_ID, "sInvoiceDate")).ToString().Trim();
                                        string Inventdate = (bug.GetPluginField(PLUGIN_ID, "sInvoiceEnteredDate")).ToString().Trim();

                                        if (Invdate == null || Invdate == "")
                                        {
                                            this.api.Notifications.AddError("'Invoice date' must be entered");
                                            i = 1;
                                            //bug.ixStatus = 98;
                                            // return;
                                        }

                                        if (Inventdate == null || Inventdate == "")
                                        {
                                            this.api.Notifications.AddError("'Invoice enetred date' must be entered");
                                            i = 1;
                                            // bug.ixStatus = 98;
                                            // return;
                                        }

                                    }


                                    catch
                                    {
                                        // let it go
                                    }

                                }




                                if (i == 1)
                                {
                                    bug.ixStatus = 126;
                                    return;
                                }
                                {
                                    //this.api.Notifications.AddMessage("renaming file");

                                    // Rename = 1;
                                }

                                {
                                    string vendor_3 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                                    string InvNo_3 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();

                                    CPersonQuery pers = api.Person.NewPersonQuery();
                                    pers.IgnorePermissions = true;
                                    pers.AddSelect("*");
                                    pers.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo.ToString());

                                    DataSet Dpers = pers.GetDataSet();

                                    if (Dpers.Tables.Count > 0 && Dpers.Tables[0] != null && Dpers.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers.Tables[0].Rows[0]["sEmail"]);

                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' awaiting your approval";
                                        mailsub = "An Invoice is awaiting your approval for vendor:" + vendor_3 + " Invoice:" + InvNo_3;
                                        mailbody = "There is an invoice requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        iperson = bug.ixPersonAssignedTo;
                                        mailsender(semail1, bug, mailsub, mailbody, iperson);
                                        MailBody = mailbody;
                                        MailSub = mailsub;
                                        ixperson = iperson;
                                        flag = 1;

                                        //if ((bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")) != null)
                                        //{
                                        //    if (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013").ToString() != "")
                                        //    {

                                        //        string CCemail = (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")).ToString();
                                        //        mailsender(CCemail.Trim(), bug, mailsub, mailbody, iperson);
                                        //        this.api.Notifications.AddMessage("A CCEmail has been sent Successfully");
                                        //    }
                                        //}
                                        this.api.Notifications.AddMessage("An email has been sent to the approver successfully");


                                    }

                                }

                                {

                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.ixPerson = " + bug.ixPersonLastEditedBy.ToString());

                                    DataSet Dpers1 = pers1.GetDataSet();

                                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' has sent for Approval";
                                        mailsub = "An Invoice is sent for Approval";
                                        mailbody = "There is an invoice Sent for Approval.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;
                                        iperson = bug.ixPersonLastEditedBy;
                                        mailsender(semail1, bug, mailsub, mailbody, iperson);

                                    }

                                }

                            } // sending emails ends here


                        }
                    }

                    if (nBugAction == BugAction.Resolve)
                    {
                        iperson = 0;
                        // string Vendor_Name1 = "";
                        // string Invoice1 = "";

                        int L = 0;
                        int L_openr = 0;
                        int L0 = 0;
                        int L1 = 0;
                        int L2 = 0;
                        int L3 = 0;
                        int L4 = 0;

                        string sL1 = "-";
                        string sL2 = "-";
                        string sL3 = "-";
                        string sL4 = "-";

                        string Lmail = "-";
                        //  string L0mail = "-";
                        string L1mail = "-";
                        string L2mail = "-";
                        string L3mail = "-";
                        string L4mail = "-";


                        if (bug.ixProject != 14)
                        {
                            return;

                        }


                        // fetching approvers details

                        // L = bug.ixPersonLastEditedBy;

                        L = bug.ixPersonOpenedBy;
                        L_openr = bug.ixPersonOpenedBy;

                        //  this.api.Notifications.AddMessage("L||" + L);

                        L0 = bug.ixPersonResolvedBy;
                        {
                            CPersonQuery intL1 = api.Person.NewPersonQuery();
                            intL1.IgnorePermissions = true;
                            intL1.AddSelect("sEmail");
                            intL1.AddWhere(" Person.ixPerson = " + L_openr);

                            DataSet Dpers1 = intL1.GetDataSet();

                            if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                            {
                                //L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                Lmail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                            }

                        }
                        sL1 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                        sL2 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                        sL3 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                        sL4 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                        {



                            // fetching L1 approver deatils
                            if (sL1 != "-")
                            {
                                CPersonQuery intL1 = api.Person.NewPersonQuery();
                                intL1.IgnorePermissions = true;
                                intL1.AddSelect("Person.ixPerson,Person.sEmail");
                                intL1.AddWhere(" Person.sFullName = " + "'" + sL1 + "'");

                                DataSet Dpers1 = intL1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    L1 = Convert.ToInt32(Dpers1.Tables[0].Rows[0]["ixPerson"]);
                                    L1mail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L2 approver deatils



                            if (sL2 != "-")
                            {
                                CPersonQuery intL2 = api.Person.NewPersonQuery();
                                intL2.IgnorePermissions = true;
                                intL2.AddSelect("Person.ixPerson,Person.sEmail");
                                intL2.AddWhere(" Person.sFullName = " + "'" + sL2 + "'");

                                DataSet Dpers2 = intL2.GetDataSet();

                                if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                {
                                    L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                    L2mail = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }
                            // fetching L3 approver deatils
                            if (sL3 != "-")
                            {
                                CPersonQuery intL3 = api.Person.NewPersonQuery();
                                intL3.IgnorePermissions = true;
                                intL3.AddSelect("Person.ixPerson,Person.sEmail");
                                intL3.AddWhere(" Person.sFullName = " + "'" + sL3 + "'");

                                DataSet Dpers3 = intL3.GetDataSet();

                                if (Dpers3.Tables.Count > 0 && Dpers3.Tables[0] != null && Dpers3.Tables[0].Rows.Count > 0)
                                {
                                    L3 = Convert.ToInt32(Dpers3.Tables[0].Rows[0]["ixPerson"]);
                                    L3mail = Convert.ToString(Dpers3.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L4 approver deatils
                            if (sL4 != "-")
                            {
                                CPersonQuery intL4 = api.Person.NewPersonQuery();
                                intL4.IgnorePermissions = true;
                                intL4.AddSelect("Person.ixPerson,Person.sEmail");
                                intL4.AddWhere(" Person.sFullName = " + "'" + sL4 + "'");

                                DataSet Dpers4 = intL4.GetDataSet();

                                if (Dpers4.Tables.Count > 0 && Dpers4.Tables[0] != null && Dpers4.Tables[0].Rows.Count > 0)
                                {
                                    L4 = Convert.ToInt32(Dpers4.Tables[0].Rows[0]["ixPerson"]);
                                    L4mail = Convert.ToString(Dpers4.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            //  this.api.Notifications.AddMessage(L1 + "|" + sL2 + L2 + "|" + sL3 + L3 + "|" + sL4 + L4);
                        }


                        // this.api.Notifications.AddMessage("A1");
                        if (bug.ixStatus == 70)
                        {
                            // this.api.Notifications.AddMessage("resolve-1");

                            vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                            InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();

                            string Appr_mailsub = "An Invoice is awaiting your approval for vendor:" + vendor_1 + " Invoice:" + InvNo_1;
                            string Appr_mailbody = "There is an invoice requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;

                            string Proc_mailsub = "An Invoice has been Approved";
                            string Proc_mailbody = "There is an invoice which has been Approved.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;

                            // this.api.Notifications.AddMessage("1");

                            /*
                            if (sL1 == "-" && sL2 == "-" && sL3 == "-" && sL4 == "-")
                            {
                                this.api.Notifications.AddMessage("2");
                                this.api.Notifications.AddMessage("An email has been sent on Invoice Approved status to the requestor");
                                // mailsub = "Invoice '" + Vendname + "-" + invoiceno + "'  has been Approved";
                                //   mailsub = "An Invoice has been Approved";
                                //  mailbody = "There is an invoice which has been Approved.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;
                                          string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                                          CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                          Update1_1.UpdateInt("ixAtlevel", 5);
                        
                                mailsender(L3mail, bug, Proc_mailsub, Proc_mailbody, iperson);
                            }
                            */




                            if (sL1 != "-")
                            {
                                // this.api.Notifications.AddMessage("resolve-2");
                                if (L1 == L0)
                                {
                                    // this.api.Notifications.AddMessage("resolve-3");
                                    if (sL2 != "-")
                                    {
                                        // this.api.Notifications.AddMessage("resolve-4");
                                        // this.api.Notifications.AddMessage("L1 level");
                                        //this.api.Notifications.AddMessage("5");
                                        this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                        bug.ixPersonAssignedTo = L2;
                                        bug.ixStatus = 72;
                                        //updating atlevel
                                        //   string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 3);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                        mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                        //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                    }
                                    else
                                    {
                                        // this.api.Notifications.AddMessage("resolve-5");
                                        //  this.api.Notifications.AddMessage("L2_1 level");
                                        //  this.api.Notifications.AddMessage("31");
                                        this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requestor");
                                        //  string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 5);

                                        // this.api.Notifications.AddMessage("assgined to email||" + Lmail);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                        //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                        //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                        bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                        //  this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                    }
                                }




                                else if (sL2 != "-")
                                {
                                    //this.api.Notifications.AddMessage("3");

                                    if (L2 == L0)
                                    {
                                        // this.api.Notifications.AddMessage("4");
                                        if (sL3 != "-")
                                        {
                                            // this.api.Notifications.AddMessage("L2 level");
                                            //this.api.Notifications.AddMessage("5");
                                            this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                            bug.ixPersonAssignedTo = L3;
                                            bug.ixStatus = 72;
                                            //updating atlevel
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                            mailsender(L3mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                            //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                        }
                                        else
                                        {
                                            //  this.api.Notifications.AddMessage("L2_1 level");
                                            //  this.api.Notifications.AddMessage("9");
                                            this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requestor");
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            // Update1_1.UpdateInt("ixAtlevel", 5);

                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                            // this.api.Notifications.AddMessage("ixPersonLastEditedBy||" + bug.ixPersonLastEditedBy);
                                            // this.api.Notifications.AddMessage("assgined to email||" + Lmail);
                                            //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                            // bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                            bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                            // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                        }
                                    }



                                    else if (sL3 != "-")
                                    {
                                        if (L3 == L0)
                                        {
                                            // this.api.Notifications.AddMessage("L3 level");
                                            if (sL4 != "-")
                                            {
                                                this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                                //  this.api.Notifications.AddMessage("SL4|" + sL4);
                                                // this.api.Notifications.AddMessage("L4|" + L4);
                                                bug.ixPersonAssignedTo = L4;
                                                bug.ixStatus = 72;

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //Update1_1.UpdateInt("ixAtlevel", 4);
                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                                mailsender(L4mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                                //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                            }
                                            else
                                            {
                                                //  this.api.Notifications.AddMessage("L3_1 level");
                                                // this.api.Notifications.AddMessage("8");
                                                this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                // Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                            }
                                        }


                                        else if (sL4 != "-")
                                        {

                                            if (L4 == L0)
                                            {
                                                // this.api.Notifications.AddMessage("L4 level");

                                                // this.api.Notifications.AddMessage("9");
                                                this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //  Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //  bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);

                                            }
                                        }
                                    }
                                }
                            }

                        }


                        // For Rejection 

                        if (bug.ixStatus == 73)
                        {


                            if (bugevent.s == "")
                            {

                                this.api.Notifications.AddMessage("                                                                                                               ");
                                this.api.Notifications.AddMessage("                                                                                                             ");
                                this.api.Notifications.AddMessage("                                                                                                              ");
                                this.api.Notifications.AddMessage("                                                                                                            ");
                                this.api.Notifications.AddError("                                                       -                                      ");
                                this.api.Notifications.AddError("-----------------------------------Error Message------------------------------");
                                this.api.Notifications.AddError("You cannot reject a case without any reason");
                                this.api.Notifications.AddError("Please reject the case again with an appropraite reason entered in the comment box");

                                this.api.Notifications.AddMessage("--------------------------------------------------------------------------------------");

                                this.api.Notifications.AddMessage("----------------------------------   -SOLUTION-  -------------------------------------");
                                this.api.Notifications.AddMessage("                     To Reject the case");
                                this.api.Notifications.AddMessage("                     Click on resolve button");
                                this.api.Notifications.AddMessage("                     Set the status to 'Rejected' ");
                                this.api.Notifications.AddMessage("                     Enter your reasons of rejections in the comment box");
                                this.api.Notifications.AddMessage("                     Click 'Resolve' Button");
                                this.api.Notifications.AddMessage("-----------------------------------------------------------------------------");
                                this.api.Notifications.AddError("-Error--------Error---------End of Error Message----------Error----------Error-");
                                bug.ixPersonAssignedTo = bugevent.ixPerson;
                                //bugevent.ixPersonAssignedTo = bugevent.ixPerson;
                                bug.ixPersonAssignedTo = bug.ixPersonResolvedBy;
                                bug.ixStatus = 72;
                                return;
                            }




                            //Finding the level of rejection and sendin email accordingly
                            //string RL0mail = "-";
                            string RL1mail = "-";
                            // string RL2mail = "-";
                            //string RL3mail = "-";
                            // string RL4mail = "-";


                            string Rej_mailsub = "An Invoice has been Rejected";
                            string Rej_mailbody = "There is an invoice which has been.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;


                            {
                                CPersonQuery rejL1 = api.Person.NewPersonQuery();
                                rejL1.IgnorePermissions = true;
                                rejL1.AddSelect("sEmail");
                                rejL1.AddWhere(" Person.ixPerson = " + bug.ixPersonOpenedBy);

                                DataSet Dpers5 = rejL1.GetDataSet();

                                if (Dpers5.Tables.Count > 0 && Dpers5.Tables[0] != null && Dpers5.Tables[0].Rows.Count > 0)
                                {
                                    //L4 = Convert.ToInt32(Dpers5.Tables[0].Rows[0]["ixPerson"]);
                                    RL1mail = Convert.ToString(Dpers5.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                                mailsender(RL1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //  this.api.Notifications.AddMessage("processor | " + RL1mail);
                            }



                            if (L0 == L1)
                            {
                                // api.Notifications.AddMessage("executed at L1 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 1);


                                // mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //   mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //this.api.Notifications.AddMessage("Second assignee| " + L1mail);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requestor");

                            }

                            if (L0 == L2)
                            {
                                //api.Notifications.AddMessage("executed at L2 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requestor");


                            }


                            else if (L0 == L3)
                            {
                                // api.Notifications.AddMessage("executed at L3 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                //   mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requestor");
                            }



                            else if (L0 == L4)
                            {
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                //   api.Notifications.AddMessage("executed at L4 level");
                                // this.api.Notifications.AddMessage("Fourth Approver| " + L1mail + "||" + L2mail + "||" + L3mail);
                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requestor");

                            }






                        }

                    }

                }

            }
            #endregion

            # region Cambridge for MLA workflow begin


            {

                //capturing  and storing atlevel and levels

                if (bug.ixProject == 19)
                {

                    if (nBugAction == BugAction.Edit || nBugAction == BugAction.Assign)
                    {

                        if (bug.ixStatus == 145)
                        {

                            // this.api.Notifications.AddMessage("BCE-1");

                            string sL1e = "-";
                            string sL2e = "-";
                            string sL3e = "-";
                            string sL4e = "-";
                            int atlevel = 0;


                            string Assignedto = "";

                            CSelectQuery approvers = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                            approvers.AddSelect("CWFApproverl1,CWFApproverl2,CWFApproverl3,CWFApproverl4");
                            approvers.AddWhere("ixBug = " + bug.ixBug.ToString());

                            DataSet ds_1 = approvers.GetDataSet();

                            if (null != ds_1.Tables && ds_1.Tables.Count == 1 && ds_1.Tables[0].Rows.Count == 1)
                            {

                                sL1e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                                sL2e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                                sL3e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                                sL4e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                                //  this.api.Notifications.AddMessage(sL1e_1 + "||" + sL1e );

                            }

                            //checking Approval sequence

                            if (sL1e == "-" && sL2e == "-" && sL3e == "-" && sL4e == "-")
                            {


                                this.api.Notifications.AddMessage(sL1e + "|" + sL2e + "|" + sL3e + "|" + sL4e);
                                this.api.Notifications.AddMessage("Please set atleast one approver in the approavl sequence ");
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                //   bug.ixStatus = 102;
                                //change by poornima
                                // bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                bug.ixStatus = 147;
                                return;
                            }


                            if ((sL4e != "-" && sL3e == "-") || (sL3e != "-" && sL2e == "-") || (sL2e != "-" && sL1e == "-"))
                            {


                                this.api.Notifications.AddMessage("Please set the approval sequence properly ");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 147;


                                return;
                            }


                            if ((sL1e != "-" && (sL1e == sL2e || sL1e == sL3e || sL1e == sL4e)) ||
                                (sL2e != "-" && (sL2e == sL3e || sL1e == sL4e)) ||
                                (sL3e != "-" && (sL3e == sL4e)))
                            {


                                this.api.Notifications.AddMessage("Improper approval sequence- make sure no approvers are repeated in the sequence");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 147;
                                return;
                            }



                            // finding assigned to person
                            {

                                CPersonQuery appr1 = api.Person.NewPersonQuery();
                                appr1.IgnorePermissions = true;
                                appr1.AddSelect("Person.sFullName");
                                appr1.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo);

                                DataSet Dpers1 = appr1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    Assignedto = Convert.ToString(Dpers1.Tables[0].Rows[0]["sFullName"]);
                                }

                            }

                            //at levels
                            {

                                if (sL1e == Assignedto)
                                {
                                    atlevel = 1;
                                }

                                else if (sL2e == Assignedto)
                                {
                                    atlevel = 2;
                                }

                                else if (sL3e == Assignedto)
                                {
                                    atlevel = 3;
                                }

                                else if (sL4e == Assignedto)
                                {
                                    atlevel = 4;
                                }

                                else if ((sL1e != Assignedto) || (sL2e != Assignedto) || (sL3e != Assignedto) || (sL4e != Assignedto))
                                {
                                    this.api.Notifications.AddMessage(" The'Assigned to' person must be one of in approval sequesnce ");
                                    bug.ixStatus = 147;
                                    return;
                                }


                                // updating atlevel
                                string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", atlevel);

                            } //updating atlevel ends here


                            // Duplicate invoice checks
                            {
                                int i = 0;

                                //string Vendor_Name = "-";
                                //string mailsub = "", mailbody = "";
                                //int iperson = 0;
                                //// string Invoice_no = "-";
                                //int old_inv_bug = 0;


                                // dates validation

                                {
                                    try
                                    {
                                        string Invdate = (bug.GetPluginField(PLUGIN_ID, "sInvoiceDate")).ToString().Trim();
                                        string Inventdate = (bug.GetPluginField(PLUGIN_ID, "sInvoiceEnteredDate")).ToString().Trim();

                                        if (Invdate == null || Invdate == "")
                                        {
                                            this.api.Notifications.AddError("'Invoice date' must be entered");
                                            i = 1;
                                            //bug.ixStatus = 98;
                                            // return;
                                        }

                                        if (Inventdate == null || Inventdate == "")
                                        {
                                            this.api.Notifications.AddError("'Invoice enetred date' must be entered");
                                            i = 1;
                                            // bug.ixStatus = 98;
                                            // return;
                                        }

                                    }


                                    catch
                                    {
                                        // let it go
                                    }

                                }




                                if (i == 1)
                                {
                                    bug.ixStatus = 147;
                                    return;
                                }
                                {
                                    //this.api.Notifications.AddMessage("renaming file");

                                    // Rename = 1;
                                }

                                {
                                    string vendor_3 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                                    string InvNo_3 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();

                                    CPersonQuery pers = api.Person.NewPersonQuery();
                                    pers.IgnorePermissions = true;
                                    pers.AddSelect("*");
                                    pers.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo.ToString());

                                    DataSet Dpers = pers.GetDataSet();

                                    if (Dpers.Tables.Count > 0 && Dpers.Tables[0] != null && Dpers.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers.Tables[0].Rows[0]["sEmail"]);

                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' awaiting your approval";
                                        mailsub = "An Invoice is awaiting your approval for vendor:" + vendor_3 + " Invoice:" + InvNo_3;
                                        mailbody = "There is an invoice requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        iperson = bug.ixPersonAssignedTo;
                                        mailsender(semail1, bug, mailsub, mailbody, iperson);

                                        if ((bug.GetPluginField("Plugin_37_CustomBugData", "emailxccx013")) != null)
                                        {
                                            if (bug.GetPluginField("Plugin_37_CustomBugData", "emailxccx013").ToString() != "")
                                            {
                                                string CCemail = (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")).ToString();
                                                mailsender(CCemail.Trim(), bug, mailsub, mailbody, iperson);
                                                this.api.Notifications.AddMessage("A CCEmail has been sent Successfully");
                                            }
                                        }
                                        this.api.Notifications.AddMessage("An email has been sent to the approver successfully");


                                    }

                                }

                                {

                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.ixPerson = " + bug.ixPersonLastEditedBy.ToString());

                                    DataSet Dpers1 = pers1.GetDataSet();

                                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' has sent for Approval";
                                        mailsub = "An Invoice is sent for Approval";
                                        mailbody = "There is an invoice Sent for Approval.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;
                                        iperson = bug.ixPersonLastEditedBy;
                                        mailsender(semail1, bug, mailsub, mailbody, iperson);

                                    }

                                }

                            } // sending emails ends here


                        }
                    }

                    if (nBugAction == BugAction.Resolve)
                    {
                        iperson = 0;
                        // string Vendor_Name1 = "";
                        // string Invoice1 = "";

                        int L = 0;
                        int L_openr = 0;
                        int L0 = 0;
                        int L1 = 0;
                        int L2 = 0;
                        int L3 = 0;
                        int L4 = 0;

                        string sL1 = "-";
                        string sL2 = "-";
                        string sL3 = "-";
                        string sL4 = "-";

                        string Lmail = "-";
                        //  string L0mail = "-";
                        string L1mail = "-";
                        string L2mail = "-";
                        string L3mail = "-";
                        string L4mail = "-";


                        if (bug.ixProject != 19)
                        {
                            return;

                        }


                        // fetching approvers details

                        // L = bug.ixPersonLastEditedBy;

                        L = bug.ixPersonOpenedBy;
                        L_openr = bug.ixPersonOpenedBy;

                        //  this.api.Notifications.AddMessage("L||" + L);

                        L0 = bug.ixPersonResolvedBy;
                        {
                            CPersonQuery intL1 = api.Person.NewPersonQuery();
                            intL1.IgnorePermissions = true;
                            intL1.AddSelect("sEmail");
                            intL1.AddWhere(" Person.ixPerson = " + L_openr);

                            DataSet Dpers1 = intL1.GetDataSet();

                            if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                            {
                                //L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                Lmail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                            }

                        }

                        {

                            sL1 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                            sL2 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                            sL3 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                            sL4 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                            // fetching L1 approver deatils
                            if (sL1 != "-")
                            {
                                CPersonQuery intL1 = api.Person.NewPersonQuery();
                                intL1.IgnorePermissions = true;
                                intL1.AddSelect("Person.ixPerson,Person.sEmail");
                                intL1.AddWhere(" Person.sFullName = " + "'" + sL1 + "'");

                                DataSet Dpers1 = intL1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    L1 = Convert.ToInt32(Dpers1.Tables[0].Rows[0]["ixPerson"]);
                                    L1mail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L2 approver deatils



                            if (sL2 != "-")
                            {
                                CPersonQuery intL2 = api.Person.NewPersonQuery();
                                intL2.IgnorePermissions = true;
                                intL2.AddSelect("Person.ixPerson,Person.sEmail");
                                intL2.AddWhere(" Person.sFullName = " + "'" + sL2 + "'");

                                DataSet Dpers2 = intL2.GetDataSet();

                                if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                {
                                    L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                    L2mail = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }
                            // fetching L3 approver deatils
                            if (sL3 != "-")
                            {
                                CPersonQuery intL3 = api.Person.NewPersonQuery();
                                intL3.IgnorePermissions = true;
                                intL3.AddSelect("Person.ixPerson,Person.sEmail");
                                intL3.AddWhere(" Person.sFullName = " + "'" + sL3 + "'");

                                DataSet Dpers3 = intL3.GetDataSet();

                                if (Dpers3.Tables.Count > 0 && Dpers3.Tables[0] != null && Dpers3.Tables[0].Rows.Count > 0)
                                {
                                    L3 = Convert.ToInt32(Dpers3.Tables[0].Rows[0]["ixPerson"]);
                                    L3mail = Convert.ToString(Dpers3.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L4 approver deatils
                            if (sL4 != "-")
                            {
                                CPersonQuery intL4 = api.Person.NewPersonQuery();
                                intL4.IgnorePermissions = true;
                                intL4.AddSelect("Person.ixPerson,Person.sEmail");
                                intL4.AddWhere(" Person.sFullName = " + "'" + sL4 + "'");

                                DataSet Dpers4 = intL4.GetDataSet();

                                if (Dpers4.Tables.Count > 0 && Dpers4.Tables[0] != null && Dpers4.Tables[0].Rows.Count > 0)
                                {
                                    L4 = Convert.ToInt32(Dpers4.Tables[0].Rows[0]["ixPerson"]);
                                    L4mail = Convert.ToString(Dpers4.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            //  this.api.Notifications.AddMessage(L1 + "|" + sL2 + L2 + "|" + sL3 + L3 + "|" + sL4 + L4);
                        }


                        // this.api.Notifications.AddMessage("A1");
                        if (bug.ixStatus == 140)
                        {
                            // this.api.Notifications.AddMessage("resolve-1");

                            vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                            InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();

                            string Appr_mailsub = "An Invoice is awaiting your approval for vendor:" + vendor_1 + " Invoice:" + InvNo_1;
                            string Appr_mailbody = "There is an invoice requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;

                            string Proc_mailsub = "An Invoice has been Approved";
                            string Proc_mailbody = "There is an invoice which has been Approved.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;

                            // this.api.Notifications.AddMessage("1");

                            /*
                            if (sL1 == "-" && sL2 == "-" && sL3 == "-" && sL4 == "-")
                            {
                                this.api.Notifications.AddMessage("2");
                                this.api.Notifications.AddMessage("An email has been sent on Invoice Approved status to the requestor");
                                // mailsub = "Invoice '" + Vendname + "-" + invoiceno + "'  has been Approved";
                                //   mailsub = "An Invoice has been Approved";
                                //  mailbody = "There is an invoice which has been Approved.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;
                                          string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                                          CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                          Update1_1.UpdateInt("ixAtlevel", 5);
                        
                                mailsender(L3mail, bug, Proc_mailsub, Proc_mailbody, iperson);
                            }
                            */




                            if (sL1 != "-")
                            {
                                // this.api.Notifications.AddMessage("resolve-2");
                                if (L1 == L0)
                                {
                                    // this.api.Notifications.AddMessage("resolve-3");
                                    if (sL2 != "-")
                                    {
                                        // this.api.Notifications.AddMessage("resolve-4");
                                        // this.api.Notifications.AddMessage("L1 level");
                                        //this.api.Notifications.AddMessage("5");
                                        this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                        bug.ixPersonAssignedTo = L2;
                                        bug.ixStatus = 145;
                                        //updating atlevel
                                        //   string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 3);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                        mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                        //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                    }
                                    else
                                    {
                                        // this.api.Notifications.AddMessage("resolve-5");
                                        //  this.api.Notifications.AddMessage("L2_1 level");
                                        //  this.api.Notifications.AddMessage("31");
                                        this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requestor");
                                        //  string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 5);

                                        // this.api.Notifications.AddMessage("assgined to email||" + Lmail);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                        //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                        //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                        bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                        // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                    }
                                }

                                else if (sL2 != "-")
                                {
                                    //this.api.Notifications.AddMessage("3");

                                    if (L2 == L0)
                                    {
                                        // this.api.Notifications.AddMessage("4");
                                        if (sL3 != "-")
                                        {
                                            // this.api.Notifications.AddMessage("L2 level");
                                            //this.api.Notifications.AddMessage("5");
                                            this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                            bug.ixPersonAssignedTo = L3;
                                            bug.ixStatus = 145;
                                            //updating atlevel
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);
                                            mailsender(L3mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                            //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                        }
                                        else
                                        {
                                            //  this.api.Notifications.AddMessage("L2_1 level");
                                            //  this.api.Notifications.AddMessage("9");
                                            this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requestor");
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            // Update1_1.UpdateInt("ixAtlevel", 5);

                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                            // this.api.Notifications.AddMessage("ixPersonLastEditedBy||" + bug.ixPersonLastEditedBy);
                                            // this.api.Notifications.AddMessage("assgined to email||" + Lmail);
                                            //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                            // bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                            bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                            // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                        }
                                    }



                                    else if (sL3 != "-")
                                    {
                                        if (L3 == L0)
                                        {
                                            // this.api.Notifications.AddMessage("L3 level");
                                            if (sL4 != "-")
                                            {
                                                this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                                //  this.api.Notifications.AddMessage("SL4|" + sL4);
                                                // this.api.Notifications.AddMessage("L4|" + L4);
                                                bug.ixPersonAssignedTo = L4;
                                                bug.ixStatus = 145;

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //Update1_1.UpdateInt("ixAtlevel", 4);
                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                                mailsender(L4mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                                //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                            }
                                            else
                                            {
                                                //  this.api.Notifications.AddMessage("L3_1 level");
                                                // this.api.Notifications.AddMessage("8");
                                                this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                // Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                //  this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                            }
                                        }


                                        else if (sL4 != "-")
                                        {

                                            if (L4 == L0)
                                            {
                                                // this.api.Notifications.AddMessage("L4 level");

                                                // this.api.Notifications.AddMessage("9");
                                                this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //  Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //  bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                //  this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);

                                            }
                                        }
                                    }
                                }
                            }
                        }


                        // For Rejection 

                        if (bug.ixStatus == 148)
                        {
                            if (bugevent.s == "")
                            {

                                this.api.Notifications.AddMessage("                                                                                                               ");
                                this.api.Notifications.AddMessage("                                                                                                             ");
                                this.api.Notifications.AddMessage("                                                                                                              ");
                                this.api.Notifications.AddMessage("                                                                                                            ");
                                this.api.Notifications.AddError("                                                       -                                      ");
                                this.api.Notifications.AddError("-----------------------------------Error Message------------------------------");
                                this.api.Notifications.AddError("You cannot reject a case without any reason");
                                this.api.Notifications.AddError("Please reject the case again with an appropraite reason entered in the comment box");
                                this.api.Notifications.AddMessage("--------------------------------------------------------------------------------------");
                                this.api.Notifications.AddMessage("----------------------------------   -SOLUTION-  -------------------------------------");
                                this.api.Notifications.AddMessage("                     To Reject the case");
                                this.api.Notifications.AddMessage("                     Click on resolve button");
                                this.api.Notifications.AddMessage("                     Set the status to 'Rejected' ");
                                this.api.Notifications.AddMessage("                     Enter your reasons of rejections in the comment box");
                                this.api.Notifications.AddMessage("                     Click 'Resolve' Button");
                                this.api.Notifications.AddMessage("-----------------------------------------------------------------------------");
                                this.api.Notifications.AddError("-Error--------Error---------End of Error Message----------Error----------Error-");
                                bug.ixPersonAssignedTo = bugevent.ixPerson;
                                //bugevent.ixPersonAssignedTo = bugevent.ixPerson;
                                bug.ixPersonAssignedTo = bug.ixPersonResolvedBy;
                                bug.ixStatus = 145;
                                return;
                            }




                            //Finding the level of rejection and sendin email accordingly
                            //string RL0mail = "-";
                            string RL1mail = "-";
                            // string RL2mail = "-";
                            //string RL3mail = "-";
                            // string RL4mail = "-";


                            string Rej_mailsub = "An Invoice has been Rejected";
                            string Rej_mailbody = "There is an invoice which has been.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;


                            {
                                CPersonQuery rejL1 = api.Person.NewPersonQuery();
                                rejL1.IgnorePermissions = true;
                                rejL1.AddSelect("sEmail");
                                rejL1.AddWhere(" Person.ixPerson = " + bug.ixPersonOpenedBy);

                                DataSet Dpers5 = rejL1.GetDataSet();

                                if (Dpers5.Tables.Count > 0 && Dpers5.Tables[0] != null && Dpers5.Tables[0].Rows.Count > 0)
                                {
                                    //L4 = Convert.ToInt32(Dpers5.Tables[0].Rows[0]["ixPerson"]);
                                    RL1mail = Convert.ToString(Dpers5.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                                mailsender(RL1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //  this.api.Notifications.AddMessage("processor | " + RL1mail);
                            }



                            if (L0 == L1)
                            {
                                // api.Notifications.AddMessage("executed at L1 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 1);


                                // mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //   mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //this.api.Notifications.AddMessage("Second assignee| " + L1mail);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requestor");

                            }

                            if (L0 == L2)
                            {
                                //api.Notifications.AddMessage("executed at L2 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requestor");
                            }


                            else if (L0 == L3)
                            {
                                // api.Notifications.AddMessage("executed at L3 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                //   mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requestor");
                            }

                            else if (L0 == L4)
                            {
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                //   api.Notifications.AddMessage("executed at L4 level");
                                // this.api.Notifications.AddMessage("Fourth Approver| " + L1mail + "||" + L2mail + "||" + L3mail);
                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requestor");

                            }

                        }

                    }

                }

            }
            #endregion

            # region Trilogy for MLA workflow begin


            {
                int assigned_change = 0;
                //   api.Notifications.AddAdminNotification("projectid1", "22");
                if (bug.ixProject == 22)
                {
                    {
                        // api.Notifications.AddAdminNotification("projectid", bug.ixProject.ToString());

                        //if ((nBugAction == BugAction.Edit && bug.ixStatus == 99 || bug.ixStatus == 102) || (nBugAction == BugAction.Assign && bug.ixStatus == 69 || bug.ixStatus == 72))
                        if (nBugAction == BugAction.Edit || nBugAction == BugAction.Assign)
                        {

                            //  api.Notifications.AddAdminNotification("projectid2", "2");
                            if (bug.ixStatus == 162)
                            {

                                //  api.Notifications.AddAdminNotification("BCE-1", "trillogy");

                                string sL1e = "-";
                                string sL2e = "-";
                                string sL3e = "-";
                                string sL4e = "-";
                                int atlevel = 0;

                                // api.Notifications.AddAdminNotification("projectid13", "3");
                                string Assignedto = "";

                                CSelectQuery approvers = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                                approvers.AddSelect("CWFApproverl1,CWFApproverl2,CWFApproverl3,CWFApproverl4");
                                approvers.AddWhere("ixBug = " + bug.ixBug.ToString());

                                DataSet ds_1 = approvers.GetDataSet();

                                if (null != ds_1.Tables && ds_1.Tables.Count == 1 && ds_1.Tables[0].Rows.Count == 1)
                                {
                                    //api.Notifications.AddAdminNotification("projectid14", "4");
                                    sL1e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                                    sL2e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                                    sL3e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                                    sL4e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                                    //  this.api.Notifications.AddMessage(sL1e_1 + "||" + sL1e );
                                }
                                // api.Notifications.AddAdminNotification("approver", sL1e.ToString());
                                //checking Approval sequence

                                if (sL1e == "-" && sL2e == "-" && sL3e == "-" && sL4e == "-")
                                {
                                    //  api.Notifications.AddAdminNotification("projectid4", "5");

                                    api.Notifications.AddError(sL1e + "|" + sL2e + "|" + sL3e + "|" + sL4e);
                                    api.Notifications.AddMessage("Please set atleast one approver in the approavl sequence ");
                                    api.Notifications.AddMessage("email is not sent to the approvers ");
                                    bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                    bug.ixStatus = 167;
                                    return;
                                }


                                if ((sL4e != "-" && sL3e == "-") || (sL3e != "-" && sL2e == "-") || (sL2e != "-" && sL1e == "-"))
                                {

                                    this.api.Notifications.AddMessage("Please set the approval sequence properly ");
                                    //change by poornima
                                    //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                    bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                    this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                    bug.ixStatus = 167;
                                    return;
                                }


                                if ((sL1e != "-" && (sL1e == sL2e || sL1e == sL3e || sL1e == sL4e)) ||
                                    (sL2e != "-" && (sL2e == sL3e || sL1e == sL4e)) ||
                                    (sL3e != "-" && (sL3e == sL4e)))
                                {


                                    this.api.Notifications.AddMessage("Improper approval sequence- make sure no approvers are repeated in the sequence");
                                    //change by poornima
                                    //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                    bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                    this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                    bug.ixStatus = 167;
                                    return;
                                }



                                // finding assigned to person
                                {

                                    CPersonQuery appr1 = api.Person.NewPersonQuery();
                                    appr1.IgnorePermissions = true;
                                    appr1.AddSelect("Person.sFullName");
                                    appr1.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo);

                                    DataSet Dpers1 = appr1.GetDataSet();

                                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                    {
                                        Assignedto = Convert.ToString(Dpers1.Tables[0].Rows[0]["sFullName"]);
                                    }

                                }

                                //at levels
                                {

                                    if (sL1e == Assignedto)
                                    {
                                        atlevel = 1;
                                    }

                                    else if (sL2e == Assignedto)
                                    {
                                        atlevel = 2;
                                    }

                                    else if (sL3e == Assignedto)
                                    {
                                        atlevel = 3;
                                    }

                                    else if (sL4e == Assignedto)
                                    {
                                        atlevel = 4;
                                    }

                                    else if ((sL1e != Assignedto) || (sL2e != Assignedto) || (sL3e != Assignedto) || (sL4e != Assignedto))
                                    {
                                        this.api.Notifications.AddMessage(" The'Assigned to' person must be one of in approval sequesnce ");
                                        bug.ixStatus = 167;
                                        return;
                                    }

                                    string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                                    bug.SetPluginField(PLUGIN_ID, "ixAtlevel", atlevel);

                                } //updating atlevel ends here

                                //   {



                                {
                                    CPersonQuery pers = api.Person.NewPersonQuery();
                                    pers.IgnorePermissions = true;
                                    pers.AddSelect("*");
                                    pers.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo.ToString());

                                    DataSet Dpers = pers.GetDataSet();

                                    if (Dpers.Tables.Count > 0 && Dpers.Tables[0] != null && Dpers.Tables[0].Rows.Count > 0)
                                    {
                                        // api.Notifications.AddAdminNotification("loop2", "2");
                                        string semail1 = Convert.ToString(Dpers.Tables[0].Rows[0]["sEmail"]);
                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' awaiting your approval";
                                        mailsub = "A Notice is awaiting your approval for vendor:" + vendor_1 + " Invoice:" + InvNo_1;
                                        mailbody = "There is a Notice requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        iperson = bug.ixPersonAssignedTo;
                                        // mailsender(semail1, bug, mailsub, mailbody, iperson);
                                        this.api.Notifications.AddMessage("An email has been sent to the approver successfully");
                                        CBugQuery pers_ass = api.Bug.NewBugQuery();
                                        pers_ass.IgnorePermissions = true;
                                        pers_ass.AddSelect("Bug.ixPersonAssignedTo");
                                        pers_ass.AddWhere(" Bug.ixBug =" + bug.ixBug.ToString());

                                        DataSet ds_per = pers_ass.GetDataSet();

                                        if (ds_per.Tables.Count > 0 && ds_per.Tables[0] != null && ds_per.Tables[0].Rows.Count > 0)
                                        {
                                            try
                                            {
                                                assigned_change = Convert.ToInt32((ds_per.Tables[0].Rows[0]["ixPersonAssignedTo"]));
                                            }

                                            catch
                                            {
                                                // just keep going
                                            }

                                        }

                                        if (assigned_change != iperson)
                                        {

                                            mailsender_Tax(semail1, bug, mailsub, mailbody, iperson);

                                            if ((bug.GetPluginField("customfields@fogcreek.com", "testxemaili026")) != null)
                                            {
                                                if (bug.GetPluginField("customfields@fogcreek.com", "testxemaili026").ToString() != "")
                                                {
                                                    string CCemail = (bug.GetPluginField("customfields@fogcreek.com", "testxemaili026")).ToString();
                                                    mailsender_Tax(CCemail.Trim(), bug, mailsub, mailbody, iperson);
                                                    this.api.Notifications.AddMessage("A CCEmail has been sent Successfully");
                                                }
                                            }

                                            this.api.Notifications.AddMessage("An email has been sent to the approver successfully");

                                        }
                                    }
                                }


                                {

                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.ixPerson = " + bug.ixPersonLastEditedBy.ToString());

                                    DataSet Dpers1 = pers1.GetDataSet();

                                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' has sent for Approval";
                                        mailsub = "A Notice is sent for Approval";
                                        mailbody = "There is a Notice Sent for Approval.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;
                                        iperson = bug.ixPersonLastEditedBy;
                                        mailsender_Tax(semail1, bug, mailsub, mailbody, iperson);

                                    }



                                }

                            }

                        }
                    }

                    if (nBugAction == BugAction.Resolve)
                    {

                        //   this.api.Notifications.AddAdminNotification("Resolve", "Resolve");
                        if (bug.ixProject == 22)
                        {
                            //   this.api.Notifications.AddAdminNotification("Resolve1", "Resolve1");
                            iperson = 0;

                            int L = 0;
                            int L_openr = 0;
                            int L0 = 0;
                            int L1 = 0;
                            int L2 = 0;
                            int L3 = 0;
                            int L4 = 0;

                            string sL1 = "-";
                            string sL2 = "-";
                            string sL3 = "-";
                            string sL4 = "-";

                            string Lmail = "-";
                            //  string L0mail = "-";
                            string L1mail = "-";
                            string L2mail = "-";
                            string L3mail = "-";
                            string L4mail = "-";

                            L = bug.ixPersonOpenedBy;
                            L_openr = bug.ixPersonOpenedBy;

                            L0 = bug.ixPersonResolvedBy;
                            {
                                CPersonQuery intL1 = api.Person.NewPersonQuery();
                                intL1.IgnorePermissions = true;
                                intL1.AddSelect("sEmail");
                                intL1.AddWhere(" Person.ixPerson = " + L_openr);

                                DataSet Dpers1 = intL1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    //L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                    Lmail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            {
                                sL1 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                                sL2 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                                sL3 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                                sL4 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                                // fetching L1 approver deatils
                                if (sL1 != "-")
                                {
                                    CPersonQuery intL1 = api.Person.NewPersonQuery();
                                    intL1.IgnorePermissions = true;
                                    intL1.AddSelect("Person.ixPerson,Person.sEmail");
                                    intL1.AddWhere(" Person.sFullName = " + "'" + sL1 + "'");

                                    DataSet Dpers1 = intL1.GetDataSet();

                                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                    {
                                        L1 = Convert.ToInt32(Dpers1.Tables[0].Rows[0]["ixPerson"]);
                                        L1mail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                        //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                    }

                                }

                                // fetching L2 approver deatils



                                if (sL2 != "-")
                                {
                                    CPersonQuery intL2 = api.Person.NewPersonQuery();
                                    intL2.IgnorePermissions = true;
                                    intL2.AddSelect("Person.ixPerson,Person.sEmail");
                                    intL2.AddWhere(" Person.sFullName = " + "'" + sL2 + "'");

                                    DataSet Dpers2 = intL2.GetDataSet();

                                    if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                    {
                                        L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                        L2mail = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                        //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                    }

                                }
                                // fetching L3 approver deatils
                                if (sL3 != "-")
                                {
                                    CPersonQuery intL3 = api.Person.NewPersonQuery();
                                    intL3.IgnorePermissions = true;
                                    intL3.AddSelect("Person.ixPerson,Person.sEmail");
                                    intL3.AddWhere(" Person.sFullName = " + "'" + sL3 + "'");

                                    DataSet Dpers3 = intL3.GetDataSet();

                                    if (Dpers3.Tables.Count > 0 && Dpers3.Tables[0] != null && Dpers3.Tables[0].Rows.Count > 0)
                                    {
                                        L3 = Convert.ToInt32(Dpers3.Tables[0].Rows[0]["ixPerson"]);
                                        L3mail = Convert.ToString(Dpers3.Tables[0].Rows[0]["sEmail"]);
                                        //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                    }

                                }

                                // fetching L4 approver deatils
                                if (sL4 != "-")
                                {
                                    CPersonQuery intL4 = api.Person.NewPersonQuery();
                                    intL4.IgnorePermissions = true;
                                    intL4.AddSelect("Person.ixPerson,Person.sEmail");
                                    intL4.AddWhere(" Person.sFullName = " + "'" + sL4 + "'");

                                    DataSet Dpers4 = intL4.GetDataSet();

                                    if (Dpers4.Tables.Count > 0 && Dpers4.Tables[0] != null && Dpers4.Tables[0].Rows.Count > 0)
                                    {
                                        L4 = Convert.ToInt32(Dpers4.Tables[0].Rows[0]["ixPerson"]);
                                        L4mail = Convert.ToString(Dpers4.Tables[0].Rows[0]["sEmail"]);
                                        //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                    }

                                }

                                //  this.api.Notifications.AddMessage(L1 + "|" + sL2 + L2 + "|" + sL3 + L3 + "|" + sL4 + L4);
                            }

                            // api.Notifications.AddAdminNotification("Resolve3", "Resolve3");
                            // this.api.Notifications.AddMessage("A1");
                            if (bug.ixStatus == 156)
                            {
                                //  api.Notifications.AddAdminNotification("Resolve4", bug.ixStatus.ToString());
                                // this.api.Notifications.AddMessage("resolve-1");

                                //string vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                                //string InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();
                                //vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                                //InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();

                                string Appr_mailsub = "A Notice is awaiting your approval";
                                string Appr_mailbody = "There is a notice requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;

                                string Proc_mailsub = "A notice has been Approved";
                                string Proc_mailbody = "There is a notice which has been Approved.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;

                                if (sL1 != "-")
                                {
                                    // this.api.Notifications.AddMessage("resolve-2");
                                    if (L1 == L0)
                                    {
                                        // this.api.Notifications.AddMessage("resolve-3");
                                        if (sL2 != "-")
                                        {
                                            // this.api.Notifications.AddMessage("resolve-4");
                                            // this.api.Notifications.AddMessage("L1 level");
                                            //this.api.Notifications.AddMessage("5");
                                            // api.Notifications.AddAdminNotification("L2", sL2);
                                            this.api.Notifications.AddMessage("The notice has been approved and assigned to next approver successfully");
                                            bug.ixPersonAssignedTo = L2;
                                            bug.ixStatus = 162;
                                            //  api.Notifications.AddAdminNotification("ixStatus", bug.ixStatus.ToString());
                                            //updating atlevel
                                            //   string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            // Update1_1.UpdateInt("ixAtlevel", 3);

                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                            mailsender_Tax(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                            //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                        }
                                        else
                                        {
                                            // this.api.Notifications.AddMessage("resolve-5");
                                            //  this.api.Notifications.AddMessage("L2_1 level");
                                            //  this.api.Notifications.AddMessage("31");
                                            this.api.Notifications.AddMessage("The notice has been approved successfully and an email notification sent to the requestor");
                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                            bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                            //this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                        }
                                    }




                                    else if (sL2 != "-")
                                    {
                                        //this.api.Notifications.AddMessage("3");

                                        if (L2 == L0)
                                        {
                                            // this.api.Notifications.AddMessage("4");
                                            if (sL3 != "-")
                                            {
                                                // this.api.Notifications.AddMessage("L2 level");
                                                //this.api.Notifications.AddMessage("5");
                                                this.api.Notifications.AddMessage("The notice has been approved and assigned to next approver successfully");
                                                bug.ixPersonAssignedTo = L3;
                                                bug.ixStatus = 162;
                                                //updating atlevel
                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                                mailsender_Tax(L3mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                                //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                            }
                                            else
                                            {
                                                //  this.api.Notifications.AddMessage("L2_1 level");
                                                //  this.api.Notifications.AddMessage("9");
                                                this.api.Notifications.AddMessage("The notice has been approved successfully and an email notification sent to the requestor");
                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                // Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                                // this.api.Notifications.AddMessage("ixPersonLastEditedBy||" + bug.ixPersonLastEditedBy);
                                                // this.api.Notifications.AddMessage("assgined to email||" + Lmail);
                                                // mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                // bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                            }
                                        }



                                        else if (sL3 != "-")
                                        {
                                            if (L3 == L0)
                                            {
                                                // this.api.Notifications.AddMessage("L3 level");
                                                if (sL4 != "-")
                                                {
                                                    this.api.Notifications.AddMessage("The notice has been approved and assigned to next approver successfully");
                                                    //  this.api.Notifications.AddMessage("SL4|" + sL4);
                                                    // this.api.Notifications.AddMessage("L4|" + L4);
                                                    bug.ixPersonAssignedTo = L4;
                                                    bug.ixStatus = 162;

                                                    // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                    // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                    //Update1_1.UpdateInt("ixAtlevel", 4);
                                                    bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                                    mailsender_Tax(L4mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                                    //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                                }
                                                else
                                                {
                                                    //  this.api.Notifications.AddMessage("L3_1 level");
                                                    // this.api.Notifications.AddMessage("8");
                                                    this.api.Notifications.AddMessage("The notice has been approved successfully and an email notification sent to the requestor");

                                                    // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                    // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                    // Update1_1.UpdateInt("ixAtlevel", 5);

                                                    bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                    //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                    //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                                    bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                    // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                                }
                                            }


                                            else if (sL4 != "-")
                                            {

                                                if (L4 == L0)
                                                {
                                                    // this.api.Notifications.AddMessage("L4 level");

                                                    // this.api.Notifications.AddMessage("9");
                                                    this.api.Notifications.AddMessage("The notice has been approved successfully and an email notification sent to the requestor");

                                                    // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                    //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                    //  Update1_1.UpdateInt("ixAtlevel", 5);

                                                    bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                    //mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                    //  bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                                    bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                    //this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);

                                                }
                                            }
                                        }
                                    }
                                }

                            }

                            // For Rejection 

                            if (bug.ixStatus == 157)
                            {


                                if (bugevent.s == "")
                                {

                                    this.api.Notifications.AddMessage("                                                                                                               ");
                                    this.api.Notifications.AddMessage("                                                                                                             ");
                                    this.api.Notifications.AddMessage("                                                                                                              ");
                                    this.api.Notifications.AddMessage("                                                                                                            ");
                                    this.api.Notifications.AddError("                                                       -                                      ");
                                    this.api.Notifications.AddError("-----------------------------------Error Message------------------------------");
                                    this.api.Notifications.AddError("You cannot reject a case without any reason");
                                    this.api.Notifications.AddError("Please reject the case again with an appropraite reason entered in the comment box");

                                    this.api.Notifications.AddMessage("--------------------------------------------------------------------------------------");

                                    this.api.Notifications.AddMessage("----------------------------------   -SOLUTION-  -------------------------------------");
                                    this.api.Notifications.AddMessage("                     To Reject the case");
                                    this.api.Notifications.AddMessage("                     Click on resolve button");
                                    this.api.Notifications.AddMessage("                     Set the status to 'Rejected' ");
                                    this.api.Notifications.AddMessage("                     Enter your reasons of rejections in the comment box");
                                    this.api.Notifications.AddMessage("                     Click 'Resolve' Button");
                                    this.api.Notifications.AddMessage("-----------------------------------------------------------------------------");
                                    this.api.Notifications.AddError("-Error--------Error---------End of Error Message----------Error----------Error-");
                                    bug.ixPersonAssignedTo = bugevent.ixPerson;
                                    //bugevent.ixPersonAssignedTo = bugevent.ixPerson;
                                    bug.ixPersonAssignedTo = bug.ixPersonResolvedBy;
                                    bug.ixStatus = 162;
                                    return;
                                }




                                //Finding the level of rejection and sendin email accordingly
                                //string RL0mail = "-";
                                string RL1mail = "-";
                                // string RL2mail = "-";
                                //string RL3mail = "-";
                                // string RL4mail = "-";


                                string Rej_mailsub = "A notice has been Rejected";
                                string Rej_mailbody = "There is a notice which has been.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;

                                {
                                    CPersonQuery rejL1 = api.Person.NewPersonQuery();
                                    rejL1.IgnorePermissions = true;
                                    rejL1.AddSelect("sEmail");
                                    rejL1.AddWhere(" Person.ixPerson = " + bug.ixPersonOpenedBy);

                                    DataSet Dpers5 = rejL1.GetDataSet();

                                    if (Dpers5.Tables.Count > 0 && Dpers5.Tables[0] != null && Dpers5.Tables[0].Rows.Count > 0)
                                    {
                                        //L4 = Convert.ToInt32(Dpers5.Tables[0].Rows[0]["ixPerson"]);
                                        RL1mail = Convert.ToString(Dpers5.Tables[0].Rows[0]["sEmail"]);
                                        //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                    }

                                    mailsender_Tax(RL1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    //  this.api.Notifications.AddMessage("processor | " + RL1mail);
                                }



                                if (L0 == L1)
                                {
                                    // api.Notifications.AddMessage("executed at L1 level");

                                    bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 1);


                                    // mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    //   mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    //this.api.Notifications.AddMessage("Second assignee| " + L1mail);
                                    this.api.Notifications.AddMessage("The notice has been rejected successfully and an email notification sent to the requestor");

                                }

                                if (L0 == L2)
                                {
                                    //api.Notifications.AddMessage("executed at L2 level");

                                    bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                    //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    mailsender_Tax(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    this.api.Notifications.AddMessage("The notice has been rejected successfully and an email notification sent to the requestor");


                                }


                                else if (L0 == L3)
                                {
                                    // api.Notifications.AddMessage("executed at L3 level");

                                    bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                    //   mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    mailsender_Tax(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    mailsender_Tax(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    this.api.Notifications.AddMessage("The notice has been rejected successfully and an email notification sent to the requestor");
                                }



                                else if (L0 == L4)
                                {
                                    bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                    //   api.Notifications.AddMessage("executed at L4 level");
                                    // this.api.Notifications.AddMessage("Fourth Approver| " + L1mail + "||" + L2mail + "||" + L3mail);
                                    //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    mailsender_Tax(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    mailsender_Tax(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    mailsender_Tax(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                    this.api.Notifications.AddMessage("The notice has been rejected successfully and an email notification sent to the requestor");

                                }






                            }

                        }
                    }

                }

            }
            #endregion

            # region Synergis for MLA workflow begin


            {

                //capturing  and storing atlevel and levels

                string status1 = "Discrepancy ";

                if (bug.ixProject == 25)
                {

                    if (nBugAction == BugAction.Edit || nBugAction == BugAction.Assign)
                    {

                        if (bug.ixStatus == 183)
                        {

                            // this.api.Notifications.AddMessage("BCE-1");

                            string sL1e = "-";
                            string sL2e = "-";
                            string sL3e = "-";
                            string sL4e = "-";
                            int atlevel = 0;



                            string Assignedto = "";

                            CSelectQuery approvers = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                            approvers.AddSelect("CWFApproverl1,CWFApproverl2,CWFApproverl3,CWFApproverl4");
                            approvers.AddWhere("ixBug = " + bug.ixBug.ToString());

                            DataSet ds_1 = approvers.GetDataSet();

                            if (null != ds_1.Tables && ds_1.Tables.Count == 1 && ds_1.Tables[0].Rows.Count == 1)
                            {

                                sL1e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                                sL2e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                                sL3e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                                sL4e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                                //  this.api.Notifications.AddMessage(sL1e_1 + "||" + sL1e );

                            }

                            //checking Approval sequence

                            if (sL1e == "-" && sL2e == "-" && sL3e == "-" && sL4e == "-")
                            {


                                this.api.Notifications.AddMessage(sL1e + "|" + sL2e + "|" + sL3e + "|" + sL4e);
                                this.api.Notifications.AddMessage("Please set atleast one approver in the approavl sequence ");
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                string stat = (bug.GetPluginField(PLUGIN_ID, "CWFUserStatus")).ToString().Trim();
                                // this.api.Notifications.AddMessage(stat);
                                bug.ixStatus = 185;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1.ToString());

                                string stat1 = (bug.GetPluginField(PLUGIN_ID, "CWFUserStatus")).ToString().Trim();
                                //  this.api.Notifications.AddMessage(stat1);
                                return;
                            }


                            if ((sL4e != "-" && sL3e == "-") || (sL3e != "-" && sL2e == "-") || (sL2e != "-" && sL1e == "-"))
                            {


                                this.api.Notifications.AddMessage("Please set the approval sequence properly ");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 185;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);


                                return;
                            }


                            if ((sL1e != "-" && (sL1e == sL2e || sL1e == sL3e || sL1e == sL4e)) ||
                                (sL2e != "-" && (sL2e == sL3e || sL1e == sL4e)) ||
                                (sL3e != "-" && (sL3e == sL4e)))
                            {


                                this.api.Notifications.AddMessage("Improper approval sequence- make sure no approvers are repeated in the sequence");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 185;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                return;
                            }




                            // finding assigned to person
                            {

                                CPersonQuery appr1 = api.Person.NewPersonQuery();
                                appr1.IgnorePermissions = true;
                                appr1.AddSelect("Person.sFullName");
                                appr1.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo);

                                DataSet Dpers1 = appr1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    Assignedto = Convert.ToString(Dpers1.Tables[0].Rows[0]["sFullName"]);
                                }

                            }

                            //at levels
                            {

                                if (sL1e == Assignedto)
                                {
                                    atlevel = 1;
                                }

                                else if (sL2e == Assignedto)
                                {
                                    atlevel = 2;
                                }

                                else if (sL3e == Assignedto)
                                {
                                    atlevel = 3;
                                }

                                else if (sL4e == Assignedto)
                                {
                                    atlevel = 4;
                                }

                                else if ((sL1e != Assignedto) || (sL2e != Assignedto) || (sL3e != Assignedto) || (sL4e != Assignedto))
                                {
                                    this.api.Notifications.AddMessage(" The'Assigned to' person must be in approval sequence ");
                                    bug.ixStatus = 185;
                                    bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                    return;
                                }


                                // updating atlevel
                                string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", atlevel);

                            } //updating atlevel ends here

                            int i = 0;

                            {


                                if (i == 1)
                                {
                                    bug.ixStatus = 185;
                                    bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                    return;
                                }

                                string vendor_3 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                                string amt = (bug.GetPluginField(PLUGIN_ID, "TotalAmount")).ToString();
                                string title = bug.sTitle.ToString();
                                {
                                  
                                    CPersonQuery pers = api.Person.NewPersonQuery();
                                    pers.IgnorePermissions = true;
                                    pers.AddSelect("*");
                                    pers.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo.ToString());

                                    DataSet Dpers = pers.GetDataSet();

                                    if (Dpers.Tables.Count > 0 && Dpers.Tables[0] != null && Dpers.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers.Tables[0].Rows[0]["sEmail"]);

                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' awaiting your approval";
                                        mailsub = "A PO is awaiting your approval for vendor : " + vendor_3 + ", amount: $" + amt.ToString() + " and description :" + title;
                                        // mailbody = "There is an PO requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        //  mailbody = "There is a PO requiring your attention.  Please click on the below link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        mailbody = "A PO is awaiting for your approval for vendor: " + vendor_3 + " and amount: $" + amt.ToString() + ". Please click on the link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        iperson = bug.ixPersonAssignedTo;
                                        mailsender_Syn(semail1, bug, mailsub, mailbody, iperson);
                                        MailBody = mailbody;
                                        MailSub = mailsub;
                                        ixperson = iperson;
                                        flag = 1;

                                        //if ((bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")) != null)
                                        //{
                                        //    if (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013").ToString() != "")
                                        //    {

                                        //        string CCemail = (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")).ToString();
                                        //        mailsender(CCemail.Trim(), bug, mailsub, mailbody, iperson);
                                        //        this.api.Notifications.AddMessage("A CCEmail has been sent Successfully");
                                        //    }
                                        //}
                                        this.api.Notifications.AddMessage("An email has been sent to the approver successfully");


                                    }

                                }

                                {

                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.ixPerson = " + bug.ixPersonLastEditedBy.ToString());

                                    DataSet Dpers1 = pers1.GetDataSet();

                                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' has sent for Approval";
                                        mailsub = "A PO is sent for Approval for Vendor:" + vendor_3 + ", amount: $" + amt.ToString() + ", description :" + title;
                                        mailbody = "There is a PO Sent for Approval for Vendor:" + vendor_3 + ", amount: $" + amt.ToString() + ", description :" + title + ". Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;
                                        //mailbody += System.Environment.NewLine;
                                        //mailbody += System.Environment.NewLine;
                                        iperson = bug.ixPersonLastEditedBy;
                                        mailsender_Syn(semail1, bug, mailsub, mailbody, iperson);

                                    }

                                }

                            } // sending emails ends here


                        }
                    }

                    if (nBugAction == BugAction.Resolve)
                    {

                        string UserStatus = (bug.GetPluginField(PLUGIN_ID, "CWFUserResolve")).ToString().Trim();
                        api.Notifications.AddAdminNotification("statusR", UserStatus.ToString());

                        iperson = 0;
                        // string Vendor_Name1 = "";
                        // string Invoice1 = "";

                        int L = 0;
                        int L_openr = 0;
                        int L0 = 0;
                        int L1 = 0;
                        int L2 = 0;
                        int L3 = 0;
                        int L4 = 0;

                        string sL1 = "-";
                        string sL2 = "-";
                        string sL3 = "-";
                        string sL4 = "-";

                        string Lmail = "-";
                        //  string L0mail = "-";
                        string L1mail = "-";
                        string L2mail = "-";
                        string L3mail = "-";
                        string L4mail = "-";


                        if (bug.ixProject != 25)
                        {
                            return;

                        }


                        // fetching approvers details

                        // L = bug.ixPersonLastEditedBy;

                        L = bug.ixPersonOpenedBy;
                        L_openr = bug.ixPersonOpenedBy;

                        //  this.api.Notifications.AddMessage("L||" + L);

                        L0 = bug.ixPersonResolvedBy;
                        {
                            CPersonQuery intL1 = api.Person.NewPersonQuery();
                            intL1.IgnorePermissions = true;
                            intL1.AddSelect("sEmail");
                            intL1.AddWhere(" Person.ixPerson = " + L_openr);

                            DataSet Dpers1 = intL1.GetDataSet();

                            if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                            {
                                //L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                Lmail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                            }

                        }
                        sL1 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                        sL2 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                        sL3 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                        sL4 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                        {



                            // fetching L1 approver deatils
                            if (sL1 != "-")
                            {
                                CPersonQuery intL1 = api.Person.NewPersonQuery();
                                intL1.IgnorePermissions = true;
                                intL1.AddSelect("Person.ixPerson,Person.sEmail");
                                intL1.AddWhere(" Person.sFullName = " + "'" + sL1 + "'");

                                DataSet Dpers1 = intL1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    L1 = Convert.ToInt32(Dpers1.Tables[0].Rows[0]["ixPerson"]);
                                    L1mail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L2 approver deatils



                            if (sL2 != "-")
                            {
                                CPersonQuery intL2 = api.Person.NewPersonQuery();
                                intL2.IgnorePermissions = true;
                                intL2.AddSelect("Person.ixPerson,Person.sEmail");
                                intL2.AddWhere(" Person.sFullName = " + "'" + sL2 + "'");

                                DataSet Dpers2 = intL2.GetDataSet();

                                if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                {
                                    L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                    L2mail = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }
                            // fetching L3 approver deatils
                            if (sL3 != "-")
                            {
                                CPersonQuery intL3 = api.Person.NewPersonQuery();
                                intL3.IgnorePermissions = true;
                                intL3.AddSelect("Person.ixPerson,Person.sEmail");
                                intL3.AddWhere(" Person.sFullName = " + "'" + sL3 + "'");

                                DataSet Dpers3 = intL3.GetDataSet();

                                if (Dpers3.Tables.Count > 0 && Dpers3.Tables[0] != null && Dpers3.Tables[0].Rows.Count > 0)
                                {
                                    L3 = Convert.ToInt32(Dpers3.Tables[0].Rows[0]["ixPerson"]);
                                    L3mail = Convert.ToString(Dpers3.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L4 approver deatils
                            if (sL4 != "-")
                            {
                                CPersonQuery intL4 = api.Person.NewPersonQuery();
                                intL4.IgnorePermissions = true;
                                intL4.AddSelect("Person.ixPerson,Person.sEmail");
                                intL4.AddWhere(" Person.sFullName = " + "'" + sL4 + "'");

                                DataSet Dpers4 = intL4.GetDataSet();

                                if (Dpers4.Tables.Count > 0 && Dpers4.Tables[0] != null && Dpers4.Tables[0].Rows.Count > 0)
                                {
                                    L4 = Convert.ToInt32(Dpers4.Tables[0].Rows[0]["ixPerson"]);
                                    L4mail = Convert.ToString(Dpers4.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            //  this.api.Notifications.AddMessage(L1 + "|" + sL2 + L2 + "|" + sL3 + L3 + "|" + sL4 + L4);
                        }

                        string vendor_3 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                        string amt = (bug.GetPluginField(PLUGIN_ID, "TotalAmount")).ToString();
                        string title = bug.sTitle.ToString();
                        // this.api.Notifications.AddMessage("A1");

                        if (UserStatus == "Rejected")
                        {
                            bug.ixStatus = 184;
                        }

                        if (bug.ixStatus == 180)
                        {
                            // this.api.Notifications.AddMessage("resolve-1");

                            vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();


                            string Appr_mailsub = "A PO is awaiting your approval for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + " and description :" + title;
                            //  string Appr_mailbody = "There is a PO requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                            // string Appr_mailbody = "There is a PO requiring your attention.  Please click on the link below and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                            string Appr_mailbody = "A PO is awaiting for your approval for vendor: " + vendor_3 + " and amount: $" + amt.ToString() + ". Please click on the link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;




                            if (sL1 != "-")
                            {
                                // this.api.Notifications.AddMessage("resolve-2");
                                if (L1 == L0)
                                {
                                    // this.api.Notifications.AddMessage("resolve-3");
                                    if (sL2 != "-")
                                    {
                                        // this.api.Notifications.AddMessage("resolve-4");
                                        // this.api.Notifications.AddMessage("L1 level");
                                        //this.api.Notifications.AddMessage("5");
                                        this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                        bug.ixPersonAssignedTo = L2;
                                        bug.ixStatus = 183;
                                        bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");
                                        //updating atlevel
                                        //   string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 3);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                        mailsender_Syn(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                        //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                    }
                                    else
                                    {
                                        // this.api.Notifications.AddMessage("resolve-5");
                                        //  this.api.Notifications.AddMessage("L2_1 level");
                                        //  this.api.Notifications.AddMessage("31");
                                        //  this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");
                                        //  string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 5);

                                        // this.api.Notifications.AddMessage("assgined to email||" + Lmail);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                        // mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                        //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                        bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                        // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                    }
                                }




                                else if (sL2 != "-")
                                {
                                    //this.api.Notifications.AddMessage("3");

                                    if (L2 == L0)
                                    {
                                        // this.api.Notifications.AddMessage("4");
                                        if (sL3 != "-")
                                        {
                                            // this.api.Notifications.AddMessage("L2 level");
                                            //this.api.Notifications.AddMessage("5");
                                            this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                            bug.ixPersonAssignedTo = L3;
                                            bug.ixStatus = 183;
                                            bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");
                                            //updating atlevel
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                            mailsender_Syn(L3mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                            //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                        }
                                        else
                                        {
                                            //  this.api.Notifications.AddMessage("L2_1 level");
                                            //  this.api.Notifications.AddMessage("9");
                                            //  this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            // Update1_1.UpdateInt("ixAtlevel", 5);

                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                            // this.api.Notifications.AddMessage("ixPersonLastEditedBy||" + bug.ixPersonLastEditedBy);
                                            // this.api.Notifications.AddMessage("assgined to email||" + Lmail);
                                            //  mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                            // bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                            bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                            //this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                        }
                                    }



                                    else if (sL3 != "-")
                                    {
                                        if (L3 == L0)
                                        {
                                            // this.api.Notifications.AddMessage("L3 level");
                                            if (sL4 != "-")
                                            {
                                                this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                                //  this.api.Notifications.AddMessage("SL4|" + sL4);
                                                // this.api.Notifications.AddMessage("L4|" + L4);
                                                bug.ixPersonAssignedTo = L4;
                                                bug.ixStatus = 183;
                                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //Update1_1.UpdateInt("ixAtlevel", 4);
                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                                mailsender_Syn(L4mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                                //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                            }
                                            else
                                            {
                                                //  this.api.Notifications.AddMessage("L3_1 level");
                                                // this.api.Notifications.AddMessage("8");
                                                // this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                // Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                //  mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                            }
                                        }


                                        else if (sL4 != "-")
                                        {

                                            if (L4 == L0)
                                            {
                                                // this.api.Notifications.AddMessage("L4 level");

                                                // this.api.Notifications.AddMessage("9");
                                                // this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //  Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                // mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //  bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                //  this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);

                                            }
                                        }
                                    }
                                }
                            }

                        }
                        {
                            int Atlevel = 0;

                            CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                            Qrynewpon.AddSelect("ixAtlevel");
                            string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                            Qrynewpon.AddWhere(sWhere2);
                            object level = Qrynewpon.GetScalarValue();
                            Atlevel = Convert.ToInt32(level);

                            { // assiging to Finance review

                                if (UserStatus == "Rejected")
                                {
                                    bug.ixStatus = 184;
                                }
                                if (bug.ixStatus == 180)
                                {
                                   // api.Notifications.AddAdminNotification("status1", bug.ixProject.ToString());
                                  //  api.Notifications.AddAdminNotification("status2", bug.ixStatus.ToString());

                                 


                                    if (bug.ixPersonResolvedBy != 294)
                                    {

                                       // int assignto = bug.ixPersonAssignedTo;
                                        string name = "";
                                        string email = "";


                                        bug.ixStatus = 183;
                                        bug.ixPersonAssignedTo = 294;
                                        int assignto = bug.ixPersonAssignedTo;

                                        CPersonQuery intfinance = api.Person.NewPersonQuery();
                                        intfinance.IgnorePermissions = true;
                                        intfinance.AddSelect("Person.sFullname,Person.sEmail");
                                        intfinance.AddWhere(" Person.ixPerson = " + assignto);

                                        DataSet Dfinance = intfinance.GetDataSet();

                                        if (Dfinance.Tables.Count > 0 && Dfinance.Tables[0] != null && Dfinance.Tables[0].Rows.Count > 0)
                                        {
                                            name = Convert.ToString(Dfinance.Tables[0].Rows[0]["sFullname"]);
                                            email = Convert.ToString(Dfinance.Tables[0].Rows[0]["sEmail"]);
                                          //  api.Notifications.AddAdminNotification("email", email.ToString());
                                            //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                        }

                                        this.api.Notifications.AddMessage("The PO has been approved and sent to Finance team for review");
                                        string Finance_mailsub = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                                        string Finance_mailbody = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + ". Click on the link to review the PO information and generate Purchase Order http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        //  string Proc_mailbody = "The PO you submitted has been approved and new PO number " + PONumb + " has been created, accounting team will share the purchase order copy shortly.";
                                        mailsender_Finance(email, bug, Finance_mailsub, Finance_mailbody, iperson);


                                    }

                                }
                            }


                            if (bug.ixPersonResolvedBy == 294) //PO generation
                            {
                                if (UserStatus == "Rejected")
                                {
                                    bug.ixStatus = 184;
                                }

                                if (bug.ixStatus == 180)
                                {
                                    int userId = bug.ixPersonOpenedBy;
                                    string mailID = "";
                                    CPersonQuery ixper = api.Person.NewPersonQuery();
                                    ixper.IgnorePermissions = true;
                                    ixper.AddSelect("Person.sEmail");
                                    ixper.AddWhere(" Person.ixPerson = " + userId);

                                    DataSet Dpers2 = ixper.GetDataSet();
                                    string PONumb = "";
                                    string sPONum_exist = "";
                                    CSelectQuery PONum_exist = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                                    PONum_exist.AddSelect("(PONumber) as PONumber");
                                    string sWhere1 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                                    PONum_exist.AddWhere(sWhere1);
                                    DataSet Synds_1 = PONum_exist.GetDataSet();

                                    if (null != Synds_1.Tables && Synds_1.Tables.Count == 1 && Synds_1.Tables[0].Rows.Count == 1)
                                    {
                                        sPONum_exist = Convert.ToString(Convert.ToString(Synds_1.Tables[0].Rows[0]["PONumber"]));


                                        if (sPONum_exist == "" || sPONum_exist == null)
                                        {
                                           // api.Notifications.AddAdminNotification("status1", sPONum_exist.ToString());
                                            int PONum = 0;
                                            CSelectQuery PONumber = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                                            PONumber.AddSelect("MAX(PONumber) as PONumber");
                                            //string sWhere1 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                                            //PONumber.AddWhere(sWhere1);
                                            DataSet Synds = PONumber.GetDataSet();
                                         //   api.Notifications.AddAdminNotification("i1", "i1");
                                            if (null != Synds.Tables && Synds.Tables.Count == 1 && Synds.Tables[0].Rows.Count == 1)
                                            {


                                              //  api.Notifications.AddAdminNotification("i2", "i2");
                                                PONum = Convert.ToInt32(Convert.ToString(Synds.Tables[0].Rows[0]["PONumber"]));
                                                int i = Convert.ToInt32(PONum) + 1;
                                              //  api.Notifications.AddAdminNotification("i3", i.ToString());
                                                bug.SetPluginField(PLUGIN_ID, "PONumber", i.ToString());
                                                PONumb = (bug.GetPluginField(PLUGIN_ID, "PONumber")).ToString().Trim();
                                                //  PONumb = (bug.GetPluginField(PLUGIN_ID, "PONumber")).ToString().Trim();
                                                if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                                {
                                                    mailID = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                                    string Proc_mailsub = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                                                    //  string Proc_mailbody = "The PO you submitted has been approved and new PO number " + PONumb + " has been created, accounting team will share the purchase order copy shortly.";
                                                    string Proc_mailbody = "The new PO you submitted has been approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + " and reviewed by Finance team. The PO number PO-1-0" + PONumb + " has been created. To access the PO please click on the link http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                                    mailsender_Syn(mailID, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                    //  this.api.Notifications.AddMessage("The PO you submitted has been approved and reviewed by Finance team");
                                                }
                                            }
                                        }

                                    }//do nothing

                                    //else
                                    //{
                                    //    bug.ixStatus = 183;
                                    //    bug.ixPersonAssignedTo = 294;
                                    //}
                                }
                            }

                            //}


                        }


                     



                        // For Rejection 
                        if (UserStatus == "Rejected")
                        {
                            bug.ixStatus = 184;
                        }
                        if (bug.ixStatus == 184)
                        {


                            if (bugevent.s == "")
                            {

                                this.api.Notifications.AddMessage("                                                                                                               ");
                                this.api.Notifications.AddMessage("                                                                                                             ");
                                this.api.Notifications.AddMessage("                                                                                                              ");
                                this.api.Notifications.AddMessage("                                                                                                            ");
                                this.api.Notifications.AddError("                                                       -                                      ");
                                this.api.Notifications.AddError("-----------------------------------Error Message------------------------------");
                                this.api.Notifications.AddError("You cannot reject a case without any reason");
                                this.api.Notifications.AddError("Please reject the case again with an appropraite reason entered in the comment box");

                                this.api.Notifications.AddMessage("--------------------------------------------------------------------------------------");

                                this.api.Notifications.AddMessage("----------------------------------   -SOLUTION-  -------------------------------------");
                                this.api.Notifications.AddMessage("                     To Reject the case");
                                this.api.Notifications.AddMessage("                     Click on resolve button");
                                this.api.Notifications.AddMessage("                     Set the status to 'Rejected' ");
                                this.api.Notifications.AddMessage("                     Enter your reasons of rejections in the comment box");
                                this.api.Notifications.AddMessage("                     Click 'Resolve' Button");
                                this.api.Notifications.AddMessage("-----------------------------------------------------------------------------");
                                this.api.Notifications.AddError("-Error--------Error---------End of Error Message----------Error----------Error-");
                                bug.ixPersonAssignedTo = bugevent.ixPerson;
                                //bugevent.ixPersonAssignedTo = bugevent.ixPerson;
                                bug.ixPersonAssignedTo = bug.ixPersonResolvedBy;
                                bug.ixStatus = 183;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Discrepancy");
                                return;
                            }




                            //Finding the level of rejection and sendin email accordingly
                            //string RL0mail = "-";
                            string RL1mail = "-";
                            // string RL2mail = "-";
                            //string RL3mail = "-";
                            // string RL4mail = "-";

                            vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                            string Rej_mailsub = "The PO has been Rejected for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                            string Rej_mailbody = "The PO has been rejected for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + ". Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;


                            {
                                CPersonQuery rejL1 = api.Person.NewPersonQuery();
                                rejL1.IgnorePermissions = true;
                                rejL1.AddSelect("sEmail");
                                rejL1.AddWhere(" Person.ixPerson = " + bug.ixPersonOpenedBy);

                                DataSet Dpers5 = rejL1.GetDataSet();

                                if (Dpers5.Tables.Count > 0 && Dpers5.Tables[0] != null && Dpers5.Tables[0].Rows.Count > 0)
                                {
                                    //L4 = Convert.ToInt32(Dpers5.Tables[0].Rows[0]["ixPerson"]);
                                    RL1mail = Convert.ToString(Dpers5.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                                mailsender_Syn(RL1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //  this.api.Notifications.AddMessage("processor | " + RL1mail);
                            }



                            if (L0 == L1)
                            {
                                // api.Notifications.AddMessage("executed at L1 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 1);


                                // mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //   mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //this.api.Notifications.AddMessage("Second assignee| " + L1mail);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");

                            }

                            if (L0 == L2)
                            {
                                //api.Notifications.AddMessage("executed at L2 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");


                            }


                            else if (L0 == L3)
                            {
                                // api.Notifications.AddMessage("executed at L3 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                //   mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");
                            }



                            else if (L0 == L4)
                            {
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                //   api.Notifications.AddMessage("executed at L4 level");
                                // this.api.Notifications.AddMessage("Fourth Approver| " + L1mail + "||" + L2mail + "||" + L3mail);
                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");

                            }

                            else if (L0 == 294)
                            {
                                mailsender_Syn(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected and an email notification sent to the requestor");
                            }




                        }

                    }

                }

            }
            #endregion

            # region Synergis_Artium for MLA workflow begin


            {

                //capturing  and storing atlevel and levels

                string status1 = "Discrepancy ";

                if (bug.ixProject == 26)
                {

                    if (nBugAction == BugAction.Edit || nBugAction == BugAction.Assign)
                    {

                        if (bug.ixStatus == 183)
                        {

                            // this.api.Notifications.AddMessage("BCE-1");

                            string sL1e = "-";
                            string sL2e = "-";
                            string sL3e = "-";
                            string sL4e = "-";
                            int atlevel = 0;



                            string Assignedto = "";

                            CSelectQuery approvers = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                            approvers.AddSelect("CWFApproverl1,CWFApproverl2,CWFApproverl3,CWFApproverl4");
                            approvers.AddWhere("ixBug = " + bug.ixBug.ToString());

                            DataSet ds_1 = approvers.GetDataSet();

                            if (null != ds_1.Tables && ds_1.Tables.Count == 1 && ds_1.Tables[0].Rows.Count == 1)
                            {

                                sL1e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                                sL2e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                                sL3e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                                sL4e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                                //  this.api.Notifications.AddMessage(sL1e_1 + "||" + sL1e );

                            }

                            //checking Approval sequence

                            if (sL1e == "-" && sL2e == "-" && sL3e == "-" && sL4e == "-")
                            {


                                this.api.Notifications.AddMessage(sL1e + "|" + sL2e + "|" + sL3e + "|" + sL4e);
                                this.api.Notifications.AddMessage("Please set atleast one approver in the approavl sequence ");
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                string stat = (bug.GetPluginField(PLUGIN_ID, "CWFUserStatus")).ToString().Trim();
                                // this.api.Notifications.AddMessage(stat);
                                bug.ixStatus = 185;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1.ToString());

                                string stat1 = (bug.GetPluginField(PLUGIN_ID, "CWFUserStatus")).ToString().Trim();
                                //  this.api.Notifications.AddMessage(stat1);
                                return;
                            }


                            if ((sL4e != "-" && sL3e == "-") || (sL3e != "-" && sL2e == "-") || (sL2e != "-" && sL1e == "-"))
                            {


                                this.api.Notifications.AddMessage("Please set the approval sequence properly ");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 185;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);


                                return;
                            }


                            if ((sL1e != "-" && (sL1e == sL2e || sL1e == sL3e || sL1e == sL4e)) ||
                                (sL2e != "-" && (sL2e == sL3e || sL1e == sL4e)) ||
                                (sL3e != "-" && (sL3e == sL4e)))
                            {


                                this.api.Notifications.AddMessage("Improper approval sequence- make sure no approvers are repeated in the sequence");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 185;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                return;
                            }




                            // finding assigned to person
                            {

                                CPersonQuery appr1 = api.Person.NewPersonQuery();
                                appr1.IgnorePermissions = true;
                                appr1.AddSelect("Person.sFullName");
                                appr1.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo);

                                DataSet Dpers1 = appr1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    Assignedto = Convert.ToString(Dpers1.Tables[0].Rows[0]["sFullName"]);
                                }

                            }

                            //at levels
                            {

                                if (sL1e == Assignedto)
                                {
                                    atlevel = 1;
                                }

                                else if (sL2e == Assignedto)
                                {
                                    atlevel = 2;
                                }

                                else if (sL3e == Assignedto)
                                {
                                    atlevel = 3;
                                }

                                else if (sL4e == Assignedto)
                                {
                                    atlevel = 4;
                                }

                                else if ((sL1e != Assignedto) || (sL2e != Assignedto) || (sL3e != Assignedto) || (sL4e != Assignedto))
                                {
                                    this.api.Notifications.AddMessage(" The'Assigned to' person must be in approval sequence ");
                                    bug.ixStatus = 185;
                                    bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                    return;
                                }


                                // updating atlevel
                                string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", atlevel);

                            } //updating atlevel ends here

                            int i = 0;

                            {


                                if (i == 1)
                                {
                                    bug.ixStatus = 185;
                                    bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                    return;
                                }

                                string vendor_3 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                                string amt = (bug.GetPluginField(PLUGIN_ID, "TotalAmount")).ToString();
                                string title = bug.sTitle.ToString();
                                {

                                    CPersonQuery pers = api.Person.NewPersonQuery();
                                    pers.IgnorePermissions = true;
                                    pers.AddSelect("*");
                                    pers.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo.ToString());

                                    DataSet Dpers = pers.GetDataSet();

                                    if (Dpers.Tables.Count > 0 && Dpers.Tables[0] != null && Dpers.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers.Tables[0].Rows[0]["sEmail"]);

                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' awaiting your approval";
                                        mailsub = "A PO is awaiting your approval for vendor : " + vendor_3 + ", amount: $" + amt.ToString() + " and description :" + title;
                                        // mailbody = "There is an PO requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        //  mailbody = "There is a PO requiring your attention.  Please click on the below link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        mailbody = "A PO is awaiting for your approval for vendor: " + vendor_3 + " and amount: $" + amt.ToString() + ". Please click on the link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        iperson = bug.ixPersonAssignedTo;
                                        mailsender_Syn(semail1, bug, mailsub, mailbody, iperson);
                                        MailBody = mailbody;
                                        MailSub = mailsub;
                                        ixperson = iperson;
                                        flag = 1;

                                        //if ((bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")) != null)
                                        //{
                                        //    if (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013").ToString() != "")
                                        //    {

                                        //        string CCemail = (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")).ToString();
                                        //        mailsender(CCemail.Trim(), bug, mailsub, mailbody, iperson);
                                        //        this.api.Notifications.AddMessage("A CCEmail has been sent Successfully");
                                        //    }
                                        //}
                                        this.api.Notifications.AddMessage("An email has been sent to the approver successfully");


                                    }

                                }

                                {

                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.ixPerson = " + bug.ixPersonLastEditedBy.ToString());

                                    DataSet Dpers1 = pers1.GetDataSet();

                                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' has sent for Approval";
                                        mailsub = "A PO is sent for Approval for Vendor:" + vendor_3 + ", amount: $" + amt.ToString() + ", description :" + title;
                                        mailbody = "There is a PO Sent for Approval for Vendor:" + vendor_3 + ", amount: $" + amt.ToString() + ", description :" + title + ". Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;
                                        //mailbody += System.Environment.NewLine;
                                        //mailbody += System.Environment.NewLine;
                                        iperson = bug.ixPersonLastEditedBy;
                                        mailsender_Syn(semail1, bug, mailsub, mailbody, iperson);

                                    }

                                }

                            } // sending emails ends here


                        }
                    }

                    if (nBugAction == BugAction.Resolve)
                    {

                        string UserStatus = (bug.GetPluginField(PLUGIN_ID, "CWFUserResolve")).ToString().Trim();
                        //api.Notifications.AddAdminNotification("statusR", UserStatus.ToString());

                        iperson = 0;
                        // string Vendor_Name1 = "";
                        // string Invoice1 = "";

                        int L = 0;
                        int L_openr = 0;
                        int L0 = 0;
                        int L1 = 0;
                        int L2 = 0;
                        int L3 = 0;
                        int L4 = 0;

                        string sL1 = "-";
                        string sL2 = "-";
                        string sL3 = "-";
                        string sL4 = "-";

                        string Lmail = "-";
                        //  string L0mail = "-";
                        string L1mail = "-";
                        string L2mail = "-";
                        string L3mail = "-";
                        string L4mail = "-";


                        if (bug.ixProject != 26)
                        {
                            return;

                        }


                        // fetching approvers details

                        // L = bug.ixPersonLastEditedBy;

                        L = bug.ixPersonOpenedBy;
                        L_openr = bug.ixPersonOpenedBy;

                        //  this.api.Notifications.AddMessage("L||" + L);

                        L0 = bug.ixPersonResolvedBy;
                        {
                            CPersonQuery intL1 = api.Person.NewPersonQuery();
                            intL1.IgnorePermissions = true;
                            intL1.AddSelect("sEmail");
                            intL1.AddWhere(" Person.ixPerson = " + L_openr);

                            DataSet Dpers1 = intL1.GetDataSet();

                            if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                            {
                                //L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                Lmail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                            }

                        }
                        sL1 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                        sL2 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                        sL3 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                        sL4 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                        {



                            // fetching L1 approver deatils
                            if (sL1 != "-")
                            {
                                CPersonQuery intL1 = api.Person.NewPersonQuery();
                                intL1.IgnorePermissions = true;
                                intL1.AddSelect("Person.ixPerson,Person.sEmail");
                                intL1.AddWhere(" Person.sFullName = " + "'" + sL1 + "'");

                                DataSet Dpers1 = intL1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    L1 = Convert.ToInt32(Dpers1.Tables[0].Rows[0]["ixPerson"]);
                                    L1mail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L2 approver deatils



                            if (sL2 != "-")
                            {
                                CPersonQuery intL2 = api.Person.NewPersonQuery();
                                intL2.IgnorePermissions = true;
                                intL2.AddSelect("Person.ixPerson,Person.sEmail");
                                intL2.AddWhere(" Person.sFullName = " + "'" + sL2 + "'");

                                DataSet Dpers2 = intL2.GetDataSet();

                                if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                {
                                    L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                    L2mail = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }
                            // fetching L3 approver deatils
                            if (sL3 != "-")
                            {
                                CPersonQuery intL3 = api.Person.NewPersonQuery();
                                intL3.IgnorePermissions = true;
                                intL3.AddSelect("Person.ixPerson,Person.sEmail");
                                intL3.AddWhere(" Person.sFullName = " + "'" + sL3 + "'");

                                DataSet Dpers3 = intL3.GetDataSet();

                                if (Dpers3.Tables.Count > 0 && Dpers3.Tables[0] != null && Dpers3.Tables[0].Rows.Count > 0)
                                {
                                    L3 = Convert.ToInt32(Dpers3.Tables[0].Rows[0]["ixPerson"]);
                                    L3mail = Convert.ToString(Dpers3.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L4 approver deatils
                            if (sL4 != "-")
                            {
                                CPersonQuery intL4 = api.Person.NewPersonQuery();
                                intL4.IgnorePermissions = true;
                                intL4.AddSelect("Person.ixPerson,Person.sEmail");
                                intL4.AddWhere(" Person.sFullName = " + "'" + sL4 + "'");

                                DataSet Dpers4 = intL4.GetDataSet();

                                if (Dpers4.Tables.Count > 0 && Dpers4.Tables[0] != null && Dpers4.Tables[0].Rows.Count > 0)
                                {
                                    L4 = Convert.ToInt32(Dpers4.Tables[0].Rows[0]["ixPerson"]);
                                    L4mail = Convert.ToString(Dpers4.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            //  this.api.Notifications.AddMessage(L1 + "|" + sL2 + L2 + "|" + sL3 + L3 + "|" + sL4 + L4);
                        }

                        string vendor_3 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                        string amt = (bug.GetPluginField(PLUGIN_ID, "TotalAmount")).ToString();
                        string title = bug.sTitle.ToString();
                        // this.api.Notifications.AddMessage("A1");

                        if (UserStatus == "Rejected")
                        {
                            bug.ixStatus = 184;
                        }

                        if (bug.ixStatus == 180)
                        {
                            // this.api.Notifications.AddMessage("resolve-1");

                            vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();


                            string Appr_mailsub = "A PO is awaiting your approval for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + " and description :" + title;
                            //  string Appr_mailbody = "There is a PO requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                            // string Appr_mailbody = "There is a PO requiring your attention.  Please click on the link below and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                            string Appr_mailbody = "A PO is awaiting for your approval for vendor: " + vendor_3 + " and amount: $" + amt.ToString() + ". Please click on the link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;




                            if (sL1 != "-")
                            {
                                // this.api.Notifications.AddMessage("resolve-2");
                                if (L1 == L0)
                                {
                                    // this.api.Notifications.AddMessage("resolve-3");
                                    if (sL2 != "-")
                                    {
                                        // this.api.Notifications.AddMessage("resolve-4");
                                        // this.api.Notifications.AddMessage("L1 level");
                                        //this.api.Notifications.AddMessage("5");
                                        this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                        bug.ixPersonAssignedTo = L2;
                                        bug.ixStatus = 183;
                                        bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");
                                        //updating atlevel
                                        //   string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 3);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                        mailsender_Syn(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                        //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                    }
                                    else
                                    {
                                        // this.api.Notifications.AddMessage("resolve-5");
                                        //  this.api.Notifications.AddMessage("L2_1 level");
                                        //  this.api.Notifications.AddMessage("31");
                                        //  this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");
                                        //  string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 5);

                                        // this.api.Notifications.AddMessage("assgined to email||" + Lmail);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                        // mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                        //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                        bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                        // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                    }
                                }




                                else if (sL2 != "-")
                                {
                                    //this.api.Notifications.AddMessage("3");

                                    if (L2 == L0)
                                    {
                                        // this.api.Notifications.AddMessage("4");
                                        if (sL3 != "-")
                                        {
                                            // this.api.Notifications.AddMessage("L2 level");
                                            //this.api.Notifications.AddMessage("5");
                                            this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                            bug.ixPersonAssignedTo = L3;
                                            bug.ixStatus = 183;
                                            bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");
                                            //updating atlevel
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                            mailsender_Syn(L3mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                            //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                        }
                                        else
                                        {
                                            //  this.api.Notifications.AddMessage("L2_1 level");
                                            //  this.api.Notifications.AddMessage("9");
                                            //  this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            // Update1_1.UpdateInt("ixAtlevel", 5);

                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                            // this.api.Notifications.AddMessage("ixPersonLastEditedBy||" + bug.ixPersonLastEditedBy);
                                            // this.api.Notifications.AddMessage("assgined to email||" + Lmail);
                                            //  mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                            // bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                            bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                            //this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                        }
                                    }



                                    else if (sL3 != "-")
                                    {
                                        if (L3 == L0)
                                        {
                                            // this.api.Notifications.AddMessage("L3 level");
                                            if (sL4 != "-")
                                            {
                                                this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                                //  this.api.Notifications.AddMessage("SL4|" + sL4);
                                                // this.api.Notifications.AddMessage("L4|" + L4);
                                                bug.ixPersonAssignedTo = L4;
                                                bug.ixStatus = 183;
                                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //Update1_1.UpdateInt("ixAtlevel", 4);
                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                                mailsender_Syn(L4mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                                //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                            }
                                            else
                                            {
                                                //  this.api.Notifications.AddMessage("L3_1 level");
                                                // this.api.Notifications.AddMessage("8");
                                                // this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                // Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                //  mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                            }
                                        }


                                        else if (sL4 != "-")
                                        {

                                            if (L4 == L0)
                                            {
                                                // this.api.Notifications.AddMessage("L4 level");

                                                // this.api.Notifications.AddMessage("9");
                                                // this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //  Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                // mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //  bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                //  this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);

                                            }
                                        }
                                    }
                                }
                            }

                        }
                        {
                            int Atlevel = 0;

                            CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                            Qrynewpon.AddSelect("ixAtlevel");
                            string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                            Qrynewpon.AddWhere(sWhere2);
                            object level = Qrynewpon.GetScalarValue();
                            Atlevel = Convert.ToInt32(level);

                            { // assiging to Finance review

                                if (UserStatus == "Rejected")
                                {
                                    bug.ixStatus = 184;
                                }
                                if (bug.ixStatus == 180)
                                {
                                   // api.Notifications.AddAdminNotification("status1", bug.ixProject.ToString());
                                   // api.Notifications.AddAdminNotification("status2", bug.ixStatus.ToString());
                                    if (bug.ixPersonResolvedBy != 299)
                                    {
                                      //  api.Notifications.AddAdminNotification("ixPersonResolvedBy", bug.ixPersonResolvedBy.ToString());
                                        this.api.Notifications.AddMessage("The PO has been approved and sent to Finance team for review");
                                        string Finance_mailsub = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                                        string Finance_mailbody = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + ". Click on the link to review the PO information and generate Purchase Order http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        //  string Proc_mailbody = "The PO you submitted has been approved and new PO number " + PONumb + " has been created, accounting team will share the purchase order copy shortly.";
                                        mailsender_Finance("poornima.r@conseroglobal.com", bug, Finance_mailsub, Finance_mailbody, iperson);
                                        bug.ixStatus = 183;
                                       // api.Notifications.AddAdminNotification("status3", bug.ixProject.ToString());
                                        bug.ixPersonAssignedTo = 299;
                                       // api.Notifications.AddAdminNotification("ixPersonResolvedBy", bug.ixPersonAssignedTo.ToString());
                                    }

                                }
                            }


                            if (bug.ixPersonResolvedBy == 299) //PO generation
                            {
                                if (UserStatus == "Rejected")
                                {
                                    bug.ixStatus = 184;
                                }

                                if (bug.ixStatus == 180)
                                {
                                    int userId = bug.ixPersonOpenedBy;
                                    string mailID = "";
                                    CPersonQuery ixper = api.Person.NewPersonQuery();
                                    ixper.IgnorePermissions = true;
                                    ixper.AddSelect("Person.sEmail");
                                    ixper.AddWhere(" Person.ixPerson = " + userId);

                                    DataSet Dpers2 = ixper.GetDataSet();
                                    string PONumb = "";
                                    string sPONum_exist = "";
                                    CSelectQuery PONum_exist = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                                    PONum_exist.AddSelect("(PONumberArt) as PONumber");
                                    string sWhere1 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                                    PONum_exist.AddWhere(sWhere1);
                                    DataSet Synds_1 = PONum_exist.GetDataSet();

                                    if (null != Synds_1.Tables && Synds_1.Tables.Count == 1 && Synds_1.Tables[0].Rows.Count == 1)
                                    {
                                        sPONum_exist = Convert.ToString(Convert.ToString(Synds_1.Tables[0].Rows[0]["PONumber"]));


                                        if (sPONum_exist == "" || sPONum_exist == null)
                                        {
                                           // api.Notifications.AddAdminNotification("status1", sPONum_exist.ToString());
                                            int PONum = 0;
                                            CSelectQuery PONumber = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                                            PONumber.AddSelect("MAX(PONumberArt) as PONumber");
                                            //string sWhere1 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                                            //PONumber.AddWhere(sWhere1);
                                            DataSet Synds = PONumber.GetDataSet();
                                            api.Notifications.AddAdminNotification("i1", "i1");
                                            if (null != Synds.Tables && Synds.Tables.Count == 1 && Synds.Tables[0].Rows.Count == 1)
                                            {


                                               // api.Notifications.AddAdminNotification("i2", "i2");
                                                PONum = Convert.ToInt32(Convert.ToString(Synds.Tables[0].Rows[0]["PONumber"]));
                                                int i = Convert.ToInt32(PONum) + 1;
                                               // api.Notifications.AddAdminNotification("i3", i.ToString());
                                                bug.SetPluginField(PLUGIN_ID, "PONumberArt", i.ToString());
                                                PONumb = (bug.GetPluginField(PLUGIN_ID, "PONumberArt")).ToString().Trim();
                                                //  PONumb = (bug.GetPluginField(PLUGIN_ID, "PONumber")).ToString().Trim();
                                                if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                                {
                                                    mailID = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                                    string Proc_mailsub = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                                                    //  string Proc_mailbody = "The PO you submitted has been approved and new PO number " + PONumb + " has been created, accounting team will share the purchase order copy shortly.";
                                                    string Proc_mailbody = "The new PO you submitted has been approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + " and reviewed by Finance team. The PO number PO-1-0" + PONumb + " has been created. To access the PO please click on the link http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                                    mailsender_Syn(mailID, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                    //  this.api.Notifications.AddMessage("The PO you submitted has been approved and reviewed by Finance team");
                                                }
                                            }
                                        }

                                    }//do nothing

                                }
                            }

                            //}


                        }



                        // For Rejection 
                        if (UserStatus == "Rejected")
                        {
                            bug.ixStatus = 184;
                        }
                        if (bug.ixStatus == 184)
                        {


                            if (bugevent.s == "")
                            {

                                this.api.Notifications.AddMessage("                                                                                                               ");
                                this.api.Notifications.AddMessage("                                                                                                             ");
                                this.api.Notifications.AddMessage("                                                                                                              ");
                                this.api.Notifications.AddMessage("                                                                                                            ");
                                this.api.Notifications.AddError("                                                       -                                      ");
                                this.api.Notifications.AddError("-----------------------------------Error Message------------------------------");
                                this.api.Notifications.AddError("You cannot reject a case without any reason");
                                this.api.Notifications.AddError("Please reject the case again with an appropraite reason entered in the comment box");

                                this.api.Notifications.AddMessage("--------------------------------------------------------------------------------------");

                                this.api.Notifications.AddMessage("----------------------------------   -SOLUTION-  -------------------------------------");
                                this.api.Notifications.AddMessage("                     To Reject the case");
                                this.api.Notifications.AddMessage("                     Click on resolve button");
                                this.api.Notifications.AddMessage("                     Set the status to 'Rejected' ");
                                this.api.Notifications.AddMessage("                     Enter your reasons of rejections in the comment box");
                                this.api.Notifications.AddMessage("                     Click 'Resolve' Button");
                                this.api.Notifications.AddMessage("-----------------------------------------------------------------------------");
                                this.api.Notifications.AddError("-Error--------Error---------End of Error Message----------Error----------Error-");
                                bug.ixPersonAssignedTo = bugevent.ixPerson;
                                //bugevent.ixPersonAssignedTo = bugevent.ixPerson;
                                bug.ixPersonAssignedTo = bug.ixPersonResolvedBy;
                                bug.ixStatus = 183;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Discrepancy");
                                return;
                            }




                            //Finding the level of rejection and sendin email accordingly
                            //string RL0mail = "-";
                            string RL1mail = "-";
                            // string RL2mail = "-";
                            //string RL3mail = "-";
                            // string RL4mail = "-";

                            vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                            string Rej_mailsub = "The PO has been Rejected for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                            string Rej_mailbody = "The PO has been rejected for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + ". Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;


                            {
                                CPersonQuery rejL1 = api.Person.NewPersonQuery();
                                rejL1.IgnorePermissions = true;
                                rejL1.AddSelect("sEmail");
                                rejL1.AddWhere(" Person.ixPerson = " + bug.ixPersonOpenedBy);

                                DataSet Dpers5 = rejL1.GetDataSet();

                                if (Dpers5.Tables.Count > 0 && Dpers5.Tables[0] != null && Dpers5.Tables[0].Rows.Count > 0)
                                {
                                    //L4 = Convert.ToInt32(Dpers5.Tables[0].Rows[0]["ixPerson"]);
                                    RL1mail = Convert.ToString(Dpers5.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                                mailsender_Syn(RL1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //  this.api.Notifications.AddMessage("processor | " + RL1mail);
                            }



                            if (L0 == L1)
                            {
                                // api.Notifications.AddMessage("executed at L1 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 1);


                                // mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //   mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //this.api.Notifications.AddMessage("Second assignee| " + L1mail);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");

                            }

                            if (L0 == L2)
                            {
                                //api.Notifications.AddMessage("executed at L2 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");


                            }


                            else if (L0 == L3)
                            {
                                // api.Notifications.AddMessage("executed at L3 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                //   mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");
                            }



                            else if (L0 == L4)
                            {
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                //   api.Notifications.AddMessage("executed at L4 level");
                                // this.api.Notifications.AddMessage("Fourth Approver| " + L1mail + "||" + L2mail + "||" + L3mail);
                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");

                            }

                            else if (L0 == 299)
                            {
                                mailsender_Syn(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Syn(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected and an email notification sent to the requestor");
                            }




                        }

                    }

                }
            }
            #endregion

            //------------------------------------------------Synergis for MLA Workflow End----------------------------------

            ////////////////////////Spreadfast////////////////////////////////////////////////////////////////////////////////

            # region Spreadfast for MLA workflow begin


            {

                //capturing  and storing atlevel and levels

                string status1 = "Discrepancy ";

                if (bug.ixProject == 27)
                {

                    if (nBugAction == BugAction.Edit || nBugAction == BugAction.Assign)
                    {

                        if (bug.ixStatus == 194)
                        {

                            // this.api.Notifications.AddMessage("BCE-1");

                            string sL1e = "-";
                            string sL2e = "-";
                            string sL3e = "-";
                            string sL4e = "-";
                            int atlevel = 0;



                            string Assignedto = "";

                            CSelectQuery approvers = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                            approvers.AddSelect("CWFApproverl1,CWFApproverl2,CWFApproverl3,CWFApproverl4");
                            approvers.AddWhere("ixBug = " + bug.ixBug.ToString());

                            DataSet ds_1 = approvers.GetDataSet();

                            if (null != ds_1.Tables && ds_1.Tables.Count == 1 && ds_1.Tables[0].Rows.Count == 1)
                            {

                                sL1e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                                sL2e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                                sL3e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                                sL4e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                                //  this.api.Notifications.AddMessage(sL1e_1 + "||" + sL1e );

                            }

                            //checking Approval sequence

                            if (sL1e == "-" && sL2e == "-" && sL3e == "-" && sL4e == "-")
                            {


                                this.api.Notifications.AddMessage(sL1e + "|" + sL2e + "|" + sL3e + "|" + sL4e);
                                this.api.Notifications.AddMessage("Please set atleast one approver in the approavl sequence ");
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                string stat = (bug.GetPluginField(PLUGIN_ID, "CWFUserStatus")).ToString().Trim();
                                // this.api.Notifications.AddMessage(stat);
                                bug.ixStatus = 195;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1.ToString());

                                string stat1 = (bug.GetPluginField(PLUGIN_ID, "CWFUserStatus")).ToString().Trim();
                                //  this.api.Notifications.AddMessage(stat1);
                                return;
                            }


                            if ((sL4e != "-" && sL3e == "-") || (sL3e != "-" && sL2e == "-") || (sL2e != "-" && sL1e == "-"))
                            {


                                this.api.Notifications.AddMessage("Please set the approval sequence properly ");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 195;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);


                                return;
                            }


                            if ((sL1e != "-" && (sL1e == sL2e || sL1e == sL3e || sL1e == sL4e)) ||
                                (sL2e != "-" && (sL2e == sL3e || sL1e == sL4e)) ||
                                (sL3e != "-" && (sL3e == sL4e)))
                            {


                                this.api.Notifications.AddMessage("Improper approval sequence- make sure no approvers are repeated in the sequence");
                                //change by poornima
                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                this.api.Notifications.AddMessage("email is not sent to the approvers ");
                                bug.ixStatus = 195;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                return;
                            }




                            // finding assigned to person
                            {

                                CPersonQuery appr1 = api.Person.NewPersonQuery();
                                appr1.IgnorePermissions = true;
                                appr1.AddSelect("Person.sFullName");
                                appr1.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo);

                                DataSet Dpers1 = appr1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    Assignedto = Convert.ToString(Dpers1.Tables[0].Rows[0]["sFullName"]);
                                }

                            }

                            //at levels
                            {

                                if (sL1e == Assignedto)
                                {
                                    atlevel = 1;
                                }

                                else if (sL2e == Assignedto)
                                {
                                    atlevel = 2;
                                }

                                else if (sL3e == Assignedto)
                                {
                                    atlevel = 3;
                                }

                                else if (sL4e == Assignedto)
                                {
                                    atlevel = 4;
                                }

                                else if ((sL1e != Assignedto) || (sL2e != Assignedto) || (sL3e != Assignedto) || (sL4e != Assignedto))
                                {
                                    this.api.Notifications.AddMessage(" The'Assigned to' person must be in approval sequence ");
                                    bug.ixStatus = 195;
                                    bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                    return;
                                }


                                // updating atlevel
                                string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", atlevel);

                            } //updating atlevel ends here

                            int i = 0;

                            {


                                if (i == 1)
                                {
                                    bug.ixStatus = 195;
                                    bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", status1);
                                    return;
                                }

                                string vendor_3 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                                string amt = (bug.GetPluginField(PLUGIN_ID, "Add_Fld2")).ToString();
                                string title = bug.sTitle.ToString();
                                {

                                    CPersonQuery pers = api.Person.NewPersonQuery();
                                    pers.IgnorePermissions = true;
                                    pers.AddSelect("*");
                                    pers.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo.ToString());

                                    DataSet Dpers = pers.GetDataSet();

                                    if (Dpers.Tables.Count > 0 && Dpers.Tables[0] != null && Dpers.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers.Tables[0].Rows[0]["sEmail"]);

                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' awaiting your approval";
                                        mailsub = "A PO is awaiting your approval for vendor : " + vendor_3 + ", amount: $" + amt.ToString() + " and description :" + title;
                                        // mailbody = "There is an PO requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        //  mailbody = "There is a PO requiring your attention.  Please click on the below link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        mailbody = "A PO is awaiting for your approval for vendor: " + vendor_3 + " , amount: $" + amt.ToString() + ". Please click on the link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                        iperson = bug.ixPersonAssignedTo;
                                        mailsender_Spreadfast(semail1, bug, mailsub, mailbody, iperson);
                                        MailBody = mailbody;
                                        MailSub = mailsub;
                                        ixperson = iperson;
                                        flag = 1;

                                        this.api.Notifications.AddMessage("An email has been sent to the approver successfully");

                                    }

                                }

                                {

                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.ixPerson = " + bug.ixPersonLastEditedBy.ToString());

                                    DataSet Dpers1 = pers1.GetDataSet();

                                    if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                    {
                                        string semail1 = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                        //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' has sent for Approval";
                                        mailsub = "A PO is sent for Approval for Vendor:" + vendor_3 + ", amount: $" + amt.ToString() + ", description :" + title;
                                        mailbody = "There is a PO Sent for Approval for Vendor:" + vendor_3 + ", amount: $" + amt.ToString() + ", description :" + title + ". Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;
                                        //mailbody += System.Environment.NewLine;
                                        //mailbody += System.Environment.NewLine;
                                        iperson = bug.ixPersonLastEditedBy;
                                        mailsender_Spreadfast(semail1, bug, mailsub, mailbody, iperson);

                                    }

                                }

                            } // sending emails ends here


                        }
                    }

                    if (nBugAction == BugAction.Resolve)
                    {

                        string UserStatus = (bug.GetPluginField(PLUGIN_ID, "CWFUserResolve")).ToString().Trim();
                      //  api.Notifications.AddAdminNotification("statusR", UserStatus.ToString());

                        iperson = 0;
                        // string Vendor_Name1 = "";
                        // string Invoice1 = "";

                        int L = 0;
                        int L_openr = 0;
                        int L0 = 0;
                        int L1 = 0;
                        int L2 = 0;
                        int L3 = 0;
                        int L4 = 0;

                        string sL1 = "-";
                        string sL2 = "-";
                        string sL3 = "-";
                        string sL4 = "-";

                        string Lmail = "-";
                        //  string L0mail = "-";
                        string L1mail = "-";
                        string L2mail = "-";
                        string L3mail = "-";
                        string L4mail = "-";


                        if (bug.ixProject != 27)
                        {
                            return;

                        }


                        // fetching approvers details

                        // L = bug.ixPersonLastEditedBy;

                        L = bug.ixPersonOpenedBy;
                        L_openr = bug.ixPersonOpenedBy;

                        //  this.api.Notifications.AddMessage("L||" + L);

                        L0 = bug.ixPersonResolvedBy;
                        {
                            CPersonQuery intL1 = api.Person.NewPersonQuery();
                            intL1.IgnorePermissions = true;
                            intL1.AddSelect("sEmail");
                            intL1.AddWhere(" Person.ixPerson = " + L_openr);

                            DataSet Dpers1 = intL1.GetDataSet();

                            if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                            {
                                //L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                Lmail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                            }

                        }
                        sL1 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                        sL2 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                        sL3 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                        sL4 = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();

                        {



                            // fetching L1 approver deatils
                            if (sL1 != "-")
                            {
                                CPersonQuery intL1 = api.Person.NewPersonQuery();
                                intL1.IgnorePermissions = true;
                                intL1.AddSelect("Person.ixPerson,Person.sEmail");
                                intL1.AddWhere(" Person.sFullName = " + "'" + sL1 + "'");

                                DataSet Dpers1 = intL1.GetDataSet();

                                if (Dpers1.Tables.Count > 0 && Dpers1.Tables[0] != null && Dpers1.Tables[0].Rows.Count > 0)
                                {
                                    L1 = Convert.ToInt32(Dpers1.Tables[0].Rows[0]["ixPerson"]);
                                    L1mail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L2 approver deatils



                            if (sL2 != "-")
                            {
                                CPersonQuery intL2 = api.Person.NewPersonQuery();
                                intL2.IgnorePermissions = true;
                                intL2.AddSelect("Person.ixPerson,Person.sEmail");
                                intL2.AddWhere(" Person.sFullName = " + "'" + sL2 + "'");

                                DataSet Dpers2 = intL2.GetDataSet();

                                if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                {
                                    L2 = Convert.ToInt32(Dpers2.Tables[0].Rows[0]["ixPerson"]);
                                    L2mail = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }
                            // fetching L3 approver deatils
                            if (sL3 != "-")
                             {
                                CPersonQuery intL3 = api.Person.NewPersonQuery();
                                intL3.IgnorePermissions = true;
                                intL3.AddSelect("Person.ixPerson,Person.sEmail");
                                intL3.AddWhere(" Person.sFullName = " + "'" + sL3 + "'");

                                DataSet Dpers3 = intL3.GetDataSet();

                                if (Dpers3.Tables.Count > 0 && Dpers3.Tables[0] != null && Dpers3.Tables[0].Rows.Count > 0)
                                {
                                    L3 = Convert.ToInt32(Dpers3.Tables[0].Rows[0]["ixPerson"]);
                                    L3mail = Convert.ToString(Dpers3.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            // fetching L4 approver deatils
                            if (sL4 != "-")
                            {
                                CPersonQuery intL4 = api.Person.NewPersonQuery();
                                intL4.IgnorePermissions = true;
                                intL4.AddSelect("Person.ixPerson,Person.sEmail");
                                intL4.AddWhere(" Person.sFullName = " + "'" + sL4 + "'");

                                DataSet Dpers4 = intL4.GetDataSet();

                                if (Dpers4.Tables.Count > 0 && Dpers4.Tables[0] != null && Dpers4.Tables[0].Rows.Count > 0)
                                {
                                    L4 = Convert.ToInt32(Dpers4.Tables[0].Rows[0]["ixPerson"]);
                                    L4mail = Convert.ToString(Dpers4.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                            }

                            //  this.api.Notifications.AddMessage(L1 + "|" + sL2 + L2 + "|" + sL3 + L3 + "|" + sL4 + L4);
                        }

                        string vendor_3 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                        string amt = (bug.GetPluginField(PLUGIN_ID, "Add_Fld2")).ToString();
                        string title = bug.sTitle.ToString();
                        // this.api.Notifications.AddMessage("A1");

                        if (UserStatus == "Rejected")
                        {
                            bug.ixStatus = 193;
                        }

                        if (bug.ixStatus == 192)
                        {
                            // this.api.Notifications.AddMessage("resolve-1");

                            vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();


                            string Appr_mailsub = "A PO is awaiting your approval for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + " and description :" + title;
                            //  string Appr_mailbody = "There is a PO requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                            // string Appr_mailbody = "There is a PO requiring your attention.  Please click on the link below and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                            string Appr_mailbody = "A PO is awaiting for your approval for vendor: " + vendor_3 + " and amount: $" + amt.ToString() + ". Please click on the link and take necessary action http://empower.conseroglobal.com/default.asp?" + bug.ixBug;




                            if (sL1 != "-")
                            {
                                // this.api.Notifications.AddMessage("resolve-2");
                                if (L1 == L0)
                                {
                                    // this.api.Notifications.AddMessage("resolve-3");
                                    if (sL2 != "-")
                                    {
                                        // this.api.Notifications.AddMessage("resolve-4");
                                        // this.api.Notifications.AddMessage("L1 level");
                                        //this.api.Notifications.AddMessage("5");
                                        this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                        bug.ixPersonAssignedTo = L2;
                                        bug.ixStatus = 194;
                                        bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");
                                        //updating atlevel
                                        //   string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 3);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                        mailsender_Spreadfast(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                        //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                    }
                                    else
                                    {
                                        // this.api.Notifications.AddMessage("resolve-5");
                                        //  this.api.Notifications.AddMessage("L2_1 level");
                                        //  this.api.Notifications.AddMessage("31");
                                        //  this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");
                                        //  string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 5);

                                        // this.api.Notifications.AddMessage("assgined to email||" + Lmail);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                        // mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                        //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                        bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                        // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                    }
                                }




                                else if (sL2 != "-")
                                {
                                    //this.api.Notifications.AddMessage("3");

                                    if (L2 == L0)
                                    {
                                        // this.api.Notifications.AddMessage("4");
                                        if (sL3 != "-")
                                        {
                                            // this.api.Notifications.AddMessage("L2 level");
                                            //this.api.Notifications.AddMessage("5");
                                            this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                            bug.ixPersonAssignedTo = L3;
                                            bug.ixStatus = 194;
                                            bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");
                                            //updating atlevel
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                            mailsender_Spreadfast(L3mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                            //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                        }
                                        else
                                        {
                                            //  this.api.Notifications.AddMessage("L2_1 level");
                                            //  this.api.Notifications.AddMessage("9");
                                            //  this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");
                                            // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                            //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            // Update1_1.UpdateInt("ixAtlevel", 5);

                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                            // this.api.Notifications.AddMessage("ixPersonLastEditedBy||" + bug.ixPersonLastEditedBy);
                                            // this.api.Notifications.AddMessage("assgined to email||" + Lmail);
                                            //  mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                            // bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                            bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                            //this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                        }
                                    }



                                    else if (sL3 != "-")
                                    {
                                        if (L3 == L0)
                                        {
                                            // this.api.Notifications.AddMessage("L3 level");
                                            if (sL4 != "-")
                                            {
                                                this.api.Notifications.AddMessage("The PO has been approved and assigned to next approver successfully");
                                                //  this.api.Notifications.AddMessage("SL4|" + sL4);
                                                // this.api.Notifications.AddMessage("L4|" + L4);
                                                bug.ixPersonAssignedTo = L4;
                                                bug.ixStatus = 194;
                                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Pending Approval");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //Update1_1.UpdateInt("ixAtlevel", 4);
                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                                mailsender_Spreadfast(L4mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                                //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                            }
                                            else
                                            {
                                                //  this.api.Notifications.AddMessage("L3_1 level");
                                                // this.api.Notifications.AddMessage("8");
                                                // this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                // Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                //  mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                // this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                            }
                                        }


                                        else if (sL4 != "-")
                                        {

                                            if (L4 == L0)
                                            {
                                                // this.api.Notifications.AddMessage("L4 level");

                                                // this.api.Notifications.AddMessage("9");
                                                // this.api.Notifications.AddMessage("The PO has been approved successfully and an email notification sent to the requestor");

                                                // string tablename1 = api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA");
                                                //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //  Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);

                                                // mailsender_Syn(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                //  bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                //  this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);

                                            }
                                        }
                                    }
                                }
                            }

                        }
                        {
                            int Atlevel = 0;

                            CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                            Qrynewpon.AddSelect("ixAtlevel");
                            string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                            Qrynewpon.AddWhere(sWhere2);
                            object level = Qrynewpon.GetScalarValue();
                            Atlevel = Convert.ToInt32(level);

                            { // assiging to Finance review

                                if (UserStatus == "Rejected")
                                {
                                    bug.ixStatus = 193;
                                }
                                if (bug.ixStatus == 192)
                                {
                                    // api.Notifications.AddAdminNotification("status1", bug.ixProject.ToString());
                                    //  api.Notifications.AddAdminNotification("status2", bug.ixStatus.ToString());




                                    if (bug.ixPersonResolvedBy != 356)
                                    {

                                     int assignto = bug.ixPersonAssignedTo;
                                    string name = "";
                                    string email = "";


                                        bug.ixStatus = 194;
                                        bug.ixPersonAssignedTo = 356;
                                   // int assignto = bug.ixPersonAssignedTo;

                                    CPersonQuery intfinance = api.Person.NewPersonQuery();
                                    intfinance.IgnorePermissions = true;
                                    intfinance.AddSelect("Person.sFullname,Person.sEmail");
                                    intfinance.AddWhere(" Person.ixPerson = " + assignto);

                                    DataSet Dfinance = intfinance.GetDataSet();

                                    if (Dfinance.Tables.Count > 0 && Dfinance.Tables[0] != null && Dfinance.Tables[0].Rows.Count > 0)
                                    {
                                        name = Convert.ToString(Dfinance.Tables[0].Rows[0]["sFullname"]);
                                        email = Convert.ToString(Dfinance.Tables[0].Rows[0]["sEmail"]);
                                        api.Notifications.AddAdminNotification("email", email.ToString());
                                        //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                    }

                                    this.api.Notifications.AddMessage("The PO has been approved and sent to Finance team for review");
                                    string Finance_mailsub = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                                    string Finance_mailbody = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + ". Click on the link to review the PO information and generate Purchase Order http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                    //  string Proc_mailbody = "The PO you submitted has been approved and new PO number " + PONumb + " has been created, accounting team will share the purchase order copy shortly.";
                                    mailsender_Finance_SF(email, bug, Finance_mailsub, Finance_mailbody, iperson);


                                    }

                                }
                            }


                            if (bug.ixPersonResolvedBy == 356) //PO generation
                           {
                                if (UserStatus == "Rejected")
                                {
                                    bug.ixStatus = 193;
                                }

                                if (bug.ixStatus == 192)
                                {
                                    int userId = bug.ixPersonOpenedBy;
                                    string mailID = "";
                                    CPersonQuery ixper = api.Person.NewPersonQuery();
                                    ixper.IgnorePermissions = true;
                                    ixper.AddSelect("Person.sEmail");
                                    ixper.AddWhere(" Person.ixPerson = " + userId);

                                    DataSet Dpers2 = ixper.GetDataSet();

                                    //////////////////// Query for checking PO/Blanket///////////////////////////

                                    CSelectQuery CheckPO = api.Database.NewSelectQuery("Plugin_37_CustomBugData");
                                    CheckPO.AddSelect("typea718");
                                    CheckPO.AddWhere("ixBug = " + bug.ixBug.ToString());
                                   
                                    string type = "";

                                    DataSet checkPOB = CheckPO.GetDataSet();

                                    if (checkPOB.Tables.Count > 0 && checkPOB.Tables[0] != null && checkPOB.Tables[0].Rows.Count > 0)
                                    {

                                        type = Convert.ToString(checkPOB.Tables[0].Rows[0]["typea718"]);

                                    }
                                 //   api.Notifications.AddAdminNotification("type", type.ToString());
                                    if (type == "General PO")
                                    {
                                        string PONumb = "";
                                        string sPONum_exist = "";
                                        CSelectQuery PONum_exist = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                                        PONum_exist.AddSelect("(PO_Number) as PONumber");
                                        string sWhere1 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                                        PONum_exist.AddWhere(sWhere1);
                                        DataSet Synds_1 = PONum_exist.GetDataSet();

                                        if (null != Synds_1.Tables && Synds_1.Tables.Count == 1 && Synds_1.Tables[0].Rows.Count == 1)
                                        {
                                            sPONum_exist = Convert.ToString(Convert.ToString(Synds_1.Tables[0].Rows[0]["PONumber"]));


                                            if (sPONum_exist == "" || sPONum_exist == null)
                                            {
                                               // api.Notifications.AddAdminNotification("status1", sPONum_exist.ToString());
                                                int PONum = 0;
                                                CSelectQuery PONumber = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                                                PONumber.AddSelect("MAX(PO_Number) as PONumber");
                                                string sWhere = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixproject = " + bug.ixProject.ToString();
                                                PONumber.AddWhere(sWhere);
                                                DataSet Synds = PONumber.GetDataSet();
                                             //   api.Notifications.AddAdminNotification("i1", "i1");
                                                if (null != Synds.Tables && Synds.Tables.Count == 1 && Synds.Tables[0].Rows.Count == 1)
                                                {

                                                    PONum = Convert.ToInt32(Convert.ToString(Synds.Tables[0].Rows[0]["PONumber"]));
                                                    int i = Convert.ToInt32(PONum) + 1;
                                                    bug.SetPluginField(PLUGIN_ID, "PO_Number", i.ToString());
                                                    PONumb = (bug.GetPluginField(PLUGIN_ID, "PO_Number")).ToString().Trim();
                                                    if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                                    {
                                                        mailID = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                                        string Proc_mailsub = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                                                        //  string Proc_mailbody = "The PO you submitted has been approved and new PO number " + PONumb + " has been created, accounting team will share the purchase order copy shortly.";
                                                        string Proc_mailbody = "The new PO you submitted has been approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + " and reviewed by Finance team. The PO number PO-0" + PONumb + " has been created. To access the PO please click on the link http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                                        mailsender_Spreadfast(mailID, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                        //  this.api.Notifications.AddMessage("The PO you submitted has been approved and reviewed by Finance team");
                                                    }
                                                }
                                            }

                                        }//do nothing
                                    }

                                    else
                                    {
                                        string B_PONumb = "";
                                        string sBPONum_exist = "";
                                        CSelectQuery PONum_exist = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                                        PONum_exist.AddSelect("(B_PO_Number) as B_PONumber");
                                        string sWhere1 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + bug.ixBug.ToString();
                                        PONum_exist.AddWhere(sWhere1);
                                        DataSet Synds_1 = PONum_exist.GetDataSet();

                                        if (null != Synds_1.Tables && Synds_1.Tables.Count == 1 && Synds_1.Tables[0].Rows.Count == 1)
                                        {
                                            sBPONum_exist = Convert.ToString(Convert.ToString(Synds_1.Tables[0].Rows[0]["B_PONumber"]));


                                            if (sBPONum_exist == "" || sBPONum_exist == null)
                                            {
                                               // api.Notifications.AddAdminNotification("status1", sBPONum_exist.ToString());
                                                int PONum = 0;
                                                CSelectQuery PONumber = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                                                PONumber.AddSelect("MAX(B_PO_Number) as B_PONumber");
                                                string sWhere = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixproject = " + bug.ixProject.ToString();
                                                PONumber.AddWhere(sWhere);
                                                DataSet Synds = PONumber.GetDataSet();
                                             //   api.Notifications.AddAdminNotification("i1", "i1");
                                                if (null != Synds.Tables && Synds.Tables.Count == 1 && Synds.Tables[0].Rows.Count == 1)
                                                {

                                                    PONum = Convert.ToInt32(Convert.ToString(Synds.Tables[0].Rows[0]["B_PONumber"]));
                                                    int i = Convert.ToInt32(PONum) + 1;
                                                    bug.SetPluginField(PLUGIN_ID, "B_PO_Number", i.ToString());
                                                    B_PONumb = (bug.GetPluginField(PLUGIN_ID, "B_PO_Number")).ToString().Trim();
                                                    if (Dpers2.Tables.Count > 0 && Dpers2.Tables[0] != null && Dpers2.Tables[0].Rows.Count > 0)
                                                    {
                                                        mailID = Convert.ToString(Dpers2.Tables[0].Rows[0]["sEmail"]);
                                                        string Proc_mailsub = "A PO has been Approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                                                        //  string Proc_mailbody = "The PO you submitted has been approved and new PO number " + PONumb + " has been created, accounting team will share the purchase order copy shortly.";
                                                        string Proc_mailbody = "The new PO you submitted has been approved for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + " and reviewed by Finance team. The PO number B-PO-0" + B_PONumb + " has been created. To access the PO please click on the link http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                                        mailsender_Spreadfast(mailID, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                        //  this.api.Notifications.AddMessage("The PO you submitted has been approved and reviewed by Finance team");
                                                    }
                                                }
                                            }

                                        }//do nothing
                                    }

                                }
                            }

                  
                        }






                        // For Rejection 
                        if (UserStatus == "Rejected")
                        {
                            bug.ixStatus = 193;
                        }
                        if (bug.ixStatus == 193)
                        {


                            if (bugevent.s == "")
                            {

                                this.api.Notifications.AddMessage("                                                                                                               ");
                                this.api.Notifications.AddMessage("                                                                                                             ");
                                this.api.Notifications.AddMessage("                                                                                                              ");
                                this.api.Notifications.AddMessage("                                                                                                            ");
                                this.api.Notifications.AddError("                                                       -                                      ");
                                this.api.Notifications.AddError("-----------------------------------Error Message------------------------------");
                                this.api.Notifications.AddError("You cannot reject a case without any reason");
                                this.api.Notifications.AddError("Please reject the case again with an appropraite reason entered in the comment box");

                                this.api.Notifications.AddMessage("--------------------------------------------------------------------------------------");

                                this.api.Notifications.AddMessage("----------------------------------   -SOLUTION-  -------------------------------------");
                                this.api.Notifications.AddMessage("                     To Reject the case");
                                this.api.Notifications.AddMessage("                     Click on resolve button");
                                this.api.Notifications.AddMessage("                     Set the status to 'Rejected' ");
                                this.api.Notifications.AddMessage("                     Enter your reasons of rejections in the comment box");
                                this.api.Notifications.AddMessage("                     Click 'Resolve' Button");
                                this.api.Notifications.AddMessage("-----------------------------------------------------------------------------");
                                this.api.Notifications.AddError("-Error--------Error---------End of Error Message----------Error----------Error-");
                                bug.ixPersonAssignedTo = bugevent.ixPerson;
                                //bugevent.ixPersonAssignedTo = bugevent.ixPerson;
                                bug.ixPersonAssignedTo = bug.ixPersonResolvedBy;
                                bug.ixStatus = 194;
                                bug.SetPluginField(PLUGIN_ID, "CWFUserStatus", "Discrepancy");
                                return;
                            }




                            //Finding the level of rejection and sendin email accordingly
                            //string RL0mail = "-";
                            string RL1mail = "-";
                            // string RL2mail = "-";
                            //string RL3mail = "-";
                            // string RL4mail = "-";

                            vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                            string Rej_mailsub = "The PO has been Rejected for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title;
                            string Rej_mailbody = "The PO has been rejected for vendor : " + vendor_1 + ", amount: $" + amt.ToString() + ", description :" + title + ". Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;


                            {
                                CPersonQuery rejL1 = api.Person.NewPersonQuery();
                                rejL1.IgnorePermissions = true;
                                rejL1.AddSelect("sEmail");
                                rejL1.AddWhere(" Person.ixPerson = " + bug.ixPersonOpenedBy);

                                DataSet Dpers5 = rejL1.GetDataSet();

                                if (Dpers5.Tables.Count > 0 && Dpers5.Tables[0] != null && Dpers5.Tables[0].Rows.Count > 0)
                                {
                                    //L4 = Convert.ToInt32(Dpers5.Tables[0].Rows[0]["ixPerson"]);
                                    RL1mail = Convert.ToString(Dpers5.Tables[0].Rows[0]["sEmail"]);
                                    //   iperson = Convert.ToInt32(Dpers.Tables[0].Rows[0]["ixPerson"]);
                                }

                                mailsender_Spreadfast(RL1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //  this.api.Notifications.AddMessage("processor | " + RL1mail);
                            }



                            if (L0 == L1)
                            {
                                // api.Notifications.AddMessage("executed at L1 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 1);


                                // mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //   mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //this.api.Notifications.AddMessage("Second assignee| " + L1mail);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");

                            }

                            if (L0 == L2)
                            {
                                //api.Notifications.AddMessage("executed at L2 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Spreadfast(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");


                            }


                            else if (L0 == L3)
                            {
                                // api.Notifications.AddMessage("executed at L3 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                //   mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Spreadfast(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Spreadfast(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");
                            }



                            else if (L0 == L4)
                            {
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);

                                //   api.Notifications.AddMessage("executed at L4 level");
                                // this.api.Notifications.AddMessage("Fourth Approver| " + L1mail + "||" + L2mail + "||" + L3mail);
                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Spreadfast(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Spreadfast(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Spreadfast(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected successfully and an email notification sent to the requestor");

                            }

                            else if (L0 == 356)
                            {
                                mailsender_Spreadfast(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Spreadfast(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender_Spreadfast(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The PO has been rejected and an email notification sent to the requestor");
                            }




                        }

                    }

                }

            }
            #endregion

            /////////////////////////////// END //////////////////////////////////////////////////////////////////////////////



        }

        private bool ExtractValue(CBug bug, CBugEvent bugevent, string fieldName, string fieldDisplay)
        {
            bool valueChanged = false;

            string sNewValue = Convert.ToString(api.Request[api.AddPluginPrefix(fieldName)]);

            if (string.IsNullOrEmpty(sNewValue))
            {

            }
            else
            {
                string preCommitValue = Convert.ToString(bug.GetPluginField(PLUGIN_ID, fieldName));
                /* if the field changed, set the plugin field and record it in the BugEvent */
                if (sNewValue != preCommitValue)
                {
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
                return Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
        }


        #region IPluginDatabase Members

        public CTable[] DatabaseSchema()
        {

            CTable Invoiceheader = api.Database.NewTable(api.Database.PluginTableName("CGSInvoice_MLA"));
            Invoiceheader.sDesc = "Caputures Invoice Header Parameters with MLA";
            Invoiceheader.AddAutoIncrementPrimaryKey("ixCGSInvoiceNumber");
            Invoiceheader.AddIntColumn("ixBug", true, 1);
            Invoiceheader.AddVarcharColumn("CWFApproverl1", 200, false);
            Invoiceheader.AddVarcharColumn("CWFApproverl2", 200, false);
            Invoiceheader.AddVarcharColumn("CWFApproverl3", 200, false);
            Invoiceheader.AddVarcharColumn("CWFApproverl4", 200, false);
            Invoiceheader.AddVarcharColumn("CWFCustomform", 200, false);
            Invoiceheader.AddVarcharColumn("CWFVendor", 200, false);
            Invoiceheader.AddVarcharColumn("CWFCountry", 200, false);
            Invoiceheader.AddVarcharColumn("CWFCurrency", 200, false);
            Invoiceheader.AddVarcharColumn("CWFPostingperiod", 200, false);
            Invoiceheader.AddVarcharColumn("CWFSubsidiary", 200, false);
            Invoiceheader.AddVarcharColumn("CWFTerms", 200, false);
            Invoiceheader.AddVarcharColumn("sInvoiceNumber", 200, false);
            Invoiceheader.AddDateColumn("sInvoiceDate", false);
            Invoiceheader.AddDateColumn("sInvoiceEnteredDate", false);
            Invoiceheader.AddVarcharColumn("sExchangeRate", 200, false);
            Invoiceheader.AddFloatColumn("sInvoiceAmount", false);
            Invoiceheader.AddFloatColumn("sTaxAmount", false);
            Invoiceheader.AddDateColumn("sInvoiceDueDate", false);
            Invoiceheader.AddVarcharColumn("sMemo", 250, false);
            Invoiceheader.AddVarcharColumn("sAddInfo", 250, false);
            Invoiceheader.AddIntColumn("ixAtlevel", false, 0);
            Invoiceheader.AddIntColumn("ixlevel", false, 0);
            Invoiceheader.AddVarcharColumn("CCUser", 200, false);
            Invoiceheader.AddVarcharColumn("CWFCustomVal2", 200, false);
            Invoiceheader.AddVarcharColumn("CWFCustomVal3", 200, false);
            Invoiceheader.AddVarcharColumn("Remarks", 200, false);
            Invoiceheader.AddVarcharColumn("Netamount", 225, false);
            Invoiceheader.AddVarcharColumn("TotalAmount", 225, false);
            Invoiceheader.AddVarcharColumn("AccountDesc", 225, false);

            Invoiceheader.AddVarcharColumn("CWFUsercate", 225, false);
            Invoiceheader.AddVarcharColumn("CWFUserAssign", 225, false);
            Invoiceheader.AddVarcharColumn("CWFUserStatus", 225, false);
            Invoiceheader.AddIntColumn("PONumber", false, 0);
            Invoiceheader.AddVarcharColumn("CWFUserResolve", 225, false);

            Invoiceheader.AddVarcharColumn("Add_Fld1", 225, false);
            Invoiceheader.AddFloatColumn("Add_Fld2", false);
            Invoiceheader.AddVarcharColumn("Add_Fld3", 225, false);
            Invoiceheader.AddVarcharColumn("Add_Fld5", 225, false);
            Invoiceheader.AddVarcharColumn("Add_Fld6", 225, false);

            Invoiceheader.AddVarcharColumn("CWFLocation", 225, false);
            Invoiceheader.AddVarcharColumn("CWFDept", 225, false);
            Invoiceheader.AddVarcharColumn("DateString1", 225, false);
            Invoiceheader.AddVarcharColumn("DateString2", 225, false);

            Invoiceheader.AddIntColumn("PONumberArt", false, 0);
            Invoiceheader.AddVarcharColumn("PONumberArt_A", 225, false);

            Invoiceheader.AddFloatColumn("POBalanceAmt", false);
            Invoiceheader.AddFloatColumn("POAmt", false);
            Invoiceheader.AddIntColumn("ixproject", false, 0);

            Invoiceheader.AddIntColumn("PO_Number", false, 0);
            Invoiceheader.AddIntColumn("B_PO_Number", false, 0);
            Invoiceheader.AddIntColumn("B_PO_ref", false, 0);
            Invoiceheader.AddVarcharColumn("B_PO_Adden", 225, false);


            

            CTable Invoiceitems = api.Database.NewTable(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
            Invoiceitems.sDesc = "A table for CGSWF LineItems with MLA";
            Invoiceitems.AddAutoIncrementPrimaryKey("ixBugLineItem");
            Invoiceitems.AddIntColumn("ixBug", true, 1);
            Invoiceitems.AddTextColumn("sAccount", "Account no");
            //bugInvoiceItemsTable.AddIntColumn("iForm99", false);
            Invoiceitems.AddFloatColumn("fAmount", false);
            Invoiceitems.AddTextColumn("sTaxtype", "Tax type");
            Invoiceitems.AddFloatColumn("fTax", false);
            Invoiceitems.AddTextColumn("sMemo", "Memo Storage");
            Invoiceitems.AddTextColumn("sDepartment", "Account no");
            Invoiceitems.AddTextColumn("sBillable", "Billable");
            Invoiceitems.AddTextColumn("sAddninfo", "Additional Info");
            Invoiceitems.AddIntColumn("iDeleted", false, 0);
            Invoiceitems.AddIntColumn("ixExtra1", false);
            Invoiceitems.AddVarcharColumn("sExtra2", 200, false);
            Invoiceitems.AddFloatColumn("sExtra3", false);
            Invoiceitems.AddTextColumn("sExtra4", "extra4");
            Invoiceitems.AddTextColumn("sExtra5", "extra5");
            Invoiceitems.AddTextColumn("sExtra6", "extra6");
            Invoiceitems.AddIntColumn("ixProject", false, 0);
            Invoiceitems.AddIntColumn("ixLineItemId", false, 0);
            Invoiceitems.AddFloatColumn("LineBalanceAmt", false);
            Invoiceitems.AddFloatColumn("IvnBalanceAmt", false);
            //return new CTable[] { Invoiceheader, Invoiceitems };


            CTable MatchedInvoice = api.Database.NewTable(api.Database.PluginTableName("CGSPOMatchedInvoice"));
            MatchedInvoice.sDesc = "A table for PO Invoice Details";
            MatchedInvoice.AddAutoIncrementPrimaryKey("ixBugLineItem_PMI");
            MatchedInvoice.AddIntColumn("ixBug", true, 1);
            MatchedInvoice.AddVarcharColumn("sInvoiceNumber", 200, false);
            MatchedInvoice.AddDateColumn("sInvoiceDate", false);
            MatchedInvoice.AddFloatColumn("fAmount", false);
            MatchedInvoice.AddVarcharColumn("sMemo", 255, false);
            MatchedInvoice.AddVarcharColumn("sExtra1", 255, false);
            MatchedInvoice.AddFloatColumn("sExtra2", false);
            MatchedInvoice.AddIntColumn("ixProject", false);
            MatchedInvoice.AddIntColumn("ixLineItemId_Inv", false, 0);
            return new CTable[] { Invoiceheader, Invoiceitems, MatchedInvoice };
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
            gridCol1.sName = ":Invoice Number";
            /* the column title in grid view */
            gridCol1.sTitle = ":Invoice Number";
            /* every column you create needs to have a unique iType */
            gridCol1.iType = 0;

            CGridColumn gridCol2 = api.Grid.CreateGridColumn();
            gridCol2.sName = "Vendor Name";
            gridCol2.sTitle = "Vendor Name";
            /* every column you create needs to have a unique iType */
            gridCol2.iType = 1;

            CGridColumn gridCol3 = api.Grid.CreateGridColumn();
            gridCol3.sName = "Total Amount";
            gridCol3.sTitle = "Total Amount";
            /* every column you create needs to have a unique iType */
            gridCol3.iType = 2;

            CGridColumn gridCol4 = api.Grid.CreateGridColumn();
            gridCol4.sName = ":Invoice Date";
            gridCol4.sTitle = ":Invoice Date";
            /* every column you create needs to have a unique iType */
            gridCol4.iType = 3;

            CGridColumn gridCol5 = api.Grid.CreateGridColumn();
            gridCol5.sName = ":Invoice Due Date";
            gridCol5.sTitle = ":Invoice Due Date";
            /* every column you create needs to have a unique iType */
            gridCol5.iType = 4;

            CGridColumn gridCol6 = api.Grid.CreateGridColumn();
            gridCol6.sName = ":At level";
            gridCol6.sTitle = ":At level";
            /* every column you create needs to have a unique iType */
            gridCol6.iType = 5;

            CGridColumn gridCol7 = api.Grid.CreateGridColumn();
            gridCol7.sName = ":Document Type";
            gridCol7.sTitle =":Document Type";
            /* every column you create needs to have a unique iType */
            gridCol7.iType = 6; 

            CGridColumn gridCol8 = api.Grid.CreateGridColumn();
            gridCol8.sName = "PO Number";
            gridCol8.sTitle = "PO Number";
            /* every column you create needs to have a unique iType */
            gridCol8.iType = 7;

            CGridColumn gridCol9 = api.Grid.CreateGridColumn();
            gridCol9.sName = "Addendum PO Number";
            gridCol9.sTitle = "Addendum PO Number";
            /* every column you create needs to have a unique iType */
            gridCol9.iType = 8;

            CGridColumn gridCol10 = api.Grid.CreateGridColumn();
            gridCol10.sName = "PO number"; //PONumberArt
            gridCol10.sTitle = "PO number";
            /* every column you create needs to have a unique iType */
            gridCol10.iType = 9;

            CGridColumn gridCol11 = api.Grid.CreateGridColumn();
            gridCol11.sName = "Addendum PO number";
            gridCol11.sTitle = "Addendum PO number";
            /* every column you create needs to have a unique iType */
            gridCol11.iType = 10;

            CGridColumn gridCol12 = api.Grid.CreateGridColumn();
            gridCol12.sName = " PO Number.";
            gridCol12.sTitle = " PO Number.";
            /* every column you create needs to have a unique iType */
            gridCol12.iType = 11;

            CGridColumn gridCol13 = api.Grid.CreateGridColumn();
            gridCol13.sName = "Blanket PO Number";
            gridCol13.sTitle = "Blanket PO Number";
            /* every column you create needs to have a unique iType */
            gridCol13.iType = 12;

            CGridColumn gridCol14 = api.Grid.CreateGridColumn();
            gridCol14.sName = ".Total Amount";
            gridCol14.sTitle = ".Total Amount";
            /* every column you create needs to have a unique iType */
            gridCol14.iType = 13;

            CGridColumn gridCol15 = api.Grid.CreateGridColumn();
            gridCol15.sName = "BPO Reference";
            gridCol15.sTitle = "BPO Reference";
            /* every column you create needs to have a unique iType */
            gridCol15.iType = 14;

            return new CGridColumn[] { gridCol1, gridCol2, gridCol3, gridCol4, gridCol5, gridCol6, gridCol7, gridCol8, gridCol9, gridCol10, gridCol11, gridCol12, gridCol13, gridCol14, gridCol15 }; 
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
            
            string sTableColumn = "sInvoiceNumber";
            switch (col.iType)
            {
                case 0:
                    sTableColumn = "sInvoiceNumber";
                    break;
                case 1:
                    sTableColumn = "CWFVendor";
                    break;
                case 2:
                    sTableColumn = "TotalAmount";
                    break;
                case 3:
                    sTableColumn = "sInvoiceDate";
                    break;
                case 4:
                    sTableColumn = "sInvoiceDueDate";
                    break;
                case 5:
                    sTableColumn = "ixAtlevel";
                    break;
                case 6:
                    sTableColumn = "CWFCustomform";
                    break;
                case 7:
                    sTableColumn = "PONumber";
                    break;
                case 8:
                    sTableColumn = "Add_Fld1";
                    break;
                case 9:
                    sTableColumn = "PONumberArt";
                    break;
                case 10:
                    sTableColumn = "PONumberArt_A";
                    break;

                case 11:
                    sTableColumn = "PO_Number";
                    break;

                case 12:
                    sTableColumn = "B_PO_Number";
                    break;

                case 13:
                    sTableColumn = "Add_Fld2";
                    break;

                case 14:
                    sTableColumn = "B_PO_ref";
                    break;


            }
            string[] sValues = new string[rgBug.Length];

            for (int i = 0; i < rgBug.Length; i++)
            {
                /* For tables joined in IPluginBugJoin, use
                 * GetPluginField to fetch the values you need
                 * for the GridColumn. */
                if (sTableColumn == "PONumber" || sTableColumn == "Add_Fld1" )
                {
                    if (sTableColumn != null)
                    {

                        object pluginField = rgBug[i].GetPluginField(PLUGIN_ID, string.Format("{0}", sTableColumn));
                        sValues[i] = (pluginField == null) ?
                                     "" :
                                     HttpUtility.HtmlEncode("PO-1-0" + pluginField.ToString());
                    }
                }

                else if (sTableColumn == "PONumberArt" || sTableColumn == "PONumberArt_A")
                {
                    if (sTableColumn != null)
                    {

                        object pluginField = rgBug[i].GetPluginField(PLUGIN_ID, string.Format("{0}", sTableColumn));
                        sValues[i] = (pluginField == null) ?
                                     "" :
                                     HttpUtility.HtmlEncode("PO-2-0" + pluginField.ToString());
                    }
                }
                else if (sTableColumn == "PO_Number")
                {
                    if (sTableColumn != null)
                    {

                        object pluginField = rgBug[i].GetPluginField(PLUGIN_ID, string.Format("{0}", sTableColumn));
                        sValues[i] = (pluginField == null) ?
                                     "" :
                                     HttpUtility.HtmlEncode("PO-0" + pluginField.ToString());
                    }
                }

                else if (sTableColumn == "B_PO_Number" || sTableColumn == "B_PO_ref")
                {
                    if (sTableColumn != null)
                    {

                        object pluginField = rgBug[i].GetPluginField(PLUGIN_ID, string.Format("{0}", sTableColumn));
                        sValues[i] = (pluginField == null) ?
                                     "" :
                                     HttpUtility.HtmlEncode("B-PO-0" + pluginField.ToString());
                    }
                }

                else if (sTableColumn == "Add_Fld2")
                {
                    if (sTableColumn != null)
                    {
                        //decimal totalamnt = 0.00;
                        
                        //decimal val = Convert.ToDecimal(sValue);

                        object oVal = rgBug[i].GetPluginField(PLUGIN_ID, sTableColumn.ToString());
                        //object pluginField = rgBug[i].GetPluginField(PLUGIN_ID, string.Format("{0:C}",Convert.ToDouble(sTableColumn)));
                        string val = string.Format("{0:C}", oVal);//.ToString();//"$"+pluginField.ToString();
                        sValues[i] = (val == null) ?


                                     "" :
                                     HttpUtility.HtmlEncode(val);

                        // decimal val = Convert.ToDecimal(sValues[i]);

                    }
                }


                else
                {
                    object pluginField = rgBug[i].GetPluginField(PLUGIN_ID, string.Format("{0}", sTableColumn));
                    sValues[i] = (pluginField == null) ?
                                 "" :
                                 HttpUtility.HtmlEncode(pluginField.ToString());
                }
            }
            return sValues;
        }

        public CBugQuery GridColumnSortQuery(CGridColumn col, bool fDescending,
                                                bool fIncludeSelect)
        {
            string sTableColumn = "sInvoiceNumber";
            switch (col.iType)
            {
                case 0:
                    sTableColumn = "sInvoiceNumber";
                    break;
                case 1:
                    sTableColumn = "CWFVendor";
                    break;
                case 2:
                    sTableColumn = "TotalAmount";
                    break;
                case 3:
                    sTableColumn = "sInvoiceDate";
                    break;
                case 4:
                    sTableColumn = "sInvoiceDueDate";
                    break;
                case 5:
                    sTableColumn = "ixAtlevel";
                    break;
                case 6:
                    sTableColumn = "CWFCustomform";
                    break;
                case 7:
                    sTableColumn = "PONumber";
                    break;
                case 8:
                    sTableColumn = "Add_Fld1";
                    break;
                case 9:
                    sTableColumn = "PONumberArt";
                    break;
                case 10:
                    sTableColumn = "PONumberArt_A";
                    break;

                case 11:
                    sTableColumn = "PO_Number";
                    break;

                case 12:
                    sTableColumn = "B_PO_Number";
                    break;

                case 13:
                    sTableColumn = "Add_Fld2";
                    break;

                case 14:
                    sTableColumn = "B_PO_ref";
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

        #region Item CRUD

        protected void InsertItem()
        {

            try
            {
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));

                insertInt(insert, "ixBug");
                //insert.InsertInt("ixBug", 1705);

                // insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("sCWFAccountId")].ToString());
                insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("CWFAccount")].ToString());
               // insert.InsertString("sAccount", "2001");

                // insert.InsertString("sTaxtype", api.Request[api.AddPluginPrefix("sCWFVatId")].ToString());
                insert.InsertString("sTaxtype", api.Request[api.AddPluginPrefix("CWFVat")].ToString());
                //insertInt(insert, "ixGlAccount");
                //insertBoolean(insert, "iForm99");
                if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                {
                    try
                    {
                        insert.InsertFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));

                    }
                    catch
                    {
                        insert.InsertFloat("fAmount", 0.00);
                        //insert.InsertFloat("fAmount", 2d);
                    }
                }
                if (api.Request[api.AddPluginPrefix("fTax")] != null)
                {
                    try
                    {
                        insert.InsertFloat("fTax", Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]));

                    }
                    catch
                    {
                        insert.InsertFloat("fTax", 0.00);
                        //insert.InsertFloat("fTax", 2d);
                    }
                }

                insert.InsertString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());

                //insert.InsertString("sMemo", "Code testing");

                // insert.InsertString("sDepartment", api.Request[api.AddPluginPrefix("sCWFDepartmentId")].ToString());
                insert.InsertString("sDepartment", api.Request[api.AddPluginPrefix("CWFDepartment")].ToString());
                //insert.InsertString("sDepartment", "Test department");

                //insert.InsertString("sBillable", api.Request[api.AddPluginPrefix("sCWFBillableValue")].ToString());
                insert.InsertString("sBillable", api.Request[api.AddPluginPrefix("CWFBillable")].ToString());
                //insert.InsertString("sBillable", "Yes");

                insert.InsertString("sAddninfo", api.Request[api.AddPluginPrefix("sAddninfo")].ToString());
                insert.Execute();
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "");
            }
        }

        protected void InsertItem_cambridge()
        {
            //api.Notifications.AddAdminNotification("InsertItem_1", "InsertItem");
            try
            {
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                insertInt(insert, "ixBug");
                insert.InsertString("sExtra2", api.Request[api.AddPluginPrefix("CWFVat")].ToString());
                string Type = api.Request[api.AddPluginPrefix("CWFVat")].ToString();
                string caseid = api.Request[api.AddPluginPrefix("ixBug")].ToString();
                if (Type == " Credit")
                {

                    insert.InsertString("sAccount", "10-000-2000-00-000-000");
                    CSelectQuery QryCBsumAmt = api.Database.NewSelectQuery("Plugin_67_CGSInvoiceItems_MLA");
                    QryCBsumAmt.AddSelect("SUM(fAmount) as CSAmount");
                    string sWhere1 = ("Plugin_67_CGSInvoiceItems_MLA") + ".ixBug = " + caseid.ToString();
                    sWhere1 += " and iDeleted = 0";
                    QryCBsumAmt.AddWhere(sWhere1);

                    DataSet dsCS = QryCBsumAmt.GetDataSet();
                    double CSAmount = 0d;
                    if (null != dsCS.Tables && dsCS.Tables.Count == 1 && dsCS.Tables[0].Rows.Count == 1)
                    {
                        CSAmount = Convert.ToDouble(dsCS.Tables[0].Rows[0]["CSAmount"].ToString());
                       // api.Notifications.AddAdminNotification("CSAmount", CSAmount.ToString());
                        if (CSAmount != 0)
                        {
                            try
                            {
                                insert.InsertFloat("fAmount", CSAmount);
                            }
                            catch
                            {
                                insert.InsertFloat("fAmount", 0.00);
                            }
                        }
                    }
                }

                else
                {
                    insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("CWFAccount")].ToString());

                    if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                    {
                        try
                        {
                            insert.InsertFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                        }
                        catch
                        {
                            insert.InsertFloat("fAmount", 0.00);
                        }
                    }
                }
                insert.InsertString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                insert.Execute();
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "");
              //  api.Notifications.AddAdminNotification("1", "inserting error");
            }
        }

        protected void InsertItem_TE()
        {
            //api.Notifications.AddAdminNotification("InsertItem_1", "InsertItem");
            try
            {
                CInsertQuery insertTE = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                insertInt(insertTE, "ixBug");
                //insertTE.InsertInt("ixBug", 39883);


                insertTE.InsertString("sAddninfo", api.Request[api.AddPluginPrefix("sAddninfo")].ToString());
                // insertTE.InsertString("sAddninfo", "sdfdsfsf");
                // api.Notifications.AddAdminNotification("sAddninfo", "sdfdsfsf");


                if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                {
                    try
                    {
                        insertTE.InsertFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));

                    }
                    catch
                    {
                        insertTE.InsertFloat("fAmount", 0.00);
                        // insertTE.InsertFloat("fAmount", 2d);
                        // api.Notifications.AddAdminNotification("fAmount", "fAmount");
                    }
                }




                insertTE.InsertString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());

                // insertTE.InsertString("sMemo", "Code testing");

                //insert.InsertString("sAddninfo", api.Request[api.AddPluginPrefix("sAddninfo")].ToString());
                insertTE.Execute();
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "");
              //  api.Notifications.AddAdminNotification("1", "insert error");
            }
        }

        protected void InsertItem_Synergis()
        {
            string newponum = "";
            double famt = -1D;
            double ftax = -1D;
            double ftotal = -1D;
            string ixbug = api.Request[api.AddPluginPrefix("ixBug")];
          //  api.Notifications.AddAdminNotification("ixbug", ixbug.ToString());
            try
            {
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));

                //// generating lineitemid
                int LineNum = 0;
                int i = 0;
                int j = 1;
                CSelectQuery LineItemNo = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                LineItemNo.AddSelect("MAX(ixLineItemId) as LineItemNo");
                string sWhere = (api.Database.PluginTableName("CGSInvoiceItems_MLA")) + ".ixBug = " + ixbug.ToString();
                LineItemNo.AddWhere(sWhere);
                object LineItemId = LineItemNo.GetScalarValue();
                string Id = Convert.ToString(LineItemId);
              //  DataSet Synds = LineItemNo.GetDataSet();
                //api.Notifications.AddAdminNotification("ixLineItemId1", Id.ToString());
               

                if (Id == "")
               // if (null == Synds.Tables)
                {
                  //  api.Notifications.AddAdminNotification("ixLineItemId3", "ixLineItemId3");
                    insert.InsertInt("ixLineItemId", Convert.ToInt32(j.ToString()));
            
                }

                else
                {
                   // api.Notifications.AddAdminNotification("ixLineItemId4", "ixLineItemId4");
                    LineNum = Convert.ToInt32(Id.ToString());
                    i = Convert.ToInt32(LineNum) + 1;
                    insert.InsertInt("ixLineItemId", Convert.ToInt32(i.ToString()));
                                           
                }
                               
               // api.Notifications.AddAdminNotification("ixLineItemId", i.ToString());
                CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                Qrynewpon.AddSelect("Add_Fld1");
                string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + ixbug.ToString();
                Qrynewpon.AddWhere(sWhere2);
                object NewPo = Qrynewpon.GetScalarValue();
                newponum = Convert.ToString(NewPo);

            //    api.Notifications.AddAdminNotification("newponum1", newponum.ToString());
                if (newponum == null)
                {
                    //nothing
                }
                else
                {
                    insert.InsertString("sExtra2", newponum);
                }

                insertInt(insert, "ixBug");
                //insert.InsertInt("ixBug", 1705);

                // insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("sCWFAccountId")].ToString());
                insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("CWFAccount")].ToString());
                // insert.InsertString("sAccount", "2001");

                // insert.InsertString("sTaxtype", api.Request[api.AddPluginPrefix("sCWFVatId")].ToString());
                //insertInt(insert, "ixGlAccount");
                //insertBoolean(insert, "iForm99");
                if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                {
                    try
                    {
                        insert.InsertFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));

                    }
                    catch
                    {
                        insert.InsertFloat("fAmount", 0.00);
                        //insert.InsertFloat("fAmount", 2d);
                    }
                }

                
               
                        insert.InsertFloat("fTax", Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]));

                        famt = Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]);
                        ftax = Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]);
                        ftotal = (famt) * ftax;
                        insert.InsertFloat("sExtra3", ftotal);

                insert.InsertString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());

                //insert.InsertString("sMemo", "Code testing");

                insert.InsertString("ixProject", api.Request[api.AddPluginPrefix("ixProject")].ToString());
                insert.InsertString("sDepartment", api.Request[api.AddPluginPrefix("CWFDepartment")].ToString()); //project code

                insert.InsertFloat("fLineBalanceAmt", ftotal);
                //insert.InsertString("sDepartment", "Test department");
                
                insert.Execute();
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "");
            }
        }

        protected void InsertItem_Spreadfast()
        {
            string newponum = "";
            double famt = -1D;
            double ftax = -1D;
            double ftotal = -1D;
            string ixbug = api.Request[api.AddPluginPrefix("ixBug")];
            //  api.Notifications.AddAdminNotification("ixbug", ixbug.ToString());
            try
            {
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));

                //// generating lineitemid
                int LineNum = 0;
                int i = 0;
                int j = 1;
                CSelectQuery LineItemNo = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                LineItemNo.AddSelect("MAX(ixLineItemId) as LineItemNo");
                string sWhere = (api.Database.PluginTableName("CGSInvoiceItems_MLA")) + ".ixBug = " + ixbug.ToString();
                LineItemNo.AddWhere(sWhere);
                object LineItemId = LineItemNo.GetScalarValue();
                string Id = Convert.ToString(LineItemId);
                //  DataSet Synds = LineItemNo.GetDataSet();
               // api.Notifications.AddAdminNotification("ixLineItemId1", Id.ToString());


                if (Id == "")
                // if (null == Synds.Tables)
                {
                    //api.Notifications.AddAdminNotification("ixLineItemId3", "ixLineItemId3");
                    insert.InsertInt("ixLineItemId", Convert.ToInt32(j.ToString()));

                }

                else
                {
                    //api.Notifications.AddAdminNotification("ixLineItemId4", "ixLineItemId4");
                    LineNum = Convert.ToInt32(Id.ToString());
                    i = Convert.ToInt32(LineNum) + 1;
                    insert.InsertInt("ixLineItemId", Convert.ToInt32(i.ToString()));

                }


                CSelectQuery Qrynewpon = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                Qrynewpon.AddSelect("Add_Fld1");
                string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".ixBug = " + ixbug.ToString();
                Qrynewpon.AddWhere(sWhere2);
                object NewPo = Qrynewpon.GetScalarValue();
                newponum = Convert.ToString(NewPo);

                //    api.Notifications.AddAdminNotification("newponum1", newponum.ToString());
                if (newponum == null)
                {
                    //nothing
                }
                else
                {
                    insert.InsertString("sExtra2", newponum);
                }

                insertInt(insert, "ixBug");
                //insert.InsertInt("ixBug", 1705);

                // insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("sCWFAccountId")].ToString());
                insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("CWFAccount")].ToString());
                // insert.InsertString("sAccount", "2001");

                // insert.InsertString("sTaxtype", api.Request[api.AddPluginPrefix("sCWFVatId")].ToString());
                //insertInt(insert, "ixGlAccount");
                //insertBoolean(insert, "iForm99");
                if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                {
                    try
                    {
                        insert.InsertFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));

                    }
                    catch
                    {
                        insert.InsertFloat("fAmount", 0.00);
                        //insert.InsertFloat("fAmount", 2d);
                    }
                }

                insert.InsertFloat("fTax", Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]));

                famt = Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]);
                ftax = Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]);
                ftotal = (famt) * ftax;
                insert.InsertFloat("sExtra3", ftotal);

                insert.InsertString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());

                //insert.InsertString("sMemo", "Code testing");

                // insert.InsertString("sDepartment", api.Request[api.AddPluginPrefix("sCWFDepartmentId")].ToString());
                insert.InsertString("sDepartment", api.Request[api.AddPluginPrefix("CWFDepartment")].ToString()); //project code
                //insert.InsertString("sDepartment", "Test department");

                insert.Execute();
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "");
            }
        }

        private void insertInt(CInsertQuery insert, string sColumnName)
        {
            try
            {
                if (api.Request[api.AddPluginPrefix(sColumnName)] != null)
                    insert.InsertInt(sColumnName, Convert.ToInt32(api.Request[api.AddPluginPrefix(sColumnName)]));
            }
            catch (Exception)
            {
                api.Notifications.AddAdminNotification(sColumnName + " has a value of " + api.Request[api.AddPluginPrefix(sColumnName)], null);
            }
        }

        private void insertBoolean(CInsertQuery insert, string sColumnName)
        {
            try
            {
                if (api.Request[api.AddPluginPrefix(sColumnName)] != null)
                    insert.InsertInt(sColumnName, Convert.ToInt32(Convert.ToBoolean(api.Request[api.AddPluginPrefix(sColumnName)])));
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString() + sColumnName + " has a value of -" + api.Request[api.AddPluginPrefix(sColumnName)].ToString() + "-", null);
            }
        }

        /* Returns a DataSet object with all the Items in the database*/
        protected DataSet FetchItems(int ixBug, bool bExcludeDeleted)
        {
            CSelectQuery select = api.Database.NewSelectQuery(
            api.Database.PluginTableName("CGSInvoiceItems_MLA"));
            select.AddSelect("*");
            string sWhere = api.Database.PluginTableName("CGSInvoiceItems_MLA") + ".ixBug = " + ixBug.ToString();

            if (bExcludeDeleted)
            {
                sWhere += " and iDeleted = 0";
            }
            select.AddWhere(sWhere);
            return select.GetDataSet();
        }

        protected DataSet FetchItems_1(int ixBug, bool bExcludeDeleted)
        {
            CSelectQuery select = api.Database.NewSelectQuery(
            api.Database.PluginTableName("CGSPOMatchedInvoice"));
            select.AddSelect("*");
            string sWhere = api.Database.PluginTableName("CGSPOMatchedInvoice") + ".ixBug = " + ixBug.ToString();

            //if (bExcludeDeleted)
            //{
            //    sWhere += " and iDeleted = 0";
            //}
            select.AddWhere(sWhere);
            return select.GetDataSet();
        }
        
        private void LeftJoinTable(CSelectQuery select, string sType)
        {
            string projectPluginId = "CGSInvoiceDetails_MLA@conseroglobal.com";
            select.AddLeftJoin(api.Database.PluginTableName(projectPluginId, sType),
            api.Database.PluginTableName("CGSInvoiceItems_MLA") + ".ix" + sType + " = " +
            api.Database.PluginTableName(projectPluginId, sType) + ".ix" + sType);
        }

        protected void UpdateItem()
        {
            string project = api.Request[api.AddPluginPrefix("ixProject")].ToString();
            double famt = -1D;
            double ftax = -1D;
            double ftotal = -1D;
            CUpdateQuery update;
            try
            {
                update = api.Database.NewUpdateQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));
                if (project == "14")
                {

                    if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                    {
                        try
                        {
                            update.UpdateFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                        }
                        catch
                        {
                            update.UpdateFloat("fAmount", 0d);
                        }
                    }

                    if (api.Request[api.AddPluginPrefix("fTax")] != null)
                    {
                        try
                        {
                            update.UpdateFloat("fTax", Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]));
                        }
                        catch
                        {
                            update.UpdateFloat("fTax", 0d);
                        }
                    }

                    update.UpdateString("sAccount", api.Request[api.AddPluginPrefix("sAccount")].ToString());
                    update.UpdateString("sTaxtype", api.Request[api.AddPluginPrefix("sTaxtype")].ToString());
                    update.UpdateString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                    update.UpdateString("sDepartment", api.Request[api.AddPluginPrefix("sDepartment")].ToString());
                    update.UpdateString("sBillable", api.Request[api.AddPluginPrefix("sBillable")].ToString());
                    update.UpdateString("sAddninfo", api.Request[api.AddPluginPrefix("sAddninfo")].ToString());
                    update.AddWhere("ixBugLineItem = @ixBugLineItem");
                    update.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItem")]));
                    update.Execute();
                }
                else if (project == "19")
                {
                    if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                    {
                        try
                        {
                            update.UpdateFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                        }
                        catch
                        {
                            update.UpdateFloat("fAmount", 0d);
                        }
                        update.UpdateString("sAccount", api.Request[api.AddPluginPrefix("sAccount")].ToString());
                        update.UpdateString("sExtra2", api.Request[api.AddPluginPrefix("sExtra2")].ToString());
                        update.UpdateString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                        update.AddWhere("ixBugLineItem = @ixBugLineItem");
                        update.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItem")]));
                        update.Execute();
                    }
                }

                else if (project == "23")
                {

                    if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                    {
                        try
                        {
                            update.UpdateFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                        }
                        catch
                        {
                            update.UpdateFloat("fAmount", 0d);
                        }
                    }


                    update.UpdateString("sAddninfo", api.Request[api.AddPluginPrefix("sAddninfo")].ToString());
                    update.UpdateString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                    update.AddWhere("ixBugLineItem = @ixBugLineItem");
                    update.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItem")]));
                    update.Execute();
                }

                else if (project == "25")
                {

                    if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                    {
                        try
                        {
                            update.UpdateFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                        }
                        catch
                        {
                            update.UpdateFloat("fAmount", 0d);
                        }
                    }


                    update.UpdateFloat("fTax", Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]));
                    update.UpdateString("sAccount", api.Request[api.AddPluginPrefix("sAccount")].ToString());
                    update.UpdateString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                    update.UpdateString("sDepartment", api.Request[api.AddPluginPrefix("sDepartment")].ToString());
                    famt = Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]);
                    ftax = Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]);
                    ftotal = (famt) * ftax;
                    update.UpdateFloat("sExtra3", ftotal);

                    update.AddWhere("ixBugLineItem = @ixBugLineItem");
                    update.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItem")]));
                    update.Execute();
                }

                else if (project == "26")
                {

                    if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                    {
                        try
                        {
                            update.UpdateFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                        }
                        catch
                        {
                            update.UpdateFloat("fAmount", 0d);
                        }
                    }


                    update.UpdateFloat("fTax", Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]));
                    update.UpdateString("sAccount", api.Request[api.AddPluginPrefix("sAccount")].ToString());
                    update.UpdateString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                    update.UpdateString("sDepartment", api.Request[api.AddPluginPrefix("sDepartment")].ToString());
                    famt = Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]);
                    ftax = Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]);
                    ftotal = (famt) * ftax;
                    update.UpdateFloat("sExtra3", ftotal);

                    update.AddWhere("ixBugLineItem = @ixBugLineItem");
                    update.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItem")]));
                    update.Execute();
                }

                else if (project == "27")
                {

                    if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                    {
                        try
                        {
                            update.UpdateFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                        }
                        catch
                        {
                            update.UpdateFloat("fAmount", 0d);
                        }
                    }


                    update.UpdateFloat("fTax", Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]));
                    update.UpdateString("sAccount", api.Request[api.AddPluginPrefix("sAccount")].ToString());
                    update.UpdateString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                    update.UpdateString("sDepartment", api.Request[api.AddPluginPrefix("sDepartment")].ToString());
                    famt = Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]);
                    ftax = Convert.ToDouble(api.Request[api.AddPluginPrefix("fTax")]);
                    ftotal = (famt) * ftax;
                    update.UpdateFloat("sExtra3", ftotal);

                    update.AddWhere("ixBugLineItem = @ixBugLineItem");
                    update.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItem")]));
                    update.Execute();
                }
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "UpdateItem()");
            }
        }
        
        private void UpdateInt(CUpdateQuery update, string sColumnName)
        {
            if (api.Request[api.AddPluginPrefix(sColumnName)] != null)
            {
                update.UpdateInt(sColumnName, Convert.ToInt32(api.Request[api.AddPluginPrefix(sColumnName)]));
            }
        }

        protected void DeleteItem()
        {
            CUpdateQuery delete =api.Database.NewUpdateQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
            delete.UpdateInt("iDeleted", 1);
            delete.AddWhere("ixBugLineItem = @ixBugLineItem");
            delete.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBugLineItem")]));
            delete.Execute();
        }

        protected void DeleteItem_Synergis()
        {
            CUpdateQuery delete = api.Database.NewUpdateQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
            delete.UpdateInt("iDeleted", 1);
            delete.UpdateInt("ixLineItemId", 0);
            delete.AddWhere("ixBugLineItem = @ixBugLineItem");
            delete.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBugLineItem")]));
            delete.Execute();
        }

        protected void CopyCase_Cambridge()
        {
            int ixBug = 0;
            int i_OldCaseID = 0;
            bool bHeaderCopy = false;
            bool bLineItemsCopy = false;
            string sProj = "";
            string Vname = "";

            
            // api.Notifications.AddAdminNotification("Raw Page display called", "");
            ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

            CBug bug = api.Bug.GetBug(ixBug);
            bug.IgnorePermissions = true;
            int iproj = bug.ixProject;

            {
                try
                {
                    if (((api.Request[api.AddPluginPrefix("CaseID")].ToString().Trim()) == null) || ((api.Request[api.AddPluginPrefix("CaseID")].ToString().Trim()) == "") || ((api.Request[api.AddPluginPrefix("CWFVendor")].ToString().Trim()) != "-"))
                    {
                        Vname = (api.Request[api.AddPluginPrefix("CWFVendor")].ToString());
                        Vname = Vname.Replace("'", "''");
                    }
                    else
                    {
                        i_OldCaseID = Int32.Parse(api.Request[api.AddPluginPrefix("CaseID")].ToString());
                    }

                    try
                    {
                        bHeaderCopy = Boolean.Parse(api.Request[api.AddPluginPrefix("Header")].ToString().Trim());
                    }
                    catch
                    {
                        bHeaderCopy = false;
                    }

                    try
                    {
                        bLineItemsCopy = Boolean.Parse(api.Request[api.AddPluginPrefix("LineItems")].ToString().Trim());
                    }
                    catch
                    {
                        bLineItemsCopy = false;
                    }

                    CSelectQuery sqlInvoiceDetails;
                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + ixBug.ToString() + " AND iDeleted = 0");

                    DataSet dsExistinglineItems = sqlInvoiceDetails.GetDataSet();

                    if (dsExistinglineItems != null)
                    {
                        if (dsExistinglineItems.Tables[0].Rows.Count > 0)
                        {

                            api.Notifications.AddAdminNotification("Line Items exist for this case, you can not copy from other cases", "");

                            return;
                        }
                    }

                    DataSet dsOldCaseDetails = new DataSet();
                    if (Vname.Trim() != "")
                    {
                       // api.Notifications.AddAdminNotification("Vname1", Vname.ToString());
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("max(ixBug)");
                        sqlInvoiceDetails.AddWhere("CWFVendor =" + "'" + Vname.ToString() + "'");


                        object VID = sqlInvoiceDetails.GetScalarValue();
                        i_OldCaseID = Convert.ToInt32(VID);
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();
                    }

                    else
                    {

                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();
                    }

                    if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                    {
                        api.Notifications.AddMessage("This Case ID is not valid");
                        return;
                    }

                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString() + " AND iDeleted = 0");

                    DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                    string s_OldProjectID = "";

                    CSelectQuery sq = api.Database.NewSelectQuery("bug");
                    sq.AddSelect("Bug.ixProject");
                    sq.AddWhere("ixBug = " + i_OldCaseID.ToString());
                    DataSet ds = sq.GetDataSet();
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                    {
                        s_OldProjectID = ds.Tables[0].Rows[0][0].ToString();
                    }
                    ds.Dispose();


                    sProj = iproj.ToString();

                    if (s_OldProjectID != sProj)
                    {

                        api.Notifications.AddMessage("The Older case id must belong to the same project");
                        return;
                    }

                    if (bHeaderCopy == true)
                    {

                        string tablename = api.Database.PluginTableName("CGSInvoice_MLA");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        Update1.UpdateString("CWFApproverl1", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl1"].ToString());
                        Update1.UpdateString("CWFApproverl2", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                        Update1.UpdateString("CWFApproverl3", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl3"].ToString());
                        Update1.UpdateString("CWFApproverl4", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl4"].ToString());
                        Update1.UpdateString("CWFCustomform", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomform"].ToString());
                        Update1.UpdateString("CWFVendor", dsOldCaseDetails.Tables[0].Rows[0]["CWFVendor"].ToString());
                       // Update1.UpdateString("CWFCountry", dsOldCaseDetails.Tables[0].Rows[0]["CWFCountry"].ToString());
                        Update1.UpdateString("CWFCurrency", dsOldCaseDetails.Tables[0].Rows[0]["CWFCurrency"].ToString());
                        //Update1.UpdateString("CWFPostingperiod", dsOldCaseDetails.Tables[0].Rows[0]["CWFPostingperiod"].ToString());
                        //Update1.UpdateString("CWFSubsidiary", dsOldCaseDetails.Tables[0].Rows[0]["CWFSubsidiary"].ToString());
                        //Update1.UpdateString("CWFTerms", dsOldCaseDetails.Tables[0].Rows[0]["CWFTerms"].ToString());
                        //Update1.UpdateString("sInvoiceNumber", dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceNumber"].ToString());
                        //Update1.UpdateString("sExchangeRate", dsOldCaseDetails.Tables[0].Rows[0]["sExchangeRate"].ToString());
                        //Update1.UpdateString("AccountDesc", dsOldCaseDetails.Tables[0].Rows[0]["AccountDesc"].ToString());
                        //Update1.UpdateString("TotalAmount", dsOldCaseDetails.Tables[0].Rows[0]["TotalAmount"].ToString());
                        //Update1.UpdateString("Netamount", dsOldCaseDetails.Tables[0].Rows[0]["Netamount"].ToString());

                        //if (dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceAmount"] != null)
                        //{
                        //    try
                        //    {
                        //        Update1.UpdateFloat("sInvoiceAmount", Convert.ToDouble(dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceAmount"].ToString()));
                        //    }
                        //    catch
                        //    {
                        //        Update1.UpdateFloat("sInvoiceAmount", 0d);
                        //    }
                        //}



                        //if (dsOldCaseDetails.Tables[0].Rows[0]["sTaxAmount"] != null)
                        //{
                        //    try
                        //    {
                        //        Update1.UpdateFloat("sTaxAmount", Convert.ToDouble(dsOldCaseDetails.Tables[0].Rows[0]["sTaxAmount"].ToString()));
                        //    }
                        //    catch
                        //    {
                        //        Update1.UpdateFloat("sTaxAmount", 0d);
                        //    }
                        //}

                        Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["sMemo"].ToString());
                        //Update1.UpdateString("sAddInfo", dsOldCaseDetails.Tables[0].Rows[0]["sAddInfo"].ToString());
                        //Update1.UpdateString("CCUser", dsOldCaseDetails.Tables[0].Rows[0]["CCUser"].ToString());
                        //Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                        //Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                        //Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());
                        Update1.AddWhere("ixBug = @ixBug");
                        Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                        Update1.Execute();

                    }
                    if (bLineItemsCopy == true)
                    {
                        if ((dsLineItemsDetails != null) && (dsLineItemsDetails.Tables[0].Rows.Count > 0))
                        {

                            foreach (DataRow dr in dsLineItemsDetails.Tables[0].Rows)
                            {
                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                                insert1.InsertInt("ixBug", ixBug);
                                insert1.InsertString("sAccount", dr["sAccount"].ToString());
                                insert1.InsertString("sTaxtype", dr["sTaxtype"].ToString());

                                if (dr["fAmount"] != null)
                                {
                                    try
                                    {
                                       // insert1.InsertFloat("fAmount", Convert.ToDouble(dr["fAmount"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("fAmount", 0d);
                                    }
                                }


                                if (dr["fTax"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("fTax", Convert.ToDouble(dr["fTax"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("fTax", 0d);
                                    }
                                }

                                insert1.InsertString("sMemo", dr["sMemo"].ToString());
                                insert1.InsertString("sDepartment", dr["sDepartment"].ToString());
                                insert1.InsertString("sBillable", dr["sBillable"].ToString());
                               // insert1.InsertString("sAddninfo", dr["sAddninfo"].ToString());

                                if (dr["iDeleted"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertInt("iDeleted", Convert.ToInt32(dr["iDeleted"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertInt("iDeleted", 0);
                                    }
                                }

                                insert1.InsertInt("iDeleted", Int32.Parse(dr["iDeleted"].ToString()));

                                if (dr["ixExtra1"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("ixExtra1", Convert.ToDouble(dr["ixExtra1"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("ixExtra1", 0d);
                                    }
                                }

                                insert1.InsertString("sExtra2", dr["sExtra2"].ToString());
                                if (dr["sExtra3"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("sExtra3", Convert.ToDouble(dr["sExtra3"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("sExtra3", 0d);
                                    }
                                }
                                //insert1.InsertString("sExtra4", dr["sExtra4"].ToString());
                                //insert1.InsertString("sExtra5", dr["sExtra5"].ToString());
                                //insert1.InsertString("sExtra6", dr["sExtra6"].ToString());
                                insert1.Execute();
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    api.Notifications.AddAdminNotification(e.ToString(), "Error in Updating case details.");
                }
            }
        }

        protected void CopyCase_Synergis()
        {
            int ixBug = 0;
            int i_OldCaseID = 0;
            bool bHeaderCopy = false;
            bool bLineItemsCopy = false;
            string sProj = "";
            string Vname = "";
            int Bugid = 0;
            string i_OldCaseID1 = "";

           // api.Notifications.AddAdminNotification("CopyCase_Synergis", "CopyCase_Synergis");
            // api.Notifications.AddAdminNotification("Raw Page display called", "");

            ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

            CBug bug = api.Bug.GetBug(ixBug);
            bug.IgnorePermissions = true;
            int iproj = bug.ixProject;

            {
                try
                {
                    if (((api.Request[api.AddPluginPrefix("PONumber")].ToString().Trim()) == null) || ((api.Request[api.AddPluginPrefix("PONumber")].ToString().Trim()) == ""))
                    {
                        api.Notifications.AddMessage("PO Number is Empty");
                    }
                    else
                    {
                        //i_OldCaseID = Int32.Parse(api.Request[api.AddPluginPrefix("PONumber")].ToString());
                        i_OldCaseID1 = api.Request[api.AddPluginPrefix("PONumber")].ToString();
                        i_OldCaseID = Int32.Parse(i_OldCaseID1.Substring(6));
                        //api.Notifications.AddAdminNotification("i_OldCaseID", i_OldCaseID.ToString());

                    }

                    try
                    {
                        bHeaderCopy = Boolean.Parse(api.Request[api.AddPluginPrefix("Header")].ToString().Trim());
                    }
                    catch
                    {
                        bHeaderCopy = false;
                    }

                    try
                    {
                        bLineItemsCopy = Boolean.Parse(api.Request[api.AddPluginPrefix("LineItems")].ToString().Trim());
                    }
                    catch
                    {
                        bLineItemsCopy = false;
                    }

                    CSelectQuery sqlInvoiceDetails;
                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + ixBug.ToString() + " AND iDeleted = 0");

                    DataSet dsExistinglineItems = sqlInvoiceDetails.GetDataSet();

                    if (dsExistinglineItems != null)
                    {
                        if (dsExistinglineItems.Tables[0].Rows.Count > 0)
                        {

                            api.Notifications.AddAdminNotification("Line Items exist for this case, you can not copy from other cases", "");

                            return;
                        }
                    }

                    DataSet dsOldCaseDetails = new DataSet();

                    if (iproj == 25)
                    {

                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("max(ixBug)");
                        sqlInvoiceDetails.AddWhere("PONumber =" + "'" + i_OldCaseID.ToString() + "'");


                        object BugId = sqlInvoiceDetails.GetScalarValue();
                        Bugid = Convert.ToInt32(BugId);
                       // api.Notifications.AddAdminNotification("Bugid", Bugid.ToString());
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + Bugid.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();
                    }

                    else
                    {
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("max(ixBug)");
                        sqlInvoiceDetails.AddWhere("PONumberArt =" + "'" + i_OldCaseID.ToString() + "'");


                        object BugId = sqlInvoiceDetails.GetScalarValue();
                        Bugid = Convert.ToInt32(BugId);
                        //api.Notifications.AddAdminNotification("Bugid", Bugid.ToString());
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + Bugid.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();
                    }
                    

                 

                    if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                    {
                        api.Notifications.AddMessage("This Case ID is not valid");
                        return;
                    }

                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + Bugid.ToString() + " AND iDeleted = 0");

                    DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                    string s_OldProjectID = "";

                    CSelectQuery sq = api.Database.NewSelectQuery("bug");
                    sq.AddSelect("Bug.ixProject");
                    sq.AddWhere("ixBug = " + Bugid.ToString());
                    DataSet ds = sq.GetDataSet();
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                    {
                        s_OldProjectID = ds.Tables[0].Rows[0][0].ToString();
                    }
                    ds.Dispose();


                    sProj = iproj.ToString();

                    if (s_OldProjectID != sProj)
                    {

                        api.Notifications.AddMessage("The Older PO Number must belong to the same project");
                        return;
                    }

                    if (bHeaderCopy == true)
                    {
                        //api.Notifications.AddAdminNotification("Line1", "Line1");
                        string tablename = api.Database.PluginTableName("CGSInvoice_MLA");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        Update1.UpdateString("CWFApproverl1", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl1"].ToString());
                        Update1.UpdateString("CWFApproverl2", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                        Update1.UpdateString("CWFApproverl3", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl3"].ToString());
                        Update1.UpdateString("CWFApproverl4", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl4"].ToString());
                        Update1.UpdateString("CWFVendor", dsOldCaseDetails.Tables[0].Rows[0]["CWFVendor"].ToString());
                        Update1.UpdateString("CWFLocation", dsOldCaseDetails.Tables[0].Rows[0]["CWFLocation"].ToString());
                        Update1.UpdateString("CWFDept", dsOldCaseDetails.Tables[0].Rows[0]["CWFDept"].ToString());
                        Update1.UpdateString("CWFPostingperiod", dsOldCaseDetails.Tables[0].Rows[0]["CWFPostingperiod"].ToString());
                        Update1.UpdateString("CWFSubsidiary", dsOldCaseDetails.Tables[0].Rows[0]["CWFSubsidiary"].ToString());
                        Update1.UpdateString("CWFTerms", dsOldCaseDetails.Tables[0].Rows[0]["CWFTerms"].ToString());
                        Update1.UpdateString("sInvoiceNumber", dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceNumber"].ToString());
                        Update1.UpdateString("DateString1", dsOldCaseDetails.Tables[0].Rows[0]["DateString1"].ToString());
                      //  Update1.UpdateString("TotalAmount", dsOldCaseDetails.Tables[0].Rows[0]["TotalAmount"].ToString());
                        Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["sMemo"].ToString());
                        Update1.UpdateString("DateString2", dsOldCaseDetails.Tables[0].Rows[0]["DateString2"].ToString());
                        Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                        Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                        Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());
                        Update1.AddWhere("ixBug = @ixBug");
                        Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                        Update1.Execute();

                    }
                    if (bLineItemsCopy == true)
                    {
                        //api.Notifications.AddAdminNotification("Line2", "Line2");
                        if ((dsLineItemsDetails != null) && (dsLineItemsDetails.Tables[0].Rows.Count > 0))
                        {
                           // api.Notifications.AddAdminNotification("Line3", "Line3");
                            foreach (DataRow dr in dsLineItemsDetails.Tables[0].Rows)
                            {

                               // api.Notifications.AddAdminNotification("Line4", "Line4");
                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));

                                insert1.InsertInt("ixLineItemId", Convert.ToInt32(dr["ixLineItemId"].ToString()));


                                insert1.InsertInt("ixBug", ixBug);
                                insert1.InsertString("sAccount", dr["sAccount"].ToString());
                                
                                if (dr["fAmount"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("fAmount", Convert.ToDouble(dr["fAmount"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("fAmount", 0d);
                                    }
                                }
                                insert1.InsertFloat("fTax", Convert.ToDouble(dr["fTax"].ToString()));
                                insert1.InsertString("sDepartment", dr["sDepartment"].ToString());
                                insert1.InsertFloat("sExtra3", Convert.ToDouble(dr["sExtra3"].ToString()));
                                insert1.InsertString("sMemo", dr["sMemo"].ToString());
                              //  insert1.InsertString("sAddninfo", dr["sAddninfo"].ToString());

                                if (dr["iDeleted"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertInt("iDeleted", Convert.ToInt32(dr["iDeleted"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertInt("iDeleted", 0);
                                    }
                                }

                                insert1.InsertInt("iDeleted", Int32.Parse(dr["iDeleted"].ToString()));

                                insert1.Execute();
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    api.Notifications.AddAdminNotification(e.ToString(), "Error in Updating case details.");
                }
            }
        }

        protected void Addendum_Synergis()
        {
            int ixBug = 0;
            int i_OldPO = 0;
         //   bool bHeaderCopy = false;
         //   bool bLineItemsCopy = false;
            string sProj = "";
            int i_CaseID = 0;
             string i_OldCaseID1 = "";
          //  string PONum = "";


            // api.Notifications.AddAdminNotification("Raw Page display called", "");
            ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

            CBug bug = api.Bug.GetBug(ixBug);
            bug.IgnorePermissions = true;
            int iproj = bug.ixProject;

            {
                try
                {

                    if (((api.Request[api.AddPluginPrefix("PONumber")].ToString().Trim()) == null) || ((api.Request[api.AddPluginPrefix("PONumber")].ToString().Trim()) == ""))
                    {
                        api.Notifications.AddMessage("PO Number is Empty");
                    }
                    else
                    {
                        // i_OldPO = Int32.Parse(api.Request[api.AddPluginPrefix("PONumber")].ToString());
                        i_OldCaseID1 = api.Request[api.AddPluginPrefix("PONumber")].ToString();
                        i_OldPO = Int32.Parse(i_OldCaseID1.Substring(6));

                    }


                    CSelectQuery sqlInvoiceDetails;
                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + ixBug.ToString() + " AND iDeleted = 0");

                    DataSet dsExistinglineItems = sqlInvoiceDetails.GetDataSet();

                    if (dsExistinglineItems != null)
                    {
                        if (dsExistinglineItems.Tables[0].Rows.Count > 0)
                        {

                            api.Notifications.AddAdminNotification("Line Items exist for this case, you can not copy from other cases", "");

                            return;
                        }
                    }

                    DataSet dsOldCaseDetails = new DataSet();

                    if(iproj == 25)
                    {


                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("max(ixBug)");
                        sqlInvoiceDetails.AddWhere("PONumber =" + "'" + i_OldPO.ToString() + "'");

                        object BugId = sqlInvoiceDetails.GetScalarValue();
                        i_CaseID = Convert.ToInt32(BugId);
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_CaseID.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();


                        if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                        {
                            api.Notifications.AddMessage("This PO Number is not valid");
                            return;
                        }

                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_CaseID.ToString() + " AND iDeleted = 0");

                        DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                        string s_OldProjectID = "";

                        CSelectQuery sq = api.Database.NewSelectQuery("bug");
                        sq.AddSelect("Bug.ixProject");
                        sq.AddWhere("ixBug = " + bug.ixBug.ToString());
                        DataSet ds = sq.GetDataSet();
                        if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                        {
                            s_OldProjectID = ds.Tables[0].Rows[0][0].ToString();
                        }
                        ds.Dispose();


                        sProj = iproj.ToString();

                        if (s_OldProjectID != sProj)
                        {

                            api.Notifications.AddMessage("The Older PO must belong to the same project");
                            return;
                        }

                        //    if (bHeaderCopy == true)
                        //   {


                        string tablename = api.Database.PluginTableName("CGSInvoice_MLA");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        string PONew = "";
                        PONew = i_OldPO.ToString() + "_A";


                        Update1.UpdateString("CWFApproverl1", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl1"].ToString());
                        Update1.UpdateString("CWFApproverl2", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                        Update1.UpdateString("CWFApproverl3", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl3"].ToString());
                        Update1.UpdateString("CWFApproverl4", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl4"].ToString());
                        Update1.UpdateString("CWFVendor", dsOldCaseDetails.Tables[0].Rows[0]["CWFVendor"].ToString());
                        Update1.UpdateString("CWFLocation", dsOldCaseDetails.Tables[0].Rows[0]["CWFLocation"].ToString());
                        Update1.UpdateString("CWFDept", dsOldCaseDetails.Tables[0].Rows[0]["CWFDept"].ToString());
                        Update1.UpdateString("CWFPostingperiod", dsOldCaseDetails.Tables[0].Rows[0]["CWFPostingperiod"].ToString());
                        Update1.UpdateString("CWFSubsidiary", dsOldCaseDetails.Tables[0].Rows[0]["CWFSubsidiary"].ToString());
                        Update1.UpdateString("CWFTerms", dsOldCaseDetails.Tables[0].Rows[0]["CWFTerms"].ToString());
                        Update1.UpdateString("sInvoiceNumber", dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceNumber"].ToString());
                        Update1.UpdateString("DateString2", dsOldCaseDetails.Tables[0].Rows[0]["DateString2"].ToString());
                        Update1.UpdateString("TotalAmount", dsOldCaseDetails.Tables[0].Rows[0]["TotalAmount"].ToString());
                        Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["sMemo"].ToString());
                        Update1.UpdateString("PONumber", dsOldCaseDetails.Tables[0].Rows[0]["PONumber"].ToString());
                        Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                        Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                        Update1.UpdateString("DateString1", dsOldCaseDetails.Tables[0].Rows[0]["DateString1"].ToString());

                        Update1.UpdateString("Add_Fld1", PONew.ToString());

                        Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());
                        Update1.AddWhere("ixBug = @ixBug");
                        Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                        Update1.Execute();

                        //  }
                        //   if (bLineItemsCopy == true)
                        //  {
                        if ((dsLineItemsDetails != null) && (dsLineItemsDetails.Tables[0].Rows.Count > 0))
                        {

                            foreach (DataRow dr in dsLineItemsDetails.Tables[0].Rows)
                            {
                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));

                               // CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));

                                insert1.InsertInt("ixLineItemId", Convert.ToInt32(dr["ixLineItemId"].ToString()));

                                insert1.InsertInt("ixBug", ixBug);
                                insert1.InsertString("sAccount", dr["sAccount"].ToString());

                                if (dr["fAmount"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("fAmount", Convert.ToDouble(dr["fAmount"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("fAmount", 0d);
                                    }
                                }
                                insert1.InsertFloat("fTax", Convert.ToDouble(dr["fTax"].ToString()));
                                insert1.InsertString("sDepartment", dr["sDepartment"].ToString());
                                insert1.InsertFloat("sExtra3", Convert.ToDouble(dr["sExtra3"].ToString()));
                                insert1.InsertString("sMemo", dr["sMemo"].ToString());

                                if (dr["iDeleted"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertInt("iDeleted", Convert.ToInt32(dr["iDeleted"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertInt("iDeleted", 0);
                                    }
                                }

                                insert1.InsertInt("iDeleted", Int32.Parse(dr["iDeleted"].ToString()));

                                insert1.Execute();
                            }
                        }
                    }
                    else
                    {


                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("max(ixBug)");
                        sqlInvoiceDetails.AddWhere("PONumberArt =" + "'" + i_OldPO.ToString() + "'");

                        object BugId = sqlInvoiceDetails.GetScalarValue();
                        i_CaseID = Convert.ToInt32(BugId);
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_CaseID.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();


                        if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                        {
                            api.Notifications.AddMessage("This PO Number is not valid");
                            return;
                        }

                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_CaseID.ToString() + " AND iDeleted = 0");

                        DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                        string s_OldProjectID = "";

                        CSelectQuery sq = api.Database.NewSelectQuery("bug");
                        sq.AddSelect("Bug.ixProject");
                        sq.AddWhere("ixBug = " + bug.ixBug.ToString());
                        DataSet ds = sq.GetDataSet();
                        if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                        {
                            s_OldProjectID = ds.Tables[0].Rows[0][0].ToString();
                        }
                        ds.Dispose();


                        sProj = iproj.ToString();

                        if (s_OldProjectID != sProj)
                        {

                            api.Notifications.AddMessage("The Older PO must belong to the same project");
                            return;
                        }

                        //    if (bHeaderCopy == true)
                        //   {


                        string tablename = api.Database.PluginTableName("CGSInvoice_MLA");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        string PONew = "";
                        PONew = i_OldPO.ToString() + "_A";


                        Update1.UpdateString("CWFApproverl1", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl1"].ToString());
                        Update1.UpdateString("CWFApproverl2", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                        Update1.UpdateString("CWFApproverl3", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl3"].ToString());
                        Update1.UpdateString("CWFApproverl4", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl4"].ToString());
                        Update1.UpdateString("CWFVendor", dsOldCaseDetails.Tables[0].Rows[0]["CWFVendor"].ToString());
                        Update1.UpdateString("CWFLocation", dsOldCaseDetails.Tables[0].Rows[0]["CWFLocation"].ToString());
                        Update1.UpdateString("CWFDept", dsOldCaseDetails.Tables[0].Rows[0]["CWFDept"].ToString());
                        Update1.UpdateString("CWFPostingperiod", dsOldCaseDetails.Tables[0].Rows[0]["CWFPostingperiod"].ToString());
                        Update1.UpdateString("CWFSubsidiary", dsOldCaseDetails.Tables[0].Rows[0]["CWFSubsidiary"].ToString());
                        Update1.UpdateString("CWFTerms", dsOldCaseDetails.Tables[0].Rows[0]["CWFTerms"].ToString());
                        Update1.UpdateString("sInvoiceNumber", dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceNumber"].ToString());
                        Update1.UpdateString("DateString2", dsOldCaseDetails.Tables[0].Rows[0]["DateString2"].ToString());
                        Update1.UpdateString("TotalAmount", dsOldCaseDetails.Tables[0].Rows[0]["TotalAmount"].ToString());
                        Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["sMemo"].ToString());
                        Update1.UpdateString("PONumberArt", dsOldCaseDetails.Tables[0].Rows[0]["PONumberArt"].ToString());
                        Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                        Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                        Update1.UpdateString("DateString1", dsOldCaseDetails.Tables[0].Rows[0]["DateString1"].ToString());

                        Update1.UpdateString("PONumberArt_A", PONew.ToString());

                        Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());
                        Update1.AddWhere("ixBug = @ixBug");
                        Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                        Update1.Execute();

                        //  }
                        //   if (bLineItemsCopy == true)
                        //  {
                        if ((dsLineItemsDetails != null) && (dsLineItemsDetails.Tables[0].Rows.Count > 0))
                        {

                            foreach (DataRow dr in dsLineItemsDetails.Tables[0].Rows)
                            {
                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));


                                insert1.InsertInt("ixLineItemId", Convert.ToInt32(dr["ixLineItemId"].ToString()));

                                insert1.InsertInt("ixBug", ixBug);
                                insert1.InsertString("sAccount", dr["sAccount"].ToString());

                                if (dr["fAmount"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("fAmount", Convert.ToDouble(dr["fAmount"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("fAmount", 0d);
                                    }
                                }
                                insert1.InsertFloat("fTax", Convert.ToDouble(dr["fTax"].ToString()));
                                insert1.InsertString("sDepartment", dr["sDepartment"].ToString());
                                insert1.InsertFloat("sExtra3", Convert.ToDouble(dr["sExtra3"].ToString()));
                                insert1.InsertString("sMemo", dr["sMemo"].ToString());

                                if (dr["iDeleted"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertInt("iDeleted", Convert.ToInt32(dr["iDeleted"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertInt("iDeleted", 0);
                                    }
                                }

                                insert1.InsertInt("iDeleted", Int32.Parse(dr["iDeleted"].ToString()));

                                insert1.Execute();
                            }
                        }
                    }

                    // }
                }
                catch (Exception e)
                {
                    api.Notifications.AddAdminNotification(e.ToString(), "Error in Updating case details.");
                }
            }
        }

        protected void CopyCase_Spreadfast()
        {
           // api.Notifications.AddAdminNotification("CopyCase_Spreadfast", "CopyCase_Spreadfast");
            
            int ixBug = 0;
            int i_OldCaseID = 0;
            bool bHeaderCopy = false;
            bool bLineItemsCopy = false;
            string sProj = "";
          //  string Vname = "";
            int Bugid = 0;
            string i_OldCaseID1 = "";


            // api.Notifications.AddAdminNotification("Raw Page display called", "");
            ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

            CBug bug = api.Bug.GetBug(ixBug);
            bug.IgnorePermissions = true;
            int iproj = bug.ixProject;

            {
                try
                {
                    if (((api.Request[api.AddPluginPrefix("PONumber")].ToString().Trim()) == null) || ((api.Request[api.AddPluginPrefix("PONumber")].ToString().Trim()) == ""))
                    {
                        api.Notifications.AddMessage("PO Number is Empty");
                    }
                    else
                    {
                        //i_OldCaseID = Int32.Parse(api.Request[api.AddPluginPrefix("PONumber")].ToString());
                        i_OldCaseID1 = api.Request[api.AddPluginPrefix("PONumber")].ToString();
                        i_OldCaseID = Int32.Parse(i_OldCaseID1.Substring(6));
                      //  api.Notifications.AddAdminNotification("i_OldCaseID", i_OldCaseID.ToString());

                    }

                    try
                    {
                        bHeaderCopy = Boolean.Parse(api.Request[api.AddPluginPrefix("Header")].ToString().Trim());
                    }
                    catch
                    {
                        bHeaderCopy = false;
                    }

                    try
                    {
                        bLineItemsCopy = Boolean.Parse(api.Request[api.AddPluginPrefix("LineItems")].ToString().Trim());
                    }
                    catch
                    {
                        bLineItemsCopy = false;
                    }

                    CSelectQuery sqlInvoiceDetails;
                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + ixBug.ToString() + " AND iDeleted = 0");

                    DataSet dsExistinglineItems = sqlInvoiceDetails.GetDataSet();

                    if (dsExistinglineItems != null)
                    {
                        if (dsExistinglineItems.Tables[0].Rows.Count > 0)
                        {

                            api.Notifications.AddAdminNotification("Line Items exist for this case, you can not copy from other cases", "");

                            return;
                        }
                    }

                    DataSet dsOldCaseDetails = new DataSet();
                  //  api.Notifications.AddAdminNotification("projid1", iproj.ToString());

                    if (iproj == 27)
                    {
                        int projid = 27;
                      //  api.Notifications.AddAdminNotification("projid2", projid.ToString());
                   // int projid = Convert.ToInt32(bug.ixProject.ToString());
                    
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("max(ixBug)");
                        sqlInvoiceDetails.AddWhere("PO_Number = " + i_OldCaseID.ToString());
                        sqlInvoiceDetails.AddWhere("ixproject = 27");
                      //  api.Notifications.AddAdminNotification("projid", projid.ToString());

                        object BugId = sqlInvoiceDetails.GetScalarValue();
                        Bugid = Convert.ToInt32(BugId);
                        api.Notifications.AddAdminNotification("Bugid_SF", Bugid.ToString());
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + Bugid.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();
                   }

                        //api.Notifications.AddAdminNotification("copycase_SF", dsOldCaseDetails.ToString());

                    if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                    {
                        api.Notifications.AddMessage("This Case ID is not valid");
                        return;
                    }

                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + Bugid.ToString() + " AND iDeleted = 0");

                    DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                    string s_OldProjectID = "";

                    CSelectQuery sq = api.Database.NewSelectQuery("bug");
                    sq.AddSelect("Bug.ixProject");
                    sq.AddWhere("ixBug = " + Bugid.ToString());
                    DataSet ds = sq.GetDataSet();
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                    {
                        s_OldProjectID = ds.Tables[0].Rows[0][0].ToString();
                    }
                    ds.Dispose();


                    sProj = iproj.ToString();

                    if (s_OldProjectID != sProj)
                    {

                        api.Notifications.AddMessage("The Older PO Number must belong to the same project");
                        return;
                    }

                    if (bHeaderCopy == true)
                    {

                        string tablename = api.Database.PluginTableName("CGSInvoice_MLA");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        Update1.UpdateString("CWFApproverl1", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl1"].ToString());
                        Update1.UpdateString("CWFApproverl2", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                        Update1.UpdateString("CWFApproverl3", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl3"].ToString());
                        Update1.UpdateString("CWFApproverl4", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl4"].ToString());
                        Update1.UpdateString("CWFVendor", dsOldCaseDetails.Tables[0].Rows[0]["CWFVendor"].ToString());
                        Update1.UpdateString("CWFLocation", dsOldCaseDetails.Tables[0].Rows[0]["CWFLocation"].ToString());
                        Update1.UpdateString("CWFDept", dsOldCaseDetails.Tables[0].Rows[0]["CWFDept"].ToString());
                        Update1.UpdateString("CWFPostingperiod", dsOldCaseDetails.Tables[0].Rows[0]["CWFPostingperiod"].ToString());
                        Update1.UpdateString("CWFSubsidiary", dsOldCaseDetails.Tables[0].Rows[0]["CWFSubsidiary"].ToString());
                        Update1.UpdateString("CWFTerms", dsOldCaseDetails.Tables[0].Rows[0]["CWFTerms"].ToString());
                        Update1.UpdateString("sInvoiceNumber", dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceNumber"].ToString());
                        Update1.UpdateString("DateString1", dsOldCaseDetails.Tables[0].Rows[0]["DateString1"].ToString());
                        //  Update1.UpdateString("TotalAmount", dsOldCaseDetails.Tables[0].Rows[0]["TotalAmount"].ToString());
                        Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["sMemo"].ToString());
                        Update1.UpdateString("DateString2", dsOldCaseDetails.Tables[0].Rows[0]["DateString2"].ToString());
                        Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                        Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                        Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());
                        Update1.AddWhere("ixBug = @ixBug");
                        Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                        Update1.Execute();

                    }
                    if (bLineItemsCopy == true)
                    {
                        if ((dsLineItemsDetails != null) && (dsLineItemsDetails.Tables[0].Rows.Count > 0))
                        {

                            foreach (DataRow dr in dsLineItemsDetails.Tables[0].Rows)
                            {
                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                                insert1.InsertInt("ixBug", ixBug);
                                insert1.InsertString("sAccount", dr["sAccount"].ToString());

                                if (dr["fAmount"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("fAmount", Convert.ToDouble(dr["fAmount"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("fAmount", 0d);
                                    }
                                }
                                insert1.InsertFloat("fTax", Convert.ToDouble(dr["fTax"].ToString()));
                                insert1.InsertString("sDepartment", dr["sDepartment"].ToString());
                                insert1.InsertFloat("sExtra3", Convert.ToDouble(dr["sExtra3"].ToString()));
                                insert1.InsertString("sMemo", dr["sMemo"].ToString());
                                //  insert1.InsertString("sAddninfo", dr["sAddninfo"].ToString());

                                if (dr["iDeleted"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertInt("iDeleted", Convert.ToInt32(dr["iDeleted"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertInt("iDeleted", 0);
                                    }
                                }

                                insert1.InsertInt("iDeleted", Int32.Parse(dr["iDeleted"].ToString()));

                                insert1.Execute();
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    api.Notifications.AddAdminNotification(e.ToString(), "Error in Updating case details.");
                }
            }
        }

        protected void Addendum_Spreadfast()
        {
            int ixBug = 0;
            int i_OldPO = 0;
            //   bool bHeaderCopy = false;
            //   bool bLineItemsCopy = false;
            string sProj = "";
            int i_CaseID = 0;
            string i_OldCaseID1 = "";
            //  string PONum = "";


            // api.Notifications.AddAdminNotification("Raw Page display called", "");
            ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

            CBug bug = api.Bug.GetBug(ixBug);
            bug.IgnorePermissions = true;
            int iproj = bug.ixProject;

            {
                try
                {

                    if (((api.Request[api.AddPluginPrefix("PONumber")].ToString().Trim()) == null) || ((api.Request[api.AddPluginPrefix("PONumber")].ToString().Trim()) == ""))
                    {
                        api.Notifications.AddMessage("PO Number is Empty");
                    }
                    else
                    {
                        // i_OldPO = Int32.Parse(api.Request[api.AddPluginPrefix("PONumber")].ToString());
                        i_OldCaseID1 = api.Request[api.AddPluginPrefix("PONumber")].ToString();
                        i_OldPO = Int32.Parse(i_OldCaseID1.Substring(4));
                       // api.Notifications.AddAdminNotification("i_OldPO", i_OldPO.ToString());
                    }


                    CSelectQuery sqlInvoiceDetails;
                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + ixBug.ToString() + " AND iDeleted = 0");

                    DataSet dsExistinglineItems = sqlInvoiceDetails.GetDataSet();

                    if (dsExistinglineItems != null)
                    {
                        if (dsExistinglineItems.Tables[0].Rows.Count > 0)
                        {

                            api.Notifications.AddAdminNotification("Line Items exist for this case, you can not copy from other cases", "");

                            return;
                        }
                    }

                    DataSet dsOldCaseDetails = new DataSet();

                  //  if (iproj == 25)
                   // {


                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("max(ixBug)");
                        sqlInvoiceDetails.AddWhere("PO_Number = " + i_OldPO.ToString());
                        sqlInvoiceDetails.AddWhere("ixproject = 27");

                        object BugId = sqlInvoiceDetails.GetScalarValue();
                        i_CaseID = Convert.ToInt32(BugId);
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_CaseID.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();


                        if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                        {
                            api.Notifications.AddMessage("This PO Number is not valid");
                            return;
                        }

                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_CaseID.ToString() + " AND iDeleted = 0");

                        DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                        string s_OldProjectID = "";

                        CSelectQuery sq = api.Database.NewSelectQuery("bug");
                        sq.AddSelect("Bug.ixProject");
                        sq.AddWhere("ixBug = " + bug.ixBug.ToString());
                        DataSet ds = sq.GetDataSet();
                        if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                        {
                            s_OldProjectID = ds.Tables[0].Rows[0][0].ToString();
                        }
                        ds.Dispose();


                        sProj = iproj.ToString();

                        if (s_OldProjectID != sProj)
                        {

                            api.Notifications.AddMessage("The Older PO must belong to the same project");
                            return;
                        }

                        //    if (bHeaderCopy == true)
                        //   {


                        string tablename = api.Database.PluginTableName("CGSInvoice_MLA");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        string PONew = "";
                        PONew = i_OldPO.ToString() + "_A";


                        Update1.UpdateString("CWFApproverl1", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl1"].ToString());
                        Update1.UpdateString("CWFApproverl2", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                        Update1.UpdateString("CWFApproverl3", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl3"].ToString());
                        Update1.UpdateString("CWFApproverl4", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl4"].ToString());
                        Update1.UpdateString("CWFVendor", dsOldCaseDetails.Tables[0].Rows[0]["CWFVendor"].ToString());
                        Update1.UpdateString("CWFLocation", dsOldCaseDetails.Tables[0].Rows[0]["CWFLocation"].ToString());
                        Update1.UpdateString("CWFDept", dsOldCaseDetails.Tables[0].Rows[0]["CWFDept"].ToString());
                        Update1.UpdateString("CWFPostingperiod", dsOldCaseDetails.Tables[0].Rows[0]["CWFPostingperiod"].ToString());
                        Update1.UpdateString("CWFSubsidiary", dsOldCaseDetails.Tables[0].Rows[0]["CWFSubsidiary"].ToString());
                        Update1.UpdateString("CWFTerms", dsOldCaseDetails.Tables[0].Rows[0]["CWFTerms"].ToString());
                        Update1.UpdateString("sInvoiceNumber", dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceNumber"].ToString());
                        Update1.UpdateString("DateString2", dsOldCaseDetails.Tables[0].Rows[0]["DateString2"].ToString());
                        Update1.UpdateString("Add_Fld2", dsOldCaseDetails.Tables[0].Rows[0]["Add_Fld2"].ToString());
                        Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["sMemo"].ToString());
                       // Update1.UpdateString("PO_Number", dsOldCaseDetails.Tables[0].Rows[0]["PONumber"].ToString());
                        Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                        Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                        Update1.UpdateString("DateString1", dsOldCaseDetails.Tables[0].Rows[0]["DateString1"].ToString());

                        Update1.UpdateString("B_PO_Adden", PONew.ToString());

                        Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());
                        Update1.AddWhere("ixBug = @ixBug");
                        Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                        Update1.Execute();

                        //  }
                        //   if (bLineItemsCopy == true)
                        //  {
                        if ((dsLineItemsDetails != null) && (dsLineItemsDetails.Tables[0].Rows.Count > 0))
                        {

                            foreach (DataRow dr in dsLineItemsDetails.Tables[0].Rows)
                            {
                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                                insert1.InsertInt("ixBug", ixBug);
                                insert1.InsertString("sAccount", dr["sAccount"].ToString());

                                if (dr["fAmount"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("fAmount", Convert.ToDouble(dr["fAmount"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("fAmount", 0d);
                                    }
                                }
                                insert1.InsertFloat("fTax", Convert.ToDouble(dr["fTax"].ToString()));
                                insert1.InsertString("sDepartment", dr["sDepartment"].ToString());
                                insert1.InsertFloat("sExtra3", Convert.ToDouble(dr["sExtra3"].ToString()));
                                insert1.InsertString("sMemo", dr["sMemo"].ToString());

                                if (dr["iDeleted"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertInt("iDeleted", Convert.ToInt32(dr["iDeleted"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertInt("iDeleted", 0);
                                    }
                                }

                                insert1.InsertInt("iDeleted", Int32.Parse(dr["iDeleted"].ToString()));

                                insert1.Execute();
                            }
                        }
                  //}
          

                    // }
                }
                catch (Exception e)
                {
                    api.Notifications.AddAdminNotification(e.ToString(), "Error in Updating case details.");
                }
            }
        }

        protected void Blanket_Spreadfast()
        {
           // api.Notifications.AddAdminNotification("Blanket", "CopyCase_Spreadfast");

            int ixBug = 0;
        //    int i_OldCaseID = 0;
            string i_OldCaseID1 = "";
      
            // api.Notifications.AddAdminNotification("Raw Page display called", "");
            ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

            CBug bug = api.Bug.GetBug(ixBug);
            bug.IgnorePermissions = true;
            int iproj = bug.ixProject;

            {
                try
                {
                    api.Notifications.AddMessage("Blanket", "Blanket");
                    //if (((api.Request[api.AddPluginPrefix("Blanket_PO")].ToString().Trim()) != null) || ((api.Request[api.AddPluginPrefix("Blanket_PO")].ToString().Trim()) != ""))
                    //{
                     //   api.Notifications.AddMessage("Blanket", api.Request[api.AddPluginPrefix("B_PO")].ToString());
                        //i_OldCaseID = Int32.Parse(api.Request[api.AddPluginPrefix("PONumber")].ToString());
                    i_OldCaseID1 = api.Request[api.AddPluginPrefix("CGSInvoice_MLA")].ToString();
                       // i_OldCaseID = Int32.Parse(i_OldCaseID1.Substring(6));
                      //  api.Notifications.AddAdminNotification("i_OldCaseID", i_OldCaseID1.ToString());

                 //   }
                    api.Notifications.AddMessage("Blanket2", "Blanket2");
                    string tablename1 = api.Database.PluginTableName("CGSInvoice_MLA");
                     CUpdateQuery Update2 = api.Database.NewUpdateQuery(tablename1);
                     Update2.UpdateString("B_PO_ref", i_OldCaseID1.ToString());
                     Update2.AddWhere("ixBug = @ixBug");
                     Update2.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                     Update2.Execute();

                     api.Notifications.AddMessage("Blanket4", "Blanket4");


                     CSelectQuery sqlInvoiceDetails;
                     sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                     sqlInvoiceDetails.AddSelect("*");
                     sqlInvoiceDetails.AddWhere("ixBug =" + ixBug.ToString() + " AND iDeleted = 0");

                     DataSet dsExistinglineItems = sqlInvoiceDetails.GetDataSet();

                     if (dsExistinglineItems != null)
                     {
                         if (dsExistinglineItems.Tables[0].Rows.Count > 0)
                         {

                             api.Notifications.AddAdminNotification("Line Items exist for this case, you can not copy from other cases", "");

                             return;
                         }
                     }

                     DataSet dsOldCaseDetails = new DataSet();
                    // api.Notifications.AddAdminNotification("projid1", iproj.ToString());

            
                  
                     sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                     sqlInvoiceDetails.AddSelect("max(ixBug)");
                     sqlInvoiceDetails.AddWhere("B_PO_Number = " + i_OldCaseID1.ToString());
                     sqlInvoiceDetails.AddWhere("ixproject = 27");
                    // api.Notifications.AddAdminNotification("i_OldCaseID13", i_OldCaseID1.ToString());

                     object BugId = sqlInvoiceDetails.GetScalarValue();
                    int Bugid = Convert.ToInt32(BugId);
                    api.Notifications.AddAdminNotification("Bugid", Bugid.ToString());
                     api.Notifications.AddAdminNotification("Bugid_SF", Bugid.ToString());
                     sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                     sqlInvoiceDetails.AddSelect("*");
                     sqlInvoiceDetails.AddWhere("ixBug =" + Bugid.ToString());
                     dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();

                    
           

                    //api.Notifications.AddAdminNotification("copycase_SF", dsOldCaseDetails.ToString());

                     if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                     {
                         api.Notifications.AddMessage("This Blanket PO is not valid");
                         return;
                     }

                     sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                     sqlInvoiceDetails.AddSelect("*");
                     sqlInvoiceDetails.AddWhere("ixBug =" + Bugid.ToString() + " AND iDeleted = 0");

                    DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();
                   // api.Notifications.AddAdminNotification("dsLineItemsDetails", dsLineItemsDetails.ToString());
                    string s_OldProjectID = "";

                    CSelectQuery sq = api.Database.NewSelectQuery("bug");
                    sq.AddSelect("Bug.ixProject");
                    sq.AddWhere("ixBug = " + Bugid.ToString());
                    DataSet ds = sq.GetDataSet();
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                    {
                        s_OldProjectID = ds.Tables[0].Rows[0][0].ToString();
                    }
                    ds.Dispose();


                        string tablename = api.Database.PluginTableName("CGSInvoice_MLA");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        Update1.UpdateString("POAmt", dsOldCaseDetails.Tables[0].Rows[0]["Add_Fld2"].ToString());
                      //  api.Notifications.AddAdminNotification("POAmt1", dsOldCaseDetails.Tables[0].Rows[0]["Add_Fld2"].ToString());
                        //Update1.UpdateString("B_PO_Number", dsOldCaseDetails.Tables[0].Rows[0]["B_PO_Number"].ToString());
                        Update1.UpdateString("POBalanceAmt", dsOldCaseDetails.Tables[0].Rows[0]["POBalanceAmt"].ToString());
                      //  api.Notifications.AddAdminNotification("POBalanceAmt1", dsOldCaseDetails.Tables[0].Rows[0]["POBalanceAmt"].ToString());
                        //Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());
                        Update1.AddWhere("ixBug = @ixBug");
                        Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                      //  api.Notifications.AddAdminNotification("Currentbug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]).ToString());
                        Update1.Execute();

                        api.Notifications.AddAdminNotification("Currentbug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]).ToString());
                        //double balance_amt = 0;
                        //double Total_Amt = 0;
                        //double PO_Amt = 0;
                        //api.Notifications.AddAdminNotification("POBalanceAmt2", "POBalanceAmt2");
                        //Total_Amt = Convert.ToDouble(dsOldCaseDetails.Tables[0].Rows[0]["Add_Fld2"].ToString());
                        //api.Notifications.AddAdminNotification("Total_Amt", Total_Amt.ToString());

                        //CSelectQuery BPO_Amt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        //BPO_Amt.AddSelect("sum(Add_Fld2)");
                        //string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".B_PO_ref = " + i_OldCaseID1.ToString();

                        //BPO_Amt.AddWhere(sWhere2);
                        //object Bamt = BPO_Amt.GetScalarValue();
                        //PO_Amt = Convert.ToDouble(Bamt);
                        //api.Notifications.AddAdminNotification("PO_Amt", PO_Amt.ToString());
                        //balance_amt = (Total_Amt) - (PO_Amt);
                        //bug.SetPluginField(PLUGIN_ID, "POBalanceAmt", balance_amt.ToString("C"));

                        //api.Notifications.AddAdminNotification("POBalanceAmt", balance_amt.ToString());
                }
                catch (Exception e)
                {
                    api.Notifications.AddAdminNotification(e.ToString(), "Error in Updating case details.");
                }
            }
        }

        public double func_BPOBalanceAMount(string BPO_ref)
        {
            //string i_OldCaseID = "";
            //i_OldCaseID = api.Request[api.AddPluginPrefix("CGSInvoice_MLA")].ToString();
           // api.Notifications.AddAdminNotification("POBalanceAmt1", "POBalanceAmt1");
            double balance_amt = 0;
            double BPO_Amt = 0;
            double BPO_Ref_Amt = 0;
           // api.Notifications.AddAdminNotification("POBalanceAmt2", "POBalanceAmt2");

            //CSelectQuery BPOBlanceAmt;
            //BPOBlanceAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
            //BPOBlanceAmt.AddSelect("Add_Fld2");
            //BPOBlanceAmt.AddWhere("B_PO_Number = " + i_OldCaseID.ToString());
            //BPOBlanceAmt.AddWhere("ixproject = 27");
            //object Bpoamt = BPOBlanceAmt.GetScalarValue();
            //BPO_Amt = Convert.ToDouble(Bpoamt);


           // api.Notifications.AddAdminNotification("Total_Amt", BPO_Amt.ToString());

            CSelectQuery PO_TotalAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
            PO_TotalAmt.AddSelect("sum(Add_Fld2)");
            string sWhere2 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".B_PO_ref = " + BPO_ref.ToString();
            string sWhere3 = (api.Database.PluginTableName("CGSInvoice_MLA")) + ".PO_Number IS NOT NULL";
            PO_TotalAmt.AddWhere(sWhere2);
            PO_TotalAmt.AddWhere(sWhere3);
            object Bamt = PO_TotalAmt.GetScalarValue();
            BPO_Ref_Amt = Convert.ToDouble(Bamt);
           // api.Notifications.AddAdminNotification("PO_Amt", BPO_Ref_Amt.ToString());
           // balance_amt = (BPO_Amt) - (BPO_Ref_Amt);
            
          //  api.Notifications.AddAdminNotification("POBalanceAmt", balance_amt.ToString());
            return (BPO_Ref_Amt);
        }

        protected void CopyCase()
        {
            int ixBug = 0;
            int i_OldCaseID = 0;
            bool bHeaderCopy = false;
            bool bLineItemsCopy = false;
            string sProj = "";
            string Vname = "";


            // api.Notifications.AddAdminNotification("Raw Page display called", "");
            ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

            CBug bug = api.Bug.GetBug(ixBug);
            bug.IgnorePermissions = true;
            int iproj = bug.ixProject;

            {
                try
                {
                    if (((api.Request[api.AddPluginPrefix("CaseID")].ToString().Trim()) == null) || ((api.Request[api.AddPluginPrefix("CaseID")].ToString().Trim()) == "") || ((api.Request[api.AddPluginPrefix("CWFVendor")].ToString().Trim()) != "-"))
                    {
                        Vname = (api.Request[api.AddPluginPrefix("CWFVendor")].ToString());
                        Vname = Vname.Replace("'", "''");
                    }
                    else
                    {
                        i_OldCaseID = Int32.Parse(api.Request[api.AddPluginPrefix("CaseID")].ToString());
                    }

                    try
                    {
                        bHeaderCopy = Boolean.Parse(api.Request[api.AddPluginPrefix("Header")].ToString().Trim());
                    }
                    catch
                    {
                        bHeaderCopy = false;
                    }

                    try
                    {
                        bLineItemsCopy = Boolean.Parse(api.Request[api.AddPluginPrefix("LineItems")].ToString().Trim());
                    }
                    catch
                    {
                        bLineItemsCopy = false;
                    }

                    CSelectQuery sqlInvoiceDetails;
                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + ixBug.ToString() + " AND iDeleted = 0");

                    DataSet dsExistinglineItems = sqlInvoiceDetails.GetDataSet();

                    if (dsExistinglineItems != null)
                    {
                        if (dsExistinglineItems.Tables[0].Rows.Count > 0)
                        {

                            api.Notifications.AddAdminNotification("Line Items exist for this case, you can not copy from other cases", "");

                            return;
                        }
                    }

                    DataSet dsOldCaseDetails = new DataSet();
                    if (Vname.Trim() != "")
                    {
                        //api.Notifications.AddAdminNotification("Vname1", Vname.ToString());
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("max(ixBug)");
                        sqlInvoiceDetails.AddWhere("CWFVendor =" + "'" + Vname.ToString() + "'");


                        object VID = sqlInvoiceDetails.GetScalarValue();
                        i_OldCaseID = Convert.ToInt32(VID);
                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();
                    }

                    else
                    {

                        sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice_MLA"));
                        sqlInvoiceDetails.AddSelect("*");
                        sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString());
                        dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();
                    }

                    if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                    {
                        api.Notifications.AddMessage("This Case ID is not valid");
                        return;
                    }

                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString() + " AND iDeleted = 0");

                    DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                    string s_OldProjectID = "";

                    CSelectQuery sq = api.Database.NewSelectQuery("bug");
                    sq.AddSelect("Bug.ixProject");
                    sq.AddWhere("ixBug = " + i_OldCaseID.ToString());
                    DataSet ds = sq.GetDataSet();
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                    {
                        s_OldProjectID = ds.Tables[0].Rows[0][0].ToString();
                    }
                    ds.Dispose();


                    sProj = iproj.ToString();

                    if (s_OldProjectID != sProj)
                    {

                        api.Notifications.AddMessage("The Older case id must belong to the same project");
                        return;
                    }

                    if (bHeaderCopy == true)
                    {

                        string tablename = api.Database.PluginTableName("CGSInvoice_MLA");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        Update1.UpdateString("CWFApproverl1", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl1"].ToString());
                        Update1.UpdateString("CWFApproverl2", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                        Update1.UpdateString("CWFApproverl3", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl3"].ToString());
                        Update1.UpdateString("CWFApproverl4", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl4"].ToString());
                        Update1.UpdateString("CWFCustomform", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomform"].ToString());
                        Update1.UpdateString("CWFVendor", dsOldCaseDetails.Tables[0].Rows[0]["CWFVendor"].ToString());
                        Update1.UpdateString("CWFCountry", dsOldCaseDetails.Tables[0].Rows[0]["CWFCountry"].ToString());
                        Update1.UpdateString("CWFCurrency", dsOldCaseDetails.Tables[0].Rows[0]["CWFCurrency"].ToString());
                        Update1.UpdateString("CWFPostingperiod", dsOldCaseDetails.Tables[0].Rows[0]["CWFPostingperiod"].ToString());
                        Update1.UpdateString("CWFSubsidiary", dsOldCaseDetails.Tables[0].Rows[0]["CWFSubsidiary"].ToString());
                        Update1.UpdateString("CWFTerms", dsOldCaseDetails.Tables[0].Rows[0]["CWFTerms"].ToString());
                        Update1.UpdateString("sInvoiceNumber", dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceNumber"].ToString());
                        Update1.UpdateString("sExchangeRate", dsOldCaseDetails.Tables[0].Rows[0]["sExchangeRate"].ToString());
                        Update1.UpdateString("AccountDesc", dsOldCaseDetails.Tables[0].Rows[0]["AccountDesc"].ToString());
                       // Update1.UpdateString("TotalAmount", dsOldCaseDetails.Tables[0].Rows[0]["TotalAmount"].ToString());
                     //   Update1.UpdateString("Netamount", dsOldCaseDetails.Tables[0].Rows[0]["Netamount"].ToString());

                        //if (dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceAmount"] != null)
                        //{
                        //    try
                        //    {
                        //        Update1.UpdateFloat("sInvoiceAmount", Convert.ToDouble(dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceAmount"].ToString()));
                        //    }
                        //    catch
                        //    {
                        //        Update1.UpdateFloat("sInvoiceAmount", 0d);
                        //    }
                        //}



                        //if (dsOldCaseDetails.Tables[0].Rows[0]["sTaxAmount"] != null)
                        //{
                        //    try
                        //    {
                        //        Update1.UpdateFloat("sTaxAmount", Convert.ToDouble(dsOldCaseDetails.Tables[0].Rows[0]["sTaxAmount"].ToString()));
                        //    }
                        //    catch
                        //    {
                        //        Update1.UpdateFloat("sTaxAmount", 0d);
                        //    }
                        //}

                        Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["sMemo"].ToString());
                        Update1.UpdateString("sAddInfo", dsOldCaseDetails.Tables[0].Rows[0]["sAddInfo"].ToString());
                        Update1.UpdateString("CCUser", dsOldCaseDetails.Tables[0].Rows[0]["CCUser"].ToString());
                        Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                        Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                        Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());
                        Update1.AddWhere("ixBug = @ixBug");
                        Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));
                        Update1.Execute();

                    }
                    if (bLineItemsCopy == true)
                    {
                        if ((dsLineItemsDetails != null) && (dsLineItemsDetails.Tables[0].Rows.Count > 0))
                        {

                            foreach (DataRow dr in dsLineItemsDetails.Tables[0].Rows)
                            {
                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                                insert1.InsertInt("ixBug", ixBug);
                                insert1.InsertString("sAccount", dr["sAccount"].ToString());
                                insert1.InsertString("sTaxtype", dr["sTaxtype"].ToString());

                                //if (dr["fAmount"] != null)
                                //{
                                //    try
                                //    {
                                //        insert1.InsertFloat("fAmount", Convert.ToDouble(dr["fAmount"].ToString()));
                                //    }
                                //    catch
                                //    {
                                //        insert1.InsertFloat("fAmount", 0d);
                                //    }
                                //}


                                //if (dr["fTax"] != null)
                                //{
                                //    try
                                //    {
                                //        insert1.InsertFloat("fTax", Convert.ToDouble(dr["fTax"].ToString()));
                                //    }
                                //    catch
                                //    {
                                //        insert1.InsertFloat("fTax", 0d);
                                //    }
                                //}

                                insert1.InsertString("sMemo", dr["sMemo"].ToString());
                                insert1.InsertString("sDepartment", dr["sDepartment"].ToString());
                                insert1.InsertString("sBillable", dr["sBillable"].ToString());
                                insert1.InsertString("sAddninfo", dr["sAddninfo"].ToString());

                                if (dr["iDeleted"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertInt("iDeleted", Convert.ToInt32(dr["iDeleted"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertInt("iDeleted", 0);
                                    }
                                }

                                insert1.InsertInt("iDeleted", Int32.Parse(dr["iDeleted"].ToString()));

                                if (dr["ixExtra1"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("ixExtra1", Convert.ToDouble(dr["ixExtra1"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("ixExtra1", 0d);
                                    }
                                }

                                insert1.InsertString("sExtra2", dr["sExtra2"].ToString());
                                if (dr["sExtra3"] != null)
                                {
                                    try
                                    {
                                        insert1.InsertFloat("sExtra3", Convert.ToDouble(dr["sExtra3"].ToString()));
                                    }
                                    catch
                                    {
                                        insert1.InsertFloat("sExtra3", 0d);
                                    }
                                }
                                insert1.InsertString("sExtra4", dr["sExtra4"].ToString());
                                insert1.InsertString("sExtra5", dr["sExtra5"].ToString());
                                insert1.InsertString("sExtra6", dr["sExtra6"].ToString());
                                insert1.Execute();
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    api.Notifications.AddAdminNotification(e.ToString(), "Error in Updating case details.");
                }
            }
        }

        #endregion

        /* use the PluginRawPage to handle AJAX requests when the user adds, edits or
         * deletes Items. */

        #region IPluginRawPageDisplay Members

        public string RawPageDisplay()
        {
            /* check for a valid action token in the request before processing to
                * prevent cross site request forgery */
            string check = "";
            string ixProject = "";
            if ((api.Request[api.AddPluginPrefix("sAction")] != null) &&
                (api.Request[api.AddPluginPrefix("actionToken")] != null) &&
                api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")]))
            {
                int ixLineItem = -1;
                //api.Notifications.AddAdminNotification("reached to this point", "");
                switch (api.Request[api.AddPluginPrefix("sAction")].ToString())
                {

                    case "new":
                        InsertItem();
                        break;

                    case "new_1":
                        InsertItem_cambridge();
                        break;

                    case "new_2":
                        //api.Notifications.AddAdminNotification("Calling", "InsertItem_TE");
                        InsertItem_TE();
                        break;

                    case "new_3":
                        //  api.Notifications.AddAdminNotification("Calling", "InsertItem_Synergis");
                        InsertItem_Synergis();
                        break;

                    case "new_4":
                        //  api.Notifications.AddAdminNotification("Calling", "InsertItem_Synergis");
                        InsertItem_Synergis();
                        break;

                    case "new_5":
                        //  api.Notifications.AddAdminNotification("Calling", "InsertItem_Spreadfast");
                        InsertItem_Spreadfast();
                        break;

                    //case "new_POInvoice":
                    //    //  api.Notifications.AddAdminNotification("Calling", "InsertItem_Spreadfast");
                    //    InsertItem_Spreadfast();
                    //    break;


                    case "edit":
                        UpdateItem();
                        break;

                    case "delete":
                        DeleteItem();
                        break;

                    case "delete_Syn":
                        DeleteItem_Synergis();
                        break;

                    case "copycase_1":
                        CopyCase();
                        break;

                    case "PE_copycase":
                        CopyCase_Cambridge();
                        break;

                    case "Syn_copycase":
                        CopyCase_Synergis();
                        break;

                    case "Syn_Addendum":
                        Addendum_Synergis();
                        break;

                    case "Spreadfast_copycase":
                        CopyCase_Spreadfast();
                        break;

                    case "SF_Blanket_PO":
                        Blanket_Spreadfast();
                        break;

                    case "Spreadfast_Addendum":
                        Addendum_Spreadfast();
                        break;

                    case "new_POInvoice":
                        check = "PMI";
                        //api.Notifications.AddAdminNotification("Calling", "new_POInvoice");
                        InsertItem_POInvoice();
                        break;
                    case "edit_POInvoice":
                        UpdateItem_POInvoice();
                        break;
                    case "delete_POInvoice":
                     //   api.Notifications.AddAdminNotification("Calling", "Delete_Synergis");
                        DeleteItem_POInvoice();
                        break;

                    case "geteditdialog":
                        if ((api.Request[api.AddPluginPrefix("ixLineItem")] != null) &&
                            (Int32.TryParse(api.Request[api.AddPluginPrefix("ixLineItem")].ToString(), out ixLineItem)) &&
                            (ixLineItem > 0))
                        {
                            string sTableId1 = api.Request[api.AddPluginPrefix("sTableId")].ToString();
                            ixProject = api.Request[api.AddPluginPrefix("ixProject")].ToString();
                            return GetEditDialog(ixLineItem, sTableId1, ixProject);
                        }
                        break;
                }
            }

            /* return the updated table as xml so FogBugz   can update the page */
            api.Response.ContentType = "text/xml";
            if (api.Request[api.AddPluginPrefix("sAction")].ToString() != "copycase_1")
            {
                if (api.Request[api.AddPluginPrefix("sAction")].ToString() != "PE_copycase")
                {
                    if (api.Request[api.AddPluginPrefix("sAction")].ToString() != "Syn_copycase")
                    {

                        if (api.Request[api.AddPluginPrefix("sAction")].ToString() != "Syn_Addendum")
                        {
                            if (api.Request[api.AddPluginPrefix("sAction")].ToString() != "Spreadfast_copycase")
                            {
                                if (api.Request[api.AddPluginPrefix("sAction")].ToString() != "SF_Blanket_PO")
                                {
                                    if (api.Request[api.AddPluginPrefix("sAction")].ToString() != "Spreadfast_Addendum")
                                    {
                                        ixProject = api.Request[api.AddPluginPrefix("ixProject")].ToString();
                                        if (ixProject == "14")
                                        {
                                            return ItemTable(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                        }
                                        else if (ixProject == "19")
                                        {
                                            return ItemTable_Cambridge(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                             Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                        }

                                        else if (ixProject == "23")
                                        {
                                            // api.Notifications.AddAdminNotification("1b", "1b");
                                            return ItemTable_TE(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                              Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                        }

                                        else if (ixProject == "25" )
                                        {
                                            if (check == "PMI")
                                            {
                                                //api.Notifications.AddAdminNotification("1bstatus", "Approved");
                                                return ItemTable_POInvoiceDetails(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                                  Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                            }
                                            else
                                            {
                                                // api.Notifications.AddAdminNotification("1b", "1b");
                                                return ItemTable_synergs(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                                  Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                            }
                                        }
                                        else if (ixProject == "26")
                                        {
                                            if (check == "PMI")
                                            {
                                                //api.Notifications.AddAdminNotification("1bstatus", "Approved");
                                                return ItemTable_POInvoiceDetails(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                                  Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                            }
                                            else
                                            {
                                                // api.Notifications.AddAdminNotification("1b", "1b");
                                                return ItemTable_synergs_Artium(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                                  Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                            }
                                        }
                                        else if (ixProject == "27")
                                        {
                                            //api.Notifications.AddAdminNotification("1bstatus", api.Request[api.AddPluginPrefix("ixStatus")].ToString());
                                            if (check == "PMI")
                                            {
                                                //api.Notifications.AddAdminNotification("1bstatus", "Approved");
                                                return ItemTable_POInvoiceDetails(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                                  Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                            }
                                            else
                                            {
                                                //api.Notifications.AddAdminNotification("1bstatus", "Pending");
                                                return ItemTable_Spreadfast(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                                                                                                Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
                                            }

                                        }
                                        else
                                        {
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public PermissionLevel RawPageVisibility()
        {
            return PermissionLevel.Normal;
        }

        public string GetEditDialog(int ixLineItem, string sTableId, string ixProject)
        {
            return DialogEditForAjax(ixLineItem, sTableId, ixProject).RenderHtml();
        }

        #endregion

        #region beautifier
        protected CEditableTable Headings_1(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            CEditableTable Heading_1 = new CEditableTable("Headings");
            string sTableId = Heading_1.sId;
            Heading_1.Header.AddCell("Invoice Approval Status Information");
            return (Heading_1);
        }

        #endregion
        
        #region Item methods

        #region IPluginJS Members

        // this js comes down on every page. I put this here instead of in the static js file
        // because we need the pluginprefix for this plugin (which is based on when the plugin is
        // installed. alternatively, we could just set a variable for it in JS-land and put the
        // rest of these functions into the static js file
        public CJSInfo JSInfo()
        {
            
            CJSInfo jsInfo = new CJSInfo();
            // most of the javascript for the plugin is static:
            jsInfo.rgsStaticFiles = new string[] { @"js\Experiments.js" };
            // this method is defined dynamically here so we can have the plugin prefix and
            // action token
            jsInfo.sInlineJS = @"
            function GetEditDialogUrl(ixLineItem,sTableId,ixProject)
            {
                return '" + AJAXUrl("geteditdialog") + @"' + '&" + api.PluginPrefix + @"ixLineItem=' + ixLineItem + '&" + api.PluginPrefix + @"sTableId=' + sTableId + '&" + api.PluginPrefix + @"ixProject=' + ixProject;

            }";
            return jsInfo;
        }

        #endregion

        protected CEditableTable ItemTable(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            if (ixProject != 14)
            {
                return null;
            }
            CEditableTable editableTableItems = new CEditableTable("itemtable");
            //string sTableId = editableTableItems.sId;
            sTableId = editableTableItems.sId;

            string str1 = "GL Account", str2 = "Amount", str3 = "Tax type", str4 = "Tax",
            str5 = "Memo", str6 = "Department", 
            //str7 = "Billable", //changed by poornima 
            str7 = "Prepaid",
            str8 = "Addninfo";
              /* Define the header row of the table */
                if (!bSuppressEditsAndDeletes)
                {
                    editableTableItems.Header.AddCell("Edit");
                    editableTableItems.Header.AddCell("Delete");
                }
                editableTableItems.Header.AddCell(str1);
                editableTableItems.Header.AddCell(str2);
                editableTableItems.Header.AddCell(str4);
                editableTableItems.Header.AddCell(str3);
                //editableTableItems.Header.AddCell("Form 99");
                editableTableItems.Header.AddCell(str5);
                editableTableItems.Header.AddCell(str6);
                editableTableItems.Header.AddCell(str7);
                editableTableItems.Header.AddCell(str8);
                //editableTableItems.Header.AddCell(str9);
                //editableTableItems.Header.AddCell(str10);
                //editableTableItems.Header.AddCell(str11);
                //editableTableItems.Header.AddCell(str9);


                /* this variable means we don't need to mess with colspans later in the code */
                int nColCount = editableTableItems.Header.Cells.Count;

                /* Create the edit dialog template object used when the user clicks the
                 * edit icon in a particular row */

                //CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sTableId, ixProject);

                //CDialogTemplate dlgTemplateEdit = DialogTemplateEditLineItem(sTableId, ixProject,ixBug,ixLineItem);

                /* Create the new item dialog template object used when the user clicks Add
                 * New Item or the add icon in the footer row */

                CDialogTemplate dlgTemplateNew = DialogTemplateNew(ixBug, ixProject, sTableId);

                /* Create the delete dialog template object used when the user clicks the
                 * delete icon in a particular row */
                CDialogTemplate dlgTemplateDelete = DialogTemplateDelete(sTableId, ixProject);

                //Created by Alok for new type of Edit dialog box

                //-----------------

                /* setup a DataSet and fetch the items from the database */
                DataSet dsItems = FetchItems(ixBug, true);
                int ixBugLineItem = -1;
                /* int ixGlAccount = -1;
                 int ixGlDepartment = -1;
                 int ixGlLocation = -1;
                 int ixGlProject = -1;
                 int ixGlClass = -1;
                 int ixGlItem = -1;
                 //int iForm99 = -1;
                 * */
                double dAmount = -1D;
                double dTax = -1D;
                string sMemo = "";
                string sAccount = "";
                string sDepartment = "";
                string sBillable = "";
                string sAddninfo = "";
                string sTaxtype = "";
                //string sExtra2 = "";
                //string sExtra4 = "";
                //string sExtra5 = "";
                //string sExtra6 = "";

                /*    
                 string sGlProjectName = "";
                 string sGlClassName = "";
                 string sGlItemName = "";
                */
                /* If the DataSet contains any rows, loop through them and populate the table
                 * and dialog template data Hashtables */

                if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                    {

                        //api.Notifications.AddAdminNotification("Trying to load the items", "This is not loading whu?");
                        ixBugLineItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugLineItem"]);
                        sAccount = Convert.ToString(dsItems.Tables[0].Rows[i]["sAccount"]).Trim();
                        ixBug = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBug"]);
                        sTaxtype = Convert.ToString(dsItems.Tables[0].Rows[i]["sTaxtype"]);
                        sDepartment = Convert.ToString(dsItems.Tables[0].Rows[i]["sDepartment"]);
                        // sDepartment = Convert.ToString(dsItems.Tables[0].Rows[i]["sDepartment"]);
                        sBillable = Convert.ToString(dsItems.Tables[0].Rows[i]["sBillable"]);
                        sAddninfo = Convert.ToString(dsItems.Tables[0].Rows[i]["sAddninfo"]);
                        sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                        //sExtra2 = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra2"]);
                        //sExtra4 = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra4"]);
                        //sExtra5 = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra5"]);
                        // sExtra6 = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra6"]);


                        //for the editable table drop downs 
                        sAccount_P = sAccount.Replace("&", "/&");
                        sTaxtype_P = sTaxtype;
                        sDepartment_P = sDepartment;
                        sBillable_P = sBillable;

                        //iForm99 = Convert.ToInt32(dsItems.Tables[0].Rows[i]["iForm99"]);
                        // sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                        try
                        {
                            dAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                        }
                        catch
                        {
                            dAmount = 0d;
                        }

                        try
                        {
                            dTax = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fTax"]);
                        }
                        catch
                        {
                            dTax = 0d;
                        }

                        //sGlAccountName = dsItems.Tables[0].Rows[i]["sGlAccountName"].ToString();

                        /* create a new table row and set the row id to the unique ixBugInvoiceItem */
                        CEditableTableRow row = new CEditableTableRow();
                        row.sRowId = ixBugLineItem.ToString();
                        int ixLineItem = ixBugLineItem;
                        /* CEditableTable provides standard edit and delete icon links.
                         * The second parameter is the name of the dialog to open. The fourth
                         * parameter is the URL to link to if javascript is not available.
                         * Note: we do not provide a non-javascript mode in this example. */
                        if (!bSuppressEditsAndDeletes)
                        {
                            row.AddCell(string.Format("<a href=\"#\" ixLineItem=\"{0}\" sTableId=\"{2}\" ixProject=\"{3}\"  onclick=\"ExamplePlugin.doPopup(this); return false;\">{1}</a>",
                                                ixLineItem.ToString(),
                                               FogCreek.FogBugz.UI.Icons.EditIcon(), sTableId.ToString(), ixProject.ToString()));
                            row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                                            sTableId,
                                            "delete",
                                            row.sRowId,
                                            CommandUrl("delete", ixBugLineItem, ixBug.ToString())));
                        }
                        /* make sure to run HtmlEncode on any user data before displaying it! */
                        row.AddCell(HttpUtility.HtmlEncode(sAccount));

                        row.AddCell(HttpUtility.HtmlEncode(dAmount.ToString()));
                        row.AddCell(HttpUtility.HtmlEncode(dTax.ToString()));
                        row.AddCell(HttpUtility.HtmlEncode(sTaxtype));
                        row.AddCell(HttpUtility.HtmlEncode(sMemo.ToString()));
                        row.AddCell(HttpUtility.HtmlEncode(sDepartment));
                        row.AddCell(HttpUtility.HtmlEncode(sBillable));
                        row.AddCell(HttpUtility.HtmlEncode(sAddninfo));
                        //row.AddCell(HttpUtility.HtmlEncode(sExtra2));
                        //row.AddCell(HttpUtility.HtmlEncode(sExtra4));
                        //row.AddCell(HttpUtility.HtmlEncode(sExtra5));
                        //row.AddCell(HttpUtility.HtmlEncode(sExtra6));
                        editableTableItems.Body.AddRow(row);

                        if (!bSuppressEditsAndDeletes)
                        {
                            /* Now that the row is populated for display, put the data in a hash table
                             * to be used in populating the pop-up add, edit and delete dialogs. */
                            Hashtable hashData = new Hashtable();
                            hashData.Add("ixBugLineItem", ixBugLineItem);
                            hashData.Add("ixBug", ixBug);
                            //hashData.Add("sAccount", sAccount);                         
                            //hashData.Add("fAmount", dAmount);
                            //hashData.Add("fTax", dTax);
                            //hashData.Add("sTaxtype", sTaxtype);
                            //hashData.Add("sMemo", sMemo);
                            //hashData.Add("sDepartment", sDepartment);
                            //hashData.Add("sBillable", sBillable);
                            //hashData.Add("sAddninfo", sAddninfo);
                            //hashData.Add("sExtra2", sExtra2);
                            //hashData.Add("sExtra4", sExtra4);
                            //hashData.Add("sExtra5", sExtra5);
                            //hashData.Add("sExtra6", sExtra6);                        

                            /* add the hash table as data to the edit template */
                            //dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                            /* add the data to the delete template as well */
                            dlgTemplateDelete.AddTemplateData(row.sRowId, hashData);
                        }

                    }
                }
                else
                {
                    /* If there are no items, just display a note in a full-width cell */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = "none";
                    row.AddCellWithColspan("No Items Yet...", nColCount);
                    editableTableItems.Body.AddRow(row);
                }

                dsItems.Dispose();

                if (!bSuppressEditsAndDeletes)
                {
                    /* Add a footer row with icon and text links to the add new item dialog */
                    editableTableItems.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                                                            sTableId,
                                                            "new",
                                                            "sDataId",
                                                            CommandUrl("new", ixBugLineItem, ixBug.ToString())));
                    editableTableItems.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                            sTableId,
                                                            "new",
                                                            "sDataId",
                                                            CommandUrl("new", ixBugLineItem, ixBug.ToString()),
                                                            "Add New Item"),
                                                            nColCount - 1);

                    /* Associate the dialog templates with the table by name */
                    editableTableItems.AddDialogTemplate("new", dlgTemplateNew);
                    //Changed by Alok
                    //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                    //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                    //End
                    editableTableItems.AddDialogTemplate("delete", dlgTemplateDelete);
                }

                return editableTableItems;

            }

        protected CEditableTable ItemTable_synergs(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            if (ixProject != 25)
            {
                return null;
            }
          //  api.Notifications.AddAdminNotification("1", "1");

            CEditableTable editableTableItems = new CEditableTable("itemtable_3");
            //string sTableId = editableTableItems.sId;
            sTableId = editableTableItems.sId;

            string str5 = "Description", str1 = "Account Label", str6 = "Project Code", str3 = "Quantity", str2 = "Unit Price",
             str4 = "Total Amount" //;
             , str7="LineItemId" , str8= "Invoice Amount", str9="LineItem Balance amount";  //now entered
       
            //string str1 = "Account Label", str2 = "Unit Price", str3 = "Quantity",
            //  str4 = "Total Amount",  //now entered
            //str5 = "Description", str6 = "Project Code";
                //str7 = "Billable", //changed by poornima 
            //str7 = "Prepaid",
            //str8 = "Addninfo";
            /* Define the header row of the table */
            if (!bSuppressEditsAndDeletes)
            {
               // api.Notifications.AddAdminNotification("2", "2");
                editableTableItems.Header.AddCell("Edit");
                editableTableItems.Header.AddCell("Delete");
            }
            editableTableItems.Header.AddCell(str7);
            editableTableItems.Header.AddCell(str5);
            editableTableItems.Header.AddCell(str1);
            editableTableItems.Header.AddCell(str6);
            editableTableItems.Header.AddCell(str3);
            editableTableItems.Header.AddCell(str2);
            editableTableItems.Header.AddCell(str4);
            editableTableItems.Header.AddCell(str8);
            editableTableItems.Header.AddCell(str9);
            
             //now entered


            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTableItems.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sTableId, ixProject);

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEditLineItem(sTableId, ixProject,ixBug,ixLineItem);

            /* Create the new item dialog template object used when the user clicks Add
             * New Item or the add icon in the footer row */

            CDialogTemplate dlgTemplateNew = DialogTemplateNew_synergs(ixBug, ixProject, sTableId);

            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            CDialogTemplate dlgTemplateDelete = DialogTemplateDelete_Synergis(sTableId, ixProject);

            //Created by Alok for new type of Edit dialog box

            //-----------------

            /* setup a DataSet and fetch the items from the database */
            DataSet dsItems = FetchItems(ixBug, true);
            int ixBugLineItem = -1;
            /* int ixGlAccount = -1;
             int ixGlDepartment = -1;
             int ixGlLocation = -1;
             int ixGlProject = -1;
             int ixGlClass = -1;
             int ixGlItem = -1;
             //int iForm99 = -1;
             * */
            double UnitPrice = -1D;
            int quantity = 0;
            string Description = "";
            string sAccount = "";
            string sprojectcode = "";
            double TotalAmount = -1D;
            int LineItemId = 0;

            double InvoiceTotalAmount = -1D;
            double BalanceAmount = -1D;

            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */

            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
               // api.Notifications.AddAdminNotification("3", "3");
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {

                    //api.Notifications.AddAdminNotification("Trying to load the items", "This is not loading whu?");
                    ixBugLineItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugLineItem"]);
                    sAccount = Convert.ToString(dsItems.Tables[0].Rows[i]["sAccount"]).Trim();
                    ixBug = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBug"]);
                    sprojectcode = Convert.ToString(dsItems.Tables[0].Rows[i]["sDepartment"]);
                    Description = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                    quantity = Convert.ToInt32(dsItems.Tables[0].Rows[i]["fTax"]);
                    TotalAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["sExtra3"]);
                    LineItemId = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixLineItemId"]);

                    
                    
                    //for the editable table drop downs 
                    sAccount_P = sAccount.Replace("&", "/&");
                    sDepartment_P = sprojectcode;
                  
                    try
                    {
                        UnitPrice = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                    }
                    catch
                    {
                        UnitPrice = 0d;
                    }

                    try
                    {
                        InvoiceTotalAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["IvnBalanceAmt"]);
                    }
                    catch
                    {
                        InvoiceTotalAmount = 0d;
                    }

                    try
                    {
                        BalanceAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fLineBalanceAmt"]);
                    }
                    catch
                    {
                        BalanceAmount = 0d;
                    }
                    //sGlAccountName = dsItems.Tables[0].Rows[i]["sGlAccountName"].ToString();

                    /* create a new table row and set the row id to the unique ixBugInvoiceItem */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = ixBugLineItem.ToString();
                    int ixLineItem = ixBugLineItem;
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    if (!bSuppressEditsAndDeletes)
                    {
                        row.AddCell(string.Format("<a href=\"#\" ixLineItem=\"{0}\" sTableId=\"{2}\" ixProject=\"{3}\"  onclick=\"ExamplePlugin.doPopup(this); return false;\">{1}</a>",
                                            ixLineItem.ToString(),
                                           FogCreek.FogBugz.UI.Icons.EditIcon(), sTableId.ToString(), ixProject.ToString()));
                        row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                                        sTableId,
                                        "delete",
                                        row.sRowId,
                                        CommandUrl("delete", ixBugLineItem, ixBug.ToString())));
                    }
                    /* make sure to run HtmlEncode on any user data before displaying it! */
                    row.AddCell(HttpUtility.HtmlEncode(LineItemId.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(Description.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sAccount));
                    row.AddCell(HttpUtility.HtmlEncode(sprojectcode));
                    row.AddCell(HttpUtility.HtmlEncode(quantity.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(UnitPrice.ToString("C")));
                    row.AddCell(HttpUtility.HtmlEncode(TotalAmount.ToString("C")));

                    row.AddCell(HttpUtility.HtmlEncode(InvoiceTotalAmount.ToString("C")));
                    row.AddCell(HttpUtility.HtmlEncode(BalanceAmount.ToString("C")));
                    
                                        
                    editableTableItems.Body.AddRow(row);
                    //api.Notifications.AddAdminNotification("4", "4");
                    if (!bSuppressEditsAndDeletes)
                    {
                        /* Now that the row is populated for display, put the data in a hash table
                         * to be used in populating the pop-up add, edit and delete dialogs. */
                        Hashtable hashData = new Hashtable();
                        hashData.Add("ixBugLineItem", ixBugLineItem);
                        hashData.Add("ixBug", ixBug);
                       
                        /* add the hash table as data to the edit template */
                        //dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                        /* add the data to the delete template as well */
                        dlgTemplateDelete.AddTemplateData(row.sRowId, hashData);
                    }

                }
            }
            else
            {
                /* If there are no items, just display a note in a full-width cell */
                CEditableTableRow row = new CEditableTableRow();
                row.sRowId = "none";
                row.AddCellWithColspan("No Items Yet...", nColCount);
                editableTableItems.Body.AddRow(row);
            }

            dsItems.Dispose();

            if (!bSuppressEditsAndDeletes)
            {
               // api.Notifications.AddAdminNotification("5", "5");
                /* Add a footer row with icon and text links to the add new item dialog */
                editableTableItems.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString())));
                editableTableItems.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString()),
                                                        "Add New Item"),
                                                        nColCount - 1);

                /* Associate the dialog templates with the table by name */
                editableTableItems.AddDialogTemplate("new", dlgTemplateNew);
                //Changed by Alok
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //End
                editableTableItems.AddDialogTemplate("delete", dlgTemplateDelete);
            }
            //api.Notifications.AddAdminNotification("6", "6");
            return editableTableItems;

        }

        protected CEditableTable ItemTable_synergs_Artium(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            if (ixProject != 26)
            {
                return null;
            }
            //  api.Notifications.AddAdminNotification("1", "1");

            CEditableTable editableTableItems = new CEditableTable("itemtable_4");
            //string sTableId = editableTableItems.sId;
            sTableId = editableTableItems.sId;

            string str5 = "Description", str1 = "Account Label", str6 = "Project Code", str3 = "Quantity", str2 = "Unit Price",
              str4 = "Total Amount"
               , str7 = "LineItemId", str8 = "Invoice Amount", str9 = "LineItem Balance amount";  //;  //now entered;  //now entered

            //string str1 = "Account Label", str2 = "Unit Price", str3 = "Quantity",
            //  str4 = "Total Amount",  //now entered
            //str5 = "Description", str6 = "Project Code";
            //str7 = "Billable", //changed by poornima 
            //str7 = "Prepaid",
            //str8 = "Addninfo";
            /* Define the header row of the table */
            if (!bSuppressEditsAndDeletes)
            {
                // api.Notifications.AddAdminNotification("2", "2");
                editableTableItems.Header.AddCell("Edit");
                editableTableItems.Header.AddCell("Delete");
            }
            editableTableItems.Header.AddCell(str7);
            editableTableItems.Header.AddCell(str5);
            editableTableItems.Header.AddCell(str1);
            editableTableItems.Header.AddCell(str6);
            editableTableItems.Header.AddCell(str3);
            editableTableItems.Header.AddCell(str2);
            editableTableItems.Header.AddCell(str4);
            editableTableItems.Header.AddCell(str8);
            editableTableItems.Header.AddCell(str9);

            //now entered


            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTableItems.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sTableId, ixProject);

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEditLineItem(sTableId, ixProject,ixBug,ixLineItem);

            /* Create the new item dialog template object used when the user clicks Add
             * New Item or the add icon in the footer row */

            CDialogTemplate dlgTemplateNew = DialogTemplateNew_synergs_Artium(ixBug, ixProject, sTableId);

            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            CDialogTemplate dlgTemplateDelete = DialogTemplateDelete_Synergis(sTableId, ixProject);

            //Created by Alok for new type of Edit dialog box

            //-----------------

            /* setup a DataSet and fetch the items from the database */
            DataSet dsItems = FetchItems(ixBug, true);
            int ixBugLineItem = -1;
            /* int ixGlAccount = -1;
             int ixGlDepartment = -1;
             int ixGlLocation = -1;
             int ixGlProject = -1;
             int ixGlClass = -1;
             int ixGlItem = -1;
             //int iForm99 = -1;
             * */
            double UnitPrice = -1D;
            int quantity = 0;
            string Description = "";
            string sAccount = "";
            string sprojectcode = "";
            double TotalAmount = -1D;
            int LineItemId = 0;

            double InvoiceTotalAmount = -1D;
            double BalanceAmount = -1D;
            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */

            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                // api.Notifications.AddAdminNotification("3", "3");
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {

                    //api.Notifications.AddAdminNotification("Trying to load the items", "This is not loading whu?");
                    ixBugLineItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugLineItem"]);
                    sAccount = Convert.ToString(dsItems.Tables[0].Rows[i]["sAccount"]).Trim();
                    ixBug = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBug"]);
                    sprojectcode = Convert.ToString(dsItems.Tables[0].Rows[i]["sDepartment"]);
                    Description = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                    quantity = Convert.ToInt32(dsItems.Tables[0].Rows[i]["fTax"]);
                    TotalAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["sExtra3"]);
                    LineItemId = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixLineItemId"]);
                                       
                    //for the editable table drop downs 
                    sAccount_P = sAccount.Replace("&", "/&");
                    sDepartment_P = sprojectcode;

                    try
                    {
                        UnitPrice = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                    }
                    catch
                    {
                        UnitPrice = 0d;
                    }

                    try
                    {
                        InvoiceTotalAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["IvnBalanceAmt"]);
                    }
                    catch
                    {
                        InvoiceTotalAmount = 0d;
                    }

                    try
                    {
                        BalanceAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fLineBalanceAmt"]);
                    }
                    catch
                    {
                        BalanceAmount = 0d;
                    }

                    //sGlAccountName = dsItems.Tables[0].Rows[i]["sGlAccountName"].ToString();

                    /* create a new table row and set the row id to the unique ixBugInvoiceItem */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = ixBugLineItem.ToString();
                    int ixLineItem = ixBugLineItem;
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    if (!bSuppressEditsAndDeletes)
                    {
                        row.AddCell(string.Format("<a href=\"#\" ixLineItem=\"{0}\" sTableId=\"{2}\" ixProject=\"{3}\"  onclick=\"ExamplePlugin.doPopup(this); return false;\">{1}</a>",
                                            ixLineItem.ToString(),
                                           FogCreek.FogBugz.UI.Icons.EditIcon(), sTableId.ToString(), ixProject.ToString()));
                        row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                                        sTableId,
                                        "delete",
                                        row.sRowId,
                                        CommandUrl("delete", ixBugLineItem, ixBug.ToString())));
                    }
                    /* make sure to run HtmlEncode on any user data before displaying it! */

                    row.AddCell(HttpUtility.HtmlEncode(LineItemId.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(Description.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sAccount));
                    row.AddCell(HttpUtility.HtmlEncode(sprojectcode));
                    row.AddCell(HttpUtility.HtmlEncode(quantity.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(UnitPrice.ToString("C")));
                    row.AddCell(HttpUtility.HtmlEncode(TotalAmount.ToString("C")));

                    row.AddCell(HttpUtility.HtmlEncode(InvoiceTotalAmount.ToString("C")));
                    row.AddCell(HttpUtility.HtmlEncode(BalanceAmount.ToString("C")));

                    editableTableItems.Body.AddRow(row);
                    //api.Notifications.AddAdminNotification("4", "4");
                    if (!bSuppressEditsAndDeletes)
                    {
                        /* Now that the row is populated for display, put the data in a hash table
                         * to be used in populating the pop-up add, edit and delete dialogs. */
                        Hashtable hashData = new Hashtable();
                        hashData.Add("ixBugLineItem", ixBugLineItem);
                        hashData.Add("ixBug", ixBug);

                        /* add the hash table as data to the edit template */
                        //dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                        /* add the data to the delete template as well */
                        dlgTemplateDelete.AddTemplateData(row.sRowId, hashData);
                    }

                }
            }
            else
            {
                /* If there are no items, just display a note in a full-width cell */
                CEditableTableRow row = new CEditableTableRow();
                row.sRowId = "none";
                row.AddCellWithColspan("No Items Yet...", nColCount);
                editableTableItems.Body.AddRow(row);
            }

            dsItems.Dispose();

            if (!bSuppressEditsAndDeletes)
            {
                // api.Notifications.AddAdminNotification("5", "5");
                /* Add a footer row with icon and text links to the add new item dialog */
                editableTableItems.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString())));
                editableTableItems.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString()),
                                                        "Add New Item"),
                                                        nColCount - 1);

                /* Associate the dialog templates with the table by name */
                editableTableItems.AddDialogTemplate("new", dlgTemplateNew);
                //Changed by Alok
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //End
                editableTableItems.AddDialogTemplate("delete", dlgTemplateDelete);
            }
            //api.Notifications.AddAdminNotification("6", "6");
            return editableTableItems;

        }

        protected CEditableTable ItemTable_Spreadfast(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            if (ixProject != 27)
            {
                return null;
            }
            //  api.Notifications.AddAdminNotification("1", "1");

            CEditableTable editableTableItems = new CEditableTable("itemtable_5");
            //string sTableId = editableTableItems.sId;
            sTableId = editableTableItems.sId;

            string str5 = "Description", str1 = "Account Label", str6 = "Item", str3 = "Quantity", str2 = "Unit Price",
              str4 = "Total Amount" //;  //now entered
               , str7 = "LineItemId", str8 = "Invoice Amount", str9 = "LineItem Balance amount"; 

            //string str1 = "Account Label", str2 = "Unit Price", str3 = "Quantity",
            //  str4 = "Total Amount",  //now entered
            //str5 = "Description", str6 = "Project Code";
            //str7 = "Billable", //changed by poornima 
            //str7 = "Prepaid",
            //str8 = "Addninfo";
            /* Define the header row of the table */
            if (!bSuppressEditsAndDeletes)
            {
                // api.Notifications.AddAdminNotification("2", "2");
                editableTableItems.Header.AddCell("Edit");
                editableTableItems.Header.AddCell("Delete");
            }

            editableTableItems.Header.AddCell(str7);
            editableTableItems.Header.AddCell(str6);
            editableTableItems.Header.AddCell(str5);
            editableTableItems.Header.AddCell(str1);
            editableTableItems.Header.AddCell(str3);
            editableTableItems.Header.AddCell(str2);
            editableTableItems.Header.AddCell(str4);
            editableTableItems.Header.AddCell(str8);
            editableTableItems.Header.AddCell(str9);

            ////editableTableItems.Header.AddCell(str5);
            ////editableTableItems.Header.AddCell(str1);
            ////editableTableItems.Header.AddCell(str6);
            ////editableTableItems.Header.AddCell(str3);
            ////editableTableItems.Header.AddCell(str2);
            ////editableTableItems.Header.AddCell(str4);

            //now entered


            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTableItems.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sTableId, ixProject);

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEditLineItem(sTableId, ixProject,ixBug,ixLineItem);

            /* Create the new item dialog template object used when the user clicks Add
             * New Item or the add icon in the footer row */

            CDialogTemplate dlgTemplateNew = DialogTemplateNew_Spreadfast(ixBug, ixProject, sTableId);

            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            CDialogTemplate dlgTemplateDelete = DialogTemplateDelete_Synergis(sTableId, ixProject);

            //Created by Alok for new type of Edit dialog box

            //-----------------

            /* setup a DataSet and fetch the items from the database */
            DataSet dsItems = FetchItems(ixBug, true);
            int ixBugLineItem = -1;
            /* int ixGlAccount = -1;
             int ixGlDepartment = -1;
             int ixGlLocation = -1;
             int ixGlProject = -1;
             int ixGlClass = -1;
             int ixGlItem = -1;
             //int iForm99 = -1;
             * */
            double UnitPrice = -1D;
            int quantity = 0;
            string Description = "";
            string sAccount = "";
            string sprojectcode = "";
            double TotalAmount = -1D;

            int LineItemId = 0;

            double InvoiceTotalAmount = -1D;
            double BalanceAmount = -1D;

            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */

            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                // api.Notifications.AddAdminNotification("3", "3");
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {

                    //api.Notifications.AddAdminNotification("Trying to load the items", "This is not loading whu?");
                    ixBugLineItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugLineItem"]);
                    sAccount = Convert.ToString(dsItems.Tables[0].Rows[i]["sAccount"]).Trim();
                    ixBug = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBug"]);
                    sprojectcode = Convert.ToString(dsItems.Tables[0].Rows[i]["sDepartment"]);
                    Description = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                    quantity = Convert.ToInt32(dsItems.Tables[0].Rows[i]["fTax"]);
                    TotalAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["sExtra3"]);
                    LineItemId = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixLineItemId"]);
                    //for the editable table drop downs 
                    sAccount_P = sAccount.Replace("&", "/&");
                    sDepartment_P = sprojectcode;

                    try
                    {
                        UnitPrice = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                    }
                    catch
                    {
                        UnitPrice = 0d;
                    }
                    try
                    {
                        InvoiceTotalAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["IvnBalanceAmt"]);
                    }
                    catch
                    {
                        InvoiceTotalAmount = 0d;
                    }

                    try
                    {
                        BalanceAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fLineBalanceAmt"]);
                    }
                    catch
                    {
                        BalanceAmount = 0d;
                    }

                    //sGlAccountName = dsItems.Tables[0].Rows[i]["sGlAccountName"].ToString();

                    /* create a new table row and set the row id to the unique ixBugInvoiceItem */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = ixBugLineItem.ToString();
                    int ixLineItem = ixBugLineItem;
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    if (!bSuppressEditsAndDeletes)
                    {
                        row.AddCell(string.Format("<a href=\"#\" ixLineItem=\"{0}\" sTableId=\"{2}\" ixProject=\"{3}\"  onclick=\"ExamplePlugin.doPopup(this); return false;\">{1}</a>",
                                            ixLineItem.ToString(),
                                           FogCreek.FogBugz.UI.Icons.EditIcon(), sTableId.ToString(), ixProject.ToString()));
                        row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                                        sTableId,
                                        "delete",
                                        row.sRowId,
                                        CommandUrl("delete", ixBugLineItem, ixBug.ToString())));
                    }
                    /* make sure to run HtmlEncode on any user data before displaying it! */
                    row.AddCell(HttpUtility.HtmlEncode(LineItemId.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sprojectcode));
                    row.AddCell(HttpUtility.HtmlEncode(Description.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sAccount));
                    row.AddCell(HttpUtility.HtmlEncode(quantity.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(UnitPrice.ToString("C")));
                    row.AddCell(HttpUtility.HtmlEncode(TotalAmount.ToString("C")));

                    row.AddCell(HttpUtility.HtmlEncode(InvoiceTotalAmount.ToString("C")));
                    row.AddCell(HttpUtility.HtmlEncode(BalanceAmount.ToString("C")));

                    editableTableItems.Body.AddRow(row);
                    //api.Notifications.AddAdminNotification("4", "4");
                    if (!bSuppressEditsAndDeletes)
                    {
                        /* Now that the row is populated for display, put the data in a hash table
                         * to be used in populating the pop-up add, edit and delete dialogs. */
                        Hashtable hashData = new Hashtable();
                        hashData.Add("ixBugLineItem", ixBugLineItem);
                        hashData.Add("ixBug", ixBug);

                        /* add the hash table as data to the edit template */
                        //dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                        /* add the data to the delete template as well */
                        dlgTemplateDelete.AddTemplateData(row.sRowId, hashData);
                    }

                }
            }
            else
            {
                /* If there are no items, just display a note in a full-width cell */
                CEditableTableRow row = new CEditableTableRow();
                row.sRowId = "none";
                row.AddCellWithColspan("No Items Yet...", nColCount);
                editableTableItems.Body.AddRow(row);
            }

            dsItems.Dispose();

            if (!bSuppressEditsAndDeletes)
            {
                // api.Notifications.AddAdminNotification("5", "5");
                /* Add a footer row with icon and text links to the add new item dialog */
                editableTableItems.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString())));
                editableTableItems.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString()),
                                                        "Add New Item"),
                                                        nColCount - 1);

                /* Associate the dialog templates with the table by name */
                editableTableItems.AddDialogTemplate("new", dlgTemplateNew);
                //Changed by Alok
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //End
                editableTableItems.AddDialogTemplate("delete", dlgTemplateDelete);
            }
            //api.Notifications.AddAdminNotification("6", "6");
            return editableTableItems;

        }

        protected CEditableTable ItemTable_Spreadfast_Inv(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            if (ixProject != 27)
            {
                return null;
            }
              //api.Notifications.AddAdminNotification("1", "1");

            CEditableTable editableTableItems = new CEditableTable("itemtable_Inv");
            //string sTableId = editableTableItems.sId;
            sTableId = editableTableItems.sId;

            string str1 = "Invoice Date", str2 = "Invoice Number", str3 = "Invoice Amount";
                //str3 = "Quantity", str2 = "Unit Price",str4 = "Total Amount";  //now entered

            //string str1 = "Account Label", str2 = "Unit Price", str3 = "Quantity",
            //  str4 = "Total Amount",  //now entered
            //str5 = "Description", str6 = "Project Code";
            //str7 = "Billable", //changed by poornima 
            //str7 = "Prepaid",
            //str8 = "Addninfo";
            /* Define the header row of the table */
            if (!bSuppressEditsAndDeletes)
            {
                 //api.Notifications.AddAdminNotification("2", "2");
                //editableTableItems.Header.AddCell("Edit");
                editableTableItems.Header.AddCell("Delete");
            }

           // editableTableItems.Header.AddCell(str6);
           // editableTableItems.Header.AddCell(str5);
            editableTableItems.Header.AddCell(str1);
            editableTableItems.Header.AddCell(str2);
            editableTableItems.Header.AddCell(str3);
           // editableTableItems.Header.AddCell(str4);

            ////editableTableItems.Header.AddCell(str5);
            ////editableTableItems.Header.AddCell(str1);
            ////editableTableItems.Header.AddCell(str6);
            ////editableTableItems.Header.AddCell(str3);
            ////editableTableItems.Header.AddCell(str2);
            ////editableTableItems.Header.AddCell(str4);

            //now entered


            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTableItems.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sTableId, ixProject);

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEditLineItem(sTableId, ixProject,ixBug,ixLineItem);

            /* Create the new item dialog template object used when the user clicks Add
             * New Item or the add icon in the footer row */

            //CDialogTemplate dlgTemplateNew = DialogTemplateNew_Spreadfast(ixBug, ixProject, sTableId);

            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            CDialogTemplate dlgTemplateDelete = DialogTemplateDelete(sTableId, ixProject);

            //Created by Alok for new type of Edit dialog box

            //-----------------

            /* setup a DataSet and fetch the items from the database */
            DataSet dsItems = FetchItems(ixBug, true);
            int ixBugLineItem = -1;
            /* int ixGlAccount = -1;
             int ixGlDepartment = -1;
             int ixGlLocation = -1;
             int ixGlProject = -1;
             int ixGlClass = -1;
             int ixGlItem = -1;
             //int iForm99 = -1;
             * */
           // double UnitPrice = -1D;
            int quantity = 0;
            string Description = "";
            string sAccount = "";
            string sprojectcode = "";
           // double TotalAmount = -1D;

            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */

            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                 //api.Notifications.AddAdminNotification("3", "3");
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {


                   // api.Notifications.AddAdminNotification("Trying to load the items", "This is not loading whu?");

                    ixBugLineItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugLineItem"]);
                   // sAccount = Convert.ToString(dsItems.Tables[0].Rows[i]["sAccount"]).Trim();
                    ixBug = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBug"]);
                    sprojectcode = Convert.ToString(dsItems.Tables[0].Rows[i]["sDepartment"]);
                    Description = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                    quantity = Convert.ToInt32(dsItems.Tables[0].Rows[i]["fTax"]);
                   // TotalAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["sExtra3"]);
                    //for the editable table drop downs 
                    sAccount_P = sAccount.Replace("&", "/&");
                    sDepartment_P = sprojectcode;

                    //try
                    //{
                    //    UnitPrice = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                    //}
                    //catch
                    //{
                    //    UnitPrice = 0d;
                    //}

                    //sGlAccountName = dsItems.Tables[0].Rows[i]["sGlAccountName"].ToString();

                    /* create a new table row and set the row id to the unique ixBugInvoiceItem */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = ixBugLineItem.ToString();
                    int ixLineItem = ixBugLineItem;
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    if (!bSuppressEditsAndDeletes)
                    {
                        //row.AddCell(string.Format("<a href=\"#\" ixLineItem=\"{0}\" sTableId=\"{2}\" ixProject=\"{3}\"  onclick=\"ExamplePlugin.doPopup(this); return false;\">{1}</a>",
                        //                    ixLineItem.ToString(),
                        //                   FogCreek.FogBugz.UI.Icons.EditIcon(), sTableId.ToString(), ixProject.ToString()));
                        row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                                        sTableId,
                                        "delete",
                                        row.sRowId,
                                        CommandUrl("delete", ixBugLineItem, ixBug.ToString())));
                    }
                    ///* make sure to run HtmlEncode on any user data before displaying it! */
                    row.AddCell(HttpUtility.HtmlEncode(sprojectcode));
                    row.AddCell(HttpUtility.HtmlEncode(Description.ToString()));
                   // row.AddCell(HttpUtility.HtmlEncode(sAccount));
                    row.AddCell(HttpUtility.HtmlEncode(quantity.ToString()));
                   // row.AddCell(HttpUtility.HtmlEncode(UnitPrice.ToString("C")));
                   // row.AddCell(HttpUtility.HtmlEncode(TotalAmount.ToString("C")));



                    editableTableItems.Body.AddRow(row);
                    //api.Notifications.AddAdminNotification("4", "4");
                    if (!bSuppressEditsAndDeletes)
                    {
                        /* Now that the row is populated for display, put the data in a hash table
                         * to be used in populating the pop-up add, edit and delete dialogs. */
                        Hashtable hashData = new Hashtable();
                        hashData.Add("ixBugLineItem", ixBugLineItem);
                        hashData.Add("ixBug", ixBug);

                        /* add the hash table as data to the edit template */
                        //dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                        /* add the data to the delete template as well */
                        dlgTemplateDelete.AddTemplateData(row.sRowId, hashData);
                    }

                }
            }
            else
            {
                /* If there are no items, just display a note in a full-width cell */
                CEditableTableRow row = new CEditableTableRow();
                row.sRowId = "none";
                row.AddCellWithColspan("No Items Yet...", nColCount);
                editableTableItems.Body.AddRow(row);
            }

            dsItems.Dispose();

            if (!bSuppressEditsAndDeletes)
            {
                // api.Notifications.AddAdminNotification("5", "5");
                ///* Add a footer row with icon and text links to the add new item dialog */
                //editableTableItems.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                //                                        sTableId,
                //                                        "new",
                //                                        "sDataId",
                //                                        CommandUrl("new", ixBugLineItem, ixBug.ToString())));
                //editableTableItems.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                //                                        sTableId,
                //                                        "new",
                //                                        "sDataId",
                //                                        CommandUrl("new", ixBugLineItem, ixBug.ToString()),
                //                                        "Add New Item"),
                //                                        nColCount - 1);

                /* Associate the dialog templates with the table by name */
                //editableTableItems.AddDialogTemplate("new", dlgTemplateNew);
                //Changed by Alok
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //End
                editableTableItems.AddDialogTemplate("delete", dlgTemplateDelete);
            }
            //api.Notifications.AddAdminNotification("6", "6");
            return editableTableItems;

        }

        protected CEditableTable ItemTable_Cambridge(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {           
            if (ixProject != 19)
            {
                return null;
            }
                       
            CEditableTable editableTableItems_1 = new CEditableTable("itemtable_1");
            sTableId = editableTableItems_1.sId;

            string str1 = "Account", str2 = "DistType", str3 = "Amount",
            str4 = "Memo";

            /* Define the header row of the table */
            if (!bSuppressEditsAndDeletes)
            {
                editableTableItems_1.Header.AddCell("Edit");
                editableTableItems_1.Header.AddCell("Delete");
            }
            editableTableItems_1.Header.AddCell(str1);
            editableTableItems_1.Header.AddCell(str2);
            editableTableItems_1.Header.AddCell(str4);
            editableTableItems_1.Header.AddCell(str3);

            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTableItems_1.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */

            /* Create the new item dialog template object used when the user clicks Add
             * New Item or the add icon in the footer row */

            CDialogTemplate dlgTemplateNew_1 = DialogTemplateNew_cambridge(ixBug, ixProject, sTableId);
            //CDialogTemplate dlgTemplateCopy = DialogTemplateCopyItem(ixBug, ixProject, sTableId);
            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            CDialogTemplate dlgTemplateDelete_1 = DialogTemplateDelete(sTableId, ixProject);

            /* setup a DataSet and fetch the items from the database */
            DataSet dsItems = FetchItems(ixBug, true);
            int ixBugLineItem = -1;

            string sMemo = "";
            string sAccount = "";
            string DistType = "";
            string Amount = "";
           // string Credit = "";

            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */

            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {

                    ixBugLineItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugLineItem"]);
                    sAccount = Convert.ToString(dsItems.Tables[0].Rows[i]["sAccount"]).Trim();
                    ixBug = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBug"]);
                    DistType = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra2"]);
                    Amount = Convert.ToString(dsItems.Tables[0].Rows[i]["fAmount"]);
                    sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);

                    //for the editable table drop downs 
                    sAccount_P = sAccount.Replace("&", "/&");
                    /* create a new table row and set the row id to the unique ixBugInvoiceItem */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = ixBugLineItem.ToString();
                    int ixLineItem = ixBugLineItem;
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    if (!bSuppressEditsAndDeletes)
                    {
                        row.AddCell(string.Format("<a href=\"#\" ixLineItem=\"{0}\" sTableId=\"{2}\" ixProject=\"{3}\"  onclick=\"ExamplePlugin.doPopup(this); return false;\">{1}</a>",
                                            ixLineItem.ToString(),
                                           FogCreek.FogBugz.UI.Icons.EditIcon(), sTableId.ToString(), ixProject.ToString()));
                        row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                                        sTableId,
                                        "delete",
                                        row.sRowId,
                                        CommandUrl("delete", ixBugLineItem, ixBug.ToString())));
                    }
                    /* make sure to run HtmlEncode on any user data before displaying it! */
                    row.AddCell(HttpUtility.HtmlEncode(sAccount));
                    // row.AddCell(HttpUtility.HtmlEncode(sAccount.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(DistType));
                    row.AddCell(HttpUtility.HtmlEncode(sMemo.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(Amount));
                    // row.AddCell(HttpUtility.HtmlEncode(Credit));
                    editableTableItems_1.Body.AddRow(row);

                    if (!bSuppressEditsAndDeletes)
                    {
                        /* Now that the row is populated for display, put the data in a hash table
                         * to be used in populating the pop-up add, edit and delete dialogs. */
                        Hashtable hashData = new Hashtable();
                        hashData.Add("ixBugLineItem", ixBugLineItem);
                        hashData.Add("ixBug", ixBug);

                        /* add the hash table as data to the edit template */
                        //dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                        /* add the data to the delete template as well */
                        dlgTemplateDelete_1.AddTemplateData(row.sRowId, hashData);
                    }

                }
            }
            else
            {
                /* If there are no items, just display a note in a full-width cell */
                CEditableTableRow row = new CEditableTableRow();
                row.sRowId = "none";
                row.AddCellWithColspan("No Items Yet...", nColCount);
                editableTableItems_1.Body.AddRow(row);
            }

            dsItems.Dispose();

            if (!bSuppressEditsAndDeletes)
            {
                /* Add a footer row with icon and text links to the add new item dialog */
                editableTableItems_1.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString())));
                editableTableItems_1.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString()),
                                                        "Add New Item"),
                                                        nColCount - 1);

                /* Associate the dialog templates with the table by name */
                editableTableItems_1.AddDialogTemplate("new", dlgTemplateNew_1);
                editableTableItems_1.AddDialogTemplate("delete", dlgTemplateDelete_1);
            }

            return editableTableItems_1;

        }

        protected CEditableTable ItemTable_TE(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            //api.Notifications.AddAdminNotification("prjID", ixProject.ToString());


            if (ixProject != 23)
            {
                return null;
            }

            // api.Notifications.AddAdminNotification("1", "1");

            CEditableTable editableTableItems_2 = new CEditableTable("itemtable_2");
            //string sTableId = editableTableItems.sId;
            sTableId = editableTableItems_2.sId;

            string str1 = "Vendor", str3 = "Amount",
            str4 = "Memo";

            /* Define the header row of the table */
            if (!bSuppressEditsAndDeletes)
            {
                editableTableItems_2.Header.AddCell("Edit");
                editableTableItems_2.Header.AddCell("Delete");
            }
            editableTableItems_2.Header.AddCell(str1);
            editableTableItems_2.Header.AddCell(str4);
            editableTableItems_2.Header.AddCell(str3);
            // editableTableItems.Header.AddCell(str5);



            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTableItems_2.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sTableId, ixProject);

            //CDialogTemplate dlgTemplateEdit = DialogTemplateEditLineItem(sTableId, ixProject,ixBug,ixLineItem);

            /* Create the new item dialog template object used when the user clicks Add
             * New Item or the add icon in the footer row */

            CDialogTemplate dlgTemplateNew_2 = DialogTemplate_TE(ixBug, ixProject, sTableId);
            //CDialogTemplate dlgTemplateCopy = DialogTemplateCopyItem(ixBug, ixProject, sTableId);
            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            CDialogTemplate dlgTemplateDelete_2 = DialogTemplateDelete(sTableId, ixProject);

            //Created by Alok for new type of Edit dialog box

            //-----------------

            /* setup a DataSet and fetch the items from the database */
            DataSet dsItems = FetchItems(ixBug, true);
            int ixBugLineItem = -1;

            string sMemo = "";
            string Vendor = "";
            //   string DistType = "";
            string Amount = "";
            //  string Credit = "";

            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */

            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {

                    ixBugLineItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugLineItem"]);
                    // sAccount = Convert.ToString(dsItems.Tables[0].Rows[i]["sAccount"]).Trim();
                    ixBug = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBug"]);
                    Vendor = Convert.ToString(dsItems.Tables[0].Rows[i]["sAddninfo"]);
                    Amount = Convert.ToString(dsItems.Tables[0].Rows[i]["fAmount"]);
                    sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);

                    //for the editable table drop downs 
                    //    sAccount_P = sAccount.Replace("&", "/&");
                    /* create a new table row and set the row id to the unique ixBugInvoiceItem */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = ixBugLineItem.ToString();
                    int ixLineItem = ixBugLineItem;
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    if (!bSuppressEditsAndDeletes)
                    {
                        row.AddCell(string.Format("<a href=\"#\" ixLineItem=\"{0}\" sTableId=\"{2}\" ixProject=\"{3}\"  onclick=\"ExamplePlugin.doPopup(this); return false;\">{1}</a>",
                                            ixLineItem.ToString(),
                                           FogCreek.FogBugz.UI.Icons.EditIcon(), sTableId.ToString(), ixProject.ToString()));
                        row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                                        sTableId,
                                        "delete",
                                        row.sRowId,
                                        CommandUrl("delete", ixBugLineItem, ixBug.ToString())));
                    }
                    /* make sure to run HtmlEncode on any user data before displaying it! */
                    //  row.AddCell(HttpUtility.HtmlEncode(sAccount));
                    // row.AddCell(HttpUtility.HtmlEncode(sAccount.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(Vendor));
                    row.AddCell(HttpUtility.HtmlEncode(sMemo.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(Amount));
                    // row.AddCell(HttpUtility.HtmlEncode(Credit));
                    editableTableItems_2.Body.AddRow(row);

                    if (!bSuppressEditsAndDeletes)
                    {
                        /* Now that the row is populated for display, put the data in a hash table
                         * to be used in populating the pop-up add, edit and delete dialogs. */
                        Hashtable hashData = new Hashtable();
                        hashData.Add("ixBugLineItem", ixBugLineItem);
                        hashData.Add("ixBug", ixBug);

                        /* add the hash table as data to the edit template */
                        //dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                        /* add the data to the delete template as well */
                        dlgTemplateDelete_2.AddTemplateData(row.sRowId, hashData);
                    }

                }
            }
            else
            {
                /* If there are no items, just display a note in a full-width cell */
                CEditableTableRow row = new CEditableTableRow();
                row.sRowId = "none";
                row.AddCellWithColspan("No Items Yet...", nColCount);
                editableTableItems_2.Body.AddRow(row);
            }

            dsItems.Dispose();

            if (!bSuppressEditsAndDeletes)
            {
                /* Add a footer row with icon and text links to the add new item dialog */
                editableTableItems_2.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString())));
                editableTableItems_2.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugLineItem, ixBug.ToString()),
                                                        "Add New Vendor"),
                                                        nColCount - 1);

                /* Associate the dialog templates with the table by name */
                editableTableItems_2.AddDialogTemplate("new", dlgTemplateNew_2);
                // editableTableItems.AddDialogTemplate("Copy", dlgTemplateCopy);
                //Changed by Alok
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //End
                editableTableItems_2.AddDialogTemplate("delete", dlgTemplateDelete_2);
            }

            return editableTableItems_2;

        }
     
        /* This method builds the template for the add new item dialog */
        protected CDialogTemplate DialogTemplateNew(int ixBug, int ixProject, string sTableId)
        {
            if (ixProject != 14)
            {
                return null;
            }
            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();
            dlgTemplateNew.Template.sTitle = "Add New Item";

            string str1 = "GL Account", str2 = "Amount", str3 = "Tax type", str4 = "Tax", str5 = "Memo",
                str6 = "Department", 
                str7 = "Prepaid",
                str8 = "Addninfo";
           
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

            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"),ixBug.ToString()));
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"),ixProject.ToString()));
            CDialogItem itemAccount = new CDialogItem(GetSelects1("CWFAccount", ixProject),str1,"this account item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemAccount);
            
            CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""),str2);
            dlgTemplateNew.Template.Items.Add(itemAmount);

            CDialogItem itemTaxtype = new CDialogItem(GetSelects1("CWFVat", ixProject),str3,"This Tax item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemTaxtype);

            CDialogItem itemTax =new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), ""),str4);
            dlgTemplateNew.Template.Items.Add(itemTax);

            CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""),str5);
            dlgTemplateNew.Template.Items.Add(itemMemo);

            CDialogItem itemDepartment = new CDialogItem(GetSelects1("CWFDepartment", ixProject),str6,
                "Choose the department this item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemDepartment);

            CDialogItem itemBillable = new CDialogItem(GetSelects("CWFBillable", ixProject),str7,
                "Choose the Billable type this item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemBillable);

            CDialogItem itemAddnInfo =new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sAddninfo"), ""),str8);
            dlgTemplateNew.Template.Items.Add(itemAddnInfo);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateNew;
        }

        protected CDialogTemplate DialogTemplateNew_synergs(int ixBug, int ixProject, string sTableId)
        {
            if (ixProject != 25)
            {
                return null;
            }
            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();
            dlgTemplateNew.Template.sTitle = "Add New Item";

           // string str1 = "Account Label", str2 = "Unit Price", str3 = "Quantity", str6 = "Total Amount",
           //str4 = "Description", str5 = "Project Code";
            string str4 = "Description", str1 = "Account Label", str5 = "Project Code", str3 = "Quantity", str2 = "Unit Price";
          // str4 = "Total Amount";  //now entered

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */

            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new_3");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */

            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);

            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));

            CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""), str4);
            dlgTemplateNew.Template.Items.Add(itemMemo);

            CDialogItem itemAccount = new CDialogItem(GetSelects1_Synergis(null,"CWFAccount", ixProject,false), str1, "this account item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemAccount);

            CDialogItem itemDepartment = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("CWFDepartment"), ""), str5);
            dlgTemplateNew.Template.Items.Add(itemDepartment);

            CDialogItem itemTax = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), ""), str3);
            dlgTemplateNew.Template.Items.Add(itemTax);

            CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""), str2);
            dlgTemplateNew.Template.Items.Add(itemAmount);

            //CDialogItem itemTotalAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra3"), ""), str6);
            //dlgTemplateNew.Template.Items.Add(itemTotalAmount);
         

            //CDialogItem itemDepartment = new CDialogItem(GetSelects1("CWFDepartment", ixProject), str5,
            //    "Choose the department this item has to be coded from the drop-down");
            //dlgTemplateNew.Template.Items.Add(itemDepartment);
                        
            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateNew;
        }

        protected CDialogTemplate DialogTemplateNew_synergs_Artium(int ixBug, int ixProject, string sTableId)
        {
            if (ixProject != 26)
            {
                return null;
            }
            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();
            dlgTemplateNew.Template.sTitle = "Add New Item";

            string str1 = "Account Label", str2 = "Unit Price", str3 = "Quantity", str6 = "Total Amount",
           str4 = "Description", str5 = "Project Code";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */

            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new_4");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */

            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);

            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));
            CDialogItem itemAccount = new CDialogItem(GetSelects1_Synergis(null,"CWFAccount", ixProject,false), str1, "this account item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemAccount);

            CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""), str2);
            dlgTemplateNew.Template.Items.Add(itemAmount);

            CDialogItem itemTax = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), ""), str3);
            dlgTemplateNew.Template.Items.Add(itemTax);

            //CDialogItem itemTotalAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra3"), ""), str6);
            //dlgTemplateNew.Template.Items.Add(itemTotalAmount);

            CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""), str4);
            dlgTemplateNew.Template.Items.Add(itemMemo);

            CDialogItem itemDepartment = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("CWFDepartment"), ""), str5);
            dlgTemplateNew.Template.Items.Add(itemDepartment);

            //CDialogItem itemDepartment = new CDialogItem(GetSelects1("CWFDepartment", ixProject), str5,
            //    "Choose the department this item has to be coded from the drop-down");
            //dlgTemplateNew.Template.Items.Add(itemDepartment);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateNew;
        }

        protected CDialogTemplate DialogTemplateNew_cambridge(int ixBug, int ixProject, string sTableId)
        {
            if (ixProject != 19)
            {
                return null;
            }
            CDialogTemplate dlgTemplateNew_Cambridge = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew_Cambridge.Template = new CDoubleColumnDialog();
            dlgTemplateNew_Cambridge.Template.sTitle = "Add New Item";

            string str1 = "Account", str2 = "DistType", str3 = "Amount",
         str5 = "Memo";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew_Cambridge.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new_1");
            dlgTemplateNew_Cambridge.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */

            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew_Cambridge.Template.Items.Add(itemActionToken);

            dlgTemplateNew_Cambridge.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
            dlgTemplateNew_Cambridge.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));
            CDialogItem itemAccount = new CDialogItem(GetSelects1("CWFAccount", ixProject), str1, "this account item has to be coded from the drop-down");
            dlgTemplateNew_Cambridge.Template.Items.Add(itemAccount);

            //CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""), str2);
            //dlgTemplateNew_Cambridge.Template.Items.Add(itemAmount);

            CDialogItem itemTaxtype = new CDialogItem(GetSelects1("CWFVat", ixProject), str2, "This Tax item has to be coded from the drop-down");
            dlgTemplateNew_Cambridge.Template.Items.Add(itemTaxtype);

            CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""), str3);
            dlgTemplateNew_Cambridge.Template.Items.Add(itemAmount);

            CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""), str5);
            dlgTemplateNew_Cambridge.Template.Items.Add(itemMemo);

            /* Standard ok and cancel buttons */
            dlgTemplateNew_Cambridge.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateNew_Cambridge;
        }

        protected CDialogTemplate DialogTemplate_TE(int ixBug, int ixProject, string sTableId)
        {
            if (ixProject != 23)
            {
                return null;
            }
            CDialogTemplate dlgTemplateNew_TE = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew_TE.Template = new CDoubleColumnDialog();
            dlgTemplateNew_TE.Template.sTitle = "Add New Item";

            string str1 = "vendor", str3 = "Amount", str5 = "Memo";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew_TE.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new_2");
            dlgTemplateNew_TE.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */

            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew_TE.Template.Items.Add(itemActionToken);

            dlgTemplateNew_TE.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
            dlgTemplateNew_TE.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));

            CDialogItem itemTax = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sAddninfo"), ""), str1);
            dlgTemplateNew_TE.Template.Items.Add(itemTax);

            CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""), str3);
            dlgTemplateNew_TE.Template.Items.Add(itemAmount);

            CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""), str5);
            dlgTemplateNew_TE.Template.Items.Add(itemMemo);


            /* Standard ok and cancel buttons */
            dlgTemplateNew_TE.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateNew_TE;
        }

        protected CDialogTemplate DialogTemplateNew_Spreadfast(int ixBug, int ixProject, string sTableId)
        {
            if (ixProject != 27)
            {
                return null;
            }
            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();
            dlgTemplateNew.Template.sTitle = "Add New Item";

            string str5 = "Item",  str4 = "Description", str1 = "Account Label", str3 = "Quantity", str2 = "Unit Price";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */

            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new_5");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */

            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);

            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));

            CDialogItem itemDepartment = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("CWFDepartment"), ""), str5);
            dlgTemplateNew.Template.Items.Add(itemDepartment);

            CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""), str4);
            dlgTemplateNew.Template.Items.Add(itemMemo);

            CDialogItem itemAccount = new CDialogItem(GetSelects1_Synergis(null, "CWFAccount", ixProject, false), str1, "this account item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemAccount);

            

            CDialogItem itemTax = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), ""), str3);
            dlgTemplateNew.Template.Items.Add(itemTax);

            CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""), str2);
            dlgTemplateNew.Template.Items.Add(itemAmount);

            //CDialogItem itemTotalAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra3"), ""), str6);
            //dlgTemplateNew.Template.Items.Add(itemTotalAmount);


            //CDialogItem itemDepartment = new CDialogItem(GetSelects1("CWFDepartment", ixProject), str5,
            //    "Choose the department this item has to be coded from the drop-down");
            //dlgTemplateNew.Template.Items.Add(itemDepartment);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateNew;
        }

        //Added by Alok

        private string sTableId;

        protected CEditableTable EditableTable(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Copycase_1");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Copy Case_1"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId,"CopyCase_1","sDataId",
                                                    CommandUrl1("CopyCase_1", ixBug),"Copy Case_1"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("CopyCase_1", dlgTemplateNew);
            return editableTable;
        }

        protected CEditableTable EditableTable_1(CBug[] rgbug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Copycase_1");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            // row.sRowId = ixBug.ToString();
            row.sRowId = rgbug[0].ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Copy Case_1"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew_1(rgbug[0].ixBug, rgbug[0].ixProject);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId, "CopyCase_1", "sDataId",
                                                    CommandUrl1("CopyCase_1", rgbug[0].ixBug), "Copy Case_1"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("CopyCase_1", dlgTemplateNew);
            return editableTable;
        }

        protected CEditableTable EditableTable_Spreadfast(CBug[] rgbug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Copy_PO");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            // row.sRowId = ixBug.ToString();
            row.sRowId = rgbug[0].ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Copy_PO"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew_Spreadfast(rgbug[0].ixBug, rgbug[0].ixProject);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId, "Copy_PO", "sDataId",
                                                    CommandUrl1("Copy_PO", rgbug[0].ixBug), "Copy_PO"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("Copy_PO", dlgTemplateNew);
            return editableTable;
        }

        protected CEditableTable EditableTable_Synergis_addendum(CBug[] rgbug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Addendum_PO");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            // row.sRowId = ixBug.ToString();
            row.sRowId = rgbug[0].ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Addendum_PO"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew_Synergis_addendum(rgbug[0].ixBug, rgbug[0].ixProject);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId, "Addendum_PO", "sDataId",
                                                    CommandUrl1("Addendum_PO", rgbug[0].ixBug), "Addendum_PO"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("Addendum_PO", dlgTemplateNew);
            return editableTable;
        }

        protected CEditableTable EditableTable_Spreadfast_addendum(CBug[] rgbug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Addendum_PO");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            // row.sRowId = ixBug.ToString();
            row.sRowId = rgbug[0].ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Addendum_PO"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew_Spreadfast_addendum(rgbug[0].ixBug, rgbug[0].ixProject);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId, "Addendum_PO", "sDataId",
                                                    CommandUrl1("Addendum_PO", rgbug[0].ixBug), "Addendum_PO"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("Addendum_PO", dlgTemplateNew);
            return editableTable;
        }

        protected CEditableTable EditableTable_Synergis(CBug[] rgbug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Copycase_PO");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            // row.sRowId = ixBug.ToString();
            row.sRowId = rgbug[0].ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Copy Case_PO"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew_Synergis(rgbug[0].ixBug, rgbug[0].ixProject);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId, "CopyCase_PO", "sDataId",
                                                    CommandUrl1("CopyCase_PO", rgbug[0].ixBug), "Copy Case_PO"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("CopyCase_PO", dlgTemplateNew);
            return editableTable;
        }

        protected CEditableTable EditableTable_Blanket_PO(CBug[] rgbug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("blanketPO");
            sTableId = editableTable.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            // row.sRowId = ixBug.ToString();
            row.sRowId = rgbug[0].ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Blanket_PO"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew_Blanket_PO(rgbug[0].ixBug, rgbug[0].ixProject);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId, "BlanketPO", "sDataId",
                                                    CommandUrl1("BlanketPO", rgbug[0].ixBug), "Blanket_PO"));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("BlanketPO", dlgTemplateNew);
            return editableTable;
        }

        protected CDialogTemplate DialogTemplateNew_Blanket_PO(int ixBug, int ixproj)
        {

            try
            {
                CDialogTemplate dlgTemplateNew = new CDialogTemplate();
                /* There are several dialog formats to choose from */
                dlgTemplateNew.Template = new CDoubleColumnDialog();

                dlgTemplateNew.Template.sTitle = "Select a Blanket PO Number to copy from";
                dlgTemplateNew.Template.sWidth = "300px";

                /* FogBugz dialogs post to default.asp via AJAX. To have this form post
                 * to the plugin raw page, we need to add the pg and ixPlugin values.
                 * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
                 * So we can just use api.Url.PluginRawPageUrl */
                CDialogItem itemNewHiddenUrl =
                    CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
                CDialogItem itemNewHiddenAction =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "SF_Blanket_PO");
                dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
                /* include a security action token */
                CDialogItem itemActionToken =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateNew.Template.Items.Add(itemActionToken);
                dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
              //  CDialogItem itemEditId = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("PONumber"), ""), "PONumber ");
                CDialogItem itemEditBlanketPO = new CDialogItem(GetSelectsBlanketPO(null, "CGSInvoice_MLA", ixproj, true), "B_PO_Number");

                dlgTemplateNew.Template.Items.Add(itemEditBlanketPO);
                //CDialogItem itemEditId2 =
                //    new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute", "Header"));
                //dlgTemplateNew.Template.Items.Add(itemEditId2);

                //CDialogItem itemEditId3 =
                //     new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("LineItems"), "true", "CheckedAttribute", "Line Items"));
                //dlgTemplateNew.Template.Items.Add(itemEditId3);

                /* Standard ok and cancel buttons */
                dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

                // api.Notifications.AddAdminNotification("dlgTemplateNew returned", "");

                return dlgTemplateNew;
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error at dialog creation");
                return null;
            }

        }

        protected CDialogTemplate DialogTemplateNew_Synergis(int ixBug, int ixproj)
        {

            try
            {
                CDialogTemplate dlgTemplateNew = new CDialogTemplate();
                /* There are several dialog formats to choose from */
                dlgTemplateNew.Template = new CDoubleColumnDialog();

                dlgTemplateNew.Template.sTitle = "Enter the PO Number to copy from";
                dlgTemplateNew.Template.sWidth = "300px";

                /* FogBugz dialogs post to default.asp via AJAX. To have this form post
                 * to the plugin raw page, we need to add the pg and ixPlugin values.
                 * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
                 * So we can just use api.Url.PluginRawPageUrl */
                CDialogItem itemNewHiddenUrl =
                    CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
                CDialogItem itemNewHiddenAction =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "Syn_copycase");
                dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
                /* include a security action token */
                CDialogItem itemActionToken =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateNew.Template.Items.Add(itemActionToken);
                dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
                CDialogItem itemEditId = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("PONumber"), ""), "PONumber ");
                // CDialogItem itemEditVenId = new CDialogItem(GetSelects1(null, "CWFVendor", 9, true), "VendorName");
              //  CDialogItem itemEditVenId = new CDialogItem(GetSelectsVName(null, "CWFVendor", ixproj, true), "VendorName");
              //  CDialogItem itemEditPONum = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("PONumber"), ""), "PO Number ");
                //CDialogItem itemEditVenId1 = new CDialogItem(GetSelects1(null, "CWFVendor", 9, true), "VendorName");
                // itemEditVenId.sContent = GetSelects("CWFVendor", "CWFVendor", 14, true);
                dlgTemplateNew.Template.Items.Add(itemEditId);
             //   dlgTemplateNew.Template.Items.Add(itemEditVenId);
               // dlgTemplateNew.Template.Items.Add(itemEditPONum);
                //  itemEditVenId.sContent = GetSelects1(null,"CWFVendor", 14, true);
                //CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemEditVenId);
                //DialogItem.sLabel = fielddisplay;
                //DialogItem.sContent = GetSelects1(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
                //return DialogItem;
                //CDialogItem itemEditId1 =
                //    new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Dummy ID"), ""),
                //                    "Case ID dummy ");
                //dlgTemplateNew.Template.Items.Add(itemEditId1);

                CDialogItem itemEditId2 =
                    new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute", "Header"));
                dlgTemplateNew.Template.Items.Add(itemEditId2);

                CDialogItem itemEditId3 =
                     new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("LineItems"), "true", "CheckedAttribute", "Line Items"));
                dlgTemplateNew.Template.Items.Add(itemEditId3);

                /* Standard ok and cancel buttons */
                dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

                // api.Notifications.AddAdminNotification("dlgTemplateNew returned", "");

                return dlgTemplateNew;
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error at dialog creation");
                return null;
            }

        }

        protected CDialogTemplate DialogTemplateNew_Synergis_addendum(int ixBug, int ixproj)
        {

            try
            {
                CDialogTemplate dlgTemplateNew = new CDialogTemplate();
                /* There are several dialog formats to choose from */
                dlgTemplateNew.Template = new CDoubleColumnDialog();

                dlgTemplateNew.Template.sTitle = "Enter the PO Number to copy from";
                dlgTemplateNew.Template.sWidth = "300px";

                /* FogBugz dialogs post to default.asp via AJAX. To have this form post
                 * to the plugin raw page, we need to add the pg and ixPlugin values.
                 * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
                 * So we can just use api.Url.PluginRawPageUrl */
                CDialogItem itemNewHiddenUrl =
                    CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
                CDialogItem itemNewHiddenAction =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "Syn_Addendum");
                dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
                /* include a security action token */
                CDialogItem itemActionToken =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateNew.Template.Items.Add(itemActionToken);
                dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
                     
                CDialogItem itemEditPONum = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("PONumber"), ""), "PO Number ");
                dlgTemplateNew.Template.Items.Add(itemEditPONum);
              
                //CDialogItem itemEditId2 =
                //    new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute", "Header"));
                //dlgTemplateNew.Template.Items.Add(itemEditId2);

                //CDialogItem itemEditId3 =
                //     new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("LineItems"), "true", "CheckedAttribute", "Line Items"));
                //dlgTemplateNew.Template.Items.Add(itemEditId3);

                /* Standard ok and cancel buttons */
                dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

                // api.Notifications.AddAdminNotification("dlgTemplateNew returned", "");

                return dlgTemplateNew;
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error at dialog creation");
                return null;
            }

        }

        protected CDialogTemplate DialogTemplateNew_Spreadfast_addendum(int ixBug, int ixproj)
        {

            try
            {
                CDialogTemplate dlgTemplateNew = new CDialogTemplate();
                /* There are several dialog formats to choose from */
                dlgTemplateNew.Template = new CDoubleColumnDialog();

                dlgTemplateNew.Template.sTitle = "Enter the PO Number to copy from";
                dlgTemplateNew.Template.sWidth = "300px";

                /* FogBugz dialogs post to default.asp via AJAX. To have this form post
                 * to the plugin raw page, we need to add the pg and ixPlugin values.
                 * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
                 * So we can just use api.Url.PluginRawPageUrl */
                CDialogItem itemNewHiddenUrl =
                    CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
                CDialogItem itemNewHiddenAction =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "Spreadfast_Addendum");
                dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
                /* include a security action token */
                CDialogItem itemActionToken =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateNew.Template.Items.Add(itemActionToken);
                dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));

                CDialogItem itemEditPONum = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("PONumber"), ""), "PO Number ");
                dlgTemplateNew.Template.Items.Add(itemEditPONum);

                //CDialogItem itemEditId2 =
                //    new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute", "Header"));
                //dlgTemplateNew.Template.Items.Add(itemEditId2);

                //CDialogItem itemEditId3 =
                //     new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("LineItems"), "true", "CheckedAttribute", "Line Items"));
                //dlgTemplateNew.Template.Items.Add(itemEditId3);

                /* Standard ok and cancel buttons */
                dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

                // api.Notifications.AddAdminNotification("dlgTemplateNew returned", "");

                return dlgTemplateNew;
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error at dialog creation");
                return null;
            }

        }

        protected CDialogTemplate DialogTemplateNew_Spreadfast(int ixBug, int ixproj)
        {

            try
            {
                CDialogTemplate dlgTemplateNew = new CDialogTemplate();
                /* There are several dialog formats to choose from */
                dlgTemplateNew.Template = new CDoubleColumnDialog();

                dlgTemplateNew.Template.sTitle = "Enter the PO Number to copy from";
                dlgTemplateNew.Template.sWidth = "300px";

                /* FogBugz dialogs post to default.asp via AJAX. To have this form post
                 * to the plugin raw page, we need to add the pg and ixPlugin values.
                 * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
                 * So we can just use api.Url.PluginRawPageUrl */
                CDialogItem itemNewHiddenUrl =
                    CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
                CDialogItem itemNewHiddenAction =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "Spreadfast_copycase");
                dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
                /* include a security action token */
                CDialogItem itemActionToken =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateNew.Template.Items.Add(itemActionToken);
                dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
                CDialogItem itemEditId = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("PONumber"), ""), "PONumber ");
                // CDialogItem itemEditVenId = new CDialogItem(GetSelects1(null, "CWFVendor", 9, true), "VendorName");
                //  CDialogItem itemEditVenId = new CDialogItem(GetSelectsVName(null, "CWFVendor", ixproj, true), "VendorName");
                //  CDialogItem itemEditPONum = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("PONumber"), ""), "PO Number ");
                //CDialogItem itemEditVenId1 = new CDialogItem(GetSelects1(null, "CWFVendor", 9, true), "VendorName");
                // itemEditVenId.sContent = GetSelects("CWFVendor", "CWFVendor", 14, true);
                dlgTemplateNew.Template.Items.Add(itemEditId);
          
                CDialogItem itemEditId2 =
                    new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute", "Header"));
                dlgTemplateNew.Template.Items.Add(itemEditId2);

                CDialogItem itemEditId3 =
                     new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("LineItems"), "true", "CheckedAttribute", "Line Items"));
                dlgTemplateNew.Template.Items.Add(itemEditId3);

                /* Standard ok and cancel buttons */
                dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

                // api.Notifications.AddAdminNotification("dlgTemplateNew returned", "");

                return dlgTemplateNew;
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error at dialog creation");
                return null;
            }

        }

        protected CDialogTemplate DialogTemplateNew_1(int ixBug, int ixproj)
        {

            try
            {
                CDialogTemplate dlgTemplateNew = new CDialogTemplate();
                /* There are several dialog formats to choose from */
                dlgTemplateNew.Template = new CDoubleColumnDialog();

                dlgTemplateNew.Template.sTitle = "Enter the case id to copy from";
                dlgTemplateNew.Template.sWidth = "300px";

                /* FogBugz dialogs post to default.asp via AJAX. To have this form post
                 * to the plugin raw page, we need to add the pg and ixPlugin values.
                 * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
                 * So we can just use api.Url.PluginRawPageUrl */
                CDialogItem itemNewHiddenUrl =
                    CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
                CDialogItem itemNewHiddenAction =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "PE_copycase");
                dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
                /* include a security action token */
                CDialogItem itemActionToken =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateNew.Template.Items.Add(itemActionToken);
                dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
                CDialogItem itemEditId = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("CaseID"), ""), "Case ID ");
                // CDialogItem itemEditVenId = new CDialogItem(GetSelects1(null, "CWFVendor", 9, true), "VendorName");
                CDialogItem itemEditVenId = new CDialogItem(GetSelectsVName(null, "CWFVendor", ixproj, true), "VendorName");

                //CDialogItem itemEditVenId1 = new CDialogItem(GetSelects1(null, "CWFVendor", 9, true), "VendorName");
                // itemEditVenId.sContent = GetSelects("CWFVendor", "CWFVendor", 14, true);
                dlgTemplateNew.Template.Items.Add(itemEditId);
                dlgTemplateNew.Template.Items.Add(itemEditVenId);
                //  itemEditVenId.sContent = GetSelects1(null,"CWFVendor", 14, true);
                //CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemEditVenId);
                //DialogItem.sLabel = fielddisplay;
                //DialogItem.sContent = GetSelects1(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
                //return DialogItem;
                //CDialogItem itemEditId1 =
                //    new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Dummy ID"), ""),
                //                    "Case ID dummy ");
                //dlgTemplateNew.Template.Items.Add(itemEditId1);

                CDialogItem itemEditId2 =
                    new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute", "Header"));
                dlgTemplateNew.Template.Items.Add(itemEditId2);

                CDialogItem itemEditId3 =
                     new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("LineItems"), "true", "CheckedAttribute", "Line Items"));
                dlgTemplateNew.Template.Items.Add(itemEditId3);

                /* Standard ok and cancel buttons */
                dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

                // api.Notifications.AddAdminNotification("dlgTemplateNew returned", "");

                return dlgTemplateNew;
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error at dialog creation");
                return null;
            }

        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew(int ixBug)
        {
        
            try
            {
                CDialogTemplate dlgTemplateNew = new CDialogTemplate();
                /* There are several dialog formats to choose from */
                dlgTemplateNew.Template = new CDoubleColumnDialog();

                dlgTemplateNew.Template.sTitle = "Enter the case id to copy from";
                dlgTemplateNew.Template.sWidth = "300px";

                /* FogBugz dialogs post to default.asp via AJAX. To have this form post
                 * to the plugin raw page, we need to add the pg and ixPlugin values.
                 * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
                 * So we can just use api.Url.PluginRawPageUrl */
                CDialogItem itemNewHiddenUrl =
                    CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
                CDialogItem itemNewHiddenAction =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "copycase_1");
                dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
                /* include a security action token */
                CDialogItem itemActionToken =
                    CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateNew.Template.Items.Add(itemActionToken);
                dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"),ixBug.ToString()));
                CDialogItem itemEditId = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("CaseID"), ""),"Case ID ");
                CDialogItem itemEditVenId = new CDialogItem(GetSelectsVName(null, "CWFVendor", 14, true), "VendorName");
                

                dlgTemplateNew.Template.Items.Add(itemEditId);
                dlgTemplateNew.Template.Items.Add(itemEditVenId);

                //CDialogItem itemEditId1 =
                //    new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Dummy ID"), ""),
                //                    "Case ID dummy ");
                //dlgTemplateNew.Template.Items.Add(itemEditId1);

                CDialogItem itemEditId2 =
                    new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute", "Header"));
                dlgTemplateNew.Template.Items.Add(itemEditId2);

                CDialogItem itemEditId3 =
                     new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("LineItems"), "true", "CheckedAttribute", "Line Items"));
                dlgTemplateNew.Template.Items.Add(itemEditId3);

                /* Standard ok and cancel buttons */
                dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

                // api.Notifications.AddAdminNotification("dlgTemplateNew returned", "");

                return dlgTemplateNew;
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error at dialog creation");
                return null;
            }

        }

        //Added by Alok Ends here
        /* This method builds the template for the item edit dialog.
         * FogBugz Dialogs use template variables, enclosed by curly braces.
         * These variables are populated later by the hashtable
         * specified by CDialogTemplate.AddTemplateData */
       


        protected CDoubleColumnDialog DialogEditForAjax(int ixLineItem, string sTableId, string ixProject)
        {
            string str1 = "GL Account", str2 = "Amount", str3 = "Tax type", str4 = "Tax",
            str5 = "Memo", str6 = "Department", 
            str7 = "Prepaid",
            str8 = "Addninfo", str10 = "vendor",
            str9 = "Dist Type",
            str11 = "Account Label:",
            str12 = "Unit Price:",
            str13="Quantity:",
            str14 = "Description:",
            str15 = "Project Code:",
            str16 = "Item:";
            int iProj = Int32.Parse(ixProject);
             
            string[] sAccountList = GetSelects3("CWFAccount", iProj, false);
            string[] sItemList = GetSelects3("CWFVat", iProj, false);
            string[] sDepartmentList = GetSelects3("CWFDepartment", iProj, false);
            string[] sBillableList = GetSelects4("CWFBillable", iProj, false);
            string[] sAccountList_Synergis = GetSelects3_Synegis("CWFAccount", iProj, false);
           
            CDoubleColumnDialog dlgTemplateEdit = new CDoubleColumnDialog();

            if (ixProject == "14")
            {
                dlgTemplateEdit.sTitle = "Edit Item ";
                CDialogItem itemEditHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateEdit.Items.Add(itemEditHiddenUrl);
                CDialogItem itemEditHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
                dlgTemplateEdit.Items.Add(itemEditHiddenAction);
                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixLineItem"), ixLineItem.ToString()));
                string strBugLineItem = ixLineItem.ToString();

                CSelectQuery sqlLineItemDetails;
                sqlLineItemDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));
                sqlLineItemDetails.AddSelect("*");
                sqlLineItemDetails.AddWhere(" ixBugLineItem = " + strBugLineItem + " AND iDeleted = 0");

                DataSet dsLineItem = sqlLineItemDetails.GetDataSet();
                DataRow dr = dsLineItem.Tables[0].Rows[0];

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), dr[1].ToString()));

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));


                /* include a security action token */
                CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateEdit.Items.Add(itemActionToken);

                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sAccount"),
                                                          Forms.SelectOptions(sAccountList, dr[2].ToString(),
                                                          sAccountList)), str1));

                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sTaxType"),
                                                          Forms.SelectOptions(sItemList, dr[4].ToString(),
                                                          sItemList)), str3));
                CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), dr[3].ToString()), str2);
                dlgTemplateEdit.Items.Add(itemAmount);

                CDialogItem itemTax = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), dr[5].ToString()), str4);
                dlgTemplateEdit.Items.Add(itemTax);

                CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), dr[6].ToString()), str5);
                dlgTemplateEdit.Items.Add(itemMemo);

                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sDepartment"),
                                                          Forms.SelectOptions(sDepartmentList, dr[7].ToString(), sDepartmentList)), str6));

                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sBillable"),
                                                          Forms.SelectOptions(sBillableList, dr[8].ToString(), sBillableList)), str7));

                CDialogItem itemAddinfo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sAddninfo"), dr[9].ToString()), str8);
                dlgTemplateEdit.Items.Add(itemAddinfo);

                /* Standard ok and cancel buttons */
              
            }
            else if (ixProject == "19")
            {
                dlgTemplateEdit.sTitle = "Edit Item ";
                CDialogItem itemEditHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateEdit.Items.Add(itemEditHiddenUrl);
                CDialogItem itemEditHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
                dlgTemplateEdit.Items.Add(itemEditHiddenAction);
                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixLineItem"), ixLineItem.ToString()));
                string strBugLineItem = ixLineItem.ToString();

                CSelectQuery sqlLineItemDetails;

                sqlLineItemDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));
                sqlLineItemDetails.AddSelect("*");
                sqlLineItemDetails.AddWhere(" ixBugLineItem = " + strBugLineItem + " AND iDeleted = 0");
                DataSet dsLineItem = sqlLineItemDetails.GetDataSet();
                DataRow dr = dsLineItem.Tables[0].Rows[0];

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), dr[1].ToString()));

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));

                /* include a security action token */
                CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateEdit.Items.Add(itemActionToken);

                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sAccount"),
                                                          Forms.SelectOptions(sAccountList, dr[2].ToString(),
                                                          sAccountList)), str1));

                //dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sTaxType"),
                //                                          Forms.SelectOptions(sItemList, dr[4].ToString(),
                //                                          sItemList)), str3));
                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sExtra2"),
                                                      Forms.SelectOptions(sItemList, dr[12].ToString(),
                                                      sItemList)), str9));
                CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), dr[3].ToString()), str2);
                dlgTemplateEdit.Items.Add(itemAmount);


                CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), dr[6].ToString()), str5);
                dlgTemplateEdit.Items.Add(itemMemo);
            }

            else if (ixProject == "23")
            {

                //sTableId = dlgTemplateEdit.sId;
             //   api.Notifications.AddAdminNotification("Projecr", "23");
                dlgTemplateEdit.sTitle = "Edit Item ";
                CDialogItem itemEditHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateEdit.Items.Add(itemEditHiddenUrl);
                CDialogItem itemEditHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
                dlgTemplateEdit.Items.Add(itemEditHiddenAction);
                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixLineItem"), ixLineItem.ToString()));
                string strBugLineItem = ixLineItem.ToString();

                CSelectQuery sqlLineItemDetails;
                // if (ixProject == "9")
                //  {
                sqlLineItemDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));

                sqlLineItemDetails.AddSelect("*");


                sqlLineItemDetails.AddWhere(" ixBugLineItem = " + strBugLineItem + " AND iDeleted = 0");

                DataSet dsLineItem = sqlLineItemDetails.GetDataSet();
                DataRow dr = dsLineItem.Tables[0].Rows[0];

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), dr[1].ToString()));

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));

                /* include a security action token */
                CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateEdit.Items.Add(itemActionToken);

                CDialogItem itemAddinfo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sAddninfo"), dr[9].ToString()), str10);
                dlgTemplateEdit.Items.Add(itemAddinfo);
                CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), dr[3].ToString()), str2);
                dlgTemplateEdit.Items.Add(itemAmount);

                CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), dr[6].ToString()), str5);
                dlgTemplateEdit.Items.Add(itemMemo);


            }

            if (ixProject == "25" )
            {
                dlgTemplateEdit.sTitle = "Edit Item ";
                CDialogItem itemEditHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateEdit.Items.Add(itemEditHiddenUrl);
                CDialogItem itemEditHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
                dlgTemplateEdit.Items.Add(itemEditHiddenAction);
                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixLineItem"), ixLineItem.ToString()));
                string strBugLineItem = ixLineItem.ToString();

                CSelectQuery sqlLineItemDetails;
                sqlLineItemDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));
                sqlLineItemDetails.AddSelect("*");
                sqlLineItemDetails.AddWhere(" ixBugLineItem = " + strBugLineItem + " AND iDeleted = 0");

                DataSet dsLineItem = sqlLineItemDetails.GetDataSet();
                DataRow dr = dsLineItem.Tables[0].Rows[0];

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), dr[1].ToString()));

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));


                /* include a security action token */
                CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateEdit.Items.Add(itemActionToken);

                CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), dr[6].ToString()), str14);
                dlgTemplateEdit.Items.Add(itemMemo);

                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sAccount"),
                                                          Forms.SelectOptions(sAccountList_Synergis, dr[2].ToString(),
                                                          sAccountList_Synergis)), str11));

                CDialogItem itemdept = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sDepartment"), dr[7].ToString()), str15);
                dlgTemplateEdit.Items.Add(itemdept);

                CDialogItem itemTax = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), dr[5].ToString()), str13);
                dlgTemplateEdit.Items.Add(itemTax);

                CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), dr[3].ToString()), str12);
                dlgTemplateEdit.Items.Add(itemAmount);

                //dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sDepartment"),
                //                                          Forms.SelectOptions(sDepartmentList, dr[7].ToString(), sDepartmentList)), str6));


                /* Standard ok and cancel buttons */

            }

            if (ixProject == "26")
            {
                dlgTemplateEdit.sTitle = "Edit Item ";
                CDialogItem itemEditHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateEdit.Items.Add(itemEditHiddenUrl);
                CDialogItem itemEditHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
                dlgTemplateEdit.Items.Add(itemEditHiddenAction);
                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixLineItem"), ixLineItem.ToString()));
                string strBugLineItem = ixLineItem.ToString();

                CSelectQuery sqlLineItemDetails;
                sqlLineItemDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));
                sqlLineItemDetails.AddSelect("*");
                sqlLineItemDetails.AddWhere(" ixBugLineItem = " + strBugLineItem + " AND iDeleted = 0");

                DataSet dsLineItem = sqlLineItemDetails.GetDataSet();
                DataRow dr = dsLineItem.Tables[0].Rows[0];

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), dr[1].ToString()));

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));


                /* include a security action token */
                CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateEdit.Items.Add(itemActionToken);

                CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), dr[6].ToString()), str14);
                dlgTemplateEdit.Items.Add(itemMemo);

                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sAccount"),
                                                          Forms.SelectOptions(sAccountList_Synergis, dr[2].ToString(),
                                                          sAccountList_Synergis)), str11));

                CDialogItem itemdept = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sDepartment"), dr[7].ToString()), str15);
                dlgTemplateEdit.Items.Add(itemdept);

                CDialogItem itemTax = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), dr[5].ToString()), str13);
                dlgTemplateEdit.Items.Add(itemTax);

                CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), dr[3].ToString()), str12);
                dlgTemplateEdit.Items.Add(itemAmount);


                //dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sDepartment"),
                //                                          Forms.SelectOptions(sDepartmentList, dr[7].ToString(), sDepartmentList)), str6));


                /* Standard ok and cancel buttons */

            }

            if (ixProject == "27")
            {
                dlgTemplateEdit.sTitle = "Edit Item ";
                CDialogItem itemEditHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
                dlgTemplateEdit.Items.Add(itemEditHiddenUrl);
                CDialogItem itemEditHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
                dlgTemplateEdit.Items.Add(itemEditHiddenAction);
                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixLineItem"), ixLineItem.ToString()));
                string strBugLineItem = ixLineItem.ToString();

                CSelectQuery sqlLineItemDetails;
                sqlLineItemDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));
                sqlLineItemDetails.AddSelect("*");
                sqlLineItemDetails.AddWhere(" ixBugLineItem = " + strBugLineItem + " AND iDeleted = 0");

                DataSet dsLineItem = sqlLineItemDetails.GetDataSet();
                DataRow dr = dsLineItem.Tables[0].Rows[0];

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), dr[1].ToString()));

                dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));


                /* include a security action token */
                CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                dlgTemplateEdit.Items.Add(itemActionToken);

                CDialogItem itemMemo = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), dr[6].ToString()), str14);
                dlgTemplateEdit.Items.Add(itemMemo);

                dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sAccount"),
                                                          Forms.SelectOptions(sAccountList_Synergis, dr[2].ToString(),
                                                          sAccountList_Synergis)), str11));

                CDialogItem itemdept = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sDepartment"), dr[7].ToString()), str16);
                dlgTemplateEdit.Items.Add(itemdept);

                CDialogItem itemTax = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), dr[5].ToString()), str13);
                dlgTemplateEdit.Items.Add(itemTax);

                CDialogItem itemAmount = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), dr[3].ToString()), str12);
                dlgTemplateEdit.Items.Add(itemAmount);


                //dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sDepartment"),
                //                                          Forms.SelectOptions(sDepartmentList, dr[7].ToString(), sDepartmentList)), str6));


                /* Standard ok and cancel buttons */

            }

            dlgTemplateEdit.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));
            return dlgTemplateEdit;
           
        }

        private CDialogItem EditDialogDropDown(string sFieldName, string sDisplay, int ixProject, string sSelected)
        {
           return null;
        }

        private CDialogItem EditDialogDropDown2(string sFieldName, string sDisplay, int ixProject, string sSelected)
        {
           return null;
        }
        
        /* This method builds the template for the delete item dialog */
        protected CDialogTemplate DialogTemplateDelete(string sTableId, int ixProject)
        {
            CDialogTemplate dlgTemplateDelete = new CDialogTemplate();
            dlgTemplateDelete.Template = new CSingleColumnDialog();
            dlgTemplateDelete.Template.sTitle = "Delete Item ";
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBugLineItem"), "{ixBugLineItem}"));
            CDialogItem itemDeleteHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateDelete.Template.Items.Add(itemDeleteHiddenUrl);
            CDialogItem itemDeleteHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "delete");
            dlgTemplateDelete.Template.Items.Add(itemDeleteHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateDelete.Template.Items.Add(itemActionToken);
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"),"{ixBug}"));
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"),ixProject.ToString()));
            /* DialogItems don't have to be form elements, they can also be simple html */
            dlgTemplateDelete.Template.Items.Add(new CDialogItem("Do you want to delete this item ?"));
            dlgTemplateDelete.Template.Items.Add(new CDialogItem("    "));

            /* Standard ok and cancel buttons */
            dlgTemplateDelete.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateDelete;
        }

        protected CDialogTemplate DialogTemplateDelete_Synergis(string sTableId, int ixProject)
        {
            CDialogTemplate dlgTemplateDelete = new CDialogTemplate();
            dlgTemplateDelete.Template = new CSingleColumnDialog();
            dlgTemplateDelete.Template.sTitle = "Delete Item ";
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBugLineItem"), "{ixBugLineItem}"));
            CDialogItem itemDeleteHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateDelete.Template.Items.Add(itemDeleteHiddenUrl);
            CDialogItem itemDeleteHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "delete_Syn");
            dlgTemplateDelete.Template.Items.Add(itemDeleteHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateDelete.Template.Items.Add(itemActionToken);
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), "{ixBug}"));
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));
            /* DialogItems don't have to be form elements, they can also be simple html */
            dlgTemplateDelete.Template.Items.Add(new CDialogItem("Do you want to delete this item ?"));
            dlgTemplateDelete.Template.Items.Add(new CDialogItem("    "));

            /* Standard ok and cancel buttons */
            dlgTemplateDelete.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateDelete;
        }
        
        #endregion

        #region Utility Methods

        //public string GetSecretCodeText(CBug[] rgbug)
        //{
        //    if (rgbug == null || rgbug.Length == 0)
        //        return "";

        //    if (PluginFieldVaries(rgbug, str_CaseIDFieldName))
        //        return VARIOUS_TEXT;
        //    else
        //        return Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, str_CaseIDFieldName));
        //}

        /* these two methods are used to construc the Urls which a user would
        * follow if javascript is disabled (preventing the use of the Dialogs */
        protected string CommandUrl(string sCommand, int ixBugLineItem, string ixBug)
        {
            return string.Concat(api.Url.PluginPageUrl(),
                                 LinkParameter("sCommand", sCommand),
                                 LinkParameter("ixBugLineItem", ixBugLineItem.ToString()),
                                 LinkParameter("ixBug", ixBug));
        }

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
                    api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", sTableName));
                sq.AddSelect("s" + sTableName + "Name");
                sq.AddWhere(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", sTableName) + ".ix" + sTableName + " = " + sValue);
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

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType),
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

                //  api.Notifications.AddAdminNotification("a", sType);

                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names,
                                                     sSelected,
                                                     names)));


            }
            ds.Dispose();
            return String.Empty;
        }

        protected string GetSelects_Terms_Syner(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            //sq.AddOrderBy(string.Format("{0}.{1} {2}",
            //                                api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType),
            //                                "s" + sType + "Value"
            //                                ));

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

                //  api.Notifications.AddAdminNotification("a", sType);

                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names,
                                                     sSelected,
                                                     names)));


            }
            ds.Dispose();
            return String.Empty;
        }

        protected string GetSelectDate(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;

            DateTime dt = new DateTime();

            dt = DateTime.Now;

            string month = (dt.ToString("MMM"));

            string year = (dt.ToString("yyyy"));

            string monyr1 = month + " " + year;


            DateTime dt2 = (dt.AddMonths(1));

            string month2 = (dt2.ToString("MMM"));

            string year2 = (dt2.ToString("yyyy"));

            string monyr2 = month2 + " " + year2;

            names = new string[2];
            names[0] = monyr1;
            names[1] = monyr2;

            return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                               Forms.SelectOptions(names, sSelected, names)));
        }

        protected string GetSelects1(string sType, int ixProject)
        {
            return GetSelects1(null, sType, ixProject, false);
        }

        protected string GetSelects1(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

          //  CSelectQuery sq = api.Database.NewSelectQuery("Plugin_55_" + sType);
           CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
                                            "s" + sType + "Name", "ASC"));

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
                        //+ " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                       + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                      //names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                      //+ " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " " +(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                 }

                ds.Dispose();
                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names,sSelected, names)));
                //return HttpUtility.HtmlEncode(Forms.SelectInputString(api.AddPluginPrefix(sType),
                //                 Forms.SelectOptions(names,sSelected, names)));
                
            }
            ds.Dispose();
            return String.Empty;
        }

        protected string GetSelects_SE(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            //  CSelectQuery sq = api.Database.NewSelectQuery("Plugin_55_" + sType);
            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
                                            "s" + sType + "Id", "ASC"));

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
                        //+ " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                       + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        //names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        //+ " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                }

                ds.Dispose();
                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names, sSelected, names)));
                //return HttpUtility.HtmlEncode(Forms.SelectInputString(api.AddPluginPrefix(sType),
                //                 Forms.SelectOptions(names,sSelected, names)));

            }
            ds.Dispose();
            return String.Empty;
        }

        protected string GetSelects1_Synergis(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            //  CSelectQuery sq = api.Database.NewSelectQuery("Plugin_55_" + sType);
            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
                                            "s" + sType + "Id", "ASC"));

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
                        //+ " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                       + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        //names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        //+ " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                }

                ds.Dispose();
                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names, sSelected, names)));
                //return HttpUtility.HtmlEncode(Forms.SelectInputString(api.AddPluginPrefix(sType),
                //                 Forms.SelectOptions(names,sSelected, names)));

            }
            ds.Dispose();
            return String.Empty;
        }

        protected string GetSelects_Cambridge(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
                                            "s" + sType + "Name", "ASC"));

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
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString());
                        // + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString());
                        //+ " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                }

                ds.Dispose();
                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names, sSelected, names)));

            }
            ds.Dispose();
            return String.Empty;
        }

        protected string GetSelectsVName(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
            * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
                                            "s" + sType + "Name", "ASC"));

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
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString());
                        // + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString());
                        //+ " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                }

                ds.Dispose();
                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names, sSelected, names)));

            }
            ds.Dispose();
            return String.Empty;
        }
                       //  (GetSelectsBlanketPO(null, "CGSInvoice_MLA", ixproj, true)
        protected string GetSelectsBlanketPO(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
            * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            //CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            //sq.AddSelect("*");
            //sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            //sq.AddOrderBy(string.Format("{0}.{1} {2}",
            //                                api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
            //                                "s" + sType + "Name", "ASC"));

            //DataSet ds = sq.GetDataSet();
            int Count;
           // api.Notifications.AddAdminNotification("1","1");
            CSelectQuery Blanket_PO = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", sType));
                Blanket_PO.AddSelect("B_PO_Number, ixBug");
                Blanket_PO.AddWhere("ixproject = 27" );
                Blanket_PO.AddWhere("B_PO_Number IS NOT NULL");
                //Blanket_PO.AddOrderBy(string.Format("{0}.{1} {2}",
                //                                api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", sType),
                //                                "s" + sType + "Name", "ASC"));
                DataSet ds = Blanket_PO.GetDataSet();
               // api.Notifications.AddAdminNotification("2", "2");

            if (ds.Tables[0] != null && (Count = ds.Tables[0].Rows.Count) > 0)
            {
                names = new string[Count];
                ixs = new string[Count];
              //  api.Notifications.AddAdminNotification("3", "3");
                for (int i = 0; i < Count; i++)

                {


                    if (bDisplayId)
                    {
                        CBugQuery bug_title = api.Bug.NewBugQuery();
                        bug_title.IgnorePermissions = true;
                        bug_title.AddSelect("Bug.sTitle");
                        bug_title.AddWhere(" Bug.ixBug =" +(ds.Tables[0].Rows[i]["ixBug"].ToString()));// bug.ixBug.ToString());

                        DataSet ds_title = bug_title.GetDataSet();

                        if (ds_title.Tables.Count > 0 && ds_title.Tables[0] != null && ds_title.Tables[0].Rows.Count > 0)
                        {
                            string bugtitle = "";
                            try
                            {
                                 bugtitle =((ds_title.Tables[0].Rows[0]["sTitle"]).ToString());
                            }

                            catch
                            {
                                // just keep going
                            }
                            names[i] = ("B-PO-" + ds.Tables[0].Rows[i]["B_PO_Number"].ToString())
                        + " - " + (bugtitle);
                            ixs[i] = (ds.Tables[0].Rows[i]["B_PO_Number"].ToString());
                        }

                       
                        
                        

                    }
                    //else
                    //{
                    //    names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString());
                    //    //+ " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    //}
                }
               // api.Notifications.AddAdminNotification("4", "4");
                ds.Dispose();
                return (Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names, sSelected, ixs)));

            }
            ds.Dispose();
            return String.Empty;
        }


        protected string GetSelects2(string sType, int ixProject)
        {
            return GetSelects2(null, sType, ixProject, false);
        }

        protected string GetSelects2(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
                                            "s" + sType + "Name",
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
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                      + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                    //  ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                }

                ds.Dispose();

                //  return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                //                              Forms.SelectOptions(names,
                //                                            sSelected,
                //                                          ixs));

                //api.Notifications.AddAdminNotification("2", sType);

                return (Forms.SelectInputString(api.AddPluginPrefix("s" + sType),
                                 Forms.SelectOptions(names,
                                                     sSelected,
                                                     names)));
                
            }
            ds.Dispose();
            return String.Empty;
        }




        // protected string GetSelects3(string sSelected, string sType, int ixProject, bool bDisplayId)
        protected string[] GetSelects3(string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;
            string[] empty = null;
            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
                                            "s" + sType + "Name",
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
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                      + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                    //  ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                }

                ds.Dispose();

               
                return (names);


            }
            ds.Dispose();
            // return String.Empty;
            return empty;

        }

        protected string[] GetSelects3_Synegis(string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;
            string[] empty = null;
            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sType),
                                            "s" + sType + "Id",
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
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        names[i] = (ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                      + " " + (ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                    //  ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                }

                ds.Dispose();


                return (names);


            }
            ds.Dispose();
            // return String.Empty;
            return empty;

        }


        // protected string GetSelects4(string sSelected, string sType, int ixProject, bool bDisplayId)

        protected string[] GetSelects4(string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;
            string[] empty = null;
            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("CGSDatabaseSchema@conseroglobal.com", sType),
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



                //  return HttpUtility.HtmlEncode(Forms.SelectInputString(api.AddPluginPrefix(sType),
                //  Forms.SelectOptions(names,
                //       sSelected,
                //     names)));
                return names;

            }
            ds.Dispose();
            return empty;
        }


        #endregion

        #region Program for sending email

        // Method for sendig emails
        public void mailsender(string sMailAdderss, CBug bug, String mailsub, string mailbody, int Iperson)
        {

            //string sStatus="", sCategory="", sArea="", sPriority="", sProject="", sPerson="";

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

        public void mailsender_Tax(string sMailAdderss, CBug bug, String mailsub, string mailbody, int Iperson)
        {

            //string sStatus="", sCategory="", sArea="", sPriority="", sProject="", sPerson="";

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

                body += "Tax Team.";
                body += System.Environment.NewLine;

                api.Mail.SendTextEmail(sMailAdderss, mailsub, body);

                //  api.Mail.SendTextEmail(sMailAdderss, "Fogbugz (case " + bug.ixBug + " ) " + sProject + " " + bug.sTitle, body);
            }
        }


        public void mailsender_Syn(string sMailAdderss, CBug bug, String mailsub, string mailbody, int Iperson)
        {

            //string sStatus="", sCategory="", sArea="", sPriority="", sProject="", sPerson="";

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

                body += "Team Synergis.";
                body += System.Environment.NewLine;

                api.Mail.SendTextEmail(sMailAdderss, mailsub, body);

                //  api.Mail.SendTextEmail(sMailAdderss, "Fogbugz (case " + bug.ixBug + " ) " + sProject + " " + bug.sTitle, body);
            }
        }

        public void mailsender_Spreadfast(string sMailAdderss, CBug bug, String mailsub, string mailbody, int Iperson)
        {

            //string sStatus="", sCategory="", sArea="", sPriority="", sProject="", sPerson="";

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

                body += "Team Spreadfast.";
                body += System.Environment.NewLine;

                api.Mail.SendTextEmail(sMailAdderss, mailsub, body);

                //  api.Mail.SendTextEmail(sMailAdderss, "Fogbugz (case " + bug.ixBug + " ) " + sProject + " " + bug.sTitle, body);
            }
        }

        public void mailsender_Finance(string sMailAdderss, CBug bug, String mailsub, string mailbody, int Iperson)
        {

            //string sStatus="", sCategory="", sArea="", sPriority="", sProject="", sPerson="";

            if (sMailAdderss != null)
            {


                string body = "Dear Finance,";
                body += System.Environment.NewLine;
                body += System.Environment.NewLine;


                body += mailbody;//"The case" + bug.ixBug + " is pending for action";
                body += System.Environment.NewLine;
                body += System.Environment.NewLine;
                body += "Regards,";
                body += System.Environment.NewLine;

                body += "Team Synergis.";
                body += System.Environment.NewLine;

                api.Mail.SendTextEmail(sMailAdderss, mailsub, body);

                //  api.Mail.SendTextEmail(sMailAdderss, "Fogbugz (case " + bug.ixBug + " ) " + sProject + " " + bug.sTitle, body);
            }
        }

        public void mailsender_Finance_SF(string sMailAdderss, CBug bug, String mailsub, string mailbody, int Iperson)
        {

            //string sStatus="", sCategory="", sArea="", sPriority="", sProject="", sPerson="";

            if (sMailAdderss != null)
            {


                string body = "Dear Finance,";
                body += System.Environment.NewLine;
                body += System.Environment.NewLine;


                body += mailbody;//"The case" + bug.ixBug + " is pending for action";
                body += System.Environment.NewLine;
                body += System.Environment.NewLine;
                body += "Regards,";
                body += System.Environment.NewLine;

                body += "Team Spreadfast.";
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
            string sCountry = "";
            string sFolderdate = "";
           
            DateTime bugdate = bug.dtOpened;
            sFolderdate = bugdate.ToString("MM.dd.yy");


            //querying Custom bugfields for invoice and vendor name to attch with mail subject start

            {
                //   String tname = "Plugin_40_CustomBugData";
                
                CSelectQuery File_det = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                File_det.AddSelect("CWFVendor,sInvoiceNumber,sInvoiceDate,TotalAmount,CWFCountry");
                File_det.AddWhere("ixBug = " + bug.ixBug.ToString());
                DataSet Dcust = File_det.GetDataSet();

                if (Dcust.Tables.Count > 0 && Dcust.Tables[0] != null && Dcust.Tables[0].Rows.Count > 0)
                {
                  
                    sInvoiceNumber = Convert.ToString(Dcust.Tables[0].Rows[0]["sInvoiceNumber"]);
                    sVendorName = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFVendor"]);
                    try
                    {
                        sINVDate = Convert.ToDateTime(Dcust.Tables[0].Rows[0]["sInvoiceDate"]);
                    }

                    catch
                    {
                        return;
                    }
                    sAmount = Convert.ToString(Dcust.Tables[0].Rows[0]["TotalAmount"]);
                    sCountry = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFCountry"]);
                    //a sInvoice will be havinf format HH:MM:SS we use below code for formating
                    DateTime dt = sINVDate;
                    sINVDate2 = dt.ToString("MM.dd.yy");
                }
              }
            
            string sDate = "";
            string sCaseNumber = bug.ixBug.ToString();

            if (!string.IsNullOrEmpty(sInvoiceNumber) && !string.IsNullOrEmpty(sVendorName))
            {
                
                DateTime now = DateTime.Now;
                sDate = now.Year.ToString() + "_" + now.Month.ToString() + "_" + now.Day.ToString();
                //sFileName = sInvoiceNumber + "" + sVendorName + "_" + sDate + "_" + sCaseNumber;
                sFileName = sVendorName + "," + sINVDate2 + "," + sInvoiceNumber + "," + sAmount;
            }

            if (!string.IsNullOrEmpty(sFileName))
            {
                string fileBackupPath = "";
               
                CProject project = api.Project.GetProject(bug.ixProject);
                //string backUpLocation = "D:";//Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sBackupLocation"));
                //string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Default Sync Folder"
               // string backUpLocation = "C:\\Users\\pramalingappa.CONSEROGLOBAL\\Documents\\My Box Files\\Ford Direct\\" + sCountry + "\\" + sFolderdate;
               //string backUpLocation = "C:\\MLA_Files\\" + sCountry + "\\" + sFolderdate;
                string backUpLocation = "C:\\Users\\Administrator\\Documents\\My Box Files\\Default Sync Folder\\" + sCountry + "\\" + sFolderdate;
               
                CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                attachmentQuery.AddWhere("Bug.ixBug = " + bug.ixBug.ToString());
                attachmentQuery.IgnorePermissions = true;
                attachmentQuery.ExcludeDeleted = true;
                DataSet ds = attachmentQuery.GetDataSet();
                List<CAttachment> attachments = new List<CAttachment>();
                //if (null != ds.Tables[0] && ds.Tables[0].Rows.Count == 1)

                if (null != ds.Tables[0] && ds.Tables[0].Rows.Count > 0)
                {
                    string sExtn = "";
                    int icount = 0;
                    //loop to check multiple attachments  
                   
                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                    {
                        string sFilename2 = sFileName;
                        //int ixAttachment = Convert.ToInt32(ds.Tables[0].Rows[0]["ixAttachment"]);
                        int ixAttachment = Convert.ToInt32(ds.Tables[0].Rows[j]["ixAttachment"]);
                        CAttachment attachment = api.Attachment.GetAttachment(ixAttachment);
                        string[] fileNameDetails = attachment.sFileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        if (fileNameDetails.Length > 1)
                        {
                           
                            string fileExtension = fileNameDetails[fileNameDetails.Length - 1];
                            sExtn = fileExtension.ToLower();
                        }
                        // for checking extension

                        if (sExtn == "doc" || sExtn == "pdf" || sExtn == "bmp" || sExtn == "jpg" || sExtn == "jpeg" || sExtn == "xls" || sExtn == "xlsx" || sExtn == "docx" || sExtn == "gif" || sExtn == "tif")// || sExtn == "png")
                        {
                          
                            int sAttachmentold = ixAttachment;
                            if (icount > 0)
                            {
                                sFilename2 += "_" + icount;
                            }

                            icount = icount + 1;
                            sFilename2 += ".";
                            sFilename2 += sExtn;

                            if (attachment.sFileName != sFilename2)
                            {
                                
                               CAttachment clonedAttachment = CloneAttachment(attachment, sFilename2);
                               attachments.Add(clonedAttachment);
                                bugevent.CommitAttachmentAssociation(attachments.ToArray());
                               //
                                
                                
                              //  if (!string.IsNullOrEmpty(backUpLocation))

                                {
                                    // fileBackupPath = backUpLocation + "\\" + project.sProject + "\\" + sVendorName + "\\" + sDate + "\\" + sFileName;
                                   // fileBackupPath = backUpLocation + "\\" + sFilename2;
                                   //fileBackupPath= "C:\\MLA_Files\\" + sCountry + "\\" + sFolderdate + "\\" + sFilename2;
                                    fileBackupPath = backUpLocation + "\\" + sFilename2;
                                    CreateDirectory(new DirectoryInfo(Path.GetDirectoryName(fileBackupPath)));
                                   // api.Notifications.AddMessage("File has been backed up as " + Path.GetFullPath(fileBackupPath));
                                    api.Notifications.AddMessage("Invoice has been backed up succsessfuly");
                                    FileStream fileStream = new FileStream(Path.GetFullPath(fileBackupPath), FileMode.Create, FileAccess.Write);
                                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                                    binaryWriter.Write(attachment.rgbData);
                                   // api.Notifications.AddMessage("File has been backed up as " + binaryWriter);
                                    binaryWriter.Close();
                                    fileStream.Close();
                                    fileStream.Dispose();
                                }

                                // bug.DeleteAttachment(ixAttachment);
                                bug.DeleteAttachment(sAttachmentold);
                            }
                        }
                        ds.Dispose();
                    }
                }


            }
        }

      //  [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
      //  [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
     // [ReflectionPermission(SecurityAction.Assert)]

      //  [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
       // [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]

        CAttachment CloneAttachment(CAttachment attachment, string sFileName)
        {

            // find the type for the internal CAttachment class    
            // (this is different from FogCreek.FogBugz.Plugins.Entity.CAttachment)   
            var ase = Assembly.Load("FogBugz");
            var tCAttachment = ase.GetType("FogCreek.FogBugz.CAttachment");

            if (tCAttachment == null)
                throw new Exception("Couldn't load 'FogCreek.FogBugz.CAttachment' type.");
            // find the constructor and create an instance    
            var ctor = tCAttachment.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                null, new Type[] { }, null);

            if (ctor == null)
                throw new Exception("Couldn't find default CAttachment constructor.");

            var entity = ctor.Invoke(null);
            // create a new attachment record in the DB    
            //this.api.Notifications.AddMessage("Renamefile-13" + entity);
            var storeAttachmentMethod = tCAttachment.GetMethod("StoreAttachmentInDB", BindingFlags.Instance | BindingFlags.NonPublic);

            if (storeAttachmentMethod == null)
                throw new Exception("Couldn't find StoreAttachmentInDB method.");
            var ixAttachment = (int)storeAttachmentMethod.Invoke(entity, new object[] { attachment.rgbData, sFileName });
            if (ixAttachment == 0)
                throw new Exception("Unable to clone attachment.");

            return api.Attachment.GetAttachment(ixAttachment);

        }

        protected string CommandUrl_1(string sCommand, int ixLineItem)
        {
            return string.Concat(api.Url.PluginPageUrl(),
                                 LinkParameter("sCommand", sCommand),
                                 LinkParameter("ixLineItem", ixLineItem.ToString()));
        }

        // hack to test ajax response
        protected string AJAXUrl(string sCommand)
        {
            return string.Concat(api.Url.PluginRawPageUrl(),
                                 LinkParameter("sAction", sCommand),
                                 LinkParameter("actionToken", api.Security.GetActionToken()));
        }

        #region Invoice Details

        protected CEditableTable ItemTable_POInvoiceDetails(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {

            api.Notifications.AddAdminNotification("bSuppressEditsAndDeletes", bSuppressEditsAndDeletes.ToString());

            CEditableTable editableTableItems = new CEditableTable("itemtable_POInvoice");
            string sTableId = editableTableItems.sId;
            /* Define the header row of the table */
            //if (!bSuppressEditsAndDeletes)
            //{
            //    // editableTableItems.Header.AddCell("Edit");
            //    editableTableItems.Header.AddCell("Delete");
            //}
            editableTableItems.Header.AddCell("LineItemId");
            editableTableItems.Header.AddCell("InvoiceNumber");
            editableTableItems.Header.AddCell("InvoiceDate");
            editableTableItems.Header.AddCell("Amount");
            editableTableItems.Header.AddCell("Memo");



            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTableItems.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */
            CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sTableId, ixProject);

            /* Create the new item dialog template object used when the user clicks Add
             * New Item or the add icon in the footer row */
            //   api.Notifications.AddAdminNotification("calling addnew 2table id ", "yes 2 -" + sTableId.ToString());
            CDialogTemplate dlgTemplateNew_PO = DialogTemplateNew_POInvoiceDetails(ixBug, ixProject, sTableId);
            //   api.Notifications.AddAdminNotification("calling addnew 3", "yes 3");
            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            //CDialogTemplate dlgTemplateDelete_POI = DialogTemplateDelete_POI(sTableId, ixProject);

            /* setup a DataSet and fetch the items from the database */
            DataSet dsItems = FetchItems_1(ixBug, true);
            int ixCGSInvoiceNumber = -1;
            string sMemo = "";
            double dAmount = -1D;
            string sInvoiceNumber = "";
            string sInvoiceDate = "";
            int LineItemId = -1;

            // api.Notifications.AddAdminNotification("calling addnew 4", "yes 4");
            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */
            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {
                    //api.Notifications.AddAdminNotification("Trying to load the items", "This is not loading whu?");
                    ixCGSInvoiceNumber = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugLineItem_PMI"]);
                    sInvoiceNumber = Convert.ToString(dsItems.Tables[0].Rows[i]["sInvoiceNumber"]);
                    sInvoiceDate = Convert.ToString(dsItems.Tables[0].Rows[i]["sInvoiceDate"]);
                    sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                    LineItemId = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixLineItemId_Inv"]);
                    try
                    {
                        dAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                    }
                    catch
                    {
                        dAmount = 0d;
                    }

                    //sGlAccountName = dsItems.Tables[0].Rows[i]["sGlAccountName"].ToString();

                    /* create a new table row and set the row id to the unique ixBugInvoiceItem */
                    CEditableTableRow row = new CEditableTableRow();
                    row.sRowId = ixCGSInvoiceNumber.ToString();
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    //    api.Notifications.AddAdminNotification("calling addnew 5", "yes 5");
                    //if (!bSuppressEditsAndDeletes)
                    //{
                    //    //    //row.AddCell(CEditableTable.LinkShowDialogEditIcon(
                    //    //    //                sTableId,
                    //    //    //                "edit",
                    //    //    //                row.sRowId,
                    //    //    //                CommandUrl("edit", ixBugInvoiceItem, ixBug.ToString())));

                    //    row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                    //                    sTableId,
                    //                    "delete_POInvoice",
                    //                    row.sRowId,
                    //                    CommandUrl("delete_POInvoice", ixCGSInvoiceNumber, ixBug.ToString())));
                    //    api.Notifications.AddAdminNotification("delete_POInvoice1", "delete1");
                    //}
                    /* make sure to run HtmlEncode on any user data before displaying it! */
                    row.AddCell(HttpUtility.HtmlEncode(LineItemId.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sInvoiceNumber.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sInvoiceDate.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(dAmount.ToString("C")));
                    row.AddCell(HttpUtility.HtmlEncode(sMemo.ToString()));

                    editableTableItems.Body.AddRow(row);

                    if (!bSuppressEditsAndDeletes)
                    {

                        /* Now that the row is populated for display, put the data in a hash table
                         * to be used in populating the pop-up add, edit and delete dialogs. */
                        Hashtable hashData = new Hashtable();
                        hashData.Add("ixBugLineItem_PMI", ixCGSInvoiceNumber);
                        hashData.Add("ixBug", ixBug);
                        hashData.Add("ixProject", ixProject);
                        hashData.Add("fAmount", dAmount);
                        hashData.Add("sMemo", sMemo);
                        hashData.Add("sInvoiceNumber", sInvoiceNumber);
                        hashData.Add("sInvoiceDate", sInvoiceDate);
                        hashData.Add("LineItemId", LineItemId);
                        //  api.Notifications.AddAdminNotification("calling addnew 6", "yes 6");
                        //hashData.Add("iFrom99", iForm99.ToString());

                        /* add the hash table as data to the edit template */
                        //  dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                        /* add the data to the delete template as well */
                        //    dlgTemplateDelete_POI.AddTemplateData(row.sRowId, hashData);
                        //api.Notifications.AddAdminNotification(row.sRowId.ToString(), hashData.ToString());
                    }
                }
            }
            else
            {
                /* If there are no items, just display a note in a full-width cell */
                CEditableTableRow row = new CEditableTableRow();
                row.sRowId = "none";
                row.AddCellWithColspan("No Items Yet...", nColCount);
                editableTableItems.Body.AddRow(row);
            }
            dsItems.Dispose();

            if (!bSuppressEditsAndDeletes)
            // if (bSuppressEditsAndDeletes==true)
            {
                // api.Notifications.AddAdminNotification("calling addnew 7", "yes 7");
                /* Add a footer row with icon and text links to the add new item dialog */
                editableTableItems.Footer.AddCell(CEditableTable.LinkShowDialogNewIcon(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixCGSInvoiceNumber, ixBug.ToString())));
                editableTableItems.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixCGSInvoiceNumber, ixBug.ToString()),
                                                        "Add New Item"),
                                                        nColCount - 1);
                // api.Notifications.AddAdminNotification("calling addnew 8", "yes 8");
                /* Associate the dialog templates with the table by name */
                editableTableItems.AddDialogTemplate("new", dlgTemplateNew_PO);
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
                //editableTableItems.AddDialogTemplate("delete_POInvoice", dlgTemplateDelete_POI);
            }

            return editableTableItems;
        }

        protected CDialogTemplate DialogTemplateNew_POInvoiceDetails(int ixBug, int ixProject, string sTableId)
        {
            // api.Notifications.AddAdminNotification ("calling addnew 1" ,"yes 1");

            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();
            dlgTemplateNew.Template.sTitle = "Add New Item";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new_POInvoice");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixBug"),
                                                   ixBug.ToString()));
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixProject"),
                                                   ixProject.ToString()));

            CDialogItem itemLineId =
               new CDialogItem(Forms.TextInput(api.AddPluginPrefix("ixLineItemId_Inv"), ""),
                               "LineItemId");
            dlgTemplateNew.Template.Items.Add(itemLineId);

            CDialogItem itemAmount =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""),
                                "Amount");
            dlgTemplateNew.Template.Items.Add(itemAmount);

            CDialogItem iteminvoiceNo =
           new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sInvoiceNumber"), ""),
                           "InvoiceNumber");
            dlgTemplateNew.Template.Items.Add(iteminvoiceNo);

            CDialogItem iteminvoicedate =
           new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sInvoiceDate"), ""),
                           "InvoiceDate");
            dlgTemplateNew.Template.Items.Add(iteminvoicedate);


            CDialogItem itemMemo =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""),
                                "Memo");
            dlgTemplateNew.Template.Items.Add(itemMemo);


            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateNew;
        }

        /* This method builds the template for the item edit dialog.
         * FogBugz Dialogs use template variables, enclosed by curly braces.
         * These variables are populated later by the hashtable
         * specified by CDialogTemplate.AddTemplateData */
        protected CDialogTemplate DialogTemplateEdit(string sTableId, int ixProject)
        {
            CDialogTemplate dlgTemplateEdit = new CDialogTemplate();
            dlgTemplateEdit.Template = new CDoubleColumnDialog();
            /* names in curly braces are replaced with the otuput of the ToString()
             * method for the corresponding value in the template's data hashtable */
            dlgTemplateEdit.Template.sTitle = "Edit Item ";
            CDialogItem itemEditHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateEdit.Template.Items.Add(itemEditHiddenUrl);
            CDialogItem itemEditHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit_POInvoice");
            dlgTemplateEdit.Template.Items.Add(itemEditHiddenAction);
            dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
                                                    api.AddPluginPrefix("ixCGSInvoiceNumber"),
                                                    "{ixCGSInvoiceNumber}"));
            dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
                                                    api.AddPluginPrefix("ixBug"),
                                                    "{ixBug}"));
            dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
                                                  api.AddPluginPrefix("ixProject"),
                                                  ixProject.ToString()));
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateEdit.Template.Items.Add(itemActionToken);

            CDialogItem itemInvoiceNo =
                   new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sInvoiceNumber"), "{sInvoiceNumber}"),
                                   "InvoiceNumber");
            CDialogItem itemInvoicedate =
               new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sInvoiceDate"), "{sInvoiceDate}"),
                               "InvoiceDate");

            CDialogItem itemAmount =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), "{fAmount}"),
                                "Amount");
            dlgTemplateEdit.Template.Items.Add(itemAmount);

            CDialogItem itemMemo =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), "{sMemo}"),
                                "Memo");
            dlgTemplateEdit.Template.Items.Add(itemMemo);


            /* Standard ok and cancel buttons */
            dlgTemplateEdit.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateEdit;
        }

        #endregion

        protected void InsertItem_POInvoice()
        {
            try
            {
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSPOMatchedInvoice"));
                insertInt(insert, "ixBug");

                if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                {
                    try
                    {
                        insert.InsertFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                    }
                    catch
                    {
                        insert.InsertFloat("fAmount", 0d);
                    }
                }
                insert.InsertInt("ixLineItemId_Inv", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItemId_Inv")]));
                insert.InsertString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                insert.InsertString("sInvoiceNumber", api.Request[api.AddPluginPrefix("sInvoiceNumber")].ToString());
                //  api.Notifications.AddAdminNotification("sInvoiceNumber", api.Request[api.AddPluginPrefix("sInvoiceNumber")].ToString());
                insert.InsertString("sInvoiceDate", api.Request[api.AddPluginPrefix("sInvoiceDate")].ToString());
                //  api.Notifications.AddAdminNotification("sInvoiceDate", api.Request[api.AddPluginPrefix("sInvoiceDate")].ToString());
                insert.InsertString("ixProject", api.Request[api.AddPluginPrefix("ixProject")].ToString());
             //   api.Notifications.AddAdminNotification("sInvoiceDate", api.Request[api.AddPluginPrefix("ixProject")].ToString());
                insert.Execute();

                CSelectQuery InvlinesumAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSPOMatchedInvoice"));
                InvlinesumAmt.AddSelect("SUM(fAmount) as InvSumAcc");
                InvlinesumAmt.AddWhere("ixBug = " + api.Request[api.AddPluginPrefix("ixBug")].ToString());
                InvlinesumAmt.AddWhere("ixLineItemId_Inv = " + Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItemId_Inv")]));

              //  api.Notifications.AddAdminNotification("CGSPOMatchedInvoice", "1");
                DataSet TotalInvLineAmt = InvlinesumAmt.GetDataSet();
                double InvLineAmount = 0d;
                double LineAmount = 0d;
                double SubtotalAmt = 0d;
              //  api.Notifications.AddAdminNotification("CGSPOMatchedInvoice2", "2");
                if (null != TotalInvLineAmt.Tables && TotalInvLineAmt.Tables.Count == 1 && TotalInvLineAmt.Tables[0].Rows.Count == 1)
                {
                   // api.Notifications.AddAdminNotification("CGSPOMatchedInvoice3", "3");
                    InvLineAmount = Convert.ToDouble(TotalInvLineAmt.Tables[0].Rows[0]["InvSumAcc"].ToString());
                    //api.Notifications.AddAdminNotification("InvLineAmount", InvLineAmount.ToString());
                }

                CSelectQuery linesumAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
                linesumAmt.AddSelect("SUM(sExtra3) as LineSumAmt");
                linesumAmt.AddWhere("ixBug = " + api.Request[api.AddPluginPrefix("ixBug")].ToString());
                linesumAmt.AddWhere("ixLineItemId = " + Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItemId_Inv")]));
                DataSet TotalLineAmt = linesumAmt.GetDataSet();

              //  api.Notifications.AddAdminNotification("CGSPOMatchedInvoice5", "5");
                if (null != TotalLineAmt.Tables && TotalLineAmt.Tables.Count == 1 && TotalLineAmt.Tables[0].Rows.Count == 1)
                {
                   // api.Notifications.AddAdminNotification("CGSPOMatchedInvoice6", "6");
                    LineAmount = Convert.ToDouble(TotalLineAmt.Tables[0].Rows[0]["LineSumAmt"].ToString());
                 //   api.Notifications.AddAdminNotification("LineAmount", LineAmount.ToString());
                }

                SubtotalAmt = (LineAmount) - (InvLineAmount);

                CUpdateQuery update = api.Database.NewUpdateQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoiceItems_MLA"));
                update.UpdateFloat("IvnBalanceAmt", InvLineAmount);
                update.UpdateFloat("fLineBalanceAmt", SubtotalAmt);
                update.AddWhere("ixLineItemId = " + Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItemId_Inv")]));
                update.AddWhere("ixBug =" + api.Request[api.AddPluginPrefix("ixBug")].ToString() + " AND iDeleted = 0");
                update.Execute();
                api.Notifications.AddAdminNotification("CGSPOMatchedInvoice4", "4");
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "");
            }
        }

        protected void UpdateItem_POInvoice()
        {
            try
            {
                CUpdateQuery update =
                    api.Database.NewUpdateQuery(api.Database.PluginTableName("CGSPOMatchedInvoice"));

                UpdateInt(update, "sInvoiceNumber");
                UpdateInt(update, "sInvoiceDate");
                //UpdateInt(update, "iForm99");
                if (api.Request[api.AddPluginPrefix("fAmount")] != null)
                {
                    try
                    {
                        update.UpdateFloat("fAmount", Convert.ToDouble(api.Request[api.AddPluginPrefix("fAmount")]));
                    }
                    catch
                    {
                        update.UpdateFloat("fAmount", 0d);
                    }
                }
                update.UpdateString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                update.AddWhere("ixBugLineItem_PMI = @ixBugInvoiceItem_PMI");
                update.SetParamInt("@ixBugInvoiceItem_PMI", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBugLineItem_PMI")]));
                update.Execute();
            }
            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "UpdateItem()");
            }
        }

        protected void DeleteItem_POInvoice()
        {
            CUpdateQuery delete_POI =
                api.Database.NewUpdateQuery(api.Database.PluginTableName("CGSPOMatchedInvoice"));
            delete_POI.UpdateInt("iDeleted", 1);
            delete_POI.AddWhere("ixBugLineItem_PMI = @ixBugInvoiceItem_PMI");
            delete_POI.SetParamInt("@ixBugInvoiceItem_PMI", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBugLineItem_PMI")]));
            delete_POI.Execute();
        }

        protected CDialogTemplate DialogTemplateDelete_POI(string sTableId, int ixProject)
        {
            CDialogTemplate dlgTemplateDelete = new CDialogTemplate();
            dlgTemplateDelete.Template = new CSingleColumnDialog();
            dlgTemplateDelete.Template.sTitle = "Delete Item ";
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBugLineItem_PMI"), "{ixBugLineItem_PMI}"));
            CDialogItem itemDeleteHiddenUrl = CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateDelete.Template.Items.Add(itemDeleteHiddenUrl);
            CDialogItem itemDeleteHiddenAction = CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "delete_POInvoice");
            dlgTemplateDelete.Template.Items.Add(itemDeleteHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken = CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateDelete.Template.Items.Add(itemActionToken);
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), "{ixBug}"));
            dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixProject"), ixProject.ToString()));
            /* DialogItems don't have to be form elements, they can also be simple html */
            dlgTemplateDelete.Template.Items.Add(new CDialogItem("Do you want to delete this item ?"));
            dlgTemplateDelete.Template.Items.Add(new CDialogItem("    "));

            /* Standard ok and cancel buttons */
            dlgTemplateDelete.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateDelete;
        }

        protected void FetchLineItemAmt()
        {
            int ixBug = 0;
            ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

            CSelectQuery sqlLineItemAmt;
            sqlLineItemAmt = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems_MLA"));
            sqlLineItemAmt.AddSelect("*");
            sqlLineItemAmt.AddWhere("ixBug =" + ixBug.ToString() + " AND iDeleted = 0" + "ixLineItemId");

            DataSet dsLineItemsDetails = sqlLineItemAmt.GetDataSet();
        }
    }
}