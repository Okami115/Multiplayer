using OkamiNet.Menssage;
using OkamiNet.Network;
using OkamiNet.Utils;
using UnityEngine;

public enum Prefabs
{
    Player,
    Bullet
}

public class Tools : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        UtilsTools.LOG += DebugLogs;
        UtilsTools.Intanciate += Instanciate;
        ClientManager.Instance.StartMatch += InitPlayer;
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void OnDestroy()
    {
        UtilsTools.LOG -= DebugLogs;
        UtilsTools.Intanciate -= Instanciate;
        ClientManager.Instance.StartMatch -= InitPlayer;
    }

    private void DebugLogs(string msg)
    {
       Debug.Log(msg);
    }

    private void Instanciate (FactoryData factoryData)
    {
        GameObject aux = Instantiate(gameManager.prefabs[factoryData.prefabId]);

        Debug.Log("Init prefab" + aux.ToString());
        INetObj auxObj = aux.GetComponent<INetObj>();

        OkamiNet.Network.Reflection.netObjets.Add(auxObj);

        Debug.Log("Get component" + auxObj.ToString());
        auxObj.SetID(factoryData.netObj.id);
        auxObj.SetOwner(factoryData.netObj.id);
        Debug.Log($"{aux.name} : Owner {auxObj.getOwner()} : ID {auxObj.getID()}");

        Vector3 newPos = new Vector3();
        Quaternion quat = new Quaternion();
        Vector3 scale = new Vector3();

        newPos.x = factoryData.pos.X; 
        newPos.y = factoryData.pos.Y;
        newPos.z = factoryData.pos.Z;

        quat.x = factoryData.rot.X;
        quat.y = factoryData.rot.Y; 
        quat.z = factoryData.rot.Z;
        quat.w = factoryData.rot.W;

        scale.x = factoryData.scale.X;
        scale.y = factoryData.scale.Y; 
        scale.z = factoryData.scale.Z;

        aux.transform.position = newPos;
        aux.transform.rotation = quat;
        aux.transform.localScale = scale;

        aux.name = auxObj.getID().ToString();

    }

    private void InitPlayer()
    {
        FactoryRequest factoryMenssage = new FactoryRequest();

        NetObj netObj = new NetObj();

        FactoryData factoryData = new FactoryData(netObj, System.Numerics.Vector3.Zero, System.Numerics.Quaternion.Identity, System.Numerics.Vector3.One, 0, 0);

        factoryMenssage.data = factoryData;

        ClientManager.Instance.SendToServer(factoryMenssage.Serialize());
    }
}
