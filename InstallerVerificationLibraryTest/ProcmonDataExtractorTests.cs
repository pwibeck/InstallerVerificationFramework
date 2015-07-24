using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstallerTestingLibraryTest
{
    using InstallerVerificationLibrary;

    [TestClass]
    public class ProcmonDataExtractorTests
    {
        [TestMethod]
        [DeploymentItem(@"TestTraceData.xml")]
        public void GetEventlist_RepairScenario()
        {
            var eventList = ProcmonDataExtractor.GetEventlist("TestTraceData.xml");

            ProcmonDataExtractor extractor = new ProcmonDataExtractor();
            extractor.ExtractData(ProcmonDataExtractor.TypeOfInstaller.Msi, null, eventList);

            Assert.AreEqual(5, extractor.FileChanges.ToList().Count);
            Assert.IsFalse(extractor.FileChanges.Any(x=> x.Type == FileChangeType.Delete));
            Assert.IsFalse(extractor.FileChanges.Any(x => x.Type == FileChangeType.Rename));
        }
    }
}
