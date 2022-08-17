using System.IO;

namespace ForteConfigurationLoader.Utilities
{
    public static class FileUtils
    {
        public static void CopyFileToDirectory(string sourceFilePath, string targetFilePath, bool overwrite = true)
        {
           
            var fiSource =  new FileInfo(sourceFilePath);
            if(!fiSource.Exists)
                return;

            var sFileName = fiSource.Name;
            
            if(string.IsNullOrWhiteSpace(sFileName) ||
               string.IsNullOrWhiteSpace(targetFilePath))
                return;
            
            var diTarget = new DirectoryInfo(targetFilePath);
            diTarget.Create();
            
            fiSource.CopyTo(Path.Combine(targetFilePath, sFileName), overwrite);
        }
        
        public static void MoveFilesFromDirectory(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            foreach (var fi in diSource.GetFiles())
            {
                fi.MoveTo(Path.Combine(diTarget.FullName, fi.Name), true);
            }
        }
        
        public static void CopyFilesFromDirectory(string sourceDirectory, string targetDirectory, bool overwrite = true)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            Directory.CreateDirectory(diTarget.FullName);
            
            // Copy each file into the new directory.
            foreach (var fi in diSource.GetFiles())
            {
                fi.CopyTo(Path.Combine(diTarget.FullName, fi.Name), overwrite);
            }
        }

        public static void ClearFilesInDirectory(string sourceDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            foreach (var fi in diSource.GetFiles())
            {
                fi.Delete();
            }
        }
    }
}