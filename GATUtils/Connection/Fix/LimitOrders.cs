using System.Collections.Generic;

namespace GATUtils.Connection.Fix
{
    public class LimitOrders
    {        
        public static Dictionary <string , string> MostRecentId;

        public LimitOrders()
        {
            MostRecentId = new Dictionary<string,string>();
        }

        public void Add(string newLimitOrderId, string recentLimitId = "")
        {
            if (newLimitOrderId == "")
            {
                return;
            }

            if (MostRecentId.ContainsKey(newLimitOrderId) && recentLimitId != "")
            {
                MostRecentId[newLimitOrderId] = recentLimitId;
            }
            else
            {
                MostRecentId.Add(newLimitOrderId, recentLimitId);
            }
        }

        public string GetRecentId(string originalOrderId)
        {
            if (MostRecentId.ContainsKey(originalOrderId))
            {
                if (MostRecentId[originalOrderId] != "")
                    return MostRecentId[originalOrderId];
            }

            return null;
        }
                
    }
}