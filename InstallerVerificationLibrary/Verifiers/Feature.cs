namespace InstallerVerificationLibrary
{
    using System.Collections.ObjectModel;
    using InstallerVerificationLibrary.Data;

    public class Feature
    {
        public string FeatureName { get; set; }

        public string InstallationDir { get; set; }

        public string Msiid { get; set; }

        public Feature()
        {
            FeatureData = new Collection<BaseTestData>();
        }

        public Collection<BaseTestData> FeatureData { get; private set; }

        public override bool Equals(object obj)
        {
            var iccf = obj as Feature;
            if (iccf != null)
            {
                if (iccf.FeatureName == FeatureName && iccf.Msiid == Msiid)
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

        public override string ToString()
        {
            return "MSI ID:" + Msiid + ", Name:" + FeatureName + ", InstallDir:" + InstallationDir;
        }
    }
}
