using System;
using System.Threading;
using System.Data;
using GATUtils.Logger;
using GATUtils.XML;

namespace GATUtils.Connection.DB.Commands
{
    public class GetFillsByDateCommand : GenericDbCommand
    {
        public override string CommandName { get { return "GetFillsByDateCommand"; } }

        public GetFillsByDateCommand(string source, string queryId, string startDate, string endDate)
            : base (source, CommandPriority.Highest, queryId, XmlSettings.Instance.Db.Db, XmlSettings.Instance.Db.FillTable)
        {
            _startDate = startDate;
            _endDate = endDate;
        }

        public GetFillsByDateCommand(string source, string queryId, string startDate)
            : this(source, queryId, startDate, startDate)
        { }

        public override object GetResult()
        {
            while (_result == null) Thread.Sleep(300); 
            return _result;
        }

        public override void Execute(DbConnection dbConnection)
        {
            try
            {
                if (CancelCommand)
                {
                    _LogCommandCancelled();
                    return;
                }

                int sleepTime = _SleepTime;
                int retries = _Retries;
                while (retries >= 0)
                {
                    if (CancelCommand)
                    {
                        _LogCommandCancelled();
                        return;
                    }

                    try
                    {
                        _result = dbConnection.RunQuery(_CommandText);
                        return;
                    }
                    catch(Exception e)
                    {
                        GatLogger.Instance.AddMessage(string.Format("DB COMMAND ERROR (retries [{2}]): \n{3} \n{0} \n\t\t {1}",
                                                                    e.Message, e.StackTrace, retries, _CommandText), LogMode.LogAndScreen);
                    }
                    
                    Thread.Sleep(sleepTime);
                    sleepTime *= 4;
                    retries--;
                }
                //OnActionFailed();
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage(string.Format("DB COMMAND EXECUTION ERROR: {0} \n {1}", e.Message, e.StackTrace));
                throw;
            }
            finally
            {
                _AfterExecute();
            }
        }

        protected override string _CommandText
        {
            get
            {
                return string.Format( string.Format("SELECT * FROM {2}.{3} WHERE DATE>='{0}' AND DATE<='{1}';", _startDate, _endDate, _MyDb, _MyTable));
            }
        }

        private readonly string _startDate;
        private readonly string _endDate;
        private DataTable _result;
    }
}
