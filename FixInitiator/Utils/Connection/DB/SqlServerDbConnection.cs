using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using System.Data.SqlClient;
using GATUtils.Utilities;

namespace GATUtils.Connection.DB
{
    public class SqlServerDbConnection : DbConnection
    {
        public SqlServerDbConnection(string hostAddress, string port, string targetDbName, string username, string password)
            : base(hostAddress, port, targetDbName, username, password, 3, 360000) { }

        public SqlServerDbConnection(string hostAddress, string port, string targetDbName, string username, string password, int retryAttempts)
            : base(hostAddress, port, targetDbName, username, password, retryAttempts, 360000) { }

        #region Public Data
        
        public override bool IsAlive
        {
            get
            {
                bool alive = false;
                if (_connectionHandle != null && _connectionHandle.State != ConnectionState.Closed &&
                    _connectionHandle.State != ConnectionState.Broken)
                    alive = true;
                return alive;
            }
        }

        public override ConnectionState MyCurrentState
        {
            get { return _connectionHandle == null ? ConnectionState.Closed : _connectionHandle.State; }
        }

        public SqlConnection ConnectionHandle { get { return _connectionHandle; } }

        #endregion

        #region Public Members

        public override bool Connect()
        {
            if (_connectionHandle == null || _connectionHandle.State == ConnectionState.Broken || _connectionHandle.State == ConnectionState.Closed)
            {
                return _Connect(0);
            }

            return true;
        }

        public override void SetDatabase(string dataBase)
        {
            if (!IsAlive)
                Connect();

            _connectionHandle.ChangeDatabase(dataBase);
        }

        public override int RunNonQuery(string query)
        {
            if (!IsAlive)
                Connect();

            int result;
            try
            {
                _commandExecutor = new SqlCommand(query, _connectionHandle);
                result = _commandExecutor.ExecuteNonQuery();
                TransactionCount++;
            }
            finally
            {
                _commandExecutor.Dispose();
            }
            return result;
        }


        public override bool Disconnect()
        {
            if (_connectionHandle == null)
                return true;

            //if (_connectionHandle.State == ConnectionState.Executing || _connectionHandle.State == ConnectionState.Connecting ||
            //    _connectionHandle.State == ConnectionState.Fetching)
            //{
            //    _connectionHandle..CancelQuery(_MillisecondTimeout);
            //}

            _connectionHandle.Close();
            _connectionHandle.Dispose();
            _connectionHandle = null;
            return true;
        }


        public override DataTable RunQuery(string query)
        {
            if (!IsAlive)
                Connect();

            SqlDataReader commandReader = null;
            DataTable dt = null;
            try
            {
                _commandExecutor = new SqlCommand(query, _connectionHandle);
                _commandExecutor.CommandTimeout = _MillisecondTimeout;
                commandReader = _commandExecutor.ExecuteReader();
                dt = DataTableUtils.GetTableFromResultSet(ref commandReader);
                TransactionCount++;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception running query: {0}\n{1}", e.Message, e.StackTrace);
            }
            finally
            {
                if (commandReader != null)
                {
                    commandReader.Close();
                    commandReader.Dispose();
                }
                _commandExecutor.Dispose();
            }

            return dt;
        }

        public override string GetExecutionResult()
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Private Data

        private bool _Connect(int attempt)
        {
            if (attempt < _ConnectionAttempts)
            {
                _connectionHandle = new SqlConnection(_GetConnectionString());
                _connectionHandle.Open();

                if (_connectionHandle.State != ConnectionState.Open)
                    return _Connect(++attempt);

                return true;
            }

            throw new Exception(string.Format("Maximum Connection Attempts {0} made to DB host {1} please", _ConnectionAttempts, _DBName));
        }

        private void _RunCommandAsynchronously(string query)
        {
            try
            {
                int count = 0;
                _commandExecutor = new SqlCommand(query, _connectionHandle);

                IAsyncResult result = _commandExecutor.BeginExecuteNonQuery();
                while (!result.IsCompleted)
                {
                    Console.WriteLine("Waiting ({0})", count++);
                    System.Threading.Thread.Sleep(500);
                }
                Console.WriteLine("Command complete. Affected {0} rows.",
                    _commandExecutor.EndExecuteNonQuery(result));
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error ({0}): {1}", ex.Number, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                // You might want to pass these errors 
                // back out to the caller.
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        protected override string _GetConnectionString()
        {
            return string.Format("Server={0};User ID={1};password={4};Connection timeout=30;", _Host, _Username, _DBName, _Port, _Password);
        }

        private void _GetCleanExecutor()
        {
            _commandExecutor.Dispose();
            _commandExecutor = null;
        }

        private SqlConnection _connectionHandle;
        private SqlCommand _commandExecutor;
        #endregion
    }
}
