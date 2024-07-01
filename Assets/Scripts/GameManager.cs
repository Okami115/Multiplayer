using OkamiNet.Menssage;
using OkamiNet.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Separa en archivos (UI manager)
public class GameManager : MonoBehaviour
{
    [SerializeField] public GameObject bullet;
    [SerializeField] public GameObject canvas;

    [SerializeField] private TextMeshProUGUI timerText;

    private float timerInSecons;

    [SerializeField] private InputController input;

    [SerializeField] public List<GameObject> prefabs;

    private NetFloat netTimer;
    private NetworkScreen netScreen;

    private void Start()
    {
        netScreen = FindAnyObjectByType<NetworkScreen>();
        netTimer = new NetFloat();
        netTimer.data = timerInSecons;
        netScreen.start += StartGame;
    }

    private void OnDestroy()
    {
        input.setChat -= SetChatScreen;
        //ClientManager.Instance.updateTimer -= UpdateTimer;
    }

    private void SetChatScreen()
    {
        if (canvas.active)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        canvas.SetActive(!canvas.active);
    }

    private void UpdateTimer(float time)
    {
        timerText.text = ((int)time).ToString();
    }

    private void StartGame()
    {
        timerText.gameObject.SetActive(true);
        input.setChat += SetChatScreen;
        //ClientManager.Instance.updateTimer += UpdateTimer;
        netTimer = new NetFloat();
    }

    /*
     
    Nose donde chota deberia ir esto

    timerInSecons -= Time.deltaTime;

    timerText.text = ((int)timerInSecons).ToString();

    if (timerInSecons <= 0)
        NetworkManager.Instance.Disconnect();

    netTimer.data = timerInSecons;

    NetworkManager.Instance.Broadcast(netTimer.Serialize());
     */
}
