namespace Up2date.ddi
{
    
    public class Response
    {
        public enum Finished
        {
            success, failture, none
        }
        
        public enum Execution
        {
            closed, proceeding,
            canceled, sheduled,
            rejected, resumed
        }
    }
    
    public interface IConfigResponseBuilder
    {
        IConfigResponseBuilder AddData(string key, string value);

        IConfigResponseBuilder SetIgnoreSleep();

        IConfigResponse Build();
    }

    public interface IResponseDeliveryListener
    {
        void OnSuccessfulDelivery();
        void OnError();
    }

    public class ConfigConfigResponseBuilderImpl : IConfigResponseBuilder
    {
        private Dictionary<string, string> _data = new();
        private bool _ignoreSleep;

        public static IConfigResponseBuilder NewInstance() => new ConfigConfigResponseBuilderImpl();
        
        public IConfigResponseBuilder AddData(string key, string value)
        {
            _data.Add(key, value);
            return this;
        }

        public IConfigResponseBuilder SetIgnoreSleep()
        {
            _ignoreSleep = true;
            return this;
        }

        public IConfigResponse Build() => new ConfigResponseImpl(_data, _ignoreSleep);
    }


    public interface IResponseBuilder
    {
        public IResponseBuilder SetFinished(Response.Finished st);
        public IResponseBuilder SetExecution(Response.Execution exec);
        public IResponseBuilder AddDetail(string sDet);
        IResponseBuilder SetResponseDeliveryListener(IResponseDeliveryListener responseDeliveryListener);
        public IResponseBuilder SetIgnoreSleep();
        public Response Build();
    }

    public class ResponseBuilderImpl : IResponseBuilder
    {
        public static IResponseBuilder NewInstance() => new ResponseBuilderImpl();


        public IResponseBuilder SetFinished(Response.Finished finished)
        {
            throw new NotImplementedException();
        }


        public IResponseBuilder SetExecution(Response.Execution execution)
        {
            throw new NotImplementedException();
        }

        public IResponseBuilder AddDetail(string detail)
        {
            throw new NotImplementedException();
        }

        public IResponseBuilder SetResponseDeliveryListener(IResponseDeliveryListener responseDeliveryListener)
        {
            throw new NotImplementedException();
        }
        
        public IResponseBuilder SetIgnoreSleep()
        {
            throw new NotImplementedException();
        }

        public Response Build()
        {
            throw new NotImplementedException();
        }
    }
}