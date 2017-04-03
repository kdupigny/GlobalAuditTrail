using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using QuickFix;
using System.IO;

namespace FixInitiator
{
    class Handler
    {
        public static Dictionary<string, QuickFix42.Message> MSGS = new Dictionary<string,QuickFix42.Message>();
        public static ArrayList LimitOrders = new ArrayList();
        public static limitOrders limitLib = new limitOrders();
        public static Dictionary<string, string> clOrdID_OrdID = new Dictionary<string, string>();
        public string persistentOrderFile = "Orders.txt";


        public Handler()
        {
            if (File.Exists(persistentOrderFile))
            {
                //readFileInfo();
            }
        }

        ~Handler()
        {
            writeOrderInfo();
        }

        private void readFileInfo()
        {
            bool endofmsgs = false;
            TextReader tr = new StreamReader(persistentOrderFile);

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
                    QuickFix42.Message oldMsg = new QuickFix42.Message();
                }
                else
                {

                }

            }

        }

        private void writeOrderInfo()
        {
            TextWriter tw = new StreamWriter(persistentOrderFile);
            foreach (KeyValuePair<string, QuickFix42.Message> kvp in MSGS)
            {
                tw.WriteLine(kvp.Value.ToString());
            }
            tw.WriteLine(';');
            foreach (KeyValuePair<string, string> kvp2 in clOrdID_OrdID)
            {
                tw.WriteLine(kvp2.Key + "," + kvp2.Value);
            }

            tw.Close();
            tw.Dispose();
        }

        public static void add(QuickFix42.Message msg)
        {
            ClOrdID clientID = new ClOrdID();
            OrdType orderType = new OrdType();

            msg.getField(clientID);
            

            MSGS.Add(clientID.getValue(), msg);


            if (msg.isSetField(orderType))
            {
                msg.getField(orderType);
                if (orderType.getValue() == OrdType.LIMIT)
                {
                    LimitOrders.Add(clientID.getValue());
                    limitLib.add(clientID.getValue());
                }
            }
        }

        public int printLimitList()
        {
            Symbol sym = new Symbol();
            Side sd = new Side();
            OrderQty oq = new OrderQty();

            Console.WriteLine("      Index\tOrderID");
            int count = 0;
            foreach (string strOrdID in LimitOrders)
            {
                QuickFix42.Message msg = MSGS[strOrdID];

                msg.getField(sym);
                msg.getField(sd);
                msg.getField(oq);

                Console.WriteLine("\t" + count + "\t" + strOrdID + "\t" + sym.ToString() + " " + sd.ToString() + " " + oq.ToString());
                count++;
            }

            return count;
        }

        public void removeLimitOrder(int index)
        {
            object removeObject = LimitOrders[index];
            LimitOrders.Remove(removeObject);
        }

        public string getLimitOrderOriginalID(int index)
        {
            return (string)LimitOrders[index];
        }

        public string getOrderID(string clOrdID)
        {
            try
            {
                return clOrdID_OrdID[clOrdID];
            }
            catch
            {
                return "";
            }
        }

        public QuickFix42.OrderCancelRequest generateCancelRequest(string clOrdId, int index)
        {
            string originalID = (string)LimitOrders[index];

            QuickFix42.Message msg = MSGS[originalID];


            //check for a recent ID
            string temp = limitLib.getRecentID(originalID);
            if (temp != null)
            {
                //limitLib.add(originalID, clOrdId);  //associate new Id             
                originalID = temp;                  //send with most recent from previous request
            }

            Side oldSide = new Side();
            msg.getField(oldSide);

            TransactTime uTime = new TransactTime();
            msg.getField(uTime);
           
            QuickFix42.OrderCancelRequest OCR = new QuickFix42.OrderCancelRequest(
                                                        new OrigClOrdID(originalID),
                                                        new ClOrdID(clOrdId),
                                                        new Symbol(msg.getField(Symbol.FIELD)), //Neccessary Field for canceling child orders
                                                        oldSide,                                //Neccessary Field for canceling child orders
                                                        uTime);

            OrderID oid = new OrderID("x");
            OCR.setField(oid);

            //Neccessary Field for canceling child orders
            Price limitPrice = new Price();
            msg.getField(limitPrice);
            //OCR.setField(limitPrice);

            OCR.setField(57, msg.getField(57));
            OCR.setField(9102, msg.getField(9102));

            return OCR;

        }

        public QuickFix42.OrderCancelReplaceRequest generateReplaceRequest(string clOrdId, int qty, double limitPx, double stopPx, int index)
        {
            string originalID = (string)LimitOrders[index];

            QuickFix42.Message msg = MSGS[originalID];


            //check for a recent ID
            string temp = limitLib.getRecentID(originalID);
            if (temp != null)
            {
                limitLib.add(originalID, clOrdId);  //associate new Id             
                originalID = temp;                  //send with most recent from previous request
            }
            else
            {
                limitLib.add(originalID, clOrdId);
            }
            
            Side oldSide = new Side();
            msg.getField(oldSide);

            TransactTime uTime = new TransactTime();
            msg.getField(uTime);

            QuickFix42.OrderCancelReplaceRequest OCRR = new QuickFix42.OrderCancelReplaceRequest(
                                                        new OrigClOrdID(originalID),
                                                        new ClOrdID(clOrdId), 
                                                        new HandlInst('1'), 
                                                        new Symbol(msg.getField(Symbol.FIELD)), 
                                                        oldSide, 
                                                        uTime, 
                                                        new OrdType(OrdType.LIMIT));

            // Update prices and quantity
            if (limitPx > 0) // If the user specified a new limit price
                OCRR.set(new Price(limitPx));
            else // Otherwise use the original limit price
                OCRR.set(new Price(double.Parse(msg.getField(Price.FIELD))));

            if (stopPx > 0) // If the user specified a new stop price
                OCRR.set(new StopPx(stopPx));
            else
            { // Otherwise use the original stop price
                StopPx spx = new StopPx();
                if (msg.isSetField(spx))
                {
                    msg.getField(spx);
                    OCRR.set(spx);
                }
                else
                {
                    msg.setField(spx);
                }
            }

            if (qty > 0) // If the user specified a new qty
                OCRR.set(new OrderQty(qty));
            else // Otherwise use the original qty
                OCRR.set(new OrderQty(int.Parse(msg.getField(OrderQty.FIELD))));

            OrderID oid = new OrderID("x");

            OCRR.setField(oid);
            OCRR.setField(57, msg.getField(57));
            OCRR.setField(9102, msg.getField(9102));

            return OCRR;
        }
    }

    public class limitOrders
    {        
        public static Dictionary <string , string> mostRecentID;

        public limitOrders()
        {
            mostRecentID = new Dictionary<string,string>();
        }

        public void add(string newLimitOrderID, string recentLimitID = "")
        {
            if (newLimitOrderID == "")
            {
                return;
            }

            if (mostRecentID.ContainsKey(newLimitOrderID) && recentLimitID != "")
            {
                mostRecentID[newLimitOrderID] = recentLimitID;
            }
            else
            {
                mostRecentID.Add(newLimitOrderID, recentLimitID);
            }
        }

        public string getRecentID(string originalOrderID)
        {
            if (mostRecentID.ContainsKey(originalOrderID))
            {
                if (mostRecentID[originalOrderID] != "")
                    return mostRecentID[originalOrderID];
            }

            return null;
        }
                
    }

    public class orderMaker
    {
        public orderMaker()
        {

        }

        public QuickFix42.OrderCancelRequest generateManualCancelRequest(string clOrdId, string OriginalID, string symbol, string side, double price, string route = "NEFAN", string channel = "X")
        {
            //QuickFix42.Message msg = MSGS[(string)LimitOrders[index]];
            Side sd = new Side(Side.BUY);

            if (side.Equals("B") || side.Equals("b") || side.Equals("buy") || side.Equals("BUY"))
                sd = new Side(Side.BUY);
            else if (side.Equals("S") || side.Equals("s") || side.Equals("sell") || side.Equals("SELL"))
                sd = new Side(Side.SELL);
            else if (side.Equals("SS") || side.Equals("ss") || side.Equals("ssell") || side.Equals("SSELL") || side.Equals("SHORT") || side.Equals("short"))
                sd = new Side(Side.SELL_SHORT);

            TransactTime uTime = new TransactTime(DateTime.Parse("20110708 15:24:26"));
            //msg.getField(uTime);

            QuickFix42.OrderCancelRequest OCR = new QuickFix42.OrderCancelRequest(
                                                        new OrigClOrdID(OriginalID),
                                                        new ClOrdID(clOrdId),
                                                        new Symbol(symbol), //Neccessary Field for canceling child orders
                                                        sd,                                //Neccessary Field for canceling child orders
                                                        uTime);

            OrderID oid = new OrderID("x");
            OCR.setField(oid);

            //Neccessary Field for canceling child orders
            Price limitPrice = new Price(price);
            //msg.getField(limitPrice);
            OCR.setField(limitPrice);

            OCR.setField(57, "ALGO1");
            OCR.setField(9102, "T");

            return OCR;

        }


        public QuickFix42.NewOrderSingle sendMarketOrder(string clOrdID, string symbol, int qty, string side, string route = "NEFAN", string channel = "X")
        {
            Side sd = new Side(Side.BUY);
            LocateReqd lReqd = new LocateReqd(true);
            bool isLocateReqd = false;

            if (side.Equals("B") || side.Equals("b") || side.Equals("buy") || side.Equals("BUY"))
                sd = new Side(Side.BUY);
            else if (side.Equals("S") || side.Equals("s") || side.Equals("sell") || side.Equals("SELL"))
                sd = new Side(Side.SELL);
            else if (side.Equals("SS") || side.Equals("ss") || side.Equals("ssell") || side.Equals("SSELL") || side.Equals("SHORT") || side.Equals("short"))
            {
                sd = new Side(Side.SELL_SHORT);
                isLocateReqd = true;
            }

            QuickFix42.NewOrderSingle msg = new QuickFix42.NewOrderSingle(new ClOrdID(clOrdID), new HandlInst('1'), new Symbol(symbol), sd, new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));

            if (isLocateReqd)
                msg.set(lReqd);

            msg.set(new OrderQty(qty));
            msg.set(new Account("54321"));
            msg.set(new TimeInForce(TimeInForce.DAY));

            msg.setField(57, route);
            msg.setField(9102, channel);

            if (route.Equals("JPMVWAP") || route.Equals("RAVAT") || route.Equals("TWAPJP") || route.Equals("MISMTW"))
            {
                msg.setField(6203, DateTime.UtcNow.ToString());
                msg.setField(6205, DateTime.UtcNow.Add(new TimeSpan(0,5,0)).ToString());
            }

            Handler.add(msg);
            return msg;
        }

        public QuickFix42.NewOrderSingle sendMarketOrder(string clOrdID, string symbol, int qty, string side, string route, string clientid, string channel = "X")
        {
            Side sd = new Side(Side.BUY);
            LocateReqd lReqd = new LocateReqd(true);
            bool isLocateReqd = false;

            if (side.Equals("B") || side.Equals("b") || side.Equals("buy") || side.Equals("BUY"))
                sd = new Side(Side.BUY);
            else if (side.Equals("S") || side.Equals("s") || side.Equals("sell") || side.Equals("SELL"))
                sd = new Side(Side.SELL);
            else if (side.Equals("SS") || side.Equals("ss") || side.Equals("ssell") || side.Equals("SSELL") || side.Equals("SHORT") || side.Equals("short"))
            {
                sd = new Side(Side.SELL_SHORT);
                isLocateReqd = true;
            }

            QuickFix42.NewOrderSingle msg = new QuickFix42.NewOrderSingle(new ClOrdID(clOrdID), new HandlInst('1'), new Symbol(symbol), sd, new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));

            if (isLocateReqd)
                msg.set(lReqd);

            msg.set(new OrderQty(qty));
            msg.set(new Account("54321"));
            msg.set(new TimeInForce(TimeInForce.DAY));

            msg.setField(57, route);
           // msg.setField(50, route);
            msg.setField(9102, channel);

            if (route.Equals("JPMVWAP") || route.Equals("RAVAT") || route.Equals("TWAPJP") || route.Equals("MISMTW") || route.Equals("SGMISMTW"))
            {
                msg.setField(6203, DateTime.UtcNow.ToString());
                msg.setField(6205, DateTime.UtcNow.Add(new TimeSpan(0, 5, 0)).ToString());
            }

            Handler.add(msg);
            return msg;
        }

        public QuickFix42.NewOrderSingle sendLimitOrder(string clOrdID, string symbol, int qty, string side, double price, string route = "NEFAN", string channel = "X")
        {
            Side sd = new Side(Side.BUY);
            LocateReqd lReqd = new LocateReqd(true);
            bool isLocateReqd = false;

            if (side.Equals("B") || side.Equals("b") || side.Equals("buy") || side.Equals("BUY"))
                sd = new Side(Side.BUY);
            else if (side.Equals("S") || side.Equals("s") || side.Equals("sell") || side.Equals("SELL"))
                sd = new Side(Side.SELL);
            else if (side.Equals("SS") || side.Equals("ss") || side.Equals("ssell") || side.Equals("SSELL") || side.Equals("SHORT") || side.Equals("short"))
            {
                sd = new Side(Side.SELL_SHORT);
                isLocateReqd = true;
            }

            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(clOrdID), new HandlInst('1'), new Symbol(symbol), sd, new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT));
            //additional required fields
            if (isLocateReqd)
                msg2.set(lReqd);

            msg2.set(new OrderQty(qty));
            msg2.set(new Account("12345"));
            msg2.set(new TimeInForce(TimeInForce.DAY));
            
            msg2.set(new Price(price));

            //For RMG Routing
            msg2.setField(57, route);
            msg2.setField(9102, channel);
            
            Handler.add(msg2);
            return msg2;
        }

        public QuickFix42.NewOrderSingle sendKnightMarketOrder(string OrderID)
        {            
            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(OrderID), new HandlInst('1'), new Symbol("BAC"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));
            //Additional required Fields
            msg2.set(new OrderQty(100));
            msg2.set(new Account("1234"));
            msg2.set(new TimeInForce(TimeInForce.DAY));

            //RMG Routing
            msg2.setField(57, "ALGO1");
            msg2.setField(9102, "T");

            Handler.add(msg2);
            return msg2;
        }

        public QuickFix42.NewOrderSingle sendKnightLimitOrder(string orderID, ref string LimitID, double price)
        {
            //int id = rand.Next(100000000, 200000000);
            LimitID = orderID;

            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(orderID), new HandlInst('1'), new Symbol("BAC"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT));
            //additional required fields
            msg2.set(new OrderQty(100));
            msg2.set(new Account("1234"));
            msg2.set(new TimeInForce(TimeInForce.DAY));
            Price p = new Price(price);
            msg2.set(p);

            //For RMG Routing
            msg2.setField(57, "ALGO1");
            msg2.setField(9102, "T");
            //msg2.setField(6210, "0");

            //Session.sendToTarget(msg2, oKnightSessionID);

            return msg2;
        }

        public QuickFix42.OrderCancelRequest sendLimitOrderCancelRequest(string OriginalID, string orderID, string ExID)
        {
            //int id = rand.Next(100000000, 200000000);
            QuickFix42.OrderCancelRequest m = new QuickFix42.OrderCancelRequest(new OrigClOrdID(OriginalID), new ClOrdID(orderID), new Symbol("BAC"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow));

            //addtional required fields
            m.set(new OrderQty(100));
            m.set(new OrderID(ExID));

            //RMG Routing
            m.setField(57, "ALGO1");
            m.setField(9102, "T");
            //m.setField(6210, "0");

            //Price p = new Price(12.99);
            //m.set(p);
            //Session.sendToTarget(m, oKnightSessionID);
            return m;
        }

        public QuickFix42.OrderCancelReplaceRequest sendLimitOrderCancelReplace(string OriginalID, string orderID, string ExID)
        {
            //int id = rand.Next(100000000, 200000000);
            QuickFix42.OrderCancelReplaceRequest m = new QuickFix42.OrderCancelReplaceRequest(new OrigClOrdID(OriginalID), new ClOrdID(orderID), new HandlInst('1'), new Symbol("ZVZZT"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT));

            //addtional required fields
            m.set(new OrderQty(100));
            m.set(new OrderID(ExID));
            m.set(new Account("1234"));
            m.set(new Price(2.00));
            m.set(new TimeInForce(TimeInForce.DAY));
            m.set(new StopPx(0));

            //RMG Routing
            m.setField(57, "ALGO");
            m.setField(9102, "T");
            m.setField(6210, "0");

            //Price p = new Price(12.99);
            //m.set(p);
            //Session.sendToTarget(m, oKnightSessionID);
            return m;
        }

        public QuickFix42.NewOrderSingle sendKnightMarketOrder(string OrderID, string route)
        {
            //int id = rand.Next(100000000, 200000000);
            //limitID = "t" + id.ToString();


            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(OrderID), new HandlInst('1'), new Symbol("ZVZZT"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));
            //Additional required Fields
            msg2.set(new OrderQty(100));
            msg2.set(new Account("1234"));
            msg2.set(new TimeInForce(TimeInForce.DAY));

            //RMG Routing
            msg2.setField(57, route);
            msg2.setField(9102, "X");
            //msg2.setField(6210, "0");

            //Session.sendToTarget(msg2, oKnightSessionID);

            return msg2;
        }

        public QuickFix42.NewOrderSingle sendKnightMarketOrder(string OrderID, string route, string symbol)
        {
            //int id = rand.Next(100000000, 200000000);
            //limitID = "t" + id.ToString();


            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(OrderID), new HandlInst('1'), new Symbol(symbol), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));
            //Additional required Fields
            msg2.set(new OrderQty(100));
            msg2.set(new Account("1234"));
            msg2.set(new TimeInForce(TimeInForce.DAY));

            //RMG Routing

            msg2.setField(57, route);

            if (route.Contains("ALGO"))
                msg2.setField(9102, "T");
            else
                msg2.setField(9102, "X");
            //msg2.setField(6210, "0");

            //Session.sendToTarget(msg2, oKnightSessionID);

            return msg2;
        }
    }
}
