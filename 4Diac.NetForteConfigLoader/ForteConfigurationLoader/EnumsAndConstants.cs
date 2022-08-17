namespace ForteConfigurationLoader
{
    public enum StatCode
    {
        eOk = 200,
        eError = 300,
        eFailed = 400
    };

    public enum NativeDeploymentDataType
    {
        eUndefined = -1,
        eBeginPayload = 0,
        ePayloadContent = 1,
        eEndPayload = 2,
        eStartTransaction = 3,
        eCommitTransaction = 4,
        eAbortTransaction = 5
    }
    
}