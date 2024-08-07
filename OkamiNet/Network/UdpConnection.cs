using OkamiNet.data;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace OkamiNet.Network
{
    public class UdpConnection
    {
        private struct DataReceived
        {
            public byte[] data;
            public IPEndPoint ipEndPoint;
        }

        private UdpClient connection;
        private IReceiveData receiver = null;
        private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

        private bool isDisposed = false;
        object handler = new object();

        public UdpConnection(int port, IReceiveData receiver = null)
        {
            connection = new UdpClient(port);

            this.receiver = receiver;

            connection.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            connection.BeginReceive(OnReceive, null);
        }

        public UdpConnection(IPAddress ip, int port, IReceiveData receiver = null)
        {
            connection = new UdpClient();
            connection.Connect(ip, port);

            this.receiver = receiver;

            connection.BeginReceive(OnReceive, null);
        }

        public void Close()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                dataReceivedQueue.Clear();
                connection.Dispose();
                connection.Close();
            }
        }

        public void FlushReceiveData()
        {
            while (dataReceivedQueue.Count > 0)
            {
                DataReceived dataReceived = dataReceivedQueue.Dequeue();
                if (receiver != null)
                    receiver.OnReceiveData(dataReceived.data, dataReceived.ipEndPoint);
            }
        }

        void OnReceive(IAsyncResult ar)
        {
            DataReceived dataReceived = new DataReceived();

            try
            {
                if (!isDisposed)
                {
                    dataReceived.data = connection.EndReceive(ar, ref dataReceived.ipEndPoint);
                }

            }
            catch (SocketException e)
            {
                UtilsTools.LOG?.Invoke("[UdpConnection] " + e.Message);
            }
            finally
            {
                lock (handler)
                {
                    dataReceivedQueue.Enqueue(dataReceived);
                }

                connection.BeginReceive(OnReceive, null);
            }
        }

        public void Send(byte[] data)
        {
            if (connection.Client != null && !isDisposed)
                connection.Send(data, data.Length);
        }

        public void Send(byte[] data, IPEndPoint ipEndpoint)
        {
            if (connection.Client != null && !isDisposed)
                connection.Send(data, data.Length, ipEndpoint);
        }
    }

}