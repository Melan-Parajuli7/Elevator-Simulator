using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Lift.Controller;
using Lift.Data;
using Lift.Models;
using Lift.Observer;
using Lift.Subject;

namespace Lift.Tests
{
    // ------------------------------------------------------------------ test doubles
    internal sealed class RecordingObserver : IElevatorObserver
    {
        public readonly List<ElevatorEvent> Events = new List<ElevatorEvent>();
        public void Update(ElevatorEvent e) { Events.Add(e); }
        public int Count(ElevatorEventKind kind) { return Events.Count(x => x.Kind == kind); }
        public ElevatorEvent Last { get { return Events[Events.Count - 1]; } }
    }

    internal sealed class ThrowingObserver : IElevatorObserver
    {
        public void Update(ElevatorEvent e) { throw new InvalidOperationException("broken observer"); }
    }

    internal sealed class FakeFloorView : IFloorStatusView
    {
        public ElevatorEvent Last;
        public void ShowElevator(ElevatorEvent e) { Last = e; }
    }

    internal sealed class FakePanelView : IPanelView
    {
        public ElevatorEvent Last;
        public readonly List<string> Texts = new List<string>();
        public void ShowStatus(ElevatorEvent e) { Last = e; Texts.Add(e.StatusText); }
    }

    internal sealed class FakeSound : ISoundPlayer
    {
        public readonly List<string> Calls = new List<string>();
        public void SetMoving(int id, bool moving) { Calls.Add("moving:" + id + ":" + moving); }
        public void SetEmergency(int id, bool on) { Calls.Add("emergency:" + id + ":" + on); }
        public void PlayArrival() { Calls.Add("arrival"); }
    }

    internal sealed class FakeWriter : ILogWriter
    {
        public readonly List<LogEntry> Entries = new List<LogEntry>();
        public void Write(LogEntry entry) { Entries.Add(entry); }
    }

    internal sealed class FakeRepository : IElevatorLogRepository
    {
        private readonly object _lock = new object();
        public readonly List<LogEntry> Saved = new List<LogEntry>();
        public bool Fail;
        public int SaveCalls;

        public void Save(IEnumerable<LogEntry> entries)
        {
            lock (_lock)
            {
                SaveCalls++;
                if (Fail) throw new DataAccessException("database is down", null);
                Saved.AddRange(entries);
            }
        }

        public int SavedCount { get { lock (_lock) { return Saved.Count; } } }
        public DataTable LoadHistory() { return new DataTable(); }
        public void ClearHistory() { lock (_lock) { Saved.Clear(); } }
    }

    // ------------------------------------------------------------------ mini test runner
    internal static class Program
    {
        private static int _passed;
        private static int _failed;

        private static void Check(string name, bool condition)
        {
            if (condition) { _passed++; Console.WriteLine("  PASS  " + name); }
            else { _failed++; Console.WriteLine("  FAIL  " + name); }
        }

        private static void Section(string title) { Console.WriteLine(); Console.WriteLine(title); }

        private static void Run(ElevatorSystem system, int ticks)
        {
            for (int i = 0; i < ticks; i++) system.Step();
        }

        private static bool RunUntil(ElevatorSystem system, Func<bool> done, int maxTicks)
        {
            for (int i = 0; i < maxTicks; i++)
            {
                if (done()) return true;
                system.Step();
            }
            return done();
        }

        private static bool WaitFor(Func<bool> condition, int milliseconds)
        {
            DateTime end = DateTime.Now.AddMilliseconds(milliseconds);
            while (DateTime.Now < end)
            {
                if (condition()) return true;
                Thread.Sleep(10);
            }
            return condition();
        }

        private static int Main()
        {
            Console.WriteLine("LIFT LOGIC TESTS");

            Task2_PanelAndHallRequests();
            Task2_Displays();
            Doors_And_Sensor();
            Task5_Dispatcher();
            Emergency();
            Estimates();
            Observers_And_Log();
            Passengers();
            Task5_BackgroundWorker();
            Database_Helpers();

            Console.WriteLine();
            Console.WriteLine("RESULT: " + _passed + " passed, " + _failed + " failed");
            return _failed == 0 ? 0 : 1;
        }

