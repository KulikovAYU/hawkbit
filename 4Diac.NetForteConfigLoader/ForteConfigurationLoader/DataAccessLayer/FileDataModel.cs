using System.Security.AccessControl;

namespace ForteConfigurationLoader.DataAccessLayer
{

    public class Hashes
    {
        public string Md5 { get; set; }
        public string Sha1 { get; set; }
        public string Sha256 { get; set; }
    }

    public class FileDataModel
    {
        public enum Type
        {
            eUndef,
            eFboot,
            eForte
        }

        public Type FileType { get; set; } = Type.eUndef;

        public Hashes Hashes { get; } = new Hashes();

        public string Name { get; set; }

        public string FullName { get; set; }
    }
}