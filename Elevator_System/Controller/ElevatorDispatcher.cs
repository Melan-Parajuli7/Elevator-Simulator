using System;
using System.Collections.Generic;
using Lift.Models;
using Lift.Subject;

namespace Lift.Controller
{
    /// <summary>
    /// Decides which lift answers a request from OUTSIDE (the hall buttons).
    /// Rule: the lift with the shortest estimated response time. A lift only ADDS the floor to its own
    /// queue, so a trip in progress is never cut short. Buttons inside a lift never come here.
    /// </summary>
    public sealed class ElevatorDispatcher
    {
        private readonly IList<Elevator> _elevators;

        public ElevatorDispatcher(IList<Elevator> elevators)
        {
            if (elevators == null || elevators.Count == 0)
            {
                throw new ArgumentException("The dispatcher needs at least one lift.", "elevators");
            }
            _elevators = elevators;
        }

        /// <summary>Raised after every decision so the GUI and the log can report it.</summary>
        public event EventHandler<HallCallResult> CallHandled;

        public HallCallResult RequestFromHall(Floor floor)
        {
            HallCallResult result = Decide(floor);

            EventHandler<HallCallResult> handler = CallHandled;
            if (handler != null) handler(this, result);

            return result;
        }

        private HallCallResult Decide(Floor floor)
        {
            // Ask every lift how long it would need and pick the quickest.
            // A lift in emergency answers "infinity", so it is never chosen.
            Elevator best = null;
            double bestTicks = double.PositiveInfinity;

            foreach (Elevator lift in _elevators)
            {
                double ticks = lift.EstimateResponseTicks(floor);
                if (IsBetter(lift, ticks, best, bestTicks, floor))
                {
                    best = lift;
                    bestTicks = ticks;
                }
            }

            if (best == null)
            {
                return new HallCallResult(floor, null, false, 0,
                    string.Format("Call from {0} rejected: no lift is available", floor.DisplayName()));
            }

            double seconds = SimulationSettings.TicksToSeconds(bestTicks);

            // The quickest lift is already going to that floor: nothing to add, and no second lift is sent.
            if (best.WillStopAt(floor))
            {
                return new HallCallResult(floor, best, true, seconds,
                    string.Format("Call from {0}: {1} is already on its way", floor.DisplayName(), best.Name));
            }

            best.AcceptHallCall(floor);
            return new HallCallResult(floor, best, false, seconds,
                string.Format("Call from {0} given to {1} (about {2:0.0} s)", floor.DisplayName(), best.Name, seconds));
        }

        // Shorter estimate wins. On a tie: a lift that already stops at that floor wins (no duplicate trip),
        // then the lift with fewer waiting requests, then the first lift in the list.
        private static bool IsBetter(Elevator candidate, double ticks, Elevator best, double bestTicks, Floor floor)
        {
            if (double.IsPositiveInfinity(ticks)) return false;      // lift in emergency
            if (best == null) return true;
            if (ticks < bestTicks - SimulationSettings.Epsilon) return true;

            if (Math.Abs(ticks - bestTicks) <= SimulationSettings.Epsilon)
            {
                bool candidateHasIt = candidate.WillStopAt(floor);
                bool bestHasIt = best.WillStopAt(floor);
                if (candidateHasIt != bestHasIt) return candidateHasIt;
                return candidate.PendingRequestCount < best.PendingRequestCount;
            }
            return false;
        }
    }
}
