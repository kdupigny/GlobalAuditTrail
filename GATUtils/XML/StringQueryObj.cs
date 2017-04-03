using System.Collections.Generic;
using System.Linq;
using GATUtils.Utilities;

namespace GATUtils.XML
{
    public class StringQueryObj
    {
        public QryTrigger Trigger { get { return _trigger; } }
        public string SqlQry { get { return _qry; } }
        public string TriggerData { get { return _triggerData; } }

        public StringQueryObj(string queryTrigger, string rawQuery, Dictionary<string, string> valueMap)
        {
            string[] triggerPieces = queryTrigger.Split(new[]{'|'});
            _trigger = (QryTrigger) EnumUtil.GetEnumFromString(typeof(QryTrigger), triggerPieces[0]);

            switch (Trigger)
            {
                case QryTrigger.AtTime:
                case QryTrigger.Condition:
                    _triggerData = triggerPieces[1];
                    break;
            }

            _qry = s_PrepQuery(rawQuery, valueMap);
        }

        private static string s_PrepQuery(string rawQuery, Dictionary<string, string> valueMap)
        {
            string outputQry = XmlTools.ResolveOperators(rawQuery);

            return valueMap.Aggregate(outputQry, (current, data) => current.Replace(data.Key, data.Value));
        }
        private readonly string _qry;
        private readonly QryTrigger _trigger;
        private readonly string _triggerData;
    }

    public enum QryTrigger { Always, AtTime, Condition }
}