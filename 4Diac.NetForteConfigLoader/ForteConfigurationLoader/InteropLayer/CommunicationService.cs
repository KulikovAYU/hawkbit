using ForteConfigurationLoader.InteropLayer.Extended;

namespace ForteConfigurationLoader.InteropLayer
{
    public class CommunicationService
    {
        private readonly HawkbitNativeMethodsExt _pInvokeImpl;
        public CommunicationService(HawkbitNativeMethodsExt pInvokeImpl)
        {
            _pInvokeImpl = pInvokeImpl;
        }

        public void StartHawkbitClient() => _pInvokeImpl.StartClient();

        public bool SetHawkbitConfig(ref NativeHawkbitConnectionCfg cfg) => _pInvokeImpl.SetConfig(ref cfg);
        
        public void Put(ref NativeResponse response) => _pInvokeImpl.Put(ref response);

        public void Get(out NativeHawkbitDeploymentData data) => _pInvokeImpl.Get(out data);
    }
}