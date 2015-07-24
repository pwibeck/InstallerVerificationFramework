namespace InstallerVerificationLibrary.Data
{
    public class PluginData
    {
        public string PluginDllPath { get; set; }

        public string DataType { get; set; }
        
        protected bool Equals(PluginData other)
        {
            return string.Equals(PluginDllPath, other.PluginDllPath) && string.Equals(DataType, other.DataType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PluginData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((PluginDllPath != null ? PluginDllPath.GetHashCode() : 0)*397) ^ (DataType != null ? DataType.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return "Data type:" + DataType + ", DLL path:" + PluginDllPath;
        }
    }
}
