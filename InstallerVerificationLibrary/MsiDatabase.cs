namespace InstallerVerificationLibrary
{
    using System;
    using System.Runtime.InteropServices;

    using NativeWrappers;

    public sealed class MsiDatabase : IDisposable
    {
        private IntPtr dbhandle;
        private string filePath;
        private bool opened;

        public void Open(string filePath, MsiMethods.MSIDBOPEN mode)
        {
            if (opened)
            {
                Close();
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            this.filePath = filePath;
            dbhandle = IntPtr.Zero;
            var returnValue = MsiMethods.MsiOpenDatabase(this.filePath, (IntPtr)mode, ref dbhandle);
            if (returnValue != (int)MsiMethods.ErrorCodes.Success)
            {
                throw new InstallerVerificationLibraryException("Error opening file:" + this.filePath);
            }

            opened = true;
        }

        public static MsiDatabase CreateNew(string filePath, string productCode, string packageCode)
        {
            if (System.IO.File.Exists(filePath))
            {
                throw new ArgumentException("File already exist:" + filePath);
            }

            if (string.IsNullOrEmpty(productCode))
            {
                throw new ArgumentNullException(productCode);
            }

            if (string.IsNullOrEmpty(packageCode))
            {
                throw new ArgumentNullException(packageCode);
            }

            var db = new MsiDatabase();
            db.Open(filePath, MsiMethods.MSIDBOPEN.MSIDBOPEN_CREATEDIRECT);

            db.SetSummaryInformation(MsiMethods.PropertyId.PID_REVNUMBER, VarEnum.VT_LPSTR, packageCode);
            db.SetSummaryInformation(MsiMethods.PropertyId.PID_PAGECOUNT, VarEnum.VT_I4, 200);
            db.SetSummaryInformation(MsiMethods.PropertyId.PID_WORDCOUNT, VarEnum.VT_I4, 0);
            db.CommitChange();

            db.Query("CREATE TABLE `Property` (`Property` CHAR(72) NOT NULL, `Value` CHAR(0) NOT NULL LOCALIZABLE PRIMARY KEY `Property`)");
            db.Query("INSERT INTO `Property` (`Property`, `Value`) VALUES ('ProductCode', '" + productCode + "')");
            db.CommitChange();

            return db;
        }

        private void CommitChange()
        {
            if (MsiMethods.MsiDatabaseCommit(dbhandle) != (int)MsiMethods.ErrorCodes.Success)
            {
                throw new InstallerVerificationLibraryException("Could not commit changes to database");
            }
        }

        private static void CloseHandle(IntPtr handle)
        {
            if (MsiMethods.MsiCloseHandle(handle) != (int)MsiMethods.ErrorCodes.Success)
            {
                throw new InstallerVerificationLibraryException("Could not close handle");
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa372045(v=vs.85).aspx
        /// </summary>
        public void SetSummaryInformation(MsiMethods.PropertyId propertyId, VarEnum datatype, object value)
        {
            var summaryHandle = IntPtr.Zero;
            if (MsiMethods.MsiGetSummaryInformation(dbhandle, null, 7, ref summaryHandle) != (int)MsiMethods.ErrorCodes.Success)
            {
                throw new InstallerVerificationLibraryException("Could not open summary handler to");
            }

            switch (datatype)
            {
                case VarEnum.VT_FILETIME:
                    break;
                case VarEnum.VT_I2:
                case VarEnum.VT_I4:
                    if (MsiMethods.MsiSummaryInfoSetProperty(summaryHandle, (int)propertyId, (int)datatype, Convert.ToInt32(value), IntPtr.Zero, null) != (int)MsiMethods.ErrorCodes.Success)
                    {
                        throw new InstallerVerificationLibraryException("Could not set summary property" + propertyId);
                    }
                    break;
                case VarEnum.VT_LPSTR:
                    if (MsiMethods.MsiSummaryInfoSetProperty(summaryHandle, (int)propertyId, (int)datatype, 0, IntPtr.Zero, Convert.ToString(value)) != (int)MsiMethods.ErrorCodes.Success)
                    {
                        throw new InstallerVerificationLibraryException("Could not set summary property" + propertyId);
                    }
                    break;
            }

            if (MsiMethods.MsiSummaryInfoPersist(summaryHandle) != (int)MsiMethods.ErrorCodes.Success)
            {
                throw new InstallerVerificationLibraryException("Could not persist summary property");
            }

            CloseHandle(summaryHandle);
        }

        public void Query(string query)
        {
            var viewHandle = IntPtr.Zero;
            var returnValue = MsiMethods.MsiDatabaseOpenView(dbhandle, query, ref viewHandle);
            if (returnValue != (int)MsiMethods.ErrorCodes.Success)
            {
                throw new InstallerVerificationLibraryException("Error open view for :" + query);
            }

            returnValue = MsiMethods.MsiViewExecute(viewHandle, 0);
            CloseHandle(viewHandle);

            if (returnValue != (int)MsiMethods.ErrorCodes.Success)
            {
                throw new InstallerVerificationLibraryException("Error executing query :" + query);
            }
        }
        
        public void Close()
        {
            var returnValue = MsiMethods.MsiCloseHandle(dbhandle);
            if (returnValue != (int)MsiMethods.ErrorCodes.Success)
            {
                throw new InstallerVerificationLibraryException("Could not close msi: " + filePath);
            }

            dbhandle = IntPtr.Zero;
            opened = false;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
