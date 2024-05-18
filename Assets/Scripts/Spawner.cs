using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private GameObject player;
    private List<GameObject> players;

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
            }
        }
    }

    private void SpawnNewPlayer(int id)
    {
        GameObject aux = Instantiate(player);

        aux.AddComponent<PlayerMovment>();

        players.Add(aux);
    }

    private void StartMap()
    {
        for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
        {
            players.Add(Instantiate(player));
        }
    }
}
