using System;
using System.ComponentModel;
using System.Windows.Forms;
using Lift.Controller;
using Lift.Models;
using Lift.Observer;
using Lift.Subject;
using Lift.UI.Controls;

namespace Lift.UI
{
    /// <summary>
    /// Everything the user sees of ONE lift: the First floor door and the Ground floor door (each with its
    /// floor status display) and the lift's own control panel. The main window shows two of these side by side.
    /// It reports button presses through delegates and receives its displays through the Observer interfaces.
    /// </summary>
    public partial class ElevatorUnitView : UserControl, IPanelView
    {
        public ElevatorUnitView()
        {
            InitializeComponent();
        }

        // ---- delegates the form subscribes to (events of the buttons INSIDE this lift) ----
        public event Action<Floor> FloorButtonPressed;
        public event Action OpenButtonPressed;
        public event Action CloseButtonPressed;
        public event Action EmergencyButtonPressed;

        /// <summary>The two floor status displays of this lift.</summary>
        public IFloorStatusView FirstFloorDisplay { get { return doorFirst; } }
        public IFloorStatusView GroundFloorDisplay { get { return doorGround; } }

        /// <summary>Control panel display window (Observer pattern): LED screen and button lights.</summary>
        public void ShowStatus(ElevatorEvent e)
        {
            ledDisplay.FloorText = e.DisplayFloor.Label();
            ledDisplay.StatusMessage = e.StatusText;
            ledDisplay.Direction = e.Direction;
            ledDisplay.Moving = e.Moving;
            ledDisplay.Emergency = e.Emergency;

            btnFloor1.Lit = e.PendingFirst;
            btnFloorG.Lit = e.PendingGround;
            btnEmergency.Lit = e.Emergency;
        }

        /// <summary>
        /// Called on every animation tick: slides the doors, moves the car in the shaft and shows the passenger.
        /// </summary>
        public void Animate(Elevator lift, Passenger occupant)
        {
            Floor? at = lift.AtFloor;
            doorFirst.OpenAmount = at.HasValue && at.Value == Floor.First ? lift.DoorOpenness : 0;
            doorGround.OpenAmount = at.HasValue && at.Value == Floor.Ground ? lift.DoorOpenness : 0;

            // the moving car: it is drawn exactly where the lift is, so it stops the moment the lift stops
            shaft.Position = lift.Position;
            shaft.Moving = lift.IsMoving;
            shaft.Emergency = lift.IsEmergency;
            shaft.PassengerColor = occupant != null && occupant.Phase == PassengerPhase.Riding ? occupant.Number - 1 : -1;

            ShowPassengerAtDoors(at, occupant);

            // blinking lamps, moving arrows and the flashing emergency need a repaint even when no value changed
            if (lift.IsMoving || lift.IsEmergency || lift.HasPendingRequests)
            {
                doorFirst.Invalidate();
                doorGround.Invalidate();
                ledDisplay.Invalidate();
                btnEmergency.Invalidate();
            }
        }

        private void ShowPassengerAtDoors(Floor? at, Passenger occupant)
        {
            PassengerMode first = PassengerMode.None;
            PassengerMode ground = PassengerMode.None;
            double progress = 0;
            int color = 0;

            if (occupant != null && at.HasValue)
            {
                color = occupant.Number - 1;
                progress = occupant.Progress;

                PassengerMode mode = PassengerMode.InCabin;
                if (occupant.Phase == PassengerPhase.Boarding) mode = PassengerMode.Entering;
                else if (occupant.Phase == PassengerPhase.Exiting) mode = PassengerMode.Leaving;

                if (at.Value == Floor.First) first = mode; else ground = mode;
            }

            doorFirst.SetPassenger(first, progress, color);
            doorGround.SetPassenger(ground, progress, color);
        }

        /// <summary>Door sensor: is the mouse pointer in the doorway of the door where the lift stands?</summary>
        public bool IsPointerInDoorway(Elevator lift)
        {
            Floor? at = lift.AtFloor;
            if (!at.HasValue) return false;
            DoorControl door = at.Value == Floor.First ? doorFirst : doorGround;
            return door.IsCursorInDoorway;
        }

        // ---- button handlers: they only forward the press (no logic here) ----
        private void btnFloor1_Click(object sender, EventArgs e)
        {
            Action<Floor> handler = FloorButtonPressed;
            if (handler != null) handler(Floor.First);
        }

        private void btnFloorG_Click(object sender, EventArgs e)
        {
            Action<Floor> handler = FloorButtonPressed;
            if (handler != null) handler(Floor.Ground);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            Action handler = OpenButtonPressed;
            if (handler != null) handler();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Action handler = CloseButtonPressed;
            if (handler != null) handler();
        }

        private void btnEmergency_Click(object sender, EventArgs e)
        {
            Action handler = EmergencyButtonPressed;
            if (handler != null) handler();
        }
    }
}
