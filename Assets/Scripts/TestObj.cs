using NUnit.Framework;
using NUnit.Framework.Constraints;
using OkamiNet.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TestObj : MonoBehaviour, INetObj
{
    public NetObj netObj = new NetObj();

    private Rigidbody rb;

    //[NetValue(0)] public float testValue;

    [NetValue(0)] public bool testBool;

    private Vector3Pro position;

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
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        /*
        if (ClientManager.Instance.idClient == getOwner())
        {
            float movimientoHorizontal = Input.GetAxis("Horizontal");
            float movimientoVertical = Input.GetAxis("Vertical");

            rb.AddForce(movimientoHorizontal * 100, 0, movimientoVertical * 100);

            position.X = rb.position.x;
            position.Y = rb.position.y;
            position.Z = rb.position.z;
        }
        else
        {
            Vector3 newPos = new Vector3();

            newPos.x = position.X;
            newPos.y = position.Y;
            newPos.z = position.Z;

            transform.position = newPos;
        }
        */
    }
}

[Serializable]
public class solci
{
    [NetValue(0)] public float gacha = 0;
}

[Serializable]
public class Vector3Pro
{
    public float X;
    public float Y;
    public float Z;

    Vector3Pro(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
