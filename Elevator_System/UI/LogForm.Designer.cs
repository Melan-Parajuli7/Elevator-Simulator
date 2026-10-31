using System.Drawing;
using System.Windows.Forms;

namespace Lift.UI
{
    partial class LogForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblTitle = new Label();
            btnRefresh = new Button();
            btnClear = new Button();
            btnClose = new Button();
            dgvLog = new DataGridView();
            lblInfo = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 14);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 34);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Activity Log";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnRefresh
            //
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Font = new Font("Segoe UI", 10F);
            btnRefresh.Location = new Point(470, 14);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 34);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            //
            // btnClear
            //
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Font = new Font("Segoe UI", 10F);
            btnClear.Location = new Point(580, 14);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 34);
            btnClear.TabIndex = 2;
            btnClear.Text = "Clear Log";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            //
            // btnClose
            //
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Font = new Font("Segoe UI", 10F);
            btnClose.Location = new Point(690, 14);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 34);
            btnClose.TabIndex = 3;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            //
            // dgvLog
            //
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AllowUserToDeleteRows = false;
            dgvLog.AllowUserToResizeRows = false;
            dgvLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvLog.BackgroundColor = Color.FromArgb(170, 170, 170);
            dgvLog.ColumnHeadersHeight = 32;
            dgvLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvLog.Location = new Point(20, 60);
            dgvLog.MultiSelect = false;
            dgvLog.Name = "dgvLog";
            dgvLog.ReadOnly = true;
            dgvLog.RowHeadersVisible = false;
            dgvLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLog.Size = new Size(770, 440);
            dgvLog.TabIndex = 4;
            //
            // lblInfo
            //
            lblInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblInfo.Location = new Point(20, 508);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(770, 24);
            lblInfo.TabIndex = 5;
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            //
            // LogForm
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(810, 540);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(btnClear);
            Controls.Add(btnClose);
            Controls.Add(dgvLog);
            Controls.Add(lblInfo);
            MinimumSize = new Size(640, 360);
            Name = "LogForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Elevator - Log";
            FormClosed += LogForm_FormClosed;
            Load += LogForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitle;
        private Button btnRefresh;
        private Button btnClear;
        private Button btnClose;
        private DataGridView dgvLog;
        private Label lblInfo;
    }
}
