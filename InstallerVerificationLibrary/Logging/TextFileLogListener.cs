namespace InstallerVerificationLibrary.Logging
{
    using System;
    using System.Globalization;
    using System.IO;

    public sealed class TextFileLogListener : BaseLogListener, IDisposable
    {
        private readonly StreamWriter swriter;
        private readonly bool writeTimestamp;

        public TextFileLogListener(string filePath) : this(filePath, true)
        {
        }

        public TextFileLogListener(string filePath, bool writeTimestamp)
            : this(filePath, writeTimestamp, LogErrorType.Information)
        {
        }

        public TextFileLogListener(string filePath, bool writeTimestamp, LogErrorType type) : base(type)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                throw new InstallerVerificationLibraryException("Could not open log file:'" + filePath +
                                                                "'. Since one or more of the directories in the path doesn't exist");
            }

            swriter = new StreamWriter(filePath) {AutoFlush = true};
            this.writeTimestamp = writeTimestamp;
        }

        #region IDisposable Members

        public void Dispose()
        {
            swriter.Dispose();
        }

        #endregion

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
            swriter.WriteLine(message);
        }
    }
}