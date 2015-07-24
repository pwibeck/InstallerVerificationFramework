namespace InstallerVerificationLibrary
{
    using System.Collections.ObjectModel;
    using System.Linq;

    public enum TypeOfInstallation
    {
        Install,
        UnInstall,
        Repair
    }

    public class SetupConfig
    {
        private readonly Collection<SetupConfigComponentData> componentList = new Collection<SetupConfigComponentData>();
        private readonly Collection<SetupConfigParameterData> parameterList = new Collection<SetupConfigParameterData>();

        public Collection<SetupConfigComponentData> ComponentList
        {
            get { return componentList; }
        }

        public Collection<SetupConfigParameterData> ParameterList
        {
            get { return parameterList; }
        }

        public TypeOfInstallation TypeOfInstallation { get; set; }
        
        protected SetupConfigComponentData GetComponent(string id)
        {
            return ComponentList.FirstOrDefault(comp => comp.Id == id);
        }

        protected SetupConfigParameterData GetParameter(string id)
        {
            return ParameterList.FirstOrDefault(parm => parm.Id == id);
        }
    }
}
