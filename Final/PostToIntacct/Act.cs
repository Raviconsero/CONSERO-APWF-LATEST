using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace PostToIntacct
{
    public class Act : Plugin, IPluginBugJoin,
        IPluginBugDisplay, IPluginBugCommit, IPluginDatabase, IPluginGridColumn, IPluginRawPageDisplay
    {
        private enum IntacctActionType
        {
            CreateBill,
            UpdateBill,
            PayBill,
            PlaceBillOnHold,
            DuplicateBill,
            ReverseBill
        }
      //  int Rename = 0;

        /* The plugin Id is a required argument for CBug.SetPluginField and 
         * CBug.GetPluginField */
        protected const string PLUGIN_ID =
            "PostToIntacct@conseroglobal.com";

        /* A constant for populating the "code name" input field for multiple case edit */
        protected const string VARIOUS_TEXT = "[various]";
        private string sPrefixedTableName;

        /* Constructor: We'll just initialize the inherited Plugin class, which 
         * takes the passed instance of CPluginApi and sets its "api" member variable. */
        public Act(CPluginApi api)
            : base(api)
        {
            sPrefixedTableName = api.Database.PluginTableName("BugInvoice");
        }

        #region IPluginBugJoin Members

        public string[] BugJoinTables()
        {
            /* All tables specified here must have an integer ixBug column so FogBugz can
            * perform the necessary join. */

            return new string[] { "BugInvoice" };
        }

        #endregion

        #region IPluginBugDisplay Members

        public CBugDisplayDialogItem[]
            BugDisplayEditLeft(CBug[] rgbug, BugEditMode nMode, bool fPublic)
        {
            return null;
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
            //  this.api.Notifications.AddMessage("category" + bug.ixCategory);
            //if (bug.ixProject != 11 || bug.ixProject != 12)
            // {
          
            //if (bug.ixCategory != 17)
            //{
            //    if (bug.ixCategory != 19)
            //    {
            //        return null;
            //    }
            //}
                //return null;
                //  }
                // }

                CProject project = api.Project.GetProject(bug.ixProject);
                string enableIntacct = Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sEnableIntacct"));
                if (string.IsNullOrEmpty(enableIntacct) || "0".Equals(enableIntacct))
                {
                    //don't do any intacct calls
                    return null;
                }

                if (nMode == BugEditMode.Edit)
                {
                    // if (rgbug[0].ixStatus == 20 || rgbug[0].ixStatus == 123 || rgbug[0].ixStatus == 27)
                    //{
                    api.Notifications.AddMessage("To Post the bill in to intacct, Once you fill in the necessary details, please set the STATUS to 'POST TO INTACCT.'");

                    string sIntacctKey = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey"));
                    if (!string.IsNullOrEmpty(sIntacctKey))
                    {
                        api.Notifications.AddError("Please be advised that this case has already been posted to Intacct. Any changes you make here related to the invoice, should be MANUALLY updated in Intacct too, as the process DICTATES you do so.");
                    }

                    return new CBugDisplayDialogItem[] 
                   { 
                         new CBugDisplayDialogItem("ApprInfo", null, "Invoice Approval Status", 3), 
                       CreateListField1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", "CWFApproverl1", true),
                       CreateListField1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", "CWFApproverl2", true),
                       CreateListField1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", "CWFApproverl3", true),
                       CreateListField1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", "CWFApproverl4", true),
                       new CBugDisplayDialogItem("Invhead", null, "Invoice Header Information", 3),
                       CreateTextInputField(rgbug, "BugInvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                       CreateTextInputField(rgbug, "BugBalanceDue", "Invoice Amount", "sBalanceDue"),
                       CreateDateInputField(rgbug, "BugInvoiceDate", "Invoice Date", "sInvoiceDate"),
                       CreateListField(rgbug, "BugNetTerms", "Net Terms", "ixGlNetTerm", "GlNetTerm", false),
                       CreateListField(rgbug, "BugVendorName", "Vendor Name", "ixGLVendor", "GlVendor", true),
                       CreateListField2(rgbug, "BugTrxCurrency", "Trx Currency", "ixGlTrxCurrency", "GlTrxCurrency", true),
                       CreateListField2(rgbug, "BugExchratetype", "Exch Rate Type", "ixGlExchratetype", "GlExchratetype", true),
                       CreateDateInputField(rgbug, "BugExchratedate", "Exch Rate Date", "sExchratedate"),
                       CreateTextInputField(rgbug, "BugExchrate", "Exchange Rate", "sExchrate"),
                       CreateTextInputField(rgbug, "BugPONumber", "PO Number", "sPONumber"),
                       CreateTextInputField(rgbug, "BugDueDate", "Due Date", "sDueDate"),
                       CreateText(rgbug, "BugIntacctKey", "Intacct Key", "iIntacctKey"),
                       CreateDateInputField(rgbug, "BugPostingPeriod", "GLPosting Period", "sPostingPeriod"),
                       CreateCheckbox(rgbug,"Force"),
                       new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   };
                    // }
                }

                if (nMode == BugEditMode.Resolve)
                    return new CBugDisplayDialogItem[] 
                   {
                       new CBugDisplayDialogItem("ApprInfo", null, "Invoice Approval Status", 3),
                        CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl2"),
            CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
            CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
            CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
            new CBugDisplayDialogItem("Invhead", null, "Invoice Header Information", 3),
                   // CreateListField(rgbug, "BugBankAccount", "Bank Account", "ixGlBankAccount", "GlBankAccount", false),
                   // CreateListField(rgbug, "BugPaymentMethod", "Payment Method", "ixGlPaymentMethod", "GlPaymentMethod", false),
                      CreateText(rgbug, "BugPaymentAmount", "Payment Amount", "sBalanceDue"),
                     CreateText(rgbug, "BugInvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                    CreateText(rgbug, "BugBalanceDue", "Invoice Amount", "sBalanceDue"),
                    CreateText(rgbug, "BugInvoiceDate", "Invoice Date", "sInvoiceDate"),
                    CreateText(rgbug, "BugNetTerms", "Net Terms", "ixGlNetTerm", true, "GlNetTerm"),
                    CreateText(rgbug, "BugVendorName", "Vendor Name", "ixGlVendor", true, "GlVendor"),
                    CreateText2(rgbug, "BugTrxCurrency", "Trx Currency", "ixGlTrxCurrency", true, "GlTrxCurrency"),
                    CreateText2(rgbug, "BugExchratetype", "Exch Rate Type", "ixGlExchratetype", true, "GlExchratetype"),
                    CreateText(rgbug, "BugExchratedate", "Exch Rate Date", "sExchratedate"),
                    CreateText(rgbug, "BugExchrate", "Exchange Rate", "sExchrate"),
                    CreateText(rgbug, "BugPONumber", "PO Number", "sPONumber"),
                    CreateText(rgbug, "BugDueDate", "Due Date", "sDueDate"),
                    
                   // CreateText(rgbug, "BugBalanceDue", "Invoice Amount", "sBalanceDue"),
                    CreateText(rgbug, "BugIntacctKey", "Intacct Key", "iIntacctKey"),
                    CreateText(rgbug, "BugPostingPeriod", "GLPosting Period", "sPostingPeriod"),
                    new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)
                  };


                return new CBugDisplayDialogItem[] { 
                CreateText(rgbug, "BugBalanceDue", "Invoice Amount", "sBalanceDue"),
                CreateText(rgbug, "BugInvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                CreateText(rgbug, "BugInvoiceDate", "Invoice Date", "sInvoiceDate"),
                CreateText(rgbug, "BugNetTerms", "Net Terms", "ixGlNetTerm", true, "GlNetTerm"),
                CreateText(rgbug, "BugVendorName", "Vendor Name", "ixGlVendor", true, "GlVendor"),
                CreateText2(rgbug, "BugTrxCurrency", "Trx Currency", "ixGlTrxCurrency", true, "GlTrxCurrency"),
                CreateText2(rgbug, "BugExchratetype", "Exch Rate Type", "ixGlExchratetype", true, "GlExchratetype"),
                CreateText(rgbug, "BugExchratedate", "Exch Rate Date", "sExchratedate"),
                CreateText(rgbug, "BugExchrate", "Exchange Rate", "sExchrate"),
                CreateText(rgbug, "BugPONumber", "PO Number", "sPONumber"),
                CreateText(rgbug, "BugDueDate", "Due Date", "sDueDate"),
               // CreateText(rgbug, "BugBalanceDue", "Invoice Amount", "sBalanceDue"),
                CreateText(rgbug, "BugIntacctKey", "Intacct Key", "iIntacctKey"),
                new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3),
                CreateText(rgbug, "BugBankAccount", "Bank Account ", "ixGlBankAccount", true, "GlBankAccount"),
                CreateText(rgbug, "BugPostingPeriod", "Posting Period", "sPostingPeriod"),
                CreateText(rgbug, "BugPaymentMethod", "Payment Method", "ixGlPaymentMethod", true, "GlPaymentMethod")

            };

            
        }

        private CBugDisplayDialogItem CreateCheckbox(CBug[] rgbug,string itemName)
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

        private CBugDisplayDialogItem CreateListField2(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelects2(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            return DialogItem;
        }


        private CBugDisplayDialogItem CreateListField1(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelects1(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            return DialogItem;
        }


        private CBugDisplayDialogItem CreateListField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, string dropDownName, bool bDisplayId)
        {
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = GetSelects(GetText(rgbug, fieldName), dropDownName, rgbug[0].ixProject, bDisplayId);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateText_1(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName)).Trim();

            int atlevel = Convert.ToInt32(rgbug[0].GetPluginField(PLUGIN_ID, "ixAtlevel"));
            // if (bLookup)
            // {
            //   sValue = QueryDbForValue(sTableName, sValue);
            // }


            // CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            // DialogItem.sLabel = fielddisplay;
            // DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
            // return DialogItem;
            // api.Notifications.AddMessage("atlevel  ||" +atlevel);
            if (atlevel == 1)
            {
                if (rgbug[0].ixStatus == 124 && fieldName == "CWFApproverl1")
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

                else if (rgbug[0].ixStatus == 124 && fieldName == "CWFApproverl2")
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

                else if (rgbug[0].ixStatus == 124 && fieldName == "CWFApproverl3")
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
                else if (rgbug[0].ixStatus == 124 && fieldName == "CWFApproverl4")
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


            //  return ;



        }

        public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
        {
            //Newly Added code by Alok
            /* If there was an error passed in the URL or the redirect from
             * the raw page, display it using the Notifications API */

               CProject project = api.Project.GetProject(rgbug[0].ixProject);
                string enableIntacct = Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sEnableIntacct"));
                if (string.IsNullOrEmpty(enableIntacct) || "0".Equals(enableIntacct))
                {
                    //don't do any intacct calls
                    return null;
                }


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
/*
            CProject project1 = api.Project.GetProject(rgbug[0].ixProject);
            string enabledCGSWorkflowSettings = Convert.ToString(project1.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            if (string.IsNullOrEmpty(enabledCGSWorkflowSettings) || "0".Equals(enabledCGSWorkflowSettings) || "1".Equals(enabledCGSWorkflowSettings))
            {
                //don't do any intacct calls
                return null;
            }
            */

           // if (rgbug[0].ixProject == 13 || rgbug[0].ixProject == 15)
            {
                return new CBugDisplayDialogItem[] {
                new CBugDisplayDialogItem("Copy Case Details", EditableTable(rgbug[0].ixBug).RenderHtml()),
                               
                };
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

            //if (bug.ixProject != 11 || bug.ixProject != 12)
            //{
            //    return null;
            //}

            //if (bug.ixCategory != 17 || !bug.fOpen)
            //{
            //    if (bug.ixCategory != 19)
            //    {
            //        return null;
            //    }
            //}

            CProject project = api.Project.GetProject(bug.ixProject);
            string enableIntacct = Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sEnableIntacct"));
            if (string.IsNullOrEmpty(enableIntacct) || "0".Equals(enableIntacct))
            {
                //don't do any intacct calls
                return null;
            }

            string sIntacctKey = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey"));
            if (!string.IsNullOrEmpty(sIntacctKey) && bug.ixStatus == 20)

            {
                api.Notifications.AddError("Please be advised that this case has already been posted to Intacct. Any changes you make here related to the invoice, should be MANUALLY updated in Intacct too, as the process DICTATES you do so.");
            }

            if (!string.IsNullOrEmpty(sIntacctKey) && bug.ixStatus == 27)
            {
                api.Notifications.AddMessage("Bill number in Intacct is " + sIntacctKey + ". Please click on 'RESOLVE' to begin the approval process.");
            }

                return new CBugDisplayDialogItem[] { 
                new CBugDisplayDialogItem("ApprInfo_1", null, "Invoice Approval Sequence Setup", 3),
                CreateText_1(rgbug, "L1Approver", "Level-1 Approver", "CWFApproverl1", true, "CWFApproverl2"),
                CreateText_1(rgbug, "L2Approver", "Level-2 Approver", "CWFApproverl2", true, "CWFApproverl2"),
                CreateText_1(rgbug, "L3Approver", "Level-3 Approver", "CWFApproverl3", true, "CWFApproverl3"),
                CreateText_1(rgbug, "L4Approver", "Level-4 Approver", "CWFApproverl4", true, "CWFApproverl4"),
                new CBugDisplayDialogItem("Invhead_1", null, "Invoice Header Information", 3),
                
                CreateText(rgbug, "BugInvoiceNumber", "Invoice Number", "sInvoiceNumber"),
                CreateText(rgbug, "BugBalanceDue", "Invoice Amount", "sBalanceDue"),
                CreateText(rgbug, "BugInvoiceDate", "Invoice Date", "sInvoiceDate"),
                CreateText(rgbug, "BugNetTerms", "Net Terms", "ixGlNetTerm", true, "GlNetTerm"),
                CreateText(rgbug, "BugVendorName", "Vendor Name", "ixGlVendor", true, "GlVendor"),
                CreateText2(rgbug, "BugTrxCurrency", "Trx Currency", "ixGlTrxCurrency", true, "GlTrxCurrency"),
                CreateText2(rgbug, "BugExchratetype", "Exch Rate Type", "ixGlExchratetype", true, "GlExchratetype"),
                CreateText(rgbug, "BugExchratedate", "Exch Rate Date", "sExchratedate"),
                CreateText(rgbug, "BugExchrate", "Exchange Rate", "sExchrate"),
                CreateText(rgbug, "BugPONumber", "PO Number", "sPONumber"),
                CreateText(rgbug, "BugDueDate", "Due Date", "sDueDate"),
                CreateText(rgbug, "BugIntacctKey", "Intacct Key", "iIntacctKey"),
                CreateText(rgbug, "BugPostingPeriod", "Posting Period", "sPostingPeriod"),
                new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)
            };
        }

        private CBugDisplayDialogItem CreateText(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            return CreateText(rgbug, itemName, fielddisplay, fieldName, false, null);
        }

        private CBugDisplayDialogItem CreateText(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
            if (bLookup)
            {
                sValue = QueryDbForValue(sTableName, sValue);
            }
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
            return DialogItem;
        }

        private CBugDisplayDialogItem CreateText2(CBug[] rgbug, string itemName, string fielddisplay, string fieldName, bool bLookup, string sTableName)
        {
            string sValue = Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
            if (bLookup)
            {
                sValue = QueryDbForValue2(sTableName, sValue);
            }
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
            CProject project = api.Project.GetProject(bug.ixProject);
            string enableIntacct = Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sEnableIntacct"));
            if (string.IsNullOrEmpty(enableIntacct) || "0".Equals(enableIntacct))
            {
                //don't do any intacct calls
                return;
            }
            
            if (nBugAction == BugAction.Edit || nBugAction == BugAction.Assign)
            {
                //don't do any of the following actions
               // return;

                if (bug.ixStatus == 121)
                {
                    // api.Notifications.AddMessage("calling rename");
                    //RenameFile_OLD(bug, bugevent);
                    RenameFile(bug, bugevent);
                }
                

                {
                    // api.Notifications.AddMessage("1", "1");
                    string Intacctkey = "";
                    CSelectQuery sq_1 = api.Database.NewSelectQuery(api.Database.PluginTableName("PostToIntacct@conseroglobal.com", "BugInvoice"));
                    sq_1.AddSelect("iIntacctKey");
                    sq_1.AddWhere(api.Database.PluginTableName("PostToIntacct@conseroglobal.com", "BugInvoice") + ".ixBug = " + bug.ixBug);
                    DataSet dkey = sq_1.GetDataSet();
                    //api.Notifications.AddMessage("2", "2");
                    if (null != dkey.Tables && dkey.Tables.Count == 1 && dkey.Tables[0].Rows.Count > 0)
                    {
                        Intacctkey = dkey.Tables[0].Rows[0]["iIntacctKey"].ToString();
                      //  api.Notifications.AddMessage("Inatkey", Intacctkey);
                        if (bug.ixStatus == 121 || bug.ixStatus == 120)
                        {
                            //api.Notifications.AddMessage("3", "3");
                            updateIntacctStatus(bug, Intacctkey, "Pending Approval");
                        }

                    }

                }
            }

                if (nBugAction == BugAction.Resolve)
                {


                    string Intacctkey = "";
                    CSelectQuery sq_1 = api.Database.NewSelectQuery(api.Database.PluginTableName("PostToIntacct@conseroglobal.com", "BugInvoice"));
                    sq_1.AddSelect("iIntacctKey");
                    sq_1.AddWhere(api.Database.PluginTableName("PostToIntacct@conseroglobal.com", "BugInvoice") + ".ixBug = " + bug.ixBug);
                    DataSet dkey = sq_1.GetDataSet();
                    //api.Notifications.AddMessage("2", "2");
                    if (null != dkey.Tables && dkey.Tables.Count == 1 && dkey.Tables[0].Rows.Count > 0)
                    {
                        Intacctkey = dkey.Tables[0].Rows[0]["iIntacctKey"].ToString();
                     //   api.Notifications.AddMessage("Inatkey", Intacctkey);
                        if (bug.ixStatus == 118)
                        {
                            //api.Notifications.AddMessage("3", "3");
                            updateIntacctStatus(bug, Intacctkey, "Approved");
                        }

                        else if (bug.ixStatus == 124)
                        {
                            //api.Notifications.AddMessage("3", "3");
                            updateIntacctStatus(bug, Intacctkey, "Rejected");
                        }

                    }

                }
         

         
            if (bug.ixStatus == 121)
            {
               // api.Notifications.AddMessage("calling rename");
                //RenameFile_OLD(bug, bugevent);
                //RenameFile(bug, bugevent);
            }
           
           
           
        }
        


        public void RenameFile_OLD(CBug bug, CBugEvent bugevent)
        {
            //date_vendor_invoicenumber_casenumber.pdf
            string sFileName = "";
            string sInvoiceNumber = "";//Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber"));
            string sVendorId = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlVendor"));
            string sVendorName = "";
           // DateTime sINVDate;
            //string sAmount = "";
            //string sINVDate2 = "";
            //string sCountry = "";
            string sFolderdate = "";
            //  string sMonth = "";
            // string sDate = "";

            /*  if (!string.IsNullOrEmpty(sVendorId))
              {
                  sVendorName = QueryDbForValue("GlVendor", sVendorId);
              }
             */
            
            

            //querying Custom bugfields for invoice and vendor name to attch with mail subject start
             sInvoiceNumber = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber"));
             sVendorId = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlVendor"));
             sVendorName = "";

            if (!string.IsNullOrEmpty(sVendorId))
            {
                sVendorName = QueryDbForValue("GlVendor", sVendorId);
            }

            string sDate = "";
            string sCaseNumber = bug.ixBug.ToString();

            if (!string.IsNullOrEmpty(sInvoiceNumber) &&
                !string.IsNullOrEmpty(sVendorName))
            {
                DateTime bugdate = bug.dtOpened;
                sFolderdate = bugdate.ToString("MM.dd.yy");
                sFileName = sInvoiceNumber + "_" + sVendorName + "_" + sFolderdate + "_" + sCaseNumber;
            }

            //api.Notifications.AddMessage("file name" + sVendorName + "_" + sFolderdate + "_" + sCaseNumber);
            if (!string.IsNullOrEmpty(sFileName))
            {
                //api.Notifications.AddMessage("file inside");
                string fileBackupPath = "";
                CProject project = api.Project.GetProject(bug.ixProject);
                //string backUpLocation = "D:";//Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sBackupLocation"));
                //string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Default Sync Folder"
                string backUpLocation = "C:\\Users\\Administrator\\Documents\\My Box Files\\Silverback\\" + project.sProject + "\\" + sVendorName + "\\" + sDate;
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



                            //      sFileName += ".";
                            //    sFileName += fileExtension;

                            sExtn = fileExtension.ToLower();

                        }
                        // for checking extension

                        if (sExtn == "doc" || sExtn == "pdf" || sExtn == "bmp" || sExtn == "jpg" || sExtn == "jpeg" || sExtn == "xls" || sExtn == "xlsx" || sExtn == "docx" || sExtn == "gif" || sExtn == "tif")// || sExtn == "png")
                        {

                            int sAttachmentold = ixAttachment;
                            if (icount > 0)
                            {

                                sFilename2 += "_" + icount;



                                // icount = icount + 1;
                            }

                            icount = icount + 1;
                            sFilename2 += ".";
                            sFilename2 += sExtn;

                            if (attachment.sFileName != sFilename2)
                            {

                                CAttachment clonedAttachment = CloneAttachment(attachment, sFilename2);
                                attachments.Add(clonedAttachment);

                                bugevent.CommitAttachmentAssociation(attachments.ToArray());

                                if (!string.IsNullOrEmpty(backUpLocation))
                                {


                                    // fileBackupPath = backUpLocation + "\\" + project.sProject + "\\" + sVendorName + "\\" + sDate + "\\" + sFileName;
                                    fileBackupPath = backUpLocation + "\\" + sFilename2;
                                    CreateDirectory(new DirectoryInfo(Path.GetDirectoryName(fileBackupPath)));
                                    // api.Notifications.AddMessage("File has been backed up as " + Path.GetFullPath(fileBackupPath));
                                    api.Notifications.AddMessage("Invoice has been backed up succsessfuly");

                                    FileStream fileStream = new FileStream(Path.GetFullPath(fileBackupPath), FileMode.Create, FileAccess.Write);
                                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                                    binaryWriter.Write(attachment.rgbData);
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

        public void RenameFile(CBug bug, CBugEvent bugevent)
        {
            string sFileName = "";
            string sInvoiceNumber = "";//Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber"));
            string sVendorId = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlVendor"));
            string sVendorName = "";
            string sFolderdate = "";
           
            //querying Custom bugfields for invoice and vendor name to attch with mail subject start
            sInvoiceNumber = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber"));
            sVendorId = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlVendor"));
            sVendorName = "";

            if (!string.IsNullOrEmpty(sVendorId))
            {
                sVendorName = QueryDbForValue("GlVendor", sVendorId);
            }

            string sDate = "";
            string sCaseNumber = bug.ixBug.ToString();

            if (!string.IsNullOrEmpty(sInvoiceNumber) &&
                !string.IsNullOrEmpty(sVendorName))
            {
                DateTime bugdate = bug.dtOpened;
                sFolderdate = bugdate.ToString("MM.dd.yy");
                sFileName = sInvoiceNumber + "_" + sVendorName + "_" + sFolderdate + "_" + sCaseNumber;
            }

            string attach1 = sFileName;
            // api.Notifications.AddMessage("attach1" + attach1);
            if (!string.IsNullOrEmpty(sFileName))
            {

                string fileBackupPath = "";
                CProject project = api.Project.GetProject(bug.ixProject);
                //string backUpLocation = "D:";//Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sBackupLocation"));
                //string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Default Sync Folder"
                // string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Ford Direct\\" + sCountry + "\\" + sFolderdate;
                string backUpLocation = "C:\\Users\\Administrator\\Documents\\My Box Files\\Silverback\\" + project.sProject + "\\" + sVendorName + "\\" + sDate;
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

                    //foreach (int n in attachid2)
                    //{
                    //    bug.DeleteAttachment(n);
                    //}

                    //foreach (int m in attachid3)
                    //{
                    //    bug.DeleteAttachment(m);
                    //}

                    ds.Dispose();
                }

            }
        }
       

        private string QueryDbForValue(string sTableName, string sValue)
        {
            string sName = "";
            if (!string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sValue))
            {
                CSelectQuery sq = api.Database.NewSelectQuery(
                    api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sTableName));
                sq.AddSelect("s" + sTableName + "Name");
                sq.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sTableName) + ".ix" + sTableName + " = " + sValue);
                DataSet ds = sq.GetDataSet();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                {
                    sName = ds.Tables[0].Rows[0]["s" + sTableName + "Name"].ToString();
                }
                ds.Dispose();
            }
            return sName;
        }

        private string QueryDbForValue2(string sTableName, string sValue)
        {
            string sName = "";
            if (!string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sValue))
            {
                CSelectQuery sq = api.Database.NewSelectQuery(
                    api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sTableName));
                sq.AddSelect("s" + sTableName + "Id");
                sq.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sTableName) + ".ix" + sTableName + " = " + sValue);
                DataSet ds = sq.GetDataSet();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                {
                    sName = ds.Tables[0].Rows[0]["s" + sTableName + "Id"].ToString();
                }
                ds.Dispose();
            }
            return sName;
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

        public void BugCommitBefore(CBug bug, BugAction nBugAction, CBugEvent bugevent,
            bool fPublic)
        {
            //if (bug.ixCategory != 17 )
            //{
            //    //api.Notifications.AddMessage("Not an Category of Account Payable");
            //    if (bug.ixCategory != 19)
            //    {
            //        return;
            //    }
            //    //return;
            //}

            CProject project = api.Project.GetProject(bug.ixProject);
            string enableIntacct = Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sEnableIntacct"));
            if (string.IsNullOrEmpty(enableIntacct) || "0".Equals(enableIntacct))
            {
                //api.Notifications.AddMessage("Intacct Not Enable For This Project.");
                //don't do any intacct calls
                return;
            }

           
            //api.Notifications.AddMessage("Bug Status is " + bug.ixStatus.ToString());
            //api.Notifications.AddMessage("Bug Change Status is " + bugevent.sStatus);

            bool bInvoiceAndLineItemsAmountMatches = true;

            //If there is a change and the status is "waiting for gl entry" then post to intacct
            if (bug.ixStatus == 20 || bug.ixStatus == 26 || bug.ixStatus == 121 || bug.ixStatus == 27 || bug.ixStatus==125)
            {
                ExtractValue(bug, bugevent, "sInvoiceNumber", "Invoice Number",1);
                bool bInvoiceDateChanged = ExtractValue(bug, bugevent, "sInvoiceDate", "Invoice Date",1);  
                bool bNetTermsChanged = ExtractValue(bug, bugevent, "ixGlNetTerm", "Net Terms",1);
                //ExtractValue(bug, bugevent, "sInvoiceDate", "Invoice Date", 1);
                //ExtractValue(bug, bugevent, "ixGlNetTerm", "Net Terms", 1);
                ExtractValue(bug, bugevent, "ixGlVendor", "Vendor Name",1);
                ExtractValue(bug, bugevent, "ixGlTrxCurrency", "Trx Currency",1);
                ExtractValue(bug, bugevent, "ixGlExchratetype", "Exch Rate Type",1);
                ExtractValue(bug, bugevent, "sExchratedate", "Exch Rate Date",1);
                ExtractValue(bug, bugevent, "sExchrate", "Exchange Rate",0);
                ExtractValue(bug, bugevent, "sPONumber", "PO Number",0);
                ExtractValue(bug, bugevent, "sBalanceDue", "Invoice Amount",0);
                ExtractValue(bug, bugevent, "CWFApproverl1", "Level-1 Approver",0);
                ExtractValue(bug, bugevent, "CWFApproverl2", "Level-2 Approver",0);
                ExtractValue(bug, bugevent, "CWFApproverl3", "Level-3 Approver",0);
                ExtractValue(bug, bugevent, "CWFApproverl4", "Level-4 Approver",0);
                ExtractValue(bug, bugevent, "sPostingPeriod", "Posting Period",1);
               

                if (bug.GetPluginField(PLUGIN_ID, "ixGlVendor").ToString() != null)
                {
                   // api.Notifications.AddMessage("Bug Status is " + bug.ixStatus.ToString());
                    string Vendval = bug.GetPluginField(PLUGIN_ID, "ixGlVendor").ToString();

                    CSelectQuery sq_1 = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlVendor"));
                    sq_1.AddSelect("sGlVendorId,sGlVendorName");
                    sq_1.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlVendor") + ".ixGlVendor = " + Vendval);
                    DataSet d_1 = sq_1.GetDataSet();

                    if (null != d_1.Tables && d_1.Tables.Count == 1 && d_1.Tables[0].Rows.Count > 0)
                    {
                        string Vendor_Name = Convert.ToString(d_1.Tables[0].Rows[0]["sGlVendorId"]) + ":" + Convert.ToString(d_1.Tables[0].Rows[0]["sGlVendorName"]);

                        bug.SetPluginField(PLUGIN_ID, "VendorName", Vendor_Name);
                    }
                }

                if (bInvoiceDateChanged || bNetTermsChanged)
                {
                    SetInvoiceDueDate(bug);
                }

                DataSet dsItems = FetchItems(bug.ixBug, true);
                double dAmount = 0d;
                if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                    {
                        try
                        {
                            dAmount += Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                            bug.SetPluginField(PLUGIN_ID, "sBalanceDue", dAmount);
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
                    invoiceAmount = Convert.ToDouble(bug.GetPluginField(PLUGIN_ID, "sBalanceDue"));
                }
                catch
                {
                    bug.SetPluginField(PLUGIN_ID, "sBalanceDue", "0");
                    api.Notifications.AddMessage("Invoice Amount value is invalid. Reset to 0.");
                    invoiceAmount = 0d;
                    bug.ixStatus=127;
                }

                if (invoiceAmount == 0 || invoiceAmount != dAmount)
                {
                   // bInvoiceAndLineItemsAmountMatches = false;
                    api.Notifications.AddMessage("Invoice Amount Is                 - " + invoiceAmount.ToString("C")) ;
                    api.Notifications.AddMessage( "Sum of the Line Items Amount is  - " + dAmount.ToString("C") ); 
                    api.Notifications.AddMessage( "If they do not match or either 0. Please Fix It." ); 

                    
                    //bug.ixStatus = 127;
                
                }
            }

            //If there is a change in status and the status says "Post To GL 26"
            if (!string.IsNullOrEmpty(bugevent.sStatus) && bug.ixStatus == 125)// 125 Post to intacct for silverback
            {
                //string vendorname = "";
                //string currency = "";
                //DateTime Postperiod;
                //DateTime Curtdate1;
                //DateTime Curtdate2;
                
                // vendorname = (bug.GetPluginField(PLUGIN_ID, "ixGlVendor")).ToString().Trim();
                // currency = (bug.GetPluginField(PLUGIN_ID, "ixGlTrxCurrency")).ToString().Trim();
                // Postperiod = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sPostingPeriod"));

                
                // Curtdate1 = DateTime.Now.AddMonths(1);
                // Curtdate2 = DateTime.Now.AddMonths(-1);

                // int diff = Postperiod.Year - DateTime.Now.Year;
                
                //int diff1 =  Postperiod.Year-Curtdate1.Year; -1 12 

                //int diff2 =  Postperiod.Year-Curtdate2.Year; 1 

                            
                // //api.Notifications.AddMessage("vname"+vendorname);
                //// api.Notifications.AddMessage("curny" + currency);
                // if (vendorname == "-")
                // {
                //     api.Notifications.AddMessage("Please select vendor Name");
                //     bug.ixStatus = 127;
                // }
                // else if (currency == "-")
                // {
                //     api.Notifications.AddMessage("Please select currency");
                //     bug.ixStatus = 127;
                // }


                // else if (diff1 == 0 && Postperiod.Month > Curtdate1.Month)
                // {
                //     api.Notifications.AddMessage("Posting period can only be current month, prior month or next month");
                //     api.Notifications.AddAdminNotification("1","1");
                //     bug.ixStatus = 127;
                // }

                // else if (diff1 == 0 &&   Postperiod.Month < Curtdate2.Month)
                // {
                //     api.Notifications.AddMessage("Posting period can only be current month, prior month or next month");
                //     api.Notifications.AddAdminNotification("2", "2");
                //     bug.ixStatus = 127;
                // }

                // else if (diff == 1 &&  Postperiod.Month!=01)
                //{
                //    api.Notifications.AddMessage("Posting period can only be current month, prior month or next month");
                //    api.Notifications.AddAdminNotification("3", "3");
                //    bug.ixStatus = 127;
                // }

                // else if (diff1 == -1 &&   Postperiod.Month != 12)
                // {
                //     api.Notifications.AddMessage("Posting period can only be current month, prior month or next month");
                //     api.Notifications.AddAdminNotification("4", "4");
                //     bug.ixStatus = 127;
                // }
                // else if (diff < -1 && diff > 1)
                // {
                //     api.Notifications.AddMessage("Posting period can only be current month, prior month or next month");
                //     api.Notifications.AddAdminNotification("5", "5");
                //     bug.ixStatus = 127;
                // }

                 //else
                 //{

                     if (bInvoiceAndLineItemsAmountMatches)
                     {
                         string sIntacctKey = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey"));
                         //If there is no entry in Intacct
                         if (string.IsNullOrEmpty(sIntacctKey))
                         {
                             PostToIntacct(bug, bugevent, IntacctActionType.CreateBill);
                         }
                         else
                         {
                             api.Notifications.AddError("Please be advised that this case has already been posted to Intacct. Any changes you make here related to the invoice, should be MANUALLY updated in Intacct too, as the process DICTATES you do so.");
                             //sganesh - 09/30/2010 - Turned off update
                             //PostToIntacct(bug, bugevent, IntacctActionType.UpdateBill);
                         }
                     }

                 //}
            }

            if (!string.IsNullOrEmpty(bugevent.sStatus) && (bug.ixStatus == 11 || bug.ixStatus == 12 || bug.ixStatus == 14 || bug.ixStatus == 21 || bug.ixStatus == 22 || bug.ixStatus == 54 || bug.ixStatus == 55))
            {
                int noOfApprovers = Convert.ToInt32(project.GetPluginField("IntacctSettings@conseroglobal.com", "sNumberOfClientApprovers"));

                if (noOfApprovers == 2)
                {
                    if (bug.ixStatus != 53)
                    {
                        //if the bug status is "pay this invoice", "released for payment", "ready for client review - Stage III or IV)
                        if (bug.ixStatus == 11 || bug.ixStatus == 22 || bug.ixStatus == 54 || bug.ixStatus == 55)
                        {
                            if (bug.ixStatus == 11)
                            {
                                api.Notifications.AddMessage("As this is a two stage approval process, only the final approver can 'Pay this invoice'. The system has the set the status to 'Ready For Client Approval - Stage II'.");
                            }

                            if (bug.ixStatus == 22)
                            {
                                api.Notifications.AddMessage("As this is a two stage approval process, only the final approver can 'Pay this invoice'. Once the invoice is marked to be paid in Intacct, the case is set to 'Released For Payment'. The system has the set the status to 'Ready For Client Approval - Stage II'.");
                            }

                            if (bug.ixStatus == 54 || bug.ixStatus == 55)
                            {
                                api.Notifications.AddMessage("As this is a two stage approval process, there are no more approval stages. The system has the set the status to 'Ready For Client Approval - Stage II'.");
                            }

                            //Reset the Status to "Ready For Client Review - Stage II";
                            bug.ixStatus = 53;

                        }
                    }
                }

                if (noOfApprovers == 3)
                {
                    if (bug.ixStatus != 54)
                    {
                        //if the bug status is "pay this invoice", "released for payment", "ready for client review - Stage IV)
                        if (bug.ixStatus == 11 || bug.ixStatus == 22 || bug.ixStatus == 55)
                        {
                            if (bug.ixStatus == 11)
                            {
                                api.Notifications.AddMessage("As this is a three stage approval process, only the final approver can 'Pay this invoice'. The system has the set the status to 'Ready For Client Approval - Stage III'.");
                            }

                            if (bug.ixStatus == 22)
                            {
                                api.Notifications.AddMessage("As this is a three stage approval process, only the final approver can 'Pay this invoice'. Once the invoice is marked to be paid in Intacct, the case is set to 'Released For Payment'. The system has the set the status to 'Ready For Client Approval - Stage III'.");
                            }

                            if (bug.ixStatus == 55)
                            {
                                api.Notifications.AddMessage("As this is a three stage approval process, there are no more approval stages. The system has the set the status to 'Ready For Client Approval - Stage III'.");
                            }
                            //Reset the Status to "Ready For Client Review - Stage III";
                            bug.ixStatus = 54;
                        }
                    }
                }

                if (noOfApprovers == 4)
                {
                    if (bug.ixStatus != 55)
                    {
                        //if the bug status is "pay this invoice", "released for payment", 
                        if (bug.ixStatus == 11 || bug.ixStatus == 22)
                        {
                            if (bug.ixStatus == 11)
                            {
                                api.Notifications.AddMessage("As this is a four stage approval process, only the final approver can 'Pay this invoice'. The system has the set the status to 'Ready For Client Approval - Stage III'.");
                            }

                            if (bug.ixStatus == 22)
                            {
                                api.Notifications.AddMessage("As this is a four stage approval process, only the final approver can 'Pay this invoice'. Once the invoice is marked to be paid in Intacct, the case is set to 'Released For Payment'. The system has the set the status to 'Ready For Client Approval - Stage III'.");
                            }

                            //Reset the Status to "Ready For Client Review - Stage IV";
                            bug.ixStatus = 55;
                        }
                    }
                }
            }

            //On Resolution
            if (!string.IsNullOrEmpty(bugevent.sStatus) && (bug.ixStatus == 11 || bug.ixStatus == 12 || bug.ixStatus == 14 || bug.ixStatus == 21))
            {
                switch (bug.ixStatus)
                {
                    case 11: //pay this invoice
                        ExtractValue(bug, bugevent, "ixGlBankAccount", "Bank Account",1);
                        ExtractValue(bug, bugevent, "ixGlPaymentMethod", "Payment Method",1);
                        PostToIntacct(bug, bugevent, IntacctActionType.PayBill);
                        break;
                    case 12: //don't pay this invoice
                        PostToIntacct(bug, bugevent, IntacctActionType.ReverseBill);
                        break;
                    case 14: //Place this invoice ON HOLD
                        PostToIntacct(bug, bugevent, IntacctActionType.PlaceBillOnHold);
                        break;
                    case 21: //Duplicate Bill
                        PostToIntacct(bug, bugevent, IntacctActionType.DuplicateBill);
                        break;
                }
            }

           

            # region Silverback workflow begin


            {
               //capturing  and storing atlevel and levels
                CProject project1 = api.Project.GetProject(bug.ixProject);
                string enableIntacct1 = Convert.ToString(project1.GetPluginField("IntacctSettings@conseroglobal.com", "sEnableIntacct"));
                if (string.IsNullOrEmpty(enableIntacct1) || "0".Equals(enableIntacct1))
                {
                    //api.Notifications.AddMessage("Intacct Not Enable For This Project.");
                    //don't do any intacct calls
                    return;
                }
               
                    //if ((nBugAction == BugAction.Edit && bug.ixStatus == 121) ||(nBugAction == BugAction.Assign && bug.ixStatus == 121) )
                if (nBugAction == BugAction.Edit || nBugAction == BugAction.Assign)
                {
                    if (bug.ixStatus == 121)
                    {
                        string sL1e = "-";
                        string sL2e = "-";
                        string sL3e = "-";
                        string sL4e = "-";
                        int atlevel = 0;

                        string Assignedto = "";

                       {
                            sL1e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl1")).ToString().Trim();
                            sL2e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl2")).ToString().Trim();
                            sL3e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl3")).ToString().Trim();
                            sL4e = (bug.GetPluginField(PLUGIN_ID, "CWFApproverl4")).ToString().Trim();
                            
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
                            // bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                            bug.ixStatus = 127;
                            return;
                        }


                        if ((sL4e != "-" && sL3e == "-") || (sL3e != "-" && sL2e == "-") || (sL2e != "-" && sL1e == "-"))
                        {
                            this.api.Notifications.AddMessage("Please set the approval sequence properly ");
                            //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                            bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                            this.api.Notifications.AddMessage("email is not sent to the approvers ");
                            bug.ixStatus = 127;
                            return;
                        }

                        if ((sL1e != "-" && (sL1e == sL2e || sL1e == sL3e || sL1e == sL4e)) ||
                            (sL2e != "-" && (sL2e == sL3e || sL1e == sL4e)) ||
                            (sL3e != "-" && (sL3e == sL4e)))
                        {

                            this.api.Notifications.AddMessage("Improper approval sequence- make sure no approvers are repeated in the sequence");
                            //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                            bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                            this.api.Notifications.AddMessage("email is not sent to the approvers ");
                            bug.ixStatus = 127;
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
                                bug.ixStatus = 127;
                                return;
                            }


                            // updating atlevel
                            string tablename1 = api.Database.PluginTableName("BugInvoice");
                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", atlevel);

                        } //updating atlevel ends here

                        // Duplicate invoice checks
                        {
                            int i = 0;

                            //string Vendor_Name = "-";
                            string mailsub = "", mailbody = "";
                            int iperson = 0;
                            string Invoice_no = "-";
                            int old_inv_bug = 0;
                            {
                                this.api.Notifications.AddMessage("1");
                                string vendor_1 = (bug.GetPluginField(PLUGIN_ID, "ixGlVendor")).ToString().Trim();
                                string InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();
                                string vendorname_1 = (bug.GetPluginField(PLUGIN_ID, "VendorName")).ToString().Trim();

                                CSelectQuery Dupcheck2 = api.Database.NewSelectQuery(api.Database.PluginTableName("PostToIntacct@conseroglobal.com", "BugInvoice"));
                                Dupcheck2.AddSelect("ixbug,ixGlVendor,sInvoiceNumber");
                                Dupcheck2.AddWhere("ixGlVendor = " + "'" + vendor_1 + "'");
                                Dupcheck2.AddWhere("sInvoiceNumber = " + "'" + InvNo_1 + "'");
                                Dupcheck2.AddWhere("ixbug > " + bug.ixBug.ToString() + " OR ixbug < " + bug.ixBug.ToString());
                                // this.api.Notifications.AddMessage("DUPLICATE");
                                DataSet d_1 = Dupcheck2.GetDataSet();

                                if (null != d_1.Tables && d_1.Tables.Count == 1 && d_1.Tables[0].Rows.Count > 0)
                                {
                                    int vid = Convert.ToInt32(d_1.Tables[0].Rows[0]["ixGlVendor"]);
                                    Invoice_no = Convert.ToString(d_1.Tables[0].Rows[0]["sInvoiceNumber"]);
                                    old_inv_bug = Convert.ToInt32(d_1.Tables[0].Rows[0]["ixbug"]);

                                    string VendName = GetVendorName(vid, 3);

                                    this.api.Notifications.AddError("--------------------------------------------------------------------------");
                                    this.api.Notifications.AddError("***DUPLICATE BILL****");
                                    this.api.Notifications.AddMessage("It seems an Invoice is already existing for the same vendor with case Id " + old_inv_bug);
                                    this.api.Notifications.AddMessage("Please verify the details");
                                    this.api.Notifications.AddError("-------------------------------------------------------------------------");

                                    mailsub = "Duplicate Invoice for Silverback in AP Workflow";
                                    mailbody = "It seems same invoice number " + InvNo_1 + " is already existing for the vendor " + VendName;
                                    iperson = bug.ixPersonAssignedTo;
                                    //mailsender("sunil.r@conseroglobal.com", bug, mailsub, mailbody, iperson);
                                    mailsender("poornima.r@conseroglobal.com", bug, mailsub, mailbody, iperson);

                                    i = 1;
                                    bug.ixStatus = 127;
                                    return;
                                }
                            }
                          
                            {

                                string vendor_1 = (bug.GetPluginField(PLUGIN_ID, "ixGlVendor")).ToString().Trim();
                                string vendorname_1 = (bug.GetPluginField(PLUGIN_ID, "VendorName")).ToString().Trim();
                                string InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();
                                CPersonQuery pers = api.Person.NewPersonQuery();
                                pers.IgnorePermissions = true;
                                pers.AddSelect("*");
                                pers.AddWhere(" Person.ixPerson = " + bug.ixPersonAssignedTo.ToString());
                                DataSet Dpers = pers.GetDataSet();

                                if (Dpers.Tables.Count > 0 && Dpers.Tables[0] != null && Dpers.Tables[0].Rows.Count > 0)
                                {
                                    string semail1 = Convert.ToString(Dpers.Tables[0].Rows[0]["sEmail"]);

                                    //mailsub = "Invoice '" + Vendname + "-" + invoiceno + "' awaiting your approval";
                                    mailsub = "An Invoice is awaiting your approval for vendor:" + vendorname_1 + " Invoice:" + InvNo_1;
                                    mailbody = "There is an invoice requiring your attention.  Please log in here to see the details http://empower.conseroglobal.com/default.asp?" + bug.ixBug;
                                    iperson = bug.ixPersonAssignedTo;
                                    
                                    mailsender(semail1, bug, mailsub, mailbody, iperson);
                                    this.api.Notifications.AddMessage("A mail has been sent to the approver Successfully");
                                    if ((bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")) != null)
                                    {
                                        if (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013").ToString() != "")
                                        {
                                            string CCemail = (bug.GetPluginField("customfields@fogcreek.com", "emailxccx013")).ToString();
                                            mailsender(CCemail.Trim(), bug, mailsub, mailbody, iperson);
                                            this.api.Notifications.AddMessage("A CCEmail has been sent Successfully");
                                        }
                                    }
                                }
                              
                            }

                           

                        } // sending emails ends here
                                                                      
                    }

                }

                    if (nBugAction == BugAction.Resolve)
                    {
                        int iperson = 0;
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
                        string L1mail = "-";
                        string L2mail = "-";
                        string L3mail = "-";
                        string L4mail = "-";

                        // fetching approvers details

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

                                if (bug.ixProject == 12 || bug.ixProject == 15 || bug.ixProject == 17 || bug.ixProject == 21)
                                {
                                    //RL1mail = 157;
                                    Lmail ="pradeep.g@conseroglobal.com";

                                }
                                else
                                {
                                    Lmail = Convert.ToString(Dpers1.Tables[0].Rows[0]["sEmail"]);
                                }
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
                        //pending approval
                        if (bug.ixStatus == 118)
                        {
                            // this.api.Notifications.AddMessage("resolve-1");

                            //string vendor_1 = (bug.GetPluginField(PLUGIN_ID, "ixGlVendor")).ToString().Trim();
                            int vendor_1 = Convert.ToInt32((bug.GetPluginField(PLUGIN_ID, "ixGlVendor")).ToString().Trim());
                            string InvNo_1 = (bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")).ToString().Trim();
                            string vendorname_1 = ((bug.GetPluginField(PLUGIN_ID, "VendorName")).ToString().Trim()).ToString();

                            string Appr_mailsub = "An Invoice is awaiting your approval for vendor:" + vendorname_1 + " Invoice:" + InvNo_1;
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
                                        //this.api.Notifications.AddMessage("L1 level");
                                        //this.api.Notifications.AddMessage("5");
                                        this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                        bug.ixPersonAssignedTo = L2;
                                        bug.ixStatus = 121;
                                        //updating atlevel
                                        //   string tablename1 = api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 3);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);
                                        //this.api.Notifications.AddMessage("L1 level" + bug.ixPersonAssignedTo);
                                        mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                        //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                    }
                                    else
                                    {
                                        // this.api.Notifications.AddMessage("resolve-l1");
                                        //  this.api.Notifications.AddMessage("L2_1 level");
                                        //  this.api.Notifications.AddMessage("31");
                                        this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requester");
                                        //  string tablename1 = api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "CGSInvoice_MLA");
                                        // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                        // Update1_1.UpdateInt("ixAtlevel", 5);

                                        // this.api.Notifications.AddMessage("assgined to email||" + Lmail);

                                        bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                        if (bug.ixProject == 11 || bug.ixProject == 13 || bug.ixProject == 16)
                                        {
                                            //RL1mail = 157;
                                            this.api.Notifications.AddAdminNotification("Kokila1","1");
                                            mailsender("kokila.n@conseroglobal.com", bug, Proc_mailsub, Proc_mailbody, iperson);

                                        }
                                        else
                                        {

                                            mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                        }
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
                                            //this.api.Notifications.AddMessage("L2 level");
                                            //this.api.Notifications.AddMessage("5");
                                            this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                            bug.ixPersonAssignedTo = L3;
                                            bug.ixStatus = 121;
                                            //updating atlevel
                                            // string tablename1 = api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "CGSInvoice_MLA");
                                            // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);
                                            //this.api.Notifications.AddMessage("L2 level" + bug.ixPersonAssignedTo);
                                            mailsender(L3mail, bug, Appr_mailsub, Appr_mailbody, iperson);

                                            //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                        }
                                        else
                                        {
                                            //  this.api.Notifications.AddMessage("L2_1 level");
                                            //  this.api.Notifications.AddMessage("31");
                                            this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requester");
                                            // string tablename1 = api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "CGSInvoice_MLA");
                                            //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                            // Update1_1.UpdateInt("ixAtlevel", 5);

                                            bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                            // this.api.Notifications.AddMessage("ixPersonLastEditedBy||" + bug.ixPersonLastEditedBy);
                                            // this.api.Notifications.AddMessage("assgined to email||" + Lmail);
                                            if (bug.ixProject == 11 || bug.ixProject == 13 || bug.ixProject == 16)
                                            {
                                                //RL1mail = 157;
                                                this.api.Notifications.AddAdminNotification("Kokila2", "2");
                                                mailsender("kokila.n@conseroglobal.com", bug, Proc_mailsub, Proc_mailbody, iperson);

                                            }
                                            else
                                            {
                                                mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                            }
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
                                                this.api.Notifications.AddMessage("The Invoice has been approved and assigned to next approver successfully");
                                                //  this.api.Notifications.AddMessage("SL4|" + sL4);
                                                // this.api.Notifications.AddMessage("L4|" + L4);
                                                bug.ixPersonAssignedTo = L4;
                                                bug.ixStatus = 121;

                                                // string tablename1 = api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //Update1_1.UpdateInt("ixAtlevel", 4);
                                               // this.api.Notifications.AddMessage("L4 level" + bug.ixPersonAssignedTo);
                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);
                                                mailsender(L4mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                                //mailsender(L2mail, bug, Appr_mailsub, Appr_mailbody, iperson);
                                            }
                                            else
                                            {
                                                //  this.api.Notifications.AddMessage("L3_1 level");
                                                // this.api.Notifications.AddMessage("8");
                                                this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requester");

                                                // string tablename1 = api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "CGSInvoice_MLA");
                                                // CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                // Update1_1.UpdateInt("ixAtlevel", 5);

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                                if (bug.ixProject == 11 || bug.ixProject == 13 || bug.ixProject == 16)
                                                {
                                                    //RL1mail = 157;
                                                    this.api.Notifications.AddAdminNotification("Kokila3", "3");
                                                    mailsender("kokila.n@conseroglobal.com", bug, Proc_mailsub, Proc_mailbody, iperson);

                                                }
                                                else
                                                {
                                                    mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                }
                                                //bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;
                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                //this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);
                                            }
                                        }


                                        else if (sL4 != "-")
                                        {

                                            if (L4 == L0)
                                            {
                                                // this.api.Notifications.AddMessage("L4 level");

                                                // this.api.Notifications.AddMessage("9");
                                                this.api.Notifications.AddMessage("The Invoice has been approved successfully and an email notification sent to the requester");

                                                // string tablename1 = api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "CGSInvoice_MLA");
                                                //  CUpdateQuery Update1_1 = api.Database.NewUpdateQuery(tablename1);
                                                //  Update1_1.UpdateInt("ixAtlevel", 5);
                                               

                                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 5);
                                                if (bug.ixProject == 11 || bug.ixProject == 13 || bug.ixProject == 16)
                                                {
                                                    //RL1mail = 157;
                                                    this.api.Notifications.AddAdminNotification("Kokila4", "4");
                                                    mailsender("kokila.n@conseroglobal.com", bug, Proc_mailsub, Proc_mailbody, iperson);

                                                }
                                                else
                                                {

                                                    mailsender(Lmail, bug, Proc_mailsub, Proc_mailbody, iperson);
                                                }
                                                //  bug.ixPersonAssignedTo = bug.ixPersonLastEditedBy;

                                                bug.ixPersonAssignedTo = bug.ixPersonOpenedBy;
                                                this.api.Notifications.AddMessage("opener" + bug.ixPersonAssignedTo);

                                            }
                                        }
                                    }
                                }
                            }
                           
                        }
                        
                        // For Rejection 

                        if (bug.ixStatus == 124)
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
                                bug.ixStatus = 121;

                                return;
                            }

                            string RL1mail = "-";
                            string Rej_mailsub = "An Invoice has been Rejected";
                            string Rej_mailbody = "There is an invoice which has been Rejected.  Please log in here to see the details: http://empower.conseroglobal.com/default.asp?" + bug.ixBug; ;

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

                                if (bug.ixProject == 12 || bug.ixProject == 15 || bug.ixProject == 17 || bug.ixProject == 21)
                                {
                                    //RL1mail = 157;
                                    mailsender("pradeep.g@conseroglobal.com", bug, Rej_mailsub, Rej_mailbody, iperson);

                                }

                               else if (bug.ixProject == 11 || bug.ixProject == 13 || bug.ixProject == 16)
                                {
                                    //RL1mail = 157;
                                    this.api.Notifications.AddAdminNotification("Kokila5", "5");
                                    mailsender("kokila.n@conseroglobal.com", bug, Rej_mailsub, Rej_mailbody, iperson);

                                }

                                else
                                {
                                    mailsender(RL1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                }
                                mailsender("chandrasekhar.y@conseroglobal.com,parashurama.r@conseroglobal.com", bug, Rej_mailsub, Rej_mailbody, iperson);
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
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requester");

                            }

                            if (L0 == L2)
                            {
                                //api.Notifications.AddMessage("executed at L2 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 2);

                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                //mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requester");


                            }


                            else if (L0 == L3)
                            {
                                // api.Notifications.AddMessage("executed at L3 level");

                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 3);

                                //   mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requester");
                            }
                                                                
                            else if (L0 == L4)
                            {
                                bug.SetPluginField(PLUGIN_ID, "ixAtlevel", 4);
                                api.Notifications.AddMessage("executed at L$ level");
                                                                                                
                                //   api.Notifications.AddMessage("executed at L4 level");
                                // this.api.Notifications.AddMessage("Fourth Approver| " + L1mail + "||" + L2mail + "||" + L3mail);
                                //  mailsender(Lmail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L1mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L2mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                mailsender(L3mail, bug, Rej_mailsub, Rej_mailbody, iperson);
                                this.api.Notifications.AddMessage("The Invoice has been rejected successfully and an email notification sent to the requester");

                            }

                        }

                    }

                

            }
            #endregion
        }

        private void PostToIntacct(CBug bug, CBugEvent bugevent, IntacctActionType intacctActionType)
        {
            CProject project = api.Project.GetProject(bug.ixProject);
            string enableIntacct = Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sEnableIntacct"));
            if (string.IsNullOrEmpty(enableIntacct) || "0".Equals(enableIntacct))
            {
                //don't do any intacct calls
                return;
            }

            XmlWriterSettings wSettings = new XmlWriterSettings();
            wSettings.Indent = true;
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
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctSenderId")));
            xw.WriteEndElement();
            xw.WriteStartElement("password");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctSenderPassword")));
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
            xw.WriteStartAttribute("transaction");
            xw.WriteString("false");
            xw.WriteEndAttribute();

            xw.WriteStartElement("authentication");
            xw.WriteStartElement("login");
            xw.WriteStartElement("userid");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctUserId")));
            xw.WriteEndElement();
            xw.WriteStartElement("companyid");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctCompanyId")));
            xw.WriteEndElement();
            xw.WriteStartElement("password");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctUserPassword")));
            xw.WriteEndElement();
            xw.WriteStartElement("locationid");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctLocationId")));
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();

            api.Notifications.AddMessage(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctLocationId")));

            switch (intacctActionType)
            {
                case IntacctActionType.CreateBill: CreateBill(bug, xw); break;
                case IntacctActionType.UpdateBill: UpdateBill(bug, xw); break;
                case IntacctActionType.PayBill: PayBill(bug, xw); break;
                case IntacctActionType.PlaceBillOnHold: PlaceBillOnHold(bug, xw, "Place This Bill On Hold"); break;
                case IntacctActionType.DuplicateBill: PlaceBillOnHold(bug, xw, "Is A Duplicate Bill"); DuplicateBill(bug, xw); break;
                case IntacctActionType.ReverseBill: PlaceBillOnHold(bug, xw, "Don't Pay This Bill"); ReverseBill(bug, xw); break;
            }

            //operation ends here
            xw.WriteEndElement();

            //request ends here
            xw.WriteEndElement();

            // Close the document
            xw.WriteEndDocument();

            // Flush the write
            xw.Flush();

            Byte[] buffer = new Byte[ms.Length];
            buffer = ms.ToArray();
            //bugevent.AppendChangeLine("Xml Posted To Intacct :: " + System.Text.Encoding.UTF8.GetString(buffer));

         

            XmlDocument xmlResponse = PostXMLTransaction(project, ms);
            bool responseSuccess = false;
            if (null != xmlResponse)
            {
                responseSuccess = true;
                switch (intacctActionType)
                {
                    case IntacctActionType.PayBill:
                        bugevent.AppendChangeLine("A Payment Request was successfully created in Intacct");
                        bug.ixStatus = 22;
                        break;

                    case IntacctActionType.CreateBill:
                    
                        bugevent.AppendChangeLine("A Bill was successfully created in Intacct");
                        bug.ixStatus = 27;

                        try
                        {
                           // api.Notifications.AddMessage(" intacct response4 checking error no1");
                            XmlNode intacctKeyNode3 = xmlResponse.SelectSingleNode("/response/errormessage/error/errorno");
                            api.Notifications.AddMessage(" intacct response4 " + intacctKeyNode3.InnerText.ToString());
                           // api.Notifications.AddMessage(" intacct response4 checking error no2");

                            XmlNode intacctKeyNode2 = xmlResponse.SelectSingleNode("/response/errormessage/error/description");
                            api.Notifications.AddMessage(" intacct response3 " + intacctKeyNode2.InnerText.ToString());
                            
                            
                        }
                        catch
                        {
                        }


                        XmlNode intacctKeyNode = xmlResponse.SelectSingleNode("/response/operation/result/key");
                        //XmlNode intacctKeyNode = xmlResponse.SelectSingleNode("/response/operation/result/bill/key");
                        api.Notifications.AddMessage(" intacctkey " + intacctKeyNode.InnerText);
                                                                       
                        if (null != intacctKeyNode)
                        {
                           
                            string intacctKey = intacctKeyNode.InnerText;
                            bugevent.AppendChangeLine("Intacct Bill Key is " + intacctKey);
                            bug.SetPluginField(PLUGIN_ID, "iIntacctKey", intacctKey);
                        }
                        break;

                    case IntacctActionType.UpdateBill:
                        bugevent.AppendChangeLine("The Bill was successfully updated in Intacct");
                        //bugevent.AppendChangeLine(xmlResponse.InnerText);
                        bug.ixStatus = 27;
                        break;
                }
            }

            if (!responseSuccess)
            {
                switch (intacctActionType)
                {
                    case IntacctActionType.PayBill: bug.ixStatus = 27; break;
                    case IntacctActionType.CreateBill: bug.ixStatus = 20; break;
                    case IntacctActionType.UpdateBill: bug.ixStatus = 20; break;
                }
                api.Notifications.AddError("Error while posting to Intacct. Please try again. ");
            }
        }

        private void CreateBill(CBug bug, XmlWriter xw)
        {
            DataSet dsItems = FetchItems(bug.ixBug, true);

            
            // Content Begins Here
            xw.WriteStartElement("content");

            //Function Starts Here
            xw.WriteStartElement("function");
            xw.WriteStartAttribute("controlid");
            xw.WriteString(bug.ixBug.ToString());
            xw.WriteEndAttribute();

            //Create Bill Starts Here
            xw.WriteStartElement("create_bill");

            //vendor id
            xw.WriteStartElement("vendorid");
            xw.WriteString(GetIntacctVendorId(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlVendor"))));
            xw.WriteEndElement();

          
            //date created
            xw.WriteStartElement("datecreated");
            DateTime invoiceDate = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sInvoiceDate"));
            xw.WriteStartElement("year");
            xw.WriteString(invoiceDate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(invoiceDate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(invoiceDate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();

            xw.WriteStartElement("dateposted");
            DateTime dateposted = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sPostingPeriod"));
            xw.WriteStartElement("year");
            xw.WriteString(dateposted.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(dateposted.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(dateposted.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();


            //date due
            xw.WriteStartElement("datedue");
            DateTime dueDate = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sDueDate"));
            xw.WriteStartElement("year");
            xw.WriteString(dueDate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(dueDate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(dueDate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();

            //Net Terms
            xw.WriteStartElement("termname");
            xw.WriteString(GetNetTerms(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlNetTerm"))));
            xw.WriteEndElement();

            //invoice Number
            xw.WriteStartElement("billno");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")));
            xw.WriteEndElement();

            //PO Number
            xw.WriteStartElement("ponumber");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sPONumber")));
            xw.WriteEndElement();

                   
            //Description
            xw.WriteStartElement("description");
            xw.WriteString("<a target='_blank' href = 'http://empower.conseroglobal.com/default.asp?" + bug.ixBug.ToString() + "'> Empower Case " + bug.ixBug.ToString() + "</a>");
            xw.WriteEndElement();

            //xw.WriteStartElement("basecurr");
            //xw.WriteString("USD");
            // xw.WriteString(GetIntacctTrxCurrency(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlTrxCurrency"))));
            //xw.WriteEndElement();

           xw.WriteStartElement("currency");
           // xw.WriteString("USD");
           xw.WriteString(GetIntacctTrxCurrency(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlTrxCurrency"))));
            xw.WriteEndElement();

          
           //exchange date 
            xw.WriteStartElement("exchratedate");
            DateTime exchdate = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sExchratedate"));
            xw.WriteStartElement("year");
            xw.WriteString(exchdate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(exchdate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(exchdate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();
            
            //api.Notifications.AddMessage("echange rate type " + GetIntacctExchratetype(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlExchratetype"))));
           xw.WriteStartElement("exchratetype");
          //  xw.WriteString("Intacct Daily Rate");
            xw.WriteString(GetIntacctExchratetype(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlExchratetype"))));
            xw.WriteEndElement();

            xw.WriteStartElement("customfields");
            xw.WriteStartElement("customfield");
            //  api.Notifications.AddMessage("1af1");
            //xw.WriteEndElement();
            xw.WriteStartElement("customfieldname");
            xw.WriteString("Bill_status");
            // api.Notifications.AddMessage("1af2");
            xw.WriteEndElement();
            xw.WriteStartElement("customfieldvalue");
            // api.Notifications.AddMessage("1af3");
            xw.WriteString("Open");
            //  api.Notifications.AddMessage("1af4");

            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();
            //   xw.WriteStartElement(

         //   xw.WriteStartElement("exchrate");
           //  xw.WriteString("1");
           //  xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sExchrate")));
             //xw.WriteString(GetIntacctTrxCurrency(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sExchrate"))));
            // xw.WriteEndElement();

            string sMemo = "";
            double dAmount = -1D;
            string sGlAccountName = "";
            string sGlDepartmentName = "";
            string sGlLocationName = "";
            string sGlProjectName = "";
            string sGlClassName = "";
            string sGlItemName = "";

            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */
            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                xw.WriteStartElement("billitems");
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {
                    xw.WriteStartElement("lineitem");
                    sGlAccountName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlAccountId"]);
                    if (!string.IsNullOrEmpty(sGlAccountName))
                    {
                        xw.WriteStartElement("glaccountno");
                        xw.WriteString(sGlAccountName);
                        xw.WriteEndElement();
                    }

                    try
                    {
                        dAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                    }
                    catch
                    {
                        dAmount = 0d;
                    }
                    xw.WriteStartElement("amount");
                    xw.WriteString(dAmount.ToString());
                    xw.WriteEndElement();

                    sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                    if (!string.IsNullOrEmpty(sMemo))
                    {
                        xw.WriteStartElement("memo");
                        xw.WriteString(sMemo);
                        xw.WriteEndElement();
                    }

                    sGlLocationName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlLocationId"]);
                    if (!string.IsNullOrEmpty(sGlLocationName))
                    {
                        xw.WriteStartElement("locationid");
                        xw.WriteString(sGlLocationName);
                        xw.WriteEndElement();
                    }

                    sGlDepartmentName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlDepartmentId"]);
                    if (!string.IsNullOrEmpty(sGlDepartmentName))
                    {
                        xw.WriteStartElement("departmentid");
                        xw.WriteString(sGlDepartmentName);
                        xw.WriteEndElement();
                    }

                    sGlProjectName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlProjectId"]);
                    if (!string.IsNullOrEmpty(sGlProjectName))
                    {
                        xw.WriteStartElement("projectid");
                        xw.WriteString(sGlProjectName);
                        xw.WriteEndElement();
                    }

                    //sGlItemName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlItemId"]);
                    //if (!string.IsNullOrEmpty(sGlItemName))
                    //{
                    //    xw.WriteStartElement("itemid");
                    //    xw.WriteString(sGlItemName);
                    //    xw.WriteEndElement();
                    //}

                    sGlClassName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlClassId"]);
                    if (!string.IsNullOrEmpty(sGlClassName))
                    {
                        xw.WriteStartElement("classid");
                        xw.WriteString(sGlClassName);
                        xw.WriteEndElement();
                    }

                    xw.WriteEndElement();
                }
                xw.WriteEndElement();
            }

            //create bill ends here
            xw.WriteEndElement();

            //function ends here
            xw.WriteEndElement();

            //content ends here
            xw.WriteEndElement();
        }

        private void UpdateBill(CBug bug, XmlWriter xw)
        {
            DataSet dsItems = FetchItems(bug.ixBug, false);

            // Content Begins Here
            xw.WriteStartElement("content");

            //Function Starts Here
            xw.WriteStartElement("function");
            xw.WriteStartAttribute("controlid");
            xw.WriteString(bug.ixBug.ToString());
            xw.WriteEndAttribute();

            //Update Bill Starts Here
            xw.WriteStartElement("update_bill");

            //Intacct Key
            xw.WriteStartAttribute("key");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey")));
            xw.WriteEndAttribute();

            //vendor id
            xw.WriteStartElement("vendorid");
            xw.WriteString(GetIntacctVendorId(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlVendor"))));
            xw.WriteEndElement();

            xw.WriteStartElement("currency");
            xw.WriteString(GetIntacctTrxCurrency(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlTrxCurrency"))));
            xw.WriteEndElement();

            //date created
            xw.WriteStartElement("datecreated");
            DateTime invoiceDate = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sInvoiceDate"));
            xw.WriteStartElement("year");
            xw.WriteString(invoiceDate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(invoiceDate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(invoiceDate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();

            //date due
            xw.WriteStartElement("datedue");
            DateTime dueDate = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sDueDate"));
            xw.WriteStartElement("year");
            xw.WriteString(dueDate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(dueDate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(dueDate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();

            //Net Terms
            xw.WriteStartElement("termname");
            xw.WriteString(GetNetTerms(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlNetTerm"))));
            xw.WriteEndElement();

            //invoice Number
            xw.WriteStartElement("billno");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sInvoiceNumber")));
            xw.WriteEndElement();

            //PO Number
            xw.WriteStartElement("ponumber");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sPONumber")));
            xw.WriteEndElement();

            //Description
            xw.WriteStartElement("description");
            xw.WriteString("<a target='_blank' href = 'http://empower.conseroglobal.com/default.asp?" + bug.ixBug.ToString() + "'> Empower Case " + bug.ixBug.ToString() + "</a>");
            xw.WriteEndElement();


            //exchange date 
            xw.WriteStartElement("exchratedate");
            DateTime exchdate = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sExchratedate"));
            xw.WriteStartElement("year");
            xw.WriteString(exchdate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(exchdate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(exchdate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();

            xw.WriteStartElement("exchratetype");
            // xw.WriteString("Intacct Daily Rate");
            xw.WriteString(GetIntacctExchratetype(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlExchratetype"))));
            xw.WriteEndElement();

           // xw.WriteStartElement("exchrate");
            //  xw.WriteString("1");
          //  xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sExchrate")));
            //xw.WriteString(GetIntacctTrxCurrency(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sExchrate"))));
           // xw.WriteEndElement();

            int iDeleted = 0;
            int iDeletedCount = 0;
            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {
                    iDeleted = Convert.ToInt32(dsItems.Tables[0].Rows[i]["iDeleted"]);
                    if (1 == iDeleted)
                    {
                        iDeletedCount++;
                    }
                }
            }

            if (iDeletedCount > 0)
            {
                string sMemo = "";
                double dAmount = -1D;
                string sGlAccountName = "";
                string sGlDepartmentName = "";
                string sGlLocationName = "";
                string sGlProjectName = "";
                string sGlClassName = "";
                string sGlItemName = "";

                /* If the DataSet contains any rows, loop through them and populate the table
                 * and dialog template data Hashtables */
                int iLineItemCount = 1;
                if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
                {
                    xw.WriteStartElement("updatebillitems");
                    for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                    {
                        iDeleted = Convert.ToInt32(dsItems.Tables[0].Rows[i]["iDeleted"]);
                        if (0 == iDeleted)
                        {

                            xw.WriteStartElement("lineitem");
                            sGlAccountName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlAccountId"]);
                            if (!string.IsNullOrEmpty(sGlAccountName))
                            {
                                xw.WriteStartElement("glaccountno");
                                xw.WriteString(sGlAccountName);
                                xw.WriteEndElement();
                            }

                            try
                            {
                                dAmount = Convert.ToDouble(dsItems.Tables[0].Rows[i]["fAmount"]);
                            }
                            catch
                            {
                                dAmount = 0d;
                            }
                            xw.WriteStartElement("amount");
                            xw.WriteString(dAmount.ToString());
                            xw.WriteEndElement();

                            sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
                            if (!string.IsNullOrEmpty(sMemo))
                            {
                                xw.WriteStartElement("memo");
                                xw.WriteString(sMemo);
                                xw.WriteEndElement();
                            }

                            sGlLocationName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlLocationId"]);
                            if (!string.IsNullOrEmpty(sGlLocationName))
                            {
                                xw.WriteStartElement("locationid");
                                xw.WriteString(sGlLocationName);
                                xw.WriteEndElement();
                            }

                            sGlDepartmentName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlDepartmentId"]);
                            if (!string.IsNullOrEmpty(sGlDepartmentName))
                            {
                                xw.WriteStartElement("departmentid");
                                xw.WriteString(sGlDepartmentName);
                                xw.WriteEndElement();
                            }

                            sGlProjectName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlProjectId"]);
                            if (!string.IsNullOrEmpty(sGlProjectName))
                            {
                                xw.WriteStartElement("projectid");
                                xw.WriteString(sGlProjectName);
                                xw.WriteEndElement();
                            }

                            sGlItemName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlItemId"]);
                            if (!string.IsNullOrEmpty(sGlItemName))
                            {
                                xw.WriteStartElement("itemid");
                                xw.WriteString(sGlItemName);
                                xw.WriteEndElement();
                            }

                            sGlClassName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlClassId"]);
                            if (!string.IsNullOrEmpty(sGlClassName))
                            {
                                xw.WriteStartElement("classid");
                                xw.WriteString(sGlClassName);
                                xw.WriteEndElement();
                            }

                            xw.WriteEndElement();
                        }
                        else
                        {
                            xw.WriteStartElement("updatelineitem");
                            xw.WriteStartAttribute("line_num");
                            xw.WriteString(iLineItemCount.ToString());
                            xw.WriteEndAttribute();
                            xw.WriteStartElement("amount");
                            xw.WriteString("0");
                            xw.WriteEndElement();
                            xw.WriteEndElement();
                            iLineItemCount++;
                        }
                    }
                    xw.WriteEndElement();
                }
            }

            //create bill ends here
            xw.WriteEndElement();

            //function ends here
            xw.WriteEndElement();

            //content ends here
            xw.WriteEndElement();
        }

        private void PayBill(CBug bug, XmlWriter xw)
        {
            // Content Begins Here
            xw.WriteStartElement("content");
            xw.WriteStartElement("function");
            xw.WriteStartAttribute("controlid");
            xw.WriteString(bug.ixBug.ToString());
            xw.WriteEndAttribute();

            xw.WriteStartElement("create_paymentrequest");
            xw.WriteStartElement("bankaccountid");
            xw.WriteString(GetIntacctBankId(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlBankAccount"))));
            xw.WriteEndElement();

            xw.WriteStartElement("vendorid");
            xw.WriteString(GetIntacctVendorId(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlVendor"))));
            xw.WriteEndElement();

            xw.WriteStartElement("currency");
            xw.WriteString(GetIntacctTrxCurrency(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlTrxCurrency"))));
            xw.WriteEndElement();


            xw.WriteStartElement("paymentmethod");
            xw.WriteString(GetIntacctPaymentMethodId(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "ixGlPaymentMethod"))));
            xw.WriteEndElement();

            xw.WriteStartElement("paymentdate");
            DateTime paymentDate = Convert.ToDateTime(bug.GetPluginField(PLUGIN_ID, "sDueDate"));
            xw.WriteStartElement("year");
            xw.WriteString(paymentDate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(paymentDate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(paymentDate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();

            xw.WriteStartElement("paymentrequestitems");
            xw.WriteStartElement("paymentrequestitem");
            xw.WriteStartElement("key");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey")));
            xw.WriteEndElement();
            xw.WriteStartElement("paymentamount");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sBalanceDue")));
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();

            xw.WriteEndElement();

            //function ends here
            xw.WriteEndElement();
            //content ends here
            xw.WriteEndElement();
        }

        private void ReverseBill(CBug bug, XmlWriter xw)
        {
            // Content Begins Here
            xw.WriteStartElement("content");
            //Funcation Begins Here
            xw.WriteStartElement("function");
            xw.WriteStartAttribute("controlid");
            xw.WriteString("R" + bug.ixBug.ToString());
            xw.WriteEndAttribute();

            //Reverser Bill Begins Here
            xw.WriteStartElement("reverse_bill");
            xw.WriteStartAttribute("key");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey")));
            xw.WriteEndAttribute();

            //DateReversed
            xw.WriteStartElement("datereversed");
            DateTime reversedDate = DateTime.Now;
            xw.WriteStartElement("year");
            xw.WriteString(reversedDate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(reversedDate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(reversedDate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();

            //Description
            xw.WriteStartElement("description");
            xw.WriteString("<a target='_blank' href = 'http://empower.conseroglobal.com/default.asp?" + bug.ixBug.ToString() + "'> Empower Case " + bug.ixBug.ToString() + "</a> :: DON'T PAY THIS BILL");
            xw.WriteEndElement();

            //Reverse Bill Ends Here
            xw.WriteEndElement();

            //function ends here
            xw.WriteEndElement();
            //content ends here
            xw.WriteEndElement();
        }

        private void PlaceBillOnHold(CBug bug, XmlWriter xw, string description)
        {
            // Content Begins Here
            xw.WriteStartElement("content");
            //Function Begins Here
            xw.WriteStartElement("function");
            xw.WriteStartAttribute("controlid");
            xw.WriteString(bug.ixBug.ToString());
            xw.WriteEndAttribute();

            //Update Bill Begins Here
            xw.WriteStartElement("update_bill");
            xw.WriteStartAttribute("key");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey")));
            xw.WriteEndAttribute();

            xw.WriteStartElement("onhold");
            xw.WriteString("true");
            xw.WriteEndElement();

            xw.WriteStartElement("description");
            xw.WriteString("<a target='_blank' href = 'http://empower.conseroglobal.com/default.asp?" + bug.ixBug.ToString() + "'> Empower Case " + bug.ixBug.ToString() + "</a> :: " + description);
            xw.WriteEndElement();
            xw.WriteEndElement();

            //function ends here
            xw.WriteEndElement();
            //content ends here
            xw.WriteEndElement();
        }

        private void DuplicateBill(CBug bug, XmlWriter xw)
        {
            // Content Begins Here
            xw.WriteStartElement("content");
            xw.WriteStartElement("function");
            xw.WriteStartAttribute("controlid");
            xw.WriteString("D" + bug.ixBug.ToString());
            xw.WriteEndAttribute();

            xw.WriteStartElement("reverse_bill");
            xw.WriteStartAttribute("key");
            xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey")));
            xw.WriteEndAttribute();

            xw.WriteStartElement("reverseddate");
            DateTime reversedDate = DateTime.Now;
            xw.WriteStartElement("year");
            xw.WriteString(reversedDate.Year.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("month");
            xw.WriteString(reversedDate.Month.ToString());
            xw.WriteEndElement();
            xw.WriteStartElement("day");
            xw.WriteString(reversedDate.Day.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();

            xw.WriteStartElement("description");
            xw.WriteString("<a target='_blank' href = 'http://empower.conseroglobal.com/default.asp?" + bug.ixBug.ToString() + "'> Empower Case " + bug.ixBug.ToString() + "</a> - DUPLICATE BILL");
            xw.WriteEndElement();
            xw.WriteEndElement();

            //function ends here
            xw.WriteEndElement();
            //content ends here
            xw.WriteEndElement();
        }

        private void SetInvoiceDueDate(CBug bug)
        {
            DataSet ds = null;
            try
            {
                string sInvoiceDate = Convert.ToString(bug.GetPluginField(PLUGIN_ID, "sInvoiceDate"));
                if (!string.IsNullOrEmpty(sInvoiceDate))
                {
                    DateTime dInvoiceDate = Convert.ToDateTime(sInvoiceDate);
                    int ixNetTerms = Convert.ToInt32(bug.GetPluginField(PLUGIN_ID, "ixGlNetTerm"));

                    CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlNetTerm"));
                    sq.AddSelect("*");
                    sq.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlNetTerm") + ".ixGlNetTerm = " + ixNetTerms.ToString());

                    ds = sq.GetDataSet();
                    int iNetTerm = ixNetTerms;

                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                    {
                        iNetTerm = Convert.ToInt32(ds.Tables[0].Rows[0]["sGlNetTermId"].ToString());
                    }

                    dInvoiceDate = dInvoiceDate.AddDays(iNetTerm);

                    bug.SetPluginField(PLUGIN_ID, "sDueDate", dInvoiceDate.ToShortDateString());
                }
            }
            catch
            {
                bug.SetPluginField(PLUGIN_ID, "sDueDate", "ERROR");
            }
            finally
            {
                if (null != ds)
                {
                    ds.Dispose();
                }
            }
        }

        private bool ExtractValue(CBug bug, CBugEvent bugevent, string fieldName, string fieldDisplay, int Check)
        {
            bool valueChanged = false;

            string sNewValue = Convert.ToString(api.Request[api.AddPluginPrefix(fieldName)]);
            if (string.IsNullOrEmpty(sNewValue))
            {
                // Check is used to validate the fields for mandatory if check is 1 validates if 0 skips the validation
                if (Check == 1)
                {
                    api.Notifications.AddMessage(fieldDisplay + " is required.");
                    //bug.ixStatus = 20;
                    bug.ixStatus = 127;
                }
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

        private void ResetField(CBug bug, string fieldName, string previousFieldValue)
        {
            if (api.Request[api.AddPluginPrefix(fieldName)] != null &&
                Convert.ToString(api.Request[api.AddPluginPrefix(fieldName)]) != VARIOUS_TEXT)
                bug.SetPluginField(PLUGIN_ID, fieldName, previousFieldValue);
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
                return "";

            if (PluginFieldVaries(rgbug, fieldName))
                return VARIOUS_TEXT;
            else
                return Convert.ToString(rgbug[0].GetPluginField(PLUGIN_ID, fieldName));
        }

        #region IPluginDatabase Members

        public CTable[] DatabaseSchema()
        {
            /* for this plugin, we'll need a bug-to-code name table to allow for a join. */

            CTable bugInvoiceNumber = api.Database.NewTable(api.Database.PluginTableName("BugInvoice"));
            bugInvoiceNumber.sDesc = "Caputures Invoice Parameters";
            bugInvoiceNumber.AddAutoIncrementPrimaryKey("ixBugInvoiceNumber");
            bugInvoiceNumber.AddIntColumn("ixBug", true, 1);
            bugInvoiceNumber.AddVarcharColumn("sInvoiceNumber", 100, false);
            bugInvoiceNumber.AddDateColumn("sInvoiceDate", false);
            bugInvoiceNumber.AddIntColumn("ixGlNetTerm", false);
            bugInvoiceNumber.AddDateColumn("sDueDate", false);
            bugInvoiceNumber.AddVarcharColumn("sPONumber", 20, false);
            bugInvoiceNumber.AddIntColumn("ixGlVendor", false);
            bugInvoiceNumber.AddIntColumn("ixGlTrxCurrency", false);
            bugInvoiceNumber.AddIntColumn("ixGlExchratetype", false);
            bugInvoiceNumber.AddDateColumn("sExchratedate", false);
            bugInvoiceNumber.AddVarcharColumn("sExchrate", 200, false);
            bugInvoiceNumber.AddFloatColumn("sBalanceDue", false);
            bugInvoiceNumber.AddIntColumn("iIntacctKey", false);
            bugInvoiceNumber.AddIntColumn("ixGlBankAccount", false);
            bugInvoiceNumber.AddIntColumn("ixGlPaymentMethod", false);
            bugInvoiceNumber.AddVarcharColumn("CWFApproverl1", 200, false);
            bugInvoiceNumber.AddVarcharColumn("CWFApproverl2", 200, false);
            bugInvoiceNumber.AddVarcharColumn("CWFApproverl3", 200, false);
            bugInvoiceNumber.AddVarcharColumn("CWFApproverl4", 200, false);
            bugInvoiceNumber.AddIntColumn("ixAtlevel", false, 0);
            bugInvoiceNumber.AddIntColumn("ixlevel", false, 0);
            bugInvoiceNumber.AddVarcharColumn("VendorName", 200, false);
            bugInvoiceNumber.AddDateColumn("sPostingPeriod", false);
            

            CTable bugInvoiceItemsTable = api.Database.NewTable(api.Database.PluginTableName("BugInvoiceItem"));
            bugInvoiceItemsTable.sDesc = "A table full of bug invoice items.";
            bugInvoiceItemsTable.AddAutoIncrementPrimaryKey("ixBugInvoiceItem");
            bugInvoiceItemsTable.AddIntColumn("ixBug", true, 1);
            bugInvoiceItemsTable.AddIntColumn("ixGlAccount", false);
            //bugInvoiceItemsTable.AddIntColumn("iForm99", false);
            bugInvoiceItemsTable.AddFloatColumn("fAmount", false);
            bugInvoiceItemsTable.AddTextColumn("sMemo", "Memo Storage");
            bugInvoiceItemsTable.AddIntColumn("ixGlDepartment", false);
            bugInvoiceItemsTable.AddIntColumn("ixGlLocation", false);
            bugInvoiceItemsTable.AddIntColumn("ixGlProject", false);
            bugInvoiceItemsTable.AddIntColumn("ixGlItem", false);
            bugInvoiceItemsTable.AddIntColumn("ixGlClass", false);
            bugInvoiceItemsTable.AddIntColumn("iDeleted", false, 0);

            return new CTable[] { bugInvoiceNumber, bugInvoiceItemsTable };
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

          //  api.Notifications.AddAdminNotification("grid1", "1");

            CGridColumn gridCol1 = api.Grid.CreateGridColumn();
            /* the name displayed in the filter drop-down */
            gridCol1.sName = "Invoice Number";
            /* the column title in grid view */
            gridCol1.sTitle = "Invoice Number";
            /* every column you create needs to have a unique iType */
            gridCol1.iType = 0;

            CGridColumn gridCol2 = api.Grid.CreateGridColumn();
            gridCol2.sName = "Vendor name";
            gridCol2.sTitle = "Vendor name";
            /* every column you create needs to have a unique iType */
            gridCol2.iType = 1;

            CGridColumn gridCol3 = api.Grid.CreateGridColumn();
            gridCol3.sName = "Invoice Date";
            gridCol3.sTitle = "Invoice Date";
            /* every column you create needs to have a unique iType */
            gridCol3.iType = 2;

            CGridColumn gridCol4 = api.Grid.CreateGridColumn();
            gridCol4.sName = "Invoice Due";
            gridCol4.sTitle = "Invoice Due";
            /* every column you create needs to have a unique iType */
            gridCol4.iType = 3;

            CGridColumn gridCol5 = api.Grid.CreateGridColumn();
            gridCol5.sName = "Invoice Amount";
            gridCol5.sTitle = "Invoice Amount";
            /* every column you create needs to have a unique iType */
            gridCol5.iType = 4;

            CGridColumn gridCol6 = api.Grid.CreateGridColumn();
            gridCol6.sName = "At level";
            gridCol6.sTitle = "At level";
            /* every column you create needs to have a unique iType */
            gridCol6.iType = 5;

            CGridColumn gridCol7 = api.Grid.CreateGridColumn();
            gridCol7.sName = "Intacct Key";
            gridCol7.sTitle = "Intacct Key";
            /* every column you create needs to have a unique iType */
            gridCol7.iType = 6;

            CGridColumn gridCol8 = api.Grid.CreateGridColumn();
            gridCol7.sName = "Vendor Name";
            gridCol7.sTitle = "Vendor Name";
            /* every column you create needs to have a unique iType */
            gridCol7.iType = 7;

            return new CGridColumn[] { gridCol1, gridCol2, gridCol3, gridCol4, gridCol5, gridCol6, gridCol7, gridCol8 };
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
                    sTableColumn = "ixGlVendor";
                    break;
                case 2:
                    sTableColumn = "sInvoiceDate";
                    break;
                case 3:
                    sTableColumn = "sDueDate";
                    break;
                case 4:
                    sTableColumn = "sBalanceDue";
                    break;
                case 5:
                    sTableColumn = "ixAtlevel";
                    break;
                case 6:
                    sTableColumn = "iIntacctKey";
                    break;
                case 7:
                    sTableColumn = "VendorName";
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
                    sTableColumn = "ixGlVendor";
                    break;
                case 2:
                    sTableColumn = "sInvoiceDate";
                    break;
                case 3:
                    sTableColumn = "sDueDate";
                    break;
                case 4:
                    sTableColumn = "sBalanceDue";
                    break;

               /* case 5:
                    sTableColumn = "ixAtlevel";
                    break;

                case 6:
                    sTableColumn = "iIntacctKey";
                    break;
            */
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


        

        #region Item Related Functions
        /* Methods to created, read, update and delete items in the database, using
         * fields in the plugin api request object */
        #region Item CRUD

        protected void InsertItem()
        {
            try
            {
                CInsertQuery insert = api.Database.NewInsertQuery(api.Database.PluginTableName("BugInvoiceItem"));
                insertInt(insert, "ixBug");
                insertInt(insert, "ixGlAccount");
                //insertBoolean(insert, "iForm99");
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
                insert.InsertString("sMemo", api.Request[api.AddPluginPrefix("sMemo")].ToString());
                insertInt(insert, "ixGlDepartment");
                insertInt(insert, "ixGlLocation");
                insertInt(insert, "ixGlProject");
                insertInt(insert, "ixGlItem");
                insertInt(insert, "ixGlClass");
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
            CSelectQuery select = api.Database.NewSelectQuery(api.Database.PluginTableName("BugInvoiceItem"));

            LeftJoinTable(select, "GlAccount");
            LeftJoinTable(select, "GlDepartment");
            LeftJoinTable(select, "GlLocation");
            LeftJoinTable(select, "GlProject");
            LeftJoinTable(select, "GlClass");
            LeftJoinTable(select, "GlItem");

            select.AddSelect("*");
            string sWhere = api.Database.PluginTableName("BugInvoiceItem") + ".ixBug = " + ixBug.ToString();
            if (bExcludeDeleted)
            {
                sWhere += " and iDeleted = 0";
            }
            select.AddWhere(sWhere);

            return select.GetDataSet();
        }

        private string GetIntacctVendorId(string vendorid)
        {
            CSelectQuery selectQuery = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlVendor"));
            selectQuery.AddSelect("*");
            selectQuery.AddWhere("ixGlVendor = " + vendorid);

            string intacctVendorId = string.Empty;

            DataSet ds = selectQuery.GetDataSet();
            if (null != ds.Tables && ds.Tables.Count == 1 && ds.Tables[0].Rows.Count == 1)
            {
                intacctVendorId = Convert.ToString(ds.Tables[0].Rows[0]["sGlVendorId"]);
            }

            return intacctVendorId;
        }


        private string GetIntacctTrxCurrency(string TrxCurrency)
        {
         
            CSelectQuery selectQuery = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlTrxCurrency"));
            selectQuery.AddSelect("*");
            selectQuery.AddWhere("ixGlTrxCurrency = " + TrxCurrency);

         
            string intacctTrxCurrency = string.Empty;

       


            DataSet ds = selectQuery.GetDataSet();
            if (null != ds.Tables && ds.Tables.Count == 1 && ds.Tables[0].Rows.Count == 1)
            {
                
                intacctTrxCurrency = Convert.ToString(ds.Tables[0].Rows[0]["sGlTrxCurrencyId"]);
            }
         
            return intacctTrxCurrency;
        }

        private string GetIntacctExchratetype(string exhratetype)
        {

            CSelectQuery selectQuery = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlExchratetype"));
            selectQuery.AddSelect("*");
            selectQuery.AddWhere("ixGlExchratetype = " + exhratetype);


            string intacctExchratetype = string.Empty;




            DataSet ds = selectQuery.GetDataSet();
            if (null != ds.Tables && ds.Tables.Count == 1 && ds.Tables[0].Rows.Count == 1)
            {

                intacctExchratetype = Convert.ToString(ds.Tables[0].Rows[0]["sGlExchratetypeId"]);
            }

            return intacctExchratetype;
        }

        private string GetNetTerms(string nettermid)
        {
            CSelectQuery selectQuery = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlNetTerm"));
            selectQuery.AddSelect("*");
            selectQuery.AddWhere("ixGlNetTerm = " + nettermid);
            string intacctNettermName = string.Empty;

            DataSet ds = selectQuery.GetDataSet();
            if (null != ds.Tables && ds.Tables.Count == 1 && ds.Tables[0].Rows.Count == 1)
            {
                intacctNettermName = Convert.ToString(ds.Tables[0].Rows[0]["sGlNetTermName"]);
            }

            return intacctNettermName;
        }

        private string GetIntacctBankId(string bankid)
        {
            CSelectQuery selectQuery = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlBankAccount"));
            selectQuery.AddSelect("*");
            selectQuery.AddWhere("ixGlBankAccount = " + bankid);

            string intacctBankAccountId = string.Empty;

            DataSet ds = selectQuery.GetDataSet();
            if (null != ds.Tables && ds.Tables.Count == 1 && ds.Tables[0].Rows.Count == 1)
            {
                intacctBankAccountId = Convert.ToString(ds.Tables[0].Rows[0]["sGlBankAccountId"]);
            }

            return intacctBankAccountId;
        }

        private string GetIntacctPaymentMethodId(string paymentmethodid)
        {
            CSelectQuery selectQuery = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlPaymentMethod"));
            selectQuery.AddSelect("*");
            selectQuery.AddWhere("ixGlPaymentMethod = " + paymentmethodid);

            string intacctPaymentMethodId = string.Empty;

            DataSet ds = selectQuery.GetDataSet();
            if (null != ds.Tables && ds.Tables.Count == 1 && ds.Tables[0].Rows.Count == 1)
            {
                intacctPaymentMethodId = Convert.ToString(ds.Tables[0].Rows[0]["sGlPaymentMethodId"]);
            }

            return intacctPaymentMethodId;
        }

        private void LeftJoinTable(CSelectQuery select, string sType)
        {
            string projectPluginId = "IntacctSettings@conseroglobal.com";
            select.AddLeftJoin(api.Database.PluginTableName(projectPluginId, sType),
               api.Database.PluginTableName("BugInvoiceItem") + ".ix" + sType + " = " +
               api.Database.PluginTableName(projectPluginId, sType) + ".ix" + sType);
        }

        protected void UpdateItem()
        {
            try
            {
                CUpdateQuery update =
                    api.Database.NewUpdateQuery(api.Database.PluginTableName("BugInvoiceItem"));

                UpdateInt(update, "ixGlAccount");
                UpdateInt(update, "ixGlDepartment");
                UpdateInt(update, "ixGlLocation");
                UpdateInt(update, "ixGlProject");
                UpdateInt(update, "ixGlClass");
                UpdateInt(update, "ixGlItem");
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
                update.AddWhere("ixBugInvoiceItem = @ixBugInvoiceItem");
                update.SetParamInt("@ixBugInvoiceItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBugInvoiceItem")]));
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
                api.Database.NewUpdateQuery(api.Database.PluginTableName("BugInvoiceItem"));
            delete.UpdateInt("iDeleted", 1);
            delete.AddWhere("ixBugInvoiceItem = @ixBugInvoiceItem");
            delete.SetParamInt("@ixBugInvoiceItem", Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBugInvoiceItem")]));
            delete.Execute();
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

                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("BugInvoiceItem"));
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

                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("BugInvoice"));
                    sqlInvoiceDetails.AddSelect("*");
                    sqlInvoiceDetails.AddWhere("ixBug =" + i_OldCaseID.ToString());

                    DataSet dsOldCaseDetails = sqlInvoiceDetails.GetDataSet();

                    if ((dsOldCaseDetails == null) || (dsOldCaseDetails.Tables[0].Rows.Count == 0))
                    {

                        api.Notifications.AddMessage("This Case ID is not valid");

                        return;
                    }

                    sqlInvoiceDetails = api.Database.NewSelectQuery(api.Database.PluginTableName("BugInvoiceItem"));
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

                        string tablename = api.Database.PluginTableName("BugInvoice");
                        CUpdateQuery Update1 = api.Database.NewUpdateQuery(tablename);

                        Update1.UpdateString("CWFApproverl1", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl1"].ToString());
                        Update1.UpdateString("CWFApproverl2", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl2"].ToString());
                        Update1.UpdateString("CWFApproverl3", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl3"].ToString());
                        Update1.UpdateString("CWFApproverl4", dsOldCaseDetails.Tables[0].Rows[0]["CWFApproverl4"].ToString());
                        Update1.UpdateString("sPONumber", dsOldCaseDetails.Tables[0].Rows[0]["sPONumber"].ToString());
                        Update1.UpdateInt("ixGlVendor",Convert.ToInt32( dsOldCaseDetails.Tables[0].Rows[0]["ixGlVendor"]));
                        Update1.UpdateString("sInvoiceNumber", dsOldCaseDetails.Tables[0].Rows[0]["sInvoiceNumber"].ToString());
                        Update1.UpdateInt("ixGlNetTerm", Convert.ToInt32(dsOldCaseDetails.Tables[0].Rows[0]["ixGlNetTerm"].ToString()));
                        Update1.UpdateString("sBalanceDue", dsOldCaseDetails.Tables[0].Rows[0]["sBalanceDue"].ToString());
                        Update1.UpdateInt("ixGlTrxCurrency", Convert.ToInt32(dsOldCaseDetails.Tables[0].Rows[0]["ixGlTrxCurrency"].ToString()));
                        Update1.UpdateInt("ixGlExchratetype", Convert.ToInt32(dsOldCaseDetails.Tables[0].Rows[0]["ixGlExchratetype"].ToString()));
                        Update1.UpdateString("sExchrate", dsOldCaseDetails.Tables[0].Rows[0]["sExchrate"].ToString());
                      // Update1.UpdateString("sExchangeRate", dsOldCaseDetails.Tables[0].Rows[0]["sExchangeRate"].ToString());
                       // Update1.UpdateString("AccountDesc", dsOldCaseDetails.Tables[0].Rows[0]["AccountDesc"].ToString());
                        //Update1.UpdateString("TotalAmount", dsOldCaseDetails.Tables[0].Rows[0]["TotalAmount"].ToString());
                        //Update1.UpdateString("Netamount", dsOldCaseDetails.Tables[0].Rows[0]["Netamount"].ToString());

                        if (dsOldCaseDetails.Tables[0].Rows[0]["sBalanceDue"] != null)
                        {
                            try
                            {
                                Update1.UpdateFloat("sBalanceDue", Convert.ToDouble(dsOldCaseDetails.Tables[0].Rows[0]["sBalanceDue"].ToString()));
                            }
                            catch
                            {
                                Update1.UpdateFloat("sBalanceDue", 0d);
                            }
                        }


                        /*
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

                        */

                       //// Update1.UpdateString("sMemo", dsOldCaseDetails.Tables[0].Rows[0]["sMemo"].ToString());
                       // Update1.UpdateString("sAddInfo", dsOldCaseDetails.Tables[0].Rows[0]["sAddInfo"].ToString());
                       // Update1.UpdateString("CWFCustomVal1", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal1"].ToString());
                       // Update1.UpdateString("CWFCustomVal2", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal2"].ToString());
                       // Update1.UpdateString("CWFCustomVal3", dsOldCaseDetails.Tables[0].Rows[0]["CWFCustomVal3"].ToString());
                       // Update1.UpdateString("Remarks", dsOldCaseDetails.Tables[0].Rows[0]["Remarks"].ToString());


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
                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("BugInvoiceItem"));
                                insert1.InsertInt("ixBug", ixBug);
                                insert1.InsertInt("ixGlAccount",Convert.ToInt32(dr["ixGlAccount"]));
                                //insert1.InsertInt("fAmount",Convert.ToInt32( dr["fAmount"]));

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



                                insert1.InsertString("sMemo", dr["sMemo"].ToString());
                                insert1.InsertInt("ixGlDepartment", Convert.ToInt32(dr["ixGlDepartment"].ToString()));
                                insert1.InsertInt("ixGlLocation",Convert.ToInt32(dr["ixGlLocation"].ToString()));
                                insert1.InsertInt("ixGlProject",Convert.ToInt32(dr["ixGlProject"].ToString()));
                                insert1.InsertInt("ixGlItem", Convert.ToInt32(dr["ixGlItem"].ToString()));
                                insert1.InsertInt("ixGlClass", Convert.ToInt32(dr["ixGlClass"].ToString()));

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
                                                               
                                //api.Notifications.AddAdminNotification("30", "");
                                insert1.Execute();
                                //api.Notifications.AddAdminNotification("9", "");
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

        #endregion

        /* This method builds the EditableTable which displays the items. It includes
         * DialogTemplates for add, edit and delete dialogs */
        protected CEditableTable ItemTable(int ixBug, int ixProject, bool bSuppressEditsAndDeletes)
        {
            CEditableTable editableTableItems = new CEditableTable("itemtable");
            string sTableId = editableTableItems.sId;
            /* Define the header row of the table */
            if (!bSuppressEditsAndDeletes)
            {
                //editableTableItems.Header.AddCell("Edit");
                editableTableItems.Header.AddCell("Delete");
            }
            editableTableItems.Header.AddCell("Account");
            editableTableItems.Header.AddCell("Amount");
            //editableTableItems.Header.AddCell("Form 99");
            editableTableItems.Header.AddCell("Memo");
            editableTableItems.Header.AddCell("Department");
            editableTableItems.Header.AddCell("Location");
            editableTableItems.Header.AddCell("Project");
            editableTableItems.Header.AddCell("Class");
            editableTableItems.Header.AddCell("Item");

            /* this variable means we don't need to mess with colspans later in the code */
            int nColCount = editableTableItems.Header.Cells.Count;

            /* Create the edit dialog template object used when the user clicks the
             * edit icon in a particular row */
            CDialogTemplate dlgTemplateEdit = DialogTemplateEdit(sTableId, ixProject);

            /* Create the new item dialog template object used when the user clicks Add
             * New Item or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew(ixBug, ixProject, sTableId);

            /* Create the delete dialog template object used when the user clicks the
             * delete icon in a particular row */
            CDialogTemplate dlgTemplateDelete = DialogTemplateDelete(sTableId, ixProject);

            /* setup a DataSet and fetch the items from the database */
            DataSet dsItems = FetchItems(ixBug, true);
            int ixBugInvoiceItem = -1;
            int ixGlAccount = -1;
            int ixGlDepartment = -1;
            int ixGlLocation = -1;
            int ixGlProject = -1;
            int ixGlClass = -1;
            int ixGlItem = -1;
            //int iForm99 = -1;
            string sMemo = "";
            double dAmount = -1D;
            string sGlAccountName = "";
            string sGlDepartmentName = "";
            string sGlLocationName = "";
            string sGlProjectName = "";
            string sGlClassName = "";
            string sGlItemName = "";
            string sGlAccountCode = "";
            string sGlDepartmentId = "";

            /* If the DataSet contains any rows, loop through them and populate the table
             * and dialog template data Hashtables */
            if (dsItems.Tables[0] != null && dsItems.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
                {
                    //api.Notifications.AddAdminNotification("Trying to load the items", "This is not loading whu?");
                    ixBugInvoiceItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixBugInvoiceItem"]);
                    ixGlAccount = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixGlAccount"]);
                    sGlAccountName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlAccountName"]);
                    sGlAccountCode = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlAccountId"]);
                    ixGlDepartment = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixGlDepartment"]);
                    sGlDepartmentName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlDepartmentName"]);
                    sGlDepartmentId = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlDepartmentId"]);
                    ixGlLocation = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixGlLocation"]);
                    sGlLocationName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlLocationName"]);
                    ixGlProject = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixGlProject"]);
                    sGlProjectName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlProjectName"]);
                    ixGlClass = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixGlClass"]);
                    sGlClassName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlClassName"]);
                    ixGlItem = Convert.ToInt32(dsItems.Tables[0].Rows[i]["ixGlItem"]);
                    sGlItemName = Convert.ToString(dsItems.Tables[0].Rows[i]["sGlItemName"]);
                    //iForm99 = Convert.ToInt32(dsItems.Tables[0].Rows[i]["iForm99"]);
                    sMemo = Convert.ToString(dsItems.Tables[0].Rows[i]["sMemo"]);
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
                    row.sRowId = ixBugInvoiceItem.ToString();
                    /* CEditableTable provides standard edit and delete icon links.
                     * The second parameter is the name of the dialog to open. The fourth
                     * parameter is the URL to link to if javascript is not available.
                     * Note: we do not provide a non-javascript mode in this example. */
                    if (!bSuppressEditsAndDeletes)
                    {
                        //row.AddCell(CEditableTable.LinkShowDialogEditIcon(
                        //                sTableId,
                        //                "edit",
                        //                row.sRowId,
                        //                CommandUrl("edit", ixBugInvoiceItem, ixBug.ToString())));
                        row.AddCell(CEditableTable.LinkShowDialogDeleteIcon(
                                        sTableId,
                                        "delete",
                                        row.sRowId,
                                        CommandUrl("delete", ixBugInvoiceItem, ixBug.ToString())));
                    }
                    /* make sure to run HtmlEncode on any user data before displaying it! */
                    row.AddCell(HttpUtility.HtmlEncode(sGlAccountCode.ToString() + ":" + sGlAccountName.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(dAmount.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sMemo.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sGlDepartmentId.ToString() + ":" + sGlDepartmentName.ToString()));
                    row.AddCell(HttpUtility.HtmlEncode(sGlLocationName));
                    row.AddCell(HttpUtility.HtmlEncode(sGlProjectName));
                    row.AddCell(HttpUtility.HtmlEncode(sGlClassName));
                    row.AddCell(HttpUtility.HtmlEncode(sGlItemName));
                    editableTableItems.Body.AddRow(row);

                    /* Now that the row is populated for display, put the data in a hash table
                     * to be used in populating the pop-up add, edit and delete dialogs. */
                    Hashtable hashData = new Hashtable();
                    hashData.Add("ixBugInvoiceItem", ixBugInvoiceItem);
                    hashData.Add("ixBug", ixBug);
                    hashData.Add("ixProject", ixProject);
                    hashData.Add("ixGlAccount", ixGlAccount);
                    hashData.Add("fAmount", dAmount);
                    hashData.Add("sMemo", sMemo);
                    hashData.Add("ixGlDepartment", ixGlDepartment);
                    hashData.Add("ixGlLocation", ixGlLocation);
                    hashData.Add("ixGlProject", ixGlProject);
                    hashData.Add("ixGlItem", ixGlItem);
                    hashData.Add("ixGlClass", ixGlClass);
                    //hashData.Add("iFrom99", iForm99.ToString());

                    /* add the hash table as data to the edit template */
                    dlgTemplateEdit.AddTemplateData(row.sRowId, hashData);

                    /* add the data to the delete template as well */
                    dlgTemplateDelete.AddTemplateData(row.sRowId, hashData);
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
                                                        CommandUrl("new", ixBugInvoiceItem, ixBug.ToString())));
                editableTableItems.Footer.AddCellWithColspan(CEditableTable.LinkShowDialog(
                                                        sTableId,
                                                        "new",
                                                        "sDataId",
                                                        CommandUrl("new", ixBugInvoiceItem, ixBug.ToString()),
                                                        "Add New Item"),
                                                        nColCount - 1);

                /* Associate the dialog templates with the table by name */
                editableTableItems.AddDialogTemplate("new", dlgTemplateNew);
                //editableTableItems.AddDialogTemplate("edit", dlgTemplateEdit);
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
            CDialogItem itemGlAccount = new CDialogItem(
                GetSelects_1("GlAccount", ixProject),
                "GL Account",
                "Choose the gl account this item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemGlAccount);

            CDialogItem itemAmount =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), ""),
                                "Amount");
            dlgTemplateNew.Template.Items.Add(itemAmount);

            //CDialogItem itemForm99 =
            //    new CDialogItem(Forms.CheckboxInput(api.AddPluginPrefix("iForm99"), api.AddPluginPrefix("iForm99"), false),
            //                    "Has Form 1099");
            //dlgTemplateNew.Template.Items.Add(itemForm99);

            CDialogItem itemMemo =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), ""),
                                "Memo");
            dlgTemplateNew.Template.Items.Add(itemMemo);

            CDialogItem itemGlDepartment = new CDialogItem(
                GetSelects_1("GlDepartment", ixProject),
                "GL Department",
                "Choose the gl department this item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemGlDepartment);

            CDialogItem itemGlLocation = new CDialogItem(
                GetSelects("GlLocation", ixProject),
                "GL Location",
                "Choose the gl location this item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemGlLocation);

            CDialogItem itemGlProject = new CDialogItem(
                GetSelects("GlProject", ixProject),
                "GL Project",
                "Choose the gl project this item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemGlProject);

            CDialogItem itemGlItem = new CDialogItem(
                GetSelects("GlItem", ixProject),
                "GL Item",
                "Choose the gl item this item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemGlItem);

            CDialogItem itemGlClass = new CDialogItem(
                GetSelects("GlClass", ixProject),
                "GL Class",
                "Choose the gl class this item has to be coded from the drop-down");
            dlgTemplateNew.Template.Items.Add(itemGlClass);

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
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "edit");
            dlgTemplateEdit.Template.Items.Add(itemEditHiddenAction);
            dlgTemplateEdit.Template.Items.Add(CDialogItem.HiddenInput(
                                                    api.AddPluginPrefix("ixBugInvoiceItem"),
                                                    "{ixBugInvoiceItem}"));
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

            //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Account Value is {ixGlAccount}."));

            dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("GlAccount", "GL Account", ixProject, "{ixGlAccount}"));

            CDialogItem itemAmount =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("fAmount"), "{fAmount}"),
                                "Amount");
            dlgTemplateEdit.Template.Items.Add(itemAmount);

            //CDialogItem itemForm99 =
            //    new CDialogItem(Forms.CheckboxInput(api.AddPluginPrefix("iForm99"), api.AddPluginPrefix("iForm99"), "{iForm99}"),
            //                    "Has Form 1099");
            //dlgTemplateEdit.Template.Items.Add(itemForm99);

            CDialogItem itemMemo =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sMemo"), "{sMemo}"),
                                "Memo");
            dlgTemplateEdit.Template.Items.Add(itemMemo);

            //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Department Value is {ixGlDepartment}"));
            dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("GlDepartment", "GL Department", ixProject, "{ixGlDepartment}"));

            //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Location Value is {ixGlLocation}."));
            dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("GlLocation", "GL Location", ixProject, "{ixGlLocation}"));

            //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Project Value is {ixGlProject}."));
            dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("GlProject", "GL Project", ixProject, "{ixGlProject}"));

            //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Item Value is {ixGlItem}."));
            dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("GlItem", "GL Item", ixProject, "{ixGlItem}"));

            //dlgTemplateEdit.Template.Items.Add(new CDialogItem("Gl Class Value is {ixGlClass}."));
            dlgTemplateEdit.Template.Items.Add(EditDialogDropDown("GlClass", "GL Class", ixProject, "{ixGlClass}"));

            /* Standard ok and cancel buttons */
            dlgTemplateEdit.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            return dlgTemplateEdit;
        }

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
            row.AddCell(HttpUtility.HtmlEncode("Copy Case_1"));
            editableTable.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(sTableId, "CopyCase", "sDataId",
                                                    CommandUrl1("CopyCase", ixBug), "Copy Case"));

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
                dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(api.AddPluginPrefix("ixBug"), ixBug.ToString()));
                CDialogItem itemEditId = new CDialogItem(Forms.TextInput(api.AddPluginPrefix("CaseID"), ""), "Case ID ");
                dlgTemplateNew.Template.Items.Add(itemEditId);

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

        private CDialogItem EditDialogDropDown(string sFieldName, string sDisplay, int ixProject, string sSelected)
        {
            return new CDialogItem(
                GetSelects(sSelected, sFieldName, ixProject, false),
                sDisplay,
                "Choose the " + sDisplay + " this item has to be coded from the drop-down");
        }

        /* This method builds the template for the delete item dialog */
        protected CDialogTemplate DialogTemplateDelete(string sTableId, int ixProject)
        {
            CDialogTemplate dlgTemplateDelete = new CDialogTemplate();
            dlgTemplateDelete.Template = new CSingleColumnDialog();
            dlgTemplateDelete.Template.sTitle = "Delete Item ";
            dlgTemplateDelete.Template.Items.Add(
                CDialogItem.HiddenInput(api.AddPluginPrefix("ixBugInvoiceItem"), "{ixBugInvoiceItem}"));
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

        /* Render an html drop-down menu from the entries in the given table */
        #region Dropdown Selects

        protected string GetSelects(string sType, int ixProject)
        {
            return GetSelects(null, sType, ixProject, false);
        }

        protected string GetSelects_1(string sType, int ixProject)
        {
            return GetSelects_1(null, sType, ixProject, false);
        }

        protected string GetSelects_1(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType),
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
                        + " :: " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " :: " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                    ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                }

                ds.Dispose();

                return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                                               Forms.SelectOptions(names,
                                                                   sSelected,
                                                                   ixs));
            }
            ds.Dispose();
            return String.Empty;
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
                        names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString())
                        + " " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                }

                ds.Dispose();
                return HttpUtility.HtmlDecode(Forms.SelectInputString(api.AddPluginPrefix(sType),
                                 Forms.SelectOptions(names, sSelected, names)));

            }
            ds.Dispose();
            return String.Empty;
        }

        protected string GetSelects(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType),
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
                        + " :: " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());

                    }
                    else
                    {
                        names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                     
                    }

                    ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                }

                ds.Dispose();

                return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                                               Forms.SelectOptions(names,
                                                                   sSelected,
                                                                   ixs));
            }
            ds.Dispose();
            return String.Empty;
        }


        protected string GetSelects2(string sSelected, string sType, int ixProject, bool bDisplayId)
        {
            /* Fetch all the names and ids from the database and
             * populate two string arrays */
            string[] names = null;
            string[] ixs = null;

            CSelectQuery sq = api.Database.NewSelectQuery(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType));
            sq.AddSelect("*");
            sq.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType) + ".ixProject = " + ixProject.ToString());
            sq.AddOrderBy(string.Format("{0}.{1} {2}",
                                            api.Database.PluginTableName("IntacctSettings@conseroglobal.com", sType),
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
                        names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Id"].ToString());
                        //+ " :: " + HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                    else
                    {
                        names[i] = HttpUtility.HtmlEncode(ds.Tables[0].Rows[i]["s" + sType + "Name"].ToString());
                    }
                    ixs[i] = ds.Tables[0].Rows[i]["ix" + sType].ToString();
                }

                ds.Dispose();

                return Forms.SelectInputString(api.AddPluginPrefix("ix" + sType),
                                               Forms.SelectOptions(names,
                                                                   sSelected,
                                                                   ixs));
            }
            ds.Dispose();
            return String.Empty;
        }

        #endregion

        #region Master Details
        //Vendor
        public string GetVendorName(int VId, int RType) //Rtype 1 for vendorid,2 for vendorname,3 for both vendorname & vendorId
        {

            CSelectQuery sq = api.Database.NewSelectQuery(
                    api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlVendor"));
            sq.AddSelect("sGlVendorId");
            sq.AddSelect("sGlVendorName");
            sq.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlVendor") + ".ixGlVendor" + " = " + VId);
            DataSet ds = sq.GetDataSet();
            string VendId="";
            string Vname = "";
            if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
            {
                if (RType == 1)
                {
                     VendId = ds.Tables[0].Rows[0]["sGlVendorId"].ToString();
                    return VendId;
                }
                else if (RType == 2)
                {
                     Vname = ds.Tables[0].Rows[0]["sGlVendorName"].ToString();
                    return Vname;
                }
                else if (RType == 3)
                {
                     VendId = ds.Tables[0].Rows[0]["sGlVendorId"].ToString();
                     Vname = ds.Tables[0].Rows[0]["sGlVendorName"].ToString();
                    return VendId+":"+Vname;
                }
              
            }
            return VendId + ":" + Vname;
        }

        public string GetVendorName(int VId)
        {
        CSelectQuery sq = api.Database.NewSelectQuery(
                    api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlVendor"));
            sq.AddSelect("sGlVendorId");
            sq.AddSelect("sGlVendorName");
            sq.AddWhere(api.Database.PluginTableName("IntacctSettings@conseroglobal.com", "GlVendor") + ".ixGlVendor" + " = " + VId);
            DataSet ds = sq.GetDataSet();
            string Vname = "";
            if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
             {
                
                    Vname = ds.Tables[0].Rows[0]["sGlVendorName"].ToString();
                    return Vname;
                }
            return Vname;
              }


        #endregion

        #region Utility Methods

        /* these two methods are used to construc the Urls which a user would
         * follow if javascript is disabled (preventing the use of the Dialogs */
        protected string CommandUrl(string sCommand, int ixBugInvoiceItem, string ixBug)
        {
            return string.Concat(api.Url.PluginPageUrl(),
                                 LinkParameter("sCommand", sCommand),
                                 LinkParameter("ixBugInvoiceItem", ixBugInvoiceItem.ToString()),
                                 LinkParameter("ixBug", ixBug));
        }

        protected string LinkParameter(string sName, string sValue)
        {
            return string.Format("&{0}={1}", api.AddPluginPrefix(sName), sValue);
        }

        protected string CommandUrl1(string sCommand, int ixBug)
        {
            return string.Concat(api.Url.PluginPageUrl(),
                                 LinkParameter("sCommand", sCommand),
                                 LinkParameter("ixBug", ixBug.ToString()));
        }


        #endregion

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert)]
        public XmlDocument PostXMLTransaction(
            CProject project, MemoryStream ms
            )
        {
            //Company ID: demo58970749
            //User ID: guest
            //Password: 46dbd2c4

            //A production XML gateway license has been created for you.  Following are instructions for posting to the production XML gateway:

            //URL: https://www.intacct.com/ia/xml/xmlgw.phtml
            //Your Sender ID is: consero
            //Your Password is: HubeJAJu$e

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
            objHttpWebRequest = (HttpWebRequest)WebRequest.Create(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctUrl")));

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
               
                ServicePointManager.ServerCertificateValidationCallback = CertChecker;
              
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
                if (null != objRequestStream)
                {
                   
                    objRequestStream.Close();
                }
                if (null != objResponseStream)
                {
                    
                    objResponseStream.Close();
                }
                if (null != objHttpWebResponse)
                {
                    
                    objHttpWebResponse.Close();
                }

                //Release objects
                objXMLReader = null;
                objRequestStream = null;
                objResponseStream = null;
                objHttpWebResponse = null;
                objHttpWebRequest = null;
            }

            //if (null != xmlResponse)
            //{
            //    api.Notifications.AddAdminNotification("XML Response to Intacct", "\"" + xmlResponse.InnerXml + "\"");
            //}

            //Return
          //  api.Notifications.AddMessage(xmlResponse.ToString());

            XmlNode intacctKeyNode1 = xmlResponse.SelectSingleNode("/response/control/status");
          //  api.Notifications.AddMessage(" intacct response control " + intacctKeyNode1.InnerText);
            XmlNode intacctKeyNode5 = xmlResponse.SelectSingleNode("/response/operation/authentication/status");
           // api.Notifications.AddMessage(" intacct response operation " + intacctKeyNode1.InnerText);
            //try
            //{
            //    XmlNode intacctKeyNode2 = xmlResponse.SelectSingleNode("/response/errormessage/error/description2");
            //    api.Notifications.AddMessage(" intacct response2 " + intacctKeyNode2.InnerText);
            //}
            //catch { 
            //}
            string keytest = "0";
            try
            {
                XmlNode intacctKeyNodex = xmlResponse.SelectSingleNode("/response/operation/result/key");

                keytest = intacctKeyNodex.InnerText;
            }

            catch
            {
                keytest = "0";

            }
            try
            {
                XmlNode intacctKeyNode = xmlResponse.SelectSingleNode("/response/operation/result/key");

                if (keytest == "0")
                {

                   // api.Notifications.AddError("Unable to create the bill in Intacct");
                  //  api.Notifications.AddMessage("*** Check for following possiblilities ***");
                    //api.Notifications.AddMessage(" The bill number may already exist in Intacct");
                   // api.Notifications.AddMessage(" Verify if Posting period is open");
                   // api.Notifications.AddMessage(" Bill number should not be more than 20 characters");
                   // api.Notifications.AddMessage(" VendorId should be matching with Intacct VendorId (case sensitive)");
                }
            }
            catch { }
           // api.Notifications.AddMessage((xmlResponse.SelectSingleNode("/response/control/status").ToString()));
            return xmlResponse;
        }

        static bool CertChecker(object sender, X509Certificate certificate,
                         X509Chain chain, SslPolicyErrors errors)
        {
            return true;
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

        public void updateIntacctStatus(CBug bug, string IntacKey, string APBillStatus)
        {
            CProject project = api.Project.GetProject(bug.ixProject);
            XmlWriterSettings wSettings = new XmlWriterSettings();
            wSettings.Indent = true;
            MemoryStream ms = new MemoryStream();
            XmlWriter xw = XmlWriter.Create(ms, wSettings);
            // Write Declaration
            //api.Notifications.AddMessage("Inatkey", IntacKey);
            xw.WriteStartDocument();
            xw.WriteDocType("request", null, "intacct_request.v2.1.dtd", null);
            //  api.Notifications.AddMessage("1a");
            // Write the root node
            xw.WriteStartElement("request");
            //  api.Notifications.AddMessage("1b");
            // Write the control and the control elements
            xw.WriteStartElement("control");
            xw.WriteStartElement("senderid");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctSenderId")));
            //   api.Notifications.AddMessage("send id" + Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctSenderId")));
            xw.WriteEndElement();
            xw.WriteStartElement("password");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctSenderPassword")));
            // api.Notifications.AddMessage("send pid" + Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctSenderPassword")));
            xw.WriteEndElement();
            xw.WriteStartElement("controlid");
            xw.WriteString("controlid");
            xw.WriteEndElement();
            //   api.Notifications.AddMessage("1c");
            xw.WriteStartElement("uniqueid");
            xw.WriteString("false");
            xw.WriteEndElement();
            xw.WriteStartElement("dtdversion");
            xw.WriteString("2.1");
            xw.WriteEndElement();
            xw.WriteEndElement();

            // Write the operation and the operation elements
            xw.WriteStartElement("operation");
            xw.WriteStartAttribute("transaction");
            xw.WriteString("false");
            xw.WriteEndAttribute();

            xw.WriteStartElement("authentication");
            xw.WriteStartElement("login");
            xw.WriteStartElement("userid");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctUserId")));
            // api.Notifications.AddMessage("userid" + Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctUserId")));
            xw.WriteEndElement();
            xw.WriteStartElement("companyid");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctCompanyId")));
            // api.Notifications.AddMessage("compid" + Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctCompanyId")));
            xw.WriteEndElement();
            xw.WriteStartElement("password");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctUserPassword")));
            //  api.Notifications.AddMessage("pword" + Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctUserPassword")));
            xw.WriteEndElement();
            xw.WriteStartElement("locationid");
            xw.WriteString(Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctLocationId")));
            // api.Notifications.AddMessage("locid" + Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sIntacctLocationId")));
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();


            //DataSet dsItems = FetchItems(bug.ixBug, false);

            // Content Begins Here
            xw.WriteStartElement("content");

            //Function Starts Here
            xw.WriteStartElement("function");
            xw.WriteStartAttribute("controlid");
            xw.WriteString(bug.ixBug.ToString());
            xw.WriteEndAttribute();
            //  api.Notifications.AddMessage("1ae");
            //Update Bill Starts Here
            xw.WriteStartElement("update_bill");

            //Intacct Key
            xw.WriteStartAttribute("key");
            // xw.WriteString(Convert.ToString(bug.GetPluginField(PLUGIN_ID, "iIntacctKey"))); 
            xw.WriteString(IntacKey);
            //  api.Notifications.AddMessage("Key" + IntacKey);
            xw.WriteEndAttribute();
            //  api.Notifications.AddMessage("1af");

            //Description
            xw.WriteStartElement("description");
            xw.WriteString("<a target='_blank' href = 'http://empower.conseroglobal.com/default.asp?" + bug.ixBug.ToString() + "'> Empower Case " + bug.ixBug.ToString() + "</a>");
            xw.WriteEndElement();

            xw.WriteStartElement("customfields");
            xw.WriteStartElement("customfield");
            //  api.Notifications.AddMessage("1af1");
            //xw.WriteEndElement();
            xw.WriteStartElement("customfieldname");
            xw.WriteString("Bill_status");
            // api.Notifications.AddMessage("1af2");
            xw.WriteEndElement();
            xw.WriteStartElement("customfieldvalue");
            // api.Notifications.AddMessage("1af3");
            xw.WriteString(APBillStatus);
            //  api.Notifications.AddMessage("1af4");

            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.WriteEndElement();

            xw.WriteEndElement();
            xw.WriteEndElement();
            //api.Notifications.AddMessage("1af5");

            // xw.WriteEndElement();
            // xw.WriteEndElement();


            xw.WriteEndElement();
            xw.WriteEndElement();
            // api.Notifications.AddMessage("1ag");


            xw.Flush();

            Byte[] buffer = new Byte[ms.Length];
            buffer = ms.ToArray();



            XmlDocument xmlResponse = PostXMLTransaction(project, ms);
            // bool responseSuccess = false;
            if (null != xmlResponse)
            {
                // responseSuccess = true;

                {

                    XmlNode intacctKeyNode = xmlResponse.SelectSingleNode("/response/operation/result");
                    //XmlNode intacctKeyNode = xmlResponse.SelectSingleNode("/response/operation/result/bill/key");
                    // api.Notifications.AddMessage(" Update Successfull " + intacctKeyNode.InnerText);

                }
            }
            //vendor id


        }
    }
}
