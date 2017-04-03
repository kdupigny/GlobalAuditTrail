using System;
using System.Collections;
using GATUtils.Connection.DB;
using GATUtils.Logger;
using GATUtils.Utilities.FileUtils;
using GATUtils.XML;
using QuickFix;
using System.Threading;
using System.Collections.Generic;

using GATUtils.Connection.Fix;
using GAT.Utils;

namespace GAT
{
    class Program
    {

        private static SessionSettings _mSettings;
        private static GatFixApplication _mApplication;
        private static FileStoreFactory _mStoreFactory;
        //        private DualLogFactory m_DualLogFactory;   // our custom implementation of screenlog and textlog
        private static FileLogFactory _mLogFactoryFile;
        private static MessageFactory _mMessageFactory;
        private static ThreadedSocketInitiator _gatInitiatorHandle;

        static SessionID _oGatSessionId;

        public static string ExecutionOrderId = "";
        public static string OriginalClId = "";
        public static FixMessageLogParser MsgLogHandler;
        //static string strFixLogFileName = "FIX.4.2-CSASAVIS-MIXIT.messages.log";

        static void Main(string[] args)
        {
            initSingletons();
       
            Random rand = new Random(DateTime.Now.Second);
            //string limitID = "";

            string strKnightSettingsFile = GatFile.Path(Dir.Fix, "Connection_Settings.fses");
            bool connected = Startup_FixConnection(strKnightSettingsFile);

            while (connected && !ConsoleControl.killApplication) 
            {
                ConsoleControl.windowListener();
            }

            if (connected && ConsoleControl.killApplication)
            {
                //add proper disconnect logic
            }
            else
            {
                Console.WriteLine("Could Not Connect to Session(s). Please close application and verify settings restart the application");
                Console.ReadKey();
                Console.WriteLine();
            }
        }

        private static void initSingletons()
        {
            XmlSettings.Instance.Validate();
            GatLogger.Instance.Validate();
            DbHandle.Instance.Validate();
        }

        static int dropCopyPosition = 0;
        static int msgNumber = 0;
        public static QuickFix42.ExecutionReport DropCopy()
        {

            if (dropCopyPosition > FixMessageLogParser.TodaysMessages.Count)
                return null;

            int internalDropPosition = 0;
            int internalMessageposition = 0;
            foreach (KeyValuePair<string, List<QuickFix42.ExecutionReport>> kvp in FixMessageLogParser.TodaysMessages)
            {
                if (internalDropPosition == dropCopyPosition)
                {
                    foreach (QuickFix42.Message msg in kvp.Value)
                    {
                        if (internalMessageposition == msgNumber)
                        {
                            msgNumber++;
                            return (QuickFix42.ExecutionReport)msg;
                        }
                        internalMessageposition++;
                    }
                    msgNumber = 0;
                    dropCopyPosition++;
                }
                internalDropPosition++;
            }

            return null;
        }

        private static bool Startup_FixConnection(string strKnightSessionSettingsPath)
        {
            bool bReturnValue = false;

            // CONNECT TO KNIGHT
            try
            {
                _mSettings = new SessionSettings(strKnightSessionSettingsPath);   // Settings File
                _mApplication = new GATUtils.Connection.Fix.MlFixApplication(/*this*/);
                //m_application.Set_Reference_to_OrderManager(this);
                _mStoreFactory = new FileStoreFactory(_mSettings);          // Our Storage Settings
                _mLogFactoryFile = new FileLogFactory(_mSettings);
                _mMessageFactory = new DefaultMessageFactory();             // Our Messaging Settings
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                return bReturnValue;
            }

            try
            {
                _gatInitiatorHandle = new ThreadedSocketInitiator(
                    _mApplication,
                    _mStoreFactory,
                    _mSettings,
                    _mLogFactoryFile,
                    _mMessageFactory);
            }
            catch (Exception e)
            {
                Console.WriteLine("Initiator Failed to be Created: " + e.Message + "\n" + e.StackTrace);
                return bReturnValue;
            }

            if (_gatInitiatorHandle != null)
            {
                // Get Session ID.  This will be used to send Orders.            
                ArrayList oList = _gatInitiatorHandle.getSessions();
                if (oList.Count > 0)
                {
                    _oGatSessionId = (QuickFix.SessionID)oList[0];

                    /*/ Get login id.  This is used to make order IDs unique across principle manager application instances.
                    if (oKnightSessionID.getSenderCompID().Length > 2)
                        strUserID = oKnightSessionID.getSenderCompID().Substring(oKnightSessionID.getSenderCompID().Length - 2);
                    else if (oKnightSessionID.getSenderCompID().Length == 1)
                        strUserID = oKnightSessionID.getSenderCompID().PadLeft(2, '0');
                    // If the SenderCompID is empty, shutdown (should never happen)
                    else
                    {
                        bReturnValue = false;
                        Shutdown_ConnectionToFixSessions();
                    }

                    // Start Execution Feed Listener; this starts its own thread.//*/
                    try
                    {
                        _gatInitiatorHandle.start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Could not Start Initiator: " + e.Message + "\n" + e.StackTrace);
                        return bReturnValue;
                    }

                    for (int i = 0; i < 15; ++i)
                    {
                        Console.WriteLine("Checking");
                        if (_mApplication.IsLoggedOn())
                        {
                            bReturnValue = true;
                            break;
                        }
                        Thread.Sleep(1000); // Wait 1 second (1,000 miliseconds) on each loop
                    }
                    if (!_mApplication.IsLoggedOn())
                    {
                        bReturnValue = false;
                        //Shutdown_ConnectionToFixSessions();
                        Console.WriteLine("Could not LogOn");
                    }

                }
                else
                {
                    bReturnValue = false;
                }
                if (bReturnValue == true)
                {
                    Console.WriteLine("Logon Successful");
                    /*/ Clear out any Daily Order Manager info (we do it here in case the Position Manager is left on overnight.  The PMgr should always be reconnected each morning.
                    if (dtLastRun != null)
                    {
                        if (dtLastRun.Value.Month == DateTime.Now.Month && dtLastRun.Value.Day == DateTime.Now.Day)
                        {
                        }
                        else
                        {
                            ClearDailyPositionInfoOnNewDay();
                        }
                    } //*/
                    //while (true) { }
                }                
            }
            return bReturnValue;
        }
    }
    
}