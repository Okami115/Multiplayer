using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float SpeedRotation;

    private PlayerManager playerManager;

    private NetVector2 rotationData;

    private void OnEnable()
    {
        NetworkManager.Instance.updateRot += RotatePlayer;
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.updateRot -= RotatePlayer;
    }

    private void Start()
    {
        rotationData = new NetVector2();
        Cursor.lockState = CursorLockMode.Locked;
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotationData.data.Y += mouseX;

        rotationData.data.X -= mouseY;

        rotationData.data.X = Mathf.Clamp(rotationData.data.X, -90f, 90f);

        NetworkManager.Instance.SendToServer(rotationData.Serialize());
    }

    private void RotatePlayer(System.Numerics.Vector2 newRotation, int id)
    {
        int ed = 0;

        for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
        {
            if (NetworkManager.Instance.playerList[i].id == id)
                ed = i;
        }

        Player character = NetworkManager.Instance.playerList[ed];

        playerManager.players[ed].transform.rotation = Quaternion.Euler(0, newRotation.Y, 0);
        playerManager.players[ed].transform.GetChild(0).transform.rotation = Quaternion.Euler(newRotation.X, newRotation.Y, 0);

        character = NetworkManager.Instance.playerList[ed];

        character.rotation = newRotation;

        NetworkManager.Instance.playerList[ed] = character;
    }

    /*
     
    Esto lo deberia hacer cada player por su cuenta y luego updatear el resultado o tambien mandar el input

    Player character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

    transform.rotation = Quaternion.Euler(0, rotationData.data.y, 0);
    transform.GetChild(0).transform.rotation = Quaternion.Euler(rotationData.data.x, rotationData.data.y, 0);

    character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

    character.rotation = rotationData.data;

    NetworkManager.Instance.playerList[NetworkManager.Instance.player.id] = character; 

     */
}
