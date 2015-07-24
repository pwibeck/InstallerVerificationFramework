namespace InstallerVerificationLibrary.Logging
{
    using System;
    using System.Globalization;

    public class ConsoleLogListener : BaseLogListener
    {
        private readonly bool writeTimestamp;

        public ConsoleLogListener() : this(LogErrorType.Information)
        {
        }

        public ConsoleLogListener(LogErrorType type) : this(type, true)
        {
        }

        public ConsoleLogListener(LogErrorType type, bool writeTimestamp) : base(type)
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

            if (writeTimestamp && data.Indent <= 0)
            {
                message += " ";
            }

            message += data.ErrorType + "->" + data.Where + " Message:" + data.Message;

            if (data.ErrorType == LogErrorType.Error)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
            else
            {
                Console.WriteLine(message);
            }
            
        }
    }
}