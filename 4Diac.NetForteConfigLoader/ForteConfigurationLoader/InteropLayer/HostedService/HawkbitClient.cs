using System.Threading;
using System.Threading.Tasks;
using ForteConfigurationLoader.Options;
using ForteConfigurationLoader.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ForteConfigurationLoader.InteropLayer.HostedService
{
    public class HawkbitClient : IHostedService
    {
        private readonly HawkbitSettings _hawkbitSettings;
        private readonly CommunicationService _service;
        private readonly  ILogger<HawkbitClient>  _logger;
        private const string _sTag = "[HAWKBIT CLIENT]";
        
        public HawkbitClient(CommunicationService service, IOptions<HawkbitSettings> hawkbitSettings, ILogger<HawkbitClient> logger)
        {
            _hawkbitSettings = hawkbitSettings.Value;
            _service = service;
            _logger = logger;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
             var nativeCfg = _hawkbitSettings.ToNative();
             if (_service.SetHawkbitConfig(ref nativeCfg))
             {
                 _logger.LogInformation($"{_sTag} hawkbit config successfully applied...");
                 Task.Run(() => { _logger.LogInformation($"{_sTag} hawkbit client started..."); _service.StartHawkbitClient();}, cancellationToken);
             }
             else
             {
                 _logger.LogInformation($"{_sTag} error to apply hawkbit config...");
             }

             return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}