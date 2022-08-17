using System.Xml;

namespace Up2date.ddi
{
    
    public interface ICancelAction
    {
        int GetId();
        int GetStopId();
    }
    
    public class CancelAction : ICancelAction
    {
        private int _id;
        private int _stopId;

        public int GetId() => _id;
        public int GetStopId() => _stopId;

        public static ICancelAction FromString(string body)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(body);
                throw new NotImplementedException("Implement");
                return new CancelAction();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }


    public interface IDownloadProvider
    {
        void DownloadTo(Uri url, string s);
        string GetBody(Uri url);
        void DownloadWithReceiver(Func<char, int, bool> cb);
    }

    public enum Actions
    {
        GetConfigData, CancelAction, DeploymentBase, None
    }

    public class PollingData
    {
        private int _sleepTime;
        private Actions _action;
        private Uri _followURI;

        
        public int GetSleepTime() => _sleepTime;
        public Actions GetAction() => _action;
        public Uri GetFollowURI() => _followURI;

        PollingData FromString(string s)
        {
            return null;
        }
    }


}

