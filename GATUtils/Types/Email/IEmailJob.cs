using System;
using System.Collections.Generic;
using System.Xml;

namespace GATUtils.Types.Email
{
    public interface IEmailJob
    {
        string JobName {get;}
        string Subject { get; }
        string Body { get; }
        List<string> Recipients { get; }

        bool HasAttachment { get; }
        List<string> AttachmentFilepath { get; }

        void SetAttachment(string attachmentFilePath);
        void SetAttachment(List<string> attachmentList);
        void SetJobParameters(XmlNode jobSettings, Dictionary<string, List<string>> mailingLists);
        void SetBodyDetails(string[] details);
        void SetSubjectDetails(string[] details);
        void SetAdditionalDetail(Object[] addtionalData);

        void AppendBodyPrefix(string prefix);
        void AppendBodySuffix(string suffix);
    }
}
