namespace Lift.Data
{
    /// <summary>Anything that can store a log entry. The LogObserver depends on this, not on SQL.</summary>
    public interface ILogWriter
    {
        void Write(LogEntry entry);
    }
}
