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
namespace Consero.Plugins.CGSAddNew
{
    public class Act : Plugin,  IPluginRawPageDisplay,//IPluginBugJoin
        IPluginBugDisplay, IPluginDatabase, IPluginBugCommit
    {


        protected const string PLUGIN_ID =
           "CGSAddNew@conseroglobal.com";

        /* A constant for populating the "code name" input field for multiple case edit */
        protected const string VARIOUS_TEXT = "[various]";
       // private string sPrefixedTableName;

        int project;
        string sProj;

        public Act(CPluginApi api)
            : base(api)
        {
           // sPrefixedTableName = api.Database.PluginTableName("TestField");
        }

     //   #region IPluginBugJoin Members

     //   public string[] BugJoinTables()
     //   {
            /* All tables specified here must have an integer ixBug column so FogBugz can
            * perform the necessary join. */

        //    return new string[] { "TestField" };
     //   }

       // #endregion
        /*
        #region IPluginProjectJoin Members

       
        public string[] ProjectJoinTables()
        {
           

            return new string[] {"CWFVendor", "CWFAccount", "CWFVat", "CWFDepartment", "CWFapproverl2", "CWFapproverl3", "CWFapproverl4" };
        }
        
        #endregion
        */
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

            //if (bug.ixProject != 8)
            //{
            //    return null;
            //}



            if (nMode == BugEditMode.Edit)

            // if (nMode == BugEditMode.Edit && rgbug[0].ixStatus == 20)
            {
               // api.Notifications.AddMessage("1");
                return new CBugDisplayDialogItem[] 
                   { 

                       
                    //   CreateTextInputField(rgbug, "TestField", "Test Field", "sTestfield")

                       
                       
                      // new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, false).RenderHtml() , "Items", 3)
                   };

            }

            if (nMode == BugEditMode.Resolve)
                return new CBugDisplayDialogItem[] 
                   {

                    //    CreateText(rgbug, "TestField", "Test Field", "sTestfield")

            };

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


            //if (rgbug[0].ixProject != 8)

             //   return null;


            // api.Notifications.AddMessage("calling CBugDisplayDialogItem");

            CProject project1 = api.Project.GetProject(rgbug[0].ixProject);
            string enabledCGSWorkflowSettings = Convert.ToString(project1.GetPluginField("CGSWorkflowSettings@conseroglobal.com", "sEnableWorkflow"));
            if (string.IsNullOrEmpty(enabledCGSWorkflowSettings) || "0".Equals(enabledCGSWorkflowSettings))
            {
                //don't do any intacct calls
                return null;
            }

            sProj = project1.sProject;

            project = project1.ixProject;


            if (rgbug[0].ixProject == 14)
            {
                return new CBugDisplayDialogItem[] {

                new CBugDisplayDialogItem("Vendor", EditableTable(rgbug[0].ixBug).RenderHtml()),
                new CBugDisplayDialogItem("Account", EditableTable2(rgbug[0].ixBug).RenderHtml()),
                  new CBugDisplayDialogItem("VAT", EditableTable4(rgbug[0].ixBug).RenderHtml()),
                 new CBugDisplayDialogItem("Department", EditableTable3(rgbug[0].ixBug).RenderHtml()),
              new CBugDisplayDialogItem("Approvers", EditableTable5(rgbug[0].ixBug).RenderHtml()),
                  
                };
            }


            else if (rgbug[0].ixProject == 25 || rgbug[0].ixProject == 26)
            {
                return new CBugDisplayDialogItem[] {

                new CBugDisplayDialogItem("Location", EditableTable_Loc(rgbug[0].ixBug).RenderHtml()),
                new CBugDisplayDialogItem("Department", EditableTable_Dept(rgbug[0].ixBug).RenderHtml()),
                new CBugDisplayDialogItem("Account", EditableTable2(rgbug[0].ixBug).RenderHtml()),
             };
            }

            else if (rgbug[0].ixProject == 19 )
            {
                return new CBugDisplayDialogItem[] {
                   new CBugDisplayDialogItem("Vendor", EditableTable(rgbug[0].ixBug).RenderHtml()),
                new CBugDisplayDialogItem("Account", EditableTable2(rgbug[0].ixBug).RenderHtml()),
                };
            }

            else if (rgbug[0].ixProject == 9)
            {
                return new CBugDisplayDialogItem[] {
                   new CBugDisplayDialogItem("Vendor", EditableTable(rgbug[0].ixBug).RenderHtml()),
                new CBugDisplayDialogItem("Account", EditableTable2(rgbug[0].ixBug).RenderHtml()),
                new CBugDisplayDialogItem("Department", EditableTable3(rgbug[0].ixBug).RenderHtml()),
                new CBugDisplayDialogItem("Class", EditableTable_Class(rgbug[0].ixBug).RenderHtml()),
                };
            }
            return null;
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

            //if (bug.ixProject != 8)
            //{
            //    return null;
            //}


            return new CBugDisplayDialogItem[] { 
               // CreateText(rgbug, "TestField", "Test Field", "sTestfield")
              
                //new CBugDisplayDialogItem("item", ItemTable(rgbug[0].ixBug, rgbug[0].ixProject, true).RenderHtml() , "Items", 3)
            };
        }

