using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net.Mail;

using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.Database;
using FogCreek.FogBugz.Database.Entity;


namespace DailyMail
{
    public class Act: Plugin, IPluginDailyTask
    {
        /* Constructor: We'll just initialize the inherited Plugin class, which 
         * takes the passed instance of CPluginApi and sets its "api" member variable. */
        public Act(CPluginApi api)
            : base(api)
        {
        }

        private string sProjectPluginId = "IntacctSettings@conseroglobal.com";
        #region IPluginDailyTask Members

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert)]
        public void DailyTask()
        {
            CProjectQuery projects = api.Project.NewProjectQuery();
            projects.IgnorePermissions = true;
            projects.ExcludeDeleted = true;
            projects.AddSelect("*");
            projects.AddWhere("sDailyMailAddress is not null");

            DataSet projectDs = projects.GetDataSet();
            if (projectDs.Tables.Count > 0 && projectDs.Tables[0] != null && projectDs.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < projectDs.Tables[0].Rows.Count; i++)
                {
                    int ixProject = Convert.ToInt32(projectDs.Tables[0].Rows[i]["ixProject"].ToString());
                    CBugQuery bugs = api.Bug.NewBugQuery();
                    bugs.IgnorePermissions = true;
                    bugs.AddSelect("*");
                    bugs.AddWhere(" Bug.ixProject = " + ixProject.ToString() + " and Bug.ixStatus = 27");

                    string sDailyMailAdderss = Convert.ToString(projectDs.Tables[0].Rows[i]["sDailyMailAddress"]);
                    if (string.IsNullOrEmpty(sDailyMailAdderss))
                    {
                        continue;
                    }
                    MailMessage mail = new MailMessage();
                    mail.To.Add(sDailyMailAdderss);
                    mail.From = new MailAddress("empower-do-not-reply@conseroglobal.com");
                    mail.Subject = Convert.ToString(projectDs.Tables[0].Rows[i]["sProject"]) + "'s Daily Payments Review Notification For " + DateTime.Now.ToShortDateString();
                    
                    string body = "The following cases are ready for payment review: ";
                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                    
                    DataSet bugsDs = bugs.GetDataSet();
                    if (bugsDs.Tables.Count > 0 && bugsDs.Tables[0] != null && bugsDs.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < bugsDs.Tables[0].Rows.Count; j++)
                        {
                            body += " Case ";
                            body += Convert.ToString(bugsDs.Tables[0].Rows[j]["ixBug"]);
                            body += " - ";
                        }
                    }
                    else 
                    {
                        body += "No Cases Are Ready For Client Payment Review";
                    }
                    bugsDs.Dispose();

                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                    
                    body += "For more details, go to ";
                    body += "http://empower.conseroglobal.com/default.asp?pgx=LF&ixFilter=4";
                    body += System.Environment.NewLine;
                    body += System.Environment.NewLine;
                    
                    body += "CONSERO BUSINESS DISCLAIMER";
                    body += System.Environment.NewLine;
                    body += "---------------------------";
                    body += System.Environment.NewLine;
                    body += "Consero Global Solutions is not a Certified Public Accounting Firm, CPA Firm, Professional Accounting Firm, or Auditing Firm. Consero is not licensed by the Texas State Board of Public Accountancy, and Consero does not prepare tax returns, provide tax advice, conduct audits, or issue assurance reports on financial statements. This message may contain confidential, proprietary or legally privileged information. In case you are not the original intended Recipient of the message, you must not, directly or indirectly, use, Disclose, distribute, print, or copy any part of this message and you are requested to delete it and inform the sender. Consero has taken enough precautions to prevent the spread of viruses. However the company accepts no liability for any damage caused by any virus transmitted by this email.";
                    body += System.Environment.NewLine;
                    mail.Body = body;
                    System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("mail.conseroglobal.com");
                    client.Credentials = new System.Net.NetworkCredential("econsero@conseroglobal.com", "econser0#123");
                    client.Send(mail);
                }
            }
            projectDs.Dispose();
        }

        #endregion
    }
}
