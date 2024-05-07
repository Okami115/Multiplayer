using System;
using System.Net;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        this.gameObject.SetActive(false);

        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
    }

    void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
    {
        MessageType aux = (MessageType)BitConverter.ToInt32(data, 0);

        if (NetworkManager.Instance.isServer)
        {
            switch (aux)
            {
                case MessageType.Console:
                    UnityEngine.Debug.Log("New mensages to clients");
                    NetworkManager.Instance.Broadcast(data);
                    NetConsole consoleMensajes = new NetConsole("");
                    string text = consoleMensajes.Deserialize(data);
                    messages.text += text;
                    UnityEngine.Debug.Log(text);
                    break;

                case MessageType.Position:
                    UnityEngine.Debug.Log("Pos");
                    break;

                case MessageType.C2S:
                    UnityEngine.Debug.Log("New C2S");
                    C2SHandShake C2SHandShake = new C2SHandShake("");
                    string name = C2SHandShake.Deserialize(data);
                    NetworkManager.Instance.AddClient(ep, name);

                    S2CHandShake s2CHandShake = new S2CHandShake(NetworkManager.Instance.playerList);
                    byte[] players = s2CHandShake.Serialize();
                    NetworkManager.Instance.Broadcast(players);
                    UnityEngine.Debug.Log("Send S2C");

                    break;

                case MessageType.Disconect:
                    UnityEngine.Debug.Log("Disconect");
                    break;

                default:

                    break;
            }
        }
        else
        {
            switch (aux)
            {
                case MessageType.Console:
                    UnityEngine.Debug.Log("New mensages to server");
                    NetConsole consoleMensajes = new NetConsole("");
                    string text = consoleMensajes.Deserialize(data);
                    messages.text += text;
                    UnityEngine.Debug.Log(text);
                    break;

                case MessageType.Position:
                    UnityEngine.Debug.Log("Pos");
                    NetVector3 v3 = new NetVector3(data);
                    break;

                case MessageType.S2C:
                    UnityEngine.Debug.Log("New S2C");
                    S2CHandShake s2cHandShake = new S2CHandShake(NetworkManager.Instance.playerList);
                    NetworkManager.Instance.playerList = s2cHandShake.Deserialize(data);

                    for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
                        if (NetworkManager.Instance.player.name == NetworkManager.Instance.playerList[i].name)
                            NetworkManager.Instance.player.id = NetworkManager.Instance.playerList[i].id;
                 
                    UnityEngine.Debug.Log("Updating player list...");

                    break;

                case MessageType.Disconect:
                    UnityEngine.Debug.Log("Disconect");
                    break;

                default: 

                    break;
            }
        }
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            if (NetworkManager.Instance.isServer)
            {
                NetConsole consoleMensajes = new NetConsole("Server: " + inputMessage.text + System.Environment.NewLine);
                messages.text += consoleMensajes.data;
                NetworkManager.Instance.Broadcast(consoleMensajes.Serialize());
            }
            else
            {
                //NetworkManager.Instance.SendToServer(System.Text.ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                NetConsole consoleMensajes = new NetConsole(NetworkManager.Instance.player.name + ": " + inputMessage.text + System.Environment.NewLine);
                NetworkManager.Instance.SendToServer(consoleMensajes.Serialize());
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }

    }

}
