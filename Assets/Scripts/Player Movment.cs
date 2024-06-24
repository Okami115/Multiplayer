using OkamiNet.Menssage;
using OkamiNet.Network;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    [SerializeField] private float speed = 5;

    public Rigidbody rb;

    private PlayerManager playerManager;

    private NetVector3 pos;

    private void OnEnable()
    {
        NetworkManager.Instance.stopPlayer += LockPlayer;
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.stopPlayer -= LockPlayer;
    }

    private void Start()
    {
        pos = new NetVector3();
        speed = 20;
        rb = GetComponent<Rigidbody>();
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal");
        float movimientoVertical = Input.GetAxis("Vertical");

        pos.data = new System.Numerics.Vector3(movimientoHorizontal, 0.0f, movimientoVertical);

        Move();
    }

    private void Move()
    {
        Vector3 aux = new Vector3();

        aux.x = pos.data.X;
        aux.y = pos.data.Y;
        aux.z = pos.data.Z;

        aux = Camera.main.transform.TransformDirection(aux);
        aux.y = 0.0f;

        rb.AddForce(aux * speed);

        Player character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

        character = NetworkManager.Instance.playerList[NetworkManager.Instance.player.id];

        character.pos.X = transform.position.x;
        character.pos.Y = transform.position.y;
        character.pos.Z = transform.position.z;

        NetworkManager.Instance.playerList[NetworkManager.Instance.player.id] = character;

        pos.data = character.pos;

        NetworkManager.Instance.SendToServer(pos.Serialize(NetworkManager.Instance.player.id));
    }

    private void LockPlayer()
    {
        gameObject.SetActive(false);
    }
}