        // ------------------------------------------------------------------ Task 2
        private static void Task2_PanelAndHallRequests()
        {
            Section("Task 2 - requests move the lift");

            var s = new ElevatorSystem();
            Elevator a = s.ById(1);                // A starts at Ground

            Check("lift A starts at the Ground floor, doors closed", a.AtFloor == Floor.Ground && a.DoorOpenness == 0);

            a.RequestFloorFromPanel(Floor.First);
            RunUntil(s, () => a.IsMoving, 5);
            Check("panel button 1 makes the lift move", a.IsMoving && a.Direction == Direction.Up);

            RunUntil(s, () => a.AtFloor == Floor.First, 400);
            Check("lift arrives at the First floor", a.AtFloor == Floor.First);

            RunUntil(s, () => a.DoorOpenness >= 1.0, 100);
            Check("doors open after arriving", a.DoorOpenness >= 1.0);

            RunUntil(s, () => a.DoorOpenness == 0 && !a.HasPendingRequests, 400);
            Check("doors close again by themselves", a.DoorOpenness == 0.0);

            a.RequestFloorFromPanel(Floor.Ground);
            RunUntil(s, () => a.AtFloor == Floor.Ground, 500);
            Check("panel button G brings it back down", a.AtFloor == Floor.Ground);

            var s2 = new ElevatorSystem();
            Elevator a2 = s2.ById(1);
            Elevator b2 = s2.ById(2);
            a2.RequestFloorFromPanel(Floor.First);
            Run(s2, 30);
            Check("a panel request is handled only by its own lift", b2.PendingRequestCount == 0 && b2.AtFloor == Floor.First);

            var s3 = new ElevatorSystem();
            var obs = new RecordingObserver();
            s3.ById(1).Attach(obs);
            s3.ById(1).RequestFloorFromPanel((Floor)7);
            Check("an invalid floor is rejected", obs.Count(ElevatorEventKind.RequestRejected) == 1);

            var s4 = new ElevatorSystem();
            Elevator a4 = s4.ById(1);
            a4.RequestFloorFromPanel(Floor.Ground);
            RunUntil(s4, () => a4.DoorOpenness >= 1.0, 100);
            Check("pressing the button of the current floor opens the doors", a4.DoorOpenness >= 1.0);
        }

        private static void Task2_Displays()
        {
            Section("Task 2 - all display areas update");

            var s = new ElevatorSystem();
            Elevator a = s.ById(1);
            var floorView1 = new FakeFloorView();
            var floorView2 = new FakeFloorView();
            var panel = new FakePanelView();
            a.Attach(new FloorDisplayObserver(floorView1, floorView2));
            a.Attach(new ControlPanelObserver(panel));

            a.RequestFloorFromPanel(Floor.First);
            RunUntil(s, () => a.IsMoving, 5);
            Check("the panel display says MOVING TO FIRST FLOOR", panel.Texts.Contains("MOVING TO FIRST FLOOR"));
            Check("both floor displays are told the lift is moving", floorView1.Last.Moving && floorView2.Last.Moving);

            RunUntil(s, () => a.AtFloor == Floor.First, 400);
            Check("floor display 1 shows the First floor on arrival", floorView1.Last.DisplayFloor == Floor.First && floorView1.Last.AtFloor == Floor.First);
            Check("floor display 2 shows the First floor on arrival", floorView2.Last.DisplayFloor == Floor.First);
            Check("the panel display shows the First floor on arrival", panel.Last.DisplayFloor == Floor.First);
        }

        private static void Doors_And_Sensor()
        {
            Section("Doors and door sensor");

            var s = new ElevatorSystem();
            Elevator a = s.ById(1);
            var obs = new RecordingObserver();
            a.Attach(obs);

            a.PressOpen();
            RunUntil(s, () => a.DoorOpenness >= 1.0, 100);
            Check("Open button opens the doors", a.DoorOpenness >= 1.0);

            a.PressClose();
            Run(s, 5);
            Check("Close button starts closing at once", a.StateName == "Doors closing");

            a.PressOpen();
            Run(s, 3);
            Check("Open while closing re-opens the doors", a.StateName == "Doors opening");

            RunUntil(s, () => a.DoorOpenness >= 1.0, 100);
            a.PressClose();
            Run(s, 10);
            a.SetObstruction(true);
            Run(s, 3);
            Check("an object in the doorway re-opens the closing doors", obs.Count(ElevatorEventKind.ObstructionDetected) == 1);

            Run(s, 300);
            Check("doors stay open while the doorway is blocked", a.DoorOpenness >= 1.0);

            a.SetObstruction(false);
            RunUntil(s, () => a.DoorOpenness == 0.0, 300);
            Check("doors close when the doorway is clear again", a.DoorOpenness == 0.0);
        }

