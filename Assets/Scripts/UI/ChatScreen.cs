﻿using System;
using System.Diagnostics;
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
                    NetConsole consoleMensajes = new NetConsole("");
                    string text = consoleMensajes.Deserialize(data);
                    NetworkManager.Instance.Broadcast(data);
                    break;

                case MessageType.Position:
                    Debug.WriteLine("Pos");
                    break;

                case MessageType.C2S:
                    Debug.WriteLine("C2S");
                    C2SHandShake C2SHandShake = new C2SHandShake("");
                    string name = C2SHandShake.Deserialize(data);
                    NetworkManager.Instance.AddClient(ep, name);

                    break;

                case MessageType.Disconect:
                    Debug.WriteLine("Disconect");
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
                    NetConsole consoleMensajes = new NetConsole("");
                    string text = consoleMensajes.Deserialize(data);
                    Debug.WriteLine(text);
                    break;

                case MessageType.Position:
                    Debug.WriteLine("Pos");
                    break;

                case MessageType.S2C: 
                    Debug.WriteLine("S2C");
                    break;

                case MessageType.Disconect:
                    Debug.WriteLine("Disconect");
                    break;

                default: 

                    break;
            }
        }

        //messages.text += System.Text.ASCIIEncoding.UTF8.GetString(data) + System.Environment.NewLine;
        //Identifica que tipo de mensaje es
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            if (NetworkManager.Instance.isServer)
            {
                NetworkManager.Instance.Broadcast(System.Text.ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                messages.text += inputMessage.text + System.Environment.NewLine;
            }
            else
            {
                //NetworkManager.Instance.SendToServer(System.Text.ASCIIEncoding.UTF8.GetBytes(inputMessage.text));

                NetConsole consoleMensajes = new NetConsole(inputMessage.text);
                
                NetworkManager.Instance.SendToServer(consoleMensajes.Serialize());
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }

    }

}
