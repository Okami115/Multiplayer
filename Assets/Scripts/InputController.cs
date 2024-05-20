using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public Action setChat;
    public Action quit;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            NetworkManager.Instance.Disconnect();
            quit?.Invoke();
        }

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            setChat?.Invoke();
        }
    }
}
