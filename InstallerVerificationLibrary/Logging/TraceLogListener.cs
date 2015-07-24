namespace InstallerVerificationLibrary.Logging
{
    using System.Diagnostics;
    using System.Globalization;

    public class TraceLogListener : BaseLogListener
    {
        private readonly bool writeTimestamp;

        public TraceLogListener() : this(LogErrorType.Information)
        {
        }

        public TraceLogListener(LogErrorType type) : this(type, true)
        {
        }

        public TraceLogListener(LogErrorType type, bool writeTimestamp) : base(type)
        {
            this.writeTimestamp = writeTimestamp;
        }

        public override void Log(LogData data)
        {
            var message = string.Empty;
            if (writeTimestamp)
            {
                message += data.Time.ToString(CultureInfo.InvariantCulture);
            }

            for (var i = 0; i < data.Indent; i++)
            {
                message += " ";
            }

            message += " " + data.ErrorType + "->" + data.Where + " Message:" + data.Message;
            Trace.WriteLine(message);
        }
    }
}