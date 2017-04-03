using System.Data;
using System.Data.SqlClient;

namespace GATUtils.Connection.DB
{
    public abstract class DbConnection : IConnection
    {
        protected DbConnection(string hostAddress, string port, string targetDbName, string username, string password, int retryAttempts)
        {
            _Host = hostAddress;
            _Port = port;
            _DBName = targetDbName;
            _Username = username;
            _Password = password;

            _ConnectionAttempts = retryAttempts;
            _MillisecondTimeout = 90000; //1.5 minutes
        }

        protected DbConnection(string hostAddress, string port, string targetDbName, string username, string password, int retryAttempts, int timeoutLength)
            : this (hostAddress, port, targetDbName, username, password, retryAttempts)
        {
            _MillisecondTimeout = timeoutLength;
        }

        #region Public Members

        public abstract bool IsAlive { get; }
        public abstract ConnectionState MyCurrentState { get; }

        public long TransactionCount { get; protected set; }
        #endregion Public Members

        #region Public Methods

        public abstract bool Connect();
        public abstract bool Disconnect();

        public abstract void SetDatabase(string dataBase);

        public abstract int RunNonQuery(string query);
        //public abstract int RunNonQuery(SqlCommand command);
        public abstract DataTable RunQuery(string query);
        //public abstract DataTable RunQuery(SqlCommand command);

        public abstract string GetExecutionResult();

        #endregion Public Methods

        #region Protected Members
        protected string _Host;
        protected string _Port;
        protected string _DBName;
        protected string _Username;
        protected string _Password;
        protected readonly int _ConnectionAttempts;
        protected readonly int _MillisecondTimeout;

        protected abstract string _GetConnectionString(); 
        #endregion Protected Members

        #region Private Members


        #endregion Private Members 
    }
}
