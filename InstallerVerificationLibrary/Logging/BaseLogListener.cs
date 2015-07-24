namespace InstallerVerificationLibrary.Logging
{
    using System;

    public abstract class BaseLogListener
    {
        protected BaseLogListener(LogErrorType type)
        {
            ShouldLog = false;
            Type = type;
        }

        protected LogErrorType Type { get; set; }
        protected bool ShouldLog { get; set; }

        public void StartLogging()
        {
            ShouldLog = true;
            switch (Type)
            {
                case LogErrorType.Information:
                    Logging.Log.InfromationEvent += DoLog;
                    Logging.Log.WarningEvent += DoLog;
                    Logging.Log.ErrorEvent += DoLog;
                    break;
                case LogErrorType.Warning:
                    Logging.Log.WarningEvent += DoLog;
                    Logging.Log.ErrorEvent += DoLog;
                    break;
                case LogErrorType.Error:
                    Logging.Log.ErrorEvent += DoLog;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", Type, null);
            }
        }

        public void StopLogging()
        {
            ShouldLog = false;
            switch (Type)
            {
                case LogErrorType.Information:
                    Logging.Log.InfromationEvent -= DoLog;
                    Logging.Log.WarningEvent -= DoLog;
                    Logging.Log.ErrorEvent -= DoLog;
                    break;
                case LogErrorType.Warning:
                    Logging.Log.WarningEvent -= DoLog;
                    Logging.Log.ErrorEvent -= DoLog;
                    break;
                case LogErrorType.Error:
                    Logging.Log.ErrorEvent -= DoLog;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", Type, null);
            }
        }

        private void DoLog(LogData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (!ShouldLog)
            {
                return;
            }

            Log(data);
        }

        public abstract void Log(LogData data);
    }
}