namespace InstallerVerificationLibrary.Logging
{
    using System.Diagnostics;

    public class EventLogListener : BaseLogListener
    {
        private readonly string log = "Application";
        private readonly string source = "Installer Testing Library";

        public EventLogListener() : this(LogErrorType.Information)
        {
        }

        public EventLogListener(LogErrorType type) : base(type)
        {
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, log);
            }
        }

        public override void Log(LogData data)
        {
            var message = data.Where + " -> " + data.Message;
            if (data.ErrorType == LogErrorType.Error)
            {
                EventLog.WriteEntry(source, message, EventLogEntryType.Warning);
            }

            if (data.ErrorType == LogErrorType.Information)
            {
                EventLog.WriteEntry(source, message, EventLogEntryType.Information);
            }
        }
    }
}