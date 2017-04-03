using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GATUtils.Connection.DB.Models
{
    public static class DBFieldModel
    {
        /// <summary>
        /// Enumeration of GAT DB execution table columns
        /// </summary>
        public enum GatExecution
        {
            DATE,
            SENDERCOMP, //'Sender session that sent the fill',
            TARGETCOMP, //    'Target session that recieved the fill',
            ORDERID,
            CLORDID,
            EXECID,
            MESSAGETYPE,  //'Execution Message type',
            EXECTYPE,     //'Execution transaction type',
            SYMBOL,
            SIDE,
            ORDERQTY,
            QTY,
            PRICE,
            FILLPX,
            AVGPX,
            TIMESTAMP, //  'Transaction time stamp',
            TIF,//     'Time In Force',
            CLIENTID,//     'Trader Identifier at exchange recieving order',
            TRADERINT,//   'Additional trader identification',
            ACC,//    'Target account',
            CLEARACC,//     'Clearing firm account',
            LIQUIDITYIND,//     'Indicator of liquidity {1 2 6 A R X}',
            PRODUCTTYPE,
            SECURITYID,
            SECURITYIDSOURCE,
            ACCRUEDINTAMT, //'Accrued Interest Amount',
            ROUTE, //     'Intermediate route to contra',
            EXCHANGE, //     'Exchange receiving the order',
            BROKER, // 'Contra Broker (Last Market)',
            EXPIRATIONDATE,
            MATURITYDATE,
            SETTLEDATE,
            PUTCALL,
            STRIKEPX,
            COUPONRATE,
            GROSSTRADEAMT,
            NETPRINCIPAL,
            COMMISSION,
            EXCHANGEFEE, //'Access Fee',
            REGFEE,
            OCCFEE,
            OTHERFEE,
            DESCRIPTION,
            SOURCE//    'Partner fix session or file where fills were obtained',
        };

        public enum WBPTExecution
        {
            DATE,
            ISALLOC,
            ORDERID,
            EXECID,
            REQTYPE,
            TIMESTAMP,
            QTY,
            PRICE,
            AVGPX,
            LVSQTY,
            LIQUIDITYIND,
            USER,
            DESCRIP,
            EXECTRANSTYPE,
            COUPONRATE,
            GROSSTRADEAMT,
            BROKER,
            CLEARINGFIRM,
            EXCHANGEFEE,
            SECURITYID,
            SECURITYIDSOURCE,
            ACCRUEDINTERESTAMT,
            FILENAME,
            ROUTE,
            SYMBOL,
            SIDE,
            ACC,
            CLIENTID,
            TRADERINT,
            EXPIRATIONDATE,
            STRIKEPX,
            PUTCALL,
            CLORDID,
            MARKET,
            ORDTYPE,
            ORDQTY,
            NETPRINCIPAL,
            ISOPTION,
            SECURITYTYPE,
            COMM,
            SETTLEDATE,
            CONTRA,
            MATURITY,
            OCCFEE,
            OTHERFEE,
            SESSION,
            ACCTYPE,
            TIF
        }

        /// <summary>
        /// Enumeration of GAT DB order table columns
        /// </summary>
        public enum GatOrder
        {
            DATE,
            CLIENTID,
            ORDERID,
            CLORDID,
            ORIGCLORDID,
            MESSAGETYPE,
            SYMBOL,
            SIDE,
            ORDQTY,
            TIF,
            ACC,
            TRADERINT,
            ORDTYPE,
            LIMITPX,
            STOPPX,
            ORDERVALUE,
            MAXFLOOR,
            TEXT,
            ACCTYPE,
            STRATEGY,
            STARTTIME,
            ENDTIME,
            IDSOURCE,
            SECURITYID,
            SECURITYEXCH,
            PEGTYPE,
            SENDINGTIME,
            TRIGGERTIME,
            RECEIVETIME,
            SENDERCOMP,
            TARGETCOMP,
            ONBEHALF,
            RULE80A,
            SOURCE
        };
    }
}
