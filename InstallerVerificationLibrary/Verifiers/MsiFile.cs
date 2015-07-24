namespace InstallerVerificationLibrary.Verifiers
{
    public class MsiFile
    {
        public string Path { get; set; }
        
        public string  Id { get; set; }
        
        public override string ToString()
        {
            return "ID:" + Id + ", Path:" + Path;
        }
    }
}
