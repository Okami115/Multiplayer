using OkamiNet.Menssage;
using OkamiNet.Network;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    private GameManager gameManager;
    private PlayerManager playerManager;

    private NetInt shootData;

    private void OnEnable()
    {
        ClientManager.Instance.updateShoot += ShootBullet;
    }

    private void OnDestroy()
    {
        ClientManager.Instance.updateShoot -= ShootBullet;
    }

    void Start()
    {
        shootData = new NetInt();
        gameManager = FindAnyObjectByType<GameManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            shootData.data = ClientManager.Instance.player.id;
            //NetworkManager.Instance.SendToServer(shootData.Serialize());
        }
    }

    private void ShootBullet(int id)
    {

    }
}
