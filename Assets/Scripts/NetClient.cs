using OkamiNet.Network;
using UnityEngine;

public class NetClient : MonoBehaviour
{
    private NetworkManager networkManager;
    void Awake()
    {
        networkManager = new NetworkManager();
        NetworkManager.Instance = networkManager;
    }


    void Update()
    {
        networkManager.UpdateClient();
    }
}
