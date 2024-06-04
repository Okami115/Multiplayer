using UnityEngine;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviour
{
    public Text messages;
    public InputField inputMessage;

    private void OnEnable()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        NetworkManager.Instance.newText += UpdateTextBox;
    }

    private void OnDisable()
    {
        NetworkManager.Instance.newText -= UpdateTextBox;
    }

    void UpdateTextBox(string text)
    {
        messages.text += text;
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
                NetConsole consoleMensajes = new NetConsole(NetworkManager.Instance.player.name + ": " + inputMessage.text + System.Environment.NewLine);
                NetworkManager.Instance.SendToServer(consoleMensajes.Serialize());
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }

    }

}
