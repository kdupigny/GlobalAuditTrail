using System;
using GATUtils.Types.Email;
using GATUtils.Utilities;
using GATUtils.XML;

namespace GATUtils.Connection.Email
{
    public class SystemMailer
    {
        public static SystemMailer Instance { get { return (s_instance ?? (s_instance = new SystemMailer())); } }

        /// <summary>
        /// Sends a formatted email to the development team
        /// </summary>
        /// <param name="e">The thrown exception.</param>
        /// <param name="bodyPrefix">The body prefix. will appear before the main text of the body</param>
        /// <param name="bodySuffix">The body suffix. will appear after the main text of the body</param>
        public void SendDevEmail(Exception e, string bodyPrefix = "", string bodySuffix = "")
        {
            Exception inner = null;

            if (e == null)
                e = new Exception("General Alert");
            else 
                inner = e.InnerException;

            string[] subject = { MyTime.TodaysDateStrYYYYMMDD, e.GetType().ToString() };
            string[] body = { e.GetType().ToString(), e.Message, e.StackTrace ?? " null ", (inner != null ? inner.Message : " null "), (inner != null ? inner.StackTrace : " null ") };

            IEmailJob emailJob = XmlSettings.Instance.WbEmail.GetJob(EmailJobTypes.SendDevNotice, "Alert", subject, body);

            if (!string.IsNullOrEmpty(bodyPrefix)) emailJob.AppendBodyPrefix(bodyPrefix);
            if (!string.IsNullOrEmpty(bodySuffix)) emailJob.AppendBodySuffix(bodySuffix);

            var outbound = new OutboundEmail(emailJob);
        }

        /// <summary>
        /// Sends a formatted email to the compliance mailing list.
        /// </summary>
        /// <param name="noticeTitle">The notice title.</param>
        /// <param name="mainText">The main text of the email.</param>
        /// <param name="bodyPrefix">The body prefix. will appear before the main text of the body</param>
        /// <param name="bodySuffix">The body suffix. will appear after the main text of the body</param>
        public void SendComplianceEmail(string noticeTitle, string mainText, string bodyPrefix = "", string bodySuffix = "")
        {
            string[] subject = { MyTime.TodaysDateStrYYYYMMDD, noticeTitle };
            string[] body = { mainText };

            IEmailJob emailJob = XmlSettings.Instance.WbEmail.GetJob(EmailJobTypes.SendComplianceNotice, "Alert", subject, body);

            if (!string.IsNullOrEmpty(bodyPrefix)) emailJob.AppendBodyPrefix(bodyPrefix);
            if (!string.IsNullOrEmpty(bodySuffix)) emailJob.AppendBodySuffix(bodySuffix);

            var outbound = new OutboundEmail(emailJob);
        }

        private static SystemMailer s_instance;
    }
}
