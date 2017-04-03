using System.Collections.Generic;

namespace GATUtils.Fix
{
    public class GatFixMessage
    {
        public QuickFix.Message Message { get; private set; }
        public string Source { get; private set; }
        public List<KeyValuePair<FixTag, string>> TagToDBFieldMap { get; private set; }

        public GatFixMessage(QuickFix.Message message, string source, List<KeyValuePair<FixTag, string>> fieldMap)
        {
            Message = message;
            Source = source;
            TagToDBFieldMap = fieldMap;
        }
    }
}
