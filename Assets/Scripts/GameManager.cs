using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public GameObject bullet;
    [SerializeField] public GameObject canvas;


    private void SetChatScreen()
    {
        if (canvas.active)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        canvas.SetActive(!canvas.active);
    }
}
