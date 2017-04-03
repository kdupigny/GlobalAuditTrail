using System;
using System.Collections.Generic;
using System.Linq;
using GATUtils.Logger;
using GATUtils.Utilities;
using GATUtils.Types;
using GATUtils.Connection.DB.Commands;
using System.Threading;

namespace GATUtils.Connection.DB
{
    public class DbMessageQueue
    {
        public DbConnection DbConnection { get; set; }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return (_commandsToExecute == null ? 0 : _commandsToExecute.Count) + _queues.Values.Sum(queue => queue.Count());
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbMessageQueue"/> class.
        /// </summary>
        /// <param name="dbConnection"></param>
        public DbMessageQueue(DbConnection dbConnection)
        {
            if (dbConnection == null) throw new ArgumentNullException("dbConnection");
            
            DbConnection = dbConnection;
            _dbConnection = dbConnection;
            _cancelledQueries = new HashSet<string>();
            EnumComparer<CommandPriority> comparer = EnumComparer<CommandPriority>.s_Instance;
            _queues = new Dictionary<CommandPriority, ThreadSafeMultiQueue<string, IDBCommand>>(comparer);

            foreach (CommandPriority priority in Enum.GetValues(typeof(CommandPriority)))
                _queues[priority] = new ThreadSafeMultiQueue<string, IDBCommand>();

            //multiQueue = new ThreadSafeQueue<string, IDBCommand>();

            _commandsToExecute = new Queue<IDBCommand>();

            _queueThread = new Thread(_QueueRunner) {Name = "CommandQueue", IsBackground = false};
            _queueThread.Start();
        }
       
        /// <summary>
        /// insert a new IDBCommand to the queue
        /// </summary>
        /// <param name="command">Command</param>
        public void Insert(IDBCommand command)
        {
            lock (_locker)
            {
                _queues[command.Priority].Add(command.SourceName, command);
                command.SetQueueInsertionTimeToNow();
            }

            // Command insertions per minute 
            DateTime now = DateTime.Now;
            if (_lastCommandInsertionTime.Minute == now.Minute)
                _commandsInsertedInLastMinute++;
            else
            {
                _commandsInsertedInLastMinute = 0;
            }
            _lastCommandInsertionTime = now;

            if (!isProcessing)
            {
                isProcessing = true;
                GatLogger.Instance.AddMessage("DB Queue has messages please do not exit.", LogMode.LogAndScreen);
            }
        }

        /// <summary>
        /// Safely remove 1 by 1 commands from the queue and execute it
        /// </summary>
        public void Execute()
        {
            try
            {
                while (Count > 0) //as long as not all commands are in DB
                {
                    CommandPriority priority = _GetHighestNonEmptyPriority();
                    ThreadSafeMultiQueue<string, IDBCommand> multiQueue = _queues[priority];
                        _ExecuteNonFilteredQueue(multiQueue, priority);
                }
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage(string.Format("DB Command failed with following message {0}\n{1}", e.Message, e.StackTrace));
                ErrorEventHandler(e);
                throw;
            }
        }
        
