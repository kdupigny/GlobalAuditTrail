using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using GATUtils.Logger;
using Tamir.SharpSsh.java;

namespace GATUtils.XML
{
    public static class XmlTools
    {
        public static bool ResolveBooleanValue(string fieldValue)
        {
            bool boolFieldValue = false;
            fieldValue = fieldValue.ToUpper();

            if (fieldValue.Equals("TRUE") || fieldValue.Equals("YES"))
                boolFieldValue = true;

            return boolFieldValue;
        }

        public static string ResolveOperators(string input)
        {
            string output = input.Replace("|GREATERTHAN|", ">");
            output = output.Replace("|GREATEREQUAL|", ">=");
            output = output.Replace("|LESSTHAN|", "<");
            output = output.Replace("|LESSEQUAL|", "<=");

            output = output.Replace("|PLUS|", "+");
            output = output.Replace("|MINUS|", "-");
            output = output.Replace("|DEVIDE|", "/");
            output = output.Replace("|MULTIPLY|", "*");

            output = output.Replace("|AMPERSAND|", "&");

            return output;
        }

        public static XmlNode GetChildNode(XmlNode sourceNode, string childTag)
        {
            return sourceNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(child => child.Name.Equals(childTag));
        }

        public static string GetXmlSetting(XmlNode settingNode, string xmlPath, string defaultValue)
        {
            XmlNode tempNode = settingNode.ChildNodes
                   .Cast<XmlNode>().FirstOrDefault(child => child.Name.Equals(xmlPath));

            if (tempNode != null) return tempNode.InnerText;

            GatLogger.Instance.AddMessage(string.Format("XmlSetting [{0}] was not found. Using defalut value \"{1}\"", xmlPath, defaultValue));
            return defaultValue;
        }

        public static int GetXmlSetting(XmlNode settingNode, string xmlPath, int defaultValue)
        {
            XmlNode tempNode = settingNode.ChildNodes
                   .Cast<XmlNode>().FirstOrDefault(child => child.Name.Equals(xmlPath));

            int value;
            if (tempNode != null && int.TryParse(tempNode.InnerText, out value)) return value;

            GatLogger.Instance.AddMessage(string.Format("XmlSetting [{0}] was not found. Using defalut value \"{1}\"", xmlPath, defaultValue));
            return defaultValue;
        }

        public static long GetXmlSetting(XmlNode settingNode, string xmlPath, long defaultValue)
        {
            XmlNode tempNode = settingNode.ChildNodes
                   .Cast<XmlNode>().FirstOrDefault(child => child.Name.Equals(xmlPath));

            long value;
            if (tempNode != null && long.TryParse(tempNode.InnerText, out value)) return value;

            GatLogger.Instance.AddMessage(string.Format("XmlSetting [{0}] was not found. Using defalut value \"{1}\"", xmlPath, defaultValue));
            return defaultValue;
        }

        public static bool GetXmlSetting(XmlNode settingNode, string xmlPath, bool defaultValue)
        {
            XmlNode tempNode = settingNode.ChildNodes
                   .Cast<XmlNode>().FirstOrDefault(child => child.Name.Equals(xmlPath));

            bool value = defaultValue;
            if (tempNode != null)
            {
                value = ResolveBooleanValue(tempNode.InnerText);
                return value;
            }

            GatLogger.Instance.AddMessage(string.Format("XmlSetting [{0}] was not found. Using defalut value \"{1}\"", xmlPath, defaultValue));
            return value;
        }
    }
}
