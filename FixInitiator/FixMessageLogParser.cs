using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickFix;
using System.IO;
using System.Threading;
//using PrincipleManager_Backend;

namespace FixInitiator
{
    class FixMessageLogParser
    {
        public static Dictionary<string, List<QuickFix42.ExecutionReport>> TodaysMessages;
        private char FixElementDelimeter = '\u0001'; //special non printing character
        //public logManager logger;

        private static string strTodaysDate { get { return DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00"); } }
        //*/ "20110818"; } }

        public FixMessageLogParser()
        {
            init();
        }

        private void init()
        {
            TodaysMessages = new Dictionary<string, List<QuickFix42.ExecutionReport>>();
            //logger = new logManager();
        }

        public void ParseFixLog(string FixLogPath)
        {
            if (!File.Exists(FixLogPath))
            {
                Console.WriteLine("File does not Exist");
                return;
            }

            TextReader tr = new StreamReader(FixLogPath);

            string currentMessage = tr.ReadLine();
            int count = 0;

            while (currentMessage != null)
            {
                if (/*msgIsToday(currentMessage) &&*/ msgIsOfInterest(currentMessage))
                {
                    QuickFix42.ExecutionReport FixMsg = getMessageTypeObject(currentMessage);

                    if (FixMsg == null)
                        continue;

                    FixMsg.setField(57, "7857955");
                    FixMsg.setField(76, "RANE");

                    //break message into pieces
                    string[] msgFields = currentMessage.Split(FixElementDelimeter);
                    //loadPieces
                    foreach (string field in msgFields)
                    {
                        string[] elements = field.Split('=');
                        if (elements.Count() < 2 || elements[0].Equals("10") || elements[0].Equals("8") || elements[0].Equals("9")
                             || elements[0].Equals("35") || elements[0].Equals("34"))/* || elements[0].Equals("49") || elements[0].Equals("52")
                             || elements[0].Equals("56"))*/
                            continue;

                        if (elements[0].Equals("1"))
                            FixMsg.setField(1, "54321");
                        else
                            FixMsg.setField(int.Parse(elements[0]), elements[1]);
                    }
                    //todo:PARSER 03-02 either add message to dictionary or send to logger 

                    //logger.logMsg("PManger 1", FixMsg);
                    addToDictionary(FixMsg);
                }
                //if (count > 200)
                //{
                //    Thread.Sleep(500);
                //    count = 0;
                //}

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

        private bool msgIsToday(string msg)
        {
            if (msg.Contains(strTodaysDate))
                return true;

            return false;
        }

        private bool msgIsOfInterest(string msg)
        {
            string anExecutionReport = "35=8";
            //string aNewOrder = "35=D";
            //string anOrderCancelReject = "35=9";
            //string aCancelRequest = "35=F";
            //string anUpdateRequest = "35=G";

            if (msg.Contains(anExecutionReport))// || msg.Contains(aNewOrder) || msg.Contains(anOrderCancelReject) || msg.Contains(aCancelRequest) || msg.Contains(anUpdateRequest))
                return true;

            return false;
        }

        private QuickFix42.ExecutionReport getMessageTypeObject(string msg)
        {
            string anExecutionReport = "35=8";
            string aNewOrder = "35=D";
            string anOrderCancelReject = "35=9";
            string aCancelRequest = "35=F";
            string anUpdateRequest = "35=G";

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
            else
                return null;

        }

        private void addToDictionary(QuickFix42.ExecutionReport msg)
        {
            ClOrdID clID = new ClOrdID();
            msg.getField(clID);
            string ID = clID.getValue();

            if (TodaysMessages.ContainsKey(ID))
            {
                TodaysMessages[ID].Add(msg);
            }
            else
            {
                List<QuickFix42.ExecutionReport> list = new List<QuickFix42.ExecutionReport>();
                list.Add(msg);
                TodaysMessages.Add(ID, list);
            }
        }
    }
}
