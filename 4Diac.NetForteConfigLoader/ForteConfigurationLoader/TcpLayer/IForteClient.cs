using System;

namespace ForteConfigurationLoader.TcpLayer
{
    public delegate void OnRecieve(byte[] bytes);

    public interface IForteClient : IDisposable
    {
        bool IsConnected { get; }
        public event OnRecieve OnReceiveHandler;
        bool Connect(string server,int port);
        void Close();
        void SendData(byte[] bytesSent);
    }
}