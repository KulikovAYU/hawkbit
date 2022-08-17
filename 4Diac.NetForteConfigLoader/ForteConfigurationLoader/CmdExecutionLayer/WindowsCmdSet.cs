using System.Linq;
using ForteConfigurationLoader.DataAccessLayer;
using ForteConfigurationLoader.InteropLayer;
using ForteConfigurationLoader.Options;
using Microsoft.Extensions.Options;

namespace ForteConfigurationLoader.CmdExecutionLayer
{

    public class WindowsCmdSet : ICmdSet
    {
        private readonly IFilesDataStore _filesDataStore;
        private readonly IOptions<DiacUpdaterSettings> _options;
        public WindowsCmdSet(IFilesDataStore filesDataStore, IOptions<DiacUpdaterSettings> options)
        {
            _filesDataStore = filesDataStore;
            _options = options;
        }

        public void FromDeploymentData(NativeHawkbitDeploymentData deploymentData)
        {
            _filesDataStore.FromDeploymentData(deploymentData);
        }

        public bool Execute()
        {
            return new DeploymentCommandBase(_options)
                .AttachIf(new DeployForteExecutableWindowsCommandSet(_filesDataStore, _options),
                    () => _filesDataStore.QueryFiles(FileDataModel.Type.eForte).ToList().Count == 1)
                .AttachIf(new DeployFbootConfigurationCommand(_filesDataStore, _options),
                    () => _filesDataStore.QueryFiles(FileDataModel.Type.eFboot).ToList().Count == 1)
                .Execute();
        }

        public void StartTransaction() => _filesDataStore.StartTransaction();

        public void CommitTransaction() => _filesDataStore.CommitTransaction();

        public void RollBackTransaction()
        {
            _filesDataStore.RollBackTransaction();
            Execute();
        }
    }

}