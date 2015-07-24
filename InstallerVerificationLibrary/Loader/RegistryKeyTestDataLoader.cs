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
    using InstallerVerificationLibrary.Tools;

    [DataType("RegistryKey")]
    [TestDataLoader]
    public sealed class RegistryKeyTestDataLoader : BaseTestDataLoader
    {
        public override ICollection<BaseTestData> ExtractData(XElement xmlNode, ICollection<DataProperty> properties)
        {
            var result = new Collection<BaseTestData>();
            var allowedAttributeNames = new Collection<string> { "key" };

            foreach (var node in xmlNode.Descendants().Where(x => string.Equals(x.Name.LocalName, this.ElementName, StringComparison.InvariantCultureIgnoreCase)))
            {
                CheckForAdditionalAttributes(node, allowedAttributeNames);
                var data = new RegistryKeyData
                {
                    Key = DataPropertyTool.ResolvePropertiesInString(properties, XmlTools.GetNamedAttributeValue(node, "key", string.Empty))                    
                };

                AddCommonData(node, data, properties);
                result.Add(data);
            }

            return result;
        }
    }
}