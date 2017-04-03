using System;
using QuickFix;

namespace GATUtils.Connection.Fix
{
    public class OrderMaker
    {
        public OrderMaker()
        { }

        public QuickFix42.OrderCancelRequest GenerateManualCancelRequest(string clOrdId, string originalId, string symbol, string side, double price, string route = "NEFAN", string channel = "X")
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

            QuickFix42.OrderCancelRequest ocr = new QuickFix42.OrderCancelRequest(
                new OrigClOrdID(originalId),
                new ClOrdID(clOrdId),
                new Symbol(symbol), //Necessary Field for canceling child orders
                sd,                                //Necessary Field for canceling child orders
                uTime);

            OrderID oid = new OrderID("x");
            ocr.setField(oid);

            //Neccessary Field for canceling child orders
            Price limitPrice = new Price(price);
            //msg.getField(limitPrice);
            ocr.setField(limitPrice);

            ocr.setField(57, "ALGO1");
            ocr.setField(9102, "T");

            return ocr;

        }


        public QuickFix42.NewOrderSingle SendMarketOrder(string clOrdId, string symbol, int qty, string side, string route = "NEFAN", string channel = "X")
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

            QuickFix42.NewOrderSingle msg = new QuickFix42.NewOrderSingle(new ClOrdID(clOrdId), new HandlInst('1'), new Symbol(symbol), sd, new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));

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

            Handler.Add(msg);
            return msg;
        }

        public QuickFix42.NewOrderSingle SendMarketOrder(string clOrdId, string symbol, int qty, string side, string route, string clientid, string channel = "X")
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

            QuickFix42.NewOrderSingle msg = new QuickFix42.NewOrderSingle(new ClOrdID(clOrdId), new HandlInst('1'), new Symbol(symbol), sd, new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));

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

            Handler.Add(msg);
            return msg;
        }

        public QuickFix42.NewOrderSingle SendLimitOrder(string clOrdId, string symbol, int qty, string side, double price, string route = "NEFAN", string channel = "X")
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

            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(clOrdId), new HandlInst('1'), new Symbol(symbol), sd, new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT));
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
            
            Handler.Add(msg2);
            return msg2;
        }

        public QuickFix42.NewOrderSingle SendKnightMarketOrder(string orderId)
        {            
            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(orderId), new HandlInst('1'), new Symbol("BAC"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));
            //Additional required Fields
            msg2.set(new OrderQty(100));
            msg2.set(new Account("1234"));
            msg2.set(new TimeInForce(TimeInForce.DAY));

            //RMG Routing
            msg2.setField(57, "ALGO1");
            msg2.setField(9102, "T");

            Handler.Add(msg2);
            return msg2;
        }

        public QuickFix42.NewOrderSingle SendKnightLimitOrder(string orderId, ref string limitId, double price)
        {
            limitId = orderId;

            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(orderId), new HandlInst('1'), new Symbol("BAC"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT));
            //additional required fields
            msg2.set(new OrderQty(100));
            msg2.set(new Account("1234"));
            msg2.set(new TimeInForce(TimeInForce.DAY));
            Price p = new Price(price);
            msg2.set(p);

            //For RMG Routing
            msg2.setField(57, "ALGO1");
            msg2.setField(9102, "T");

            //Session.sendToTarget(msg2, oKnightSessionID);

            return msg2;
        }

        public QuickFix42.OrderCancelRequest SendLimitOrderCancelRequest(string originalId, string orderId, string exId)
        {
            //int id = rand.Next(100000000, 200000000);
            QuickFix42.OrderCancelRequest m = new QuickFix42.OrderCancelRequest(new OrigClOrdID(originalId), new ClOrdID(orderId), new Symbol("BAC"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow));

            //addtional required fields
            m.set(new OrderQty(100));
            m.set(new OrderID(exId));

            //RMG Routing
            m.setField(57, "ALGO1");
            m.setField(9102, "T");
            //m.setField(6210, "0");

            //Price p = new Price(12.99);
            //m.set(p);
            //Session.sendToTarget(m, oKnightSessionID);
            return m;
        }

        public QuickFix42.OrderCancelReplaceRequest SendLimitOrderCancelReplace(string originalId, string orderId, string exId)
        {
            QuickFix42.OrderCancelReplaceRequest m = new QuickFix42.OrderCancelReplaceRequest(new OrigClOrdID(originalId), new ClOrdID(orderId), new HandlInst('1'), new Symbol("ZVZZT"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.LIMIT));

            //additional required fields
            m.set(new OrderQty(100));
            m.set(new OrderID(exId));
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

        public QuickFix42.NewOrderSingle SendKnightMarketOrder(string orderId, string route)
        {
            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(orderId), new HandlInst('1'), new Symbol("ZVZZT"), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));
            //Additional required Fields
            msg2.set(new OrderQty(100));
            msg2.set(new Account("1234"));
            msg2.set(new TimeInForce(TimeInForce.DAY));

            //RMG Routing
            msg2.setField(57, route);
            msg2.setField(9102, "X");

            return msg2;
        }

        public QuickFix42.NewOrderSingle SendKnightMarketOrder(string orderId, string route, string symbol)
        {
            QuickFix42.NewOrderSingle msg2 = new QuickFix42.NewOrderSingle(new ClOrdID(orderId), new HandlInst('1'), new Symbol(symbol), new Side(Side.BUY), new TransactTime(DateTime.UtcNow), new OrdType(OrdType.MARKET));
            //Additional required Fields
            msg2.set(new OrderQty(100));
            msg2.set(new Account("1234"));
            msg2.set(new TimeInForce(TimeInForce.DAY));

            msg2.setField(57, route);
            msg2.setField(9102, route.Contains("ALGO") ? "T" : "X");
           
            return msg2;
        }
    }
}