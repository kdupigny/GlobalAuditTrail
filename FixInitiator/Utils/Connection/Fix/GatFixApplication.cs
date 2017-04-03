using System;
using System.Threading;
using GATUtils.Logger;
using QuickFix;

namespace GATUtils.Connection.Fix
{
    public class GatFixApplication : MessageCracker, QuickFix.Application
    {
        public void Run()
        {
            while (true)
            {
                try
                {
                    if (_bStopRunning)
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

        public void Stop() { _bStopRunning = true; }

        public bool IsLoggedOn() { return _bLoggedOn; }
        public bool IsCreated() { return _bCreated; }

        public void onShutdown()
        {
            Console.WriteLine("FixSession Disconnected!!");
        }


        // APPLICATION INTERFACE FUNCTIONS
        #region APPLICATION INTERFACE FUNCTIONS
        public void onCreate(SessionID sessionId)
        {
            #region Documentation
            // gets called when quickfix creates a new session. A session comes into and remains in existence for the life
            // of the application. Sessions exist whether or not a counter party is connected to it. As soon as a session
            // is created, you can begin sending messages to it. If no one is logged on, the messages will be sent at the
            // time a connection is established with the counterparty.
            #endregion

            _bCreated = true;
            string msg = string.Format("OnCreate: Session({0} <-> {1})", sessionId.getSenderCompID(), sessionId.getTargetCompID());
            GatLogger.Instance.AddMessage(msg, LogMode.LogAndScreen);
        }

        public void onLogon(SessionID sessionId)
        {
            #region Documentation
            // notifies you when a valid logon has been established with a counter party. This is called when a connection
            // has been established and the FIX logon process has completed with both parties exchanging valid logon
            // messages.
            #endregion

            _bLoggedOn = true;
            string msg = string.Format("OnLogon: Session({0} <-> {1})", sessionId.getSenderCompID(), sessionId.getTargetCompID());
            GatLogger.Instance.AddMessage(msg, LogMode.LogAndScreen);
        }

        public void onLogout(SessionID sessionId)
        {
            #region Documentation
            // notifies you when an FIX session is no longer online. This could happen during a normal logout exchange or
            // because of a forced termination or a loss of network connection.
            #endregion

            _bLoggedOn = false;
            string msg = string.Format("OnLogout: Session({0} <-> {1})", sessionId.getSenderCompID(), sessionId.getTargetCompID());
            GatLogger.Instance.AddMessage(msg, LogMode.LogAndScreen);
        }

        public void onEvent(string str)
        {
            //            MessageBox.Show("EVENT");
        }

        public virtual void toAdmin(Message message, SessionID sessionId)
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
                if (msgtype.getValue() == "0" || msgtype.getValue() == "A")
                    print = false;
                else if (msgtype.getValue() == MsgType.LOGON)
                {

                }
            }

            if (print)
                Console.WriteLine("ToAdmin: " + message);
        }

        public void toApp(Message message, SessionID sessionId)
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
                Console.WriteLine("ToApp: " + message);
        }

        public void fromAdmin(Message message, SessionID sessionId)
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
                if (msgtype.getValue() == "0" || msgtype.getValue() == "A")
                    print = false;
            }

            if (print)
                Console.WriteLine("fromAdmin: " + message);
        }

        public void fromApp(Message message, SessionID sessionId)
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

            //if (bPrintApplicationMessages)
            //    Console.WriteLine ("fromApp: " + message.ToString());

            crack(message, sessionId);  // using the most type-safe recommendation from FIX
            // this passes to the crack function, which in-turn gives us responses via the OnMessage functions
        }
        #endregion


        private bool bPrintApplicationMessages = true;

        protected bool _bLoggedOn;
        protected bool _bCreated;
        protected bool _bStopRunning;

    }
}
