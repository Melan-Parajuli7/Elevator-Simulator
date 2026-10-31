using System;
using System.ComponentModel;

namespace Lift.Data
{
    /// <summary>
    /// Runs one database job on a BackgroundWorker and reports the result back on the GUI thread
    /// (success or error), so the screen never freezes while the database works.
    /// </summary>
    public static class DbBackgroundRunner
    {
        public static void Run<T>(Func<T> work, Action<T> onSuccess, Action<Exception> onError)
        {
            var worker = new BackgroundWorker();

            worker.DoWork += (sender, e) => { e.Result = work(); };

            worker.RunWorkerCompleted += (sender, e) =>
            {
                worker.Dispose();

                if (e.Error != null)
                {
                    if (onError != null) onError(e.Error);
                }
                else if (onSuccess != null)
                {
                    onSuccess((T)e.Result);
                }
            };

            worker.RunWorkerAsync();
        }

        /// <summary>The same for a job that returns nothing.</summary>
        public static void Run(Action work, Action onSuccess, Action<Exception> onError)
        {
            Run<object>(
                () => { work(); return null; },
                result => { if (onSuccess != null) onSuccess(); },
                onError);
        }
    }
}
