using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using GATUtils.Connection.DB;

namespace GAT.Utils.Data
{
    public class FillDataView
    {
        public FillDataView()
        {
            Update();
        }

        //public string RECORDINSERTSTAMP { get { return ""; } }  
        //public string DATE { get { return ""; } }
        //public string SENDERCOMP { get { return ""; } } 
        //public string TARGETCOMP { get { return ""; } }  
        //public string ORDERID { get { return ""; } } 
        //public string CLORDID { get { return ""; } } 
        //public string EXECID { get { return ""; } } 
        //public string MESSAGETYPE { get { return ""; } }
        //public string EXECTYPE { get { return ""; } }
        //public string SYMBOL { get { return ""; } } 
        //public string SIDE { get { return ""; } } 
        //public string ORDERQTY { get { return ""; } } 
        //public string QTY { get { return ""; } } 
        //public string PRICE { get { return ""; } } 
        //public string FILLPX { get { return ""; } }
        //public string AVGPX { get { return ""; } } 
        //public string TIMESTAMP { get { return ""; } }
        //public string TIF { get { return ""; } }
        //public string CLIENTID{ get { return ""; } }
        //public string TRADERINT{ get { return ""; } }
        //public string ACC{ get { return ""; } } 
        //public string CLEARACC{ get { return ""; } }
        //public string LIQUIDITYIND { get { return ""; } } 
        //public string PRODUCTTYPE { get { return ""; } } 
        //public string SECURITYID { get { return ""; } } 
        //public string SECURITYIDSOURCE { get { return ""; } } 
        //public string ACCRUEDINTAMT { get { return ""; } } 
        //public string ROUTE { get { return ""; } } 
        //public string EXCHANGE { get { return ""; } } 
        //public string BROKER { get { return ""; } }  
        //public string EXPIRATIONDATE { get { return ""; } }
        //public string MATURITYDATE { get { return ""; } }
        //public string SETTLEDATE { get { return ""; } }
        //public string PUTCALL { get { return ""; } } 
        //public string STRIKEPX { get { return ""; } } 
        //public string COUPONRATE { get { return ""; } }  
        //public string GROSSTRADEAMT { get { return ""; } }  
        //public string NETPRINCIPAL { get { return ""; } }  
        //public string COMMISSION { get { return ""; } } 
        //public string EXCHANGEFEE { get { return ""; } }    
        //public string REGFEE { get { return ""; } } 
        //public string OCCFEE { get { return ""; } } 
        //public string OTHERFEE { get { return ""; } } 
        //public string DESCRIPTION { get { return ""; } }
        //public string SOURCE { get { return ""; } }

        public DataTable FillData { get { return fillData; } }

        private void Update() 
        { 
            fillData = DbHandle.Instance.GetFills("GATUI", "2014-02-06"); 
        }

        DataTable fillData;
    }
}
