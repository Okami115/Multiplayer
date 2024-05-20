using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private NetDisconect dis;

    private void Start()
    {
        dis = new NetDisconect();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            if(NetworkManager.Instance.isServer)
            {
                for (int i = 1; i < NetworkManager.Instance.playerList.Count; i++)
                {
                    //NetworkManager.Instance.RemoveClient(i);
                }
            }
            else 
            {
                gameObject.SetActive(false);
                dis.data = NetworkManager.Instance.player.id;
                NetworkManager.Instance.SendToServer(dis.Serialize());
            }
        }
    }
}
