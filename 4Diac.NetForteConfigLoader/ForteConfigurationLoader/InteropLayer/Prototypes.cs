using System.Runtime.InteropServices;

namespace ForteConfigurationLoader.InteropLayer
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeEndPointEnv
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Value;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeGatewayTokenEnv
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Value;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeControllerIdEnv
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string Value;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeHawkbitConnectionCfg
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string DownloadFilePath;
        
        public NativeEndPointEnv NativeEndPoint;

        public NativeGatewayTokenEnv NativeGatewayToken;

        public NativeControllerIdEnv NativeControllerId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeHawkbitHashes
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string md5;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string sha1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string sha256;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeHawkbitArtifact
    {
        public int size;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string fileName;
        
        public NativeHawkbitHashes hashes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeHawkbitDeploymentData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string part;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string name;
        
        public int version;

        public int type;
        
        public NativeHawkbitArtifact payload;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeResponse
    {
        public int statusCode;
        
        public int type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
        public string detail;
    }
}