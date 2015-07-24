namespace InstallerVerificationLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using InstallerVerificationLibrary.Logging;
    using InstallerVerificationLibrary.Tools;
    using InstallerVerificationLibrary.Verifiers;

    public static class DataPropertyTool
    {
        public static ICollection<DataProperty> ExtractProperties(XElement node, ICollection<Parameter> parameters)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var properties = node.Descendants().Where(x => x.Name.LocalName == "property").ToList();
            if (properties.Count < 1)
            {
                return new Collection<DataProperty>();
            }

            var proplist = new Collection<DataProperty>();

            CheckForMissSpelledPropertyTypes(properties);
            CollectionTools.MergeCollection(proplist, ReadTextValue(properties), true);
            CollectionTools.MergeCollection(proplist, ReadWindowsInstallerValue(properties), true);
            CollectionTools.MergeCollection(proplist, ReadSpecialFolderPath(properties), true);
            CollectionTools.MergeCollection(proplist, ReadEnvironmentVariable(properties), true);
            CollectionTools.MergeCollection(proplist, ReadRegistryPathExistValue(properties), true);
            CollectionTools.MergeCollection(proplist, ReadRegistryValue(properties), true);
            CollectionTools.MergeCollection(proplist, ReadParameter(properties, parameters), true);

            return proplist;
        }
        
        private static void CheckForMissSpelledPropertyTypes(ICollection<XElement> properties)
        {
            var allowedTypes = new Collection<string>
            {
                DataPropertyType.EnvironmentVariable.ToString().ToUpperInvariant(),
                DataPropertyType.Text.ToString().ToUpperInvariant(),
                DataPropertyType.Parameter.ToString().ToUpperInvariant(),
                DataPropertyType.RegistryPathExist.ToString().ToUpperInvariant(),
                DataPropertyType.RegistryValue.ToString().ToUpperInvariant(),
                DataPropertyType.SpecialFolderPath.ToString().ToUpperInvariant(),
                DataPropertyType.WindowsInstaller.ToString().ToUpperInvariant()
            };

            foreach (var property in properties)
            {
                var type = XmlTools.GetNamedAttributeValue(property, "type", string.Empty).ToUpperInvariant();
                if (allowedTypes.Contains(type))
                {
                    continue;
                }

                var msg = "Wrong type attribute in Property Node: '" + property.CreateNavigator().OuterXml + "'";
                Log.WriteError(msg, "ExtractProperties");
                throw new InstallerVerificationLibraryException(msg);
            }
        }

        private static Collection<DataProperty> ReadWindowsInstallerValue(ICollection<XElement> properties)
        {
            var proplist = new Collection<DataProperty>();
            foreach (var property in GetPropertyNodesWithSpecificType(properties, DataPropertyType.WindowsInstaller))
            {
                var prop = new DataProperty
                            {
                                Id = XmlTools.GetNamedAttributeValue(property, "id", null),
                                PropertyType = DataPropertyType.WindowsInstaller,
                                Value = XmlTools.GetNamedAttributeValue(property, "valueIfNotFound", null),
                            };

                if (prop.Id == null)
                {
                    var msg = "Missing 'id' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                proplist.Add(prop);
            }

            WindowsInstallerPropertyExtractor.CacheProperties(proplist);

            foreach (var prop in proplist)
            {
                var value = WindowsInstallerPropertyExtractor.GetValueForProperty(prop);
                if (!string.IsNullOrEmpty(value))
                {
                    prop.Value = value;
                }
                else if (prop.Value == null)
                {
                    var msg = "Missing 'valueIfNotFound' attribute for property '" + prop.Id + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }
            }

            return proplist;
        }

        private static Collection<DataProperty> ReadTextValue(ICollection<XElement> properties)
        {
            var proplist = new Collection<DataProperty>();

            foreach (var property in GetPropertyNodesWithSpecificType(properties, DataPropertyType.Text))
            {
                var prop = new DataProperty
                               {
                                   Id = XmlTools.GetNamedAttributeValue(property, "id", null),
                                   PropertyType = DataPropertyType.Text,
                                   Value = XmlTools.GetNamedAttributeValue(property, "value", null)
                               };

                if (prop.Id == null)
                {
                    var msg = "Missing 'id' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                if (prop.Value == null)
                {
                    var msg = "Missing 'value' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                proplist.Add(prop);
            }

            return proplist;
        }

        private static IEnumerable<XElement> GetPropertyNodesWithSpecificType(ICollection<XElement> properties, DataPropertyType type)
        {
            return properties.Where(property => string.Equals(XmlTools.GetNamedAttributeValue(property, "type", string.Empty), type.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }

        private static Collection<DataProperty> ReadRegistryPathExistValue(ICollection<XElement> properties)
        {
            var proplist = new Collection<DataProperty>();
            foreach (var property in GetPropertyNodesWithSpecificType(properties, DataPropertyType.RegistryPathExist))
            {
                var prop = new DataProperty
                               {
                                   Id = XmlTools.GetNamedAttributeValue(property, "id", null),
                                   PropertyType = DataPropertyType.RegistryPathExist
                               };
                var keyName = XmlTools.GetNamedAttributeValue(property, "key", null);
                
                if (prop.Id == null)
                {
                    var msg = "Missing 'id' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                if (keyName == null)
                {
                    var msg = "Missing 'value' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }
                
                prop.Value = RegistryTool.RegistryKeyExist(keyName).ToString();
                proplist.Add(prop);
            }

            return proplist;
        }

        private static Collection<DataProperty> ReadRegistryValue(ICollection<XElement> properties)
        {
            var proplist = new Collection<DataProperty>();
            foreach (var property in GetPropertyNodesWithSpecificType(properties, DataPropertyType.RegistryValue))
            {
                var prop = new DataProperty
                               {
                                   Id = XmlTools.GetNamedAttributeValue(property, "id", null),
                                   PropertyType = DataPropertyType.RegistryValue
                               };

                var keyName = XmlTools.GetNamedAttributeValue(property, "key", null);
                var valueName = XmlTools.GetNamedAttributeValue(property, "valueName", null);
                var valueIfNotFound = XmlTools.GetNamedAttributeValue(property, "valueIfNotFound", null);
                
                if (prop.Id == null)
                {
                    var msg = "Missing 'id' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                if (string.IsNullOrEmpty(keyName))
                {
                    var msg = "Missing 'key' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                if (string.IsNullOrEmpty(valueName))
                {
                    var msg = "Missing 'valueName' attribute in Property Node: '" + XmlTools.GetOuterXml(property) +
                              "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                var value = (string) RegistryTool.RegistryValue(keyName, valueName, null);

                if (value == null)
                {
                    if (valueIfNotFound == null)
                    {
                        var msg = "Missing 'valueIfNotFound' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                        Log.WriteError(msg, "ExtractProperties");
                        throw new InstallerVerificationLibraryException(msg);
                    }

                    prop.Value = valueIfNotFound;
                }
                else
                {
                    prop.Value = value;
                }

                proplist.Add(prop);
            }

            return proplist;
        }

        private static Collection<DataProperty> ReadEnvironmentVariable(ICollection<XElement> properties)
        {
            var proplist = new Collection<DataProperty>();
            foreach (var property in GetPropertyNodesWithSpecificType(properties, DataPropertyType.EnvironmentVariable))
            {
                var prop = new DataProperty
                               {
                                   Id = XmlTools.GetNamedAttributeValue(property, "id", null),
                                   PropertyType = DataPropertyType.EnvironmentVariable,
                                   Value = XmlTools.GetNamedAttributeValue(property, "value", null)
                               };

                if (prop.Id == null)
                {
                    var msg = "Missing 'id' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                if (prop.Value != null)
                {
                    prop.Value = Environment.GetEnvironmentVariable(prop.Value);
                    proplist.Add(prop);
                }
                else
                {
                    var msg = "Missing 'value' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }
            }

            return proplist;
        }

        private static Collection<DataProperty> ReadSpecialFolderPath(ICollection<XElement> properties)
        {
            var proplist = new Collection<DataProperty>();
            foreach (var property in GetPropertyNodesWithSpecificType(properties, DataPropertyType.SpecialFolderPath))
            {
                var prop = new DataProperty
                               {
                                   Id = XmlTools.GetNamedAttributeValue(property, "id", null),
                                   PropertyType = DataPropertyType.SpecialFolderPath,
                                   Value = null
                               };

                if (prop.Id == null)
                {
                    var msg = "Missing 'id' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                var value = XmlTools.GetNamedAttributeValue(property, "value", null);
                if (value != null)
                {
                    foreach (int specialFolderIndex in Enum.GetValues(typeof(Environment.SpecialFolder)))
                    {
                        if (((Environment.SpecialFolder)specialFolderIndex).ToString() == value)
                        {
                            prop.Value = Environment.GetFolderPath((Environment.SpecialFolder) specialFolderIndex);
                            break;
                        }
                    }

                    if (prop.Value != null)
                    {
                        proplist.Add(prop);
                    }
                    else
                    {
                        var msg = "Could not find folder path '" + value + "' in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                        Log.WriteError(msg, "ExtractProperties");
                        throw new InstallerVerificationLibraryException(msg);
                    }
                }
                else
                {
                    var msg = "Missing 'value' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }
            }

            return proplist;
        }

        private static ICollection<DataProperty> ReadParameter(ICollection<XElement> properties,
            ICollection<Parameter> parameters)
        {
            var proplist = new Collection<DataProperty>();
            var proplistError = new Collection<DataProperty>();

            foreach (var property in GetPropertyNodesWithSpecificType(properties, DataPropertyType.Parameter))
            {
                var prop = new DataProperty
                               {
                                   Id = XmlTools.GetNamedAttributeValue(property, "id", null),
                                   PropertyType = DataPropertyType.Parameter
                               };

                if (prop.Id == null)
                {
                    var msg = "Missing 'id' attribute in Property Node: '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }

                var parameterId = XmlTools.GetNamedAttributeValue(property, "parameterID", null);
                if (!string.IsNullOrEmpty(parameterId))
                {
                    var found = false;
                    foreach (var parm in parameters.Where(parm => parm.Id == parameterId))
                    {
                        prop.Value = parm.Value;
                        proplist.Add(prop);
                        found = true;
                        break;
                    }

                    if (found)
                    {
                        //// If found delete if property exist in error list
                        if (proplistError.Contains(prop))
                        {
                            proplistError.Remove(prop);
                        }
                    }
                    else
                    {
                        //// If not found and it doesn't allready exist add property to error list
                        if (!proplist.Contains(prop))
                        {
                            prop.Value = XmlTools.GetOuterXml(property);
                            proplistError.Add(prop);
                        }
                    }
                }
                else
                {
                    var msg = "parameterID attribute is missing for property '" + XmlTools.GetOuterXml(property) + "'";
                    Log.WriteError(msg, "ExtractProperties");
                    throw new InstallerVerificationLibraryException(msg);
                }
            }

            //// Check if any error exist
            if (proplistError.Count > 0)
            {
                var msg = proplistError.Aggregate(string.Empty, (current, prop) => current + ("Property Node: '" + prop.Value + "' doesn't contain a valid parameterId"));

                Log.WriteError(msg, "ExtractProperties");
                throw new InstallerVerificationLibraryException(msg);
            }

            return proplist;
        }

        public static void ResolveProperties(ICollection<DataProperty> properties)
        {
            RecursiveResolveProperty(properties, 1);
        }

        public static string ResolvePropertiesInString(ICollection<DataProperty> properties, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            return RecursiveResolvePropertiesInString(properties, value, 1);
        }

        private static void RecursiveResolveProperty(ICollection<DataProperty> properties, int level)
        {
            if (level < 0)
            {
                throw new ArgumentException("Level can't be negative", "level");
            }
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            var continueSearch = false;

            foreach (var entry in properties.Where(entry => !string.IsNullOrEmpty(entry.Value)).Where(entry => entry.Value.Contains("[") && entry.Value.Contains("]")))
            {
                foreach (var entry2 in properties)
                {
                    entry.Value = entry.Value.Replace("[" + entry2.Id + "]", entry2.Value);
                }

                if (entry.Value.Contains("[") && entry.Value.Contains("]"))
                {
                    continueSearch = true;
                }
            }

            if (!continueSearch)
            {
                return;
            }

            if (level > 20)
            {
                var exceptionString = properties.Aggregate(string.Empty, (current, entry3) => current + (entry3.Id + "->" + entry3.Value + "**"));

                Log.WriteError("Could not resolve all properties" + exceptionString, "RecursiveResolveProperty");
                throw new InstallerVerificationLibraryException("Could not resolve all properties" + exceptionString);
            }
            RecursiveResolveProperty(properties, ++level);
        }

        private static string RecursiveResolvePropertiesInString(ICollection<DataProperty> properties, string value,
            int level)
        {
            if (level < 0)
            {
                throw new ArgumentException("Level can't be negative", "level");
            }
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            var continueSearch = false;
            if (value.Contains("[") && value.Contains("]"))
            {
                value = properties.Aggregate(value, (current, entry) => current.Replace("[" + entry.Id + "]", entry.Value));

                if (value.Contains("[") && value.Contains("]"))
                {
                    if (value.IndexOf("[", StringComparison.OrdinalIgnoreCase) !=
                        value.LastIndexOf("[", StringComparison.OrdinalIgnoreCase) && !value.Contains("[!]"))
                    {
                        continueSearch = true;
                    }
                }
            }

            if (!continueSearch)
            {
                return value;
            }

            if (level > 20)
            {
                Log.WriteError("Could not resolve all properties for '" + value + "'",
                    "RecursiveResolvePropertiesInString");
                return value;
            }

            value = RecursiveResolvePropertiesInString(properties, value, ++level);

            return value;
        }
    }
}