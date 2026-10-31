using System.Drawing;
using System.Windows.Forms;
using Lift.Models;
using Lift.UI.Controls;

namespace Lift.UI
{
    partial class ElevatorUnitView
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            doorFirst = new DoorControl();
            doorGround = new DoorControl();
            shaft = new ShaftControl();
            panelMetal = new MetalPanel();
            ledDisplay = new LedDisplay();
            btnFloor1 = new PanelButton();
            btnFloorG = new PanelButton();
            btnClose = new PanelButton();
            btnOpen = new PanelButton();
            btnEmergency = new PanelButton();
            panelMetal.SuspendLayout();
            SuspendLayout();
            //
            // doorFirst
            //
            doorFirst.Caption = "FIRST FLOOR";
            doorFirst.Location = new Point(0, 0);
            doorFirst.Name = "doorFirst";
            doorFirst.ServedFloor = Floor.First;
            doorFirst.Size = new Size(215, 322);
            doorFirst.TabIndex = 0;
            doorFirst.TabStop = false;
            //
            // doorGround
            //
            doorGround.Caption = "GROUND FLOOR";
            doorGround.Location = new Point(0, 350);
            doorGround.Name = "doorGround";
            doorGround.ServedFloor = Floor.Ground;
            doorGround.Size = new Size(215, 322);
            doorGround.TabIndex = 1;
            doorGround.TabStop = false;
            //
            // shaft: the moving car (container)
            //
            shaft.FirstLevelY = 175;
            shaft.GroundLevelY = 525;
            shaft.Location = new Point(219, 0);
            shaft.Name = "shaft";
            shaft.Size = new Size(80, 672);
            shaft.TabIndex = 3;
            shaft.TabStop = false;
            //
            // panelMetal
            //
            panelMetal.Controls.Add(ledDisplay);
            panelMetal.Controls.Add(btnFloor1);
            panelMetal.Controls.Add(btnFloorG);
            panelMetal.Controls.Add(btnClose);
            panelMetal.Controls.Add(btnOpen);
            panelMetal.Controls.Add(btnEmergency);
            panelMetal.Location = new Point(303, 0);
            panelMetal.Name = "panelMetal";
            panelMetal.Size = new Size(172, 672);
            panelMetal.TabIndex = 2;
            //
            // ledDisplay
            //
            ledDisplay.Location = new Point(16, 22);
            ledDisplay.Name = "ledDisplay";
            ledDisplay.Size = new Size(140, 262);
            ledDisplay.TabIndex = 0;
            ledDisplay.TabStop = false;
            //
            // btnFloor1
            //
            btnFloor1.Location = new Point(50, 300);
            btnFloor1.Name = "btnFloor1";
            btnFloor1.Size = new Size(72, 72);
            btnFloor1.TabIndex = 1;
            btnFloor1.Text = "1";
            btnFloor1.Click += btnFloor1_Click;
            //
            // btnFloorG
            //
            btnFloorG.Location = new Point(50, 384);
            btnFloorG.Name = "btnFloorG";
            btnFloorG.Size = new Size(72, 72);
            btnFloorG.TabIndex = 2;
            btnFloorG.Text = "G";
            btnFloorG.Click += btnFloorG_Click;
            //
            // btnClose
            //
            btnClose.Glyph = PanelGlyph.CloseDoors;
            btnClose.Location = new Point(8, 474);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(72, 72);
            btnClose.TabIndex = 3;
            btnClose.Click += btnClose_Click;
            //
            // btnOpen
            //
            btnOpen.Glyph = PanelGlyph.OpenDoors;
            btnOpen.Location = new Point(92, 474);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(72, 72);
            btnOpen.TabIndex = 4;
            btnOpen.Click += btnOpen_Click;
            //
            // btnEmergency
            //
            btnEmergency.Glyph = PanelGlyph.Emergency;
            btnEmergency.Location = new Point(36, 566);
            btnEmergency.Look = PanelLook.Alarm;
            btnEmergency.Name = "btnEmergency";
            btnEmergency.Size = new Size(100, 80);
            btnEmergency.TabIndex = 5;
            btnEmergency.Click += btnEmergency_Click;
            //
            // ElevatorUnitView
            //
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(doorFirst);
            Controls.Add(doorGround);
            Controls.Add(shaft);
            Controls.Add(panelMetal);
            Name = "ElevatorUnitView";
            Size = new Size(475, 672);
            panelMetal.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DoorControl doorFirst;
        private DoorControl doorGround;
        private ShaftControl shaft;
        private MetalPanel panelMetal;
        private LedDisplay ledDisplay;
        private PanelButton btnFloor1;
        private PanelButton btnFloorG;
        private PanelButton btnClose;
        private PanelButton btnOpen;
        private PanelButton btnEmergency;
    }
}
