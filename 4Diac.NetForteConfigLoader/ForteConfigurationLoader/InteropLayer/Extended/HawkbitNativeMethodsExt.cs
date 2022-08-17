using System;
using System.Reflection;
using System.Runtime.InteropServices;
using ForteConfigurationLoader.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ForteConfigurationLoader.InteropLayer.Extended
{

    public class HawkbitNativeMethodsExt
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool MethodNameDelegate(ref NativeHawkbitConnectionCfg y);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativePutDelegate(ref NativeResponse y);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeGetDelegate(out NativeHawkbitDeploymentData y);

        private readonly CoreLibraryMap _libraryMap;
        private readonly IOptions<HawkbitSettings> _settings;
        private readonly ILogger<HawkbitNativeMethodsExt> _logger;

        private readonly string _sAliasName;

        public HawkbitNativeMethodsExt(CoreLibraryMap libraryMap, IOptions<HawkbitSettings> settings, ILogger<HawkbitNativeMethodsExt> logger)
        {
            _libraryMap = libraryMap;
            _settings = settings;
            _logger = logger;
            _sAliasName = _settings.Value.LibraryMappings[0].Alias;
        }

        public bool SetConfig(ref NativeHawkbitConnectionCfg cfg)
        {
            try
            {
                var invokeFunc = _libraryMap.Invoke<MethodNameDelegate>(_sAliasName, MethodBase.GetCurrentMethod()?.Name);
                return invokeFunc(ref cfg);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }

        public void StartClient()
        {
            try
            {
                var invokeFunc = _libraryMap.Invoke<Action>(_sAliasName, MethodBase.GetCurrentMethod()?.Name);
                invokeFunc();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }

        public void Put(ref NativeResponse response)
        {
            try
            {
                var invokeFunc = _libraryMap.Invoke<NativePutDelegate>(_sAliasName, MethodBase.GetCurrentMethod()?.Name);
                invokeFunc(ref response);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }

        public void Get(out NativeHawkbitDeploymentData data)
        {
            try
            {
                var invokeFunc = _libraryMap.Invoke<NativeGetDelegate>(_sAliasName, MethodBase.GetCurrentMethod()?.Name);
                invokeFunc(out data);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }
    }
}