        // ------------------------------------------------------------------ Task 5.2
        private static void Task5_Dispatcher()
        {
            Section("Task 5.2 - two lifts, dispatcher, no disruption");

            var s = new ElevatorSystem();      // A at Ground, B at First
            HallCallResult r1 = s.Dispatcher.RequestFromHall(Floor.Ground);
            Check("call from Ground goes to A (A is there)", r1.Elevator.Name == "Elevator A");

            var s2 = new ElevatorSystem();
            HallCallResult r2 = s2.Dispatcher.RequestFromHall(Floor.First);
            Check("call from First goes to B (B is there)", r2.Elevator.Name == "Elevator B");

            var s3 = new ElevatorSystem();
            Elevator a3 = s3.ById(1);
            Elevator b3 = s3.ById(2);
            a3.RequestFloorFromPanel(Floor.First);          // A is busy going up
            RunUntil(s3, () => a3.IsMoving, 5);
            Run(s3, 20);
            HallCallResult r3 = s3.Dispatcher.RequestFromHall(Floor.Ground);
            Check("a call goes to the other lift when A is busy", r3.Elevator == b3);
            Check("the busy lift's trip is not disturbed", a3.IsMoving && a3.MovingTarget == Floor.First && !a3.WillStopAt(Floor.Ground));

            RunUntil(s3, () => a3.AtFloor == Floor.First && b3.AtFloor == Floor.Ground, 600);
            Check("both lifts finish their own jobs", a3.AtFloor == Floor.First && b3.AtFloor == Floor.Ground);

            var s4 = new ElevatorSystem();
            HallCallResult first = s4.Dispatcher.RequestFromHall(Floor.Ground);
            HallCallResult second = s4.Dispatcher.RequestFromHall(Floor.Ground);
            Check("pressing the same call button twice sends only one lift",
                  first.Accepted && second.AlreadyServed && second.Elevator == first.Elevator);

            var s5 = new ElevatorSystem();
            s5.ById(1).PressEmergency();
            HallCallResult r5 = s5.Dispatcher.RequestFromHall(Floor.Ground);
            Check("a lift in emergency never gets a call", r5.Accepted && r5.Elevator.Name == "Elevator B");

            s5.ById(2).PressEmergency();
            HallCallResult r6 = s5.Dispatcher.RequestFromHall(Floor.First);
            Check("with both lifts in emergency the call is rejected", !r6.Accepted);

            var s7 = new ElevatorSystem();
            Elevator a7 = s7.ById(1);
            Elevator b7 = s7.ById(2);
            s7.Dispatcher.RequestFromHall(Floor.Ground);                    // A answers
            a7.RequestFloorFromPanel(Floor.First);                          // a passenger in A wants 1
            HallCallResult r7 = s7.Dispatcher.RequestFromHall(Floor.First); // B is already at First
            Check("the lift with the shortest estimated response time is chosen", r7.Elevator == b7);

            var s8 = new ElevatorSystem();
            HallCallResult r8 = s8.Dispatcher.RequestFromHall(Floor.Ground);
            int events = 0;
            s8.Dispatcher.CallHandled += (sender, result) => events++;
            s8.Dispatcher.RequestFromHall(Floor.First);
            Check("the dispatcher reports every decision", events == 1 && r8.Message.Length > 0);
        }

        private static void Emergency()
        {
            Section("Emergency (SOS)");

            var s = new ElevatorSystem();
            Elevator a = s.ById(1);
            var obs = new RecordingObserver();
            a.Attach(obs);

            a.RequestFloorFromPanel(Floor.First);
            Run(s, 60);
            double stoppedAt = a.Position;
            a.PressEmergency();
            Run(s, 100);
            Check("SOS stops the lift between the floors", a.IsEmergency && Math.Abs(a.Position - stoppedAt) < 0.0001 && !a.AtFloor.HasValue);
            Check("SOS raises the emergency event", obs.Count(ElevatorEventKind.EmergencyOn) == 1);

            a.RequestFloorFromPanel(Floor.Ground);
            Check("requests are rejected during an emergency", obs.Count(ElevatorEventKind.RequestRejected) == 1);

            a.PressEmergency();
            Check("SOS again cancels the emergency", !a.IsEmergency && obs.Count(ElevatorEventKind.EmergencyOff) == 1);

            RunUntil(s, () => a.AtFloor == Floor.First, 600);
            Check("the interrupted trip is finished after the emergency", a.AtFloor == Floor.First);

            var s2 = new ElevatorSystem();
            Elevator a2 = s2.ById(1);
            a2.PressEmergency();
            a2.PressOpen();
            Run(s2, 80);
            Check("doors can still be opened in an emergency when stopped at a floor", a2.DoorOpenness >= 1.0);
        }

