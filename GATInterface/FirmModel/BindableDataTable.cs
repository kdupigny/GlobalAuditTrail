using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GATInterface.FirmModel
{
    /// <summary>
    /// Thread safe Data table with exposed binding
    /// </summary>
    public class BindableDataTable
    {
        /// <summary>
        /// Gets the table.
        /// </summary>
        public DataTable Table
        {
            get { return _table ?? (_table = new DataTable()); }
        }
        /// <summary>
        /// Gets the table columns.
        /// </summary>
        public DataColumnCollection TableColumns { get { return Table.Columns; } }

        /// <summary>
        /// Gets the table's binding source.
        /// </summary>
        public BindingSource Binding
        {
            get { return _binder ?? (_binder = new BindingSource {DataSource = Table}); }
        }

        public BindableDataTable(DataTable schemaTable)
        {
            CleanAndFillTable(schemaTable);
        }
        public BindableDataTable(DataColumn[] tableColumns)
        {
            Table.Columns.AddRange(tableColumns);
        }
        public BindableDataTable(string [] tableColumns)
        {
            SetTableColumns(tableColumns);
        }

        /// <summary>
        /// Cleans and refills the data table.
        /// </summary>
        /// <param name="newTable">The new table.</param>
        public void CleanAndFillTable(DataTable newTable)
        {
            try
            {
                Monitor.Enter(Table);
                Table.BeginLoadData();
                Table.Clear();
                Table.Load(newTable.CreateDataReader());
            }
            finally
            {
                Table.EndLoadData(); 
                Monitor.Exit(Table);
            }
        }

        /// <summary>
        /// Cleans and refills the data table.
        /// </summary>
        /// <param name="tableRows">A collection of table rows.</param>
        public void CleanAndFillTable(IEnumerable<DataRow> tableRows)
        {
            try
            {
                Monitor.Enter(Table);

                Table.BeginLoadData();
                foreach (var tableRow in tableRows)
                {
                    //Table.Rows.Add(tableRow);
                    Table.LoadDataRow(tableRow.ItemArray, LoadOption.OverwriteChanges);
                }
            }
            finally
            {
                Table.EndLoadData(); 
                Monitor.Exit(Table);
            }
        }

        /// <summary>
        /// Appends the new table's data rows to the existing table.
        /// </summary>
        /// <param name="newTable">The new table.</param>
        public void MergeTable(DataTable newTable)
        {
            try
            {
                Monitor.Enter(Table);
                Table.BeginLoadData();
                Table.Merge(newTable, false, MissingSchemaAction.AddWithKey);
            }
            finally
            {
                Table.EndLoadData(); 
                Monitor.Exit(Table);
            }
        }

        /// <summary>
        /// Appends data row collection to the existing table.
        /// </summary>
        /// <param name="tableRows">The table rows.</param>
        public void MergeTable(IEnumerable<DataRow> tableRows)
        {
            try
            {
                Monitor.Enter(Table);
                Table.BeginLoadData();
                foreach (var dataRow in tableRows)
                {
                    Table.Rows.Add(dataRow);
                }
            }
            finally
            {
                Table.EndLoadData();
                Monitor.Exit(Table);
            }
        }
        
        /// <summary>
        /// Sets the table columns to the list provided.  The existing data
        /// will be dumped.
        /// </summary>
        /// <param name="columnNames">The column names.</param>
        public void SetTableColumns(string [] columnNames)
        {
            try
            {
                Monitor.Enter(Table);
                Table.BeginLoadData();
                
                Table.Clear();         //clears data
                Table.Columns.Clear(); //clears columns
                foreach (var columnName in columnNames)
                {
                    Table.Columns.Add(columnName);
                }
            }
            finally
            {
                Table.EndLoadData();
                Monitor.Exit(Table);
            }
        }

        /// <summary>
        /// Adds column constraints to the data table.
        /// </summary>
        /// <param name="constraintCols">Array of constraint columns.</param>
        public void AddConstraints(string [] constraintCols)
        {
            Table.Constraints.Clear();
            UniqueConstraint constraint = new UniqueConstraint(constraintCols.Select(uniqueCol => Table.Columns[uniqueCol.ToUpper()]).ToArray());
            Table.Constraints.Add(constraint);
        }

        private DataTable _table;
        private BindingSource _binder;
    }
}
