using System;
using System.Collections.Generic;
using Lift.Models;
using Lift.Subject;

namespace Lift.Controller
{
    /// <summary>
    /// Facade for the whole lift installation: the two lifts, the dispatcher and the step method.
    /// The GUI talks to this class only, and the console tests can use it without any screen.
    /// </summary>
    public sealed class ElevatorSystem
    {
        private readonly List<Elevator> _elevators = new List<Elevator>();

        public ElevatorSystem()
        {
            _elevators.Add(new Elevator(1, "Elevator A", Floor.Ground));
            _elevators.Add(new Elevator(2, "Elevator B", Floor.First));
            Dispatcher = new ElevatorDispatcher(_elevators);
        }

        public IList<Elevator> Elevators { get { return _elevators; } }
        public ElevatorDispatcher Dispatcher { get; private set; }

        /// <summary>Delegate event raised after every step: the GUI redraws the animation here.</summary>
        public event EventHandler Stepped;

        /// <summary>One animation tick for every lift. Called by the timer.</summary>
        public void Step()
        {
            foreach (Elevator lift in _elevators)
            {
                lift.Tick();
            }

            EventHandler handler = Stepped;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public Elevator ById(int id)
        {
            foreach (Elevator lift in _elevators)
            {
                if (lift.Id == id) return lift;
            }
            throw new ArgumentException("There is no lift with id " + id, "id");
        }
    }
}
