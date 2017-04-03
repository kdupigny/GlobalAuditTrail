using System;
using System.Data;
using GATUtils.Logger;
using GATUtils.Utilities;

namespace GATUtils.Connection.DB.Commands
{
    public class GenericDbCommand : IDBCommand
    {
        public CommandPriority Priority { get; private set; }
        public string SourceName { get; private set; }
        public string QueryId { get; set; }
        public bool CancelCommand { get; set; }
        public bool Failed { get; protected set; }

        public virtual string CommandName { get { return "GenericDBCommand"; } } 

        public GenericDbCommand(string sourceName, CommandPriority priority, string queryId, string datbase, string table)
        {
            SourceName = sourceName;
            Priority = priority;
            QueryId = queryId;
            _MyDb = datbase;
            _MyTable = table;
        }

        public virtual void Execute(DbConnection dbConnection)
        { throw new NotImplementedException(); }

        public virtual void ErrorEventHandler()
        {
           GatLogger.Instance.AddMessage(string.Format("Generic Command is Failing on Script: {0}", _CommandText));
            throw new Exception(CommandName + ": Command failed to execute");
        }
        
        public virtual string ToResultString()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the result of the command
        /// </summary>
        /// <returns>Object Type</returns>
        public virtual object GetResult() { return null; }

        /// <summary>
        /// Gets the queue insertion time.
        /// </summary>
        /// <returns></returns>
        public DateTime GetQueueInsertionTime()
        {
            return _QueueInsertionTime;
        }

        /// <summary>
        /// Sets the queue insertion time to now.
        /// </summary>
        public void SetQueueInsertionTimeToNow()
        {
            _QueueInsertionTime = DateTime.Now;
        }

        /// <summary>
        /// Cancels the command if it belongs to the query.
        /// </summary>
        /// <param name="queryId">The query ID.</param>
        public virtual void CancelCommandIfNeeded(string queryId)
        { }

        /// <summary>
        /// Waits if needed.
        /// </summary>
        public virtual void WaitIfNeeded(int milliSecondsToWait)
        { }

        /// <summary>
        /// Gets or sets the number of retries for the command.
        /// </summary>
        /// <value>The retries.</value>
        protected virtual int _Retries { get { return 3; } }

        /// <summary>
        /// Gets the sleep time (before retrying in case of failure).
        /// </summary>
        /// <value>The sleep time.</value>
        protected virtual int _SleepTime { get { return 300; } }

        /// <summary>
        /// Called after the command has been executed.
        /// </summary>
        protected virtual void _AfterExecute()
        { }

        /// <summary>
        /// Logs the command cancelled.
        /// </summary>
        protected virtual void _LogCommandCancelled()
        {
            GatLogger.Instance.AddMessage(string.Format("Cancelled Command ({0}): {1}", QueryId, CommandName));
        }
        
        protected DateTime _QueueInsertionTime;
        protected string _MyTable;
        protected string _MyDb;

        protected virtual string _CommandText
        {
            get
            {
                string date = MyTime.TodaysMySqlDateString;
                return string.Format( string.Format("SELECT * FROM {1}.{2} WHERE DATE>='{0}' AND DATE<='{0}';", date, _MyDb, _MyTable));
            }
        }

        private string _startDate;
        private string _endDate;
        private DataRow _singleMessage;
    

    }
}
