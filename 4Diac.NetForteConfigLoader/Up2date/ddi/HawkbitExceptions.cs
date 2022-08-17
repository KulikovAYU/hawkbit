namespace Up2date.ddi
{
    public class ClientInitiailizeError : Exception
    {
        private string _message;
        
        public ClientInitiailizeError(string message)
        {
            _message = message;
        }

        public override string Message  => $"{_message}\t {base.Message}";
    }
    
    public class HttpRequestError : Exception
    {
        private string _message;
        
        public HttpRequestError(int message)
        {
            _message = $"HTTP request error. Error code {message}";
        }

        public override string Message  => _message;
    }
    
    public class UnauthorizedException : Exception
    {
        public override string Message  => $"Got {Constants.HTTP_UNAUTHORIZED} code";
    }
}