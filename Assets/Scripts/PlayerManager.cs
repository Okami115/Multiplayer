using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private GameObject player;
    public List<GameObject> players;
    NetworkScreen netScreen;

    private void OnEnable()
    {
        netScreen = FindAnyObjectByType<NetworkScreen>();

        netScreen.start += StartGame;
    }
    private void OnDestroy()
    {
        netScreen.start -= StartGame;
        NetworkManager.Instance.spawnPlayer -= SpawnNewPlayer;
        NetworkManager.Instance.disconectPlayer -= DeletePlayer;
        NetworkManager.Instance.stopPlayer -= LockPlayer;

        NetworkManager.Instance.StartMap -= StartMap;
        NetworkManager.Instance.connectPlayer -= SpawnNewPlayer;
    }


    private void Update()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Vector3 pos = new Vector3();

            pos.x = NetworkManager.Instance.playerList[i].pos.X;
            pos.y = NetworkManager.Instance.playerList[i].pos.Y;
            pos.z = NetworkManager.Instance.playerList[i].pos.Z;

            players[i].transform.position = pos;

            Vector2 rot = new Vector2();

            rot.x = NetworkManager.Instance.playerList[i].rotation.X;
            rot.y = NetworkManager.Instance.playerList[i].rotation.Y;

            players[i].transform.rotation = Quaternion.Euler(0, rot.y, 0);
            players[i].transform.GetChild(0).transform.rotation = Quaternion.Euler(rot.x, rot.y, 0);
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

        NetworkManager.Instance.spawnPlayer += SpawnNewPlayer;
        NetworkManager.Instance.disconectPlayer += DeletePlayer;
        NetworkManager.Instance.stopPlayer += LockPlayer;

        NetworkManager.Instance.StartMap += StartMap;
        NetworkManager.Instance.connectPlayer += SpawnNewPlayer;
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
