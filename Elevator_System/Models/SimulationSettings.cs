namespace Lift.Models
{
    /// <summary>
    /// Every timing value of the simulation in one place.
    /// One "tick" is one tick of the animation timer, so all speeds are "per tick".
    /// </summary>
    public static class SimulationSettings
    {
        public const int TickIntervalMs = 30;

        public const double FloorsPerTick = 0.0075;       // about 4.0 seconds between the two floors
        public const double DoorFractionPerTick = 0.02;   // doors need 50 ticks (1.5 s) to open or close
        public const int HoldOpenTicks = 150;             // doors stay open about 4.5 seconds
        public const int HoldOpenWhenCalledTicks = 50;    // ... or 1.5 s if somebody is waiting elsewhere
        public const int HoldAfterObstructionTicks = 50;  // doors wait 1.5 s after the doorway is clear

        public const double Epsilon = 0.000001;

        /// <summary>Ticks for a complete door cycle: open, hold, close.</summary>
        public static double DoorCycleTicks
        {
            get { return 1 / DoorFractionPerTick + HoldOpenTicks + 1 / DoorFractionPerTick; }
        }

        public static double TicksToSeconds(double ticks)
        {
            return ticks * TickIntervalMs / 1000.0;
        }
    }
}
