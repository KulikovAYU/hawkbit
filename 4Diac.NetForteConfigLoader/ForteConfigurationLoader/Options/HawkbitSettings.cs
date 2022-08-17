using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ForteConfigurationLoader.Options
{

    public class EndPointEnv
    {
        public string Name { get; set; }
        public string Value  { get; set; }
    }

    public class GatewayTokenEnv
    {
        public string Name  { get; set; }
        public string Value  { get; set; }
    }
    

    public class ControllerIdEnv
    {
        public string Name  { get; set; }
        public string Value { get; set; }
    }


    public class ConnectionData
    {
        public EndPointEnv EndPointEnv { get; set; }

        public GatewayTokenEnv GatewayTokenEnv { get; set; }

        public ControllerIdEnv ControllerIdEnv { get; set; }
    }


    public class LibraryDesc
    {
        public string Alias { get; set; }
        public string WinDllDir { get; set; }
        public string WinDllName { get; set; }
        public string LinuxLibDir { get; set; }
        public string LinuxLibName { get; set; }
    }

    public class HawkbitSettings
    {
        public List<LibraryDesc> LibraryMappings { get; set; }
        
        public string DownloadWindowsFilePath { get; set; }
        
        public string DownloadLinuxFilePath { get; set; }
       
        public ConnectionData ConnectionData { get; set; }


        public string DownloadFilePath => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? DownloadWindowsFilePath
            : DownloadLinuxFilePath;
    }
}