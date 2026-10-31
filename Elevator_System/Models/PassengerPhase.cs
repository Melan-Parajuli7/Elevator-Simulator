namespace Lift.Models
{
    /// <summary>Where a passenger is in the trip "wait, step in, ride, step out, rest".</summary>
    public enum PassengerPhase
    {
        Resting,         // standing in the lobby, pausing before the next trip
        WaitingForLift,  // call button pressed, waiting for a lift to open its doors
        Boarding,        // walking into the car
        Riding,          // inside the car (the car carries the passenger)
        Exiting          // walking out of the car
    }
}
