using OkamiNet.Menssage;
using OkamiNet.Network;
using UnityEngine;

public class PlayerMovment : NetObj
{
    [SerializeField] private float speed = 5;

    public Rigidbody rb;

    private PlayerManager playerManager;

    private NetVector3 pos;

    private void Start()
    {
        pos = new NetVector3();
        speed = 20;
    }

    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal");
        float movimientoVertical = Input.GetAxis("Vertical");

        pos.data = new System.Numerics.Vector3(movimientoHorizontal, 0.0f, movimientoVertical);

    }


}
