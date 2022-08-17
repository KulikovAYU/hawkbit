using System.Collections.Generic;

namespace ForteConfigurationLoader.InnerCommandLayer
{
    
    /*
     * <Response ID="1">
        <FBList>
            <FB name="EMB_RES" type="EMB_RES"/>
        </FBList>
        </Response>
     */
    
    public class ResponseFb
    {
        public int Id { get; set; }

        public string Reason { get; set; }
        public List<FunctionBlock> FbList { get; } = new List<FunctionBlock>();
    }
}