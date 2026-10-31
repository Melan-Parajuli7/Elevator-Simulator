using System.Drawing;
using System.Windows.Forms;
using Lift.UI.Controls;

namespace Lift.UI
{
    partial class MainForm
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
            lblTitleA = new Label();
            lblTitleB = new Label();
            btnLog = new Button();
            chkPassengers = new CheckBox();
            lobbyFirst = new LobbyControl();
            lobbyGround = new LobbyControl();
            btnExit = new Button();
            unitA = new ElevatorUnitView();
            unitB = new ElevatorUnitView();
            lblCallFirst = new Label();
            btnCallFirst = new PanelButton();
            lblCallGround = new Label();
            btnCallGround = new PanelButton();
            lblStatus = new Label();
            SuspendLayout();
            //
            // lblTitleA
            //
            lblTitleA.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitleA.ForeColor = Color.FromArgb(60, 60, 64);
            lblTitleA.Location = new Point(147, 16);
            lblTitleA.Name = "lblTitleA";
            lblTitleA.Size = new Size(220, 32);
            lblTitleA.TabIndex = 0;
            lblTitleA.Text = "ELEVATOR A";
            lblTitleA.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblTitleB
            //
            lblTitleB.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitleB.ForeColor = Color.FromArgb(60, 60, 64);
            lblTitleB.Location = new Point(702, 16);
            lblTitleB.Name = "lblTitleB";
            lblTitleB.Size = new Size(220, 32);
            lblTitleB.TabIndex = 1;
            lblTitleB.Text = "ELEVATOR B";
            lblTitleB.TextAlign = ContentAlignment.MiddleCenter;
            //
            // btnLog
            //
            btnLog.Font = new Font("Segoe UI", 10F);
            btnLog.Location = new Point(375, 13);
            btnLog.Name = "btnLog";
            btnLog.Size = new Size(90, 38);
            btnLog.TabIndex = 2;
            btnLog.Text = "Log";
            btnLog.UseVisualStyleBackColor = true;
            btnLog.Click += btnLog_Click;
            //
            // chkPassengers
            //
            chkPassengers.Appearance = Appearance.Button;
            chkPassengers.Checked = true;
            chkPassengers.CheckState = CheckState.Checked;
            chkPassengers.Font = new Font("Segoe UI", 10F);
            chkPassengers.Location = new Point(475, 13);
            chkPassengers.Name = "chkPassengers";
            chkPassengers.Size = new Size(120, 38);
            chkPassengers.TabIndex = 11;
            chkPassengers.Text = "Passengers: ON";
            chkPassengers.TextAlign = ContentAlignment.MiddleCenter;
            chkPassengers.UseVisualStyleBackColor = true;
            chkPassengers.CheckedChanged += chkPassengers_CheckedChanged;
            //
            // btnExit
            //
            btnExit.Font = new Font("Segoe UI", 10F);
            btnExit.Location = new Point(605, 13);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(90, 38);
            btnExit.TabIndex = 3;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            //
            // unitA
            //
            unitA.Location = new Point(20, 65);
            unitA.Name = "unitA";
            unitA.Size = new Size(475, 672);
            unitA.TabIndex = 4;
            //
            // unitB
            //
            unitB.Location = new Point(575, 65);
            unitB.Name = "unitB";
            unitB.Size = new Size(475, 672);
            unitB.TabIndex = 5;
            //
            // lblCallFirst
            //
            lblCallFirst.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblCallFirst.ForeColor = Color.FromArgb(95, 95, 100);
            lblCallFirst.Location = new Point(495, 190);
            lblCallFirst.Name = "lblCallFirst";
            lblCallFirst.Size = new Size(80, 18);
            lblCallFirst.TabIndex = 6;
            lblCallFirst.Text = "CALL";
            lblCallFirst.TextAlign = ContentAlignment.MiddleCenter;
            //
            // btnCallFirst   (outside button on the First floor: "bring a lift down to me")
            //
            btnCallFirst.Glyph = PanelGlyph.ArrowDown;
            btnCallFirst.Location = new Point(512, 214);
            btnCallFirst.Look = PanelLook.Dark;
            btnCallFirst.Name = "btnCallFirst";
            btnCallFirst.Size = new Size(46, 46);
            btnCallFirst.TabIndex = 7;
            btnCallFirst.Click += btnCallFirst_Click;
            //
            // lblCallGround
            //
            lblCallGround.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblCallGround.ForeColor = Color.FromArgb(95, 95, 100);
            lblCallGround.Location = new Point(495, 540);
            lblCallGround.Name = "lblCallGround";
            lblCallGround.Size = new Size(80, 18);
            lblCallGround.TabIndex = 8;
            lblCallGround.Text = "CALL";
            lblCallGround.TextAlign = ContentAlignment.MiddleCenter;
            //
            // btnCallGround   (outside button on the Ground floor: "bring a lift up to me")
            //
            btnCallGround.Glyph = PanelGlyph.ArrowUp;
            btnCallGround.Location = new Point(512, 564);
            btnCallGround.Look = PanelLook.Dark;
            btnCallGround.Name = "btnCallGround";
            btnCallGround.Size = new Size(46, 46);
            btnCallGround.TabIndex = 9;
            btnCallGround.Click += btnCallGround_Click;
            //
            // lobbyFirst: people waiting on the First floor
            //
            lobbyFirst.FloorLabel = "1";
            lobbyFirst.Location = new Point(495, 266);
            lobbyFirst.Name = "lobbyFirst";
            lobbyFirst.Size = new Size(80, 84);
            lobbyFirst.TabIndex = 12;
            lobbyFirst.TabStop = false;
            //
            // lobbyGround: people waiting on the Ground floor
            //
            lobbyGround.FloorLabel = "G";
            lobbyGround.Location = new Point(495, 616);
            lobbyGround.Name = "lobbyGround";
            lobbyGround.Size = new Size(80, 84);
            lobbyGround.TabIndex = 13;
            lobbyGround.TabStop = false;
            //
            // lblStatus
            //
            lblStatus.Font = new Font("Segoe UI", 9.5F);
            lblStatus.ForeColor = Color.FromArgb(70, 70, 75);
            lblStatus.Location = new Point(20, 745);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1030, 24);
            lblStatus.TabIndex = 10;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1070, 780);
            Controls.Add(lblTitleA);
            Controls.Add(lblTitleB);
            Controls.Add(btnLog);
            Controls.Add(chkPassengers);
            Controls.Add(lobbyFirst);
            Controls.Add(lobbyGround);
            Controls.Add(btnExit);
            Controls.Add(unitA);
            Controls.Add(unitB);
            Controls.Add(lblCallFirst);
            Controls.Add(btnCallFirst);
            Controls.Add(lblCallGround);
            Controls.Add(btnCallGround);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Elevator - two lifts";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitleA;
        private Label lblTitleB;
        private Button btnLog;
        private CheckBox chkPassengers;
        private LobbyControl lobbyFirst;
        private LobbyControl lobbyGround;
        private Button btnExit;
        private ElevatorUnitView unitA;
        private ElevatorUnitView unitB;
        private Label lblCallFirst;
        private PanelButton btnCallFirst;
        private Label lblCallGround;
        private PanelButton btnCallGround;
        private Label lblStatus;
    }
}
