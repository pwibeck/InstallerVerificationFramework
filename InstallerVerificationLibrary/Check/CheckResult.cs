namespace InstallerVerificationLibrary.Check
{
    using InstallerVerificationLibrary.Data;

    public class CheckResult
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public string InfomrationMessage { get; set; }

        public static CheckResult NotCheckDone()
        {
            return new CheckResult { Success = true };
        }

        public static CheckResult Succeeded(BaseTestData data)
        {
            return new CheckResult { Success = true, InfomrationMessage = data.ToString()};
        }

        public static CheckResult Failure(string errorMesasge)
        {
            return new CheckResult { Success = false, ErrorMessage = errorMesasge};
        }
    }
}
