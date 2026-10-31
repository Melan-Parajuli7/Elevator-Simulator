namespace Lift.Models
{
    /// <summary>What happened to a lift. Observers decide for themselves which kinds they care about.</summary>
    public enum ElevatorEventKind
    {
        PanelButtonPressed,   // a floor button INSIDE the lift was pressed
        HallCallAssigned,     // the dispatcher gave an OUTSIDE request to this lift
        RequestRejected,      // request ignored (emergency, invalid floor)
        MovementStarted,
        FloorChanged,         // display only: the car passed the half-way point to another floor
        FloorArrived,
        DoorsOpened,
        DoorsClosed,
        ObstructionDetected,  // object in the doorway while the doors were closing
        EmergencyOn,
        EmergencyOff,
        StateChanged          // display only: the lift entered a new State object
    }
}
