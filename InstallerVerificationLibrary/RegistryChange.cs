namespace InstallerVerificationLibrary
{
    public enum RegistryChangeType
    {
        CreateKey,
        DeleteKey,
        SetValue,
        DeleteValue
    }

    public class Registryhange
    {
        protected bool Equals(Registryhange other)
        {
            return Type == other.Type && string.Equals(Key, other.Key) && string.Equals(Value, other.Value) && string.Equals(ValueName, other.ValueName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode*397) ^ (Key != null ? Key.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ValueName != null ? ValueName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public RegistryChangeType Type;
        public string Key;
        public string Value;
        public string ValueName;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Registryhange) obj);
        }

        public override string ToString()
        {
            if (Type == RegistryChangeType.SetValue || Type == RegistryChangeType.DeleteValue)
            {
                return "Type: " + Type + " Path:" + Key + " ValueName:" + ValueName + " Value:" + Value;
            }

            return "Type: " + Type + " Path:" + Key;
        }
    }
}
