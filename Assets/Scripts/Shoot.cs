using UnityEngine;

public class Shoot : MonoBehaviour
{
    private GameManager gameManager;
    private PlayerManager playerManager;

    private NetShoot shootData;

    private void OnEnable()
    {
        NetworkManager.Instance.updateShoot += ShootBullet;
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.updateShoot -= ShootBullet;
    }

    void Start()
    {
        shootData = new NetShoot();
        gameManager = FindAnyObjectByType<GameManager>();
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            if(NetworkManager.Instance.isServer)
            {
                ShootBullet(NetworkManager.Instance.player.id);
            }
            else
            {
                shootData.data = NetworkManager.Instance.player.id;
                NetworkManager.Instance.SendToServer(shootData.Serialize());
            }
        }
    }

    private void ShootBullet(int id)
    {
        int ed = 0;

        for (int i = 0; i < NetworkManager.Instance.playerList.Count; i++)
        {
            if (NetworkManager.Instance.playerList[i].id == id)
                ed = i;
        }

        GameObject aux = Instantiate(gameManager.bullet, playerManager.players[ed].transform.GetChild(0).transform.position, Quaternion.identity);

        Rigidbody rb = aux.GetComponent<Rigidbody>();

        rb.AddForce(playerManager.players[ed].transform.GetChild(0).transform.forward * 1000);

        Destroy(aux, 3);

        shootData.data = id;

        if(NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(shootData.Serialize());
        }
    }
}
