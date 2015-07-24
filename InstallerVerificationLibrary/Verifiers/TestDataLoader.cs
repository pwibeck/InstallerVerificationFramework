namespace InstallerTestingToolset.Verifiers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using InstallerVerificationLibrary;
    using InstallerVerificationLibrary.Data;
    using InstallerVerificationLibrary.Loader;
    using InstallerVerificationLibrary.Logging;
    using InstallerVerificationLibrary.Verifiers;

    public class TestDataLoader
    {
        public static Collection<BaseTestData> LoadTestDataFromFile(string filePathToConfigurationData, SetupConfig setupConfig)
        {
            filePathToConfigurationData = CheckFilePath(filePathToConfigurationData);
            var parameters = GetParameters(setupConfig);

            var xdoc = XElement.Load(filePathToConfigurationData);
            ResolveIncludes(filePathToConfigurationData, xdoc, 1);

            var properties = LoadProperties(xdoc, filePathToConfigurationData, parameters);
            var msifiles = LoadMsiFileData(xdoc, filePathToConfigurationData);
            var plugins = LoadPlugins(xdoc, filePathToConfigurationData);

            switch (xdoc.Name.LocalName.ToLowerInvariant())
            {
                case "msi":
                    var features = new Collection<Feature>();
                    foreach (var feature in LoadFeatureDataFromXmlNode(xdoc, properties, null, msifiles, plugins))
                    {
                        features.Add(feature);
                    }

                    return CollectTestDataFromFeatures(setupConfig, features);
                case "installchainer":
                    var components = LoadInstallChainerComponentData(xdoc, filePathToConfigurationData, msifiles, plugins, properties);
                    return CollectTestDataFromInstallChainerComponents(setupConfig, components);
            }

            return new Collection<BaseTestData>();
        }

        public static Collection<PluginData> LoadPluginsDataFromFile(string filePathToConfigurationData)
        {
            filePathToConfigurationData = CheckFilePath(filePathToConfigurationData);

            var xdoc = XElement.Load(filePathToConfigurationData);
            ResolveIncludes(filePathToConfigurationData, xdoc, 1);

            return LoadPlugins(xdoc, filePathToConfigurationData);
        }

        public static void ResolveSetupConfigParameters(string filePathToConfigurationData, SetupConfig setupConfig)
        {
            filePathToConfigurationData = CheckFilePath(filePathToConfigurationData);

            var xdoc = XElement.Load(filePathToConfigurationData);
            ResolveIncludes(filePathToConfigurationData, xdoc, 1);

            var properties = LoadProperties(xdoc, filePathToConfigurationData, GetParameters(setupConfig));

            foreach (var setupConfigParameterData in setupConfig.ParameterList)
            {
                setupConfigParameterData.Value = DataPropertyTool.ResolvePropertiesInString(properties, setupConfigParameterData.Value);
            }
        }

        private static string CheckFilePath(string filePathToConfigurationData)
        {
            // If not full path add full path data    
            if (!filePathToConfigurationData.Contains(@":\"))
            {
                filePathToConfigurationData = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, filePathToConfigurationData);
            }

            if (!File.Exists(filePathToConfigurationData))
            {
                throw new ArgumentException("filePathToConfigurationData doesn't exist on file system");
            }
            return filePathToConfigurationData;
        }

        private static void ResolveIncludes(string filePathToConfigurationData, XElement xdoc, int recursiveCounter)
        {
            if (recursiveCounter > 3)
            {
                throw new InstallerVerificationLibraryException(
                    "Recursive loading data on a level deeper than 3 is not allowed");
            }

            recursiveCounter++;

            var includeList = xdoc.Descendants().Where(x => x.Name.LocalName == "include").ToList();
            foreach (var include in includeList)
            {
                var referencePath = XmlTools.GetNamedAttributeValue(include, "ref", null);
                if (String.IsNullOrEmpty(referencePath))
                {
                    throw new InstallerVerificationLibraryException("ref attribute doesn't exists for all include in file:" +
                                                               filePathToConfigurationData);
                }

                var resolvedReferencePath = ResolveRelativePaths(referencePath, filePathToConfigurationData);
                if (!File.Exists(resolvedReferencePath))
                {
                    throw new InstallerVerificationLibraryException("Could not find include file:" + referencePath);
                }

                var referencedxDoc = XElement.Load((string)resolvedReferencePath);
                if (referencedxDoc.Descendants().Any(x=>x.Name.LocalName == "include"))
                {
                    ResolveIncludes(filePathToConfigurationData, referencedxDoc, recursiveCounter);
                }

                if (referencedxDoc.Name.LocalName.ToLowerInvariant() == "installchainer" && xdoc.Name.LocalName.ToLowerInvariant() == "installchainer")
                {
                    foreach (var chainerElement in referencedxDoc.Elements())
                    {
                        AddIncludeElements(xdoc, chainerElement);
                    }
                }
                else
                {
                    AddIncludeElements(xdoc, referencedxDoc);
                }

                include.Remove();
            }
        }

        private static void AddIncludeElements(XElement xdoc, XElement includeElement)
        {
            XElement existingNode = null;
            if (includeElement.Name.LocalName == "properties" ||
                includeElement.Name.LocalName == "msifiles" ||
                includeElement.Name.LocalName == "components" ||
                includeElement.Name.LocalName == "plugins")
            {
                existingNode = xdoc.Descendants().FirstOrDefault(x => x.Name.LocalName == includeElement.Name.LocalName);
            }

            if (existingNode != null)
            {
                existingNode.Add(includeElement.Elements());
            }
            else
            {
                xdoc.Add(includeElement);
            }
        }

        private static Collection<Parameter> GetParameters(SetupConfig setupConfig)
        {
            var parameters = new Collection<Parameter>();
            foreach (var baseData in setupConfig.ParameterList)
            {
                var configParameter = baseData;
                var param = new Parameter
                {
                    Id = configParameter.Id,
                    Value = configParameter.Value
                };
                parameters.Add(param);
            }

            return parameters;
        }

        private static Collection<InstallChainerComponent> LoadInstallChainerComponentData(XElement xdoc, string filePath, ICollection<MsiFile> msifiles, ICollection<PluginData> plugins, ICollection<DataProperty> properties)
        {
            var components = new Collection<InstallChainerComponent>();
            foreach (var comp in xdoc.Descendants().Where(x => x.Name.LocalName == "component").Select(componentNode => CreateInstallChainerComponent(componentNode, filePath, msifiles, plugins, properties)).Where(comp => components.All(x => x.Id != comp.Id)))
            {
                components.Add(comp);
            }

            return components;
        }

        private static InstallChainerComponent CreateInstallChainerComponent(XElement componentNode, string filePath, ICollection<MsiFile> msifiles, ICollection<PluginData> plugins, ICollection<DataProperty> properties)
        {
            var comp = new InstallChainerComponent
            {
                Id = XmlTools.GetNamedAttributeValue(componentNode, "id", null),
                InstallationDir = XmlTools.GetNamedAttributeValue(componentNode, "installDir", null)
            };

            // Not mandatory if all featuers have there own installDir
            // Feature installDir does overull component installDir if it does exist

            if (comp.Id == null)
            {
                throw new InstallerVerificationLibraryException("Component is missing id attribute in file:" + filePath +
                                                           ". Data:" + componentNode.CreateNavigator().OuterXml);
            }

            var loader = new MsiFeatureTestDataLoader
            {
                Msifiles = msifiles,
                Plugins = plugins,
                ComponentId = comp.Id
            };
            
            foreach (var item in loader.ExtractData(componentNode, properties))
            {
                comp.TestData.Add(item);
            }

            return comp;
        }

        private static Collection<MsiFile> LoadMsiFileData(XElement xdoc, string filePath)
        {
            var msiFiles = new Collection<MsiFile>();
            foreach (var msifileNode in xdoc.Descendants().Where(x => x.Name.LocalName == "msifile"))
            {
                var file = new MsiFile
                {
                    Id = XmlTools.GetNamedAttributeValue(msifileNode, "id", null),
                    Path = XmlTools.GetNamedAttributeValue(msifileNode, "path", null)
                };

                if (String.IsNullOrWhiteSpace(file.Id))
                {
                    throw new InstallerVerificationLibraryException("MSI file is misisng 'id' attribute. Referenced in '" + filePath + "'");
                }

                if (String.IsNullOrWhiteSpace(file.Path))
                {
                    throw new InstallerVerificationLibraryException("MSI file is misisng 'path' attribute. Referenced in '" + filePath + "'");
                }

                file.Path = ResolveRelativePaths(file.Path, filePath);
                if (!File.Exists(file.Path))
                {
                    throw new InstallerVerificationLibraryException("Could not find msi file:" + file.Path + " referenced in '" + filePath + "'");
                }

                msiFiles.Add(file);
            }

            return msiFiles;
        }

        private static Collection<DataProperty> LoadProperties(XElement xdoc, string filePath, Collection<Parameter> parameters)
        {
            var propertiesResult = new Collection<DataProperty>();
            foreach (var properties in xdoc.Descendants().Where(x => x.Name.LocalName == "properties"))
            {
                try
                {
                    CollectionTools.MergeCollection(propertiesResult, DataPropertyTool.ExtractProperties(properties, parameters), true);
                }
                catch (Exception e)
                {
                    throw new InstallerVerificationLibraryException("Could not load properties referenced in '" + filePath +
                                                               "' due to:" + e.Message);
                }

            }

            DataPropertyTool.ResolveProperties(propertiesResult);

            return propertiesResult;
        }

        private static Collection<PluginData> LoadPlugins(XElement xdoc, string filePath)
        {
            var pluginsResult = new Collection<PluginData>();
            foreach (var pluginNode in xdoc.Descendants().Where(x => x.Name.LocalName == "plugin"))
            {
                var plugin = new PluginData
                {
                    DataType = XmlTools.GetNamedAttributeValue(pluginNode, "datatype", null),
                    PluginDllPath = XmlTools.GetNamedAttributeValue(pluginNode, "path", null)
                };

                if (String.IsNullOrEmpty(plugin.DataType))
                {
                    throw new InstallerVerificationLibraryException(
                        "Plugin node doesn't contain 'datatype' attribute. Node:'" +
                        pluginNode.CreateNavigator().OuterXml + "' in file '" + filePath + "'");
                }

                var path = ResolveRelativePaths(plugin.PluginDllPath, filePath);
                if (!File.Exists(path))
                {
                    throw new InstallerVerificationLibraryException("Could not find '" + path + "' referenced in file '" +
                                                                    filePath + "'");
                }

                pluginsResult.Add(plugin);
            }

            return pluginsResult;
        }

        private static Collection<BaseTestData> CollectTestDataFromFeatures(SetupConfig setupConfig, ICollection<Feature> features)
        {
            if (setupConfig == null)
            {
                throw new InstallerVerificationLibraryException("setupConfiguration can't be null");
            }

            var testDataCollection = new Collection<BaseTestData>();

            foreach (var setupConfigcomponentData in setupConfig.ComponentList)
            {
                Feature feature;

                try
                {
                    feature = features.SingleOrDefault(x => x.FeatureName == setupConfigcomponentData.Id);
                }
                catch (Exception)
                {
                    throw new InstallerVerificationLibraryException("Multiple features with name '" + setupConfigcomponentData.Id + "' found. Feature name need to be uniqe.");
                }

                if (feature == null)
                {
                    throw new InstallerVerificationLibraryException("Could not find data for feature '" + setupConfigcomponentData.Id + "'. This could be because miss spelling of feature name.");
                }

                if (feature.FeatureData.Count == 0)
                {
                    throw new InstallerVerificationLibraryException("Feature '" + feature.FeatureName + "' doesn't have any feature data. This could be because miss spelling of feature name.");
                }

                if (!setupConfigcomponentData.Installed)
                {
                    continue;
                }

                foreach (var data in feature.FeatureData)
                {
                    data.ComponentID = feature.FeatureName;
                    if (setupConfig.TypeOfInstallation == TypeOfInstallation.Install ||
                        setupConfig.TypeOfInstallation == TypeOfInstallation.Repair)
                    {
                        AddTestData(true, data, testDataCollection);
                    }
                    else if (setupConfig.TypeOfInstallation == TypeOfInstallation.UnInstall)
                    {
                        AddTestData(false, data, testDataCollection);
                    }
                }
            }

            return testDataCollection;
        }

        private static Collection<BaseTestData> CollectTestDataFromInstallChainerComponents(SetupConfig setupConfig, ICollection<InstallChainerComponent> components)
        {
            if (setupConfig == null)
            {
                throw new InstallerVerificationLibraryException("setupConfiguration can't be null");
            }

            var testDataCollection = new Collection<BaseTestData>();

            foreach (var setupConfigcomponentData in setupConfig.ComponentList)
            {
                InstallChainerComponent component;

                try
                {
                    component = components.SingleOrDefault(x => x.Id == setupConfigcomponentData.Id);
                }
                catch (Exception)
                {
                    throw new InstallerVerificationLibraryException("Multiple components with name '" + setupConfigcomponentData.Id + "' found. Component name need to be uniqe.");
                }

                if (component == null)
                {
                    throw new InstallerVerificationLibraryException("Could not find data for component '" + setupConfigcomponentData.Id + "'. This could be because miss spelling of component name.");
                }

                if (component.TestData.Count == 0)
                {
                    throw new InstallerVerificationLibraryException("Component '" + setupConfigcomponentData.Id + "' doesn't have any testdata data. This could be because miss spelling of component name.");
                }

                if (!setupConfigcomponentData.Installed)
                {
                    continue;
                }

                foreach (var data in component.TestData)
                {
                    if (setupConfig.TypeOfInstallation == TypeOfInstallation.Install ||
                        setupConfig.TypeOfInstallation == TypeOfInstallation.Repair)
                    {
                        AddTestData(true, data, testDataCollection);
                    }
                    else if (setupConfig.TypeOfInstallation == TypeOfInstallation.UnInstall)
                    {
                        AddTestData(false, data, testDataCollection);
                    }
                }
            }

            return testDataCollection;
        }

        public static void AddTestData(bool expectedToBeInstalled, BaseTestData data, ICollection<BaseTestData> testDataCollection)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            data.Exist = expectedToBeInstalled && data.Condition;

            // Avoid double copy
            if (testDataCollection.Contains(data))
            {
                return;
            }

            testDataCollection.Add(data);
        }

        public static Collection<Feature> LoadFeatureDataFromXmlNode(XElement xdoc, ICollection<DataProperty> properties, string featureName, ICollection<MsiFile> msifiles, ICollection<PluginData> plugins)
        {
            var testDataLoaders = new Collection<ITestDataLoader>();

            var asm = new CheckerDataTypeLoader(plugins);
            foreach (var loader in asm.GetAllLoaders())
            {
                testDataLoaders.Add(loader);
            }

            var internalPropertyCollection = CollectionTools.CopyCollection(properties);
            DataPropertyTool.ResolveProperties(internalPropertyCollection);

            var features = new Collection<Feature>();

            foreach (var featureNode in xdoc.Descendants().Where(x => x.Name.LocalName == "feature"))
            {
                var feature = new Feature
                                  {
                                      FeatureName = XmlTools.GetNamedAttributeValue(featureNode, "name", null)
                                  };
                if (featureName != null && featureName != feature.FeatureName)
                {
                    continue;
                }

                foreach (var loader in testDataLoaders)
                {
                    try
                    {
                        var mentry = loader as MsiFeatureTestDataLoader;
                        if (mentry != null)
                        {
                            ((MsiFeatureTestDataLoader)loader).Msifiles = msifiles;
                            ((MsiFeatureTestDataLoader)loader).Plugins = plugins;
                        }

                        foreach (var item in loader.ExtractData(featureNode, internalPropertyCollection))
                        {
                            feature.FeatureData.Add(item);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.WriteError(e, "From method LoadFeatureDataFromXmlNode. An exception was thrown when executing the ExtractData method on loader:'" + loader.Name + "'");
                        throw;
                    }
                }

                features.Add(feature);
            }

            return features;
        }

        private static string ResolveRelativePaths(string filePath, string referenceInFilePath)
        {
            //// If not full path add full path data relative to the file it's referenced from
            if (!filePath.Contains(@":\"))
            {
                FileInfo fil = new FileInfo(referenceInFilePath);
                filePath = Path.Combine(fil.DirectoryName, filePath);
            }

            return filePath;
        }
    }
}
