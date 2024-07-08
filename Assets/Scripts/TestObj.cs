using NUnit.Framework;
using NUnit.Framework.Constraints;
using OkamiNet.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TestObj : MonoBehaviour, INetObj
{
    public NetObj netObj = new NetObj();

    [NetValue(0)] public float testValue;

    //[NetValue(1)] public List<float> testList;

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

    private void Start()
    {
        //testList = new List<float>();

        //testList.Add(4f);
        //testList.Add(8f);
    }
}

[Serializable]
public class solci
{
    [NetValue(0)] public float gacha = 0;
}
