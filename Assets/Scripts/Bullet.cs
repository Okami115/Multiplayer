using OkamiNet.Network;
using UnityEngine;

// Optimizar
public class Bullet : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Instance.isServer && other.tag == "Player")
        {
            
        }
    }
}
