namespace InstallerVerificationLibrary.Data
{
    using System;
    using InstallerVerificationLibrary.Attribute;

    [DataType("File")]
    public sealed class FileData : BaseTestData
    {
        private string path = string.Empty;

        public string Path
        {
            get { return path; }
            set
            {
                if (value == null)
                {
                    throw new InstallerVerificationLibraryException("Path can't be null");
                }

                path = value.Replace(@"\\", @"\"); 
            }
        }

        public string FileVersion { get; set; }

        private bool Equals(FileData other)
        {
            return string.Equals(path, other.path) && string.Equals(FileVersion, other.FileVersion);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FileData && Equals((FileData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((path != null ? path.GetHashCode() : 0)*397) ^ (FileVersion != null ? FileVersion.GetHashCode() : 0);
            }
        }
        
        public override string ToString()
        {
            return "File path:'" + Path + "' File version:'" + FileVersion + "'";
        }
    }
}
