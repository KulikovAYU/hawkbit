using System.Collections.Generic;
using ForteConfigurationLoader.InteropLayer;

namespace ForteConfigurationLoader.DataAccessLayer
{
    public interface IFilesDataStore : IEnumerable<FileDataModel>
    {
        IEnumerable<FileDataModel> QueryFiles(FileDataModel.Type type);
        void FromDeploymentData(NativeHawkbitDeploymentData deploymentData);
        void StartTransaction();
        void CommitTransaction();
        void RollBackTransaction();
    }
}