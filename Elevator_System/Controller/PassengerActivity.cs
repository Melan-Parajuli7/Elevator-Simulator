using Lift.Models;
using Lift.Subject;

namespace Lift.Controller
{
    /// <summary>Something a passenger did. The GUI writes it to the log.</summary>
    public sealed class PassengerActivity
    {
        public PassengerActivity(Passenger passenger, Elevator lift, Floor floor, string text)
        {
            Passenger = passenger;
            Lift = lift;
            Floor = floor;
            Text = text;
        }

        public Passenger Passenger { get; private set; }
        public Elevator Lift { get; private set; }      // may be null (still in the lobby)
        public Floor Floor { get; private set; }
        public string Text { get; private set; }
    }
}
