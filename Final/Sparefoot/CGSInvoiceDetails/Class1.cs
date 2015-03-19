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
using System;
using System.Web;
namespace Consero.Plugins.CGSInvoiceDetails
{
    public class Act : Plugin, IPluginBugJoin,
        IPluginBugDisplay, IPluginBugCommit, IPluginDatabase, IPluginRawPageDisplay, IPluginGridColumn, IPluginJS
    {

        protected const string PLUGIN_ID =
           "CGSInvoiceDetails@conseroglobal.com";

        /* A constant for populating the "code name" input field for multiple case edit */
        protected const string VARIOUS_TEXT = "[various]";
        private string sPrefixedTableName;

        ///* our plugin's custom bug field */
        //protected const string str_CaseID_DisplayName = "Case ID";
        //protected const string str_CaseIDFieldName = "sCaseID";
        //protected const string str_option1_DisplayName = "Header Only";
        //protected const string str_Option1_Name = "sHeaderOnly";
        //protected const string str_Option2_DisplayName = "Line Items Only";
        //protected const string str_Option2_Name = "sLineItemsOnly";
        //protected const string str_Option3_DisplayName = "Both";
        //protected const string str_Option3_Name = "sBoth";

        string sAccount_P = "";
        string sTaxtype_P = "";
        string sDepartment_P = "";
        string sBillable_P = "";
   
        

        /* Constructor: We'll just initialize the inherited Plugin class, which 
         * takes the passed instance of CPluginApi and sets its "api" member variable. */
        public Act(CPluginApi api)
            : base(api)
        {
            sPrefixedTableName = api.Database.PluginTableName("CGSInvoice");
        }

        #region IPluginBugJoin Members

        public string[] BugJoinTables()
        {
            /* All tables specified here must have an integer ixBug column so FogBugz can
            * perform the necessary join. */

            return new string[] { "CGSInvoice" };
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

            // CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem("caseid");
            // DialogItem.sLabel = str_CaseID_DisplayName;
            // /* you do not need to htmlencode text used as sInputValue in a Forms.TextInput
            //  * because it is done for you */
            // DialogItem.sContent = Forms.TextInput(api.PluginPrefix + str_CaseIDFieldName, GetSecretCodeText(rgbug));

            // CBugDisplayDialogItem DialogItem1 = new CBugDisplayDialogItem("Headeronly");
            // DialogItem1.sLabel = str_option1_DisplayName;
            // DialogItem1.sContent = Forms.RadioInput(api.PluginPrefix + str_Option1_Name, GetSecretCodeText(rgbug),false);

            // CBugDisplayDialogItem DialogItem2 = new CBugDisplayDialogItem("LineItemOnly");
            // DialogItem2.sLabel = str_Option2_DisplayName;
            // DialogItem2.sContent = Forms.RadioInput(api.PluginPrefix + str_Option2_Name, GetSecretCodeText(rgbug), false);

            // CBugDisplayDialogItem DialogItem3 = new CBugDisplayDialogItem("LineItemOnly");
            // DialogItem3.sLabel = str_Option3_DisplayName;
            // DialogItem3.sContent = Forms.RadioInput(api.PluginPrefix + str_Option3_Name, GetSecretCodeText(rgbug), false);
            
            // /* specify the url of a 16x16 pixel icon shown when listed in the "Add Fields" area */
            // //DialogItem.sIconURL = api.Url.PluginStaticFileUrl("shh_icon.gif");

            // return new CBugDisplayDialogItem[] { DialogItem, DialogItem1, DialogItem2, DialogItem3 }; 

            ///* don't show multi-edits the invoice details */
            //if (rgbug.Length != 1)
            //    return null;

            //CBug bug = rgbug[0];

            // CProject project = api.Project.GetProject(bug.ixProject);
            //string enableCGSWorkflowSettings = Convert.ToString(project.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            //if (string.IsNullOrEmpty(enableCGSWorkflowSettings) || "0".Equals(enableCGSWorkflowSettings))
            //{
            //    //don't do any intacct calls
            //    return null;
            //}

            //if (nMode == BugEditMode.Edit)
            //{
            //    return new CBugDisplayDialogItem[] 
            //    {
            //        CreateTextInputField(rgbug, "CaseID", "CaseID","CaseID"),
            //        new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
            //    };
            //}
            //return null;        
            
        }
        
        public CBugDisplayDialogItem[] BugDisplayEditTop(CBug[] rgbug,
            BugEditMode nMode, bool fPublic)
        {
            /* don't show non-logged in users the invoice details */
            if (fPublic)
                return null;

            /* don't show multi-edits the invoice details */
            if (rgbug.Length != 1)
                return null;

            CBug bug = rgbug[0];

            //if (bug.ixCategory != 3)
            // {
            //     return null;
            //   }

         

            CProject project = api.Project.GetProject(bug.ixProject);
            string enableCGSWorkflowSettings = Convert.ToString(project.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            if (string.IsNullOrEmpty(enableCGSWorkflowSettings) || "0".Equals(enableCGSWorkflowSettings))
            {
                //don't do any intacct calls
                return null;
            }

            //  if (nMode == BugEditMode.Edit && rgbug[0].ixStatus == 20)
            
            if (nMode == BugEditMode.Edit)
            {

                if (bug.ixProject == 9)
                {
                    return new CBugDisplayDialogItem[] 
                   {                                             

                         // CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                         // CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                         // CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),

                          CreateListField(rgbug, "CustomForm", "Custom Form", "CWFCustomform", "CWFCustomform", true),
                          CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateListField(rgbug, "Country", "Country", "CWFCountry", "CWFCountry", true),
                         // CreateListField(rgbug, "Currency", "Currency", "CWFCurrency", "CWFCurrency", true),
                        //  CreateListField(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", "CWFPostingperiod", true),
                         // CreateListField(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", "CWFSubsidiary", true),

                         CreateTextInputField(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod"),
                          CreateListField(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                      
                          CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                          CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateDateInputField(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                         // CreateTextInputField(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                          CreateTextInputField(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                          //CreateTextInputField(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                          CreateDateInputField(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                          CreateTextInputField(rgbug, "Memo", "Memo", "sMemo"),

                          

                         // CreateListField(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", "CWFCustomVal1", true),
                         // CreateListField(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", "CWFCustomVal2", true),
                         // CreateListField(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", "CWFCustomVal3", true),

                         // CreateTextInputField(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                         // CreateTextInputField(rgbug, "Remarks", "Remarks", "Remarks"),
                   
                       new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   };
                }
                 

                else if(bug.ixProject == 8)
                
                {
                    return new CBugDisplayDialogItem[] 
                   { 
                                             

                         // CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                          //CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                          //CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),

                          CreateListField(rgbug, "CustomForm", "Custom Form", "CWFCustomform", "CWFCustomform", true),
                          CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                          CreateListField(rgbug, "Country", "Country", "CWFCountry", "CWFCountry", true),
                          CreateListField(rgbug, "Currency", "Currency", "CWFCurrency", "CWFCurrency", true),
                          CreateListField(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", "CWFPostingperiod", true),
                          CreateListField(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", "CWFSubsidiary", true),
                          CreateListField(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                      
                          CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                          CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateDateInputField(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                          CreateTextInputField(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                          CreateTextInputField(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                         // CreateTextInputField(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                          CreateDateInputField(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                          CreateTextInputField(rgbug, "Memo", "Memo", "sMemo"),

                          

                          CreateListField(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", "CWFCustomVal1", true),
                       //   CreateListField(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", "CWFCustomVal2", true),
                        //  CreateListField(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", "CWFCustomVal3", true),

                        //  CreateTextInputField(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                        //  CreateTextInputField(rgbug, "Remarks", "Remarks", "Remarks"),
                   
                       new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   };
                }

                else {
                    return null;
                //return new CBugDisplayDialogItem[] 
                //   { 
                                             

                //        //  CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                //       //   CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                //         // CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),

                //          CreateListField(rgbug, "CustomForm", "Custom Form", "CWFCustomform", "CWFCustomform", true),
                //          CreateListField1(rgbug, "Vendor", "Vendor Name", "CWFVendor", "CWFVendor", true),
                //          CreateListField(rgbug, "Country", "Country", "CWFCountry", "CWFCountry", true),
                //          CreateListField(rgbug, "Currency", "Currency", "CWFCurrency", "CWFCurrency", true),
                //          CreateListField(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", "CWFPostingperiod", true),
                //          CreateListField(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", "CWFSubsidiary", true),
                //          CreateListField(rgbug, "Terms", "Terms", "CWFTerms", "CWFTerms", true),
                      
                //          CreateTextInputField(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                //          CreateDateInputField(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                //          CreateDateInputField(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                //          CreateTextInputField(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                //          CreateTextInputField(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                //          CreateTextInputField(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                //          CreateDateInputField(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                //          CreateTextInputField(rgbug, "Memo", "Memo", "sMemo"),

                          

                //          CreateListField(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", "CWFCustomVal1", true),
                //          CreateListField(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", "CWFCustomVal2", true),
                //          CreateListField(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", "CWFCustomVal3", true),

                //          CreateTextInputField(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                //          CreateTextInputField(rgbug, "Remarks", "Remarks", "Remarks"),
                   
                //       new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                //   };

               }
            }

            if (nMode == BugEditMode.Resolve)



                if (bug.ixProject == 9)
                {

                    return new CBugDisplayDialogItem[] 
                   {
           

        //    CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
         //   CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
         //   CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),

            CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
            CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
            CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
           // CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
          //  CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
             CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod"),
           // CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
            CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

                         CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                        //  CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                          CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                        //  CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                          CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "Memo", "Memo", "sMemo"),

                      

                         // CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
                         // CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
                         // CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                       
                         // CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                        //  CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

             new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };


                }

                else if (bug.ixProject == 8)
                {

                    return new CBugDisplayDialogItem[] 
                   {
          //  CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
          //  CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
           // CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
            CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
            CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
            CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
            CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
            CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
            CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

                         CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                          CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                          CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                        //  CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                          CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "Memo", "Memo", "sMemo"),

                      

                          CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
                         // CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
                       //   CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                       
                         // CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                        //  CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

             new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };
                }

                else
                {
                    return null;
          //          return new CBugDisplayDialogItem[] 
          //         {
          ////  CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
          ////  CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
          ////  CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
          //  CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
          //  CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
          //  CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
          //  CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
          //  CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
          //  CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
          //  CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

          //               CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

          //                CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
          //                CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
          //                CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
          //                CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
          //                CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

          //                CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

          //                CreateText(rgbug, "Memo", "Memo", "sMemo"),

                      

          //                CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
          //                CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
          //                CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                       
          //                CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
          //                CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

          //   new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
          //         };
                }



            if (bug.ixProject == 9)
            {
                return new CBugDisplayDialogItem[] {
         //  CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
         // CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
         // CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
            CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
            CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
        //    CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
           // CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
           CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod"),
          //  CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
            CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

                         CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                       //   CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                          CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                        //  CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                          CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "Memo", "Memo", "sMemo"),

                        //  CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                   
                         // CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
                         // CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
                          //CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                   
                         // CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                         // CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

             new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };

            }
            else if (bug.ixProject == 8)
            {

                return new CBugDisplayDialogItem[] {
         //   CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
         //   CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
          //  CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
            CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
            CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
            CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
            CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
            CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
            CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

                         CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                          CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                          CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                        //  CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                          CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "Memo", "Memo", "sMemo"),

                        //  CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                   
                          CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
                        //  CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
                        //  CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                   
                     //     CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                       //   CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

             new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
                   };

            }
            else
            {
                return null;

         //       return new CBugDisplayDialogItem[] {
         ////   CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
         ////   CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
         ////   CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
         //   CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
         //   CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
         //   CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
         //   CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
         //   CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
         //   CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
         //   CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

         //                CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

         //                 CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
         //                 CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
         //                 CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
         //                 CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
         //                 CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

         //                 CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

         //                 CreateText(rgbug, "Memo", "Memo", "sMemo"),

         //                 CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                   
         //                 CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
         //                 CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
         //                 CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                   
         //                 CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
         //                 CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

         //    new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)      
         //          };
            }

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
            //DialogItem.sContent = GetSelects(dropDownName, rgbug[0].ixProject);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateListField1(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
           DialogItem.sContent = GetSelects1(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            //DialogItem.sContent = GetSelects1(dropDownName, rgbug[0].ixProject);
            return DialogItem;
        }
     

        public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
        {
            /* don't show non-logged in users the secret code name */
            //if (fPublic)
            //    return null;

            ///* If GetPluginField returns null or "", don't show anything
            // * in display mode. Note: Convert.ToString() returns "" when passed a NULL object,
            // * so this one line handles both cases. */

            //string sCodeName = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, str_CaseIDFieldName));
            //if (sCodeName.Length == 0)
            //    return null;

            //CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem("secretcodename");
            //DialogItem.sLabel = str_CaseID_DisplayName;
            //DialogItem.sContent = HttpUtility.HtmlEncode(sCodeName);
            //return new CBugDisplayDialogItem[] { DialogItem };

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


           //if (rgbug[0].ixProject != 8)

           //   return null;


           // api.Notifications.AddAdminNotification("calling CBugDisplayDialogItem","");

           CProject project1 = api.Project.GetProject(rgbug[0].ixProject);
           string enabledCGSWorkflowSettings = Convert.ToString(project1.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
           if (string.IsNullOrEmpty(enabledCGSWorkflowSettings) || "0".Equals(enabledCGSWorkflowSettings))
           {
               //don't do any intacct calls
               return null;
           }

           //sProj = project1.sProject;

           //project = project1.ixProject;

          // api.Notifications.AddAdminNotification("New Project id = " + project.ToString(), "");



           return new CBugDisplayDialogItem[] {
                new CBugDisplayDialogItem("Copy Case Details", EditableTable(rgbug[0].ixBug).RenderHtml()),
                               
                };
            
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

             
            CProject project = api.Project.GetProject(bug.ixProject);
            string enableCGSWorkflowSettings = Convert.ToString(project.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            if (string.IsNullOrEmpty(enableCGSWorkflowSettings) || "0".Equals(enableCGSWorkflowSettings))
            {
                //don't do any intacct calls
                return null;
            }

            if (bug.ixProject == 9)
            {

                return new CBugDisplayDialogItem[] 
                   {
        //    CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
         //   CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
         //   CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
            CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
            CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
           // CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
            //CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
            CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod"),
            //CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
            CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

                         CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                        //  CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                          CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                        //  CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                          CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "Memo", "Memo", "sMemo"),

                       
                   
                        //  CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
                        //  CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
                        //  CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                          
                        //  CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                         // CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

             new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  
                   };

            }

            else if (bug.ixProject == 8)
            {
                return new CBugDisplayDialogItem[] 
                   {
          //  CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
         //   CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
          //  CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
            CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
            CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
            CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
            CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
            CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
            CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

                         CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

                          CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
                          CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
                          CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
                          CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
                         // CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

                          CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

                          CreateText(rgbug, "Memo", "Memo", "sMemo"),

                       
                   
                          CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
                         // CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
                         // CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                          
                          //CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
                          //CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

             new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  
                   };

            }

            else
            {
                return null;
         //       return new CBugDisplayDialogItem[] 
         //          {
         ////   CreateText(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
         ////   CreateText(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
         ////   CreateText(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
         //   CreateText(rgbug, "CustomForm", "Custom Form", "CWFCustomform", true, "CWFCustomform"),
         //   CreateText(rgbug, "Vendor", "Vendor Name", "CWFVendor", true, "CWFVendor"),
         //   CreateText(rgbug, "Country", "Country", "CWFCountry", true, "CWFCountry"),
         //   CreateText(rgbug, "Currency", "Currency", "CWFCurrency", true, "CWFCurrency"),
         //   CreateText(rgbug, "PostingPeriod", "Posting Period", "CWFPostingperiod", true, "CWFPostingperiod"),
         //   CreateText(rgbug, "Subsidiary", "Subsidiary", "CWFSubsidiary", true, "CWFSubsidiary"),
         //   CreateText(rgbug, "Terms", "Terms", "CWFTerms", true, "CWFTerms"),

         //                CreateText(rgbug, "InvoiceNumber", "Invoice Number", "sInvoiceNumber"),

         //                 CreateText(rgbug, "InvoiceDate", "Invoice Date", "sInvoiceDate"),
         //                 CreateText(rgbug, "InvoiceEnteredDate", "Invoice Entered Date", "sInvoiceEnteredDate"),
                          
         //                 CreateText(rgbug, "ExchangeRate", "Exchange Rate", "sExchangeRate"),
                          
         //                 CreateText(rgbug, "InvoiceAmount", "Invoice Amount", "sInvoiceAmount"),
         //                 CreateText(rgbug, "TaxAmount", "Tax Amount (VAT)", "sTaxAmount"),

         //                 CreateText(rgbug, "InvoiceDueDate", "Invoice Due Date", "sInvoiceDueDate"),

         //                 CreateText(rgbug, "Memo", "Memo", "sMemo"),

                       
                   
         //                 CreateText(rgbug, "CustomVal1", "Custom Value1", "CWFCustomVal1", true, "CWFCustomVal1"),
         //                 CreateText(rgbug, "CustomVal2", "Custom Value2", "CWFCustomVal2", true, "CWFCustomVal2"),
         //                 CreateText(rgbug, "CustomVal3", "Custom Value3", "CWFCustomVal3", true, "CWFCustomVal3"),
                          
         //                 CreateText(rgbug, "AddInfo", "Additional Info", "sAddInfo"),
         //                 CreateText(rgbug, "Remarks", "Remarks", "Remarks"),

         //    new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)  
         //          };
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

         //private CBugDisplayDialogItem CreateCheckBoxField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
         //{
         //    CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
         //    DialogItem.sLabel = fielddisplay;
         //    DialogItem.sContent = Forms.CheckboxInput(api.PluginPrefix + fieldName, GetText(rgbug, fieldName), true);
         //    return DialogItem;
         //}

        #endregion

        #region IPluginBugCommit Members
         public void BugCommitAfter(CBug bug, BugAction nBugAction, CBugEvent bugevent,
             bool fPublic)
         {
         }

         public void BugCommitBefore(CBug bug, BugAction nBugAction, CBugEvent bugevent,
             bool fPublic)
         {

             CProject project = api.Project.GetProject(bug.ixProject);
             string enableCGSWorkflowSettings = Convert.ToString(project.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
             if (string.IsNullOrEmpty(enableCGSWorkflowSettings) || "0".Equals(enableCGSWorkflowSettings))
             {
                 //don't do anything
                 return ;
             }


          //   bool bInvoiceAndLineItemsAmountMatches = true;

             //If there is a change and the status is "waiting for gl entry" then post to intacct
            // if (bug.ixStatus == 20 || bug.ixStatus == 26)
             if (bug.ixProject == 9)
             {
                 {
                     ExtractValue(bug, bugevent, "sInvoiceNumber", "Invoice Number");
                    // ExtractValue(bug, bugevent, "CWFApproverl2", "Level-2 Approver");
                    // ExtractValue(bug, bugevent, "CWFApproverl3", "Level-3 Approver");
                   //  ExtractValue(bug, bugevent, "CWFApproverl4", "Level-4 Approver");
                     ExtractValue(bug, bugevent, "CWFCustomform", "Custom Form");
                     ExtractValue(bug, bugevent, "CWFVendor", "Vendor");
                     ExtractValue(bug, bugevent, "CWFCountry", "Country");
                    // ExtractValue(bug, bugevent, "CWFCurrency", "Currency");
                     ExtractValue(bug, bugevent, "CWFPostingperiod", "Posting Period");
                    // ExtractValue(bug, bugevent, "CWFSubsidiary", "Subsidiary");
                     ExtractValue(bug, bugevent, "CWFTerms", "Terms");
                     ExtractValue(bug, bugevent, "sInvoiceDate", "Invoice Date");
                     ExtractValue(bug, bugevent, "sInvoiceEnteredDate", "Invoice Entered Date");
                   //  ExtractValue(bug, bugevent, "sExchangeRate", "Exchange Rate");
                     ExtractValue(bug, bugevent, "sInvoiceAmount", "Invoice Amount");
                    // ExtractValue(bug, bugevent, "sTaxAmount", "Tax Amount");
                     ExtractValue(bug, bugevent, "sInvoiceDueDate", "Invoice DueDate");
                     ExtractValue(bug, bugevent, "sMemo", "sMemo");
                   

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
                        // TaxAmount = Convert.ToDouble(bug.GetPluginField(PLUGIN_ID, "sTaxAmount"));
                     }
                     catch
                     {
                       //  bug.SetPluginField(PLUGIN_ID, "sTaxAmount", "0");
                       //  api.Notifications.AddMessage("Tax Amount value is invalid. Reset to 0.");
                       //  invoiceAmount = 0d;
                     }
                     //duplicate invoice
                     {
                         string mailsub = "", mailbody = "";
                         int iperson = 0;
                         int old_inv_bug = 0;
                         string vendor_1 = "";
                         string InvNo_1 = "";
                         vendor_1 = (bug.GetPluginField(PLUGIN_ID, "CWFVendor")).ToString().Trim();
                         InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();
                       //  api.Notifications.AddMessage(vendor_1.ToString());
                        // api.Notifications.AddMessage(InvNo_1.ToString());
                         vendor_1 = vendor_1.Replace("'", "''");
                         //if (vendor_1.Trim() == "-" || InvNo_1.Trim() == "-")
                         //{
                         //    return;
                         //}
                         if (vendor_1.Trim() != "-")
                         {
                            // api.Notifications.AddMessage("vendor"+vendor_1.ToString());
                             if (InvNo_1.Trim() != string.Empty || InvNo_1.Trim() != "")
                             {
                               //  api.Notifications.AddMessage("InvNo_1" + InvNo_1.ToString());
                                 CSelectQuery Dupcheck2 = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails@conseroglobal.com", "CGSInvoice"));
                                 Dupcheck2.AddSelect("ixbug");
                                 Dupcheck2.AddWhere("CWFVendor = " + "'" + vendor_1 + "'");
                                 Dupcheck2.AddWhere("sInvoiceNumber = " + "'" + InvNo_1 + "'");
                                 Dupcheck2.AddWhere("ixbug > " + bug.ixBug.ToString() + " OR ixbug < " + bug.ixBug.ToString());
                                 DataSet d_1 = Dupcheck2.GetDataSet();

                              //   api.Notifications.AddMessage("d_1" + d_1.ToString());

                                 if (null != d_1.Tables && d_1.Tables.Count == 1 && d_1.Tables[0].Rows.Count > 0)
                                 {
                                     //Vendor_Name = Convert.ToString(d_1.Tables[0].Rows[0]["CWFVendor"]);
                                     // Invoice_no = Convert.ToString(d_1.Tables[0].Rows[0]["sInvoiceNumber"]);
                                     old_inv_bug = Convert.ToInt32(d_1.Tables[0].Rows[0]["ixbug"]);
                                   //  api.Notifications.AddMessage("old_inv_bug" + old_inv_bug.ToString());
                                     this.api.Notifications.AddError("--------------------------------------------------------------------------");
                                     this.api.Notifications.AddError("***DUPLICATE BILL****");
                                     this.api.Notifications.AddMessage("It seems An Invoice is already existing for the same vendor with ( case Id " + old_inv_bug + " )");
                                     this.api.Notifications.AddMessage("Please verify again");
                                     this.api.Notifications.AddError("-------------------------------------------------------------------------");

                                     mailsub = "Duplicate Invoice for Sparefoot in AP Workflow";
                                     mailbody = "It seems same invoice number " + InvNo_1 + " is already existing for the vendor " + vendor_1;
                                     iperson = bug.ixPersonAssignedTo;
                                     mailsender("prema.k@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                     mailsender("syed.k@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                     mailsender("poornima.r@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                     //  i = 1;

                                 }
                             }

                         }
                     }

                     // POsting Period
                                         
                     DateTime InvoiceEDate = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sInvoiceEnteredDate"));
                     //api.Notifications.AddMessage(InvoiceEDate.ToString());
                     string month = (InvoiceEDate.ToString("MMM"));
                    // api.Notifications.AddMessage(month.ToString());
                     string year = (InvoiceEDate.ToString("yyyy"));
                   //  api.Notifications.AddMessage(year.ToString());
                     string monyr1 = month + " " + year;
                    // api.Notifications.AddMessage(monyr1.ToString());
                     bug.SetPluginField(PLUGIN_ID, "CWFPostingperiod", monyr1);
                 }
             }


             if (bug.ixProject == 8)
             {
                 {
                     ExtractValue(bug, bugevent, "sInvoiceNumber", "Invoice Number");
                    // ExtractValue(bug, bugevent, "CWFApproverl2", "Level-2 Approver");
                    // ExtractValue(bug, bugevent, "CWFApproverl3", "Level-3 Approver");
                    // ExtractValue(bug, bugevent, "CWFApproverl4", "Level-4 Approver");
                     ExtractValue(bug, bugevent, "CWFCustomform", "Custom Form");
                     ExtractValue(bug, bugevent, "CWFVendor", "Vendor");
                     ExtractValue(bug, bugevent, "CWFCountry", "Country");
                     ExtractValue(bug, bugevent, "CWFCurrency", "Currency");
                     ExtractValue(bug, bugevent, "CWFPostingperiod", "Posting Period");
                     ExtractValue(bug, bugevent, "CWFSubsidiary", "Subsidiary");
                     ExtractValue(bug, bugevent, "CWFTerms", "Terms");
                     ExtractValue(bug, bugevent, "sInvoiceDate", "Invoice Date");
                     ExtractValue(bug, bugevent, "sInvoiceEnteredDate", "Invoice Entered Date");
                     ExtractValue(bug, bugevent, "sExchangeRate", "Exchange Rate");
                     ExtractValue(bug, bugevent, "sInvoiceAmount", "Invoice Amount");
                   //  ExtractValue(bug, bugevent, "sTaxAmount", "Tax Amount");
                     ExtractValue(bug, bugevent, "sInvoiceDueDate", "Invoice DueDate");
                     ExtractValue(bug, bugevent, "sMemo", "sMemo");
                     //ExtractValue(bug, bugevent, "sAddInfo", "AddInfo");
                     ExtractValue(bug, bugevent, "CWFCustomVal1", "Custom Value1");
                   

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
                       //  TaxAmount = Convert.ToDouble(bug.GetPluginField(PLUGIN_ID, "sTaxAmount"));
                     }
                     catch
                     {
                       //  bug.SetPluginField(PLUGIN_ID, "sTaxAmount", "0");
                        // api.Notifications.AddMessage("Tax Amount value is invalid. Reset to 0.");
                        // invoiceAmount = 0d;
                     }

                 }
             }

             else
             {
                 {
                     ExtractValue(bug, bugevent, "sInvoiceNumber", "Invoice Number");
                   //  ExtractValue(bug, bugevent, "CWFApproverl2", "Level-2 Approver");
                    // ExtractValue(bug, bugevent, "CWFApproverl3", "Level-3 Approver");
                   //  ExtractValue(bug, bugevent, "CWFApproverl4", "Level-4 Approver");
                     ExtractValue(bug, bugevent, "CWFCustomform", "Custom Form");
                     ExtractValue(bug, bugevent, "CWFVendor", "Vendor");
                     ExtractValue(bug, bugevent, "CWFCountry", "Country");
                     ExtractValue(bug, bugevent, "CWFCurrency", "Currency");
                     ExtractValue(bug, bugevent, "CWFPostingperiod", "Posting Period");
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
                     ExtractValue(bug, bugevent, "CWFCustomVal1", "Custom Value1");
                     ExtractValue(bug, bugevent, "CWFCustomVal2", "Custom Value2");
                     ExtractValue(bug, bugevent, "CWFCustomVal3", "Custom Value3");
                     ExtractValue(bug, bugevent, "Remarks", "Remarks");
                     //bool bInvoiceDateChanged = ExtractValue(bug, bugevent, "sInvoiceDate", "Invoice Date");
                     //bool bNetTermsChanged = ExtractValue(bug, bugevent, "ixGlNetTerm", "Net Terms");
                     // ExtractValue(bug, bugevent, "ixGlVendor", "Vendor Name");
                     // ExtractValue(bug, bugevent, "sPONumber", "PO Number");
                     // ExtractValue(bug, bugevent, "sBalanceDue", "Invoice Amount");


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


             


               }

         private bool ExtractValue(CBug bug, CBugEvent bugevent, string fieldName, string fieldDisplay)
         {
             bool valueChanged = false;

             string sNewValue = Convert.ToString(api.Request[api.AddPluginPrefix(fieldName)]);
           
             if (string.IsNullOrEmpty(sNewValue))
            {
             //    api.Notifications.AddMessage(fieldDisplay + " is required.");
                 //bug.ixStatus = 20;
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

             CTable Invoiceheader = api.Database.NewTable(api.Database.PluginTableName("CGSInvoice"));
             Invoiceheader.sDesc = "Caputures Invoice Header Parameters";
             Invoiceheader.AddAutoIncrementPrimaryKey("ixCGSInvoiceNumber");
             Invoiceheader.AddIntColumn("ixBug", true, 1);
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
             Invoiceheader.AddVarcharColumn("CWFCustomVal1", 200, false);
             Invoiceheader.AddVarcharColumn("CWFCustomVal2", 200, false);
             Invoiceheader.AddVarcharColumn("CWFCustomVal3", 200, false);
             Invoiceheader.AddVarcharColumn("Remarks", 200, false);


             CTable Invoiceitems = api.Database.NewTable(api.Database.PluginTableName("CGSInvoiceItems"));
             Invoiceitems.sDesc = "A table for CGSWF LineItems";
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
             Invoiceitems.AddTextColumn("sExtra2", "extra");
             Invoiceitems.AddFloatColumn("sExtra3", false);
             Invoiceitems.AddTextColumn("sExtra4", "extra4");
             Invoiceitems.AddTextColumn("sExtra5", "extra5");
             Invoiceitems.AddTextColumn("sExtra6", "extra6");
             return new CTable[] { Invoiceheader, Invoiceitems };
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
             gridCol1.sName = ".Invoice Number";
             /* the column title in grid view */
             gridCol1.sTitle = ".Invoice Number";
             /* every column you create needs to have a unique iType */
             gridCol1.iType = 0;

             CGridColumn gridCol2 = api.Grid.CreateGridColumn();
             gridCol2.sName = ".Vendor Name";
             gridCol2.sTitle = ".Vendor Name";
             /* every column you create needs to have a unique iType */
             gridCol2.iType = 1;

             CGridColumn gridCol3 = api.Grid.CreateGridColumn();
             gridCol3.sName = ".Invoice Amount";
             gridCol3.sTitle = ".Invoice Amount";
             /* every column you create needs to have a unique iType */
             gridCol3.iType = 2;

             CGridColumn gridCol4 = api.Grid.CreateGridColumn();
             gridCol4.sName = ".Invoice Date";
             gridCol4.sTitle = ".Invoice Date";
             /* every column you create needs to have a unique iType */
             gridCol4.iType = 3;

             CGridColumn gridCol5 = api.Grid.CreateGridColumn();
             gridCol5.sName = ".Invoice Due Date";
             gridCol5.sTitle = ".Invoice Due Date";
             /* every column you create needs to have a unique iType */
             gridCol5.iType = 4;

             return new CGridColumn[] { gridCol1, gridCol2, gridCol3, gridCol4, gridCol5 };
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
                     sTableColumn = "sInvoiceAmount";
                     break;
                 case 3:
                     sTableColumn = "sInvoiceDate";
                     break;
                 case 4:
                     sTableColumn = "sInvoiceDueDate";
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
                     sTableColumn = "sInvoiceAmount";
                     break;
                 case 3:
                     sTableColumn = "sInvoiceDate";
                     break;
                 case 4:
                     sTableColumn = "sInvoiceDueDate";
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
                 CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems"));


                 insertInt(insert, "ixBug");
                 //insert.InsertInt("ixBug", 1705);

                // insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("sCWFAccountId")].ToString());
                 insert.InsertString("sAccount", api.Request[api.AddPluginPrefix("CWFAccount")].ToString());
                 //insert.InsertString("sAccount", "2001");

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
                 insert.InsertString("sBillable", api.Request[api.AddPluginPrefix("CWFDept")].ToString());
                 //insert.InsertString("sBillable", "Yes");

                 insert.InsertString("sAddninfo", api.Request[api.AddPluginPrefix("sAddninfo")].ToString());
               
                 //insert.InsertString("sAddninfo", "Caode additional info");


                 insert.InsertString("sExtra2", api.Request[api.AddPluginPrefix("sExtra2")].ToString());
                 insert.InsertString("sExtra4", api.Request[api.AddPluginPrefix("sExtra4")].ToString());
                 insert.InsertString("sExtra5", api.Request[api.AddPluginPrefix("sExtra5")].ToString());
                 insert.InsertString("sExtra6", api.Request[api.AddPluginPrefix("sExtra6")].ToString());


                 /*
                  insertInt(insert, "ixGlDepartment");
                  insertInt(insert, "ixGlLocation");
                  insertInt(insert, "ixGlProject");
                  insertInt(insert, "ixGlItem");
                  insertInt(insert, "ixGlClass");
                  */

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
                 api.Database.PluginTableName("CGSInvoiceItems"));

             /*  LeftJoinTable(select, "GlAccount");
               LeftJoinTable(select, "GlDepartment");
               LeftJoinTable(select, "GlLocation");
               LeftJoinTable(select, "GlProject");
               LeftJoinTable(select, "GlClass");
               LeftJoinTable(select, "GlItem");
               */
             select.AddSelect("*");
             string sWhere = api.Database.PluginTableName("CGSInvoiceItems") + ".ixBug = " + ixBug.ToString();

             if (bExcludeDeleted)
             {
                 sWhere += " and iDeleted = 0";
             }


             select.AddWhere(sWhere);

             return select.GetDataSet();
         }


         private void LeftJoinTable(CSelectQuery select, string sType)
         {


             string projectPluginId = "CGSInvoiceDetails@conseroglobal.com";
             select.AddLeftJoin(api.Database.PluginTableName(projectPluginId, sType),
                api.Database.PluginTableName("CGSInvoiceItems") + ".ix" + sType + " = " +
                api.Database.PluginTableName(projectPluginId, sType) + ".ix" + sType);
         }

         protected void UpdateItem()
         {

           


             try
             {
                 CUpdateQuery update =
                     api.Database.NewUpdateQuery(api.Database.PluginTableName("CGSInvoiceItems"));

                 /* UpdateInt(update, "ixGlAccount");
                  UpdateInt(update, "ixGlDepartment");
                  UpdateInt(update, "ixGlLocation");
                  UpdateInt(update, "ixGlProject");
                  UpdateInt(update, "ixGlClass");
                  UpdateInt(update, "ixGlItem");
                  */
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


                 //commented as it was redusing system response time

                 update.UpdateString("sAccount", api.Request[api.AddPluginPrefix("sAccount")].ToString());
                 update.UpdateString("sTaxtype", api.Request[api.AddPluginPrefix("sTaxtype")].ToString());
                 update.UpdateString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                 update.UpdateString("sDepartment", api.Request[api.AddPluginPrefix("sDepartment")].ToString());
                 update.UpdateString("sBillable", api.Request[api.AddPluginPrefix("sBillable")].ToString());
                 update.UpdateString("sAddninfo", api.Request[api.AddPluginPrefix("sAddninfo")].ToString());
                 update.UpdateString("sExtra2", api.Request[api.AddPluginPrefix("sExtra2")].ToString());
                 update.UpdateString("sExtra4", api.Request[api.AddPluginPrefix("sExtra4")].ToString());
                 update.UpdateString("sExtra5", api.Request[api.AddPluginPrefix("sExtra5")].ToString());
                 update.UpdateString("sExtra6", api.Request[api.AddPluginPrefix("sExtra6")].ToString());

                // api.Notifications.AddAdminNotification((api.Request[api.AddPluginPrefix("ixBugLineItem")]), "ixBugLineItem");

                 update.AddWhere("ixBugLineItem = @ixBugLineItem");
                // update.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBugLineItem")]));
                 update.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixLineItem")]));
                 update.Execute();

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

             CUpdateQuery delete =
                 api.Database.NewUpdateQuery(api.Database.PluginTableName("CGSInvoiceItems"));
             delete.UpdateInt("iDeleted", 1);
             delete.AddWhere("ixBugLineItem = @ixBugLineItem");
             delete.SetParamInt("@ixBugLineItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBugLineItem")]));
             delete.Execute();
         }

         protected void EditLineItems()
         {
         }

         protected void CopyCase()
         {
             int ixBug = 0;
             int i_OldCaseID = 0;
             bool bHeaderCopy = false;
             bool bLineItemsCopy = false;
             string sProj = "";


             // api.Notifications.AddAdminNotification("Raw Page display called", "");
             ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());

             CBug bug = api.Bug.GetBug(ixBug);
             bug.IgnorePermissions = true;
             int iproj = bug.ixProject;

             //Boolean b_IsCaseOpen = bug.fOpen;
             //if (b_IsCaseOpen != true)
             //{
             //    api.Notifications.AddAdminNotification("This case is not open, Unable to update case details","Case not open");
             //    return;
             //}
             {
                 try
                 {

                     if (((api.Request[api.AddPluginPrefix("CaseID")].ToString().Trim()) == null) || ((api.Request[api.AddPluginPrefix("CaseID")].ToString().Trim()) == ""))
                     {
                         api.Notifications.AddMessage("Case ID is Empty");
                     }
                     else
                     {
                         i_OldCaseID = Int32.Parse(api.Request[api.AddPluginPrefix("CaseID")].ToString());

                     }
                     //api.Notifications.AddAdminNotification("checkbox processing starts here", "");

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

                     sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems"));
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

                     sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoice"));
                     sqlInvoiceDetails.AddSelect("*");
                     sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString());

                     DataSet dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();

                     if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                     {

                         api.Notifications.AddMessage("This Case ID is not valid");

                         return;
                     }

                     sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems"));
                     sqlInvoiceDetails.AddSelect("*");
                     sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString() + " AND iDeleted = 0");
                     DataSet dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                   //  api.Notifications.AddAdminNotification("Record count = " + dsLineItemsDetails.Tables[0].Rows.Count.ToString(), "");

                     // DataSet dsLineItemsDetails = FetchItems(i_OldCaseID, true);
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

                         string tablename = api.Database.PluginTableName("CGSInvoice");
                         CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

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

                         if (dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceAmount"] != null)
                         {
                             try
                             {
                                 Update1.UpdateFloat("sInvoiceAmount", Convert.ToDouble(dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceAmount"].ToString()));
                             }
                             catch
                             {
                                 Update1.UpdateFloat("sInvoiceAmount", 0d);
                             }
                         }


                         if (dsOldCaseDetails.Tables[0].Rows[0]["sTaxAmount"] != null)
                         {
                             try
                             {
                                 Update1.UpdateFloat("sTaxAmount", Convert.ToDouble(dsOldCaseDetails.Tables[0].Rows[0]["sTaxAmount"].ToString()));
                             }
                             catch
                             {
                                 Update1.UpdateFloat("sTaxAmount", 0d);
                             }
                         }

                         Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                         Update1.UpdateString("sAddInfo", dsOldCaseDetails.Tables[0].Rows[0]["sAddInfo"].ToString());
                         Update1.UpdateString("CWFCustomVal1", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal1"].ToString());
                         Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                         Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                         Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());


                         Update1.AddWhere("ixBug = @ixBug");
                         Update1.SetParamInt("@ixBug", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")]));

                         Update1.Execute();

                     }
                     if (bLineItemsCopy == true)
                     {

                         //sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems"));
                         //sqlInvoiceDetails.AddSelect("*");
                         //sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString());
                         //dsLineItemsDetails = sqlInvoiceDetails.GetDataSet();

                         if ((dsLineItemsDetails != null) && (dsLineItemsDetails.Tables[0].Rows.Count > 0))
                         {

                             foreach (DataRow dr in dsLineItemsDetails.Tables[0].Rows)
                             {
                                 CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CGSInvoiceItems"));
                                 insert1.InsertInt("ixBug", ixBug);
                                 insert1.InsertString("sAccount", dr["sAccount"].ToString());
                                 insert1.InsertString("sTaxtype", dr["sTaxtype"].ToString());

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

                                 //insert1.InsertFloat("fTax", float.Parse(dr["fTax"].ToString()));
                                 insert1.InsertString("sMemo", dr["sMemo"].ToString());
                                 insert1.InsertString("sDepartment", dr["sDepartment"].ToString());
                                 insert1.InsertString("sBillable", dr["sBillable"].ToString());
                                 insert1.InsertString("sAddninfo", dr["sAddninfo"].ToString());

                                 //api.Notifications.AddAdminNotification(dr["iDeleted"].ToString(), "");
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
                                 //api.Notifications.AddAdminNotification("10", "");
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
                                 // insert1.InsertInt("ixExtra1", Int32.Parse(dr["ixExtra1"].ToString()));
                                 //api.Notifications.AddAdminNotification("11", "");
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
                                 //insert1.InsertFloat("sExtra3", float.Parse(dr["sExtra3"].ToString()));
                                 insert1.InsertString("sExtra4", dr["sExtra4"].ToString());
                                 insert1.InsertString("sExtra5", dr["sExtra5"].ToString());
                                 insert1.InsertString("sExtra6", dr["sExtra6"].ToString());
                                 //api.Notifications.AddAdminNotification("13", "");
                                 insert1.Execute();
                                 //api.Notifications.AddAdminNotification("14", "");
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

             if ((api.Request[api.AddPluginPrefix("sAction")] != null) &&
                 (api.Request[api.AddPluginPrefix("actionToken")] != null) &&
                 api.Security.ValidateActionToken(api.Request[api.AddPluginPrefix("actionToken")]))
             {
                 //api.Notifications.AddAdminNotification("reached to this point", "");
                 api.Notifications.AddAdminNotification("p1", "p1");
                 int ixLineItem = -1; // added for Js edit popup option
                 switch (api.Request[api.AddPluginPrefix("sAction")].ToString())
                 {
                         
                     case "new":

                         InsertItem();

                         break;
                     case "edit":
                        
                         UpdateItem();
                         break;
                     case "delete":

                         DeleteItem();

                         break;
                     case "copycase":
                         CopyCase();
                         break;
                         
                     case "geteditdialog":
                      
                         if ((api.Request[api.AddPluginPrefix("ixLineItem")] != null) &&
                             (Int32.TryParse(api.Request[api.AddPluginPrefix("ixLineItem")].ToString(), out ixLineItem)) &&
                             (ixLineItem > 0))
                         {
                             string sTableId1 = api.Request[api.AddPluginPrefix("sTableId")].ToString();
                            string ixProject = api.Request[api.AddPluginPrefix("ixProject")].ToString();

                            api.Notifications.AddAdminNotification("proeject value", ixProject.ToString());

                           //  string ixProject = "8";
                             return GetEditDialog(ixLineItem, sTableId1, ixProject);
                         }
                         // error?
                         break;
                 }                 
             }
            
             /* return the updated table as xml so FogBugz can update the page */
             api.Response.ContentType = "text/xml";
             if (api.Request[api.AddPluginPrefix("sAction")].ToString() != "copycase")
             {
                 return ItemTable(Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString()),
                     Convert.ToInt32(api.Request[api.AddPluginPrefix("ixProject")].ToString()), false).RenderXml();
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

         public string GetEditDialog(int ixLineItem, string sTableId,string ixProject)
         {

             return DialogEditForAjax(ixLineItem, sTableId, ixProject).RenderHtml();
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
             CEditableTable editableTableItems = new CEditableTable("itemtable");
             //string sTableId = editableTableItems.sId;

             sTableId = editableTableItems.sId;


             string str1 = "GL Account", str2 = "Amount", str3 = "Tax type", str4 = "Tax",
                 str5 = "Memo", str6 = "Department", str7 = "Billable", str8 = "Addninfo",
                 str9 = "Addninfo1", str10 = "Addninfo2", str11 = "Addninfo3", str12 = "Addninfo4";

             if (ixProject == 8 || ixProject == 9)
             {
                 str1 = "GL Account";
                 str2 = "Rate";
                 str3 = "Item";
                 str4 = "Quantity";
                 str5 = "Memo";
                 str6 = "Department";
                 str7 = "Class";
                 str8 = "Amount";
                 str9 = "Billable";
                 str10 = "Amort Schedule";
                 str11 = "Start Date";
                 str12 = "End Date";
             }

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
             editableTableItems.Header.AddCell(str9);
             editableTableItems.Header.AddCell(str10);
             editableTableItems.Header.AddCell(str11);
             editableTableItems.Header.AddCell(str12);


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
             string sExtra2 = "";
             string sExtra4 = "";
             string sExtra5 = "";
             string sExtra6 = "";

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

                     sTaxtype = Convert.ToString(dsItems.Tables[0].Rows[i]["sTaxtype"]);
                     sDepartment = Convert.ToString(dsItems.Tables[0].Rows[i]["sDepartment"]);
                     // sDepartment = Convert.ToString(dsItems.Tables[0].Rows[i]["sDepartment"]);
                     sBillable = Convert.ToString(dsItems.Tables[0].Rows[i]["sBillable"]);
                     sAddninfo = Convert.ToString(dsItems.Tables[0].Rows[i]["sAddninfo"]);
                     sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                     sExtra2 = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra2"]);
                     sExtra4 = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra4"]);
                     sExtra5 = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra5"]);
                     sExtra6 = Convert.ToString(dsItems.Tables[0].Rows[i]["sExtra6"]);


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



                         row.AddCell(string.Format("<a href=\"#\" ixLineItem=\"{0}\" sTableId=\"{2}\" ixProject=\"{3}\" onclick=\"ExamplePlugin.doPopup(this); return false;\">{1}</a>",
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
                     row.AddCell(HttpUtility.HtmlEncode(sExtra2));
                     row.AddCell(HttpUtility.HtmlEncode(sExtra4));
                     row.AddCell(HttpUtility.HtmlEncode(sExtra5));
                     row.AddCell(HttpUtility.HtmlEncode(sExtra6));
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

         /* This method builds the template for the add new item dialog */
         protected CDialogTemplate DialogTemplateNew(int ixBug, int ixProject, string sTableId)
         {
             CDialogTemplate dlgTemplateNew = new CDialogTemplate();
             /* There are several dialog formats to choose from */
             dlgTemplateNew.Template = new CDoubleColumnDialog();
             dlgTemplateNew.Template.sTitle = "Add New Item";

             string str1 = "GL Account", str2 = "Amount", str3 = "Tax type", str4 = "Tax", str5 = "Memo",
                 str6 = "Department", str7 = "Billable", str8 = "Addninfo", str9 = "Addninfo1", 
                 str10 = "Addninfo2", str11 = "Addninfo3", str12 = "Addninfo4";


             if (ixProject == 8 || ixProject == 9)
             {
                 str1 = "GL Account";
                 str2 = "Rate";
                 str3 = "Item";
                 str4 = "Quantity";
                 str5 = "Memo";
                 str6 = "Department";
                 str7 = "Class";
                 str8 = "Amount";
                 str9 = "Billable";
                 str10 = "Amort Schedule";
                 str11 = "Start Date";
                 str12 = "End Date";
             }

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
             dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                    api.AddPluginPrefix("ixProject"),
                                                    ixProject.ToString()));
             CDialogItem itemAccount = new CDialogItem(
                 GetSelects1("CWFAccount", ixProject),
                 str1,
                 "this account item has to be coded from the drop-down");
             dlgTemplateNew.Template.Items.Add(itemAccount);



             CDialogItem itemAmount =
                 new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""),
                                 str2);
             dlgTemplateNew.Template.Items.Add(itemAmount);

             CDialogItem itemTaxtype = new CDialogItem(
               GetSelects1("CWFVat", ixProject),
               str3,
               "This Tax item has to be coded from the drop-down");
             dlgTemplateNew.Template.Items.Add(itemTaxtype);

             CDialogItem itemTax =
     new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), ""),
                     str4);
             dlgTemplateNew.Template.Items.Add(itemTax);







             //CDialogItem itemForm99 =
             //    new CDialogItem(Forms.CheckboxInput(api.AddPluginPrefix("iForm99"), api.AddPluginPrefix("iForm99"), false),
             //                    "Has Form 1099");
             //dlgTemplateNew.Template.Items.Add(itemForm99);

             CDialogItem itemMemo =
                 new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""),
                                 str5);
             dlgTemplateNew.Template.Items.Add(itemMemo);

             CDialogItem itemDepartment = new CDialogItem(
                 GetSelects1("CWFDepartment", ixProject),
                 str6,
                 "Choose the department this item has to be coded from the drop-down");
             dlgTemplateNew.Template.Items.Add(itemDepartment);

             CDialogItem itemBillable = new CDialogItem(
                 GetSelects("CWFDept", ixProject),
                 str7,
                 "Choose the Class type this item has to be coded from the drop-down");
             dlgTemplateNew.Template.Items.Add(itemBillable);

             CDialogItem itemAddnInfo =
                   new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sAddninfo"), ""),
                                   str8);
             dlgTemplateNew.Template.Items.Add(itemAddnInfo);


             CDialogItem Extra2 =
                   new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra2"), ""),
                                   str9);
             dlgTemplateNew.Template.Items.Add(Extra2);

             CDialogItem Extra4 =
                   new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra4"), ""),
                                   str10);
             dlgTemplateNew.Template.Items.Add(Extra4);

             CDialogItem Extra5 =
                  new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra5"), ""),
                                  str11);
             dlgTemplateNew.Template.Items.Add(Extra5);


             CDialogItem Extra6 =
                  new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra6"), ""),
                                  str12);
             dlgTemplateNew.Template.Items.Add(Extra6);


             /* Standard ok and cancel buttons */
             dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

             return dlgTemplateNew;
         }

        //Added by Alok

         private string sTableId;

         protected CEditableTable EditableTable(int ixBug)
         {

             // api.Notifications.AddMessage("calling editable table");

             CEditableTable editableTable = new CEditableTable("Copycase");
             sTableId = editableTable.sId;
             /* Define the header row of the table */
             //editableTable.Header.AddCell("TestUpdate for updating");

             /* create a new table row and set the row id to the unique ixtype */
             CEditableTableRow row = new CEditableTableRow();
             row.sRowId = ixBug.ToString();
             row.AddCell(HttpUtility.HtmlEncode("Copy Case"));
             editableTable.Body.AddRow(row);

             /* Create the new dialog template object used when the user clicks Add
              * New type or the add icon in the footer row */
             CDialogTemplate dlgTemplateNew = DialogTemplateNew(ixBug);

             /* Add a footer row with icon and text links to the add new dialog */
             editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                     sTableId,
                                                     "CopyCase",
                                                     "sDataId",
                                                     CommandUrl1("CopyCase", ixBug),
                                                     "  Copy Case  "));

             /* Associate the dialog templates with the table by name */
             editableTable.AddDialogTemplate("CopyCase", dlgTemplateNew);


             return editableTable;
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
                     CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "copycase");
                 dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
                 /* include a security action token */
                 CDialogItem itemActionToken =
                     CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
                 dlgTemplateNew.Template.Items.Add(itemActionToken);
                 dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                        api.AddPluginPrefix("ixBug"),
                                                        ixBug.ToString()));
                 CDialogItem itemEditId =
                     new CDialogItem(Forms.TextInput(api.AddPluginPrefix("CaseID"), ""),
                                     "Case ID ");

                 dlgTemplateNew.Template.Items.Add(itemEditId);

                 //CDialogItem itemEditId1 =
                 //    new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Dummy ID"), ""),
                 //                    "Case ID dummy ");
                 //dlgTemplateNew.Template.Items.Add(itemEditId1);

                 CDialogItem itemEditId2 =
                     new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("Header"), "true", "CheckedAttribute","Header"));
                 dlgTemplateNew.Template.Items.Add(itemEditId2);

                 CDialogItem itemEditId3 =
                      new CDialogItem(Forms.CheckboxInputString(api.AddPluginPrefix("LineItems"), "true", "CheckedAttribute","Line Items"));
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
         //protected CDialogTemplate DialogTemplateEdit(string sTableId, int ixProject)
         //{

         //    string str1 = "GL Account", str2 = "Amount", str3 = "Tax type", str4 = "Tax",
         //        str5 = "Memo", str6 = "Department", str7 = "Billable", str8 = "Addninfo",
         //        str9 = "Addninfo1", str10 = "Addninfo2", str11 = "Addninfo3", str12 = "Addninfo4";

         //    if (ixProject == 8 || ixProject == 9)
         //    {
         //        str1 = "GL Account";
         //        str2 = "Rate";
         //        str3 = "Item";
         //        str4 = "Quantity";
         //        str5 = "Memo";
         //        str6 = "Department";
         //        str7 = "Customer:Job";
         //        str8 = "Amount";
         //        str9 = "Billable";
         //         str10 = "Amort Schedule";
         //         str11 = "Start Date";
         //         str12 = "End Date";
         //    }


         //    CDialogTemplate dlgTemplateEdit = new CDialogTemplate();
         //    dlgTemplateEdit.Template = new CDoubleColumnDialog();
         //    /* names in curly braces are replaced with the otuput of the ToString()
         //     * method for the corresponding value in the template's data hashtable */
         //    dlgTemplateEdit.Template.sTitle = "Edit Item ";
         //    CDialogItem itemEditHiddenUrl =
         //        CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
         //    dlgTemplateEdit.Template.Items.Add(itemEditHiddenUrl);
         //    CDialogItem itemEditHiddenAction =
         //        CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
         //    dlgTemplateEdit.Template.Items.Add(itemEditHiddenAction);
         //    dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
         //                                            api.AddPluginPrefix("ixBugLineItem"),
         //                                            "{ixBugLineItem}"));
         //    dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
         //                                            api.AddPluginPrefix("ixBug"),
         //                                            "{ixBug}"));
         //    dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
         //                                          api.AddPluginPrefix("ixProject"),
         //                                          ixProject.ToString()));
         //    /* include a security action token */
         //    CDialogItem itemActionToken =
         //        CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
         //    dlgTemplateEdit.Template.Items.Add(itemActionToken);

         //    //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Account Value is {ixGlAccount}."));

         // //   dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("CWFAccount", "GL Account", ixProject, "{ixCWFAccount}"));
            
             
         //  // dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("CWFAccount", "GL Account", ixProject, "{sAccount}"));

            
            
         //   // {sAccount}

             

         ////    dlgTemplateEdit.Template.Items.Add(new CDialogItem( Forms.SelectInputString(api.AddPluginPrefix("CWFAccount"),
         //       //                              Forms.SelectOptions(GetSelects3("CWFAccount",ixProject,false),
         //                                                //    "-Item-",
         //                                              //   GetSelects3("CWFAccount",ixProject,false))),"GL Account", "Choose the  GL Account  this item has to be coded from the drop-down."));

         //    dlgTemplateEdit.Template.Items.Add(new CDialogItem("{sAccount}", str1, "This item has to be coded from the drop-down."));

         //   // dlgTemplateEdit.Template.Items.Add(new Hashtable { {"selectItem", Forms.SelectInputString(Prefix + "SelectedStuff", Forms.SelectOptions(GetSelects("CWFAccount", ixProject), selectetItemValue, displayValues))}}

         //    //dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("CWFVat", "Tax type", ixProject, "{sTaxtype}"));

         //   // dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("CWFVat", "Tax type", ixProject, sTaxtype_P));

         //    dlgTemplateEdit.Template.Items.Add(new CDialogItem("{sTaxtype}", str3, "This item has to be coded from the drop-down."));

         //    CDialogItem itemAmount =
         //        new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), "{fAmount}"),
         //                        str2);
         //    dlgTemplateEdit.Template.Items.Add(itemAmount);  


             
             
                 
               
         //    /* CDialogItem itemTaxtype =
         //     new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sTaxtype"), "{sTaxtype}"),
         //                     "Taxtype");
         //     dlgTemplateEdit.Template.Items.Add(itemTaxtype);
         //     */

         //    CDialogItem itemTax =
         //      new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), "{fTax}"),
         //                      str4);
         //    dlgTemplateEdit.Template.Items.Add(itemTax);



         //    //CDialogItem itemForm99 =
         //    //    new CDialogItem(Forms.CheckboxInput(api.AddPluginPrefix("iForm99"), api.AddPluginPrefix("iForm99"), "{iForm99}"),
         //    //                    "Has Form 1099");
         //    //dlgTemplateEdit.Template.Items.Add(itemForm99);

         //    CDialogItem itemMemo =
         //        new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), "{sMemo}"),
         //                        str5);
         //    dlgTemplateEdit.Template.Items.Add(itemMemo);

         //    //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Department Value is {ixGlDepartment}"));
         //    // dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("CWFDepartment", "Department", ixProject, "{sDepartment}"));


         //   // dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("CWFDepartment", "Department", ixProject, "{sDepartment}"));

         //    dlgTemplateEdit.Template.Items.Add(new CDialogItem("{sDepartment}", str6, "This item has to be coded from the drop-down."));

         //    //dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("CWFDepartment", "Department", ixProject, sDepartment));


         //    //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Location Value is {ixGlLocation}."));
         //     //dlgTemplateEdit.Template.Items.Add(EditDialogDropDown2("CWFBillable", "Billable", ixProject, "{sBillable}"));
         //  //  dlgTemplateEdit.Template.Items.Add(EditDialogDropDown2("CWFBillable", "Billable", ixProject, sBillable_P)); 

         //    dlgTemplateEdit.Template.Items.Add(new CDialogItem("{sBillable}", str7, "This item has to be coded from the drop-down."));

         //    //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Project Value is {ixGlProject}."));
         //    //   dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("sAddninfo", "Additional Info", ixProject, "{sAddninfo}"));

         //    CDialogItem itemAddinfo =
         //        new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sAddninfo"), "{sAddninfo}"),
         //                        str8);
         //    dlgTemplateEdit.Template.Items.Add(itemAddinfo); 


         //        CDialogItem Extra2 =
         //        new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra2"), "{sExtra2}"),
         //                        str9);
         //        dlgTemplateEdit.Template.Items.Add(Extra2);

         //        CDialogItem Extra4 =
         //            new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra4"), "{sExtra4}"),
         //                            str10);
         //        dlgTemplateEdit.Template.Items.Add(Extra4);

         //        CDialogItem Extra5 =
         //           new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra5"), "{sExtra5}"),
         //                           str11);
         //        dlgTemplateEdit.Template.Items.Add(Extra5);

         //        CDialogItem Extra6 =
         //           new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra6"), "{sExtra6}"),
         //                           str12);
         //        dlgTemplateEdit.Template.Items.Add(Extra6);



         //    //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Item Value is {ixGlItem}."));
         //    //  dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("GlItem", "GL Item", ixProject, "{ixGlItem}"));

         //    //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Class Value is {ixGlClass}."));
         //    // dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("GlClass", "GL Class", ixProject, "{ixGlClass}"));

         //    /* Standard ok and cancel buttons */
         //    dlgTemplateEdit.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

         //    return dlgTemplateEdit;
         //}

         protected CDoubleColumnDialog DialogEditForAjax(int ixLineItem, string sTableId, string ixProject_1)
         {
             
            

             string str1 = "GL Account", str2 = "Amount", str3 = "Tax type", str4 = "Tax",
                 str5 = "Memo", str6 = "Department", str7 = "Billable", str8 = "Addninfo",
                 str9 = "Addninfo1", str10 = "Addninfo2", str11 = "Addninfo3", str12 = "Addninfo4";

             int ixProject = Convert.ToInt32(ixProject_1);
             //int iProj = Int32.Parse(ixProject);
             //string ixProject = api.Request[api.AddPluginPrefix("ixProject")].ToString();

             string[] sAccountList = GetSelects3("CWFAccount", ixProject, false);
             string[] sItemList = GetSelects3("CWFVat", ixProject, false);
             string[] sDepartmentList = GetSelects3("CWFDepartment", ixProject, false);
             string[] sBillableList = GetSelects4("CWFDept", ixProject, false);
             //string ixBugLineItem = "";

             //if (ixProject == "8" || ixProject == "9")
             if (ixProject == 8 || ixProject == 9)
             {
                 str1 = "GL Account";
                 str2 = "Rate";
                 str3 = "Item";
                 str4 = "Quantity";
                 str5 = "Memo";
                 str6 = "Department";
                 str7 = "Customer:Job";
                 str8 = "Amount";
                 str9 = "Billable";
                 str10 = "Amort Schedule";
                 str11 = "Start Date";
                 str12 = "End Date";
             }
             CDoubleColumnDialog dlgTemplateEdit = new CDoubleColumnDialog();
             //sTableId = dlgTemplateEdit.sId;

             dlgTemplateEdit.sTitle = "Edit Item ";
             CDialogItem itemEditHiddenUrl =
                 CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
             dlgTemplateEdit.Items.Add(itemEditHiddenUrl);
             CDialogItem itemEditHiddenAction =
                 CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
             dlgTemplateEdit.Items.Add(itemEditHiddenAction);
             dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(
                                                     api.AddPluginPrefix("ixLineItem"),
                                                     ixLineItem.ToString()));
             string strBugLineItem = ixLineItem.ToString();

             CSelectQuery sqlLineItemDetails;

             sqlLineItemDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceItems"));
             sqlLineItemDetails.AddSelect("*");

             //api.Notifications.AddAdminNotification("Message 7", "");
             //api.Notifications.AddAdminNotification("ixBugLineItem  =" + strBugLineItem, "");
             //api.Notifications.AddAdminNotification("ixBug =" + api.Bug.CurrentBug().ToString(), "");
             //sqlLineItemDetails.AddWhere("ixBug =" + api.Bug.CurrentBug().ToString() + " AND ixBugLineItem = " + strBugLineItem + " AND iDeleted = 0");
             sqlLineItemDetails.AddWhere(" ixBugLineItem = " + strBugLineItem + " AND iDeleted = 0");


             //api.Notifications.AddAdminNotification("Message 8", "");
             DataSet dsLineItem = sqlLineItemDetails.GetDataSet();
             DataRow dr = dsLineItem.Tables[0].Rows[0];

             dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(
                                                     api.AddPluginPrefix("ixBug"),
                                                     dr[1].ToString()));
             //api.Notifications.AddAdminNotification("Message 9", "");
             //dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(
             //                                      api.AddPluginPrefix("ixProject"),
             //                                      ixProject.ToString()));

             dlgTemplateEdit.Items.Add(CDialogItem.HiddenInput(
                                                  api.AddPluginPrefix("ixProject"),
                                                  ixProject.ToString()));
    
             //iProj.ToString()

             /* include a security action token */
             CDialogItem itemActionToken =
                 CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
             dlgTemplateEdit.Items.Add(itemActionToken);

             //api.Notifications.AddAdminNotification("Message 9", "");

             dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sAccount"),
                                                                Forms.SelectOptions(sAccountList,
                                                                dr[2].ToString(),
                                                                sAccountList)), str1));

             dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sTaxType"),
                                                                Forms.SelectOptions(sItemList,
                                                                dr[4].ToString(),
                                                                sItemList)), str3));
             CDialogItem itemAmount =
                 new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), dr[3].ToString()),
                                 str2);
             dlgTemplateEdit.Items.Add(itemAmount);

             CDialogItem itemTax =
               new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fTax"), dr[5].ToString()),
                               str4);

             dlgTemplateEdit.Items.Add(itemTax);

             CDialogItem itemMemo =
                 new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), dr[6].ToString()),
                                 str5);
             dlgTemplateEdit.Items.Add(itemMemo);

             dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sDepartment"),
                                                                Forms.SelectOptions(sDepartmentList,
                                                                dr[7].ToString(),
                                                                sDepartmentList)), str6));

             dlgTemplateEdit.Items.Add(new CDialogItem(Forms.SelectInputString(api.AddPluginPrefix("sBillable"),
                                                                Forms.SelectOptions(sBillableList,
                                                                dr[8].ToString(),
                                                                sBillableList)), str7));

             CDialogItem itemAddinfo =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sAddninfo"), dr[9].ToString()),
                                str8);
             dlgTemplateEdit.Items.Add(itemAddinfo);

             CDialogItem Extra2 =
             new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra2"), dr[12].ToString()),
                             str9);
             dlgTemplateEdit.Items.Add(Extra2);

             CDialogItem Extra4 =
                 new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra4"), dr[14].ToString()),
                                 str10);
             dlgTemplateEdit.Items.Add(Extra4);

             CDialogItem Extra5 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra5"), dr[15].ToString()),
                                str11);
             dlgTemplateEdit.Items.Add(Extra5);

             CDialogItem Extra6 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sExtra6"), dr[16].ToString()),
                                str12);
             dlgTemplateEdit.Items.Add(Extra6);

             //api.Notifications.AddAdminNotification("Message 10", "");

             /* Standard ok and cancel buttons */
             dlgTemplateEdit.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));
             //api.Notifications.AddAdminNotification("Message 11", "");
             //api.Notifications.AddAdminNotification(sTableId.ToString(), "");
             //api.Notifications.AddAdminNotification("Message 12", "");
             return dlgTemplateEdit;

             //CDoubleColumnDialog dlgEdit = new CDoubleColumnDialog();
             ///* names in curly braces are replaced with the otuput of the ToString()
             // * method for the corresponding value in the template's data hashtable */
             //dlgEdit.sTitle = string.Format("Edit Kiwi id {0}: \"{1}\"", ixKiwi, sKiwiName);
             //CDialogItem itemEditHiddenUrl =
             //    CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
             //dlgEdit.Items.Add(itemEditHiddenUrl);
             //CDialogItem itemEditHiddenAction =
             //    CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
             //dlgEdit.Items.Add(itemEditHiddenAction);
             //dlgEdit.Items.Add(CDialogItem.HiddenInput(
             //                                        api.AddPluginPrefix("ixKiwi"),
             //                                        ixKiwi.ToString()));
             ///* include a security action token */
             //CDialogItem itemActionToken =
             //    CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
             //dlgEdit.Items.Add(itemActionToken);

             //CDialogItem itemEditName =
             //    new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sKiwiFullName"),
             //                                    sKiwiName),
             //                    "Kiwi Name");
             //dlgEdit.Items.Add(itemEditName);

             //DateInputOptions birthDateOptions = new DateInputOptions();
             //birthDateOptions.fAllowFuture = false;
             ///* Forms.DateInputString takes a string date value, which is neccessary for dialog
             // * templates because that value is filled in later by javascript. Forms.DateInput
             // * requires a DateTime object which won't work here */
             //CDialogItem itemEditBirthDate = new CDialogItem(
             //    Forms.DateInputString("dtDateOfBirth",
             //                          api.AddPluginPrefix("dtDateOfBirth"),
             //                          dtDateOfBirth.ToString("o"),
             //                          birthDateOptions),
             //    "Kiwi Birthdate",
             //    "Enter a date or choose from the calendar (optional).");
             //dlgEdit.Items.Add(itemEditBirthDate);

             //// we use a placeholder here "zooSelect" which will be replaced by the entire
             //// <select><option></option><option></option></select> string because we need to
             //// put "selected" as an attribute on the right <option> tag server-side, not in JS
             //CDialogItem itemEditZoo = new CDialogItem(GetZooSelect(ixZoo),
             //    "Zoo Of Residence",
             //    "Choose the zoo where the kiwi lives from the drop-down.");
             //dlgEdit.Items.Add(itemEditZoo);

             ///* Standard ok and cancel buttons */
             //dlgEdit.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

             //return dlgEdit;
         }

         private CDialogItem EditDialogDropDown(string sFieldName, string sDisplay, int ixProject, string sSelected)
         {
                           

             //api.Notifications.AddAdminNotification("sSelected.  ", "-Item-");
             //api.Notifications.AddAdminNotification("sFieldName.  ", sFieldName);
            // sSelected = "-Item-";
     
        // sSelected = "-Item-";
            // Hashtable hash=  new Hashtable ();
            // hash.Add{ {"selectItem", GetSelects3(sSelected, sFieldName, ixProject, false)}};

          // CDialogItem cdi=  new CDialogItem("{selectItem}","Choose the " + sDisplay + " this item has to be coded from the drop-down.");

            // new Hashtable { { "selectItem", GetSelects3(sSelected, sFieldName, ixProject, false) } };

             //CDialogItem cdi = new CDialogItem(ha.ToString(),sDisplay,"Choose the " + sDisplay + " this item has to be coded from the drop-down.");


            // return new CDialogItem(sSelected,sDisplay, "Choose the " + sDisplay + " this item has to be coded from the drop-down."); ;

            // sSelected = " -Item-";

        // return new CDialogItem(
         //  GetSelects3(sSelected, sFieldName, ixProject, false),
            //    sDisplay,
              //   "Choose the " + sDisplay + " this item has to be coded from the drop-down.");

           //  return new CDialogItem("test", "test");

             return null;
         }

         private CDialogItem EditDialogDropDown2(string sFieldName, string sDisplay, int ixProject, string sSelected)
         {
            // return new CDialogItem(
                // GetSelects4(sSelected, sFieldName, ixProject, false),
               //  sDisplay,
                // "Choose the " + sDisplay + " this item has to be coded from the drop-down");

             return null;
         }

    
      
    
         /* This method builds the template for the delete item dialog */
         protected CDialogTemplate DialogTemplateDelete(string sTableId, int ixProject)
         {
             CDialogTemplate dlgTemplateDelete = new CDialogTemplate();
             dlgTemplateDelete.Template = new CSingleColumnDialog();
             dlgTemplateDelete.Template.sTitle = "Delete Item ";
             dlgTemplateDelete.Template.Items.Add(
                 CDialogItem.HiddenInput(api.AddPluginPrefix("ixBugLineItem"), "{ixBugLineItem}"));
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
             dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(
                                                     api.AddPluginPrefix("ixBug"),
                                                     "{ixBug}"));
             dlgTemplateDelete.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixProject"),
                                                   ixProject.ToString()));
             /* DialogItems don't have to be form elements, they can also be simple html */
             dlgTemplateDelete.Template.Items.Add(
                 new CDialogItem("Do you want to delete this item ?")
             );

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
                     api.Database.PluginTableName("CGSInvoiceDetails@conseroglobal.com", sTableName));
                 sq.AddSelect("s" + sTableName + "Name");
                 sq.AddWhere(api.Database.PluginTableName("CGSInvoiceDetails@conseroglobal.com", sTableName) + ".ix" + sTableName + " = " + sValue);
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

                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Value"].ToString());

                     }
                     else
                     {
                        
                       //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                        // names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                      // + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Value"].ToString());
                     }
                    // ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                 }

                 ds.Dispose();

               //  return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                  //                              Forms.SelectOptions(names,
                        //                                            sSelected,
                          //                                          ixs));

               //  api.Notifications.AddAdminNotification("a", sType);

                 return HttpUtility.HtmlDecode(Forms.SelectInputString(api.AddPluginPrefix(sType),
                                  Forms.SelectOptions(names,
                                                      sSelected,
                                                      names)));


             }
             ds.Dispose();
             return String.Empty;
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
                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                         + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                     }
                     else
                     {
                         //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                       + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                     }
                   //  ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                 }

                 ds.Dispose();

                 //  return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                 //                              Forms.SelectOptions(names,
                 //                                            sSelected,
                 //                                          ixs));

                 return HttpUtility.HtmlDecode(Forms.SelectInputString(api.AddPluginPrefix(sType),
                                  Forms.SelectOptions(names,
                                                      sSelected,
                                                      names)));


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
                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                         + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                     }
                     else
                     {
                         //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                       + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                     }
                     //  ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                 }

                 ds.Dispose();

                 //  return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                 //                              Forms.SelectOptions(names,
                 //                                            sSelected,
                 //                                          ixs));

                 //api.Notifications.AddAdminNotification("2", sType);

                 return HttpUtility.HtmlDecode(Forms.SelectInputString(api.AddPluginPrefix("s" + sType),
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
                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                         + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                     }
                     else
                     {
                         //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                       + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                     }
                     //  ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                 }

                 ds.Dispose();

                 //  return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                 //                              Forms.SelectOptions(names,
                 //                                            sSelected,
                 //                                          ixs));

               //  api.Notifications.AddAdminNotification("2 type", sType);
                // api.Notifications.AddAdminNotification("3 selected", sSelected);

               //  sSelected = "-Item-";
                // api.Notifications.AddAdminNotification("3 selected", sSelected);
                 //new CDialogItem("{selectItem}");

                 //Hashtable hash1 = new Hashtable { { "selectItem", HttpUtility.HtmlDecode(Forms.SelectInputString(api.AddPluginPrefix("s" + sType), Forms.SelectOptions(names, sSelected, names))) } };

                 // Hashtable hash1 = new Hashtable { { "selectItem",GetSelects3 (sSelected,

                // return HttpUtility.HtmlDecode(Forms.SelectInputString(api.AddPluginPrefix("s" + sType),
                //                 Forms.SelectOptions(names,
                    //                                sSelected,
                    //                                 names)));

                 //return (Forms.SelectInputString(api.AddPluginPrefix("s" + sType),
                            //     Forms.SelectOptions(names,
                                               //     sSelected,
                                                 //    names)));
                 return (names);


                 /*
                 return HttpUtility.HtmlDecode(Forms.SelectInputString(api.AddPluginPrefix("s" + sType),
                                  Forms.SelectOptions(names,
                                                      sSelected,
                                                      names)));

                 */

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

                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Value"].ToString());

                     }
                     else
                     {

                         //  names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                         // names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                         // + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                         names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Value"].ToString());
                     }
                     // ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                 }

                 ds.Dispose();

                 //  return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                 //                              Forms.SelectOptions(names,
                 //                                            sSelected,
                 //                                          ixs));

                

               //  return HttpUtility.HtmlDecode(Forms.SelectInputString(api.AddPluginPrefix(sType),
                                //  Forms.SelectOptions(names,
                                               //       sSelected,
                                                 //     names)));
                 return names;

             }
             ds.Dispose();
             return empty;
         }


         #endregion

             protected string CommandUrl(string sCommand, int ixLineItem)
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
    }
        }