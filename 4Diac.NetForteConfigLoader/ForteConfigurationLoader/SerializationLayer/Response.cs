using System.Xml;
using ForteConfigurationLoader.InnerCommandLayer;

namespace ForteConfigurationLoader.SerializationLayer
{
    public static class Response
    {
        public static bool FromString(string sResponse, out ResponseFb response)
        {
            response = new ResponseFb();
            if (string.IsNullOrEmpty(sResponse))
                return false;
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sResponse);


            var idAttr = doc.DocumentElement?.SelectSingleNode("//Response[@ID]")?.Attributes?["ID"]?.Value ?? "";

            if (!int.TryParse(idAttr, out var id))
                return false;

            response.Id = id;
            
            var reasonAttr = doc.DocumentElement?.SelectSingleNode("//Response[@Reason]")?.Attributes?["Reason"]?.Value ?? "";
            response.Reason = reasonAttr;
            
            var fbListNode = doc.DocumentElement?.SelectNodes("//FBList/*");

            if (fbListNode == null)
                return false;

            foreach (var fbNode in fbListNode)
            {
                if(fbNode == null)
                    continue;

                var name = ((XmlNode) fbNode).Attributes?["name"]?.Value;
                var type = ((XmlNode) fbNode).Attributes?["type"]?.Value;
                
                if(string.IsNullOrEmpty(name) ||
                   string.IsNullOrEmpty(type))
                    continue;
                
                response.FbList.Add(new FunctionBlock{Name = name, Type = type});
            }

            return true;
        }
    }
}