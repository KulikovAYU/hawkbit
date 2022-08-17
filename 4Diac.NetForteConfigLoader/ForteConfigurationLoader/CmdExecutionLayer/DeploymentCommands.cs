using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ForteConfigurationLoader.DataAccessLayer;
using ForteConfigurationLoader.FBootParcerLayer;
using ForteConfigurationLoader.InnerCommandLayer;
using ForteConfigurationLoader.Options;
using ForteConfigurationLoader.SerializationLayer;
using ForteConfigurationLoader.TcpLayer;
using ForteConfigurationLoader.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using systemd1.DBus;
using Tmds.DBus;

namespace ForteConfigurationLoader.CmdExecutionLayer
{
    public class DeploymentCommandBase
    {
        protected readonly Queue<DeploymentCommandBase> CommandsSet = new();

        protected readonly IOptions<DiacUpdaterSettings> Options;

        public DeploymentCommandBase(IOptions<DiacUpdaterSettings> options)
        {
            Options = options;
        }

        public virtual bool Execute()
        {
            while (CommandsSet.Count != 0)
            {
                var bIsOk = CommandsSet.Dequeue().Execute();
                if (!bIsOk)
                    return false;
            }

            return true;
        }

        public DeploymentCommandBase Attach(DeploymentCommandBase newCommand)
        {
            CommandsSet.Enqueue(newCommand);

            return this;
        }

        public DeploymentCommandBase AttachIf(DeploymentCommandBase newCommand, Func<bool> predicate)
        {
            if (predicate != null &&
                predicate())
                CommandsSet.Enqueue(newCommand);

            return this;
        }
    }

    #region Linux Command Set

    public class RunDbusCommand : DeploymentCommandBase
    {
        private readonly ILogger<RunDbusCommand> _logger;

        public RunDbusCommand(IOptions<DiacUpdaterSettings> options) : base(options)
        {
            _logger = ServiceLocator.DeployRunDbusLogger;
        }

        public override bool Execute()
        {
            var serviceName = Options.Value.ServiceName;

            if (string.IsNullOrWhiteSpace(serviceName))
                throw new Exception($"ServiceName must be initialized");

            _logger.LogInformation($"Begin starting service:{serviceName}");

            var networkManager = Connection.System.CreateProxy<IManager>("org.freedesktop.systemd1",
                new ObjectPath("/org/freedesktop/systemd1"));

            var result = networkManager.StartUnitAsync(serviceName, "replace");
            result.Wait();


            Thread.Sleep((int) TimeSpan.FromSeconds(5.0).TotalMilliseconds); //wait to run service

            _logger.LogInformation($"Complete starting service:{serviceName}");
            return result.IsCompletedSuccessfully;
        }
    }

    public class StopDbusCommand : DeploymentCommandBase
    {
        private readonly ILogger<StopDbusCommand> _logger;

        public StopDbusCommand(IOptions<DiacUpdaterSettings> options) : base(options)
        {
            _logger = ServiceLocator.DeployStopDbusLogger;
        }

        public override bool Execute()
        {
            var serviceName = Options.Value.ServiceName;

            if (string.IsNullOrWhiteSpace(serviceName))
                throw new Exception($"ServiceName must be initialized");

            _logger.LogInformation($"Begin stopping service:{serviceName}");

            var networkManager = Connection.System.CreateProxy<IManager>("org.freedesktop.systemd1",
                new ObjectPath("/org/freedesktop/systemd1"));

            var result = networkManager.StopUnitAsync(serviceName, "fail");
            result.Wait();

            _logger.LogInformation($"Complete stopping service:{serviceName}");

            return result.IsCompletedSuccessfully;
        }
    }

    public class CpyForteExecLinuxCommand : CpyForteExecWindowsCommand
    {
        public CpyForteExecLinuxCommand(IFilesDataStore filesDataStore, IOptions<DiacUpdaterSettings> options) : base(
            filesDataStore, options)
        {
        }

        public override bool Execute()
        {
            if (!base.Execute())
                return false;

            return MakeExecutable();
        }

        private bool MakeExecutable()
        {
            //to make executable file we must invoke script sudo chmod +x <file>.
            //that script makes executable file
            //for unix system is important to set file type
            try
            {
                var proc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \" chmod +x  {Options.Value.ForteFullExecutableName}\" ",
                        CreateNoWindow = true
                    }
                };


                if (proc.Start())
                    return proc.WaitForExit((int) TimeSpan.FromSeconds(5.0).TotalMilliseconds);
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }

