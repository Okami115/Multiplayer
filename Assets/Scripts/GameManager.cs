using TMPro;
using UnityEngine;

// Separa en archivos (UI manager)
public class GameManager : MonoBehaviour
{
    [SerializeField] public GameObject bullet;
    [SerializeField] public GameObject canvas;

    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private float timerInSecons;

    [SerializeField] private InputController input;

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
        NetworkManager.Instance.updateTimer -= UpdateTimer;
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
        NetworkManager.Instance.updateTimer += UpdateTimer;
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
