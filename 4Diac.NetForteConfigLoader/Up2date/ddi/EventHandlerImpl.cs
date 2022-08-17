namespace Up2date.ddi
{
    public interface IDeploymentBase
    {
        int GetId();
        string GetDownloadType();
        string GetUpdateType();
        bool IsInMaintenanceWindow();
        List<IChunk> GetChunks();
    }

    public interface IChunk
    {
        string GetPart();
        string GetVersion();
        string GetName();
        List<IArtifact> GetArtifacts();
    }

    public struct Hashes
    {
        public string sha1;
        public string md5;
        public string sha256;
    }
    
    public interface IArtifact
    {
        void DownloadTo(string path);
        string GetBody();
        void DownloadWithReceiver(Func<char, int, bool> cb);
        string GetFileName();
        Hashes GetFileHashes();
        int Size();
    }

    public interface IConfigResponse
    {
    }

    public class ConfigResponseImpl : IConfigResponse
    {
        public Dictionary<string, string> Data { get; }
        public bool IgnoreSleep { get; }

        public ConfigResponseImpl(Dictionary<string, string> data, bool ignoreSleep)
        {
            Data = data;
            IgnoreSleep = ignoreSleep;
        }
    }



    

    public interface IEventHandler
    {
        IConfigResponse OnConfigRequest();
        Response OnDeploymentAction(IDeploymentBase dp);
        Response OnCancelAction(ICancelAction ca);
        void OnNoActions();
    }


    public class EventHandlerImpl : IEventHandler
    {
        public IConfigResponse OnConfigRequest()
        {
            return ConfigConfigResponseBuilderImpl.NewInstance()
                .AddData("some", "config1")
                .AddData("some1", "new config")
                .AddData("some2", "RITMS123")
                .AddData("some3", "TES_TEST_TEST")
                .SetIgnoreSleep()
                .Build();
        }

        public Response OnDeploymentAction(IDeploymentBase dp)
        {
            var builder = ResponseBuilderImpl.NewInstance();
            builder.AddDetail("Printed deployment base info");

            foreach (var chunk in dp.GetChunks())
            {
                var part = chunk.GetPart();
                var name = chunk.GetName();

                foreach (var artifact in chunk.GetArtifacts())
                {
                    var filename = artifact.GetFileName();
                    var md5 = artifact.GetFileHashes().md5;
                    var sha1 = artifact.GetFileHashes().sha1;
                    var sha256 = artifact.GetFileHashes().sha256;

                    builder.AddDetail($"{filename} described. Starting download ...");
                    artifact.DownloadTo(filename);
                    builder.AddDetail($"Downloaded {filename}");
                }
            }

            return builder.AddDetail("Work done. Sending response")
                .SetIgnoreSleep()
                .SetExecution(Response.Execution.closed)
                .SetFinished(Response.Finished.success)
                .SetResponseDeliveryListener(new DeploymentBaseFeedbackDeliveryListener())
                .Build();
        }

        public Response OnCancelAction(ICancelAction ca)
        {
            return ResponseBuilderImpl.NewInstance()
                .SetExecution(Response.Execution.closed)
                .SetFinished(Response.Finished.success)
                .AddDetail("Some feedback")
                .AddDetail("One more feedback")
                .AddDetail("Really important feedback")
                .SetResponseDeliveryListener(new CancelActionFeedbackDeliveryListener())
                .SetIgnoreSleep()
                .Build();
        }

        public void OnNoActions()
        {
        }
    }
}