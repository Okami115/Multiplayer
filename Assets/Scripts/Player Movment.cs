using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    [SerializeField] private float speed = 5;

    public Rigidbody rb;

    private PlayerManager playerManager;

    private NetVector3 pos;

    private void OnEnable()
    {
        NetworkManager.Instance.updatePos += MovePlayer;
    }

    private void Start()
    {
        pos = new NetVector3();
        speed = 5;
        rb = GetComponent<Rigidbody>();
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal");
        float movimientoVertical = Input.GetAxis("Vertical");

        pos.data = new Vector3(movimientoHorizontal, 0.0f, movimientoVertical);

        if (NetworkManager.Instance.isServer)
        {
            pos.data = Camera.main.transform.TransformDirection(pos.data);
            pos.data.y = 0.0f;

            rb.AddForce(pos.data * speed);

            Player character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

            character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

            character.pos = transform.position;

            NetworkManager.Instance.playerList[NetworkManager.Instance.player.id] = character;
        }
        else
        {
            NetworkManager.Instance.SendToServer(pos.Serialize());
        }
    }

    private void MovePlayer(Vector3 newPos, int id)
    {
        int ed = 0;

        for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
        {
            if (NetworkManager.Instance.playerList[i].id == id)
                ed = i;
        }

        Vector3 pos = newPos;

        pos = playerManager.players[ed].transform.GetChild(0).transform.TransformDirection(pos);
        pos.y = 0.0f;

        Rigidbody RB = playerManager.players[ed].GetComponent<Rigidbody>();

        RB.AddForce(pos * speed);

        Player character = NetworkManager.Instance.playerList[ed];

        character = NetworkManager.Instance.playerList[ed];

        character.pos = playerManager.players[ed].transform.position;

        NetworkManager.Instance.playerList[ed] = character;
    }
}
