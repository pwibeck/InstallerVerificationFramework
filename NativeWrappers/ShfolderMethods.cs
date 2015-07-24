namespace NativeWrappers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ShfolderMethods
    {
        [DllImport("shfolder.dll", CharSet = CharSet.Unicode)]
        public static extern int SHGetFolderPath(IntPtr owner, int folder, IntPtr token, int flags, StringBuilder path);
    }
}
