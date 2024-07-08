using OkamiNet.Menssage;
using OkamiNet.Network;
using UnityEngine;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviour
{
    public Text messages;
    public InputField inputMessage;

    private void OnEnable()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        ClientManager.Instance.newText += UpdateTextBox;
    }

    private void OnDisable()
    {
        ClientManager.Instance.newText -= UpdateTextBox;
    }

    void UpdateTextBox(string text)
    {
        messages.text += text;
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            //NetworkManager.Instance.SendToServer(consoleMensajes.Serialize());

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }

    }

}