        private static void Estimates()
        {
            Section("Response time estimate");

            var s = new ElevatorSystem();
            Elevator a = s.ById(1);

            Check("an idle lift answers a call at its own floor at once", a.EstimateResponseTicks(Floor.Ground) == 0);
            double idleOther = a.EstimateResponseTicks(Floor.First);
            Check("an idle lift needs travel time for the other floor", idleOther > 100);

            a.RequestFloorFromPanel(Floor.First);
            Run(s, 10);
            Check("a busy lift takes longer than the same lift idle", a.EstimateResponseTicks(Floor.Ground) > idleOther);

            a.PressEmergency();
            Check("a lift in emergency has an infinite estimate", double.IsPositiveInfinity(a.EstimateResponseTicks(Floor.Ground)));
        }

        // ------------------------------------------------------------------ Observer pattern + logging
        private static void Observers_And_Log()
        {
            Section("Observer pattern and logging");

            var s = new ElevatorSystem();
            Elevator a = s.ById(1);
            var writer = new FakeWriter();
            var sound = new FakeSound();
            var good = new RecordingObserver();
            a.Attach(new ThrowingObserver());
            a.Attach(new LogObserver(writer));
            a.Attach(new SoundObserver(sound));
            a.Attach(good);

            a.RequestFloorFromPanel(Floor.First);
            RunUntil(s, () => a.AtFloor == Floor.First && a.DoorOpenness >= 1.0, 600);

            Check("a broken observer does not stop the others", good.Events.Count > 0 && writer.Entries.Count > 0);
            Check("the log gets the button press", writer.Entries.Any(e => e.EventType == "PanelButtonPressed"));
            Check("the log gets movement, arrival and doors",
                  writer.Entries.Any(e => e.EventType == "MovementStarted") &&
                  writer.Entries.Any(e => e.EventType == "FloorArrived") &&
                  writer.Entries.Any(e => e.EventType == "DoorsOpened"));
            Check("display-only events are not logged", !writer.Entries.Any(e => e.EventType == "FloorChanged" || e.EventType == "StateChanged"));
            Check("log entries carry lift and floor ids", writer.Entries.All(e => e.ElevatorId == 1 && e.FloorId.HasValue));
            Check("the sound observer plays moving and arrival sounds", sound.Calls.Contains("moving:1:True") && sound.Calls.Contains("moving:1:False") && sound.Calls.Contains("arrival"));

            a.PressEmergency();
            Check("the sound observer starts the siren", sound.Calls.Contains("emergency:1:True"));

            a.Detach(good);
            int before = good.Events.Count;
            a.PressEmergency();
            Check("a detached observer gets no more events", good.Events.Count == before);
        }


        // ------------------------------------------------------------------ passengers
        private static void Passengers()
        {
            Section("Passengers ride G to 1 and 1 to G in both lifts");

            var s = new ElevatorSystem();                 // A at Ground, B at First
            var p1 = new Passenger(1, Floor.Ground, 30);
            var p2 = new Passenger(2, Floor.First, 60);
            var sim = new PassengerSimulator(s, p1, p2);
            var texts = new List<string>();
            sim.Activity += act => texts.Add(act.Text);

            bool sharedLift = false;
            bool p1WentUp = false, p1CameDown = false, p2WentDown = false, p2CameUp = false;

            for (int i = 0; i < 4000; i++)
            {
                sim.Step();
                s.Step();

                if (p1.Lift != null && p1.Lift == p2.Lift) sharedLift = true;
                if (p1.Phase == PassengerPhase.Riding) { if (p1.Destination == Floor.First) p1WentUp = true; else p1CameDown = true; }
                if (p2.Phase == PassengerPhase.Riding) { if (p2.Destination == Floor.Ground) p2WentDown = true; else p2CameUp = true; }
            }

            Check("passenger 1 rides from G to 1", p1WentUp);
            Check("passenger 1 rides from 1 back to G", p1CameDown);
            Check("passenger 2 rides from 1 to G", p2WentDown);
            Check("passenger 2 rides from G back to 1", p2CameUp);
            Check("both passengers complete several trips", p1.Trips >= 2 && p2.Trips >= 2);
            Check("two passengers are never inside the same lift", !sharedLift);
            Check("passengers press the call button and the inside button", texts.Any(t => t.Contains("call button")) && texts.Any(t => t.Contains("pressed button")));
            Check("passengers report stepping in and out", texts.Any(t => t.Contains("steps into")) && texts.Any(t => t.Contains("steps out")));

            // ---- the SOS button stops the car that carries the passenger at once
            var s2 = new ElevatorSystem();
            var rider = new Passenger(1, Floor.Ground, 90);
            var sim2 = new PassengerSimulator(s2, rider);
            Elevator a = s2.ById(1);

            bool moving = false;
            for (int i = 0; i < 2000 && !moving; i++)
            {
                sim2.Step();
                s2.Step();
                moving = rider.Phase == PassengerPhase.Riding && a.IsMoving && a.Position > 1.3;
            }
            Check("the passenger is riding in the moving car", moving && sim2.OccupantOf(a) == rider);

            double positionAtSos = a.Position;
            a.PressEmergency();
            Check("the car stops in the same instant SOS is pressed", a.IsEmergency && !a.IsMoving);

            for (int i = 0; i < 300; i++) { sim2.Step(); s2.Step(); }
            Check("the car does not move during the emergency", Math.Abs(a.Position - positionAtSos) < 0.0000001);
            Check("the passenger stays inside the stopped car", rider.Phase == PassengerPhase.Riding && sim2.OccupantOf(a) == rider);

            a.PressEmergency();
            RunWithPassengers(s2, sim2, () => rider.Trips >= 1, 1500);
            Check("after the emergency the car carries the passenger to the destination", rider.Trips >= 1);

            // ---- passengers can be switched off
            var s3 = new ElevatorSystem();
            var idle = new Passenger(1, Floor.Ground, 30);
            var sim3 = new PassengerSimulator(s3, idle);
            sim3.Enabled = false;
            for (int i = 0; i < 600; i++) { sim3.Step(); s3.Step(); }
            Check("with passengers switched off nobody starts a trip", idle.Trips == 0 && idle.Phase == PassengerPhase.Resting);
        }

