namespace InstallerVerificationLibrary.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class TestDataLoaderAttribute : Attribute
    {
    }
}