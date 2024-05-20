
using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;
    public InputField nameInputField;
    public GameObject messages;
    public GameObject messages2;

    private IPAddress ipAddress;
    private int port;
    private string name;

    public Action start;

    private void OnDestroy()
    {
        NetworkManager.Instance.deniedConnection -= SwitchToChatScreen;
    }

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        ipAddress = IPAddress.Parse(addressInputField.text);
        port = System.Convert.ToInt32(portInputField.text);
        name = System.Convert.ToString(nameInputField.text);

        C2SHandShake c2SHandShake = new C2SHandShake(name);

        NetworkManager.Instance.deniedConnection += SwitchToChatScreen;
        NetworkManager.Instance.StartClient(ipAddress, port, c2SHandShake.data);
        NetworkManager.Instance.SendToServer(c2SHandShake.Serialize());
    }

    void OnStartServerBtnClick()
    {
        port = System.Convert.ToInt32(portInputField.text);
        NetworkManager.Instance.StartServer(port);
        SwitchToChatScreen("Authorized");
    }

    void SwitchToChatScreen(string set)
    {
        if(set == "Authorized")
        {
            start?.Invoke();
            gameObject.SetActive(false);
            if(!NetworkManager.Instance.isServer)
                NetworkManager.Instance.player.name = name;
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
