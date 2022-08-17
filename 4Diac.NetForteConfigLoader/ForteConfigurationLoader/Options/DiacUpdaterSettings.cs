using System.IO;

namespace ForteConfigurationLoader.Options
{
    public struct RemoteDevice
    {
        public string HostUrl { get; set; }
        
        public int Port { get; set; }
    }

    public class DiacUpdaterSettings
    {
        public RemoteDevice RemoteDevice { get; set; }
        
        public string ForteExecutablePath { get; set; }
        
        public string ForteExecutableName { get; set; }
        
        public string ServiceName { get; set; }

        public string ForteFullExecutableName => Path.Combine(ForteExecutablePath, ForteExecutableName);
    }
}