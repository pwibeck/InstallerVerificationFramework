namespace InstallerVerificationLibrary.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using InstallerTestingToolset.Verifiers;
    using InstallerVerificationLibrary.Attribute;
    using InstallerVerificationLibrary.Data;
    using InstallerVerificationLibrary.Logging;
    using InstallerVerificationLibrary.Verifiers;

    [DataType("MsiFeature")]
    [TestDataLoader]
    public sealed class MsiFeatureTestDataLoader : BaseTestDataLoader
    {
        public ICollection<MsiFile> Msifiles { get; set; }
        public ICollection<PluginData> Plugins { get; set; }
        public string ComponentId { get; set; }

        public override ICollection<BaseTestData> ExtractData(XElement xmlNode, ICollection<DataProperty> properties)
        {
            var result = new Collection<BaseTestData>();
            var allowedAttributeNames = new Collection<string> { "msiid", "name" };

            foreach (var node in xmlNode.Descendants().Where(x => string.Equals(x.Name.LocalName, this.ElementName, StringComparison.InvariantCultureIgnoreCase)))
            {
                CheckForAdditionalAttributes(node, allowedAttributeNames);

                var msiid = XmlTools.GetNamedAttributeValue(node, "msiid", string.Empty);
                var featureName = XmlTools.GetNamedAttributeValue(node, "name", string.Empty);
                MsiFile msifle;
                try
                {
                    msifle = FindMsiFile(Msifiles, msiid);
                }
                catch (InstallerVerificationLibraryException e)
                {
                    Log.WriteEntryError(e.Message, "MsiFeature");
                    throw;
                }

                var msiNode = GetMsiNode(msifle.Path);

                // Load local MSI properties                            
                var localMsiProperties = CollectionTools.CopyCollection(properties);
                CollectionTools.MergeCollection(localMsiProperties,
                    DataPropertyTool.ExtractProperties(msiNode, new Collection<Parameter>()), true);
                DataPropertyTool.ResolveProperties(localMsiProperties);

                Collection<Feature> extractedFeatures;
                try
                {
                    extractedFeatures = TestDataLoader.LoadFeatureDataFromXmlNode(msiNode, localMsiProperties, featureName, Msifiles,
                        Plugins);
                    if (extractedFeatures.Count == 0)
                    {
                        throw new InstallerVerificationLibraryException("Feature '" + featureName + "' was not found in file '" +
                                                                   msifle.Path + "'");
                    }
                }
                catch (Exception e)
                {
                    Log.WriteError(e,
                        "An exception was found when loading data for feature '" + featureName + "' from file '" + msifle.Path +
                        "'");
                    throw;
                }

                foreach (var data in extractedFeatures[0].FeatureData)
                {
                    data.ComponentID = ComponentId + " -> " + featureName;
                    result.Add(data);
                }
            }

            return result;
        }

        public static XElement GetMsiNode(string filePathMsi)
        {
            var msifileDoc = XElement.Load(filePathMsi);
            var msiNode = msifileDoc.Descendants().Where(x => x.Name.LocalName == "msi").ToList()[0];
            return msiNode;
        }

        public static MsiFile FindMsiFile(ICollection<MsiFile> msifiles, string msiid)
        {
            if (string.IsNullOrEmpty(msiid))
            {
                throw new ArgumentNullException("msiid");
            }

            if (msifiles == null)
            {
                throw new ArgumentNullException("msifiles");
            }

            var msifile = msifiles.FirstOrDefault(item => string.CompareOrdinal(item.Id, msiid) == 0);

            if (msifile == null)
            {
                throw new InstallerVerificationLibraryException("Could not find a msi file with id '" + msiid + "'");
            }

            return msifile;
        }
    }
}