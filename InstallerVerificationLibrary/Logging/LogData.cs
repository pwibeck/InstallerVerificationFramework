namespace InstallerVerificationLibrary.Logging
{
    using System;

    public class LogData
    {
        public string Message { get; set; }
        public LogErrorType ErrorType { get; set; }
        public DateTime Time { get; set; }
        public string Where { get; set; }
        public int Indent { get; set; }
    }
}