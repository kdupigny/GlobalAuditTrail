using System.Collections.Generic;
using System.Linq;
using GATUtils.XML;

namespace GATUtils.Types.SQL
{
    public class WbAuditTrailExecutionQueryBuilder
    {
        public WbAuditTrailExecutionQueryBuilder()
        {
            Map = new Dictionary<string, string>
                      {
                          {"date", " "},{"orderid", " "},{"execid", " "},{"timestamp", " "},{"qty", "0"},{"price", "0"},
                          {"avgpx", "0"},{"lvsqty", "0"},{"user", " "},{"descrip", " "},{"filename", " "},{"symbol", " "},
                          {"route", " "},{"side", " "},{"acc", " "},{"clientid", " "},{"traderint", " "},{"clordid", " "},
                          {"ordqty", " "},{"netprincipal", " "},{"settledate", " "},{"maturity", " "},{"otherfee", " "},
                          {"session", "MKTX"},{"acctype", " "},{"broker", " "},{"securityidsource", " "},{"securityid", " "},
                          {"couponrate", " "},{"grosstradeamt", " "},{"accruedinterestamt", " "},{"reqtype", " "},

                          {"liquidityind", " "},{"exectranstype", " "},{"execrefid", " "},{"msgid", " "},{"completionid", " "},
                          {"refmsgid", " "},{"refpenmsgid", " "},{"eg_u", " "},{"g_u", " "},{"b_n", " "},{"eb_n", " "},
                          {"clearingfirm", " "},{"exchangefee", "0"},{"solicited", " "},{"tempdouble", "0"},{"tempdouble2", "0"},
                          {"comm", "0"},{"regfee", "0"},{"isalloc", "0"},{"securitytype", " "}
                      };
        }

        public Dictionary<string, string> Map;

        public static WbAuditTrailExecutionQueryBuilder operator +(WbAuditTrailExecutionQueryBuilder a, WbAuditTrailExecutionQueryBuilder b)
        {
            WbAuditTrailExecutionQueryBuilder c = new WbAuditTrailExecutionQueryBuilder();

            return c;
        }

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
                "qty","price","lvsqty","ordqty","netprincipal","otherfee","grosstradeamt","accruedinterestamt","exchangefee","tempdouble","tempdouble2","comm", "regfee"
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
            return string.Format("insert into {2} ({0}) values{1};", GetFields(), GetValues(), XmlSettings.Instance.Db.FillTable);
        }
    }
}
