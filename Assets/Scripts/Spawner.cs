using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private GameObject player;
    public List<GameObject> players;

    private void Start()
    {
        NetworkManager.Instance.spawnPlayer += SpawnNewPlayer;
        players = new List<GameObject>();

        if(NetworkManager.Instance.isServer)
        {
            SpawnNewPlayer(NetworkManager.Instance.player.id);
        }
        else
        {
            NetworkManager.Instance.StartMap += StartMap;
        }
    }

    private void Update()
    {
        if(!NetworkManager.Instance.isServer)
        {
            for(int i = 0; i < players.Count; i++) 
            {
                players[i].transform.position = NetworkManager.Instance.playerList[i].pos;

                players[i].transform.rotation = Quaternion.Euler(0, NetworkManager.Instance.playerList[i].rotation.y, 0);
                players[i].transform.GetChild(0).transform.rotation = Quaternion.Euler(NetworkManager.Instance.playerList[i].rotation.x, NetworkManager.Instance.playerList[i].rotation.y, 0);
            }
        }
    }

    private void SpawnNewPlayer(int id)
    {
        GameObject aux = Instantiate(player);

        aux.name = id.ToString();

        if(id == NetworkManager.Instance.player.id)
        {
            Camera temp = aux.transform.GetComponentInChildren<Camera>();
            temp.tag = "MainCamera";
            aux.AddComponent<PlayerMovment>();
            aux.AddComponent<CameraMovement>();
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
}
