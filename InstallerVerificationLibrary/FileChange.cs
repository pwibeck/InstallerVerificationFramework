namespace InstallerVerificationLibrary
{
    public enum FileChangeType
    {
        Create,
        Delete,
        Rename
    }

    public class FileChange
    {
        protected bool Equals(FileChange other)
        {
            return Type == other.Type && string.Equals(Path, other.Path) && string.Equals(OldPath, other.OldPath) && IsDirectory.Equals(other.IsDirectory);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode*397) ^ (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (OldPath != null ? OldPath.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ IsDirectory.GetHashCode();
                return hashCode;
            }
        }

        public FileChangeType Type;
        public string Path;
        public string OldPath;
        public bool IsDirectory;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileChange) obj);
        }

        public override string ToString()
        {
            if (Type == FileChangeType.Rename)
            {
                return "Type:" + Type + " Path:" + Path + " PathOld:" + OldPath + " IsDirectory:" + IsDirectory;
            }
            
            return "Type:" + Type + " Path:" + Path + " IsDirectory:" + IsDirectory;
        }
    }
}
