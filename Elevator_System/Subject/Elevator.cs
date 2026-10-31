using System;
using System.Collections.Generic;
using Lift.Models;
using Lift.Observer;
using Lift.State;

namespace Lift.Subject
{
    /// <summary>
    /// One lift. It plays two roles at the same time:
    ///   * the SUBJECT of the Observer pattern - it tells its observers what it does, and
    ///   * the CONTEXT of the State pattern - it hands every action to its current state object.
    /// It contains no GUI code and no database code.
    /// </summary>
    public class Elevator : IElevatorSubject
    {
        private sealed class Request
        {
            public Request(Floor floor, bool fromHall)
            {
                Floor = floor;
                FromHall = fromHall;
            }

            public Floor Floor { get; private set; }
            public bool FromHall { get; private set; }
        }

        private readonly List<IElevatorObserver> _observers = new List<IElevatorObserver>();
        private readonly List<Request> _requests = new List<Request>();   // first come, first served
        private IElevatorState _state;
        private Floor _shownFloor;

        public Elevator(int id, string name, Floor startFloor)
        {
            if (!FloorExtensions.IsValid((int)startFloor))
            {
                throw new ArgumentOutOfRangeException("startFloor");
            }

            Id = id;
            Name = name;
            Position = (int)startFloor;
            _shownFloor = startFloor;
            _state = new IdleState();
            _state.Enter(this);
        }

        // ------------------------------------------------------------------ status (read by observers and the GUI)
        public int Id { get; private set; }
        public string Name { get; private set; }

        /// <summary>1.0 = Ground floor, 2.0 = First floor, 1.5 = half way between.</summary>
        public double Position { get; internal set; }

        /// <summary>0 = doors closed, 1 = doors fully open.</summary>
        public double DoorOpenness { get; internal set; }

        public Direction Direction { get; internal set; }
        public Floor? MovingTarget { get; internal set; }
        public bool ObstructionPresent { get; private set; }

        public bool IsEmergency { get { return _state is EmergencyState; } }
        public bool IsMoving { get { return _state is MovingState; } }
        public string StateName { get { return _state.Name; } }
        public string StatusText { get { return _state.StatusText(this); } }
        public bool HasPendingRequests { get { return _requests.Count > 0; } }
        public int PendingRequestCount { get { return _requests.Count; } }

        /// <summary>The floor the car is nearest to.</summary>
        public Floor CurrentFloor
        {
            get
            {
                int nearest = (int)Math.Round(Position);
                nearest = Math.Max((int)FloorExtensions.Lowest, Math.Min((int)FloorExtensions.Highest, nearest));
                return (Floor)nearest;
            }
        }

        /// <summary>The floor when the car stands exactly at one, otherwise null (between floors).</summary>
        public Floor? AtFloor
        {
            get
            {
                bool exact = Math.Abs(Position - Math.Round(Position)) < SimulationSettings.Epsilon;
                return exact ? (Floor?)CurrentFloor : null;
            }
        }

        /// <summary>True when the car stands at a floor with its doors open (or opening) wide enough to walk through.</summary>
        public bool DoorsOpenForPassengers
        {
            get
            {
                bool doorsOpening = _state is DoorsOpenState || _state is DoorsOpeningState;
                return doorsOpening && AtFloor.HasValue && !IsEmergency && DoorOpenness >= 0.6;
            }
        }

