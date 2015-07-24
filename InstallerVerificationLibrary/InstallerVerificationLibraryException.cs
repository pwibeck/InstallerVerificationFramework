namespace InstallerVerificationLibrary
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InstallerVerificationLibraryException : Exception
    {
        public InstallerVerificationLibraryException(string message)
            : base(message)
        {
        }

        public InstallerVerificationLibraryException()
        {
        }

        public InstallerVerificationLibraryException(string message, Exception exception)
            : base(message, exception)
        {
        }

        #region ISerializable Members

        protected InstallerVerificationLibraryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }
}
