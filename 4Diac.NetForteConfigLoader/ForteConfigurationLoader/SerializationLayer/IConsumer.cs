namespace ForteConfigurationLoader.SerializationLayer
{
    public interface IConsumer
    {
        void OnRecieve(EDataTypeTags tag, byte[] payload);

        object GetContent();

        void Flush();
    }
}