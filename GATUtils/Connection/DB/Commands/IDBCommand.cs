using System;

namespace GATUtils.Connection.DB.Commands
{
    public interface IDBCommand
    {
        CommandPriority Priority { get; }
        string SourceName { get; }
        object GetResult();
        string CommandName { get; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        void Execute(DbConnection dbConnection);

        /// <summary>
        /// Handles errors.
        /// </summary>
        void ErrorEventHandler();
        
        /// <summary>
        /// Waits if needed.
        /// </summary>
        void WaitIfNeeded(int milliSecondsToWait);

        /// <summary>
        /// Gets the result string.
        /// </summary>
        /// <returns>The result string</returns>
        string ToResultString();

        /// <summary>
        /// Gets the queue insertion time.
        /// </summary>
        /// <returns></returns>
        DateTime GetQueueInsertionTime();

        /// <summary>
        /// Sets the queue insertion time to now.
        /// </summary>
        void SetQueueInsertionTimeToNow();

        /// <summary>
        /// Gets or sets a value indicating whether this command failed.
        /// </summary>
        /// <value><c>true</c> if failed; otherwise, <c>false</c>.</value>
        bool Failed { get; }

        /// <summary>
        /// Gets or sets the query ID that this command is attached to.
        /// Null means that the command is not attached to a query.        
        /// Each call in the Facade that desires the ability to be cancelled,
        /// needs to add query ID string to itself and assign it to its respectful command.
        /// </summary>
        /// <value>The query ID.</value>
        string QueryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the command.
        /// </summary>
        /// <value>
        ///   <c>true</c> if command should be cancelled; otherwise, <c>false</c>.
        /// </value>
        bool CancelCommand { get; set; }

        /// <summary>
        /// Cancels the command if it belongs to the query.
        /// </summary>
        /// <param name="queryId">The query ID.</param>
        void CancelCommandIfNeeded(string queryId);
    }
}
