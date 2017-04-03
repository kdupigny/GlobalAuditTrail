using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GATInterface.FirmModel;
using GATInterface.Forms.PositionData;
using GATInterface.XML;
using GATUtils.Connection.DB;
using GATUtils.Logger;
using GATUtils.Utilities;
using System.Data;

namespace GATInterface.Forms.OrderLogData
{
    public partial class GatInterfaceDevX : Form
    {
        public GatInterfaceDevX()
        {
            _Init();
        }

        public GatInterfaceDevX(DateTime displayDate)
        {
            ApplicationState.Instance.DisplayDate = displayDate;
        }

        private void _Init()
        {
            InitializeComponent();

            //Background worker code for UI updates
            OnProgressReport += MessageReciever;
            _worker = new BackgroundWorker();
            _worker.DoWork += _WorkerDoWork;
            _worker.RunWorkerCompleted += _WorkerRunWorkerCompleted;
            _worker.ProgressChanged += _WorkerProgressChanged;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerAsync();
        }

        private void _RunDate(DateTime dateToGet)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
               
                Monitor.Enter(_fillTable);
                dxGrid1.BeginUpdate();
                if (ApplicationState.Instance.DisplayDate == dateTimePicker1.Value)
                {
                    _fillTable.MergeTable(DbHandle.Instance.GetFills(ct_sourceName,
                                                                   dateToGet.ToString("yyyy-MM-dd")));
                }
                else
                {
                    _fillTable.CleanAndFillTable(DbHandle.Instance.GetFills(ct_sourceName,
                                                                            dateToGet.ToString("yyyy-MM-dd")));
                }
                ApplicationState.Instance.DisplayDate = dateToGet;
            }
            catch(Exception e)
            {
                GatLogger.Instance.AddMessage(string.Format("GUI Error ({0}", e.Message));
            }
            finally
            {
                Monitor.Exit(_fillTable);
                dxGrid1.EndUpdate();
            }
        }

        private void _RefreshThread()
        {
            while (checkBox1.Checked)
            {
                _RunDate(dateTimePicker1.Value);
                Thread.Sleep(5000);
            }
        }

        private static Color s_GetDbStatusColor()
        {
            switch (DbHandle.Instance.ConnectionState)
            {
                case ConnectionState.Broken:
                case ConnectionState.Closed:
                    return Color.Red;
                case ConnectionState.Connecting:
                    return Color.Yellow;
                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                    return Color.Blue;
                case ConnectionState.Open:
                    return Color.Green;
                default:
                    return Color.Black;
            }
        }

        #region Events
        
        private void _BtnGetDateDataClick(object sender, EventArgs e)
        {
            _RunDate(dateTimePicker1.Value);
        }

        private void _CheckBox1CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked) return;

            _refreshThreadHandle = null;
            _refreshThreadHandle = new Thread(_RefreshThread) { Name = "GridDayRefresher", IsBackground = true };
            _refreshThreadHandle.Start();
        }

        private void _GatInterfaceFormClosing(object sender, FormClosingEventArgs e)
        {
            _suspendBackgroundWorker = true;
        }

        //private void _DataGridView1DataError(object sender, DataGridViewDataErrorEventArgs e)
        //{

        //    dataGridView1.Refresh();
        //}
        
        //private void _DataGridView1DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        //{
        //    Cursor.Current = Cursors.Default;
        //    if (_positionForm != null)
        //        _positionForm.UpdateGrid(_fillTable.Table);

        //    dataGridView1.Refresh();
        //}

        //private void _DataGridView1BindingContextChanged(object sender, EventArgs e)
        //{
        //    if (dataGridView1.DataSource == null) return;
            
        //    foreach (DataGridViewColumn col in dataGridView1.Columns)
        //    {
        //        col.HeaderCell = new
        //            DataGridViewAutoFilterColumnHeaderCell(col.HeaderCell);
        //    }
        //    dataGridView1.AutoResizeColumns();
        //    dataGridView1.Refresh();
        //}
        
        //private void _DataGridView1KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Alt && (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up))
        //    {
        //        var filterCell =
        //            dataGridView1.CurrentCell.OwningColumn.HeaderCell as
        //            DataGridViewAutoFilterColumnHeaderCell;
        //        if (filterCell != null)
        //        {
        //            filterCell.ShowDropDownList();
        //            e.Handled = true;
        //        }
        //    }

        //}

        private void _GatInterfaceShown(object sender, EventArgs e)
        {
            dateTimePicker1.Value = MyTime.GetLastTradingDay();
            if (_fillTable == null)
            {
                _fillTable =
                    new BindableDataTable(DbHandle.Instance.GetFills(ct_sourceName,
                                                                     dateTimePicker1.Value.ToString("yyyy-MM-dd")));
                //fillTable.AddConstraints(new string[] {"OrderId", "ClOrdId", "ExecId"});
            }

            //dataGridView1.DataSource = _fillTable.Binding;
            dxGrid1.DataSource = _fillTable.Binding;
        }

        private void _PositionsToolStripMenuItemClick(object sender, EventArgs e)
        {
            _positionForm = new PositionViewDevX(_fillTable.Table);
            _positionForm.Show(this);
        }
        #endregion

        #region ##FORM WORKER##

        BackgroundWorker _worker;
        public event Action<Exception> OnError;
        public event Action<string> OnCanceled;
        public event Action<string> OnCompleted;
        public event Action<string> OnProgressReport;

        bool _suspendBackgroundWorker;

        private void _WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            using (BackgroundWorker wrkr = (BackgroundWorker) sender)
            {
                try
                {
                    while (!_suspendBackgroundWorker)
                    {
                        Thread.Sleep(100);
                        wrkr.ReportProgress(0);
                    }
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            }
        }

        private void _WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (OnProgressReport != null)
            {
                const string message = "test";
                OnProgressReport(message);
            }
        }

        public void MessageReciever(string e)
        {
            _UpdateMainForm();
        }

        private void _WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                if (OnError != null)
                {
                    OnError((Exception)e.Result);
                }

                return;
            }

            if (e.Error != null)
            {
                if (OnError != null)
                {
                    OnError((Exception)e.Result);
                }

                return;
            }

            if (e.Cancelled)
            {
                if (OnCanceled != null)
                {
                    OnCanceled("Worker was canceled!");
                }

                return;
            }

            if (OnCompleted != null)
            {
                OnCompleted("Worker completed!");
            }
        }

        private void _UpdateMainForm()
        {
            SuspendLayout();

            //TODO this is where the grid context should be refreshed

            toolStripLabel_DB_Indicator.ForeColor = s_GetDbStatusColor();
            //toolStripLabel_DisplayedRows.Text = string.Format("{0} Rows", dataGridView1.RowCount);

            Validate();
            ValidateChildren();

            ResumeLayout();
        }

        #endregion ##FORM WORKER##

        private const string ct_sourceName = "GATGUI";

        private Thread _refreshThreadHandle;
        private BindableDataTable _fillTable;
        private PositionViewDevX _positionForm;

        private void dxGrid1_BindingContextChanged(object sender, EventArgs e)
        { 
            dxGrid1.EndUpdate();
        }

    }
}
