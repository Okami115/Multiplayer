using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviour
{
    public Button connectBtn;
    public InputField addressInputField;
    public InputField nameInputField;
    public GameObject messages;
    public GameObject messages2;

    private IPAddress ipAddress;
    private string playerName;

    public Action start;

    private void OnDestroy()
    {
        NetworkManager.Instance.deniedConnection -= SwitchToChatScreen;
    }

    private void Start()
    {
#if UNITY_SERVER
        gameObject.SetActive(false);
#endif
        connectBtn.onClick.AddListener(OnConnectBtnClick);
    }

    void OnConnectBtnClick()
    {
        ipAddress = IPAddress.Parse(addressInputField.text);
        playerName = Convert.ToString(nameInputField.text);

        C2SHandShake c2SHandShake = new C2SHandShake(playerName);

        NetworkManager.Instance.deniedConnection += SwitchToChatScreen;
        NetworkManager.Instance.StartClient(ipAddress, 55555, c2SHandShake.data);
        NetworkManager.Instance.SendToServer(c2SHandShake.Serialize());
    }

    // Sintetizar
    void SwitchToChatScreen(string set)
    {
        if(set == "Authorized")
        {
            start?.Invoke();
            gameObject.SetActive(false);
            NetworkManager.Instance.player.name = playerName;
        }

        if (set == "Name")
        {
            messages.SetActive(true);
            messages2.SetActive(false);
        }
        
        if(set == "Full")
        {
            messages.SetActive(false);
            messages2.SetActive(true);
        }

    }
}
