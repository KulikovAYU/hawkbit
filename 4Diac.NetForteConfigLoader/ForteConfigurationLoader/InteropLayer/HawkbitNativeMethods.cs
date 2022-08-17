using System.Runtime.InteropServices;


namespace ForteConfigurationLoader.InteropLayer
{
    //http://www.cyberguru.ru/microsoft-net/net-framework/managed-unmanaged.html?showall=1
    //https://www.mono-project.com/docs/advanced/pinvoke/
    public static class HawkbitNativeMethods
    {
        
        
        [DllImport(@"/home/user/hawkbit updater/hawkbit/hawkbit-cpp/cmake-build-debug/interop/libhawkbit_interop.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Test();
        
        [DllImport(@"/home/user/hawkbit updater/hawkbit/hawkbit-cpp/cmake-build-debug/interop/libhawkbit_interop.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartClient();
        
        // [DllImport(@"/home/user/hawkbit updater/hawkbit/hawkbit-cpp/cmake-build-debug/Test/libhawkbit_test.so", CallingConvention = CallingConvention.Cdecl)]
        // public static extern int Foo();
///home/user/hawkbit updater/hawkbit/hawkbit-cpp/cmake-build-debug/interop/
        [DllImport(@"libhawkbit_interop.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetConfig(ref NativeHawkbitConnectionCfg cfg);
        
        [DllImport(@"/home/user/hawkbit updater/hawkbit/hawkbit-cpp/cmake-build-debug/interop/libhawkbit_interop.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Put(NativeResponse response);
        
        [DllImport(@"libhawkbit_interop", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Get(out NativeHawkbitDeploymentData data);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);
    }
}