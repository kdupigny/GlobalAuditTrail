using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using GATUtils.Types.Email;
using GATUtils.Logger;
using GATUtils.XML;
using GATUtils.Generic;


namespace GATUtils.Connection.Email
{
    public class OutboundEmail
    {
        public OutboundEmail()
        {
            //_Init();
        }

        public OutboundEmail(IEmailJob emailJob)
        {
            if (!XmlSettings.Instance.WbEmail.IsActive)
            {
                GatLogger.Instance.AddMessage(string.Format("Email Job [{0}] was halted.  Email System Inactive", emailJob.JobName), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage(emailJob.Body);
                return;
            }
            _Init();
            EmailFile(emailJob);
        }
                
        public OutboundEmail(string jobName, string strSubject, string body, List<string> toList, string attachmentPath, bool isHtmlMessage = false)
        {
            _Init();
            List<string> attachments = new List<string> {attachmentPath};
            EmailFile(jobName, strSubject, body, toList, attachments, isHtmlMessage);
        }

        public OutboundEmail(string jobName, string strSubject, string body, List<string> toList, List<string> attachments, bool isHtmlMessage = false)
        {
            _Init();
            EmailFile(jobName, strSubject, body, toList, attachments, isHtmlMessage);
        }
                
        public void SetEmailMailSource(string mailServerAddress, int port, string username, string password, string emailFromAddress)
        {
            _strMailServer = mailServerAddress;
            _port = port;
            _strMailUsername = username;
            _strMailPass = password;            
            _strMailFrom = emailFromAddress;
        }

        public void EmailFile(IEmailJob emailJob)
        {
            EmailFile(emailJob.JobName, emailJob.Subject, emailJob.Body, emailJob.Recipients, emailJob.AttachmentFilepath);
        }

        public void EmailFile(string emailJobName, string strSubject, string body, List<string> recipients, string attachmentPath, bool isHtml = false)
        {
            List<string> attachments = new List<string>{ attachmentPath };
            EmailFile(emailJobName, strSubject, body, recipients, attachments, isHtml);
        }

        public void EmailFile(string emailJobName, string strSubject, string body, List<string> recipients, List<string> attachmentPaths, bool isHtml = false)
        {
            MailMessage mail = new MailMessage {From = new MailAddress(_strMailFrom), Priority = MailPriority.High};

            foreach (string recipient in recipients)
                mail.To.Add(new MailAddress(recipient));

            mail.Subject = strSubject;
            mail.IsBodyHtml = isHtml;
            mail.Body = body;

            if(attachmentPaths != null)
            foreach (string attachmentPath in attachmentPaths)
            {
                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    // Create  the file attachment for this e-mail message.
                    Attachment data = new Attachment(attachmentPath, MediaTypeNames.Application.Octet);
                    // Add time stamp information for the file.
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = File.GetCreationTime(attachmentPath);
                    disposition.ModificationDate = File.GetLastWriteTime(attachmentPath);
                    disposition.ReadDate = File.GetLastAccessTime(attachmentPath);
                    // Add the file attachment to this e-mail message.
                    mail.Attachments.Add(data);
                }
                else
                {
                    GatLogger.Instance.AddMessage(string.Format("ALERT {1} : Attachment {0} could not be found. Email [{2}]", attachmentPath, emailJobName, strSubject));
                }
            }
            _SendEmail(mail, emailJobName);            
        }

        private void _SendEmail(MailMessage mail, string jobName)
        {
            SmtpClient client = new SmtpClient(_strMailServer)
                                    {
                                        Port = _port,
                                        Credentials = new System.Net.NetworkCredential(_strMailUsername, _strMailPass)
                                    };

            client.Send(mail);
            GatLogger.Instance.AddMessage(string.Format("Email Job ({0}) has completed successfully.", jobName));
        }

        private void _Init()
        {
            WbEmailSettings mailSettings = XmlSettings.Instance.WbEmail;
            SetEmailMailSource(mailSettings.MailServer, mailSettings.Port, mailSettings.Username, mailSettings.Password, mailSettings.FromAddress);
        }

        private string _strMailServer;
        private int _port;
        private string _strMailUsername;
        private string _strMailPass;
        private string _strMailFrom;
    }
}
