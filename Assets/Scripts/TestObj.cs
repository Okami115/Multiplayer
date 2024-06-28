using OkamiNet.Network;
using UnityEngine;

public class TestObj : MonoBehaviour, INetObj
{
    private NetObj netObj;

    [NetValue(0)] public float testFloat;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
