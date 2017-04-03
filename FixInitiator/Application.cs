using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickFix;
using System.IO;
using System.Threading;

namespace FixInitiator
{
    public class Application : MessageCracker, QuickFix.Application
    {
        private bool bPrintApplicationMessages = true;

        private bool bLoggedOn = false;
        private bool bCreated = false;
        private bool bStopRunning = false;

        public void run()
        {
            while (true)
            {
                try
                {
                    if (bStopRunning)
                    {
                        onShutdown();
                        return;
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void stop() { bStopRunning = true; }

        public bool isLoggedOn() { return bLoggedOn; }
        public bool isCreated() { return bCreated; }
        
        //TextWriter tw;

        long orderCount;
        bool once = false;

        public Application(/*OrderManager oOM*/)
        {
            //oOrderManager = oOM;
            //tw = new StreamWriter("C:\\RMG\\OrdersAccessed_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt");
            //orderCount = 0;
        }

        // APPLICATION INTERFACE FUNCTIONS
        #region APPLICATION INTERFACE FUNCTIONS
        public void onCreate(SessionID sessionID)
        {
            #region Documentation
            // gets called when quickfix creates a new session. A session comes into and remains in existence for the life
            // of the application. Sessions exist whether or not a counter party is connected to it. As soon as a session
            // is created, you can begin sending messages to it. If no one is logged on, the messages will be sent at the
            // time a connection is established with the counterparty.
            #endregion

            bCreated = true;
            Console.WriteLine("onCreate");
        }

        public void onEvent(string str)
        {
            //            MessageBox.Show("EVENT");
        }

        public void onLogon(SessionID sessionID)
        {
            #region Documentation
            // notifies you when a valid logon has been established with a counter party. This is called when a connection
            // has been established and the FIX logon process has completed with both parties exchanging valid logon
            // messages.
            #endregion

            bLoggedOn = true;
            Console.WriteLine("onLogon");
        }

        public void onLogout(SessionID sessionID)
        {
            #region Documentation
            // notifies you when an FIX session is no longer online. This could happen during a normal logout exchange or
            // because of a forced termination or a loss of network connection.
            #endregion

            bLoggedOn = false;
            Console.WriteLine("onLogout");
        }

        public void toAdmin(QuickFix.Message message, SessionID sessionID)
        {
            #region Documentation
            // provides you with a peak at the administrative messages that are being sent from your FIX engine to the
            // counter party. This is normally not useful for an application however it is provided for any logging you may
            // wish to do. Notice that the FIX::Message is not const. This allows you to add fields before an adminstrative
            // message before it is sent out.
            #endregion

            bool print = true;
            if (message.getHeader().isSetField(35))
            {
                MsgType msgtype = new MsgType();
                message.getHeader().getField(msgtype);
                if (msgtype.getValue() == "0")
                    print = false;    
                else if(msgtype.getValue() == MsgType.LOGON)
                {
                    message.getHeader().setField(Username.FIELD, "wbaychidrop");
                    message.getHeader().setField(Password.FIELD, "W3aychi!");
                    //message.setField(RawData.FIELD, "W3aychi!");
                    //message.setField(RawDataLength.FIELD, "W3aychi!".Length.ToString());
                }

            //    SenderCompID sender = new SenderCompID();
            //    message.getHeader().getField(sender);

            //    if (sender.getValue().Equals("DHIRACH"))
            //    {
            //        message.getHeader().setField(SenderSubID.FIELD, "DHIRACH");
            //        message.getHeader().setField(TargetSubID.FIELD, "ITGD");
            //    }
            //    else
            //    {
            //        message.getHeader().setField(TargetSubID.FIELD, "DHIRACH");
            //        message.getHeader().setField(SenderSubID.FIELD, "ITGD");
            //    }
            }

            if (print)   
                Console.WriteLine("Logon Message: " + message.ToString());
        }

        public void toApp(QuickFix.Message message, SessionID sessionID)
        {

            #region Documentation
            // is a callback for application messages that you are being sent to a counterparty. If you throw a DoNotSend
            // exception in this function, the application will not send the message. This is mostly useful if the
            // application has been asked to resend a message such as an order that is no longer relevant for the current
            // market. Messages that are being resent are marked with the PossDupFlag in the header set to true; If a
            // DoNotSend exception is thrown and the flag is set to true, a sequence reset will be sent in place of the
            // message. If it is set to false, the message will simply not be sent. Notice that the FIX::Message is not
            // const. This allows you to add fields before an application message before it is sent out.
            #endregion

            if (bPrintApplicationMessages)
                Console.WriteLine("toApp: " + message.ToString());
        }

        public void fromAdmin(QuickFix.Message message, SessionID sessionID)
        {
            #region Documentation
            // notifies you when an administrative message is sent from a counterparty to your FIX engine. This can be
            // useful for doing extra validation on logon messages such as for checking passwords. Throwing a RejectLogon
            // exception will disconnect the counterparty.
            #endregion
            bool print = true;
            if (message.getHeader().isSetField(35))
            {
                MsgType msgtype = new MsgType();
                message.getHeader().getField(msgtype);
                if (msgtype.getValue() == "0")
                    print = false;
            }

            if (print)   
                Console.WriteLine ("fromAdmin: " + message.ToString());
        }

        public void fromApp(QuickFix.Message message, SessionID sessionID)
        {
            // Documentation
            // is one of the core entry points for your FIX application. Every application level request will come through
            // here. If, for example, your application is a sell-side OMS, this is where you will get your new order
            // requests. If you were a buy side, you would get your execution reports here. If a FieldNotFound exception is
            // thrown, the counterparty will receive a reject indicating a conditionally required field is missing. The
            // Message class will throw this exception when trying to retrieve a missing field, so you will rarely need the
            // throw this explicitly. You can also throw an UnsupportedMessageType exception. This will result in the
            // counterparty getting a reject informing them your application cannot process those types of messages. An
            // IncorrectTagValue can also be thrown if a field contains a value that is out of range or you do not support.

            if (bPrintApplicationMessages)
                Console.WriteLine ("fromApp: " + message.ToString());

            crack(message, sessionID);  // using the most type-safe recommendation from FIX
            // this passes to the crack function, which in-turn gives us responses via the OnMessage functions
        }
        #endregion

        public void onShutdown()
        {
            Console.WriteLine("FixSession Disconnected!!");
        }

        // MESSAGECRACKER BASE CLASS FUNCTIONS
        #region MESSAGECRACKER INHERITED FUNCTIONS

        //need new order single
        //Order cancel request
        //order cancel replace reject
        //session level reject 35=3

        // Upon receipt of an executed trade message
        public override void onMessage(QuickFix42.ExecutionReport message, SessionID sessionID)
        {
            
            //oOrderManager.ProcessExecution(message, sessionID); // passing the work to the order manager
            //            MessageBox.Show("ExecutionReport: " + message.ToString());
            string theMessage = message.ToString();
            Console.WriteLine("Execution Received: "+ theMessage);

            //ConsoleKeyInfo keyInfo;
            //if(!once)
            //     keyInfo = Console.ReadKey();

            //once = true;

            //orderCount++;
            //tw.WriteLine(orderCount + " *__* " + message.ToString());
            
            OrderID orderID = new OrderID();
            message.getField(orderID);
            Program.ExecutionOrderID = orderID.getValue(); 
        }

        // Upon receipt of a rejected cancel message
        public override void onMessage(QuickFix42.OrderCancelReject message, SessionID sessionID)
        {
            //oOrderManager.ProcessCancelReject(message, sessionID); // passing the work to the order manager
            //            MessageBox.Show("CancelReject: " + message.ToString());
            Console.WriteLine("Reject Message Received: " + message.ToString());
        }

        // Upon receipt of an Email Positions response
        public override void onMessage(QuickFix42.Email message, SessionID sessionID)
        {
            //            MessageBox.Show("Email Positions: " + message.ToString());
            //oOrderManager.ProcessEmail(message, sessionID); // passing the work to the order manager
            Console.WriteLine("Email Message Received: " + message.ToString());
        }

        public override void onMessage(QuickFix42.OrderCancelReplaceRequest message, SessionID sessionID)
        {

            Console.WriteLine("Replace Message Received: " + message.ToString());
            //services.ProcessCancelRequest(message, sessionID);
        }

        #endregion

        // reference to Principle
        //OrderManager oOrderManager;

        /*
        // set reference to Principle
        public void Set_Reference_to_OrderManager(OrderManager oOM)
        {
            oOrderManager = oOM;
        }
         */
    }
}
