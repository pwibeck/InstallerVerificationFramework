namespace InstallerVerificationLibrary.Loader
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    public interface ITestDataLoader
    {
        string Name { get; }

        ICollection<Data.BaseTestData> ExtractData(XElement xmlNode, ICollection<DataProperty> properties);
    }
}
