using Lift.Models;
using Lift.Subject;

namespace Lift.Controller
{
    /// <summary>One simulated person who keeps using the lifts: Ground to First, then First to Ground, and so on.</summary>
    public sealed class Passenger
    {
        public Passenger(int number, Floor startFloor, int startDelayTicks)
        {
            Number = number;
            Floor = startFloor;
            Destination = startFloor.Other();
            Phase = PassengerPhase.Resting;
            Ticks = PassengerSimulator.RestTicks - startDelayTicks;
        }

        public int Number { get; private set; }
        public string Name { get { return "Passenger " + Number; } }

        /// <summary>The floor the passenger is standing at (or is stepping out at).</summary>
        public Floor Floor { get; internal set; }

        public Floor Destination { get; internal set; }
        public PassengerPhase Phase { get; internal set; }

        /// <summary>0..1 while walking into or out of the car.</summary>
        public double Progress { get; internal set; }

        /// <summary>The lift the passenger is boarding, riding in or leaving. Null while in the lobby.</summary>
        public Elevator Lift { get; internal set; }

        /// <summary>Completed trips (a trip ends when the passenger has stepped out).</summary>
        public int Trips { get; internal set; }

        internal int Ticks;

        public bool IsInCar
        {
            get { return Phase == PassengerPhase.Riding; }
        }

        public bool IsInLobby
        {
            get { return Phase == PassengerPhase.Resting || Phase == PassengerPhase.WaitingForLift; }
        }
    }
}
