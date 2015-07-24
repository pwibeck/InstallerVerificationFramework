namespace InstallerVerificationLibrary.Logging
{
    using System;

    public enum LogErrorType
    {
        Information = 0,
        Warning = 1,
        Error = 2
    }

    public static class Log
    {
        public delegate void ErrorLogged(LogData data);

        public delegate void InformationLogged(LogData data);

        public delegate void WarningLogged(LogData data);

        private static int indent;
        private const int IndentSpace = 3;

        public static event ErrorLogged ErrorEvent;
        public static event WarningLogged WarningEvent;
        public static event InformationLogged InfromationEvent;

        public static void WriteInfo(string message, string where)
        {
            WriteToLog(message, where, LogErrorType.Information);
        }

        public static void WriteWarning(string message, string where)
        {
            WriteToLog(message, where, LogErrorType.Warning);
        }

        public static void WriteError(string message, string where)
        {
            WriteToLog(message, where, LogErrorType.Error);
        }

        public static void WriteCheckError(string message, string check, string componentId)
        {
            WriteError(check + ". ComponentID:" + componentId + ". Message:" + message, "Check");
        }

        public static void WriteCheckInfo(string message, string check, string componentId)
        {
            WriteInfo(check + ". ComponentID: " + componentId + ". Data: " + message, "Check");
        }

        public static void WriteEntryError(string message, string entry)
        {
            WriteError(entry + ". Message:" + message, "Entry");
        }

        public static void WriteError(Exception exception, string where)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            var message = "Exception '" + exception.Message + "'. Stack trace '" + exception.StackTrace + "'";
            WriteToLog(message, where, LogErrorType.Error);
        }

        private static void WriteToLog(string message, string where, LogErrorType type)
        {
            var data = new LogData
            {
                Time = DateTime.Now,
                Message = message,
                Where = @where,
                ErrorType = type,
                Indent = indent
            };

            switch (type)
            {
                case LogErrorType.Information:
                    if (InfromationEvent != null) InfromationEvent(data);
                    break;
                case LogErrorType.Warning:
                    if (WarningEvent != null) WarningEvent(data);
                    break;
                case LogErrorType.Error:
                    if (ErrorEvent != null) ErrorEvent(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
        }

        public static void Indent()
        {
            indent += IndentSpace;
        }

        public static void Unindent()
        {
            indent -= IndentSpace;
            if (indent < 0)
            {
                indent = 0;
            }
        }
    }
}