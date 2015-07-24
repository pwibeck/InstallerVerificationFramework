namespace InstallerVerificationLibrary.Tools
{
    using System;
    using System.IO;
    using InstallerVerificationLibrary.Logging;

    public static class FileSystemTool
    {
        public static void RemoveFile(string filePath)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                return;
            }

            Log.WriteInfo("Deleting file '" + filePath + "'", "FileSystemTool");
            file.Delete();
        }

        public static void RemoveDirectory(string path)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                return;
            }

            Log.WriteInfo("Deleting directory '" + dir.FullName + "'", "FileSystemTool");
            try
            {
                dir.Delete(true);
            }
            catch (UnauthorizedAccessException e)
            {
                Log.WriteError("Could not delete directory '" + dir.FullName + "' because of '" + e.Message,
                    "FileSystemTool");
            }
            catch (IOException e)
            {
                Log.WriteError("Could not delete directory '" + dir.FullName + "' because of '" + e.Message,
                    "FileSystemTool");
            }
        }

        public static string DestroyTextFile(string path)
        {
            if (!File.Exists(path))
            {
                Log.WriteError("Could not find file : " + path, "DestroyTextFile");
                throw new FileNotFoundException("Could not find file : " + path);
            }

            var orgContent = File.ReadAllText(path);

            var newContent = orgContent.Remove(30, 50);

            File.WriteAllText(path, newContent);

            return orgContent;
        }

        public static bool CheckIfTextFileIsRestored(string expectedContent, string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Could not find file : " + path);
            }

            var contentFromCurrunetFile = File.ReadAllText(path);

            if (string.Compare(contentFromCurrunetFile, expectedContent, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            Log.WriteError("Expected content : " + expectedContent + " \nReal content : " + contentFromCurrunetFile,
                "CheckIfTextFileIsRestored");

            return false;
        }

        public static void DestroyBinaryFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Could not find file : " + path);
            }

            byte[] buff;

            using (var fs = new FileStream(path, FileMode.Open))
            {
                var br = new BinaryReader(fs);
                buff = new byte[fs.Length];
                br.Read(buff, 0, buff.Length);
            }

            for (var i = 1; i < 10; i++)
            {
                var tmp = buff[buff.Length - i];
                buff[buff.Length - i] = buff[i];
                buff[i] = tmp;
            }

            using (var fs = new FileStream(path, FileMode.Create))
            {
                var bw = new BinaryWriter(fs);
                bw.Write(buff);
            }
        }
    }
}