        /// <summary>
        /// Empty the queue from commands, calling errorEventHandler of each command (calling set() on commands blocking events)
        /// </summary>
        public void ErrorEventHandler(Exception es)
        {
            //update error in the rest of the commands in the queue
            foreach (ThreadSafeMultiQueue<string, IDBCommand> multiQueue in _queues.Values)
            {
                foreach (IDBCommand command in multiQueue)
                {
                    try
                    {
                        if (command != null)
                        {
                            command.ErrorEventHandler();
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        //GeneralLogger.Instance.AddLine(LogMessageType.CommandsQueueExceptionInErrorHandler,
                        //    e.Message, e.StackTrace);
                        GatLogger.Instance.AddMessage("Error could not be handled " + e.Message, LogMode.LogAndScreen);
                    }
                }
            }
        }

        /// <summary>
        /// Cancels the query.
        /// </summary>
        /// <param name="queryId">The query id.</param>
        public void CancelQuery(string queryId)
        {
            GatLogger.Instance.AddMessage(string.Format("{0}: Cancelling DB command [Id={1}]", ct_sourceName, queryId));
            try
            {
                _cancelledQueries.Add(queryId);
            }
            catch (Exception e) //Since calling CancelQuery is rare, we'll use try catch in order to avoid locking cancelledQueries, which is used by executeCommand
            {
                GatLogger.Instance.AddMessage(string.Format("{0}: Exception Cancelling Command [Id={1}] - {2}\n{3}", ct_sourceName, queryId, e.Message, e));
            }
        }

        private void _ExecuteNonFilteredQueue(ThreadSafeMultiQueue<string, IDBCommand> multiQueue, CommandPriority currentPriority)
        {
            while (multiQueue.Count() > 0)
            {
                foreach (IDBCommand command in multiQueue)
                {
                    _ExecuteCommand(command);
                    if (_GetHighestNonEmptyPriority() < currentPriority)
                        break;
                }
            }

            GatLogger.Instance.AddMessage("DB Queue is empty.  You can exit at anytime.",LogMode.LogAndScreen);
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="e">The e.</param>
        private static void s_LogAndThrowException(IDBCommand command, Exception e)
        {
            if (e != null)
            {
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    GatLogger.Instance.AddMessage(string.Format("{0}: Exception Executing Command [{1}] - \n{2}",
                                                                ct_sourceName, e.Message, e.StackTrace), LogMode.LogAndScreen);
                }
            }
            throw new GatDataBaseException(string.Format("Exception was thrown while executing command: {0} Exception: {1}", command, e == null ? "Null" : e.Message), new InvalidOperationException());
        }
        /// <summary>
        /// Filters and executes the commands until multi queue getting full again and or command is new is queue
        /// </summary>
        private void _FilterAndExecuteCommands(ThreadSafeMultiQueue<string, IDBCommand> multiQueue, CommandPriority currentPriority)
        {
            bool queueTooLoaded = multiQueue.Count() > ct_queueThreshold;

            while (_commandsToExecute.Count > 0)
            {
                if (_GetHighestNonEmptyPriority() < currentPriority)
                    return;

                if (queueTooLoaded)
                {
                    //if filtering process began when queue was too loaded or there is blocking command in queue
                    //all commands that passed in the filter will be executed regardless of
                    //how much time they were in queue
                    _ExecuteCommand(_commandsToExecute.Dequeue());
                }
                else if (_MinimumCommandQueueTime.HasValue)
                {
                    //If commands was not in queue for enough time the execution process is halted
                    IDBCommand nextCommand = _commandsToExecute.Peek();
                    DateTime filterThreshold = nextCommand.GetQueueInsertionTime() + _MinimumCommandQueueTime.Value;
                    if (_lastFilterEndTime < filterThreshold)
                        return;

                    _ExecuteCommand(_commandsToExecute.Dequeue());
                }
                else
                {
                    throw new GatDataBaseException(
                        "filterAndExecuteCommands was called when queueTooLoaded = false and !parameters.MinimumCommandQueueTime.HasValue");
                }

                //If queue is getting loaded the execution process is halted
                if (multiQueue.Count() >= ct_queueThreshold)
                    return;
            }
        }
        private void _ExecuteCommand(IDBCommand command)
        {
            if (command != null)
            {
                if (command.QueryId != null && _cancelledQueries.Contains(command.QueryId))
                {
                    command.CancelCommand = true;
                }

                //if (command.Blocking)
                //    Interlocked.Decrement(ref blockingCommandsCount);

               
                try
                {
                    TimeSpan waitInQueueSpanTime = DateTime.Now.Subtract(command.GetQueueInsertionTime());
                    DateTime executionStartTime = DateTime.Now;

                    //if (!isDummy)
                    //    GeneralLogger.Instance.AddLine(LogMessageType.CommandsQueueBlocking, command, command.Blocking);
                  
                    command.Execute(_dbConnection);

                    // Writing Command DONE message to log
                    TimeSpan executionSpanTime = DateTime.Now.Subtract(executionStartTime);                    
                    /******************************************/

                    // Queue size exceeded - issuing warning, activating callback
                    if (Count > ct_commandQueueSizeExceededWarningThreshold)
                        GatLogger.Instance.AddMessage("Commands queue threshold exceeded", LogMode.LogAndScreen);
                        //GeneralLogger.Instance.AddLine(
                        //    LogMessageType.CommandsQueueNumberOfDBCommandsExceeded,
                        //    commandQueueSizeExceededWarningThreshold, Count);


                    // Command executions per minute 
                    DateTime now = DateTime.Now;
                    if (_lastCommandExecutionTime.Minute == now.Minute)
                        _commandsExecutedInLastMinute++;
                    else
                    {
                        //GeneralLogger.Instance.AddLine(LogMessageType.CommandsQueueCurrExecutionRate, commandsExecutedInLastMinute);
                        _commandsExecutedInLastMinute = 0;
                    }

                    _lastCommandExecutionTime = now;                    
                }
                catch (Exception e)
                {
                    s_LogAndThrowException(command, e);
                    command.ErrorEventHandler();
                }

            }
        }

        private void _QueueRunner() // Thread
        {
            while (true)
            {
                Execute();
                Thread.Sleep(100);
            }
        }

        private CommandPriority _GetHighestNonEmptyPriority()
        {
            CommandPriority priority = _commandsToExecute.Count > 0 ? CommandPriority.Normal : CommandPriority.Lowest;

            foreach (KeyValuePair<CommandPriority, ThreadSafeMultiQueue<string, IDBCommand>> keyValuePair in _queues)
            {
                if (keyValuePair.Key < priority && !keyValuePair.Value.IsEmpty())
                    priority = keyValuePair.Key;
            }

            return priority;
        }


        private long _commandsExecutedInLastMinute;
        private long _commandsInsertedInLastMinute;
        private readonly DateTime _lastFilterEndTime = DateTime.Now;
        private DateTime _lastCommandExecutionTime = DateTime.Now;
        private DateTime _lastCommandInsertionTime = DateTime.Now;
        private readonly HashSet<string> _cancelledQueries;

        private readonly Thread _queueThread;

        private readonly Queue<IDBCommand> _commandsToExecute;
        private readonly Dictionary<CommandPriority, ThreadSafeMultiQueue<string, IDBCommand>> _queues;
        private readonly object _locker = new object();

        private const int ct_queueThreshold = 100000;
        private static TimeSpan? _MinimumCommandQueueTime { get { return new TimeSpan(0, 0, 0, 0, 500); } }
        private const int ct_commandQueueSizeExceededWarningThreshold = 50000;

        private readonly DbConnection _dbConnection;
        private const string ct_sourceName = "DB Message Queue";

        private bool isProcessing = false;

    }
}
