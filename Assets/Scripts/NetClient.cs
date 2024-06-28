using OkamiNet.Network;
using UnityEngine;

public class NetClient : MonoBehaviour
{
    private ClientManager clientManager;
    void Awake()
    {
        clientManager = new ClientManager();
        ClientManager.Instance = clientManager;
    }


    void Update()
    {
        clientManager.UpdateClient();
    }
}
