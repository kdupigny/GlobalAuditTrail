using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ReportsFactory.Utils;
using GATUtils.Fix;

namespace ReportsFactory.AuditTrail
{
    class GATFixLogToCleanReport
    {
        public static string strTodaysDate { get { return DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00"); } }
        private char[] FixElementDelimeters = 
        {
            FixConstants.CharDelimSOH, //special non printing character
            FixConstants.CharDelimWS  // space
        }; 
        
        //filtration arguements and options
        //[0] path to file
        //[1]-[n] various filters        
        //filter format
        //  tagNum_filterVal
        //if multiple filters of the same tag are specified it is understood as OR

        string filepath = "";
        public string outputFilenamePath = "mlFixLog_";
        Dictionary<string, List<string>> filters = new Dictionary<string, List<string>>();
        string targetDate;

        public GATFixLogToCleanReport(string[] args, string targetDate)
        {
            this.targetDate = targetDate;
            filepath = args[0];
            for (int i = 1; i < args.Count(); i++)
            {
                string[] filPieces = args[i].Split('_');

                if (filters.ContainsKey(filPieces[0]))
                    filters[filPieces[0]].Add(filPieces[1]);
                else
                {
                    List<string> filterValues = new List<string>();
                    filterValues.Add(filPieces[1]);
                    filters.Add(filPieces[0], filterValues);
                }
            }
            init();
        }

        public GATFixLogToCleanReport(string filePath, string [] contentFilters, string targetDate)
        {
            this.targetDate = targetDate;
            this.filepath = filePath;

            for (int i = 0; i < contentFilters.Count(); i++)
            {
                string[] filPieces = contentFilters[i].Split('_');

                if (filters.ContainsKey(filPieces[0]))
                    filters[filPieces[0]].Add(filPieces[1]);
                else
                {
                    List<string> filterValues = new List<string>();
                    filterValues.Add(filPieces[1]);
                    filters.Add(filPieces[0], filterValues);
                }
            }
            init();
        }

        private void init()
        {
            ParseFixLog(filepath);
        }

        public void ParseFixLog(string FixLogPath)
        {
            if (!File.Exists(FixLogPath))
            {
                Console.WriteLine("File does not Exist");
                return;
            }

            if (!File.Exists(FixLogPath))
            {
                Console.WriteLine("Check file path. File " + FixLogPath + " does not exist");
                return;
            }

            FileInfo logFile = new FileInfo(FixLogPath);
            outputFilenamePath = logFile.DirectoryName + "\\" + outputFilenamePath + targetDate + ".csv";
                //+ DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + ".txt";

            string logCopyPath = string.Empty;
            TextReader tr;// = //File.Open(FixLogPath, FileMode.Open, FileAccess.ReadWrite);
            
            try
            {
                tr = new StreamReader(logFile.FullName);
            }
            catch
            {
                logCopyPath = string.Format("{0}\\FixCopy", logFile.DirectoryName);
                File.Copy(logFile.FullName, logCopyPath);
                tr = new StreamReader(logCopyPath);
            }

            TextWriter tw = new StreamWriter(outputFilenamePath);
            tw.WriteLine(writeHeader());
            
            string currentMessage = tr.ReadLine();
            int count = 0;

            while (currentMessage != null)
            {
                
                string[] msgFields = currentMessage.Replace("\t","").Split(FixElementDelimeters, StringSplitOptions.RemoveEmptyEntries);
                    //loadPieces

                Dictionary<string, string> msgDictionary = new Dictionary<string, string>();
                foreach (string field in msgFields)
                {
                    string[] elements = field.Split('=');
                    if (elements.Count() < 2 || elements[0].Replace("-","") == "8" || elements[0] == "9" || elements[0] == "10")
                        continue;
                    try
                    {
                        msgDictionary.Add(elements[0], elements[1]);
                    }
                    catch { }
                }


                if (msgIsOfInterest(msgDictionary))
                {
                    tw.WriteLine(formatCSVLine(msgDictionary));
                        //currentMessage);
                }

                currentMessage = tr.ReadLine();
                count++;                
            }

            tr.Close();
            tr.Dispose();
            tw.Close();
            tw.Dispose();

            if (!string.IsNullOrEmpty(logCopyPath))
                File.Delete(logCopyPath);

            Console.WriteLine("Loading log complete...");
           
        }

        private bool msgIsOfInterest(Dictionary<string, string> msg)
        {    
            
            //int filterCount = 0;

            //foreach (KeyValuePair<string, List<string>> kvp in filters)
            //{
            //    string field = kvp.Key;
            //    foreach (string value in kvp.Value)
            //    {
            //        try
            //        {
            //            if (msg[field].Contains(value))
            //                filterCount++;
            //        }
            //        catch { }
            //    }
            //}

            //if (filterCount == filters.Count)
            string msgType = msg["35"];
            string sendingTime = msg["52"];

            if (!(msgType.Equals("0") || msgType.Equals("A") || msgType.Equals("1") || msgType.Equals("5")
                     || msgType.Equals("2") || msgType.Equals("4")) && sendingTime.Contains(targetDate))
                return true;

            return false;
        }

        private string formatCSVLine(Dictionary<string, string> msg)
        {
            StringBuilder sb = new StringBuilder();

            foreach(string col in ReportComponents.headerMap.Keys)
            {   
                string value = string.Empty;
                msg.TryGetValue(col, out value);

                string temp = FixFieldValueConverter.Instance[col, value];

                sb.AppendFormat("{0},", (string.IsNullOrEmpty(temp) ? value : temp));
            }

            return sb.ToString();
        }

        private string writeHeader()
        {
            StringBuilder sb = new StringBuilder();

            foreach(KeyValuePair<string, string> col in ReportComponents.headerMap)
            {
                sb.AppendFormat("{0},", col.Value);
            }

            return sb.ToString().TrimEnd(new [] { ',' });
        }

    }
}
