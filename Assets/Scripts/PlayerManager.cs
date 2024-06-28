using OkamiNet.Menssage;
using OkamiNet.Network;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private GameObject player;
    NetworkScreen netScreen;

    private void OnEnable()
    {
        netScreen = FindAnyObjectByType<NetworkScreen>();
        netScreen.start += StartGame;
    }
    private void OnDestroy()
    {
        netScreen.start -= StartGame;
        ClientManager.Instance.stopPlayer -= LockPlayer;
    }

    private void LockPlayer()
    {
        gameObject.SetActive(false);
    }

    private void StartGame()
    {
        ClientManager.Instance.stopPlayer += LockPlayer;
    }

    /*
    
    Desconectar players si su vida es 0.
        
    Esto deberia ir en el update de cada jugador, cuando detecta que se quedo sin vida, se desconecta solo.

    for (int i = 1; i < NetworkManager.Instance.playerList.Count; i++) 
    {
        if (NetworkManager.Instance.playerList[i].HP <= 0)
        {
            NetDisconect dis = new NetDisconect();
                
            IPEndPoint ip = NetworkManager.Instance.GetIpById(NetworkManager.Instance.playerList[i].id);
            dis.data = NetworkManager.Instance.playerList[i].id;
            NetworkManager.Instance.Broadcast(dis.Serialize());
            NetworkManager.Instance.RemoveClient(NetworkManager.Instance.playerList[i].id, ip);
        }
    }

     */
}
