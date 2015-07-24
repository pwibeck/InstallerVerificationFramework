namespace InstallerVerificationLibrary.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Linq;
    using InstallerVerificationLibrary;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;

    [DataType("Directory")]
    [TestDataLoader]
    public sealed class DirectoryTestDataLoader : BaseTestDataLoader
    {
        public override ICollection<BaseTestData> ExtractData(XElement xmlNode, ICollection<DataProperty> properties)
        {
            var result = new Collection<BaseTestData>();
            var allowedAttributeNames = new Collection<string> { "path" };

            foreach (var node in xmlNode.Descendants().Where(x => string.Equals(x.Name.LocalName, this.ElementName, StringComparison.InvariantCultureIgnoreCase)))
            {
                CheckForAdditionalAttributes(node, allowedAttributeNames);
                var data = new DirectoryData
                {
                    Path = DataPropertyTool.ResolvePropertiesInString(properties, XmlTools.GetNamedAttributeValue(node, "path", string.Empty))
                        .Replace(@"\\\", @"\")
                        .Replace(@"\\", @"\")
                        .TrimEnd('\\')
                };

                AddCommonData(node, data, properties);
                result.Add(data);
            }

            return result;
        }
    }
}