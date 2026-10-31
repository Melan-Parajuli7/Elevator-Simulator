using System;
using System.Collections.Generic;
using Lift.Models;
using Lift.Subject;

namespace Lift.Controller
{
    /// <summary>
    /// Moves the simulated passengers. A passenger uses the lifts only through the same buttons a real person has:
    /// the call button outside (through the dispatcher) and the floor button inside the car it boarded.
    /// The car carries the passenger, so when the SOS button stops the car, the passenger stops with it.
    /// </summary>
    public sealed class PassengerSimulator
    {
        public const int WalkTicks = 40;      // 1.2 s to step into or out of the car
        public const int RestTicks = 90;      // pause in the lobby between two trips
        public const int RecallTicks = 150;   // press the call button again if no lift came

        private readonly ElevatorSystem _system;
        private readonly List<Passenger> _passengers = new List<Passenger>();

        public PassengerSimulator(ElevatorSystem system, params Passenger[] passengers)
        {
            _system = system;
            _passengers.AddRange(passengers);
            Enabled = true;
        }

        /// <summary>When false no NEW trip starts; a trip in progress is finished.</summary>
        public bool Enabled { get; set; }

        public IList<Passenger> Passengers { get { return _passengers; } }

        /// <summary>Raised for every important step, so it can be logged.</summary>
        public event Action<PassengerActivity> Activity;

        /// <summary>The passenger that is boarding, riding in or leaving the given lift (or null).</summary>
        public Passenger OccupantOf(Elevator lift)
        {
            foreach (Passenger p in _passengers)
            {
                if (p.Lift == lift) return p;
            }
            return null;
        }

        /// <summary>One animation tick. Call it before ElevatorSystem.Step().</summary>
        public void Step()
        {
            foreach (Passenger p in _passengers)
            {
                switch (p.Phase)
                {
                    case PassengerPhase.Resting: StepResting(p); break;
                    case PassengerPhase.WaitingForLift: StepWaiting(p); break;
                    case PassengerPhase.Boarding: StepBoarding(p); break;
                    case PassengerPhase.Riding: StepRiding(p); break;
                    case PassengerPhase.Exiting: StepExiting(p); break;
                }
            }
        }

        // ------------------------------------------------------------------ phases
        private void StepResting(Passenger p)
        {
            if (!Enabled) return;

            p.Ticks++;
            if (p.Ticks >= RestTicks)
            {
                p.Phase = PassengerPhase.WaitingForLift;
                p.Ticks = 0;
                PressCallButton(p);
            }
        }

        private void StepWaiting(Passenger p)
        {
            p.Ticks++;

            foreach (Elevator lift in _system.Elevators)
            {
                bool free = OccupantOf(lift) == null;
                if (free && lift.DoorsOpenForPassengers && lift.AtFloor.Value == p.Floor)
                {
                    p.Lift = lift;
                    p.Phase = PassengerPhase.Boarding;
                    p.Progress = 0;
                    Report(p, "{0} steps into {1} at {2}", p.Name, lift.Name, p.Floor.DisplayName());
                    return;
                }
            }

            if (p.Ticks % RecallTicks == 0) PressCallButton(p);     // nobody came: press again
        }

        private void StepBoarding(Passenger p)
        {
            Elevator lift = p.Lift;
            if (lift.IsEmergency) return;           // frozen in the doorway until the emergency ends

            lift.PressOpen();                       // the doors wait while somebody walks through
            p.Progress += 1.0 / WalkTicks;

            if (p.Progress >= 1.0)
            {
                p.Progress = 1.0;
                p.Phase = PassengerPhase.Riding;
                p.Destination = p.Floor.Other();
                lift.RequestFloorFromPanel(p.Destination);      // the passenger presses the floor button inside
                Report(p, "{0} is inside {1} and pressed button {2}", p.Name, lift.Name, p.Destination.Label());
            }
        }

        private void StepRiding(Passenger p)
        {
            // nothing to do while the car moves; the passenger steps out when the doors open at the destination
            Elevator lift = p.Lift;
            if (lift.DoorsOpenForPassengers && lift.AtFloor.Value == p.Destination)
            {
                p.Floor = p.Destination;
                p.Phase = PassengerPhase.Exiting;
                p.Progress = 0;
                Report(p, "{0} arrived at {1} and steps out of {2}", p.Name, p.Floor.DisplayName(), lift.Name);
            }
        }

        private void StepExiting(Passenger p)
        {
            Elevator lift = p.Lift;
            if (lift.IsEmergency) return;

            lift.PressOpen();
            p.Progress += 1.0 / WalkTicks;

            if (p.Progress >= 1.0)
            {
                Report(p, "{0} left {1} at {2}", p.Name, lift.Name, p.Floor.DisplayName());
                p.Lift = null;
                p.Phase = PassengerPhase.Resting;
                p.Ticks = 0;
                p.Progress = 0;
                p.Trips++;
            }
        }

        private void PressCallButton(Passenger p)
        {
            Report(p, "{0} presses the call button at {1}", p.Name, p.Floor.DisplayName());
            _system.Dispatcher.RequestFromHall(p.Floor);
        }

        private void Report(Passenger p, string format, params object[] args)
        {
            Action<PassengerActivity> handler = Activity;
            if (handler != null)
            {
                handler(new PassengerActivity(p, p.Lift, p.Floor, string.Format(format, args)));
            }
        }
    }
}
