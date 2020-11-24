using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBrowserClient
{
    internal class UdpBroadcastMessage : IDisposable
    {
        private const int SocketTimeoutExceptionCode = 10060;

        private readonly int _port;
        private readonly byte[] _message;
        private readonly TimeSpan _timeout;
        private UdpClient _udpClient;
        private CancellationTokenSource _cancellation;

        public UdpBroadcastMessage(int port, byte[] message, TimeSpan timeout)
        {
            _port = port;
            _message = message;
            _timeout = timeout;
            _udpClient = new UdpClient { Client = { ReceiveTimeout = 1000 } };
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            _udpClient.EnableBroadcast = true;
            _cancellation = new CancellationTokenSource();
            _cancellation.CancelAfter(timeout);
        }

        public List<string> GetResponse()
        {
            var responses = new List<string>();

            var receive = new Task((cancelToken) =>
            {
                var anyEndPoint = new IPEndPoint(IPAddress.Any, 0);
                while (!_cancellation.IsCancellationRequested)
                {
                    try
                    {
                        var receiveBuffer = _udpClient.Receive(ref anyEndPoint);
                        responses.Add(Encoding.UTF8.GetString(receiveBuffer));
                    }
                    catch (SocketException se)
                    {
                        if (se.ErrorCode != SocketTimeoutExceptionCode) throw;
                    }
                }
            }, _cancellation.Token);

            receive.Start();
            _udpClient.Send(_message, _message.Length, new IPEndPoint(IPAddress.Broadcast, _port));
            Task.WaitAll(new[] { receive });
            return responses;
        }

        public void Dispose()
        {
            _udpClient?.Dispose();
            _cancellation?.Dispose();
        }
    }
}
