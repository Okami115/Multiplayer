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


    [NetValue(0)] public List<solci> testValue = new List<solci>();
    /*[NetValue(1)] */public bool testBool;
    /*[NetValue(2)] */public string myString = "pepe";
    /*[NetValue(3)] */public char myChar = 'a';
    /*[NetValue(4)] */public decimal myDecimal;
    /*[NetValue(5)] */public double myDouble = 1;
    /*[NetValue(6)] */public short myShort = 1;
    /*[NetValue(7)] */public ushort myUShort = 1;
    /*[NetValue(8)] */public int myInt = 1;
    /*[NetValue(9)] */public uint myUInt = 1;
    /*[NetValue(10)]*/ public long myLong = 1;
    /*[NetValue(11)]*/ public ulong myULong = 1;
    /*[NetValue(12)]*/ public byte myByte = 1;
    /*[NetValue(13)]*/ public sbyte mySByte = 1;
    /*[NetValue(14)]*/ public List<float> testList = new List<float>();
    /*[NetValue(15)]*/ public Vector3Pro position = new Vector3Pro(0f, 0f, 0f);



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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        position = new Vector3Pro(0f, 0f, 0f);

        testList.Add(1);
        testList.Add(2);

        testValue.Add(new solci() { gacha = true});
        testValue.Add(new solci() { gacha = true});
    }

    private void Update()
    {

        if (ClientManager.Instance.idClient == getOwner())
        {
            float movimientoHorizontal = Input.GetAxis("Horizontal");
            float movimientoVertical = Input.GetAxis("Vertical");

            rb.AddForce(movimientoHorizontal * 10, 0, movimientoVertical * 10);

            position.X = transform.position.x;
            position.Y = transform.position.y;
            position.Z = transform.position.z;
        }
        else
        {
            transform.position.Set(position.X, position.Y, position.Z);
        }


        Debug.Log($"Solci es : {testValue == null}");

        Debug.Log($"Decimal es : {myDecimal}");
    }

    [ContextMenu("Matar lista")]
    private void NullList()
    {
        testList = null;
    }

    [ContextMenu("Matar Sol")]
    private void NullSol()
    {
        testValue = null;
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

[Serializable]
public class solci
{
    [NetValue(0)] public bool gacha = true;
}

[Serializable]
public class Vector3Pro
{
    [NetValue(0)] public float X;
    [NetValue(1)] public float Y;
    [NetValue(2)] public float Z;

    public Vector3Pro(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
