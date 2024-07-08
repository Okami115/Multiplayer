using OkamiNet.Menssage;
using OkamiNet.Network;
using UnityEngine;

public class PlayerMovment : NetObj
{
    [SerializeField] private float speed = 5;

    public Rigidbody rb;

    private PlayerManager playerManager;

    private void Start()
    {
        speed = 20;
    }

    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal");
        float movimientoVertical = Input.GetAxis("Vertical");

    }


}
