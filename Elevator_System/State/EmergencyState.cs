using Lift.Models;
using Lift.Subject;

namespace Lift.State
{
    /// <summary>
    /// The SOS button was pressed: the lift stops where it is and the siren sounds.
    /// Doors can still be operated when the car is standing at a floor. SOS again ends the emergency.
    /// </summary>
    public sealed class EmergencyState : ElevatorStateBase
    {
        private double _doorGoal;   // 0 = closing, 1 = opening

        public override string Name { get { return "Emergency"; } }

        public override void Enter(Elevator e)
        {
            _doorGoal = e.DoorOpenness;
            e.Raise(ElevatorEventKind.EmergencyOn,
                e.AtFloor.HasValue
                    ? "EMERGENCY pressed in " + e.Name + " - siren ON"
                    : "EMERGENCY pressed in " + e.Name + " - siren ON, lift halted between floors");
        }

        public override void Tick(Elevator e)
        {
            if (!e.AtFloor.HasValue) return;                // between floors the doors never move

            double step = SimulationSettings.DoorFractionPerTick;
            if (e.DoorOpenness < _doorGoal) e.DoorOpenness = System.Math.Min(_doorGoal, e.DoorOpenness + step);
            else if (e.DoorOpenness > _doorGoal) e.DoorOpenness = System.Math.Max(_doorGoal, e.DoorOpenness - step);
        }

        public override void OnOpenPressed(Elevator e)
        {
            if (e.AtFloor.HasValue) _doorGoal = 1.0;
        }

        public override void OnClosePressed(Elevator e)
        {
            if (e.AtFloor.HasValue) _doorGoal = 0.0;
        }

        /// <summary>SOS pressed a second time: cancel the emergency and carry on.</summary>
        public override void OnEmergencyPressed(Elevator e)
        {
            e.Raise(ElevatorEventKind.EmergencyOff, "Emergency cancelled in " + e.Name + " - siren OFF");
            e.ChangeState(e.StateAfterEmergency());
        }

        public override string StatusText(Elevator e)
        {
            return e.AtFloor.HasValue ? "EMERGENCY - SIREN ON" : "EMERGENCY - LIFT HALTED";
        }

        public override double RemainingTicks(Elevator e)
        {
            return double.PositiveInfinity;
        }
    }
}
