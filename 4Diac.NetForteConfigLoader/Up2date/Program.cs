using Up2date.ddi;

namespace Up2date
{
    static class Program
    {
        static void Main(string[] args)
        {
            
            var gatewayToken = "1ef6137927dba99f934d554cc671c4b0";
            var hawkbitEndpoint = "http://cls-lxc-12:8080";
            var controllerId = "MA_TEST";
            
            var builder = DefaultClientBuilderImpl.NewInstance();
            builder.SetHawkbitEndpoint(hawkbitEndpoint, controllerId)
                .SetGatewayToken(gatewayToken)
                .SetEventHandler(new EventHandlerImpl())
                .Build()
                .Run();
        }
    }
}


