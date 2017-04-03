using System;

namespace GATUtils.Generic
{
    public class GatException : Exception
    {
        public GatException(string msg) : base(msg) { }
        
        public GatException(string msg, Exception innerException) : base(msg, innerException) { }

        /// <summary>
        /// Gets the details of the exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>Exception details</returns>
        public static string GetDetails(Exception e)
        {
            if (e == null) return string.Empty;
            GatException gatException = e as GatException;
            if (gatException == null || string.IsNullOrEmpty(gatException.Details)) return e.ToString();
            return string.Format("{0}\r\n\r\nMore Details: {1}",
                        gatException.Details, e);            
        }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>The details.</value>
        public string Details { get; set; }
    }
}
