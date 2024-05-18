using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.UI;


public enum MessageType
{
    Console = 0,
    Position,
    Disconect,
    C2S,
    S2C,
    PlayerList,
    Ping,
}

public abstract class BaseMenssaje<PayLoadType>
{
    public PayLoadType data;
    public abstract MessageType GetMessageType();

    public static Action<PayLoadType> OnDispatch;
    public abstract byte[] Serialize();

    public abstract bool Checksum(byte[] data);

    public abstract PayLoadType Deserialize(byte[] message);

    public abstract PayLoadType GetData();
}

public class NetConsole : BaseMenssaje<string>
{
    public NetConsole(string data) 
    { 
        this.data = data;
    }

    public override bool Checksum(byte[] data)
    {
        int checkSum1 = BitConverter.ToInt32(data, data.Length - 8);
        int checkSum2 = BitConverter.ToInt32(data, data.Length - 4);

        int result1 = 0;
        int result2 = 0;

        for (int i = 0; i < data.Length - 8; i++)
        {
            result1 += data[i];
            result2 -= data[i];
        }

        bool aux = checkSum1 == result1 && checkSum2 == result2;
        Debug.Log("Check1 : " + checkSum1);
        Debug.Log("Check2 : " + checkSum2);
        Debug.Log("res1 : " + result1);
        Debug.Log("res2 : " + result2);
        Debug.Log("Checksum? : " + aux);
        return checkSum1 == result1 && checkSum2 == result2;
    }

    public override string Deserialize(byte[] message)
    {
        int stringlength = BitConverter.ToInt32(message, 4);
        
        return Encoding.UTF8.GetString(message, 8, stringlength);
    }

    public override string GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.Length));
        outData.AddRange(Encoding.UTF8.GetBytes(data));

        int result1 = 0;
        int result2 = 0;

        for (int i = 0; i < outData.Count; i++)
        {
            result1 += outData[i];
            result2 -= outData[i];
        }

        Debug.Log("Check1 : " + result1);
        Debug.Log("Check2 : " + result2);

        outData.AddRange(BitConverter.GetBytes(result1));
        outData.AddRange(BitConverter.GetBytes(result2));

        return outData.ToArray();
    }
}

public class NetDisconect : BaseMenssaje<bool>
{
    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override bool Deserialize(byte[] message)
    {
        throw new NotImplementedException();
    }

    public override bool GetData()
    {
        throw new NotImplementedException();
    }

    public override MessageType GetMessageType()
    {
        throw new NotImplementedException();
    }

    public override byte[] Serialize()
    {
        throw new NotImplementedException();
    }
}

public class C2SHandShake : BaseMenssaje<string>
{

    public C2SHandShake(string data)
    {
        this.data = data;
    }

    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override string Deserialize(byte[] message)
    {
        int stringlength = BitConverter.ToInt32(message, 4);

        return Encoding.UTF8.GetString(message, 8, stringlength);
    }

    public override string GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.C2S;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.Length));
        outData.AddRange(Encoding.UTF8.GetBytes(data));

        return outData.ToArray();
    }
}

public class S2CHandShake : BaseMenssaje<List<Player>>
{

    public S2CHandShake(List<Player> data)
    {
        this.data = data;
    }

    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override List<Player> Deserialize(byte[] message)
    {
        List<Player> aux = new List<Player>();

        int playersAmmount = BitConverter.ToInt32(message, 4);
        int offset = 0;
        for (int i = 0; i < playersAmmount; i++)
        {
            Player temp = new Player();
            temp.id = BitConverter.ToInt32(message, offset + 8);
            int stringlength = BitConverter.ToInt32(message, offset + 12);
            temp.pos.x = BitConverter.ToInt32(message, offset + 16);
            temp.pos.y = BitConverter.ToInt32(message, offset + 20);
            temp.pos.z = BitConverter.ToInt32(message, offset + 24);
            temp.name = Encoding.UTF8.GetString(message, offset + 28, stringlength);
            aux.Add(temp);
            offset += 20 + stringlength;
        }

        return aux;
    }

    public override List<Player> GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.S2C;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.Count));

        foreach (var player in data)
        {
            outData.AddRange(BitConverter.GetBytes(player.id));
            outData.AddRange(BitConverter.GetBytes(player.name.Length));
            outData.AddRange(BitConverter.GetBytes(player.pos.x));
            outData.AddRange(BitConverter.GetBytes(player.pos.y));
            outData.AddRange(BitConverter.GetBytes(player.pos.z));
            outData.AddRange(Encoding.UTF8.GetBytes(player.name));
        }

        return outData.ToArray();
    }
}

public class NetPing : BaseMenssaje<int>
{
    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override int Deserialize(byte[] message)
    {
       return BitConverter.ToInt32(message, 4);
    }

    public override int GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Ping;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }
}

public class NetVector3 : BaseMenssaje<UnityEngine.Vector3>
{
    public override Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;
        outData.x = BitConverter.ToSingle(message, 12);
        outData.y = BitConverter.ToSingle(message, 16);
        outData.z = BitConverter.ToSingle(message, 20);

        Debug.LogWarning("recive : " + outData);
        return outData;
    }

    public override Vector3 GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Position;
    }

    public int GetMessageNumber(byte[] data)
    {
        return BitConverter.ToInt32(data, 8);
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        //outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        Debug.LogWarning("Send : " + data);

        return outData.ToArray();
    }

    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }
}

public class NetPlayerListUpdate : BaseMenssaje<List<Player>>
{
    public NetPlayerListUpdate(List<Player> data)
    {
        this.data = data;
    }
    public override bool Checksum(byte[] data)
    {
        return true;
    }

    public override List<Player> Deserialize(byte[] message)
    {
        List<Player> aux = new List<Player>();

        int playersAmmount = BitConverter.ToInt32(message, 4);
        int offset = 0;
        for (int i = 0; i < playersAmmount; i++)
        {
            Player temp = new Player();
            temp.id = BitConverter.ToInt32(message, offset + 8);
            int stringlength = BitConverter.ToInt32(message, offset + 12);
            temp.pos.x = BitConverter.ToInt32(message, offset + 16);
            temp.pos.y = BitConverter.ToInt32(message, offset + 20);
            temp.pos.z = BitConverter.ToInt32(message, offset + 24);
            temp.name = Encoding.UTF8.GetString(message, offset + 28, stringlength);
            Debug.LogWarning("recive : " + temp.name + " : " + temp.pos);
            aux.Add(temp);
            offset += 20 + stringlength;
        }

        return aux;
    }

    public override List<Player> GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.PlayerList;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.Count));

        foreach (var player in data)
        {
            outData.AddRange(BitConverter.GetBytes(player.id));
            outData.AddRange(BitConverter.GetBytes(player.name.Length));
            outData.AddRange(BitConverter.GetBytes(player.pos.x));
            outData.AddRange(BitConverter.GetBytes(player.pos.y));
            outData.AddRange(BitConverter.GetBytes(player.pos.z));
            outData.AddRange(Encoding.UTF8.GetBytes(player.name));
            Debug.LogWarning("Send : " + player.name + " : " + player.pos);
        }
        return outData.ToArray();
    }
}