    public class DeployForteExecutableLinuxCommandSet : DeploymentCommandBase
    {
        public DeployForteExecutableLinuxCommandSet(IFilesDataStore filesDataStore,
            IOptions<DiacUpdaterSettings> options) : base(options)
        {
            CommandsSet.Enqueue(new StopDbusCommand(options));
            CommandsSet.Enqueue(new RemoveForteExecutableCommand(options));
            CommandsSet.Enqueue(new CpyForteExecLinuxCommand(filesDataStore, options));
            CommandsSet.Enqueue(new RunDbusCommand(options));
        }
    }

    #endregion

    #region Windows Command Set

    public class RunProcCommand : DeploymentCommandBase
    {
        public RunProcCommand(IOptions<DiacUpdaterSettings> options) : base(options)
        {
        }

        public override bool Execute()
        {
            try
            {
                using var p = Process.Start(new ProcessStartInfo
                {
                    FileName = Options.Value.ForteFullExecutableName, //file to execute
                    Arguments = "", //arguments to use
                    UseShellExecute = false, //use process Creation semantics
                    RedirectStandardOutput = true, //redirect standart output to this proc object
                    CreateNoWindow = false, //if this is a terminal app, don't show it
                    WindowStyle = ProcessWindowStyle.Normal //if this is a terminal app, don't show it
                });

                Thread.Sleep((int) TimeSpan.FromSeconds(5.0).TotalMilliseconds); //wait to run service
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public class StopProcCommand : DeploymentCommandBase
    {
        public StopProcCommand(IOptions<DiacUpdaterSettings> options) : base(options)
        {
        }

        public override bool Execute()
        {
            try
            {
                
                var execProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Options.Value.ForteExecutableName));
                foreach (var execProcess in execProcesses)
                {
                    execProcess.Kill();
                    execProcess.WaitForExit();
                    execProcess.Dispose();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class CpyForteExecWindowsCommand : DeploymentCommandBase
    {
        private readonly IFilesDataStore _filesDataStore;

        public CpyForteExecWindowsCommand(IFilesDataStore filesDataStore, IOptions<DiacUpdaterSettings> options) :
            base(options)
        {
            _filesDataStore = filesDataStore;
        }

        public override bool Execute()
        {
            var execPath = Options.Value.ForteExecutablePath;
            if (string.IsNullOrWhiteSpace(execPath))
                throw new Exception($"Unlegal exec path");

            var forteExecutable = _filesDataStore.QueryFiles(FileDataModel.Type.eForte).ToList();
            var forteFile = forteExecutable.First();

            var downloadedPath = forteFile.FullName;
            if (!File.Exists(downloadedPath))
                return false;

            FileUtils.CopyFileToDirectory(downloadedPath, execPath);

            return true;
        }
    }

    public class DeployForteExecutableWindowsCommandSet : DeploymentCommandBase
    {
        public DeployForteExecutableWindowsCommandSet(IFilesDataStore filesDataStore,
            IOptions<DiacUpdaterSettings> options) : base(options)
        { 
            CommandsSet.Enqueue(new StopProcCommand(options));
            CommandsSet.Enqueue(new RemoveForteExecutableCommand(options));
            CommandsSet.Enqueue(new CpyForteExecWindowsCommand(filesDataStore, options));
            CommandsSet.Enqueue(new RunProcCommand(options));
        }
    }

    #endregion

    public class RemoveForteExecutableCommand : DeploymentCommandBase
    {
        private readonly ILogger<RemoveForteExecutableCommand> _logger;

        public RemoveForteExecutableCommand(IOptions<DiacUpdaterSettings> options) : base(options)
        {
            _logger = ServiceLocator.RemoveForteExecutableLogger;
        }

        public override bool Execute()
        {
            var execfullPath = Options.Value.ForteFullExecutableName;

            if (File.Exists(execfullPath))
            {
                _logger.LogInformation($"Remove forte executable:{execfullPath}");
                File.Delete(execfullPath);
            }

            return true;
        }
    }

    //deploy forte fboot configuration
    public class DeployFbootConfigurationCommand : DeploymentCommandBase
    {
        private readonly ILogger<DeployFbootConfigurationCommand> _logger = ServiceLocator.DeployFbootLogger;
        private readonly IForteClient _forteClient = ServiceLocator.ForteClient;
        private readonly BaseSerializer _serializer = ServiceLocator.Serializer;
        private IFBootParser _fbootParser => ServiceLocator.FbootParser;

        private readonly List<FunctionBlock> _fbList = new();

        private readonly IFilesDataStore _filesDataStore;

        public DeployFbootConfigurationCommand(IFilesDataStore filesDataStore, IOptions<DiacUpdaterSettings> options) :
            base(options)
        {
            _filesDataStore = filesDataStore;
        }

        public override bool Execute()
        {
            try
            {
                var diacUpdaterSettings = Options.Value;
                var remoteDeviceData = diacUpdaterSettings.RemoteDevice;

                if (!_forteClient.Connect(remoteDeviceData.HostUrl, remoteDeviceData.Port))
                    throw new Exception(
                        $"Unable establish connect to uri: {remoteDeviceData.HostUrl}:{remoteDeviceData.Port}");

                _logger.LogInformation(
                    $"successfully connected to uri:{remoteDeviceData.HostUrl}:{remoteDeviceData.Port}");


                if (QueryFBootFiles())
                {
                    var fBootFiles = _filesDataStore.QueryFiles(FileDataModel.Type.eFboot);
                    foreach (var fBootFile in fBootFiles)
                    {
                        _logger.LogInformation($"Start parsing boot file on path: {fBootFile.Name}");
                        _fbootParser.Parse(fBootFile.FullName);
                        if (!_fbootParser.IsValid)
                            throw new Exception($"parsing boot file on path: {fBootFile.Name}");

                        OnPublish();
                    }

                    return true;
                }

                return false;
            }
            finally
            {
                if (_forteClient.IsConnected)
                {
                    _logger.LogInformation($"Close forte connection...");
                    _forteClient.Close();
                }
            }
        }

        private bool QueryFBootFiles()
        {
            _fbList.Clear();

            string sMsgAction = "QueryFBootFiles";
            _logger.LogInformation(sMsgAction);

            var queryFbCommand = new QueryFbCommand(new FunctionBlock {Name = "*", Type = "*"});

            var payload = _serializer.Serialize(TagIdentify.e_STRING, queryFbCommand.Context, queryFbCommand.Tag);
            if (payload.Length == 0)
                return false;

            bool bIsOk = false;
            OnRecieve onRecieve = byteResponse =>
            {
                bIsOk = OnResponse(byteResponse, out var oResponse);
                if (bIsOk)
                {
                    bIsOk = oResponse.Id == queryFbCommand.Id;
                    _fbList.AddRange(oResponse.FbList);
                }
            };

            _forteClient.OnReceiveHandler += onRecieve;

            _forteClient.SendData(payload);

            _forteClient.OnReceiveHandler -= onRecieve;

            if (bIsOk)
                _logger.LogInformation(sMsgAction);
            else
                _logger.LogError(sMsgAction);

            return bIsOk;
        }

        private bool OnResponse(byte[] byteResponse, out ResponseFb response)
        {
            _serializer.Flush();
            _serializer.Deserialize(byteResponse);
            var sResponse = (string) _serializer.GetContent();

            return Response.FromString(sResponse, out response);
        }

        private void OnPublish()
        {
            _logger.LogInformation("Start publish file");
            var execTasks = _fbootParser.GetExecTasks();

            if (_fbList.Count > 0)
            {
                foreach (var task in execTasks)
                {
                    var nameGroup = task.Name;
                    var existFb = _fbList.Find(fb => fb.Name.Equals(nameGroup));
                    if (existFb == null)
                        continue;

                    _logger.LogInformation($"Replace configuration of block {nameGroup}");

                    task.Commands.Insert(0, new KillFbCommand(existFb));
                    task.Commands.Insert(1, new DeleteFbCommand(existFb));
                }
            }

            foreach (var cmd in execTasks.SelectMany(task => task.Commands))
            {
                _logger.LogInformation($"Executing FbTag={cmd.Tag}, context={cmd.Context}");

                var payload = _serializer.Serialize(TagIdentify.e_STRING, cmd.Context, cmd.Tag);

                OnRecieve onRecieve = bResponse =>
                {
                    if (OnResponse(bResponse, out var fbResponse))
                    {
                        bool bIsOk = cmd.Id == fbResponse.Id && string.IsNullOrEmpty(fbResponse.Reason);
                        if (bIsOk)
                        {
                            _logger.LogInformation($"Succsess Cmd id= {fbResponse.Id}");
                        }
                        else
                        {
                            //NO_SUCH_OBJECT
                            //INVALID_STATE
                            _logger.LogError(
                                $"Error Cmd id= {cmd.Id}, context = {cmd.Context}, reason = {fbResponse.Reason}");
                        }
                    }
                };

                _forteClient.OnReceiveHandler += onRecieve;
                _forteClient.SendData(payload);
                _forteClient.OnReceiveHandler -= onRecieve;
            }
        }
    }
}