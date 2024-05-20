using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(NetworkManager.Instance.isServer && other.tag == "Player")
        {
            for(int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
            {
                if(other.gameObject.name == gameObject.name)
                    return;

                if(other.gameObject.name == NetworkManager.Instance.playerList[i].id.ToString())
                {
                    Player player = NetworkManager.Instance.playerList[i];

                    player.HP--;

                    NetworkManager.Instance.playerList[i] = player;

                    Destroy(gameObject);
                }
            }
        }
    }
}
