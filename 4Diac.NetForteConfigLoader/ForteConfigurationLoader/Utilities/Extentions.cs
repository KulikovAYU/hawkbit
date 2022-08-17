using ForteConfigurationLoader.InteropLayer;
using ForteConfigurationLoader.Options;


namespace ForteConfigurationLoader.Utilities
{
    public static class Extentions
    {
        private static NativeEndPointEnv ToNative(this EndPointEnv endPointEnv)
        {
            NativeEndPointEnv nativeEndPointEnv = new NativeEndPointEnv
            {
                Name = endPointEnv.Name,
                Value = endPointEnv.Value
            };

            return nativeEndPointEnv;
        }
        
        private static NativeGatewayTokenEnv ToNative(this GatewayTokenEnv gatewayTokenEnv)
        {
            NativeGatewayTokenEnv nativeGatewayTokenEnv = new NativeGatewayTokenEnv
            {
                Name = gatewayTokenEnv.Name,
                Value = gatewayTokenEnv.Value
            };

            return nativeGatewayTokenEnv;
        }
        
        private static NativeControllerIdEnv ToNative(this ControllerIdEnv controllerIdEnv)
        {
            NativeControllerIdEnv nativeControllerIdEnv = new NativeControllerIdEnv
            {
                Name = controllerIdEnv.Name,
                Value = controllerIdEnv.Value
            };

            return nativeControllerIdEnv;
        }
        
        public static NativeHawkbitConnectionCfg ToNative(this HawkbitSettings data)
        {
            NativeHawkbitConnectionCfg cfg = new NativeHawkbitConnectionCfg
            {
                DownloadFilePath = data.DownloadFilePath,
                NativeEndPoint = data.ConnectionData.EndPointEnv.ToNative(),
                NativeGatewayToken =  data.ConnectionData.GatewayTokenEnv.ToNative(),
                NativeControllerId =  data.ConnectionData.ControllerIdEnv.ToNative()
            };

            return cfg;
        }

    }
}