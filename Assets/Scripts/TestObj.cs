using NUnit.Framework.Constraints;
using OkamiNet.Network;
using UnityEngine;

public class TestObj : MonoBehaviour, INetObj
{
    public NetObj netObj = new NetObj();

    [NetValue(0)] public float testFloat;
    [NetValue(1)] public int testInt;

    public int getID()
    {
        return netObj.id;
    }

    public NetObj getNetObj()
    {
        return netObj;
    }

    public int getOwner()
    {
        return netObj.owner;
    }

    public void SetID(int id)
    {
        netObj.id = id;
    }

    public void SetOwner(int owner)
    {
        netObj.owner = owner;
    }

    private void Update()
    {
        Debug.LogWarning($"I {ClientManager.Instance.idClient} try read : Test Obj Owner to read : {netObj.owner}  ID : {netObj.id}");
    }
}
