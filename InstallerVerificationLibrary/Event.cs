namespace InstallerVerificationLibrary
{
    using System;

    public class Event
    {
        public string Operation;
        public string Path;
        public string Detail;
        public int ProcessId;
        public string ProcessName;
        public DateTime Time;

        public override string ToString()
        {
            return "Operation:" + Operation + " Path:" + Path + " Details:" + Detail;
        }
    }
}
