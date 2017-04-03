using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GATUtils
{
    public class MyTime
    {
        public enum TimeFormat { HHMMSS, HHMMSS_COLON, HHMMSSMMM, HHMMSSMMM_COLON, HHMMSSMMM_DOT };
        public static int Utc2EasternOffset { get { return -5; } }

        public static DateTime Today { get { return DateTime.Now; } }
        public static string TodaysDateStrYYYYMMDD { get { return Today.ToString("yyyyMMdd"); } }
        public static string TodaysMySqlDateString { get { return Today.ToString("yyyy-MM-dd"); } }
        
        public static TimeSpan CurrentTime
        {
            get 
            { 
                DateTime currentDate = Today; 
                return new TimeSpan(currentDate.Hour, currentDate.Minute, currentDate.Second, currentDate.Millisecond); 
            }
        }

        public static TimeSpan CurrentUtcTime
        {
            get 
            { 
                DateTime currentDate = DateTime.UtcNow; 
                return new TimeSpan(currentDate.Hour, currentDate.Minute, currentDate.Second, currentDate.Millisecond); 
            }
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
                string [] temp = utcTimeString.Split(timeDateDelimeters, StringSplitOptions.RemoveEmptyEntries);
                utcTimeString = temp[temp.Count() - 1];
            }

            TimeSpan utcTimeSpan;
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
                    timePieces = utcTimeString.Split(':');
                    hour = int.Parse(timePieces[0]);
                    minute = int.Parse(timePieces[1]);
                    second = int.Parse(timePieces[2]);
                    milliSecs = int.Parse(timePieces[3]);
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
                    throw new ArgumentException(string.Format("Invalid time format: [{0}]  {1}", timeFormat.ToString(), utcTime));
            }

            utcTimeSpan = new TimeSpan(0, hour, minute, second, milliSecs);
            return GetEasternTime(utcTimeSpan);
        }

        private static char[] timeDateDelimeters = { ' ', '-' };
    }
}
