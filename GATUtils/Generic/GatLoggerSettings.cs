using System.Xml;
using GATUtils.Logger;

namespace GATUtils.Generic
{
    public class GatLoggerSettings
    {
        public int MaxLines { get; private set; }
        public LoggerType LoggingType { get; private set; }

        public GatLoggerSettings(LoggerType loggingType, int maxLines)
        {
            MaxLines = maxLines;
            LoggingType = loggingType;
        }

        public GatLoggerSettings(XmlNodeList loggerBlock)
        {
            MaxLines = 200000;
            LoggingType = LoggerType.DayLogger;

            _LoadLoggerSettings(loggerBlock);
        }

        private void _LoadLoggerSettings(XmlNodeList loggerBlock)
        {
            if (loggerBlock == null || loggerBlock.Count == 0) return;

            // ReSharper disable PossibleNullReferenceException
            foreach (XmlNode innerNode in loggerBlock.Item(0))
            // ReSharper restore PossibleNullReferenceException
            {
                switch (innerNode.Name)
                {
                    case "LineLimit":
                        int maxLines;
                        int.TryParse(innerNode.InnerText, out maxLines);
                        MaxLines = maxLines > 0 ? maxLines : MaxLines;
                        break;
                    case "LoggingType":
                        switch(innerNode.InnerText.Trim())
                        {
                            case "DayLog":
                                LoggingType = LoggerType.DayLogger;
                                break;
                            case "TimestampLog":
                                LoggingType = LoggerType.StartTimeLogger;
                                break;
                            case "SingleFile":
                                LoggingType = LoggerType.SingleLog;
                                break;
                        }
                        break;
                }
            }
        }
    }
}
