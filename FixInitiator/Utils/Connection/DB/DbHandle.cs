using System.Data;
using GATUtils.Connection.DB.Commands;
using GATUtils.Fix;
using GATUtils.Utilities;
using GATUtils.XML;

namespace GATUtils.Connection.DB
{
    public class DbHandle
    {
        public static DbHandle Instance { get { return s_instance ?? (s_instance = new DbHandle()); } }

        public ConnectionState ConnectionState { get { return _dbConnection.MyCurrentState;  } }

        public long TransactionCount { get { return _dbConnection.TransactionCount; } }
        public DbHandle()
        {
            _Init();
        }

        public bool Validate()
        {
            return s_instance != null;
        }

        public int MessageQueueSize { get { return _messageQueue.Count; } }

        public bool InsertMessageCommand(string source, DataRow message)
        {
            IDBCommand command = new InsertGeneralExecutionMessageCommand(source, _GetUniqueQueryId(), message);
            _messageQueue.Insert(command);
            return true;
        }

        public bool InsertMessageCommand(string source, GatFixMessage fixMsg)
        {
            IDBCommand command = new InsertGeneralExecutionMessageCommand(source, _GetUniqueQueryId(), fixMsg);
            _messageQueue.Insert(command);
            return true;
        }

        public bool InsertMessageCommand(string source, string qry)
        {
            IDBCommand command = new InsertGeneralExecutionMessageCommand(source, _GetUniqueQueryId(), qry);
            _messageQueue.Insert(command);
            return true;
        }

        public DataTable GetFills(string source, string date)
        {
            IDBCommand command = new GetFillsByDateCommand(source, _GetUniqueQueryId(), date);
            _messageQueue.Insert(command);
            return command.GetResult() as DataTable;// new DataTable();
        }

        private void _Init()
        {
            _commandCount = 0;
            _dbConnection = new MySqlDbConnection(XmlSettings.Instance.Db.Host, XmlSettings.Instance.Db.Port, XmlSettings.Instance.Db.Db, XmlSettings.Instance.Db.User, XmlSettings.Instance.Db.Password);
            _messageQueue = new DbMessageQueue(_dbConnection);
        }

        private string _GetUniqueQueryId()
        {
            _commandCount++;
            const string prefix = "QRY";
            return string.Format("{0}{1}{2}", prefix, MyTime.TodaysDateStrYYYYMMDD, _commandCount);
        }

        private long _commandCount;
        private DbConnection _dbConnection;
        private DbMessageQueue _messageQueue;
        private static DbHandle s_instance;
    }
}
