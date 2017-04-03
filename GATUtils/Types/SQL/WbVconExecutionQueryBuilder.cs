using System.Collections.Generic;
using System.Linq;
using GATUtils.XML;

namespace GATUtils.Types.SQL
{
    public class WbVconExecutionQueryBuilder
    {
        public WbVconExecutionQueryBuilder()
        {
            Map = new Dictionary<string, string>
                      {
                          {"xdate", " "},{"side", " "},{"email_time", " "},{"basket", " "},{"quantity", "0"},{"account", " "},
                          {"accrued", "0"},{"price", "0"},{"cusip", " "},{"origquantity", "0"},{"net", "0"},
                          {"broker", " "},{"broker_name", " "},{"status", " "},{"issue", " "},{"principal", "0"},{"trade_date", " "},
                          {"settle_date", " "},{"spread", "0"},{"trade_time", " "},{"customer", " "},{"user", " "}, {"source", " "}
                      };

        }

        public Dictionary<string, string> Map;

        //public static WbVconExecutionQueryBuilder operator +(WbVconExecutionQueryBuilder a, WbVconExecutionQueryBuilder b)
        //{
        //    WbVconExecutionQueryBuilder c = new WbVconExecutionQueryBuilder();

        //    return c;
        //}

        public string this[string key]
        {
            get { return Map[key]; }
            set { Map[key] = value; }
        }

        public string GetFields()
        {
            string[] keys = new string[Map.Keys.Count];
            Map.Keys.CopyTo(keys, 0);

            string result = keys.Aggregate("", (current, t) => current + t + ",");

            return result.Trim(',');
        }

        public string GetValues()
        {
            List<string> nonTextTypes = new List<string>
                                            {
                "quantity", "accrued","price","origquantity","net","principal","spread"
            };
            string[] keys = new string[Map.Keys.Count];
            Map.Keys.CopyTo(keys, 0);
            string results = "(";
            string val;
            foreach (string t in keys)
            {
                val = Map[t];
                if (nonTextTypes.Contains(t))
                    results = results + ((val.Equals("null") || string.IsNullOrEmpty(val.Trim())) ? "0," : val + ",");
                else
                    results = results + "'" + val + "',";
            }
            return results.Trim(',') + ")";
        }

        public override string ToString()
        {
            return string.Format("insert into {2} ({0}) values{1};", GetFields(), GetValues(), "vcon_trades");
        }

    }
}
