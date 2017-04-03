using System;
using System.Collections.Generic;
using System.Collections;
using QuickFix;
using System.IO;

namespace GATUtils.Connection.Fix
{
    class Handler
    {
        public static Dictionary<string, QuickFix42.Message> Msgs = new Dictionary<string,QuickFix42.Message>();
        public static ArrayList LimitOrders = new ArrayList();
        public static LimitOrders LimitLib = new LimitOrders();
        public static Dictionary<string, string> ClOrdIdOrdId = new Dictionary<string, string>();
        public string PersistentOrderFile = "Orders.txt";


        public Handler()
        {
            if (File.Exists(PersistentOrderFile))
            {
                _ReadFileInfo();
            }
        }

        ~Handler()
        {
            _WriteOrderInfo();
        }

        private void _ReadFileInfo()
        {
            bool endofmsgs = false;
            TextReader tr = new StreamReader(PersistentOrderFile);

            string line = tr.ReadLine();

            while (line != null)
            {
                if (line.Equals(";"))
                {
                    endofmsgs = true;
                    line = tr.ReadLine();
                    continue;
                }

                if (!endofmsgs)
                {
                    new QuickFix42.Message();
                }
            }

        }

        private void _WriteOrderInfo()
        {
            TextWriter tw = new StreamWriter(PersistentOrderFile);
            foreach (KeyValuePair<string, QuickFix42.Message> kvp in Msgs)
            {
                tw.WriteLine(kvp.Value.ToString());
            }
            tw.WriteLine(';');
            foreach (KeyValuePair<string, string> kvp2 in ClOrdIdOrdId)
            {
                tw.WriteLine(kvp2.Key + "," + kvp2.Value);
            }

            tw.Close();
            tw.Dispose();
        }

        public static void Add(QuickFix42.Message msg)
        {
            ClOrdID clientId = new ClOrdID();
            OrdType orderType = new OrdType();

            msg.getField(clientId);
            

            Msgs.Add(clientId.getValue(), msg);


            if (msg.isSetField(orderType))
            {
                msg.getField(orderType);
                if (orderType.getValue() == OrdType.LIMIT)
                {
                    LimitOrders.Add(clientId.getValue());
                    LimitLib.Add(clientId.getValue());
                }
            }
        }

        public int PrintLimitList()
        {
            Symbol sym = new Symbol();
            Side sd = new Side();
            OrderQty oq = new OrderQty();

            Console.WriteLine("      Index\tOrderID");
            int count = 0;
            foreach (string strOrdId in LimitOrders)
            {
                QuickFix42.Message msg = Msgs[strOrdId];

                msg.getField(sym);
                msg.getField(sd);
                msg.getField(oq);

                Console.WriteLine("\t" + count + "\t" + strOrdId + "\t" + sym + " " + sd + " " + oq);
                count++;
            }

            return count;
        }

        public void RemoveLimitOrder(int index)
        {
            object removeObject = LimitOrders[index];
            LimitOrders.Remove(removeObject);
        }

        public string GetLimitOrderOriginalId(int index)
        {
            return (string)LimitOrders[index];
        }

        public string GetOrderId(string clOrdId)
        {
            try
            {
                return ClOrdIdOrdId[clOrdId];
            }
            catch
            {
                return "";
            }
        }

        public QuickFix42.OrderCancelRequest GenerateCancelRequest(string clOrdId, int index)
        {
            string originalId = (string)LimitOrders[index];

            QuickFix42.Message msg = Msgs[originalId];


            //check for a recent ID
            string temp = LimitLib.GetRecentId(originalId);
            if (temp != null)
            {
                //limitLib.add(originalID, clOrdId);  //associate new Id             
                originalId = temp;                  //send with most recent from previous request
            }

            Side oldSide = new Side();
            msg.getField(oldSide);

            TransactTime uTime = new TransactTime();
            msg.getField(uTime);
           
            QuickFix42.OrderCancelRequest ocr = new QuickFix42.OrderCancelRequest(
                                                        new OrigClOrdID(originalId),
                                                        new ClOrdID(clOrdId),
                                                        new Symbol(msg.getField(Symbol.FIELD)), //Necessary Field for canceling child orders
                                                        oldSide,                                //Necessary Field for canceling child orders
                                                        uTime);

            OrderID oid = new OrderID("x");
            ocr.setField(oid);

            //Neccessary Field for canceling child orders
            Price limitPrice = new Price();
            msg.getField(limitPrice);
            //OCR.setField(limitPrice);

            ocr.setField(57, msg.getField(57));
            ocr.setField(9102, msg.getField(9102));

            return ocr;

        }

        public QuickFix42.OrderCancelReplaceRequest GenerateReplaceRequest(string clOrdId, int qty, double limitPx, double stopPx, int index)
        {
            string originalId = (string)LimitOrders[index];

            QuickFix42.Message msg = Msgs[originalId];


            //check for a recent ID
            string temp = LimitLib.GetRecentId(originalId);
            if (temp != null)
            {
                LimitLib.Add(originalId, clOrdId);  //associate new Id             
                originalId = temp;                  //send with most recent from previous request
            }
            else
            {
                LimitLib.Add(originalId, clOrdId);
            }
            
            Side oldSide = new Side();
            msg.getField(oldSide);

            TransactTime uTime = new TransactTime();
            msg.getField(uTime);

            QuickFix42.OrderCancelReplaceRequest ocrr = new QuickFix42.OrderCancelReplaceRequest(
                                                        new OrigClOrdID(originalId),
                                                        new ClOrdID(clOrdId), 
                                                        new HandlInst('1'), 
                                                        new Symbol(msg.getField(Symbol.FIELD)), 
                                                        oldSide, 
                                                        uTime, 
                                                        new OrdType(OrdType.LIMIT));

            // Update prices and quantity
            ocrr.set(limitPx > 0 ? new Price(limitPx) : new Price(double.Parse(msg.getField(Price.FIELD))));

            if (stopPx > 0) // If the user specified a new stop price
                ocrr.set(new StopPx(stopPx));
            else
            { // Otherwise use the original stop price
                StopPx spx = new StopPx();
                if (msg.isSetField(spx))
                {
                    msg.getField(spx);
                    ocrr.set(spx);
                }
                else
                {
                    msg.setField(spx);
                }
            }

            ocrr.set(qty > 0 ? new OrderQty(qty) : new OrderQty(int.Parse(msg.getField(OrderQty.FIELD))));

            OrderID oid = new OrderID("x");

            ocrr.setField(oid);
            ocrr.setField(57, msg.getField(57));
            ocrr.setField(9102, msg.getField(9102));

            return ocrr;
        }
    }
}
