using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lift.Data
{
    /// <summary>
    /// Saves log entries on a BackgroundWorker thread, so the lifts and the screen never wait for the database.
    /// Entries are queued in memory first; the worker saves them in batches. Saving starts when the
    /// database is ready (Enable) and the queue is kept if the database is not reachable yet.
    /// </summary>
    public sealed class BackgroundLogWriter : ILogWriter, IDisposable
    {
        private readonly IElevatorLogRepository _repository;
        private readonly Queue<LogEntry> _queue = new Queue<LogEntry>();
        private readonly object _lock = new object();
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private bool _enabled;

        public BackgroundLogWriter(IElevatorLogRepository repository)
        {
            _repository = repository;
            _worker.DoWork += OnDoWork;
            _worker.RunWorkerCompleted += OnCompleted;
        }

        /// <summary>Raised (on the GUI thread) when a batch could not be saved.</summary>
        public event Action<Exception> SaveFailed;

        /// <summary>Raised (on the GUI thread) after a batch was saved. The argument is the number of rows.</summary>
        public event Action<int> BatchSaved;

        public int QueuedCount
        {
            get { lock (_lock) { return _queue.Count; } }
        }

        public void Write(LogEntry entry)
        {
            if (entry == null) return;

            lock (_lock)
            {
                _queue.Enqueue(entry);
            }
            StartWorkerIfNeeded();
        }

        /// <summary>Call when the database is ready: queued entries are saved from now on.</summary>
        public void Enable()
        {
            lock (_lock) { _enabled = true; }
            StartWorkerIfNeeded();
        }

        /// <summary>Stops saving (for example after a database error). Entries stay in the queue.</summary>
        public void Disable()
        {
            lock (_lock) { _enabled = false; }
        }

        private void StartWorkerIfNeeded()
        {
            lock (_lock)
            {
                if (_enabled && _queue.Count > 0 && !_worker.IsBusy)
                {
                    _worker.RunWorkerAsync();
                }
            }
        }

        // ---- runs on the worker thread ----
        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            var batch = new List<LogEntry>();
            lock (_lock)
            {
                while (_queue.Count > 0 && batch.Count < 100)
                {
                    batch.Add(_queue.Dequeue());
                }
            }

            if (batch.Count == 0)
            {
                e.Result = 0;
                return;
            }

            try
            {
                _repository.Save(batch);
                e.Result = batch.Count;
            }
            catch
            {
                // put the entries back at the front so nothing is lost, then report the error
                lock (_lock)
                {
                    var rest = new Queue<LogEntry>(_queue);
                    _queue.Clear();
                    foreach (LogEntry entry in batch) _queue.Enqueue(entry);
                    foreach (LogEntry entry in rest) _queue.Enqueue(entry);
                }
                throw;
            }
        }

        // ---- runs on the thread that created the writer (the GUI thread) ----
        private void OnCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Disable();                       // do not retry in a loop; the form decides what to do
                Action<Exception> failed = SaveFailed;
                if (failed != null) failed(e.Error);
                return;
            }

            int saved = (int)e.Result;
            Action<int> done = BatchSaved;
            if (done != null && saved > 0) done(saved);

            StartWorkerIfNeeded();               // more entries arrived while this batch was saving
        }

        public void Dispose()
        {
            _worker.Dispose();
        }
    }
}
