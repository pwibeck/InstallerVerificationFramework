namespace InstallerVerificationLibrary.Logging
{
    using System;
    using System.Globalization;
    using System.IO;

    public sealed class StreamLogListener : BaseLogListener, IDisposable
    {
        private readonly StreamWriter streamWriter;
        private readonly bool writeTimestamp;

        public StreamLogListener(Stream stream) : this(stream, true)
        {
        }

        public StreamLogListener(Stream stream, bool writeTimestamp)
            : this(stream, writeTimestamp, LogErrorType.Information)
        {
        }

        public StreamLogListener(Stream stream, bool writeTimestamp, LogErrorType type) : base(type)
        {
            streamWriter = new StreamWriter(stream) {AutoFlush = true};
            this.writeTimestamp = writeTimestamp;
        }

        public Stream Stream
        {
            get { return streamWriter.BaseStream; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            streamWriter.Dispose();
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
            streamWriter.WriteLine(message);
        }
    }
}