        /// <summary>True if this lift is already going to, or has queued, a stop at that floor.</summary>
        public bool WillStopAt(Floor floor)
        {
            if (MovingTarget.HasValue && MovingTarget.Value == floor) return true;
            foreach (Request r in _requests)
            {
                if (r.Floor == floor) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------ Subject (Observer pattern)
        public void Attach(IElevatorObserver observer)
        {
            if (!_observers.Contains(observer)) _observers.Add(observer);
        }

        public void Detach(IElevatorObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(ElevatorEvent e)
        {
            foreach (IElevatorObserver observer in _observers.ToArray())
            {
                try
                {
                    observer.Update(e);
                }
                catch (Exception ex)
                {
                    // one broken observer must never stop the lift or the other observers
                    System.Diagnostics.Debug.WriteLine("Observer " + observer.GetType().Name + " failed: " + ex.Message);
                }
            }
        }

        internal void Raise(ElevatorEventKind kind, string message, Floor? relatedFloor = null)
        {
            Notify(BuildEvent(kind, message, relatedFloor));
        }

        private ElevatorEvent BuildEvent(ElevatorEventKind kind, string message, Floor? relatedFloor)
        {
            var e = new ElevatorEvent(Id, Name, kind, message, DateTime.Now);
            e.DisplayFloor = CurrentFloor;
            e.AtFloor = AtFloor;
            e.Direction = Direction;
            e.Moving = IsMoving;
            e.Emergency = IsEmergency;
            e.TargetFloor = MovingTarget;
            if (!e.TargetFloor.HasValue && _requests.Count > 0) e.TargetFloor = _requests[0].Floor;
            e.RelatedFloor = relatedFloor;
            e.StateName = _state.Name;
            e.StatusText = _state.StatusText(this);
            e.PendingGround = WillStopAt(Floor.Ground);
            e.PendingFirst = WillStopAt(Floor.First);
            return e;
        }

        /// <summary>Sends the current status to all observers, so freshly attached displays show the right values.</summary>
        public void PublishStatus()
        {
            Raise(ElevatorEventKind.StateChanged, _state.Name);
        }

        // ------------------------------------------------------------------ actions (all handed to the current state)
        /// <summary>Called by the animation timer: lets the current state do one step.</summary>
        public void Tick()
        {
            _state.Tick(this);
        }

        /// <summary>A floor button INSIDE this lift was pressed. Only this lift handles it.</summary>
        public bool RequestFloorFromPanel(Floor floor)
        {
            return AddRequest(floor, false, ElevatorEventKind.PanelButtonPressed,
                string.Format("Button {0} pressed inside {1}", floor.Label(), Name));
        }

        /// <summary>A request from OUTSIDE. Only the dispatcher calls this.</summary>
        public bool AcceptHallCall(Floor floor)
        {
            return AddRequest(floor, true, ElevatorEventKind.HallCallAssigned,
                string.Format("Call from {0} assigned to {1}", floor.DisplayName(), Name));
        }

        public void PressOpen()
        {
            _state.OnOpenPressed(this);
        }

        public void PressClose()
        {
            _state.OnClosePressed(this);
        }

        /// <summary>The SOS button: starts the emergency, or ends it when pressed again.</summary>
        public void PressEmergency()
        {
            _state.OnEmergencyPressed(this);
        }

        /// <summary>Door sensor: true while an object is in the doorway.</summary>
        public void SetObstruction(bool present)
        {
            ObstructionPresent = present;
        }

        // ------------------------------------------------------------------ response time estimate (used by the dispatcher)
        /// <summary>
        /// How many ticks until this lift could answer a call at the given floor.
        /// The lift first finishes what it is doing and everything already queued, then goes to the new floor,
        /// so the estimate also shows that adding the call will not disturb the work in progress.
        /// </summary>
        public double EstimateResponseTicks(Floor floor)
        {
            if (IsEmergency) return double.PositiveInfinity;

            // doors already open (or moving) at that floor: the call is answered at once
            bool doorsActiveHere = _state is DoorsOpenState || _state is DoorsOpeningState || _state is DoorsClosingState;
            if (doorsActiveHere && AtFloor.HasValue && AtFloor.Value == floor) return 0;

            var plan = new List<Floor>();
            if (MovingTarget.HasValue) plan.Add(MovingTarget.Value);
            foreach (Request r in _requests)
            {
                if (!plan.Contains(r.Floor)) plan.Add(r.Floor);
            }
            if (!plan.Contains(floor)) plan.Add(floor);

            double ticks = _state.RemainingTicks(this);
            double position = Position;

            foreach (Floor stop in plan)
            {
                ticks += Math.Abs((int)stop - position) / SimulationSettings.FloorsPerTick;
                position = (int)stop;
                if (stop == floor) return ticks;
                ticks += SimulationSettings.DoorCycleTicks;      // the stop before ours takes a door cycle
            }

            return ticks;
        }

        // ------------------------------------------------------------------ helpers used by the State classes
        internal void ChangeState(IElevatorState state)
        {
            _state = state;
            state.Enter(this);
            Raise(ElevatorEventKind.StateChanged, state.Name);
        }

        internal bool TryPeekRequest(out Floor floor)
        {
            if (_requests.Count == 0)
            {
                floor = Floor.Ground;
                return false;
            }

            floor = _requests[0].Floor;
            return true;
        }

        internal void CompleteRequestsAt(Floor floor)
        {
            _requests.RemoveAll(r => r.Floor == floor);
        }

        /// <summary>Moves the car and tells the displays when the nearest floor changes.</summary>
        internal void MoveTo(double position)
        {
            Position = Math.Max((int)FloorExtensions.Lowest, Math.Min((int)FloorExtensions.Highest, position));

            if (CurrentFloor != _shownFloor)
            {
                _shownFloor = CurrentFloor;
                Raise(ElevatorEventKind.FloorChanged, Name + " is now at " + _shownFloor.DisplayName());
            }
        }

        /// <summary>Which state the lift returns to when the emergency is cancelled.</summary>
        internal IElevatorState StateAfterEmergency()
        {
            double eps = SimulationSettings.Epsilon;
            if (DoorOpenness >= 1.0 - eps) return new DoorsOpenState();
            if (DoorOpenness > eps) return new DoorsClosingState();
            if (AtFloor.HasValue) return new IdleState();

            // stopped between floors: finish the trip that was interrupted
            return new MovingState(MovingTarget ?? CurrentFloor);
        }

        private bool AddRequest(Floor floor, bool fromHall, ElevatorEventKind kind, string message)
        {
            if (!FloorExtensions.IsValid((int)floor))
            {
                Raise(ElevatorEventKind.RequestRejected, "Invalid floor requested - ignored");
                return false;
            }

            if (IsEmergency)
            {
                Raise(ElevatorEventKind.RequestRejected,
                    string.Format("{0} is in EMERGENCY - request for {1} ignored", Name, floor.DisplayName()), floor);
                return false;
            }

            if (WillStopAt(floor))
            {
                return true;     // already on its way or queued: pressing a lit button again changes nothing
            }

            _requests.Add(new Request(floor, fromHall));
            Raise(kind, message, floor);
            _state.OnRequestAdded(this, floor);
            return true;
        }
    }
}
