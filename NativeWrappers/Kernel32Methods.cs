namespace NativeWrappers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Kernel32Methods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(
                 [MarshalAs(UnmanagedType.LPTStr)]
                   string path,
                 [MarshalAs(UnmanagedType.LPTStr)]
                   StringBuilder shortPath,
                 int shortPathLength);

        [DllImport("kernel32.dll")]
        public static extern void GetNativeSystemInfo(ref SystemInfoNative systemInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemInfoNative
        {
            public ushort ProcessorArchitecture;
            public ushort Reserved;
            public uint PageSize;
            public IntPtr MinimumApplicationAddress;
            public IntPtr MaximumApplicationAddress;
            public IntPtr ActiveProcessorMask;
            public uint NumberOfProcessors;
            public uint ProcessorType;
            public uint AllocationGranularity;
            public ushort ProcessorLevel;
            public ushort ProcessorRevision;
        }
    }
}