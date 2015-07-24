namespace InstallerTestingLibraryTest.Tools
{
    using InstallerVerificationLibrary;
    using InstallerVerificationLibrary.Tools;
    using Microsoft.Win32;
    using Xunit;

    public class RegistryToolTests
    {
        [Fact]
        public void RemoveRegistryKey_LongLocalMachineNameExist_KeyIsRemoved()
        {
            // Setup
            var key = @"SOFTWARE\test";
            Registry.LocalMachine.CreateSubKey(key);

            // Execute
            RegistryTool.RemoveRegistryKey(@"HKEY_LOCAL_MACHINE\" + key);
            
            // Validate
            Assert.Null(Registry.LocalMachine.OpenSubKey(key));

            // Cleanup
            Registry.LocalMachine.DeleteSubKeyTree(key, false);
        }

        [Fact]
        public void RemoveRegistryKey_ShortLocalMachineNameExist_KeyIsRemoved()
        {
            // Setup
            var key = @"SOFTWARE\test";
            Registry.LocalMachine.CreateSubKey(key);

            // Execute
            RegistryTool.RemoveRegistryKey(@"HKLM\" + key);

            // Validate
            Assert.Null(Registry.LocalMachine.OpenSubKey(key));

            // Cleanup
            Registry.LocalMachine.DeleteSubKeyTree(key, false);
        }

        [Fact]
        public void RemoveRegistryKey_MisspelledHive_ThrowException()
        {
            var exception = Assert.Throws<InstallerVerificationLibraryException>(
                () => RegistryTool.RemoveRegistryKey(@"HHKLM\SOFTWARE\test"));
            Assert.Equal(@"Could not find hive for key:HHKLM\SOFTWARE\test", exception.Message);
        }

        [Fact]
        public void RegistryKeyExist_LongLocalMachineNameExist_ReturnTrue()
        {
            // Setup
            var key = @"SOFTWARE\test";
            Registry.LocalMachine.CreateSubKey(key);

            // Execute
            var result = RegistryTool.RegistryKeyExist(@"HKEY_LOCAL_MACHINE\" + key);

            // Validate
            Assert.True(result);

            // Cleanup
            Registry.LocalMachine.DeleteSubKeyTree(key, false);
        }

        [Fact]
        public void RegistryKeyExist_ShortLocalMachineNameExist_ReturnTrue()
        {
            // Setup
            var key = @"SOFTWARE\test";
            Registry.LocalMachine.CreateSubKey(key);

            // Execute
            var result = RegistryTool.RegistryKeyExist(@"HKLM\" + key);

            // Validate
            Assert.True(result);

            // Cleanup
            Registry.LocalMachine.DeleteSubKeyTree(key, false);
        }

        [Fact]
        public void RegistryKeyExist_ShortLocalMachineNameDoesNotExist_ReturnFalse()
        {
            // Setup
            var key = @"SOFTWARE\test";

            // Execute
            var result = RegistryTool.RegistryKeyExist(@"HKLM\" + key);

            // Validate
            Assert.False(result);
        }

        [Fact]
        public void RegistryValue_ShortLocalMachineNameWithValueExist_ReturnValue()
        {
            // Setup
            var key = @"SOFTWARE\test";
            var value = "hi";
            var valueName = "thing";
            Registry.LocalMachine.CreateSubKey(key).SetValue(valueName, value);

            // Execute
            var result = RegistryTool.RegistryValue(@"HKLM\" + key, valueName);

            // Validate
            Assert.Equal(value, result);

            // Cleanup
            Registry.LocalMachine.DeleteSubKeyTree(key, false);
        }

        [Fact]
        public void RegistryValue_LongLocalMachineNameWithOutValue_ReturnDefault()
        {
            // Setup
            var key = @"SOFTWARE\test";
            var valueName = "thing";
            var defaultValue = "bye";

            // Execute
            var result = RegistryTool.RegistryValue(@"HKEY_LOCAL_MACHINE\" + key, valueName, defaultValue);

            // Validate
            Assert.Equal(defaultValue, result);

            // Cleanup
            Registry.LocalMachine.DeleteSubKeyTree(key, false);
        }
    }
}
