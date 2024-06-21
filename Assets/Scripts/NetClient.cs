using OkamiNet.Network;
using UnityEngine;

public class NetClient : MonoBehaviour
{
    private NetworkManager networkManager;
    void Start()
    {
        networkManager = new NetworkManager();
        NetworkManager.Instance = networkManager;
    }


    void Update()
    {
        networkManager.UpdateClient();
    }
}
