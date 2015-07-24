namespace NativeWrappers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class MsiMethods
    {
        public enum MSIDBOPEN
        {
            MSIDBOPEN_READONLY = 0,  // database open read-only, no persistent changes
            MSIDBOPEN_TRANSACT = 1,  // database read/write in transaction mode
            MSIDBOPEN_DIRECT = 2,  // database direct read/write without transaction
            MSIDBOPEN_CREATE = 3,  // create new database, transact mode read/write
            MSIDBOPEN_CREATEDIRECT = 4  // create new database, direct mode read/write
        }

        public enum ErrorCodes
        {
            Success = 0,
            MoreData = 234,
            NoMoreItems = 259
        }
        
        public enum PropertyId
        {
            PID_UNKNOWN = -1,
            PID_DICTIONARY = 0,
            /// <summary>
            /// Document Code Page, short integer
            /// </summary>
            PID_CODEPAGE = 1,
            /// <summary>
            /// Document title, string
            /// </summary>
            PID_TITLE = 2,
            /// <summary>
            /// Subject, string
            /// </summary>
            PID_SUBJECT = 3,
            /// <summary>
            /// Author, string
            /// </summary>
            PID_AUTHOR = 4,
            /// <summary>
            /// Keywords, string
            /// </summary>
            PID_KEYWORDS = 5,
            /// <summary>
            /// Comments, string
            /// </summary>
            PID_COMMENTS = 6,
            /// <summary>
            /// Template name, string
            /// </summary>
            PID_TEMPLATE = 7,
            /// <summary>
            /// Last Author, string
            /// </summary>
            PID_LASTAUTHOR = 8,
            /// <summary>
            ///  Revision Number, string
            /// </summary>
            PID_REVNUMBER = 9,
            /// <summary>
            /// Edit Date Time, DateTime
            /// </summary>
            PID_EDITTIME = 10,
            /// <summary>
            /// Last Printed, DateTime
            /// </summary>
            PID_LASTPRINTED = 11,
            /// <summary>
            /// Create date time, DateTime
            /// </summary>
            PID_CREATE_DTM = 12,
            /// <summary>
            /// Last save date time, DateTime
            /// </summary>
            PID_LASTSAVE_DTM = 13,
            /// <summary>
            /// Page count, integer
            /// </summary>
            PID_PAGECOUNT = 14,
            /// <summary>
            /// Word count, integer
            /// </summary>
            PID_WORDCOUNT = 15,
            /// <summary>
            ///  Character count, integer
            /// </summary>
            PID_CHARCOUNT = 16,
            /// <summary>
            /// Thumbnail, clipboard format + metafile/bitmap (not supported)
            /// </summary>
            PID_THUMBNAIL = 17,
            /// <summary>
            /// App used for creation, string
            /// </summary>
            PID_APPNAME = 18,
            /// <summary>
            /// Security, integer
            /// </summary>
            PID_SECURITY = 19
        }

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiGetProperty(IntPtr handle, string name, [Out] StringBuilder value, ref int valueLenght);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiOpenPackage(string fileName, out IntPtr handle);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiCloseHandle(IntPtr handle);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiOpenDatabase(string batabasePath, IntPtr persist, ref IntPtr handle);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiDatabaseOpenView(IntPtr databaseHandle, string query, ref IntPtr viewHandle);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiViewExecute(IntPtr viewHandle, int record);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiViewFetch(IntPtr viewHandle, ref IntPtr recordHandle);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiRecordGetString(IntPtr recordHandle, int field, [Out] StringBuilder valueBuffer, ref int pcchValueBuf);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiRecordDataSize(IntPtr recordHandle, int field);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiRecordReadStream(IntPtr recordHandle, int field, [Out] byte[] valueBuffer, ref int pcchValueBuf);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiGetSummaryInformation(IntPtr handle, string databasePath, int updateCount, ref IntPtr summaryInfoHandle);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiSummaryInfoSetProperty(IntPtr handle, int property, int dataType, int value, IntPtr filetime, string valueSize);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiSummaryInfoPersist(IntPtr handle);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern int MsiDatabaseCommit(IntPtr handle);
    }
}