namespace InstallerVerificationLibrary
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;

    public static class XmlTools
    {
        public static string GetNamedAttributeValue(XElement node, string attributeName, string defaultValue)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            
            var attribute = node.Attributes().FirstOrDefault(x => x.Name.LocalName == attributeName);
            return attribute == null ? defaultValue : attribute.Value;
        }

        public static string GetOuterXml(XElement node)
        {
            var xPathNavigator = node.CreateNavigator();
            var outerXml = String.Empty;
            if (xPathNavigator != null)
            {
                outerXml = xPathNavigator.OuterXml;
            }
            return outerXml;
        }
    }
}