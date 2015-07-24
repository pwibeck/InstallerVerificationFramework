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

    [DataType("RegistryValue")]
    [TestDataLoader]
    public sealed class RegistryValueTestDataLoader : BaseTestDataLoader
    {
        public override ICollection<BaseTestData> ExtractData(XElement xmlNode, ICollection<DataProperty> properties)
        {
            var result = new Collection<BaseTestData>();
            var allowedAttributeNames = new Collection<string> { "key", "name", "data", "dataCmpMethod" };

            foreach (var node in xmlNode.Descendants().Where(x => string.Equals(x.Name.LocalName, this.ElementName, StringComparison.InvariantCultureIgnoreCase)))
            {
                CheckForAdditionalAttributes(node, allowedAttributeNames);
                var data = new RegistryValueData
                {
                    Key = DataPropertyTool.ResolvePropertiesInString(properties, XmlTools.GetNamedAttributeValue(node, "key", string.Empty)),
                    Name = XmlTools.GetNamedAttributeValue(node, "name", string.Empty),
                    DataCmpMethod = GetDataCmpMethod(node),
                    Data = GetData(properties, node)
                };

                AddCommonData(node, data, properties);
                result.Add(data);
            }

            return result;
        }

        private static string GetData(ICollection<DataProperty> properties, XElement node)
        {
            return !string.IsNullOrEmpty(XmlTools.GetNamedAttributeValue(node, "data", string.Empty)) 
                ? DataPropertyTool.ResolvePropertiesInString(properties, XmlTools.GetNamedAttributeValue(node, "data", string.Empty)) 
                : string.Empty;
        }

        private static RegistryValueData.DataComparisonMethod GetDataCmpMethod(XElement node)
        {
            try
            {
                return (RegistryValueData.DataComparisonMethod)
                        Enum.Parse(typeof (RegistryValueData.DataComparisonMethod),
                            XmlTools.GetNamedAttributeValue(node, "dataCmpMethod", "Equal"), true);
            }
            catch (ArgumentException)
            {
                throw new InstallerVerificationLibraryException(
                    "dataCmpMethod not correct. Possible options 'Equal' and 'Contains'. Found '" +
                    XmlTools.GetNamedAttributeValue(node, "dataCmpMethod", string.Empty) + "'");
            }
        }        
    }
}