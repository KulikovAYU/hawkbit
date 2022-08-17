namespace ForteConfigurationLoader.SerializationLayer
{
    public abstract class BaseSerializer
    {
        protected readonly IConsumer Consumer;

        protected BaseSerializer(IConsumer consumer)
        {
            Consumer = consumer;
        }

        public abstract byte[] Serialize(TagIdentify dataType, byte[] payload, string sResIdent);

        public abstract byte[] Serialize(TagIdentify dataType, object payload, string sResIdent);
        
        public abstract void Deserialize(byte[] payload);

        public object GetContent() => Consumer.GetContent();

        public void Flush() => Consumer.Flush();
    }
}