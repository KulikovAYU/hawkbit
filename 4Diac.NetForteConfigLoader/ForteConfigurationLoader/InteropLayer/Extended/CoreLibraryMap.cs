using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ForteConfigurationLoader.Options;
using Microsoft.Extensions.Options;

namespace ForteConfigurationLoader.InteropLayer.Extended
{

    public class CoreLibraryMap
    {

        private readonly Dictionary<string, IntPtr> _registeredAssemblies = new();

        public CoreLibraryMap(IOptions<HawkbitSettings> opt)
        {
            ReadFromConfig(opt);
        }

        public TDelegate Invoke<TDelegate>(string sAliasName, string sName)
        {
            if (!_registeredAssemblies.TryGetValue(sAliasName, out var domainAssembly))
                throw new Exception($"Assembly with alias name {sAliasName} is not registered");

            if (!NativeLibrary.TryGetExport(domainAssembly, sName, out IntPtr methodHandle))
                throw new Exception($"Unable to get function: {sName} in assembly {sAliasName}");


            var functionPointer = Marshal.GetDelegateForFunctionPointer<TDelegate>(methodHandle);
            return functionPointer;

        }

        public void RegisterLibrary(string alias, KeyValuePair<string, string> pathAndName)
        {
            if (string.IsNullOrWhiteSpace(pathAndName.Key) ||
               string.IsNullOrWhiteSpace(pathAndName.Value))
                return;


            var sFilePath = Path.Combine(pathAndName.Key, pathAndName.Value);
            if (!File.Exists(sFilePath))
                return;

            if (!NativeLibrary.TryLoad(sFilePath, out var domainAssemblyPtr))
                return;

            if (_registeredAssemblies.ContainsKey(pathAndName.Key))
                return;

            _registeredAssemblies.Add(alias, domainAssemblyPtr);
        }


        void ReadFromConfig(IOptions<HawkbitSettings> opt)
        {
            var libraries = opt.Value.LibraryMappings;

            foreach (var libDesc in libraries)
            {
                var pathAndName = GetDllPath(libDesc);

                RegisterLibrary(libDesc.Alias, pathAndName);
            }
        }

        KeyValuePair<string, string> GetDllPath(LibraryDesc desc)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new KeyValuePair<string, string>(desc.WinDllDir, desc.WinDllName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new KeyValuePair<string, string>(desc.LinuxLibDir, desc.LinuxLibName);

            return new KeyValuePair<string, string>("", "");
        }
    }
}