        # endregion


        #region IPluginRawPageDisplay Members

        public string RawPageDisplay()
        {


            try
            {

                // api.Notifications.AddMessage("calling RawPageDisplay");
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

                    //api.Notifications.AddMessage("calling RawPageDisplay_2");

                  //  api.Notifications.AddAdminNotification("calling RawPageDisplay_3","3");

                    ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());
                    CBug bug = api.Bug.GetBug(ixBug);
                    bug.IgnorePermissions = true;
                    int iproj = bug.ixProject;

                   // api.Notifications.AddAdminNotification("iproj", iproj.ToString());
                    // adding new Vendor 
                    {
                        try
                        {
                            string vendorid = (api.Request[api.AddPluginPrefix("Vendorid")].ToString());
                            string vendorname = (api.Request[api.AddPluginPrefix("Vendorname")].ToString());

                            //api.Notifications.AddAdminNotification("Vendorname", "1");
                            if ((api.Request[api.AddPluginPrefix("Vendorid")].ToString().Trim()) != null)
                            {
                                if ((api.Request[api.AddPluginPrefix("Vendorid")].ToString()).Trim() != "")
                                {
                                    //api.Notifications.AddAdminNotification("project 4", sProj);

                                    CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFVendor"));
                                 //   api.Notifications.AddMessage("3");
                                    insert1.InsertInt("ixProject", iproj);
                                    insert1.InsertString("sCWFVendorId", vendorid);
                                    insert1.InsertString("sCWFVendorName", vendorname);
                                    insert1.Execute();
                                }
                            }
                        }


                        catch
                        {
                            //dont do anything
                        }
                    }
                    // adding new Account  
                    try
                    {


                        string accntid = (api.Request[api.AddPluginPrefix("accountid")].ToString());
                        string accntdesc = (api.Request[api.AddPluginPrefix("accountdesc")].ToString());


                      //  api.Notifications.AddAdminNotification("accountdesc", "2");
                        if ((api.Request[api.AddPluginPrefix("accountid")].ToString().Trim()) != null)
                        {

                            if ((api.Request[api.AddPluginPrefix("accountid")].ToString()).Trim() != "")
                            {


                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFAccount"));
                                insert1.InsertInt("ixProject", iproj);
                                insert1.InsertString("sCWFAccountId", accntid);
                                insert1.InsertString("sCWFAccountName", accntdesc);
                                insert1.Execute();
                            }
                        }
                    }




                    catch
                    {
                        //dont do anything
                    }

                    // adding new Department 
                    try
                    {


                        string dptid = (api.Request[api.AddPluginPrefix("deptid")].ToString());
                        string dptdesc = (api.Request[api.AddPluginPrefix("deptdesc")].ToString());

                      //  api.Notifications.AddAdminNotification("dptdesc", "3");

                        if ((api.Request[api.AddPluginPrefix("deptid")].ToString().Trim()) != null)
                        {

                            if ((api.Request[api.AddPluginPrefix("deptid")].ToString()).Trim() != "")
                            {


                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFDepartment"));
                                insert1.InsertInt("ixProject", iproj);
                                insert1.InsertString("sCWFDepartmentId", dptid);
                                insert1.InsertString("sCWFDepartmentName", dptdesc);
                                insert1.Execute();
                            }
                        }
                    }




                    catch
                    {
                        //dont do anything
                    }
                    // CInsertQuery testinsert = api.Database.NewInsertQuery("CBGGLDepartments");

                    // adding new Vat Code 
                    try
                    {
                        // api.Notifications.AddAdminNotification("Accnt 4", "acnt 4");

                        string vatid = (api.Request[api.AddPluginPrefix("vatid")].ToString());
                        string vatdesc = (api.Request[api.AddPluginPrefix("vatdesc")].ToString());

                      //  api.Notifications.AddAdminNotification("vatdesc", "4");

                        if ((api.Request[api.AddPluginPrefix("vatid")].ToString().Trim()) != null)
                        {

                            if ((api.Request[api.AddPluginPrefix("vatid")].ToString()).Trim() != "")
                            {


                                CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFVat"));
                                insert1.InsertInt("ixProject", iproj);
                                insert1.InsertString("sCWFVatId", vatid);
                                insert1.InsertString("sCWFVatName", vatdesc);
                                insert1.Execute();
                            }
                        }
                    }




                    catch
                    {
                        //dont do anything
                    }

                    // adding loc and dept to synergis


                    {
                        try
                        {
                            string Locationid = (api.Request[api.AddPluginPrefix("Locationid")].ToString());
                            string Location = (api.Request[api.AddPluginPrefix("LocationName")].ToString());
                            CInsertQuery insert2 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFLocation"));
                          //  api.Notifications.AddAdminNotification("1", "3");
                            insert2.InsertInt("ixProject", iproj);
                            insert2.InsertString("sCWFLocationId", Locationid);
                            insert2.InsertString("sCWFLocationName", Location);
                            insert2.Execute();
                             
                        }


                        catch
                        {
                            //dont do anything
                        }
                    }

                    {
                        try
                        {
                            string deptid = (api.Request[api.AddPluginPrefix("DepartmentId")].ToString());
                            string dept = (api.Request[api.AddPluginPrefix("DepartmentName")].ToString());
                                                        
                            CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFDept"));
                          //  api.Notifications.AddMessage("3");
                            insert1.InsertInt("ixProject", iproj);
                            insert1.InsertString("sCWFDeptId", deptid);
                            insert1.InsertString("sCWFDeptName", dept);
                            insert1.Execute();
                            //}
                            // }
                        }


                        catch
                        {
                            //dont do anything
                        }
                    }

                    // Adding Class to SF

                    {
                        try
                        {
                            string dept = (api.Request[api.AddPluginPrefix("DepartmentName")].ToString());

                            CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFDept"));
                            //  api.Notifications.AddMessage("3");
                            insert1.InsertInt("ixProject", iproj);
                            insert1.InsertString("sCWFDeptId", deptid);
                            insert1.InsertString("sCWFDeptName", dept);
                            insert1.Execute();
                            //}
                            // }
                        }


                        catch
                        {
                            //dont do anything
                        }
                    }



                    // adding new Approvers
                    #region adding new Approvers

                    // Level2 Approver
                    try
                    {
                        // api.Notifications.AddAdminNotification("Accnt 4", "acnt 4");

                        string L2 = (api.Request[api.AddPluginPrefix("L2id")].ToString());


                        if (L2 != null)
                        {

                            if (L2 != "")
                            {
                                try
                                {
                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.sFullName = " + "'" + L2.Trim() + "'");

                                    DataSet ds_per = pers1.GetDataSet();

                                    if (ds_per.Tables.Count > 0 && ds_per.Tables[0] != null && ds_per.Tables[0].Rows.Count > 0)
                                    {

                                        CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFapproverl2"));
                                        insert1.InsertInt("ixProject", iproj);
                                        insert1.InsertString("sCWFApproverl2Id", L2.Trim());
                                        //insert1.InsertString("sCWFVatName", vatdesc);
                                        insert1.Execute();
                                    }
                                }

                                catch
                                {
                                    //dont do anything
                                }



                            }
                        }
                    }

                    catch
                    {
                        //dont do anything
                    }

                    // Level3 Approver
                    try
                    {
                        // api.Notifications.AddAdminNotification("Accnt 4", "acnt 4");

                        string L3 = (api.Request[api.AddPluginPrefix("L3id")].ToString());


                        if (L3 != null)
                        {

                            if (L3 != "")
                            {
                                try
                                {
                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.sFullName = " + "'" + L3.Trim() + "'");

                                    DataSet ds_per = pers1.GetDataSet();

                                    if (ds_per.Tables.Count > 0 && ds_per.Tables[0] != null && ds_per.Tables[0].Rows.Count > 0)
                                    {

                                        CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFApproverl3"));
                                        insert1.InsertInt("ixProject", iproj);
                                        insert1.InsertString("sCWFApproverl3Id", L3.Trim());
                                        //insert1.InsertString("sCWFVatName", vatdesc);
                                        insert1.Execute();
                                    }
                                }

                                catch
                                {
                                    //dont do anything
                                }



                            }
                        }
                    }

                    catch
                    {
                        //dont do anything
                    }


                    // Level4 Approver
                    try
                    {
                        // api.Notifications.AddAdminNotification("Accnt 4", "acnt 4");

                        string L4 = (api.Request[api.AddPluginPrefix("L4id")].ToString());


                        if (L4 != null)
                        {

                            if (L4 != "")
                            {
                                try
                                {
                                    CPersonQuery pers1 = api.Person.NewPersonQuery();
                                    pers1.IgnorePermissions = true;
                                    pers1.AddSelect("*");
                                    pers1.AddWhere(" Person.sFullName = " + "'" + L4.Trim() + "'");

                                    DataSet ds_per = pers1.GetDataSet();

                                    if (ds_per.Tables.Count > 0 && ds_per.Tables[0] != null && ds_per.Tables[0].Rows.Count > 0)
                                    {

                                        CInsertQuery insert1 = api.Database.NewInsertQuery(api.Database.PluginTableName("CWFApproverl4"));
                                        insert1.InsertInt("ixProject", iproj);
                                        insert1.InsertString("sCWFApproverl4Id", L4.Trim());
                                        //insert1.InsertString("sCWFVatName", vatdesc);
                                        insert1.Execute();
                                    }
                                }

                                catch
                                {
                                    //dont do anything
                                }



                            }
                        }
                    }

                    catch
                    {
                        //dont do anything
                    }

                    #endregion



                }

                

            }

                /*
                
                {
                    // api.Notifications.AddMessage("action token passed");

                    ixBug = Convert.ToInt32(api.Request[api.AddPluginPrefix("ixBug")].ToString());
                    // iCopies = Convert.ToInt32(api.Request[api.AddPluginPrefix("iCopies")].ToString());

                    CBug bug = api.Bug.GetBug(ixBug);
                    bug.IgnorePermissions = true;
                    CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                    attachmentQuery.AddWhere(" Bug.ixBug = " + ixBug.ToString());
                    attachmentQuery.IgnorePermissions = true;
                    attachmentQuery.ExcludeDeleted = true;
                    DataSet ds = attachmentQuery.GetDataSet();

                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)

                    //     for (int i = 0; i < iCopies; i++)
                    {
                        CBug newbug = api.Bug.NewBug();
                        newbug.IgnorePermissions = true;
                        newbug.ixProject = bug.ixProject;
                        newbug.ixArea = bug.ixArea;
                        newbug.sTitle = bug.sTitle + "- Split " + (j + 1).ToString();
                        newbug.ixCategory = bug.ixCategory;
                        newbug.ixPersonAssignedTo = bug.ixPersonAssignedTo;
                        newbug.ixPriority = bug.ixPriority;
                        newbug.ixStatus = bug.ixStatus;
                        newbug.ixBugParent = bug.ixBug;
                        newbug.sCustomerEmail = bug.sCustomerEmail;
                        //newbug.sTitle += " - Attachment Count : ";
                        //newbug.sTitle += ds.Tables[0].Rows.Count.ToString();
                        List<CAttachment> attachments = new List<CAttachment>();
                        CAttachment attachment = api.Attachment.GetAttachment(Convert.ToInt32(ds.Tables[0].Rows[j]["ixAttachment"]));
                        attachment.IgnorePermissions = true;
                        attachments.Add(CloneAttachment(attachment, "Split" + (j + 1).ToString() + "-" + attachment.sFileName));


                        newbug.Commit("Is A Split of Case " + ixBug.ToString(), attachments.ToArray());

                        //api.Notifications.AddMessage("splitted successfuly");

                    }

                    /*
                        
                  for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                  {
                      //newbug.sTitle += "Attachment No : ";
                      //newbug.sTitle += ds.Tables[0].Rows[j]["ixAttachment"];
                      CAttachment attachment = api.Attachment.GetAttachment(Convert.ToInt32(ds.Tables[0].Rows[j]["ixAttachment"]));
                      attachment.IgnorePermissions = true;
                      attachments.Add(CloneAttachment(attachment, "Copy_" + (i + 1).ToString() + "_Of_" + attachment.sFileName));
                  }
                  
                }/////////

                */



            catch (Exception e)
            {
                api.Notifications.AddAdminNotification(e.ToString(), "Error while Segregating cases");
            }

            return string.Empty;
        }

        public PermissionLevel RawPageVisibility()
        {
            return PermissionLevel.Normal;
        }

        #endregion



        # region commit members

        public void BugCommitBefore(CBug bug, BugAction nBugAction, CBugEvent bugevent,
           bool fPublic)
        {

          //  if (bug.ixProject != 8)
           // {
                //api.Notifications.AddMessage("Not an Category of Account Payable");

            //    return;
           // }

            {
              //  ExtractValue(bug, bugevent, "sTestfield", "Test Field");
            }
        }
        private bool ExtractValue(CBug bug, CBugEvent bugevent, string fieldName, string fieldDisplay)
        {

            bool valueChanged = false;

            string sNewValue = Convert.ToString(api.Request[api.AddPluginPrefix(fieldName)]);
            if (string.IsNullOrEmpty(sNewValue))
            {
                api.Notifications.AddMessage(fieldDisplay + " is required.");
                // bug.ixStatus = 20;
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




        public void BugCommitAfter(CBug bug, BugAction nBugAction, CBugEvent bugevent,
               bool fPublic)
        {

        }
        public void BugCommitRollback(CBug bug, BugAction nBugAction, bool fPublic)
        {
        }


        #endregion

        private CBugDisplayDialogItem CreateTextInputField(CBug[] rgbug, string itemName, string fielddisplay, string fieldName)
        {
            System.Collections.IDictionary dictionary = new System.Collections.Specialized.ListDictionary();
            dictionary.Add("required", "true");
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = Forms.TextInput(api.PluginPrefix + fieldName, GetText(rgbug, fieldName), dictionary);
            return DialogItem;
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
                sValue = QueryDbForValue("TestField", sValue);
            }
            CBugDisplayDialogItem DialogItem = new CBugDisplayDialogItem(itemName);
            DialogItem.sLabel = fielddisplay;
            DialogItem.sContent = HttpUtility.HtmlEncode(sValue);
            return DialogItem;
        }


        private string QueryDbForValue(string sTableName, string sValue)
        {
            string sName = "";
            if (!string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sValue))
            {
                CSelectQuery sq = api.Database.NewSelectQuery(
                    api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sTableName));
                // sq.AddSelect("s" + sTableName + "Name");
                sq.AddSelect("s" + sTableName);
                sq.AddWhere(api.Database.PluginTableName("CGSAddNew@conseroglobal.com", sTableName) + ".ix" + sTableName + " = " + sValue);
                DataSet ds = sq.GetDataSet();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count == 1)
                {
                    sName = ds.Tables[0].Rows[0]["s" + sTableName].ToString();
                }
                ds.Dispose();
            }
            return sName;
        }




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

            CTable bugTestField = api.Database.NewTable(api.Database.PluginTableName("TestField"));
            bugTestField.sDesc = "Caputures TestField Parameters";
            bugTestField.AddAutoIncrementPrimaryKey("ixTestField");
            bugTestField.AddIntColumn("ixBug", true, 1);
            bugTestField.AddVarcharColumn("sTestfield", 20, false);

            CTable vendor = api.Database.NewTable(api.Database.PluginTableName("CWFVendor"));
            vendor.sDesc = "Caputures vendor Parameters";
            vendor.AddAutoIncrementPrimaryKey("ixCFWVendor");
            vendor.AddIntColumn("ixProject", true, 1);
            vendor.AddVarcharColumn("sCWFVendorId", 200, false);
            vendor.AddVarcharColumn("sCWFVendorName", 200, false);

            CTable account = api.Database.NewTable(api.Database.PluginTableName("CWFAccount"));
            account.sDesc = "Caputures Accounts Parameters";
            account.AddAutoIncrementPrimaryKey("ixCFWAccount");
            account.AddIntColumn("ixProject", true, 1);
            account.AddVarcharColumn("sCWFAccountId", 200, false);
            account.AddVarcharColumn("sCWFAccountName", 200, false);

            CTable vat = api.Database.NewTable(api.Database.PluginTableName("CWFVat"));
            vat.sDesc = "Caputures VAT Parameters";
            vat.AddAutoIncrementPrimaryKey("ixCFWVat");
            vat.AddIntColumn("ixProject", true, 1);
            vat.AddVarcharColumn("sCWFVatId", 200, false);
            vat.AddVarcharColumn("sCWFVatName", 200, false);


            CTable department = api.Database.NewTable(api.Database.PluginTableName("CWFDepartment"));
            department.sDesc = "Caputures Department Parameters";
            department.AddAutoIncrementPrimaryKey("ixCFWDepartment");
            department.AddIntColumn("ixProject", true, 1);
            department.AddVarcharColumn("sCWFDepartmentId", 200, false);
            department.AddVarcharColumn("sCWFDepartmentName", 200, false);

            CTable approverl2 = api.Database.NewTable(api.Database.PluginTableName("CWFapproverl2"));
            approverl2.sDesc = "Caputures approverl2 Parameters";
            approverl2.AddAutoIncrementPrimaryKey("ixCFWApproverl2");
            approverl2.AddIntColumn("ixProject", true, 1);
            approverl2.AddVarcharColumn("sCWFApproverl2Id", 200, false);
            approverl2.AddVarcharColumn("sCWFApproverl2Name", 200, false);


            CTable approverl3 = api.Database.NewTable(api.Database.PluginTableName("CWFApproverl3"));
            approverl3.sDesc = "Caputures approverl3 Parameters";
            approverl3.AddAutoIncrementPrimaryKey("ixCFWApproverl3");
            approverl3.AddIntColumn("ixProject", true, 1);
            approverl3.AddVarcharColumn("sCWFApproverl3Id", 200, false);
            approverl3.AddVarcharColumn("sCWFApproverl3Name", 200, false);


            CTable approverl4 = api.Database.NewTable(api.Database.PluginTableName("CWFApproverl4"));
            approverl4.sDesc = "Caputures approverl4 Parameters";
            approverl4.AddAutoIncrementPrimaryKey("ixCFWApproverl4");
            approverl4.AddIntColumn("ixProject", true, 1);
            approverl4.AddVarcharColumn("sCWFApproverl4Id", 200, false);
            approverl4.AddVarcharColumn("sCWFApproverl4Name", 200, false);


            CTable UserCategory = api.Database.NewTable(api.Database.PluginTableName("CWFUsercate"));
            UserCategory.sDesc = "Caputures Category Parameters";
            UserCategory.AddAutoIncrementPrimaryKey("ixCWFUsercate");
            UserCategory.AddIntColumn("ixProject", true, 1);
            UserCategory.AddVarcharColumn("sCWFUsercateId", 200, false);
            UserCategory.AddVarcharColumn("sCWFUsercateName", 200, false);

            CTable UserStatus = api.Database.NewTable(api.Database.PluginTableName("CWFUserStatus"));
            UserStatus.sDesc = "Caputures Status Parameters";
            UserStatus.AddAutoIncrementPrimaryKey("ixCWFUserStatus");
            UserStatus.AddIntColumn("ixProject", true, 1);
            UserStatus.AddVarcharColumn("sCWFUserStatusId", 200, false);
            UserStatus.AddVarcharColumn("sCWFUserStatusName", 200, false);

            CTable UserAssign= api.Database.NewTable(api.Database.PluginTableName("CWFUserAssign"));
            UserAssign.sDesc = "Caputures Assign to Parameters";
            UserAssign.AddAutoIncrementPrimaryKey("ixCWFUserAssign");
            UserAssign.AddIntColumn("ixProject", true, 1);
            UserAssign.AddVarcharColumn("sCWFUserAssignId", 200, false);
            UserAssign.AddVarcharColumn("sCWFUserAssignName", 200, false);

            CTable UserResolve = api.Database.NewTable(api.Database.PluginTableName("CWFUserResolve"));
            UserResolve.sDesc = "Caputures Resolve to Parameters";
            UserResolve.AddAutoIncrementPrimaryKey("ixCWFUserResolve");
            UserResolve.AddIntColumn("ixProject", true, 1);
            UserResolve.AddVarcharColumn("sCWFUserResolveId", 200, false);
            UserResolve.AddVarcharColumn("sCWFUserResolveName", 200, false);


            CTable Department1 = api.Database.NewTable(api.Database.PluginTableName("CWFDept"));
            Department1.sDesc = "Caputures Department Parameters";
            Department1.AddAutoIncrementPrimaryKey("ixCFWDept");
            Department1.AddIntColumn("ixProject", true, 1);
            Department1.AddVarcharColumn("sCWFDeptId", 200, false);
            Department1.AddVarcharColumn("sCWFDeptName", 200, false);

            CTable LocationID = api.Database.NewTable(api.Database.PluginTableName("CWFLocation"));
            LocationID.sDesc = "Caputures Location Parameters";
            LocationID.AddAutoIncrementPrimaryKey("ixCFWLocation");
            LocationID.AddIntColumn("ixProject", true, 1);
            LocationID.AddVarcharColumn("sCWFLocationId", 200, false);
            LocationID.AddVarcharColumn("sCWFLocationName", 200, false);

            //return new CTable[] { bugTestField };
            return new CTable[] { bugTestField, vendor, account, vat, department, approverl2, approverl3, approverl4, UserCategory, UserStatus, UserAssign, UserResolve, Department1, LocationID };
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


        private string sTableId;

        protected CEditableTable EditableTable(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Vendor");
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


            return editableTable;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new vendor to " + sProj;
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

            dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Vendorname"), ""),
                                "Vendor Name");
            dlgTemplateNew.Template.Items.Add(itemEditId2);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }


        //-------------------------Account dialogue------------------------------------

        protected CEditableTable EditableTable2(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable2 = new CEditableTable("Account");
            sTableId = editableTable2.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Add New"));
            editableTable2.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew2(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable2.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                    sTableId,
                                                    "new2",
                                                    "sDataId",
                                                    CommandUrl("new2", ixBug),
                                                    "  Account  "));

            /* Associate the dialog templates with the table by name */
            editableTable2.AddDialogTemplate("new2", dlgTemplateNew);


            return editableTable2;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew2(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new Account to " + sProj;
            dlgTemplateNew.Template.sWidth = "320px";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new2");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixBug"),
                                                   ixBug.ToString()));
            CDialogItem itemEditId =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("accountid"), ""),
                                "Account Code ");

            dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("accountdesc"), ""),
                                "Account Desc ");

            dlgTemplateNew.Template.Items.Add(itemEditId2);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }

        //-----------------------------------Department dialogue--------------------------

        protected CEditableTable EditableTable3(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable3 = new CEditableTable("Department");
            sTableId = editableTable3.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Add New"));
            editableTable3.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew3(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable3.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                    sTableId,
                                                    "new3",
                                                    "sDataId",
                                                    CommandUrl("new3", ixBug),
                                                    "Department"));

            /* Associate the dialog templates with the table by name */
            editableTable3.AddDialogTemplate("new3", dlgTemplateNew);


            return editableTable3;
        }


   

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew3(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new Department to " + sProj;
            dlgTemplateNew.Template.sWidth = "300px";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new3");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixBug"),
                                                   ixBug.ToString()));
            CDialogItem itemEditId =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("deptid"), ""),
                                "Department Code ");

            dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("deptdesc"), ""),
                                "Department Desc ");

            dlgTemplateNew.Template.Items.Add(itemEditId2);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }


        //----------------------------------VatCode dialogue------------------------------------

        protected CEditableTable EditableTable4(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable4 = new CEditableTable("VAT");
            sTableId = editableTable4.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Add New"));
            editableTable4.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew4(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable4.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                    sTableId,
                                                    "new4",
                                                    "sDataId",
                                                    CommandUrl("new4", ixBug),
                                                    " VAT "));

            /* Associate the dialog templates with the table by name */
            editableTable4.AddDialogTemplate("new4", dlgTemplateNew);


            return editableTable4;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew4(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new VAT code to " + sProj;
            dlgTemplateNew.Template.sWidth = "300px";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new4");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixBug"),
                                                   ixBug.ToString()));
            CDialogItem itemEditId =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("vatid"), ""),
                                "VAT Code ");

            dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("vatdesc"), ""),
                                "VAT Desc");

            dlgTemplateNew.Template.Items.Add(itemEditId2);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }


        //----------------------------------Approvers dialogue------------------------------------

        protected CEditableTable EditableTable5(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable5 = new CEditableTable("Approvers");
            sTableId = editableTable5.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Add New"));
            editableTable5.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew5(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable5.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                    sTableId,
                                                    "new5",
                                                    "sDataId",
                                                    CommandUrl("new5", ixBug),
                                                    "Approvers"));

            /* Associate the dialog templates with the table by name */
            editableTable5.AddDialogTemplate("new5", dlgTemplateNew);


            return editableTable5;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew5(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new Approver(s) to " + sProj;
            dlgTemplateNew.Template.sWidth = "300px";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "new5");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixBug"),
                                                   ixBug.ToString()));
            CDialogItem itemEditId =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("L2id"), ""),
                                "Level-2 approver");

            dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("L3id"), ""),
                                "Level-3 approver");

            dlgTemplateNew.Template.Items.Add(itemEditId2);

            CDialogItem itemEditId3 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("L4id"), ""),
                                "Level-4 approver");

            dlgTemplateNew.Template.Items.Add(itemEditId3);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }



        protected CEditableTable EditableTable_Loc(int ixBug)
        {

          //   api.Notifications.AddMessage("calling editable_loc table");
           //  api.Notifications.AddAdminNotification("Location", "editable_loc");
            CEditableTable editableTable_loc = new CEditableTable("Location");
            sTableId = editableTable_loc.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Add New"));
            editableTable_loc.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew_Loc(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable_loc.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                    sTableId,
                                                    "new",
                                                    "sDataId",
                                                    CommandUrl("new", ixBug),
                                                    "Location"));

            /* Associate the dialog templates with the table by name */
            editableTable_loc.AddDialogTemplate("new", dlgTemplateNew);


            return editableTable_loc;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew_Loc(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new Location to " + sProj;
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
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("Locationid"), ""),
                                "Location ID ");

            dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("LocationName"), ""),
                                "Location Name");
            dlgTemplateNew.Template.Items.Add(itemEditId2);

           // api.Notifications.AddAdminNotification("dlgTemplateNew returned", itemEditId2.ToString());
          //  api.Notifications.AddAdminNotification("Location", itemEditId2.ToString());
            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }


        //depatment

        protected CEditableTable EditableTable_Dept(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable = new CEditableTable("Department");
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
            CDialogTemplate dlgTemplateNew = DialogTemplateNew_Dept(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                    sTableId,
                                                    "new",
                                                    "sDataId",
                                                    CommandUrl("new", ixBug),
                                                    "  Department  "));

            /* Associate the dialog templates with the table by name */
            editableTable.AddDialogTemplate("new", dlgTemplateNew);


            return editableTable;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew_Dept(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new Department to " + sProj;
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
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("DepartmentId"), ""),
                                "Department ID ");

            dlgTemplateNew.Template.Items.Add(itemEditId);

            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("DepartmentName"), ""),
                                "Department Name");
            dlgTemplateNew.Template.Items.Add(itemEditId2);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }

   
        ///////////////////////////////////synergis///////////////////////////////

        //------------------------------Class for Sparefoot--------------------------------

        protected CEditableTable EditableTable_Class(int ixBug)
        {

            // api.Notifications.AddMessage("calling editable table");

            CEditableTable editableTable4 = new CEditableTable("Class");
            sTableId = editableTable4.sId;
            /* Define the header row of the table */
            //editableTable.Header.AddCell("TestUpdate for updating");

            /* create a new table row and set the row id to the unique ixtype */
            CEditableTableRow row = new CEditableTableRow();
            row.sRowId = ixBug.ToString();
            row.AddCell(HttpUtility.HtmlEncode("Add New"));
            editableTable4.Body.AddRow(row);

            /* Create the new dialog template object used when the user clicks Add
             * New type or the add icon in the footer row */
            CDialogTemplate dlgTemplateNew = DialogTemplateNew3(ixBug);

            /* Add a footer row with icon and text links to the add new dialog */
            editableTable4.Footer.AddCell(CEditableTable.LinkShowDialog(
                                                    sTableId,
                                                    "newClass",
                                                    "sDataId",
                                                    CommandUrl("newClass", ixBug),
                                                    "Department"));

            /* Associate the dialog templates with the table by name */
            editableTable4.AddDialogTemplate("newClass", dlgTemplateNew);


            return editableTable4;
        }

        /* This method builds the template for the add new dialog */
        protected CDialogTemplate DialogTemplateNew_Class(int ixBug)
        {


            CDialogTemplate dlgTemplateNew = new CDialogTemplate();
            /* There are several dialog formats to choose from */
            dlgTemplateNew.Template = new CDoubleColumnDialog();

            dlgTemplateNew.Template.sTitle = "You are adding new Class to " + sProj;
            dlgTemplateNew.Template.sWidth = "300px";

            /* FogBugz dialogs post to default.asp via AJAX. To have this form post
             * to the plugin raw page, we need to add the pg and ixPlugin values.
             * Luckily, Forms.UrlAsFormFields will convert a Url into hidden form fields
             * So we can just use api.Url.PluginRawPageUrl */
            CDialogItem itemNewHiddenUrl =
                CDialogItem.HiddenItem(Forms.UrlAsFormFields(api.Url.PluginRawPageUrl()));
            dlgTemplateNew.Template.Items.Add(itemNewHiddenUrl);
            CDialogItem itemNewHiddenAction =
                CDialogItem.HiddenInput(api.AddPluginPrefix("sAction"), "newClass");
            dlgTemplateNew.Template.Items.Add(itemNewHiddenAction);
            /* include a security action token */
            CDialogItem itemActionToken =
                CDialogItem.HiddenInput(api.AddPluginPrefix("actionToken"), api.Security.GetActionToken());
            dlgTemplateNew.Template.Items.Add(itemActionToken);
            dlgTemplateNew.Template.Items.Add(CDialogItem.HiddenInput(
                                                   api.AddPluginPrefix("ixBug"),
                                                   ixBug.ToString()));
           
            CDialogItem itemEditId2 =
                new CDialogItem(Forms.TextInput(api.AddPluginPrefix("sCWFBillableValue"), ""),
                                "Class");

            dlgTemplateNew.Template.Items.Add(itemEditId2);

            /* Standard ok and cancel buttons */
            dlgTemplateNew.Template.Items.Add(CEditableTable.DialogItemOkCancel(sTableId));

            //  api.Notifications.AddMessage("dlgTemplateNew returned");

            return dlgTemplateNew;

        }

        //---------------------------------------------------------------------

     



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





