namespace InstallerVerificationLibrary.Data
{
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Tools;

    [DataType("RegistryKey")]
    public sealed class RegistryKeyData : BaseTestData
    {
        public string Key { get; set; }       

        private bool Equals(RegistryKeyData other)
        {
            return string.Equals(Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RegistryKeyData && Equals((RegistryKeyData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Key != null ? Key.GetHashCode() : 0;
            }
        }
        
        public override string ToString()
        {
            return "Registry Key:'" + Key;
        }
    }
}
