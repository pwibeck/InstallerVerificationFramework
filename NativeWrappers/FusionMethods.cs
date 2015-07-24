namespace NativeWrappers
{
    using System;
    using System.Runtime.InteropServices;

    public static class FusionMethods
    {
        [DllImport("fusion.dll")]
        public static extern int CreateAssemblyEnum(
            out IAssemblyEnum ppEnum,
            IntPtr pUnkReserved,
            IAssemblyName pName,
            AssemblyCacheFlags flags,
            IntPtr pvReserved);

        [DllImport("fusion.dll")]
        public static extern int CreateAssemblyNameObject(
            out IAssemblyName ppAssemblyNameObj,
            [MarshalAs(UnmanagedType.LPWStr)]
                string szAssemblyName,
            CreateAssemblyNameObjectFlags flags,
            IntPtr pvReserved);

        [DllImport("fusion.dll")]
        public static extern int CreateAssemblyCache(
            out IAssemblyCache ppAsmCache,
            int reserved);

        [DllImport("fusion.dll")]
        public static extern int CreateInstallReferenceEnum(
            out IInstallReferenceEnum ppRefEnum,
            IAssemblyName pName,
            int dwFlags,
            IntPtr pvReserved);
    }
}