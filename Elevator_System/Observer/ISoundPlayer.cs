namespace Lift.Observer
{
    /// <summary>Plays the lift sounds. Several lifts share one speaker, so the player knows the lift id.</summary>
    public interface ISoundPlayer
    {
        void SetMoving(int elevatorId, bool moving);
        void SetEmergency(int elevatorId, bool emergency);
        void PlayArrival();
    }
}