        private static void RunWithPassengers(ElevatorSystem s, PassengerSimulator sim, Func<bool> done, int maxTicks)
        {
            for (int i = 0; i < maxTicks && !done(); i++)
            {
                sim.Step();
                s.Step();
            }
        }

        // ------------------------------------------------------------------ Task 5.1
        private static void Task5_BackgroundWorker()
        {
            Section("Task 5.1 - BackgroundWorker");

            var repo = new FakeRepository();
            var writer = new BackgroundLogWriter(repo);
            writer.Write(LogEntry.ForSystem("Test", "one"));
            writer.Write(LogEntry.ForSystem("Test", "two"));
            Thread.Sleep(100);
            Check("nothing is saved before the database is ready, entries wait in the queue", repo.SavedCount == 0 && writer.QueuedCount == 2);

            writer.Enable();
            Check("queued entries are saved on a background thread", WaitFor(() => repo.SavedCount == 2, 3000));

            writer.Write(LogEntry.ForSystem("Test", "three"));
            Check("new entries are saved too", WaitFor(() => repo.SavedCount == 3, 3000));

            var failRepo = new FakeRepository { Fail = true };
            var failWriter = new BackgroundLogWriter(failRepo);
            Exception caught = null;
            failWriter.SaveFailed += ex => caught = ex;
            failWriter.Enable();
            failWriter.Write(LogEntry.ForSystem("Test", "x"));
            Check("a database error is reported, not thrown", WaitFor(() => caught != null, 3000));
            Check("entries are kept after a failed save", WaitFor(() => failWriter.QueuedCount == 1, 3000));

            string result = null;
            bool done = false;
            DbBackgroundRunner.Run<string>(() => "loaded", r => { result = r; done = true; }, ex => done = true);
            Check("the runner returns a result from the background thread", WaitFor(() => done, 3000) && result == "loaded");

            Exception runnerError = null;
            DbBackgroundRunner.Run<string>(() => { throw new InvalidOperationException("boom"); }, r => { }, ex => runnerError = ex);
            Check("the runner reports an exception to the caller", WaitFor(() => runnerError != null, 3000) && runnerError.Message == "boom");
        }

        private static void Database_Helpers()
        {
            Section("Database helpers");

            IList<string> batches = SqlScript.SplitBatches("CREATE TABLE a (x INT);\nGO\r\n  go  \nINSERT INTO a VALUES (1);\nGO");
            Check("the script splitter breaks a script at GO lines", batches.Count == 2);
            Check("the script splitter keeps the statements", batches[0].StartsWith("CREATE TABLE") && batches[1].StartsWith("INSERT"));
            Check("an empty script gives no batches", SqlScript.SplitBatches("   ").Count == 0);

            Check("floor ids match the database ids", (int)Floor.Ground == 1 && (int)Floor.First == 2);
            Check("floor labels are G and 1", Floor.Ground.Label() == "G" && Floor.First.Label() == "1");
        }
    }
}
