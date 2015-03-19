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
using System.Net.Mail;
using System.Collections;
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

namespace FogCreek.Plugins.SaveAttachmentForMLA
{

    public class Act : Plugin, IPluginBugCommit
    {
        /* Constructor: We'll just initialize the inherited Plugin class, which 
         * takes the passed instance of CPluginApi and sets its "api" member variable. */
        protected const string PLUGIN_ID =
            "SaveAttachmentForMLA@conseroglobal.com";

        /* A constant for populating the "code name" input field for multiple case edit */
        // protected const string VARIOUS_TEXT = "[various]";
        // private string sPrefixedTableName;

        public Act(CPluginApi api)
            : base(api)
        {

        }

        #region IPluginBugCommit Members

        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]

        //int Old_person_assigned = 0;

        public static void CreateDirectory(DirectoryInfo directory)
        {
            if (!directory.Parent.Exists)
                CreateDirectory(directory.Parent);
            directory.Create();
        }

        public void BugCommitAfter(CBug bug, BugAction nBugAction, CBugEvent bugevent,
          bool fPublic)
        {
            
            # region workflow control logic for EDIT MODE
            {
                // if (nBugAction == BugAction.Edit)
                if (bugevent.EventType == BugEventType.Edited || bugevent.EventType == BugEventType.Assigned)
                {

                    if (bug.ixProject != 9)
                    {
                        if (bug.ixProject != 16)
                        {
                            if (bug.ixProject != 18)
                            {
                                return;
                            }
                        }
                    }
                   
                    if (bug.ixStatus == 102)
                    {
                        //string Vendor_Name = "";
                //api.Notifications.AddMessage("calling rename");                                                                       
                        RenameFile(bug, bugevent);
                        
                    }
                    if (bug.ixStatus == 132)
                    {
                        //string Vendor_Name = "";
                      //  api.Notifications.AddMessage("calling rename cambridge");  
                        RenameFile_Cambridge(bug, bugevent);                                           
                       
                    }

                    if (bug.ixStatus == 144)
                    {
                        api.Notifications.AddMessage("calling rename cambridge");
                        api.Notifications.AddAdminNotification("calling rename cambridge", "cambridge");
                        // RenameFile_Cambridge_Status(bug, bugevent);
                        RenameFile_Trilogy(bug, bugevent);

                    }

                    if (bug.ixStatus == 135)
                    {
                        //string Vendor_Name = "";
                        api.Notifications.AddMessage("calling rename cambridge");
                        api.Notifications.AddAdminNotification("calling rename cambridge", "cambridge");  
                       // RenameFile_Cambridge_Status(bug, bugevent);
                        RenameFile_Cambridge_test(bug, bugevent);

                    }
                }

            #endregion

            }
        }


        public void BugCommitBefore(CBug bug, BugAction nBugAction, CBugEvent bugevent,
           bool fPublic)
        { }
        public void BugCommitRollback(CBug bug, BugAction nBugAction, bool fPublic)
        {
        }

        #endregion

