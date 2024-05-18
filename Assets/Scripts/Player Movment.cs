using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    [SerializeField] private float speed = 5;

    public GameObject player;
    public Rigidbody rb;

    private NetVector3 pos;

    public PlayerMovment(Rigidbody rb, GameObject player)
    {
        this.rb = rb;
        this.player = player;  
    }

    private void OnEnable()
    {
        NetworkManager.Instance.updatePos += MovePlayer;
    }

    private void Start()
    {
        pos = new NetVector3();
        speed = 5;
        rb = GetComponent<Rigidbody>();
        player = gameObject;
    }

    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal");
        float movimientoVertical = Input.GetAxis("Vertical");

        pos.data = new Vector3(movimientoHorizontal, 0.0f, movimientoVertical);
        
        if(NetworkManager.Instance.isServer)
        {
            pos.data = Camera.main.transform.TransformDirection(pos.data);
            pos.data.y = 0.0f;

            rb.AddForce(pos.data * speed);

            Player character = new Player();

            character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

            character.pos = player.transform.position;

            NetworkManager.Instance.playerList[NetworkManager.Instance.player.id] = character;
        }
        else
        {
            NetworkManager.Instance.SendToServer(pos.Serialize());
        }
    }

    private void MovePlayer(Vector3 newPos, int id)
    {
        Vector3 pos = newPos;

        pos = Camera.main.transform.TransformDirection(pos);
        pos.y = 0.0f;

        rb.AddForce(pos * speed);

        Player character = new Player();

        character = NetworkManager.Instance.playerList[id];

        character.pos = player.transform.position;

        NetworkManager.Instance.playerList[id] = character;
    }
}
