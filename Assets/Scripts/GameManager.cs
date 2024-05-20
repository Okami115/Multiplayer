using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public GameObject bullet;
    [SerializeField] public GameObject canvas;

    [SerializeField] private float timer = 0;

    private void Update()
    {
        if(NetworkManager.Instance.isServer)
        {
            timer += Time.deltaTime;

            if(timer >= 120)
                NetworkManager.Instance.Disconnect();
        }
        else
        {
            if(NetworkManager.Instance.player.HP <= 0)
                NetworkManager.Instance.Disconnect();
        }
    }

    private void SetChatScreen()
    {
        if (canvas.active)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        canvas.SetActive(!canvas.active);
    }
}
