using System;
using System.Linq;
using GATUtils.XML;

namespace GATUtils.Utilities
{
    public class MyTime
    {
        public enum TimeFormat { HHMMSS, HHMMSS_COLON, HHMMSSMMM, HHMMSSMMM_COLON, HHMMSSMMM_DOT };
        public static int Utc2EasternOffset { get { return -5; } }

        public static DateTime Today
        {
            get
            {
                if (XmlSettings.Instance.SpecificRunDate.Date.Equals(DateTime.Now.Date))
                    return DateTime.Now.AddDays(-1*XmlSettings.Instance.DaysBack);
                
                return XmlSettings.Instance.SpecificRunDate;
            }
        }
        public static string TodaysDateStrYYYYMMDD { get { return Today.ToString("yyyyMMdd"); } }
        public static string TodaysMySqlDateString { get { return Today.ToString("yyyy-MM-dd"); } }

        public static string CurrentTimeString { get { return DateTime.Now.ToString("hh:mm:ss"); } }
        public static string CurrentTimeStringWithMilliSeconds { get { return DateTime.Now.ToString("hh:mm:ss.fff"); } }

        public static TimeSpan CurrentTime
        {
            get 
            { 
                DateTime currentDate = DateTime.Now; 
                return new TimeSpan(0, currentDate.Hour, currentDate.Minute, currentDate.Second, currentDate.Millisecond); 
            }
        }

        public static TimeSpan CurrentUtcTime
        {
            get 
            { 
                DateTime currentDate = DateTime.UtcNow; 
                return new TimeSpan(0, currentDate.Hour, currentDate.Minute, currentDate.Second, currentDate.Millisecond); 
            }
        }

        public static void SetTodaysDate(DateTime today)
        {
            XmlSettings.Instance.SpecificRunDate = today;
        }

        public static TimeSpan GetEasternTime(TimeSpan utcTime)
        {
            DateTime utcDateTime = new DateTime(utcTime.Ticks, DateTimeKind.Utc);
            DateTime localDateTime = utcDateTime.ToLocalTime();

            return new TimeSpan(localDateTime.Hour, localDateTime.Minute, localDateTime.Second);
        }

        public static TimeSpan GetEasternTime(string utcTime, TimeFormat timeFormat, bool hasDate = false)
        {
            string utcTimeString = utcTime;
            if (hasDate)
            {
                string [] temp = utcTimeString.Split(STimeDateDelimeters, StringSplitOptions.RemoveEmptyEntries);
                utcTimeString = temp[temp.Count() - 1];
            }

            string[] timePieces;
            int hour; int minute; int second; int milliSecs;
            switch (timeFormat)
            {
                case TimeFormat.HHMMSS:
                    hour = int.Parse(utcTimeString.Substring(0, 2));
                    minute = int.Parse(utcTimeString.Substring(2, 2));
                    second = int.Parse(utcTimeString.Substring(4, 2));
                    milliSecs = 0;
                    break;
                case TimeFormat.HHMMSS_COLON:
                    timePieces = utcTimeString.Split(':');
                    hour = int.Parse(timePieces[0]);
                    minute = int.Parse(timePieces[1]);
                    second = int.Parse(timePieces[2]);
                    milliSecs = 0;
                    break;
                case TimeFormat.HHMMSSMMM:
                    hour = int.Parse(utcTimeString.Substring(0, 2));
                    minute = int.Parse(utcTimeString.Substring(2, 2));
                    second = int.Parse(utcTimeString.Substring(4, 2));
                    milliSecs = int.Parse(utcTimeString.Substring(6));
                    break;
                case TimeFormat.HHMMSSMMM_COLON:
                    timePieces = utcTimeString.Split(STimeDelimeters);
                    hour = int.Parse(timePieces[0]);
                    minute = int.Parse(timePieces[1]);
                    second = int.Parse(timePieces[2]);
                    milliSecs = timePieces.Count() > 3 ? int.Parse(timePieces[3]) : 0;
                    break;
                case TimeFormat.HHMMSSMMM_DOT:
                    timePieces = utcTimeString.Split(':');
                    hour = int.Parse(timePieces[0]);
                    minute = int.Parse(timePieces[1]);

                    timePieces = timePieces[2].Split('.');                    
                    second = int.Parse(timePieces[0]);
                    milliSecs = int.Parse(timePieces[1]);
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid time format: [{0}]  {1}", timeFormat, utcTime));
            }

            TimeSpan utcTimeSpan = new TimeSpan(0, hour, minute, second, milliSecs);
            return GetEasternTime(utcTimeSpan);
        }

        public static string Get24HourTimeFromAmPmTime(string twelveHrTime)
        {
            if (!(twelveHrTime.Contains("AM") || twelveHrTime.Contains("PM")))
                throw new Exception("Timestamp not in correct AM/PM format: " + twelveHrTime);

            string[] timeAndDay = twelveHrTime.Split(' ');
            string outTime;

            string[] timePieces = timeAndDay[0].Split(':');
            if (timeAndDay[1].Equals("PM"))
            {
                int hour;
                if(!int.TryParse(timePieces[0], out hour))
                    throw new Exception("Timestamp not in correct AM/PM format: " + twelveHrTime);

                outTime = string.Format("{0}:{1}:{2}", (hour==12 ? "12" : (hour+12).ToString("00")), timePieces[1], timePieces[2]);
            }
            else outTime = string.Format("{0}:{1}:{2}", timePieces[0].PadLeft(2,'0'), timePieces[1].PadLeft(2,'0'), timePieces[2].PadLeft(2,'0'));

            return outTime;
        }

        public static DateTime GetLastTradingDay()
        {
            int lastTradingDay = -1;
            DateTime today = DateTime.Now;
            switch (today.DayOfWeek)
            {
                case DayOfWeek.Monday: lastTradingDay = -3; break;
                case DayOfWeek.Sunday: lastTradingDay = -2; break;
            }

            return DateTime.Now.AddDays(lastTradingDay);

        }

        private static readonly char[] STimeDateDelimeters = { ' ', '-' };
        private static readonly char[] STimeDelimeters = { ':', '.' };
    }
}
