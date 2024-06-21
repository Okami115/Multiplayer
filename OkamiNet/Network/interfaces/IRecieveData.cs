using System.Net;

namespace OkamiNet.data
{    
    public interface IReceiveData
    {
        void OnReceiveData(byte[] data, IPEndPoint ipEndpoint);
    }
}