        public void RenameFile(CBug bug, CBugEvent bugevent)
        {
            //date_vendor_invoicenumber_casenumber.pdf
            string sFileName = "";
            string sInvoiceNumber = "";
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
                CSelectQuery File_det = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                File_det.AddSelect("CWFVendor,sInvoiceNumber,sInvoiceDate,sInvoiceAmount,CWFCountry");
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
                    sAmount = Convert.ToString(Dcust.Tables[0].Rows[0]["sInvoiceAmount"]);
                    sCountry = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFCountry"]);
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
                string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Ford Direct\\" + sCountry + "\\" + sFolderdate;
                CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                attachmentQuery.AddWhere("Bug.ixBug = " + bug.ixBug.ToString());
                attachmentQuery.IgnorePermissions = true;
                attachmentQuery.ExcludeDeleted = true;
                DataSet ds = attachmentQuery.GetDataSet();
                List<CAttachment> attachments = new List<CAttachment>();

                int ixAttachment = 0;
                string sFilename2 = "";
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
                        bool Exsit = Array.Exists(attachName1, element => element == attachment.sFileName); //Array.Find(attachName[],"ads");
                       // api.Notifications.AddMessage("cnt" + "x");
                        //  api.Notifications.AddMessage("j and filname", j.ToString()+"||" +  attachName[j].ToString() +" ||"+ attachment.sFileName);
                        if (Exsit == false)
                        {
                            

                           // api.Notifications.AddMessage("j and filname", ixAttachment.ToString());
                            attachName1[j] = attachment.sFileName;
                            attachid1[j] = ixAttachment;

                        }
                        if (Exsit == true)
                        {
                            bug.DeleteAttachment(ixAttachment);
                        }

                    }//  attachName[j] = attachment.sFileName;
                  //  api.Notifications.AddMessage("cnt" + 2);
                    int cntindex= 0;
                    foreach (int i in attachid1)
                    {
                       // api.Notifications.AddMessage("cnt" + i);
                       // api.Notifications.AddMessage("cnt" + 3);
                       // api.Notifications.AddMessage("cnt" + attachid1[i].ToString());
                        int p = Convert.ToInt32(i);
                        CAttachment attachment = api.Attachment.GetAttachment(p);
                        string[] fileNameDetails = attachment.sFileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        //api.Notifications.AddMessage("j and filname",+Convert.ToInt32(p));
                        //foreach (string CurrentItem in fileNameDetails)
                        //{
                      //  api.Notifications.AddMessage("cnt" + 4);
                        //}
                        //  api.Notifications.AddMessage("att" + att);
                        // api.Notifications.AddAdminNotification("att", fileNameDetails[fileNameDetails.Length - 1]);
                        if (fileNameDetails.Length > 1)
                        {
                            string fileExtension = fileNameDetails[fileNameDetails.Length - 1];
                           // api.Notifications.AddMessage("cnt" + 5);
                            sExtn = fileExtension.ToLower();

                        }
                        
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
                               // api.Notifications.AddMessage("cnt" + 7);
                                CAttachment clonedAttachment = CloneAttachment(attachment, sFilename2);
                                attachments.Add(clonedAttachment);
                               // api.Notifications.AddMessage("cnt" + 8);
                                bugevent.CommitAttachmentAssociation(attachments.ToArray());

                                if (!string.IsNullOrEmpty(backUpLocation))
                                {
                                   // api.Notifications.AddMessage("cnt" + 10);
                                    // fileBackupPath = backUpLocation + "\\" + project.sProject + "\\" + sVendorName + "\\" + sDate + "\\" + sFileName;
                                    fileBackupPath = backUpLocation + "\\" + sFilename2;
                                    CreateDirectory(new DirectoryInfo(Path.GetDirectoryName(fileBackupPath)));
                                    // api.Notifications.AddMessage("File has been backed up as " + Path.GetFullPath(fileBackupPath));
                                  //  api.Notifications.AddMessage("Invoice has been backed up succsessfuly");

                                    FileStream fileStream = new FileStream(Path.GetFullPath(fileBackupPath), FileMode.Create, FileAccess.Write);
                                    BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                                    binaryWriter.Write(attachment.rgbData);
                                    binaryWriter.Close();
                                    fileStream.Close();
                                    fileStream.Dispose();

                                    bug.DeleteAttachment(sAttachmentold);
                                }

                            }
                        }
                        
                    }

                    ds.Dispose();
                }
               
            }
        }

        public void RenameFile_Cambridge(CBug bug, CBugEvent bugevent)
        {
            //date_vendor_invoicenumber_casenumber.pdf
            string sFileName = "";
            string sInvoiceNumber = "";
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
                CSelectQuery File_det = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                File_det.AddSelect("CWFVendor,sInvoiceNumber,sInvoiceDate,TotalAmount");
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
                    //sCountry = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFCountry"]);
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
                sFileName = sVendorName + "," + sInvoiceNumber + "," + sINVDate2 + "," + sAmount + " - " + bug.ixBug;
            }

            string attach1 = sFileName;
            // api.Notifications.AddMessage("attach1" + attach1);
            if (!string.IsNullOrEmpty(sFileName))
            {

                string fileBackupPath = "";
                CProject project = api.Project.GetProject(bug.ixProject);
                //string backUpLocation = "D:";//Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sBackupLocation"));
                //string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Default Sync Folder"
               // string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Cambridge\\" + sVendorName + sInvoiceNumber + sFolderdate + sAmount;
                string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Cambridge\\" + sFolderdate;
                CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                attachmentQuery.AddWhere("Bug.ixBug = " + bug.ixBug.ToString());
                attachmentQuery.IgnorePermissions = true;
                attachmentQuery.ExcludeDeleted = true;
                DataSet ds = attachmentQuery.GetDataSet();
                List<CAttachment> attachments = new List<CAttachment>();

                int ixAttachment = 0;
                string sFilename2 = "";
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
                        bool Exsit = Array.Exists(attachName1, element => element == attachment.sFileName); //Array.Find(attachName[],"ads");
                       // api.Notifications.AddMessage("cnt" + "x");
                        //  api.Notifications.AddMessage("j and filname", j.ToString()+"||" +  attachName[j].ToString() +" ||"+ attachment.sFileName);
                        if (Exsit == false)
                        {


                           // api.Notifications.AddMessage("cam and filname", ixAttachment.ToString());
                            attachName1[j] = attachment.sFileName;
                            attachid1[j] = ixAttachment;

                        }
                        if (Exsit == true)
                        {
                            bug.DeleteAttachment(ixAttachment);
                        }

                    }//  attachName[j] = attachment.sFileName;
                   // api.Notifications.AddMessage("cnt" + 2);
                    int cntindex = 0;
                    foreach (int i in attachid1)
                    {
                       // api.Notifications.AddMessage("cnt" + i);
                      //  api.Notifications.AddMessage("cnt" + 3);
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
                          //  api.Notifications.AddMessage("cnt" + 5);
                            sExtn = fileExtension.ToLower();

                        }

                        if (sExtn == "doc" || sExtn == "pdf" || sExtn == "bmp" || sExtn == "jpg" || sExtn == "jpeg" || sExtn == "xls" || sExtn == "xlsx" || sExtn == "docx" || sExtn == "gif" || sExtn == "tif")// || sExtn == "png")
                        {
                            //api.Notifications.AddMessage("cnt" + 6);
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
                          //  api.Notifications.AddMessage("sFilename2", sFilename2);

                            if (attachment.sFileName != sFilename2)
                            {
                                //api.Notifications.AddMessage("cnt" + 7);
                                CAttachment clonedAttachment = CloneAttachment(attachment, sFilename2);
                                attachments.Add(clonedAttachment);
                             //   api.Notifications.AddMessage("cnt" + 8);
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

                            }
                        }

                    }

                    ds.Dispose();
                }

            }
        }

        public void RenameFile_Cambridge_Status(CBug bug, CBugEvent bugevent)
        {
            //date_vendor_invoicenumber_casenumber.pdf
            string sFileName = "";
            string sInvoiceNumber = "";
            string sVendorName = "";
            DateTime sINVDate;
            string sAmount = "";
            string sINVDate2 = "";
            string sCountry = "";
            string sFolderdate = "";
            int icount = 0;
            string sFilename2 = "";

            DateTime bugdate = bug.dtOpened;
            sFolderdate = bugdate.ToString("MM.dd.yy");




            //querying Custom bugfields for invoice and vendor name to attch with mail subject start

            {
                CSelectQuery File_det = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                File_det.AddSelect("CWFVendor,sInvoiceNumber,sInvoiceDate,TotalAmount");
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
                    //sCountry = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFCountry"]);
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
                sFileName = sVendorName + "," + sInvoiceNumber + "," + sINVDate2 + "," + sAmount + " - " + bug.ixBug;
            }

            string attach1 = sFileName;
            // api.Notifications.AddMessage("attach1" + attach1);
         
            if (!string.IsNullOrEmpty(sFileName))
            {

                string fileBackupPath = "";
                CProject project = api.Project.GetProject(bug.ixProject);
                //string backUpLocation = "D:";//Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sBackupLocation"));
                //string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Default Sync Folder"
                // string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Cambridge\\" + sVendorName + sInvoiceNumber + sFolderdate + sAmount;
                string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Cambridge\\Outsource\\" + sVendorName+"\\"+sInvoiceNumber;
                CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                attachmentQuery.AddWhere("Bug.ixBug = " + bug.ixBug.ToString());
                attachmentQuery.IgnorePermissions = true;
                attachmentQuery.ExcludeDeleted = true;
                DataSet ds = attachmentQuery.GetDataSet();
                List<CAttachment> attachments = new List<CAttachment>();




                    //loop to check multiple attachments  

                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                    {

                        //sFilename2 = sFileName;
                        //int ixAttachment = Convert.ToInt32(ds.Tables[0].Rows[0]["ixAttachment"]);
                       int ixAttachment = Convert.ToInt32(ds.Tables[0].Rows[j]["ixAttachment"]);

                        CAttachment attachment = api.Attachment.GetAttachment(ixAttachment);
                        //attachment.
                        //if (icount > 0)
                        //{
                        //    attachment += "_" + icount;
                        //    sFilename2 += "_" + icount;

                        //    // icount = icount + 1;
                        //}

                        icount = icount + 1;
                        sFilename2 += ".";

                        
                        fileBackupPath = backUpLocation + "\\" + attachment.sFileName +"\\" + bug.ixBug;
                         CreateDirectory(new DirectoryInfo(Path.GetDirectoryName(fileBackupPath)));

                          FileStream fileStream = new FileStream(Path.GetFullPath(fileBackupPath), FileMode.Create, FileAccess.Write);
                          BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                          binaryWriter.Write(attachment.rgbData);
                              binaryWriter.Close();
                              fileStream.Close();
                             fileStream.Dispose();
                    }

                  //      string[] fileNameDetails = attachment.sFileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                  //      //checking for duplicate in attachment
                  //      bool Exsit = Array.Exists(attachName1, element => element == attachment.sFileName); //Array.Find(attachName[],"ads");
                  //      // api.Notifications.AddMessage("cnt" + "x");
                  //      //  api.Notifications.AddMessage("j and filname", j.ToString()+"||" +  attachName[j].ToString() +" ||"+ attachment.sFileName);
                  //      if (Exsit == false)
                  //      {


                  //          // api.Notifications.AddMessage("cam and filname", ixAttachment.ToString());
                  //          attachName1[j] = attachment.sFileName;
                  //          attachid1[j] = ixAttachment;

                  //      }
                  //      if (Exsit == true)
                  //      {
                  //          bug.DeleteAttachment(ixAttachment);
                  //      }

                  //  }//  attachName[j] = attachment.sFileName;
                  //  // api.Notifications.AddMessage("cnt" + 2);
                  //  int cntindex = 0;
                  //  //foreach (int i in attachid1)
                  ////  {
                  //      // api.Notifications.AddMessage("cnt" + i);
                  //      //  api.Notifications.AddMessage("cnt" + 3);
                  //      // api.Notifications.AddMessage("cnt" + attachid1[i].ToString());
                  //    int p = Convert.ToInt32(i);
                  //      CAttachment attachment = api.Attachment.GetAttachment(p);
                  //   //   string[] fileNameDetails = attachment.sFileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                  //      //api.Notifications.AddMessage("j and filname",+Convert.ToInt32(p));
                  //      //foreach (string CurrentItem in fileNameDetails)
                  //      //{
                  //      //api.Notifications.AddMessage("cnt" + 4);
                  //      //}
                  //      //  api.Notifications.AddMessage("att" + att);
                  //      // api.Notifications.AddAdminNotification("att", fileNameDetails[fileNameDetails.Length - 1]);
                  //      //if (fileNameDetails.Length > 1)
                  //      //{
                  //      //    string fileExtension = fileNameDetails[fileNameDetails.Length - 1];
                  //      //    //  api.Notifications.AddMessage("cnt" + 5);
                  //      //    sExtn = fileExtension.ToLower();

                  //      //}

                  //      if (sExtn == "doc" || sExtn == "pdf" || sExtn == "bmp" || sExtn == "jpg" || sExtn == "jpeg" || sExtn == "xls" || sExtn == "xlsx" || sExtn == "docx" || sExtn == "gif" || sExtn == "tif")// || sExtn == "png")
                  //      {
                  //          //api.Notifications.AddMessage("cnt" + 6);
                  //          //  int sAttachmentold = ixAttachment;
                  //          int sAttachmentold = p;
                  //          // api.Notifications.AddMessage("sAttachmentold", sAttachmentold.ToString());
                  //          if (icount > 0)
                  //          {

                  //              sFilename2 += "_" + icount;

                  //              // icount = icount + 1;
                  //          }

                  //          icount = icount + 1;
                  //          sFilename2 += ".";
                  //          sFilename2 += sExtn;

                  //          // api.Notifications.AddMessage("attachment", attachment.sFileName);
                  //          //  api.Notifications.AddMessage("sFilename2", sFilename2);

                  //         // if (attachment.sFileName != sFilename2)
                  //          //{
                  //              //api.Notifications.AddMessage("cnt" + 7);
                  //             CAttachment clonedAttachment = CloneAttachment(attachment, sFilename2);
                  //             // attachments.Add(clonedAttachment);
                  //              //   api.Notifications.AddMessage("cnt" + 8);
                  //              bugevent.CommitAttachmentAssociation(attachments.ToArray());

                  //              if (!string.IsNullOrEmpty(backUpLocation))
                  //              {
                  //                  // api.Notifications.AddMessage("cnt" + 10);
                  //                  // fileBackupPath = backUpLocation + "\\" + project.sProject + "\\" + sVendorName + "\\" + sDate + "\\" + sFileName;
                  //                  fileBackupPath = backUpLocation + "\\" + sFilename2;
                  //                  CreateDirectory(new DirectoryInfo(Path.GetDirectoryName(fileBackupPath)));
                  //                  // api.Notifications.AddMessage("File has been backed up as " + Path.GetFullPath(fileBackupPath));
                  //                  // api.Notifications.AddMessage("Invoice has been backed up succsessfuly");

                  //                  FileStream fileStream = new FileStream(Path.GetFullPath(fileBackupPath), FileMode.Create, FileAccess.Write);
                  //                  BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                  //                  binaryWriter.Write(attachment.rgbData);
                  //                  binaryWriter.Close();
                  //                  fileStream.Close();
                  //                  fileStream.Dispose();

                  //                  bug.DeleteAttachment(sAttachmentold);
                  //              }

                  //         // }
                  //      }

                   // }

                    ds.Dispose();
               // }

            }
        }

        public void RenameFile_Cambridge_test(CBug bug, CBugEvent bugevent)
        {
            //date_vendor_invoicenumber_casenumber.pdf
            string sFileName = "";
            string sInvoiceNumber = "";
            string sVendorName = "";
            DateTime sINVDate;
            string sAmount = "";
            string sINVDate2 = "";
            string sCountry = "";
            string sFolderdate = "";

            DateTime bugdate = bug.dtOpened;
            sFolderdate = bugdate.ToString("MM.dd.yy");




            //querying Custom bugfields for invoice and vendor name to attch with mail subject start

            //{
            //    CSelectQuery File_det = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
            //    File_det.AddSelect("CWFVendor,sInvoiceNumber,sInvoiceDate,TotalAmount");
            //    File_det.AddWhere("ixBug = " + bug.ixBug.ToString());


            //    DataSet Dcust = File_det.GetDataSet();

            //    if (Dcust.Tables.Count > 0 && Dcust.Tables[0] != null && Dcust.Tables[0].Rows.Count > 0)
            //    {
            //        sInvoiceNumber = Convert.ToString(Dcust.Tables[0].Rows[0]["sInvoiceNumber"]);
            //        sVendorName = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFVendor"]);
            //        try
            //        {
            //            sINVDate = Convert.ToDateTime(Dcust.Tables[0].Rows[0]["sInvoiceDate"]);
            //        }

            //        catch
            //        {
            //            return;
            //        }
            //        sAmount = Convert.ToString(Dcust.Tables[0].Rows[0]["TotalAmount"]);
            //        //sCountry = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFCountry"]);
            //        //a sInvoice will be havinf format HH:MM:SS we use below code for formating
            //        DateTime dt = sINVDate;
            //        sINVDate2 = dt.ToString("MM.dd.yy");

            //    }

            //}

            //string sDate = "";
            //string sCaseNumber = bug.ixBug.ToString();

            //if (!string.IsNullOrEmpty(sInvoiceNumber) &&
            //    !string.IsNullOrEmpty(sVendorName))
            //{
            //    DateTime now = DateTime.Now;
            //    sDate = now.Year.ToString() + "_" + now.Month.ToString() + "_" + now.Day.ToString();
            //    //sFileName = sInvoiceNumber + "" + sVendorName + "_" + sDate + "_" + sCaseNumber;
            //    sFileName = sVendorName + "," + sInvoiceNumber + "," + sINVDate2 + "," + sAmount + " - " + bug.ixBug;
            //}

            string attach1 = sFileName;
            // api.Notifications.AddMessage("attach1" + attach1);
            //if (!string.IsNullOrEmpty(sFileName))
            {

                string fileBackupPath = "";
                CProject project = api.Project.GetProject(bug.ixProject);
                //string backUpLocation = "D:";//Convert.ToString(project.GetPluginField("IntacctSettings@conseroglobal.com", "sBackupLocation"));
                //string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Default Sync Folder"
                // string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Cambridge\\" + sVendorName + sInvoiceNumber + sFolderdate + sAmount;
               // string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Cambridge\\" + sFolderdate;
                CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                attachmentQuery.AddWhere("Bug.ixBug = " + bug.ixBug.ToString());
                attachmentQuery.IgnorePermissions = true;
                attachmentQuery.ExcludeDeleted = true;
                DataSet ds = attachmentQuery.GetDataSet();
                List<CAttachment> attachments = new List<CAttachment>();

                int ixAttachment = 0;
                string sFilename2 = "";
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
                       // string[] fileNameDetails = attachment.sFileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        //checking for duplicate in attachment
                        bool Exsit = Array.Exists(attachName1, element => element == attachment.sFileName); //Array.Find(attachName[],"ads");
                        // api.Notifications.AddMessage("cnt" + "x");
                        //  api.Notifications.AddMessage("j and filname", j.ToString()+"||" +  attachName[j].ToString() +" ||"+ attachment.sFileName);
                        if (Exsit == false)
                        {

                            
                            // api.Notifications.AddMessage("cam and filname", ixAttachment.ToString());
                            attachName1[j] = attachment.sFileName;
                            attachid1[j] = ixAttachment;

                        }
                        if (Exsit == true)
                        {
                            bug.DeleteAttachment(ixAttachment);
                        }

                    }//  attachName[j] = attachment.sFileName;
                    // api.Notifications.AddMessage("cnt" + 2);
                    int cntindex = 0;
                    foreach (int i in attachid1)
                    {
                        // api.Notifications.AddMessage("cnt" + i);
                        //  api.Notifications.AddMessage("cnt" + 3);
                        // api.Notifications.AddMessage("cnt" + attachid1[i].ToString());
                        int p = Convert.ToInt32(i);
                        CAttachment attachment = api.Attachment.GetAttachment(p);
                       // string[] fileNameDetails = attachment.sFileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        string fileNameDetails = "(" +bug.ixBug.ToString() +")" + "-" +attachment.sFileName ;
                        //api.Notifications.AddMessage("j and filname",+Convert.ToInt32(p));
                        //foreach (string CurrentItem in fileNameDetails)
                        //{
                        //api.Notifications.AddMessage("cnt" + 4);
                        //}
                        //  api.Notifications.AddMessage("att" + att);
                        // api.Notifications.AddAdminNotification("att", fileNameDetails[fileNameDetails.Length - 1]);
                        //if (fileNameDetails.Length > 1)
                        //{
                        //    string fileExtension = fileNameDetails[fileNameDetails.Length - 1];
                        //    //  api.Notifications.AddMessage("cnt" + 5);
                        //    sExtn = fileExtension.ToLower();

                        //}

                       // if (sExtn == "doc" || sExtn == "pdf" || sExtn == "bmp" || sExtn == "jpg" || sExtn == "jpeg" || sExtn == "xls" || sExtn == "xlsx" || sExtn == "docx" || sExtn == "gif" || sExtn == "tif")// || sExtn == "png")
                        {
                            //api.Notifications.AddMessage("cnt" + 6);
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
                            //  api.Notifications.AddMessage("sFilename2", sFilename2);

                          //  if (attachment.sFileName != sFilename2)
                            {
                                //api.Notifications.AddMessage("cnt" + 7);
                                CAttachment clonedAttachment = CloneAttachment(attachment, fileNameDetails);
                                attachments.Add(clonedAttachment);
                                //   api.Notifications.AddMessage("cnt" + 8);
                                bugevent.CommitAttachmentAssociation(attachments.ToArray());

                             //   if (!string.IsNullOrEmpty(backUpLocation))
                                {
                                    // api.Notifications.AddMessage("cnt" + 10);
                                    // fileBackupPath = backUpLocation + "\\" + project.sProject + "\\" + sVendorName + "\\" + sDate + "\\" + sFileName;
                                   // fileBackupPath = backUpLocation + "\\" + fileNameDetails;
                                 //   CreateDirectory(new DirectoryInfo(Path.GetDirectoryName(fileBackupPath)));
                                    // api.Notifications.AddMessage("File has been backed up as " + Path.GetFullPath(fileBackupPath));
                                    // api.Notifications.AddMessage("Invoice has been backed up succsessfuly");
//----
                                    //FileStream fileStream = new FileStream(Path.GetFullPath(fileBackupPath), FileMode.Create, FileAccess.Write);
                                    //BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                                    //binaryWriter.Write(attachment.rgbData);
                                    //binaryWriter.Close();
                                    //fileStream.Close();
                                    //fileStream.Dispose();
                                    bug.DeleteAttachment(sAttachmentold);
    //----                                
                                }

                            }
                        }

                    }

                    ds.Dispose();
                }

            }
        }

        public void RenameFile_Trilogy(CBug bug, CBugEvent bugevent)
        {
            //date_vendor_invoicenumber_casenumber.pdf
            string sFileName = "";
            string sState = "";
            string sEntity = "";
            string ActivityType = "";
            string sFolderdate = "";

            DateTime bugdate = bug.dtOpened;
            sFolderdate = bugdate.ToString("MM.dd.yy");




            //querying Custom bugfields for invoice and vendor name to attch with mail subject start

            {
                CSelectQuery File_det = api.Database.NewSelectQuery(api.Database.PluginTableName("CGSInvoiceDetails_MLA@conseroglobal.com", "CGSInvoice_MLA"));
                File_det.AddSelect("CWFVendor,CWFCountry,CWFSubsidiary");
                File_det.AddWhere("ixBug = " + bug.ixBug.ToString());


                DataSet Dcust = File_det.GetDataSet();

                if (Dcust.Tables.Count > 0 && Dcust.Tables[0] != null && Dcust.Tables[0].Rows.Count > 0)
                {
                    sState = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFCountry"]);
                    sEntity = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFVendor"]);
                    ActivityType = Convert.ToString(Dcust.Tables[0].Rows[0]["CWFSubsidiary"]);
                 }

            }

            string sCaseNumber = bug.ixBug.ToString();

            if (!string.IsNullOrEmpty(sState) &&
                !string.IsNullOrEmpty(sEntity))
            {
                string state = sState.Substring(0, 3);
                string entity = sEntity.Substring(0, 2);

                sFileName = entity + "," + state + "," + ActivityType + " - " + bug.ixBug;
            }

            string attach1 = sFileName;
             api.Notifications.AddMessage("attach1" + attach1);
            if (!string.IsNullOrEmpty(sFileName))
            {

                string fileBackupPath = "";
                CProject project = api.Project.GetProject(bug.ixProject);
                
                string backUpLocation = "C:\\Users\\rbabu.CONSEROGLOBAL\\Documents\\My Box Files\\Trilogy\\" + sFolderdate;
                CAttachmentQuery attachmentQuery = api.Attachment.NewAttachmentQuery();
                attachmentQuery.AddWhere("Bug.ixBug = " + bug.ixBug.ToString());
                attachmentQuery.IgnorePermissions = true;
                attachmentQuery.ExcludeDeleted = true;
                DataSet ds = attachmentQuery.GetDataSet();
                List<CAttachment> attachments = new List<CAttachment>();

                int ixAttachment = 0;
                string sFilename2 = "";
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
                        bool Exsit = Array.Exists(attachName1, element => element == attachment.sFileName); //Array.Find(attachName[],"ads");
                        // api.Notifications.AddMessage("cnt" + "x");
                        //  api.Notifications.AddMessage("j and filname", j.ToString()+"||" +  attachName[j].ToString() +" ||"+ attachment.sFileName);
                        if (Exsit == false)
                        {


                            // api.Notifications.AddMessage("cam and filname", ixAttachment.ToString());
                            attachName1[j] = attachment.sFileName;
                            attachid1[j] = ixAttachment;

                        }
                        if (Exsit == true)
                        {
                            bug.DeleteAttachment(ixAttachment);
                        }

                    }//  attachName[j] = attachment.sFileName;
                    // api.Notifications.AddMessage("cnt" + 2);
                    int cntindex = 0;
                    foreach (int i in attachid1)
                    {
                        // api.Notifications.AddMessage("cnt" + i);
                        //  api.Notifications.AddMessage("cnt" + 3);
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
                            //  api.Notifications.AddMessage("cnt" + 5);
                            sExtn = fileExtension.ToLower();

                        }

                        if (sExtn == "doc" || sExtn == "pdf" || sExtn == "bmp" || sExtn == "jpg" || sExtn == "jpeg" || sExtn == "xls" || sExtn == "xlsx" || sExtn == "docx" || sExtn == "gif" || sExtn == "tif")// || sExtn == "png")
                        {
                            //api.Notifications.AddMessage("cnt" + 6);
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
                            //  api.Notifications.AddMessage("sFilename2", sFilename2);

                            if (attachment.sFileName != sFilename2)
                            {
                                //api.Notifications.AddMessage("cnt" + 7);
                                CAttachment clonedAttachment = CloneAttachment(attachment, sFilename2);
                                attachments.Add(clonedAttachment);
                                //   api.Notifications.AddMessage("cnt" + 8);
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


    }
}





