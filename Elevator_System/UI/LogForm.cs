using System;
using System.Data;
using System.Windows.Forms;
using Lift.Data;

namespace Lift.UI
{
    /// <summary>
    /// Shows the stored history of both lifts (ID, Date, Time, Elevator, Floor, Event, Action).
    /// Loading and clearing run on a BackgroundWorker, so the window and the lifts never freeze.
    /// </summary>
    public partial class LogForm : Form
    {
        private readonly IElevatorLogRepository _repository;
        private readonly BackgroundLogWriter _writer;
        private bool _busy;

        public LogForm(IElevatorLogRepository repository, BackgroundLogWriter writer)
        {
            InitializeComponent();
            _repository = repository;
            _writer = writer;
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            _writer.BatchSaved += OnBatchSaved;       // new entries reach the database: refresh the grid
            LoadHistory();
        }

        private void LogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _writer.BatchSaved -= OnBatchSaved;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadHistory();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show(
                "Delete the whole log from the database?", "Clear Log",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            RunDatabaseJob(() => _repository.ClearHistory());
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OnBatchSaved(int rows)
        {
            if (!_busy && !IsDisposed) LoadHistory();
        }

        private void LoadHistory()
        {
            SetBusy(true, "Loading ...");

            DbBackgroundRunner.Run<DataTable>(
                () => _repository.LoadHistory(),
                table =>
                {
                    if (IsDisposed) return;
                    dgvLog.DataSource = table;
                    ConfigureColumns();
                    SetBusy(false, table.Rows.Count + " records");
                },
                ex =>
                {
                    if (IsDisposed) return;
                    SetBusy(false, "Could not load the log: " + ex.Message);
                });
        }

        // One method for every change to the database (clear, and any future one): no duplicated handlers.
        private void RunDatabaseJob(Action job)
        {
            SetBusy(true, "Working ...");

            DbBackgroundRunner.Run(
                job,
                () => { if (!IsDisposed) LoadHistory(); },
                ex =>
                {
                    if (IsDisposed) return;
                    SetBusy(false, "Database error: " + ex.Message);
                    MessageBox.Show(ex.Message, "Database problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
        }

        private void ConfigureColumns()
        {
            if (dgvLog.Columns.Count < 7) return;

            dgvLog.Columns["ID"].Width = 55;
            dgvLog.Columns["Date"].Width = 90;
            dgvLog.Columns["Time"].Width = 80;
            dgvLog.Columns["Elevator"].Width = 100;
            dgvLog.Columns["Floor"].Width = 100;
            dgvLog.Columns["Event"].Width = 150;
            dgvLog.Columns["Action"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvLog.ClearSelection();
        }

        private void SetBusy(bool busy, string text)
        {
            _busy = busy;
            btnRefresh.Enabled = !busy;
            btnClear.Enabled = !busy;
            lblInfo.Text = text;
        }
    }
}
