namespace InstallerVerificationLibrary.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;

    [DataType("StartMenu")]
    [TestDataLoader]
    public sealed class StartMenuTestDataLoader : BaseTestDataLoader
    {
        public override ICollection<BaseTestData> ExtractData(XElement xmlNode, ICollection<DataProperty> properties)
        {
            var result = new Collection<BaseTestData>();
            var allowedAttributeNames = new Collection<string> { "name", "allusers" };

            foreach (var node in xmlNode.Descendants().Where(x => string.Equals(x.Name.LocalName, this.ElementName, StringComparison.InvariantCultureIgnoreCase)))
            {
                CheckForAdditionalAttributes(node, allowedAttributeNames);
                var data = new StartMenuData
                {
                    Name = XmlTools.GetNamedAttributeValue(node, "name", string.Empty),
                    AllUsers = bool.Parse(XmlTools.GetNamedAttributeValue(node, "allusers", string.Empty))
                };

                AddCommonData(node, data, properties);
                result.Add(data);
            }

            return result;
        }
    }
}