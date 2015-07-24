namespace InstallerVerificationLibrary.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Linq;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;

    [DataType("File")]
    [TestDataLoader]
    public sealed class FileTestDataLoader : BaseTestDataLoader
    {
        public override ICollection<BaseTestData> ExtractData(XElement xmlNode, ICollection<DataProperty> properties)
        {
            var result = new Collection<BaseTestData>();
            var allowedAttributeNames = new Collection<string> { "path", "fileVersion" };

            foreach (var node in xmlNode.Descendants().Where(x => string.Equals(x.Name.LocalName, this.ElementName, StringComparison.InvariantCultureIgnoreCase)))
            {
                CheckForAdditionalAttributes(node, allowedAttributeNames);
                var data = new FileData
                {
                    Path = DataPropertyTool.ResolvePropertiesInString(properties, XmlTools.GetNamedAttributeValue(node, "path", string.Empty)),
                    FileVersion = XmlTools.GetNamedAttributeValue(node, "fileVersion", string.Empty)
                };

                AddCommonData(node, data, properties);

                result.Add(data);
            }

            return result;
        }
    }
}