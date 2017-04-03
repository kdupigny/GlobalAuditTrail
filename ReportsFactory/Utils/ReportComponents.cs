using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportsFactory.Utils
{
    public static class ReportComponents
    {
        public static Dictionary<string, string> headerMap = new Dictionary<string, string> 
        {
            {"35","MessageType"},           {"34","SequenceNum"},
            {"49","Sender"},                {"56","Target"},
            {"43","IsPossDupe"},            {"77", "PositionEffect"},
            {"52","SendingTime"},
            {"122","OrigSendingTime"},      {"37","OrderId"},
            {"11","ClOrdId"},               {"39","OrdStatus"},
            {"17","ExecId"},                {"55","Symbol"},
            {"54","Side"},                  {"31","LastPrice"},
            {"44","Price"},                 {"6","AvgPrice"},
            {"40","OrdType"},               {"167","SecurityType"},
            {"60","TransactTime"},          {"32","FillQty"},
            {"151","LeavesQty"},            {"14","TotalSharesFill"},
            {"30","MktExecLastFill"},       {"38","OrderQty"},
            {"382","NoContraBrokers"},      {"375","ContraBroker"},
            {"1","Account"},                {"440","ClearingAccount"},
            {"201", "Put/Call"},            {"202", "StrikePrice"},
            {"200","MaturityMonthYear"},    {"205", "MaturityDay"},
            {"21","HandlInst"},             {"20","ExecTransType"},
            {"150","ExecType"},             {"59","TIF"},
            {"58","Text"},


            //mktx fields
            //{"9872","mxkt9872"}{"523","PartySubId"},{"803","PartySubIdType"},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
            //{"",""}{"",""},{"",""},
            //{"",""},{"",""},{"",""},
        };
    }
}
