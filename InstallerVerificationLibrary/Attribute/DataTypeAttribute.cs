namespace InstallerVerificationLibrary.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DataTypeAttribute : Attribute
    {
        private readonly string dataType;

        public string CheckType
        {
            get { return dataType; }
        }

        public DataTypeAttribute(string type)
        {
            dataType = type;
        }
    }
}