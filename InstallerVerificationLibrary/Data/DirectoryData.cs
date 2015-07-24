namespace InstallerVerificationLibrary.Data
{
    using InstallerVerificationLibrary.Attribute;

    [DataType("Directory")]
    public sealed class DirectoryData : BaseTestData
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

        private bool Equals(DirectoryData other)
        {
            return string.Equals(path, other.path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is DirectoryData && Equals((DirectoryData) obj);
        }

        public override int GetHashCode()
        {
            return (path != null ? path.GetHashCode() : 0);
        }
        
        public override string ToString()
        {
            return "Directory Path:'" + Path + "'";
        }
    }
}
