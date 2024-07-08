using OkamiNet.Menssage;
using OkamiNet.Network;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    private GameManager gameManager;
    private PlayerManager playerManager;

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
        gameManager = FindAnyObjectByType<GameManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            //NetworkManager.Instance.SendToServer(shootData.Serialize());
        }
    }

    private void ShootBullet(int id)
    {

    }
}
