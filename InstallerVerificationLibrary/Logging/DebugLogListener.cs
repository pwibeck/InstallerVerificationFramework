namespace InstallerVerificationLibrary.Logging
{
    using System.Diagnostics;
    using System.Globalization;

    public class DebugLogListener : BaseLogListener
    {
        private readonly bool writeTimestamp;

        public DebugLogListener() : this(LogErrorType.Information)
        {
        }

        public DebugLogListener(LogErrorType type) : this(type, true)
        {
        }

        public DebugLogListener(LogErrorType type, bool writeTimestamp) : base(type)
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
            Debug.WriteLine(message);
        }
    }
}