using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lift.Controller;
using Lift.Data;
using Lift.Models;
using Lift.Observer;
using Lift.Subject;

namespace Lift.UI
{
    /// <summary>
    /// The main window and the "composition root" of the program: it creates the lifts, the observers,
    /// the timer and the database classes and connects them. It contains no lift logic and no SQL.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly ElevatorSystem _system = new ElevatorSystem();
        private readonly SimulationClock _clock = new SimulationClock();
        private readonly SoundService _sound = new SoundService();
        private readonly DatabaseSettings _settings = DatabaseSettings.CreateDefault();
        private readonly IElevatorLogRepository _repository;
        private readonly BackgroundLogWriter _logWriter;
        private readonly ElevatorUnitView[] _units;
        private readonly PassengerSimulator _passengers;

        private LogForm _logForm;
        private bool _databaseErrorShown;

        public MainForm()
        {
            InitializeComponent();

            _repository = new SqlElevatorLogRepository(_settings);
            _logWriter = new BackgroundLogWriter(_repository);   // created on the GUI thread: events come back here
            _logWriter.SaveFailed += OnDatabaseError;
            _units = new ElevatorUnitView[] { unitA, unitB };

            // one passenger starts on each floor, so both lifts are used in both directions
            _passengers = new PassengerSimulator(_system, new Passenger(1, Floor.Ground, 30), new Passenger(2, Floor.First, 70));
            _passengers.Activity += OnPassengerActivity;

            WireLifts();

            _system.Dispatcher.CallHandled += OnHallCallHandled;
            _system.Stepped += OnSystemStepped;     // delegate: redraw after every step
            _clock.Tick += OnClockTick;             // delegate: the timer drives the animation
        }

        // ------------------------------------------------------------------ set-up
        /// <summary>Connects every lift (Subject) to its observers and its control panel.</summary>
        private void WireLifts()
        {
            for (int i = 0; i < _system.Elevators.Count; i++)
            {
                Elevator lift = _system.Elevators[i];
                ElevatorUnitView unit = _units[i];

                lift.Attach(new FloorDisplayObserver(unit.FirstFloorDisplay, unit.GroundFloorDisplay));
                lift.Attach(new ControlPanelObserver(unit));
                lift.Attach(new LogObserver(_logWriter));
                lift.Attach(new SoundObserver(_sound));

                // buttons INSIDE a lift go only to that lift
                unit.FloorButtonPressed += floor => lift.RequestFloorFromPanel(floor);
                unit.OpenButtonPressed += () => lift.PressOpen();
                unit.CloseButtonPressed += () => lift.PressClose();
                unit.EmergencyButtonPressed += () => lift.PressEmergency();

                lift.PublishStatus();               // show the starting position on every display
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetStatus("Connecting to the database ...");
            _logWriter.Write(LogEntry.ForSystem("Startup", "Elevator system started"));

            // creating the database can take a few seconds the first time: do it on a BackgroundWorker
            DbBackgroundRunner.Run(
                () => new DatabaseInitializer(_settings).EnsureCreated(),
                OnDatabaseReady,
                OnDatabaseError);

            _clock.Start();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _clock.Stop();
            _clock.Dispose();
            _sound.Dispose();
        }

        // ------------------------------------------------------------------ outside (hall) buttons
        private void btnCallFirst_Click(object sender, EventArgs e)
        {
            CallFromHall(Floor.First);
        }

        private void btnCallGround_Click(object sender, EventArgs e)
        {
            CallFromHall(Floor.Ground);
        }

        // Both hall buttons use this one method: no duplicated code.
        private void CallFromHall(Floor floor)
        {
            _logWriter.Write(new LogEntry(null, (int)floor, "HallButtonPressed",
                "Call button pressed at " + floor.DisplayName(), DateTime.Now));

            _system.Dispatcher.RequestFromHall(floor);
        }

        private void OnHallCallHandled(object sender, HallCallResult result)
        {
            SetStatus(result.Message);

            if (!result.Accepted)
            {
                _logWriter.Write(new LogEntry(null, (int)result.Floor, "HallCallRejected", result.Message, DateTime.Now));
            }
            else if (result.AlreadyServed)
            {
                _logWriter.Write(new LogEntry(result.Elevator.Id, (int)result.Floor, "HallCallAlreadyServed", result.Message, DateTime.Now));
            }
        }

        // ------------------------------------------------------------------ animation (timer + delegates)
        private void OnClockTick(object sender, EventArgs e)
        {
            // door sensor first, so the doors react to the pointer on this very tick
            for (int i = 0; i < _units.Length; i++)
            {
                Elevator lift = _system.Elevators[i];
                lift.SetObstruction(_units[i].IsPointerInDoorway(lift));
            }

            _passengers.Step();     // people press buttons, step in and out
            _system.Step();         // lifts, doors and car move; raises Stepped
        }

        private void OnSystemStepped(object sender, EventArgs e)
        {
            for (int i = 0; i < _units.Length; i++)
            {
                Elevator lift = _system.Elevators[i];
                _units[i].Animate(lift, _passengers.OccupantOf(lift));
            }

            UpdateLobbies();
            btnCallFirst.Lit = CallPending(Floor.First);
            btnCallGround.Lit = CallPending(Floor.Ground);
        }

        /// <summary>People standing in a lobby (resting or waiting for a lift) are drawn next to the call buttons.</summary>
        private void UpdateLobbies()
        {
            var first = new List<int>();
            var ground = new List<int>();

            foreach (Passenger p in _passengers.Passengers)
            {
                if (!p.IsInLobby) continue;
                if (p.Floor == Floor.First) first.Add(p.Number - 1); else ground.Add(p.Number - 1);
            }

            lobbyFirst.SetPeople(first);
            lobbyGround.SetPeople(ground);
        }

        private void OnPassengerActivity(PassengerActivity activity)
        {
            int? liftId = activity.Lift != null ? (int?)activity.Lift.Id : null;
            _logWriter.Write(new LogEntry(liftId, (int)activity.Floor, "Passenger", activity.Text, DateTime.Now));
        }

        private void chkPassengers_CheckedChanged(object sender, EventArgs e)
        {
            _passengers.Enabled = chkPassengers.Checked;
            chkPassengers.Text = chkPassengers.Checked ? "Passengers: ON" : "Passengers: OFF";
            SetStatus(chkPassengers.Checked ? "Passengers will use the lifts." : "Passengers are paused after their current trip.");
        }

        private bool CallPending(Floor floor)
        {
            foreach (Elevator lift in _system.Elevators)
            {
                if (!lift.IsEmergency && lift.WillStopAt(floor)) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------ log window
        private void btnLog_Click(object sender, EventArgs e)
        {
            if (_logForm == null || _logForm.IsDisposed)
            {
                _logForm = new LogForm(_repository, _logWriter);
                _logForm.Show(this);
            }
            else
            {
                _logForm.Activate();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ------------------------------------------------------------------ database results (come back on the GUI thread)
        private void OnDatabaseReady()
        {
            _logWriter.Enable();
            SetStatus("Database ready on " + _settings.Server + ". Every operation is being logged.");
        }

        private void OnDatabaseError(Exception ex)
        {
            _logWriter.Disable();
            SetStatus("Database not available - the lifts still work, but nothing is saved.");

            if (_databaseErrorShown) return;
            _databaseErrorShown = true;

            MessageBox.Show(
                ex.Message +
                "\n\nThe lifts still work, but the log cannot be saved.",
                "Database problem",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void SetStatus(string text)
        {
            lblStatus.Text = text;
        }
    }
}
