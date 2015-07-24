namespace InstallerVerificationLibrary
{
    using System;

    public class DataProperty : ICloneable, IComparable
    {
        public DataProperty(string id, string value, DataPropertyType propertyType)
        {
            Id = id;
            Value = value;
            PropertyType = propertyType;
        }

        public DataProperty()
        {
        }

        public string Id { get; set; }
        public string Value { get; set; }
        public DataPropertyType PropertyType { get; set; }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        public override bool Equals(object obj)
        {
            var prop = obj as DataProperty;
            if (prop != null)
            {
                if (prop.Id == Id)
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public DataProperty Clone()
        {
            var dat = new DataProperty(Id, Value, PropertyType);
            return dat;
        }

        public override string ToString()
        {
            return "ID:" + Id + ", Value:" + Value + ", Type:" + PropertyType;
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            var dp = obj as DataProperty;
            if (dp != null)
            {
                if (dp.Id == Id)
                {
                    return 0;
                }
            }

            return -1;
        }

        public static int Compare(DataProperty left, DataProperty right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (left.Equals(right))
            {
                return 0;
            }

            if (ReferenceEquals(left, null))
            {
                return -1;
            }

            return left.CompareTo(right);
        }

        public static bool operator ==(DataProperty left, DataProperty right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(DataProperty left, DataProperty right)
        {
            return !(left == right);
        }

        public static bool operator <(DataProperty left, DataProperty right)
        {
            return (Compare(left, right) < 0);
        }

        public static bool operator >(DataProperty left, DataProperty right)
        {
            return (Compare(left, right) > 0);
        }

        #endregion
    }
}