using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float SpeedRotation;

    private Spawner spawner;

    private NetRotation rotationData;

    private void OnEnable()
    {
        NetworkManager.Instance.updateRot += RotatePlayer;
    }

    private void Start()
    {
        rotationData = new NetRotation();
        Cursor.lockState = CursorLockMode.Locked;
        spawner = FindFirstObjectByType<Spawner>();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotationData.data.y += mouseX;

        rotationData.data.x -= mouseY;

        rotationData.data.x = Mathf.Clamp(rotationData.data.x, -90f, 90f);


        if (NetworkManager.Instance.isServer)
        {
            Player character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

            transform.rotation = Quaternion.Euler(0, rotationData.data.y, 0);
            transform.GetChild(0).transform.rotation = Quaternion.Euler(rotationData.data.x, rotationData.data.y, 0);

            character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

            character.rotation = rotationData.data;

            NetworkManager.Instance.playerList[NetworkManager.Instance.player.id] = character;
        }
        else
        {
            NetworkManager.Instance.SendToServer(rotationData.Serialize());
        }
    }

    private void RotatePlayer(Vector2 newRotation, int id)
    {
        Player character = NetworkManager.Instance.playerList[id];

        spawner.players[id].transform.rotation = Quaternion.Euler(0, newRotation.y, 0);
        spawner.players[id].transform.GetChild(0).transform.rotation = Quaternion.Euler(newRotation.x, newRotation.y, 0);

        character = NetworkManager.Instance.playerList[id];

        character.rotation = newRotation;

        NetworkManager.Instance.playerList[id] = character;
    }
}
