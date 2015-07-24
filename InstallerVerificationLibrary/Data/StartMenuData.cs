namespace InstallerVerificationLibrary.Data
{
    using InstallerVerificationLibrary.Attribute;

    [DataType("StartMenu")]
    public sealed class StartMenuData : BaseTestData
    {
        public string Name { get; set; }

        public bool AllUsers { get; set; }

        private bool Equals(StartMenuData other)
        {
            return string.Equals(Name, other.Name) && AllUsers == other.AllUsers;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is StartMenuData && Equals((StartMenuData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ AllUsers.GetHashCode();
            }
        }
        
        public override string ToString()
        {
            return "Name:'" + Name + "' All Users:'" + AllUsers + "'";
        }
    }
}