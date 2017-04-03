using System;
using System.Collections.Generic;
using GATUtils.Logger;
using System.Xml;
using GATUtils.Types.Email;
using GATUtils.XML;

namespace GATUtils.Generic
{
    public class WbEmailSettings
    {
        public bool IsActive { get; private set; }
        public string ServerName { get; set; }
        public string MailServer { get { return _mailServer; } }
        public string Username { get { return _mailUsername; } }
        public string Password { get { return _mailPass; } }
        public string FromAddress { get { return _mailFrom; } }
        public int Port { get { return _port; } }

        public string DefaultSubject { get { return _defaultSubject; } }

        public Dictionary<string, List<string>> Recipients { get { return _recipientLists; } }
        
        public WbEmailSettings(string host, int port, string username, string password, string fromAddress)
        { 
            _Init(host, port, username, password, fromAddress);
        }

        public WbEmailSettings(XmlDocument emailSettingsDoc)
        {
            _Init(emailSettingsDoc);
        }

        public IEmailJob GetJob(string jobName, string emailJobType, string [] subjectDetails = null, string [] bodyDetails = null, Object[] additionalData = null)
        {
            GeneralEmailJob builtJob = new GeneralEmailJob();
            builtJob.SetJobParameters(_jobBlocks[jobName], _recipientLists);
            builtJob.SetSubjectDetails(subjectDetails);
            builtJob.SetBodyDetails(bodyDetails);
            builtJob.SetAdditionalDetail(additionalData);

            return builtJob;
        }

        public List<string> GetRecipientList(List<string> mailingLists)
        {
            List<string> output = new List<string>();

            foreach (string listName in mailingLists)
            {
                List<string> temp;
                if (Recipients.TryGetValue(listName, out temp))
                {
                    output.AddRange(temp);
                }
                else
                    GatLogger.Instance.AddMessage(string.Format("Invalid Mailing List '{0}'", listName));
            }

            return output;
        }

        public void AddRecipientList(string listName, List<string> recipients)
        {
            if (!_recipientLists.ContainsKey(listName))
            {
                _recipientLists[listName] = recipients;
            }
            else
                GatLogger.Instance.AddMessage(string.Format("Recipient List '{0}({1})' has already been added to listing.", listName, recipients.Count));
        }

        private void _Init(string host, int port, string username, string password, string fromAddress)
        {
            _mailServer = host;
            _port = port;
            _mailUsername = username;
            _mailPass = password;
            _mailFrom = fromAddress;
        }

        private void _Init(XmlDocument emailSettingsDoc)
        {
            _defaultSubject = "WB Robot Emailer";
            XmlNode xmlNode;
            try
            {
                xmlNode = emailSettingsDoc.GetElementsByTagName("DefaultSubject").Item(0);
                if (xmlNode != null)
                    _defaultSubject = xmlNode.InnerText;
            }
            catch (NullReferenceException)
            { }

            try
            {
                xmlNode = emailSettingsDoc.GetElementsByTagName("Active").Item(0);
                IsActive = XmlTools.ResolveBooleanValue(xmlNode.InnerText.Trim());
            }
            catch (NullReferenceException)
            { }

            XmlNodeList serverBlock = emailSettingsDoc.GetElementsByTagName("EmailSender");
            _LoadSenderSettings(serverBlock);

            XmlNodeList mailingListsBlock = emailSettingsDoc.GetElementsByTagName("MailingList");
            _LoadMailingLists(mailingListsBlock);

            XmlNodeList emailJobBlock = emailSettingsDoc.GetElementsByTagName("EmailJob");
            _LoadEmailJobs(emailJobBlock);
        }

        private void _LoadEmailJobs(XmlNodeList emailJobBlock)
        {
            if (emailJobBlock == null || emailJobBlock.Count == 0) return;

            foreach (XmlNode emailJob in emailJobBlock)
            {
                XmlNode currentJob = emailJob.SelectSingleNode("JobName");
                if (currentJob == null) continue;

                string jobName = currentJob.InnerText.Trim();
                _jobBlocks.Add(jobName,emailJob);
            }
        }

        private void _LoadMailingLists(XmlNodeList mailingListBlock)
        {
            if (mailingListBlock == null || mailingListBlock.Count == 0) return;

            foreach (XmlNode mailingList in mailingListBlock)
            {
                string listName = null;
                List<string> recipients = new List<string>();

                foreach (XmlNode innerNode in mailingList)
                {
                    switch (innerNode.Name)
                    {
                        case "Name":
                            listName = innerNode.InnerText.Trim();
                            break;
                        case "EmailAddress":
                            recipients.Add(innerNode.InnerText.Trim());
                            break;
                    }
                }

                if (listName != null && !_recipientLists.ContainsKey(listName)) 
                    _recipientLists.Add(listName, recipients);
            }
        }

        private void _LoadSenderSettings(XmlNodeList senderBlock)
        {
            if (senderBlock == null || senderBlock.Count == 0) return;

// ReSharper disable PossibleNullReferenceException
            foreach (XmlNode innerNode in senderBlock.Item(0))
// ReSharper restore PossibleNullReferenceException
            {
                switch (innerNode.Name)
                {
                    case "ServerName":
                        ServerName = innerNode.InnerText.Trim();
                        break;
                    case "MailServer":
                        _mailServer = innerNode.InnerText.Trim();
                        break;
                    case "Port":
                        if (!int.TryParse(innerNode.InnerText.Trim(), out _port))
                            _port = 587;
                        break;
                    case "Username":
                        _mailUsername = innerNode.InnerText.Trim();
                        break;
                    case "Password":
                        _mailPass = innerNode.InnerText.Trim();
                        break;
                    case "FromAddress":
                        _mailFrom = innerNode.InnerText.Trim();
                        break;
                }
            }
        }

        private readonly Dictionary<string, XmlNode> _jobBlocks = new Dictionary<string, XmlNode>();
        private readonly Dictionary<string, List<string>> _recipientLists = new Dictionary<string, List<string>>();

        private string _defaultSubject = "GAT Email";
        private string _mailServer = "owa.smarshexchange.com";
        private string _mailUsername = "egulkowitz@ravensecurities.com";
        private string _mailPass = "Elma0323";
        private string _mailFrom = "Operations@ravensecurities.com";
        private int _port = 587;
    }
}
