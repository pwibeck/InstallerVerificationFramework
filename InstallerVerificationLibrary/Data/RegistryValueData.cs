namespace InstallerVerificationLibrary.Data
{
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Tools;

    [DataType("RegistryValue")]
    public sealed class RegistryValueData : BaseTestData
    {
        private bool Equals(RegistryValueData other)
        {
            return string.Equals(Key, other.Key) && 
                   string.Equals(Data, other.Data) && 
                   string.Equals(Name, other.Name) && 
                   DataCmpMethod == other.DataCmpMethod;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Key != null ? Key.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Data != null ? Data.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) DataCmpMethod;
                return hashCode;
            }
        }

        public string Key { get; set; }

        public string Data { get; set; }

        public string Name { get; set; }
        
        public enum DataComparisonMethod
        {
            Equal,
            Contains            
        }

        public DataComparisonMethod DataCmpMethod { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RegistryValueData && Equals((RegistryValueData) obj);
        }
        
        public override string ToString()
        {
            return "Registry Key:'" + Key + "' Name:'" + Name + "' Data:'" + Data + "' DataComparisonMethod:'" +  DataCmpMethod + "'";
        }
    }
}
