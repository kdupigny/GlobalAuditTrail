using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace GATUtils.Types.Email
{
    public class GeneralEmailJob : IEmailJob
    {
        public virtual string JobName { get { return _Job; } }

        public virtual string Subject
        {
            get
            {
                string subject = _Subject;
                if (_subjectDetails != null)
                {
                    object[] objs = new object[_subjectDetails.Length];
                    _subjectDetails.CopyTo(objs, 0);
                    subject = string.Format(subject, objs);
                }
                return subject;
            }
        }

        public virtual string Body
        {
            get
            {
                string body = _Body;
                if (_bodyDetails != null)
                {
                    object [] objs = new object[_bodyDetails.Length];
                    _bodyDetails.CopyTo(objs, 0);

                    body = string.Format(body, objs);
                }
                return body;
            }
        }

        public List<string> Recipients { get { return _MailList; } }
        public bool HasAttachment { get { return _HasAttachment; } }
        public List<string> AttachmentFilepath { get { return _AttachementPaths; } }

        public GeneralEmailJob()
        { }

        public GeneralEmailJob(string jobName, string subject, string body, List<string> addressList, bool attachmentExpected = false)
        {
            _Init(jobName, subject, body, addressList, attachmentExpected);
        }

        public void SetAttachment(string attachmentFilePath)
        {
            if (_AttachementPaths == null)
            {
                _AttachementPaths = new List<string>();
                _HasAttachment = false;
            }

            if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
            {
                _HasAttachment = true;
                _AttachementPaths.Add(attachmentFilePath);
            }
        }

        public void SetAttachment(List<string> attachmentList)
        {
            foreach (string attachment in attachmentList)
            {
                SetAttachment(attachment);
            }
        }

        public virtual void SetJobParameters(XmlNode jobSettings, Dictionary<string, List<string>> mailingLists)
        {
            _Init(jobSettings, mailingLists);
        }

        public virtual void SetAdditionalDetail(Object[] additionalData)
        {  }

        public void AppendBodyPrefix(string prefix)
        {
            _Body = prefix + "\n\n" + _Body;
        }

        public void AppendBodySuffix(string suffix)
        {
            _Body = _Body + "\n\n" + suffix;
        }

        public virtual void SetBodyDetails(string[] details)
        {
            _bodyDetails = details;
        }

        public virtual void SetSubjectDetails(string[] details)
        {
            _subjectDetails = details;
        }

        protected void _Init(string jobName, string emailSubject, string emailBody, List<string> addressList, bool attachmentExpected)
        {
            _Job = (string.IsNullOrEmpty(jobName) ? _Job : jobName);
            
            _Subject = emailSubject;
            _Body = emailBody;

            _MailList = new List<string>();
            _MailList.AddRange(addressList);

            _HasAttachment = attachmentExpected;
        }

        protected void _Init(XmlNode jobSettings, Dictionary<string, List<string>> mailingLists)
        {
            _MailList = new List<string>();
            foreach (XmlNode innerNode in jobSettings)
            {
                switch (innerNode.Name)
                {
                    case "JobName":
                        _Job = innerNode.InnerText.Trim();
                        break;
                    case "Subject":
                        _Subject = innerNode.InnerText.Trim();
                        break;
                    case "Body":
                        _Body = innerNode.InnerText.Trim();
                        break;
                    case "AttachmentExpected":
                        _HasAttachment = (innerNode.InnerText.Trim().ToUpper().Equals("YES"));
                        break;
                    case "Recipients":
                        foreach (XmlNode listNode in innerNode)
                        {
                            string listName = listNode.InnerText.Trim();
                            List<string> temp;
                            if (mailingLists.TryGetValue(listName, out temp))
                            {
                                _MailList.AddRange(temp);
                            }
                        }
                        break;
                }
            }
        }

        protected string _Job = "GeneralEmailJob";
        protected string _Subject;
        protected string _Body;
        protected List<string> _MailList;
        protected string _AlternateSender;

        protected bool _HasAttachment;
        protected List<string> _AttachementPaths;

        private string[] _bodyDetails;
        private string[] _subjectDetails;
    }
}
