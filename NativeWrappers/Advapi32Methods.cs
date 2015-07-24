namespace NativeWrappers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Advapi32Methods
    {
        public const int StandardRightsRequired = 0x000F0000;
        public const int ReadControl = 0x00020000;
        public const int Synchronize = 0x00100000;
        public const int StandardRightsRead = ReadControl;
        public const int StandardRightsWrite = ReadControl;
        public const int StandardRightsExecute = ReadControl;
        public const int StandardRightsAll = 0x001F0000;
        public const int KeyQueryValue = 0x0001;
        public const int KeySetValue = 0x0002;
        public const int KeyCreateSubKey = 0x0004;
        public const int KeyEnumerateSubKeys = 0x0008;
        public const int KeyNotify = 0x0010;
        public const int KeyCreateLink = 0x0020;
        public const int KeyWow6432Key = 0x0200;
        public const int KeyWow64Key = 0x0100;
        public const int KeyAllAccess = StandardRightsAll | KeyQueryValue | KeySetValue | KeyCreateSubKey | KeyEnumerateSubKeys | KeyNotify | KeyCreateLink & (~Synchronize);

        public enum Type
        {
            REG_BINARY = 3,
            REG_DWORD = 4,
            REG_DWORD_BIG_ENDIAN = 5,
            ////REG_DWORD_LITTLE_ENDIAN = 4,
            REG_EXPAND_SZ = 2,
            REG_LINK = 6,
            REG_MULTI_SZ = 7,
            REG_NONE = 0,
            REG_RESOURCE_LIST = 8,
            REG_SZ = 1
        }

        public static UIntPtr ClassesRootKey 
        {
            get
            {
                return new UIntPtr(0x80000000u);
            }
        }

        public static UIntPtr CurrentUserKey
        {
            get
            {
                return new UIntPtr(0x80000001u);
            }
        }

        public static UIntPtr LocalMachineKey
        {
            get
            {
                return new UIntPtr(0x80000002u);
            }
        }

        public static UIntPtr UsersKey
        {
            get
            {
                return new UIntPtr(0x80000003u);
            }
        }

        public static UIntPtr PerformanceDataKey
        {
            get
            {
                return new UIntPtr(0x80000004u);
            }
        }

        public static UIntPtr CurrentConfigKey
        {
            get
            {
                return new UIntPtr(0x80000005u);
            }
        }

        public static UIntPtr DynDataKey
        {
            get
            {
                return new UIntPtr(0x80000006u);
            }
        }
                        
        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern int RegOpenKeyEx(
          UIntPtr handleKey,
          string subKey,
          int options,
          int samDesired,
          out UIntPtr handlekResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        public static extern int RegQueryValueEx(
            UIntPtr handleKey,
            string valueName,
            int[] reserved,
            ref int type,
            byte[] data,
            ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        public static extern int RegQueryValueEx(
            UIntPtr handleKey,
            string valueName,
            int[] reserved,
            ref int type,
            StringBuilder data,
            ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        public static extern int RegQueryValueEx(
            UIntPtr handleKey,
            string valueName,
            int[] reserved,
            ref int type,
            ref int data,
            ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        public static extern int RegQueryValueEx(
            UIntPtr handleKey,
            string valueName,
            int[] reserved,
            ref int type,
            char[] data,
            ref int lpcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
        public static extern int RegQueryValueEx(
            UIntPtr handleKey,
            string valueName,
            int[] reserved,
            ref int type,
            ref long data,
            ref int lpcbData);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(UIntPtr handleKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegDeleteKeyExW", SetLastError = true)]
        public static extern int RegDeleteKeyEx(
            UIntPtr handleKey,
            string subKey,
            int samDesired,
            uint reserved);       
    }
}