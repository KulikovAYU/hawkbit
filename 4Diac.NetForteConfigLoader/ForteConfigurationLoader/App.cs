using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ForteConfigurationLoader.CmdExecutionLayer;
using ForteConfigurationLoader.DataAccessLayer;
using ForteConfigurationLoader.FBootParcerLayer;
using ForteConfigurationLoader.InteropLayer;
using ForteConfigurationLoader.InteropLayer.Extended;
using ForteConfigurationLoader.InteropLayer.HostedService;
using ForteConfigurationLoader.Options;
using ForteConfigurationLoader.SerializationLayer;
using ForteConfigurationLoader.TcpLayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ForteConfigurationLoader
{
    public static class App
    {
        public static bool IsDesignMode { get; private set; } = true;

        private static IHost __Host;

        public static IHost Host => __Host ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        internal static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton<CoreLibraryMap>();
            services.TryAddSingleton<HawkbitNativeMethodsExt>();
            
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                services.AddScoped<ICmdSet, LinuxCmdSet>();
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                services.AddScoped<ICmdSet, WindowsCmdSet>();
            
            services.AddScoped<IForteClient, ForteClient>();
            services.AddScoped<IFBootParser, FBootParser>();
            services.AddScoped<BaseSerializer, FBDKASN1ComSerializer>();
            services.AddScoped<IConsumer, FBDKASN1ComConsumer>();

            services.AddSingleton<CommunicationService>();

            services.AddOptions<HawkbitSettings>()
                .Bind(host.Configuration.GetSection("Hakwbit"))
                .Validate(
                config =>
                {
                    const string sTag = "Appsettings.json [Hakwbit]\t";
                    if (string.IsNullOrEmpty(config.LibraryMappings[0].Alias))
                        throw new Exception($"{sTag}config.LibraryMappings[0].Alias mustn't be empty");

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if (string.IsNullOrEmpty(config.LibraryMappings[0].WinDllDir))
                            throw new Exception($"{sTag}config.LibraryMappings[0].WinDllDir mustn't be empty");

                        if (string.IsNullOrEmpty(config.LibraryMappings[0].WinDllName))
                            throw new Exception($"{sTag}config.LibraryMappings[0].WinDllName mustn't be empty");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(config.LibraryMappings[0].LinuxLibDir))
                            throw new Exception($"{sTag}config.LibraryMappings[0].LinuxLibDir mustn't be empty");

                        if (string.IsNullOrEmpty(config.LibraryMappings[0].LinuxLibName))
                            throw new Exception($"{sTag}config.LibraryMappings[0].LinuxLibName mustn't be empty");
                    }

                    if (string.IsNullOrEmpty(config.DownloadFilePath))
                        throw new Exception($"{sTag}DownloadFilePath mustn't be empty");

                    if (string.IsNullOrEmpty(config.ConnectionData.EndPointEnv.Name))
                        throw new Exception($"{sTag}ConnectionData.EndPointEnv.Name mustn't be empty");

                    if (string.IsNullOrEmpty(config.ConnectionData.EndPointEnv.Value))
                        throw new Exception($"{sTag}ConnectionData.EndPointEnv.Value mustn't be empty");

                    if (string.IsNullOrEmpty(config.ConnectionData.GatewayTokenEnv.Name))
                        throw new Exception($"{sTag}ConnectionData.GatewayTokenEnv.Name mustn't be empty");

                    if (string.IsNullOrEmpty(config.ConnectionData.GatewayTokenEnv.Value))
                        throw new Exception($"{sTag}ConnectionData.GatewayTokenEnv.Value mustn't be empty");

                    if (string.IsNullOrEmpty(config.ConnectionData.ControllerIdEnv.Name))
                        throw new Exception($"{sTag}ConnectionData.ControllerIdEnv.Name mustn't be empty");

                    if (string.IsNullOrEmpty(config.ConnectionData.ControllerIdEnv.Value))
                        throw new Exception($"{sTag}ConnectionData.ControllerIdEnv.Value mustn't be empty");


                    return true;
                });
            
            services.AddOptions<DiacUpdaterSettings>()
                .Bind(host.Configuration.GetSection("DiacUpdater"))
                .Validate(
                config =>
                {
                    const string sTag = "Appsettings.json [DiacUpdater]\t";

                    if (string.IsNullOrEmpty(config.ForteExecutablePath))
                        throw new Exception($"{sTag}ForteExecutablePath mustn't be empty");

                    if (string.IsNullOrEmpty(config.ForteExecutableName))
                        throw new Exception($"{sTag}ForteExecutableName mustn't be empty");

                    if (string.IsNullOrEmpty(config.ServiceName))
                        throw new Exception($"{sTag}ServiceName mustn't be empty");

                    if (string.IsNullOrEmpty(config.RemoteDevice.HostUrl))
                        throw new Exception($"{sTag}RemoteDevice.HostUr mustn't be empty");

                    if (config.RemoteDevice.Port <= 0)
                        throw new Exception($"{sTag}RemoteDevice.Port incorrect");

                    return true;
                });

            
            //register files data store
            services.AddSingleton<IFilesDataStore,FilesDataStore>();
            
            //register hawkbit client
            services.AddHostedService<HawkbitClient>();
            //register data consumer
            services.AddHostedService<DataConsumer>();
        }


        public static string CurrentDirectory => IsDesignMode
            ? Path.GetDirectoryName(GetSourceCodePath())
            : Environment.CurrentDirectory;

        private static string GetSourceCodePath([CallerFilePath] string Path = null) => Path;
    }
}