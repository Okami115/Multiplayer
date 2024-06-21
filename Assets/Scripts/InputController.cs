using OkamiNet.Network;
using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public Action setChat;
    public Action quit;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            NetworkManager.Instance.Disconnect();
            quit?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            setChat?.Invoke();
        }
    }
}
