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

    private void Start()
    {
        clientManager.StartClient();
    }

    void Update()
    {
        clientManager.UpdateClient();
    }
}
