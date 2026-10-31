using Lift.Models;

namespace Lift.Observer
{
    /// <summary>Turns lift events into sound: moving hum, arrival ding and the emergency siren.</summary>
    public sealed class SoundObserver : IElevatorObserver
    {
        private readonly ISoundPlayer _player;

        public SoundObserver(ISoundPlayer player)
        {
            _player = player;
        }

        public void Update(ElevatorEvent e)
        {
            switch (e.Kind)
            {
                case ElevatorEventKind.MovementStarted:
                    _player.SetMoving(e.ElevatorId, true);
                    break;

                case ElevatorEventKind.FloorArrived:
                    _player.SetMoving(e.ElevatorId, false);
                    _player.PlayArrival();
                    break;

                case ElevatorEventKind.EmergencyOn:
                    _player.SetEmergency(e.ElevatorId, true);
                    break;

                case ElevatorEventKind.EmergencyOff:
                    _player.SetEmergency(e.ElevatorId, false);
                    break;
            }
        }
    }
}
