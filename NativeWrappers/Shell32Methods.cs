namespace NativeWrappers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class Shell32API
    {
        const int CSIDL_COMMON_STARTMENU = 0x16;  // \Windows\Start Menu
        const int CSIDL_COMMON_PROGRAMS = 0x17;// \Windows\Start Menu\Programs 
        const int CSIDL_STARTMENU = 11;  // \Windows\Start Menu

        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);

        public static string GetStartMenuFolder()
        {
            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_STARTMENU, false);
            return path.ToString();
        }

        public static string GetCommonStartMenuFolder()
        {
            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_COMMON_STARTMENU, false);
            return path.ToString();
        }

        public static string GetCommonProgramsFolder()
        {
            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_COMMON_PROGRAMS, false);
            return path.ToString();
        }
    }
}