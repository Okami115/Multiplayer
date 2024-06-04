using System.Net;

// Go to DLL
public interface IReceiveData
{
    void OnReceiveData(byte[] data, IPEndPoint ipEndpoint);
}