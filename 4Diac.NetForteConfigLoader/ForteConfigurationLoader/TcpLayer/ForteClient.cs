using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ForteConfigurationLoader.TcpLayer
{
    public class ForteClient : IForteClient
    {
        public bool IsConnected { get; private set; } = false;
        public event OnRecieve OnReceiveHandler;
        private TcpClient _client;

        private BinaryWriter _bWriter;
        private BinaryReader _bReader;
        
        public bool Connect(string server, int port)
        {
            try
            {
                bool bIsOk = IPAddress.TryParse(server, out var adress);
                if (!bIsOk)
                    return false;

                _client = new TcpClient();
       
  
#if DEBUG
                _client.ReceiveTimeout = (int)TimeSpan.FromSeconds(10.0).TotalMilliseconds;
#else
                 _client.ReceiveTimeout = (int)TimeSpan.FromSeconds(1.0).TotalMilliseconds;
#endif
                
                _client.Connect(adress, port);

                IsConnected = _client.Connected;
                if (!IsConnected)
                    return false;

                _bWriter = new BinaryWriter(_client.GetStream(), Encoding.ASCII);
                _bReader = new BinaryReader(_client.GetStream(), Encoding.ASCII);
               
                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        public void SendData(byte[] bytesSent)
        {
            if (!_client.Connected)
                return;
            
            _bWriter.Write(bytesSent, 0,  bytesSent.Length);
            
            const int CHUNK_SIZE = 1024;
            byte[] bytesReceived = new byte[CHUNK_SIZE];
            int bytes = 0;
            List<byte> resultData = new List<byte>();
            do
            {
                try
                {
                    bytes = _bReader.Read(bytesReceived, 0, bytesReceived.Length);
                    if (bytes > 0)
                    {
                        resultData.AddRange( bytesReceived.Take(bytes));
                        
                        if (CHUNK_SIZE > bytes)
                            break;
                    }
                }
                catch (Exception)
                {
                    break;
                }
            } while (bytes > 0);
            
            //if we have a content
             if (resultData.Count > 0 &&
                 OnReceiveHandler != null)
             {
                 OnReceiveHandler(resultData.ToArray());
             }
        }

        public void Close()
        {
            // Release the socket.
            if (_client != null && IsConnected)
            {
                IsConnected = false;
                _bWriter.Close();
                _bReader.Close();
                _client.Close();
                _client = null;
            }
        }

        public void Dispose() =>  Close();

    }
}