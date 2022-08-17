using ForteConfigurationLoader.InteropLayer;

namespace ForteConfigurationLoader.CmdExecutionLayer 
{
    public interface ICmdSet
    {
        void FromDeploymentData(NativeHawkbitDeploymentData deploymentData);

        bool Execute();

        void StartTransaction();

        void CommitTransaction();

        void RollBackTransaction();
    }
}