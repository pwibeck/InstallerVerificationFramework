namespace InstallerVerificationLibrary.Data
{
    public abstract class BaseTestData
    {
        public string ComponentID { get; set; }

        public bool Exist { get; set; }

        public bool NeverUninstall { get; set; }

        private bool condition = true;

        public bool Condition
        {
            get { return condition; }
            set { condition = value; }
        }

        public abstract override string ToString();
    }
}
