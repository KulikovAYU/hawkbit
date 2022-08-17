using ForteConfigurationLoader.CmdExecutionLayer;
using ForteConfigurationLoader.DataAccessLayer;
using ForteConfigurationLoader.FBootParcerLayer;
using ForteConfigurationLoader.SerializationLayer;
using ForteConfigurationLoader.TcpLayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ForteConfigurationLoader
{

    public static class ServiceLocator
    {
        public static ILogger<DeployFbootConfigurationCommand> DeployFbootLogger =>
           App.Host.Services.GetService<ILogger<DeployFbootConfigurationCommand>>();

        public static ILogger<RunDbusCommand> DeployRunDbusLogger =>
            App.Host.Services.GetService<ILogger<RunDbusCommand>>();

        public static ILogger<StopDbusCommand> DeployStopDbusLogger =>
            App.Host.Services.GetService<ILogger<StopDbusCommand>>();

        public static ILogger<RemoveForteExecutableCommand> RemoveForteExecutableLogger =>
            App.Host.Services.GetService<ILogger<RemoveForteExecutableCommand>>();

        public static IForteClient ForteClient => App.Host.Services.GetService<IForteClient>();

        public static BaseSerializer Serializer => App.Host.Services.GetService<BaseSerializer>();

        public static IFBootParser FbootParser => App.Host.Services.GetService<IFBootParser>();
    }
}