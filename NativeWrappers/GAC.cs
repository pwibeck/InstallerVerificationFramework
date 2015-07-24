
////-------------------------------------------------------------
///// original code is here http://blogs.msdn.com/junfeng/articles/229649.aspx
////
//// GACWrap.cs
////
//// This implements managed wrappers to GAC API Interfaces
////-------------------------------------------------------------

namespace NativeWrappers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    //-------------------------------------------------------------
    // Interfaces defined by fusion
    //-------------------------------------------------------------
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
    public interface IAssemblyCache
    {
        [PreserveSig()]
        int UninstallAssembly(
                            int flags,
                            [MarshalAs(UnmanagedType.LPWStr)]
                            string assemblyName,
                            InstallReference refData,
                            out AssemblyCacheUninstallDisposition disposition);

        [PreserveSig()]
        int QueryAssemblyInfo(
                            int flags,
                            [MarshalAs(UnmanagedType.LPWStr)]
                            string assemblyName,
                            ref AssemblyInfo assemblyInfo);
        [PreserveSig()]
        int Reserved(
                            int flags,
                            IntPtr pvReserved,
                            out object ppAsmItem,
                            [MarshalAs(UnmanagedType.LPWStr)]
                            string assemblyName);
        [PreserveSig()]
        int Reserved(out object ppAsmScavenger);

        [PreserveSig()]
        int InstallAssembly(
                            int flags,
                            [MarshalAs(UnmanagedType.LPWStr)]
                            string assemblyFilePath,
                            InstallReference refData);
    }// IAssemblyCache

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
    public interface IAssemblyName
    {
        [PreserveSig()]
        int SetProperty(
                int PropertyId,
                IntPtr pvProperty,
                int cbProperty);

        [PreserveSig()]
        int GetProperty(
                int PropertyId,
                IntPtr pvProperty,
                ref int pcbProperty);

        [PreserveSig()]
        int Finalize();

        [PreserveSig()]
        int GetDisplayName(
                StringBuilder pDisplayName,
                ref int pccDisplayName,
                int displayFlags);

        [PreserveSig()]
        int Reserved(ref Guid guid,
            object obj1,
            object obj2,
            string string1,
            long llFlags,
            IntPtr pvReserved,
            int cbReserved,
            out IntPtr ppv);

        [PreserveSig()]
        int GetName(
                ref int pccBuffer,
                StringBuilder pwzName);

        [PreserveSig()]
        int GetVersion(
                out int versionHi,
                out int versionLow);
        [PreserveSig()]
        int IsEqual(
                IAssemblyName pAsmName,
                int cmpFlags);

        [PreserveSig()]
        int Clone(out IAssemblyName pAsmName);
    }// IAssemblyName

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
    public interface IAssemblyEnum
    {
        [PreserveSig()]
        int GetNextAssembly(
                IntPtr pvReserved,
                out IAssemblyName ppName,
                int flags);
        [PreserveSig()]
        int Reset();
        [PreserveSig()]
        int Clone(out IAssemblyEnum ppEnum);
    }// IAssemblyEnum

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("582dac66-e678-449f-aba6-6faaec8a9394")]
    public interface IInstallReferenceItem
    {
        // A pointer to a FUSION_INSTALL_REFERENCE structure. 
        // The memory is allocated by the GetReference method and is freed when 
        // IInstallReferenceItem is released. Callers must not hold a reference to this 
        // buffer after the IInstallReferenceItem object is released. 
        // This uses the InstallReferenceOutput object to avoid allocation 
        // issues with the interop layer. 
        // This cannot be marshaled directly - must use IntPtr 
        [PreserveSig()]
        int GetReference(
                out IntPtr pRefData,
                int flags,
                IntPtr pvReserced);
    }// IInstallReferenceItem

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("56b1a988-7c0c-4aa2-8639-c3eb5a90226f")]
    public interface IInstallReferenceEnum
    {
        [PreserveSig()]
        int GetNextInstallReferenceItem(
                out IInstallReferenceItem ppRefItem,
                int flags,
                IntPtr pvReserced);
    }// IInstallReferenceEnum

    public enum AssemblyCommitOption
    {
        /// <summary>
        /// None value
        /// </summary>
        None = 0,

        /// <summary>
        /// Default value
        /// </summary>
        Default = 1,

        /// <summary>
        /// Force value
        /// </summary>
        Force = 2
    }// enum AssemblyCommitFlags

    public enum AssemblyCacheUninstallDisposition
    {
        /// <summary>
        /// Unknow value
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Uninstalled value
        /// </summary>
        Uninstalled = 1,

        /// <summary>
        /// Still in use value
        /// </summary>
        StillInUse = 2,

        /// <summary>
        /// Already installed value
        /// </summary>
        AlreadyUninstalled = 3,

        /// <summary>
        /// Delete is pending
        /// </summary>
        DeletePending = 4,

        /// <summary>
        /// Has an install reference
        /// </summary>
        HasInstallReference = 5,

        /// <summary>
        /// Reference not found
        /// </summary>
        ReferenceNotFound = 6
    }

    [Flags]
    public enum AssemblyCacheFlags
    {
        GAC = 2,
    }

    public enum CreateAssemblyNameObjectFlags
    {
        CANOF_DEFAULT = 0,
        CANOF_PARSE_DISPLAY_NAME = 1,
    }

    [Flags]
    public enum AssemblyNameDisplayFlags
    {
        VERSION = 0x01,
        CULTURE = 0x02,
        PUBLIC_KEY_TOKEN = 0x04,
        PROCESSORARCHITECTURE = 0x20,
        RETARGETABLE = 0x80,
        //// This enum will change in the future to include
        //// more attributes.
        ALL = VERSION
                                    | CULTURE
                                    | PUBLIC_KEY_TOKEN
                                    | PROCESSORARCHITECTURE
                                    | RETARGETABLE
    }

    [StructLayout(LayoutKind.Sequential)]
    public class InstallReference
    {
        public InstallReference(Guid guid, string id, string data)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            cbSize = (int)(2 * IntPtr.Size + 16 + (id.Length + data.Length) * 2);
            //// quiet compiler warning 
            if (flags == 0) { }
            guidScheme = guid;
            identifier = id;
            description = data;
        }

        public Guid GuidScheme
        {
            get { return guidScheme; }
        }

        public string Identifier
        {
            get { return identifier; }
        }

        public string Description
        {
            get { return description; }
        }

        int cbSize;
        int flags;
        Guid guidScheme;
        [MarshalAs(UnmanagedType.LPWStr)]
        string identifier;
        [MarshalAs(UnmanagedType.LPWStr)]
        string description;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AssemblyInfo
    {
        public int cbAssemblyInfo; //// size of this structure for future expansion
        public int assemblyFlags;
        public long assemblySizeInKB;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string currentAssemblyPath;
        public int cchBuf; //// size of path buf.
    }

    [ComVisible(false)]
    public class InstallReferenceGuid
    {
        public static bool IsValidGuidScheme(Guid guid)
        {
            return (guid.Equals(UninstallSubkeyGuid) ||
                    guid.Equals(FilePathGuid) ||
                    guid.Equals(OpaqueGuid) ||
                    guid.Equals(Guid.Empty));
        }

        public readonly static Guid UninstallSubkeyGuid = new Guid("8cedc215-ac4b-488b-93c0-a50a49cb2fb8");
        public readonly static Guid FilePathGuid = new Guid("b02f9d65-fb77-4f7a-afa5-b391309f11c9");
        public readonly static Guid OpaqueGuid = new Guid("2ec93463-b0c3-45e1-8364-327e96aea856");
        //// these GUID cannot be used for installing into GAC.
        public readonly static Guid MsiGuid = new Guid("25df0fc1-7f97-4070-add7-4b13bbfd7cb8");
        public readonly static Guid OsInstallGuid = new Guid("d16d444c-56d8-11d5-882d-0080c847b195");
    }

    [ComVisible(false)]
    public static class AssemblyCache
    {
        public static void InstallAssembly(string assemblyPath, InstallReference reference, AssemblyCommitOption commitOption)
        {
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
                {
                    throw new ArgumentException("Invalid reference guid.", "reference");
                }
            }

            IAssemblyCache ac = null;

            int hr = 0;

            hr = FusionMethods.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.InstallAssembly((int)commitOption, assemblyPath, reference);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        //// assemblyName has to be fully specified name. 
        //// A.k.a, for v1.0/v1.1 assemblies, it should be "name, Version=xx, Culture=xx, PublicKeyToken=xx".
        //// For v2.0 assemblies, it should be "name, Version=xx, Culture=xx, PublicKeyToken=xx, ProcessorArchitecture=xx".
        //// If assemblyName is not fully specified, a random matching assembly will be uninstalled.         
        public static void UninstallAssembly(string assemblyName, InstallReference reference, out AssemblyCacheUninstallDisposition disposition)
        {
            AssemblyCacheUninstallDisposition dispResult = AssemblyCacheUninstallDisposition.Uninstalled;
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidGuidScheme(reference.GuidScheme))
                {
                    throw new ArgumentException("Invalid reference guid.", "reference");
                }
            }

            IAssemblyCache ac = null;

            int hr = FusionMethods.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.UninstallAssembly(0, assemblyName, reference, out dispResult);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            disposition = dispResult;
        }

        //// See comments in UninstallAssembly        
        public static string QueryAssemblyInfo(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentException("Invalid name", "assemblyName");
            }

            AssemblyInfo aInfo = new AssemblyInfo();

            aInfo.cchBuf = 1024;
            // Get a string with the desired length
            aInfo.currentAssemblyPath = new string('\0', aInfo.cchBuf);

            IAssemblyCache ac = null;
            int hr = FusionMethods.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.QueryAssemblyInfo(0, assemblyName, ref aInfo);
            }
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return aInfo.currentAssemblyPath;
        }
    }

    [ComVisible(false)]
    public class AssemblyCacheEnum
    {
        //// null means enumerate all the assemblies
        public AssemblyCacheEnum(string assemblyName)
        {
            IAssemblyName fusionName = null;
            int hr = 0;

            if (assemblyName != null)
            {
                hr = FusionMethods.CreateAssemblyNameObject(
                        out fusionName,
                        assemblyName,
                        CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME,
                        IntPtr.Zero);
            }

            if (hr >= 0)
            {
                hr = FusionMethods.CreateAssemblyEnum(
                        out m_AssemblyEnum,
                        IntPtr.Zero,
                        fusionName,
                        AssemblyCacheFlags.GAC,
                        IntPtr.Zero);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }
        
        public string GetNextAssembly()
        {
            int hr = 0;
            IAssemblyName fusionName = null;

            if (done)
            {
                return null;
            }

            //// Now get next IAssemblyName from m_AssemblyEnum
            hr = m_AssemblyEnum.GetNextAssembly((IntPtr)0, out fusionName, 0);

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            if (fusionName != null)
            {
                return GetFullName(fusionName);
            }
            else
            {
                done = true;
                return null;
            }
        }

        private string GetFullName(IAssemblyName fusionAsmName)
        {
            StringBuilder sDisplayName = new StringBuilder(1024);
            int iLen = 1024;

            int hr = fusionAsmName.GetDisplayName(sDisplayName, ref iLen, (int)AssemblyNameDisplayFlags.ALL);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return sDisplayName.ToString();
        }

        private IAssemblyEnum m_AssemblyEnum;
        private bool done;
    }

    public class AssemblyCacheInstallReferenceEnum
    {
        public AssemblyCacheInstallReferenceEnum(string assemblyName)
        {
            IAssemblyName fusionName = null;

            int hr = FusionMethods.CreateAssemblyNameObject(
                        out fusionName,
                        assemblyName,
                        CreateAssemblyNameObjectFlags.CANOF_PARSE_DISPLAY_NAME,
                        IntPtr.Zero);

            if (hr >= 0)
            {
                hr = FusionMethods.CreateInstallReferenceEnum(out refEnum, fusionName, 0, IntPtr.Zero);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public InstallReference GetNextReference()
        {
            IInstallReferenceItem item = null;
            int hr = refEnum.GetNextInstallReferenceItem(out item, 0, IntPtr.Zero);
            if ((uint)hr == 0x80070103)
            {   //// ERROR_NO_MORE_ITEMS
                return null;
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            IntPtr refData;
            InstallReference instRef = new InstallReference(Guid.Empty, string.Empty, string.Empty);

            hr = item.GetReference(out refData, 0, IntPtr.Zero);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            Marshal.PtrToStructure(refData, instRef);
            return instRef;
        }

        private IInstallReferenceEnum refEnum;
    }
}

