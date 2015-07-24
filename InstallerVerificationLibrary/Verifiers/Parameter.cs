namespace InstallerVerificationLibrary.Verifiers
{
    public class Parameter
    {
        public string Id { get; set; }
                      
        public string Value { get; set; }

        protected bool Equals(Parameter other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Parameter) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0)*397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return "ID:" + Id + ", Value:" + Value;
        }
    }
}
