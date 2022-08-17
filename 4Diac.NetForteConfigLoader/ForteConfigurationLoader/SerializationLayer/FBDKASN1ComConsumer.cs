using System.Text;

namespace ForteConfigurationLoader.SerializationLayer
{
    public class FBDKASN1ComConsumer : IConsumer
    {
        private readonly StringBuilder _sContent = new StringBuilder(50);
       
        public void OnRecieve(EDataTypeTags tag, byte[] payload)
        {
            if (tag == EDataTypeTags.e_STRING_TAG)
            {
              
                //get response from server
                string resp = Encoding.ASCII.GetString(payload);
                if (!string.IsNullOrEmpty(resp))
                    _sContent.Append(resp);
            }
        }

        public object GetContent() => _sContent.ToString();
        public void Flush() =>  _sContent.Clear();
    }
}