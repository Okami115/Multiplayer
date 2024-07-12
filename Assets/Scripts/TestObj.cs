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

    // Check
    //[NetValue(0)] public solci testValue = new solci();
    // Check [NetValue(1)] public bool testBool;
    // Check [NetValue(2)] public string myString = "pepe";
    // Check [NetValue(3)] public char myChar = 'a';
    [NetValue(4)] public decimal myDecimal;
    // Check [NetValue(5)] public double myDouble = 1;
    // Check [NetValue(6)] public short myShort = 1;
    // Check [NetValue(7)] public ushort myUShort = 1;
    // Check [NetValue(8)] public int myInt = 1;
    // Check [NetValue(9)] public uint myUInt = 1;
    // Check [NetValue(10)] public long myLong = 1;
    // Check [NetValue(11)] public ulong myULong = 1;
    // Check [NetValue(12)] public byte myByte = 1;
    // Check [NetValue(13)] public sbyte mySByte = 1;
    //[NetValue(14)] public List<float> testList;

    private Vector3Pro position;


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

        //testList.Add(1);
        //testList.Add(2);
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

        //Debug.Log($"Solci es : {testValue == null}");

        Debug.Log($"Decimal es : {myDecimal}");
    }

    [ContextMenu("Matar lista")]
    private void NullList()
    {
        //testList = null;
    }

    [ContextMenu("Matar Sol")]
    private void NullSol()
    {
        //testValue = null;
    }

    [ContextMenu("Crear Sol")]
    private void Sol()
    {
        //testValue = new solci();
    }

    [ContextMenu("Sumar Decimal")]
    private void SumarDecimal()
    {
        myDecimal++;
    }

    [ContextMenu("Restar Decimal")]
    private void RestarDecimal()
    {
        myDecimal--;
    }
}

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
