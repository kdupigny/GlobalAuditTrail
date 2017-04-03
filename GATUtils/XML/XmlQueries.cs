using System.Collections.Generic;
using GATUtils.Logger;
using System.Xml;

namespace GATUtils.XML
{
    public class XmlQueries
    {
        public static XmlQueries Instance { get { return _instance ?? (_instance = new XmlQueries()); } }

        public XmlQueries(Dictionary<string, string> rawQueries)
        {
            _Init();
            _rawQueries = rawQueries;
            _instance = this;
        }

        private XmlQueries()
        {
            _rawQueries = new Dictionary<string, string>();
            _Init();
        }

        public StringQueryObj GenerateQueryObject(string queryName, Dictionary<string, string> valueMap)
        {
            string rawQuery;
            if (_rawQueries.TryGetValue(queryName, out rawQuery))
            {
                string[] queryPieces = rawQuery.Split(ct_triggerQryDelimiter.ToCharArray());
                return new StringQueryObj(queryPieces[0], queryPieces[1], valueMap);
            }
            GatLogger.Instance.AddMessage(string.Format("Unknown Query [{0}] please check XML configuration", queryName));
            return null;
        }

        private void _Init()
        {
            XmlDocument settings = XmlSettings.Instance.SettingDocument;

            XmlNodeList nonQueries = settings.GetElementsByTagName("NonQuery");

            foreach (XmlNode qry in nonQueries)
            {
                string name = string.Empty;
                string trigger = string.Empty;
                string sql = string.Empty;

                foreach (XmlNode innerNode in qry)
                {
                    switch (innerNode.Name)
                    {
                        case "Name":
                            name = innerNode.InnerText.Trim();
                            break;
                        case "Trigger":
                            trigger = innerNode.InnerText.Trim();
                            break;
                        case "Sql":
                            sql = innerNode.InnerText.Trim();
                            break;
                    }    
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(sql))
                {
                    GatLogger.Instance.AddMessage(string.Format("Incorrectly specified query. NAME: {0} TRIGGER: {1} SQL: {2}", name, trigger, qry));
                }
                else
                {
                    _rawQueries.Add(name, string.Format("{0}{1}{2}", trigger,ct_triggerQryDelimiter,sql));
                }
            }
        }

        private readonly Dictionary<string, string> _rawQueries;
        private static XmlQueries _instance;

        private const string ct_triggerQryDelimiter = "$";

    }
}
