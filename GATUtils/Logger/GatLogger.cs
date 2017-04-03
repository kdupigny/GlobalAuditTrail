using System;
using GATUtils.Types;
using System.Threading;
using System.IO;
using GATUtils.Utilities;
using GATUtils.Utilities.FileUtils;
using GATUtils.XML;

namespace GATUtils.Logger
{
    public enum LogMode { Log, LogAndScreen, Screen }
    public enum LoggerType { DayLogger, StartTimeLogger, SingleLog }

    public class GatLogger
    {
        private string _NextLogFilePath
        {
            get
            {
                switch (_loggingType)
                {
                    case LoggerType.StartTimeLogger:
                        _currentFilename = GatFile.Path(Dir.ApplicationLogs, string.Format("{0}Log_{1}.{2}.txt", XmlSettings.Instance.ApplicationName, MyTime.TodaysDateStrYYYYMMDD, MyTime.CurrentTime.ToString().Replace(":", "_")));
                        break;
                    case LoggerType.SingleLog:
                        _currentFilename = GatFile.Path(Dir.ApplicationLogs, string.Format("{0}Log.{1}.txt", XmlSettings.Instance.ApplicationName, _fileCountIndex.ToString("00")));
                        break;
                    case LoggerType.DayLogger:
                        _currentFilename = GatFile.Path(Dir.ApplicationLogs, string.Format("{0}Log_{1}.{2}.txt", XmlSettings.Instance.ApplicationName, MyTime.TodaysDateStrYYYYMMDD, _fileCountIndex.ToString("00")));
                        break;
                }
                _fileCountIndex++;

                return _currentFilename;
            }
        }

        public string CurrentLogFilePath { get; private set; }

        public static GatLogger Instance
        {
            get { return _instance ?? (_instance = new GatLogger()); }
        }

        public GatLogger()
        {
            _InitLogger();
        }

        public bool Validate()
        {
            return _instance != null;
        }

        public void AddMessage(string message, LogMode mode = LogMode.Log)
        {
            _loggerMsgs.Enqueue(new LogPackage(message, mode));
        }

        private void _InitLogger()
        {
            _currentRunDate = MyTime.Today;
            _maxLines = XmlSettings.Instance.GatLogger.MaxLines;
            _loggingType = XmlSettings.Instance.GatLogger.LoggingType;

            _loggerMsgs = new GeneralThreadSafeQueue<LogPackage>();

            _loggerThread = new Thread(_RunLogger) {Name = "GATLogger", IsBackground = true};
            _loggerThread.Start();
        }

        private void _RunLogger() //thread
        {
            while (true)
            {
                Thread.Sleep(0);
                if (_loggerMsgs.Count > 0)
                {
                    _VerifyWriteToCurrentFile();
                    _WriteMessage(_loggerMsgs.Dequeue());
                }
            }
// ReSharper disable FunctionNeverReturns
        }
// ReSharper restore FunctionNeverReturns

        private void _WriteMessage(LogPackage msg)
        {
            switch (msg.WriteMode)
            {
                case LogMode.Log:
                    _WriteToLog(msg.Message);
                    break;
                case LogMode.Screen:
                    s_WriteToScreen(msg.Message);
                    break;
                case LogMode.LogAndScreen:
                    _WriteToLog(msg.Message);
                    s_WriteToScreen(msg.Message);
                    break;
            }
        }

        private void _VerifyWriteToCurrentFile()
        {
            if (_logWriter == null || !MyTime.Today.Date.Equals(_currentRunDate.Date) || _logLineCount >= _maxLines)
            {
                _ResetLogFile();
            }
        }

        private void _ResetLogFile()
        {
            if (_logWriter != null)
            {
                _logWriter.Flush();
                _logWriter.Close();
                _logWriter.Dispose();
            }

            CurrentLogFilePath = _NextLogFilePath;
            
            while (File.Exists(_currentFilename) && s_CountFileLines(_currentFilename) >= _maxLines)
                CurrentLogFilePath = _NextLogFilePath;

            _logWriter = File.AppendText(CurrentLogFilePath);
        }

        private void _WriteToLog(string message)
        {
            _logWriter.WriteLine(string.Format("{0}-{2}:  {1}",MyTime.Today.ToString("yyyyMMdd"), message, DateTime.Now.ToString("hh:mm:ss.fff")));
            _logWriter.Flush();
            _logLineCount++;
        }

        private static void s_WriteToScreen(string message)
        {
            Console.WriteLine(string.Format("{0}-{2}:  {1}", MyTime.Today.ToString("yyyyMMdd"), message, DateTime.Now.ToString("hh:mm:ss.fff")));
        }

        private static int s_CountFileLines(string filePath)
        {
            int lineCount = 0;

            using(TextReader tr = new StreamReader(filePath))
                while (tr.ReadLine() != null) lineCount++;

            return lineCount;
        }

        private Thread _loggerThread;

        private DateTime _currentRunDate;

        private GeneralThreadSafeQueue<LogPackage> _loggerMsgs;
        private StreamWriter _logWriter;
        private int _logLineCount;
        private int _maxLines;

        private LoggerType _loggingType = LoggerType.DayLogger;
        private string _currentFilename;
        private int _fileCountIndex;

        private static GatLogger _instance;
    }
}
