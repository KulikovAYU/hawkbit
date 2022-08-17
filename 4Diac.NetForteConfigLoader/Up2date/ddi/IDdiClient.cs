namespace Up2date.ddi
{
    public interface IAuthRestoreHandler
    {
        void SetEndPoint(string hawkbitEndpoint);

        void SetDeviceToken(string deviceToken);
        void SetGatewayToken(string gatewayToken);
    }

    public class AuthRestoreHandler : IAuthRestoreHandler
    {
        private HawkbitCommunicationClient _cli;

        public AuthRestoreHandler(HawkbitCommunicationClient cli_)
        {
            _cli = cli_;
        }

        public void SetEndPoint(string hawkbitEndpoint)
        {
            _cli.SetEndPoint(hawkbitEndpoint);
        }

        public void SetDeviceToken(string deviceToken)
        {
            _cli.SetDeviceToken(deviceToken);
        }

        public void SetGatewayToken(string gatewayToken)
        {
            _cli.SetGatewayToken(gatewayToken);
        }
    }

    public interface IAuthErrorHandler
    {
        void OnAuthError(IAuthRestoreHandler authRestoreHandler);
    }

    public interface IClient
    {
        void Run();
    }


    public class HawkbitCommunicationClient : IDownloadProvider, IClient, IAuthRestoreHandler
    {
        private Uri _hawkbitURI;
        private IEventHandler _handler;
        private IAuthErrorHandler _authErrorHandler;

        private int _defaultSleepTime;
        private int _currentSleepTime;

        private bool _ignoreSleep;

        private Dictionary<string, string> _headers = new();

        const string AUTHORIZATION_HEADER = "Authorization";
        const string GATEWAY_TOKEN_HEADER = "GatewayToken";
        const string TARGET_TOKEN_HEADER = "TargetToken";

        private int _pollingTimeout;

        public void Run()
        {
            if (_hawkbitURI == null && _authErrorHandler == null)
                throw new ClientInitiailizeError("endpoint or AuthErrorHandler is not set");


            while (true)
            {
                _ignoreSleep = false;
                DoPoll();
                if (_ignoreSleep && _currentSleepTime > 0)
                    Thread.Sleep(TimeSpan.FromMilliseconds(_currentSleepTime));
            }
        }

        public void SetEndPoint(string hawkbitEndpoint)
        {
            _hawkbitURI = new Uri(hawkbitEndpoint);
        }


        public void SetGatewayToken(string gatewayToken)
        {
            _headers.Add(AUTHORIZATION_HEADER, $"{GATEWAY_TOKEN_HEADER} {gatewayToken}");
        }

        public void DownloadTo(Uri downloadURI, string path)
        {
            throw new NotImplementedException();
        }

        public string GetBody(Uri url)
        {
            // var res = _client.SendAsync().ConfigureAwait(false);
            //HttpResponseMessage
            throw new NotImplementedException();
        }

        public void DownloadWithReceiver(Func<char, int, bool> cb)
        {
            throw new NotImplementedException();
        }

        public void SetEventHandler(IEventHandler handler)
        {
            _handler = handler;
        }

        public void SetDeviceToken(string deviceToken)
        {
            _headers.Add(AUTHORIZATION_HEADER, $"{TARGET_TOKEN_HEADER} {deviceToken}");
        }

        public void SetDefaultPollingTimeoutInMs(int pollingTimeout)
        {
            _pollingTimeout = pollingTimeout;
        }

        private void DoPoll()
        {
            var resp = RetryHandler(_hawkbitURI, client =>
                {
                    var res = client.GetAsync(_hawkbitURI.AbsoluteUri);
                    res.Wait();
                    return res.Result;
                }
            );

            var contents = resp.Content.ReadAsStringAsync();
            contents.Wait();
            var polingData = contents.Result;

            //Pooling
            throw new NotImplementedException("Stopped here Implement");
        }

        private HttpResponseMessage RetryHandler(Uri reqUri, Func<HttpClient, HttpResponseMessage> cbFunc)
        {
            try
            {
                return WrappedRequest(reqUri, cbFunc);
            }
            catch (UnauthorizedException e)
            {
                if (_authErrorHandler == null)
                    throw e;

                _authErrorHandler.OnAuthError(new AuthRestoreHandler(this));
            }

            return WrappedRequest(reqUri, cbFunc);
        }

        private HttpResponseMessage WrappedRequest(Uri reqUri, Func<HttpClient, HttpResponseMessage> cbFunc)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(_pollingTimeout);
            foreach (var (name, value) in _headers)
                client.DefaultRequestHeaders.Add(name, value);

            var resp = cbFunc(client);

            if (resp.IsSuccessStatusCode == false)
                throw new HttpRequestError((int) resp.StatusCode);

            return resp;
        }
    }

    public interface IDDIClientBuilder
    {
        IDDIClientBuilder SetDefaultPollingTimeoutInMs(int pollingTimeout);

        IDDIClientBuilder SetEventHandler(IEventHandler handler);

        IDDIClientBuilder SetGatewayToken(string gatewayToken);

        IDDIClientBuilder SetHawkbitEndpoint(string endpoint,
            string controllerId, string tenant = "default");

        IClient Build();
    }

    public class DefaultClientBuilderImpl : IDDIClientBuilder
    {
        private string _hawkbitUri = null!;

        private IEventHandler _handler = null!;
        private string _token = null!;

        private int _pollingTimeout = 30000;

        public static DefaultClientBuilderImpl NewInstance()
        {
            return new DefaultClientBuilderImpl();
        }

        public IDDIClientBuilder SetDefaultPollingTimeoutInMs(int pollingTimeout)
        {
            _pollingTimeout = pollingTimeout;
            return this;
        }

        public IDDIClientBuilder SetEventHandler(IEventHandler handler)
        {
            _handler = handler;
            return this;
        }

        public IDDIClientBuilder SetGatewayToken(string token)
        {
            _token = token;
            return this;
        }

        public IDDIClientBuilder SetHawkbitEndpoint(string endpoint,
            string controllerId, string tenant = "default")
        {
            _hawkbitUri = Utils.HawkbitEndpointFrom(endpoint, controllerId, tenant);
            return this;
        }

        public IClient Build()
        {
            var cli = new HawkbitCommunicationClient();
            cli.SetEndPoint(_hawkbitUri);
            cli.SetGatewayToken(_token);
            cli.SetEventHandler(_handler);
            cli.SetDefaultPollingTimeoutInMs(_pollingTimeout);
            return cli;
        }

        private DefaultClientBuilderImpl()
        {
        }
    }
}