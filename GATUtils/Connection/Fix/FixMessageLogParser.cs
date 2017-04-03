using System;
using System.Collections.Generic;
using System.Linq;
using QuickFix;
using System.IO;

//using PrincipleManager_Backend;

namespace GATUtils.Connection.Fix
{
    public class FixMessageLogParser
    {
        public static Dictionary<string, List<QuickFix42.ExecutionReport>> TodaysMessages;
        private const char ct_fixElementDelimeter = '\u0001'; //special non printing character
        //public logManager logger;

        private static string _StrTodaysDate { get { return DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00"); } }
        //*/ "20110818"; } }

        public FixMessageLogParser()
        {
            _Init();
        }

        private static void _Init()
        {
            TodaysMessages = new Dictionary<string, List<QuickFix42.ExecutionReport>>();
            //logger = new logManager();
        }

        public void ParseFixLog(string fixLogPath)
        {
            if (!File.Exists(fixLogPath))
            {
                Console.WriteLine("File does not Exist");
                return;
            }

            TextReader tr = new StreamReader(fixLogPath);

            string currentMessage = tr.ReadLine();
            int count = 0;

            while (currentMessage != null)
            {
                if (/*msgIsToday(currentMessage) &&*/ _MsgIsOfInterest(currentMessage))
                {
                    QuickFix42.ExecutionReport fixMsg = _GetMessageTypeObject(currentMessage);

                    if (fixMsg == null)
                        continue;

                    fixMsg.setField(57, "7857955");
                    fixMsg.setField(76, "RANE");

                    //break message into pieces
                    string[] msgFields = currentMessage.Split(ct_fixElementDelimeter);
                    //loadPieces
                    foreach (string field in msgFields)
                    {
                        string[] elements = field.Split('=');
                        if (elements.Count() < 2 || elements[0].Equals("10") || elements[0].Equals("8") || elements[0].Equals("9")
                             || elements[0].Equals("35") || elements[0].Equals("34"))/* || elements[0].Equals("49") || elements[0].Equals("52")
                             || elements[0].Equals("56"))*/
                            continue;

                        if (elements[0].Equals("1"))
                            fixMsg.setField(1, "54321");
                        else
                            fixMsg.setField(int.Parse(elements[0]), elements[1]);
                    }
                    _AddToDictionary(fixMsg);
                }

                currentMessage = tr.ReadLine();
                count++;
            }

            Console.WriteLine("Loading log complete...");
            //passtoLogger();
        }

        //private void passtoLogger()
        //{
        //    TimeSpan startTime = DateTime.Now.TimeOfDay;
        //    Console.WriteLine("Parsing Started @ " + startTime);
        //    int count = 0;
        //    int msgCount = 0;

        //    foreach (KeyValuePair<string, List<QuickFix42.Message>> kvp in TodaysMessages)
        //    {
        //        foreach (QuickFix42.Message msg in kvp.Value)
        //        {
        //            logger.logMsg("PManger 1", msg);
        //        }

        //        msgCount += kvp.Value.Count;
        //        count++;
        //        Console.WriteLine("\tCompleted " + count + " of " + TodaysMessages.Count + " blocks. (" + kvp.Value.Count + ")");

        //        logger.proccessingComplete = false;
        //        while (!logger.proccessingComplete)
        //        {
        //            Thread.Sleep(500);
        //        }
        //    }

        //    Console.WriteLine("Parsing Complete @ " + DateTime.Now.TimeOfDay + " " + msgCount + " messages processed.");
        //    Console.WriteLine("Duration: " + (DateTime.Now.TimeOfDay - startTime).Duration());
        //}

        private bool _MsgIsToday(string msg)
        {
            if (msg.Contains(_StrTodaysDate))
                return true;

            return false;
        }

        private static bool _MsgIsOfInterest(string msg)
        {
            const string anExecutionReport = "35=8";
            //string aNewOrder = "35=D";
            //string anOrderCancelReject = "35=9";
            //string aCancelRequest = "35=F";
            //string anUpdateRequest = "35=G";

            if (msg.Contains(anExecutionReport))// || msg.Contains(aNewOrder) || msg.Contains(anOrderCancelReject) || msg.Contains(aCancelRequest) || msg.Contains(anUpdateRequest))
                return true;

            return false;
        }

        private static QuickFix42.ExecutionReport _GetMessageTypeObject(string msg)
        {
            const string anExecutionReport = "35=8";
            //string aNewOrder = "35=D";
            //string anOrderCancelReject = "35=9";
            //string aCancelRequest = "35=F";
            //string anUpdateRequest = "35=G";

            if (msg.Contains(anExecutionReport))
                return new QuickFix42.ExecutionReport();
            //else if (msg.Contains(aNewOrder))
            //    return new QuickFix42.NewOrderSingle();
            //else if (msg.Contains(anOrderCancelReject))
            //    return new QuickFix42.OrderCancelReject();
            //else if (msg.Contains(aCancelRequest))
            //    return new QuickFix42.OrderCancelRequest();
            //else if (msg.Contains(anUpdateRequest))
            //    return new QuickFix42.OrderCancelReplaceRequest();
            return null;
        }

        private static void _AddToDictionary(QuickFix42.ExecutionReport msg)
        {
            ClOrdID clId = new ClOrdID();
            msg.getField(clId);
            string id = clId.getValue();

            if (TodaysMessages.ContainsKey(id))
            {
                TodaysMessages[id].Add(msg);
            }
            else
            {
                List<QuickFix42.ExecutionReport> list = new List<QuickFix42.ExecutionReport> {msg};
                TodaysMessages.Add(id, list);
            }
        }
    }
}
