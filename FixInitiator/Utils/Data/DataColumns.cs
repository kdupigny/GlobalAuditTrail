using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAT.Utils.Data
{
    public static class DataColumns
    {
        public static List<string> DBAuditTrail = new List<string>
        {
            "ORDERID",              "CLORDID",                  "ORIGCLORDID",
            "PARENTORDERID",        "MESSAGETYPE",              "EXECUTIONID",
            
            "PRODUCTTYPE",          "SYMBOL",                   "SECURITYIDTYPE",
            "SECURITYID",           "SIDE",                     "CALLPUT",
            "TRADEDATE",            "SETTLEMENTDATE",           "MATURITYDATE",             
            "EXPIRATIONDATE",       "TRANSACTTIME",             "SPREAD", 
                 
            "ORDERQTY",             "FILLQTY",                  "CURRENCY",
            "ORDERTYPE",            "LIMITPX",                  "TRADEPRICE",
            "STRIKEPX",             "NETPX",                    "NETPRINCIPLE",
            "COMMISSION",           "EXCHANGEFEE",              "SECFEE",
            "CLEARINGFEE",          "FEEDESCRIPTION",           "OTHERFEE",                 
            "GROSSTRADEAMT",        "NFAFEE",                   "BROKERAGECHARGE",
            "COUPONRATE",
            
            "ACCOUNT",              "ACCOUNTTYPE",	            "AGGUNIT",
            "TRADERID",             "SESSIONID",                
             
            "TRADEDESCRIPTION",     "CONTRACTID",               "EXCHANGE",
            "BROKER",               "ROUTE",                    
            
            "COMMENT",              "TRAILSOURCE",              "FILENAME"

        };

        public static List<string> AuditTrail = new List<string>
        {
            "RECORDID",             "TRANSACTIONTYPE",             "FIRM",
            "OFFICE",               "ACCOUNT",                      "ACCOUNTTYPE",	
            "TRADEDATE",            "BUYSELLCODE",                  "QUANTITY", 
            "TRADEDESCRIPTION",     "MULTFACTOR",                   "TRADEPRICE",
            "PRINTABLETRADEPRICE",  "CONTRACTID",                   "OPENCLOSE",
            "EXCHANGE",             "PRODUCTCODE",                  "CONTRACTYEARMONTH",
            "PUTCALL",              "STRIKEPRICE",                  "OPTIONPREMIUM",
            "NETAMOUNT",            "GIVEUPCODE",                   "BROKERNAME",
            "COMMISSIONS",          "CLEARINGFEE",                  "EXCHANGEFEE",
            "NFAFEE",               "GIVEUPCHARGE",                 "BROKERAGECHARGE",
            "ELECEXECCHARGE",       "OTHERCHARGES",                 "CURRENCY",
            "CUSIP",                "CUSIP2",                       "PROMPTDAY",
            "EXPIRYDATE",           "LASTTRADEDATE",                "BBGCODE",
            "BBGYELLOWKEY",         "COBDATE",                      "ACCTBASECURR",
            "CONVERSIONRATE",       "FIRSTNOTICEDATE",              "TRADE_EXECUTION_TIME",
            "SPREAD",               "COMMENT1",                     "COMMENT_TEXT1",
            "COMMENT2",             "COMMENT_TEXT2",                 "COMMENT3",
            "COMMENT_TEXT3",        "REFNO"

        };

        public static List<string> OrderMatchFields = new List<string>
        {
            "TRADEDATE",            "CLORDID",                        "SYMBOL",
            "SIDE",                 "QUANTITY",                     "PRICE"
        };
    }
}
