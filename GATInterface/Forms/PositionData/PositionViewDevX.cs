using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using GATInterface.Forms.OrderLogData;
using GATInterface.XML;

namespace GATInterface.Forms.PositionData
{
    public partial class PositionViewDevX : Form
    {
        public PositionTable Table { get { return _positionTable ?? (_positionTable = new PositionTable()); } }

        public PositionViewDevX()
        {
            InitializeComponent();

            dateTimePicker1.Value = ApplicationState.Instance.DisplayDate;

            //Background worker code for UI updates
            OnProgressReport += MessageReciever;
            _worker = new BackgroundWorker();
            _worker.DoWork += _WorkerDoWork;
            _worker.RunWorkerCompleted += _WorkerRunWorkerCompleted;
            _worker.ProgressChanged += _WorkerProgressChanged;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerAsync();

            //Table.Update(dateTimePicker1.Value);
        }

        public PositionViewDevX (DataTable rawData)
            : this ()
        {
            UpdateGrid(rawData);
        }

        public void UpdateGrid(DataTable rawData)
        {
            _rawData = rawData;
            Table.Update(rawData);
        }

        #region Events

//        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
//        {
//            dataGridView1.Refresh();
//        }

//        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
//        {
//            Cursor.Current = Cursors.Default;
//            dataGridView1.Refresh();
//        }

//        private void dataGridView1_BindingContextChanged(object sender, EventArgs e)
//        {
//            if (dataGridView1.DataSource == null) return;

//            foreach (DataGridViewColumn col in dataGridView1.Columns)
//            {
//                col.HeaderCell = new
//                    DataGridViewAutoFilterColumnHeaderCell(col.HeaderCell);
//            }
//            dataGridView1.AutoResizeColumns();
//            dataGridView1.Refresh();
//        }

//        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
//        {
//            if (e.Alt && (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up))
//            {
//                var filterCell =
//                    dataGridView1.CurrentCell.OwningColumn.HeaderCell as
//                    DataGridViewAutoFilterColumnHeaderCell;
//                if (filterCell != null)
//                {
//                    filterCell.ShowDropDownList();
//                    e.Handled = true;
//                }
//            }

//        }


//// ReSharper disable MemberCanBeMadeStatic.Local
//        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
//// ReSharper restore MemberCanBeMadeStatic.Local
//        {
//            if (e.Value != null)
//                e.Value = e.Value.ToString();
//        }

        private void PositionView_Shown(object sender, EventArgs e)
        {
            if(_rawData != null)
                Table.Update(_rawData);

            //dataGridView1.DataSource = Table.Binding;
            dxGrid1.DataSource = Table.Binding;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PositionView_FormClosing(object sender, FormClosingEventArgs e)
        {
            _suspendBackgroundWorker = true;
        }

        #endregion

        #region ##FORM WORKER##

        private bool _suspendBackgroundWorker = false;
        
        readonly BackgroundWorker _worker;
        public event Action<Exception> OnError;
        public event Action<string> OnCanceled;
        public event Action<string> OnCompleted;
        public event Action<string> OnProgressReport;

        private void _WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker wrkr = (BackgroundWorker)sender;

            try
            {
                while (!_suspendBackgroundWorker)
                {
                    System.Threading.Thread.Sleep(100);
                    wrkr.ReportProgress(0);
                }
            }
            catch (Exception ex)
            {
                e.Result = (object)ex;
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
                    if (e.Result != null) 
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

            //toolStripLabel_DisplayedRows.Text = string.Format("{0} Rows", dataGridView1.RowCount);
            dxGrid1.RefreshDataSource();

            Validate();
            ValidateChildren();

            this.ResumeLayout();
        }

        #endregion ##FORM WORKER##

        private PositionTable _positionTable;
        private DataTable _rawData;

        private void button2_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Table.Update(dateTimePicker1.Value);

            Cursor.Current = Cursors.Default;
        }

        private void orderLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            devExecutionForm = new GatInterfaceDevX ();
            devExecutionForm.Show();
        }

        GatInterfaceDevX devExecutionForm;
    }
}
