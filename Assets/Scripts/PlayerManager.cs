using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private GameObject player;
    public List<GameObject> players;
    NetworkScreen netScreen;

    private void OnDestroy()
    {
        netScreen.start -= StartGame;
        NetworkManager.Instance.spawnPlayer -= SpawnNewPlayer;
        NetworkManager.Instance.disconectPlayer -= DeletePlayer;
        NetworkManager.Instance.stopPlayer -= LockPlayer;

        if (!NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.StartMap -= StartMap;
            NetworkManager.Instance.connectPlayer -= SpawnNewPlayer;
        }
    }

    private void OnEnable()
    {
        netScreen = FindAnyObjectByType<NetworkScreen>();

        netScreen.start += StartGame;
    }

    private void Update()
    {
        if(NetworkManager.Instance.isServer) 
        {
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
        }

        if (!NetworkManager.Instance.isServer)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].transform.position = NetworkManager.Instance.playerList[i].pos;

                players[i].transform.rotation = Quaternion.Euler(0, NetworkManager.Instance.playerList[i].rotation.y, 0);
                players[i].transform.GetChild(0).transform.rotation = Quaternion.Euler(NetworkManager.Instance.playerList[i].rotation.x, NetworkManager.Instance.playerList[i].rotation.y, 0);
            }
        }
    }

    private void SpawnNewPlayer(int id)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].transform.name == id.ToString())
                return;
        }

        GameObject aux = Instantiate(player);
        aux.name = id.ToString();

        if (id == NetworkManager.Instance.player.id)
        {
            Camera temp = aux.transform.GetComponentInChildren<Camera>();
            temp.tag = "MainCamera";
            aux.AddComponent<PlayerMovment>();
            aux.AddComponent<CameraMovement>();
            aux.AddComponent<Shoot>();
        }
        else
        {
            Camera temp = aux.transform.GetComponentInChildren<Camera>();

            Destroy(temp);
        }

        players.Add(aux);
    }

    private void StartMap()
    {
        for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
        {
            SpawnNewPlayer(NetworkManager.Instance.playerList[i].id);
        }
    }

    private void LockPlayer()
    {
        gameObject.SetActive(false);
    }

    private void DeletePlayer(int id)
    {
        int ed = 0;

        for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
        {
            if (NetworkManager.Instance.playerList[i].id == id)
                ed = i;
        }

        Destroy(players[ed]);
        players.Remove(players[ed]);
    }

    private void StartGame()
    {
        players = new List<GameObject>();

        if (NetworkManager.Instance.isServer)
        {
            SpawnNewPlayer(NetworkManager.Instance.player.id);
        }

        NetworkManager.Instance.spawnPlayer += SpawnNewPlayer;
        NetworkManager.Instance.disconectPlayer += DeletePlayer;
        NetworkManager.Instance.stopPlayer += LockPlayer;

        if (!NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.StartMap += StartMap;
            NetworkManager.Instance.connectPlayer += SpawnNewPlayer;

        }
    }
}
