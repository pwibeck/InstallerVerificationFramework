namespace NativeWrappers
{
    using System.Runtime.InteropServices;

    public class MscoreeMethods
    {
        [DllImport("mscoree.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool StrongNameSignatureVerificationEx(string filePath,
            [MarshalAs(UnmanagedType.U1)] bool forceVerification, 
            [MarshalAs(UnmanagedType.U1)] ref bool wasVerified);
    }
}