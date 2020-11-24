using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class UdpBroadcastMessage : IDisposable
    {
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
            _udpClient = new UdpClient();
            _udpClient.Client.ReceiveTimeout = 1000;
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            _udpClient.EnableBroadcast = true;
            _cancellation = new CancellationTokenSource();
            _cancellation.CancelAfter(timeout);
        }

        public List<string> GetResponse()
        {
            try
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
                            Console.WriteLine("READING results");
                        }
                        catch (SocketException se)
                        {
                            if (se.ErrorCode != 10060)  throw;
                        }
                    }
                }, _cancellation.Token);

                Console.WriteLine("Start");
                receive.Start();
                Console.WriteLine("Send");
                _udpClient.Send(_message, _message.Length, new IPEndPoint(IPAddress.Broadcast, _port));
                Task.WaitAll(new[] {receive});
                Console.WriteLine("Waiting");
                return responses;
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Error thrown {exp.Message}");
                throw;
            }

        }

        public void Dispose()
        {
            Console.WriteLine($"Disposing");
            _udpClient?.Dispose();
            _cancellation?.Dispose();
        }
    }
}
