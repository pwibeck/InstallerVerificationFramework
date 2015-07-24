namespace InstallerVerificationLibrary.Loader
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using InstallerVerificationLibrary.Data;
    using InstallerVerificationLibrary.Logging;

    public abstract class BaseTestDataLoader : ITestDataLoader
    {
        private const string JscriptSource =
            @"package Evaluator
            {
               class Evaluator
               {
                  public function Eval(expr : String) : String 
                  { 
                     return eval(expr); 
                  }
               }
            }";

        private static Assembly evaluatorAssembly;
        
        protected BaseTestDataLoader()
        {
            if (evaluatorAssembly == null)
            {
                using (var provider = CodeDomProvider.CreateProvider("JScript"))
                {
                    var parameters = new CompilerParameters {GenerateInMemory = true};

                    evaluatorAssembly = provider.CompileAssemblyFromSource(parameters, JscriptSource).CompiledAssembly;
                }
            }

            ElementName = CheckerDataTypeLoader.FindDataType(GetType());
        }

        protected static void AddCommonData(XElement node, BaseTestData data, ICollection<DataProperty> properties)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            var neverUninstall = XmlTools.GetNamedAttributeValue(node, "neverUnInstall", string.Empty);
            data.NeverUninstall = !string.IsNullOrEmpty(neverUninstall) && bool.Parse(neverUninstall);

            var condition = XmlTools.GetNamedAttributeValue(node, "condition", string.Empty);
            if (!string.IsNullOrEmpty(condition))
            {
                //// Example condition ([VersionNT] &gt; 600) OR ([VersionNT64] &gt; 600)
                var conditionBakup = condition;
                condition = DataPropertyTool.ResolvePropertiesInString(properties, condition);
                condition = condition.Replace("&gt;", ">");
                condition = condition.Replace("&lt;", "<");
                condition = condition.Replace(" OR ", " || ");
                condition = condition.Replace(" AND ", " && ");

                var evalType = evaluatorAssembly.GetType("Evaluator.Evaluator");
                var evaluator = Activator.CreateInstance(evalType);

                string result;
                try
                {
                    result =
                        evalType.InvokeMember("Eval", BindingFlags.InvokeMethod, null, evaluator, new object[] {condition},
                            CultureInfo.InvariantCulture).ToString();
                }
                catch (TargetInvocationException e)
                {
                    Log.WriteError(conditionBakup + " resolved to " + condition, "AddCommonData");
                    Log.WriteError(e, "AddCommonData");
                    throw new InstallerVerificationLibraryException(
                        "Evaluating condition '" + conditionBakup + "' resolved to '" + condition + "' failed", e);
                }

                bool res;
                if (!bool.TryParse(result, out res))
                {
                    throw new InstallerVerificationLibraryException("Could not evaluate condition '" + condition + "' in node '" + node + "'");
                }

                data.Condition = res;
            }
            else
            {
                data.Condition = true;
            }
        }

        protected void CheckForAdditionalAttributes(XElement node, Collection<string> allowedAttributeNames)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (allowedAttributeNames == null)
            {
                throw new ArgumentNullException("allowedAttributeNames");
            }

            allowedAttributeNames.Add("neverUnInstall");
            allowedAttributeNames.Add("condition");

            var errorFound = false;
            foreach (var attribute in node.Attributes())
            {
                if (allowedAttributeNames.Any(name => string.Compare(attribute.Name.LocalName, name, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    continue;
                }

                Log.WriteEntryError(
                    "'" + attribute.Name.LocalName + "' is not an allowed attribute. Node:'" + node.CreateNavigator().OuterXml + "' \n",
                    ElementName);
                errorFound = true;
            }

            if (errorFound)
            {
                throw new InstallerVerificationLibraryException("Did find additional attributes. More information is found in the log.");
            }
        }

        protected string ElementName { get; set; }

        public string Name
        {
            get
            {
                return ElementName;
            }
        }

        public abstract ICollection<BaseTestData> ExtractData(XElement xmlNode, ICollection<DataProperty> properties);
    }
}