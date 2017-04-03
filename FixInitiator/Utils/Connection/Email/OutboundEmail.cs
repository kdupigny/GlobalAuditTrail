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
            _Init();
        }

        public OutboundEmail(IEmailJob emailJob)
            : this()
        {
            EmailFile(emailJob);
        }
                
        public OutboundEmail(string jobName, string strSubject, string body, List<string> toList, string attachmentPath = null, bool isHtmlMessage = false)
            : this()
        {
            EmailFile(jobName, strSubject, body, toList, attachmentPath, isHtmlMessage);
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
            MailMessage mail = new MailMessage {From = new MailAddress(_strMailFrom), Priority = MailPriority.High};

            foreach (string recipient in recipients)
                mail.To.Add(new MailAddress(recipient));

            mail.Subject = strSubject;
            mail.IsBodyHtml = isHtml;
            mail.Body = body;

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
            WBEmailSettings mailSettings = XmlSettings.Instance.WbEmail;
            SetEmailMailSource(mailSettings.MailServer, mailSettings.Port, mailSettings.Username, mailSettings.Password, mailSettings.FromAddress);
        }

        private string _strMailServer;
        private int _port;
        private string _strMailUsername;
        private string _strMailPass;
        private string _strMailFrom;
    }
}
