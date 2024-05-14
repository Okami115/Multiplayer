using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;


// Ping : Cuando el Cliente recibe el S2C manda el primer ping
// El servidor devuelve un "pong"
public enum MessageType
{
    Console = 0,
    Position,
    Disconect,
    C2S,
    S2C,
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
            temp.name = Encoding.UTF8.GetString(message, offset + 16, stringlength);
            aux.Add(temp);
            offset += 8 + stringlength;
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

public abstract class BaseOrdenableMenssage<PayloadType> : BaseMenssaje<PayloadType>
{
    protected BaseOrdenableMenssage(byte[] msg)
    {
        MsgID = BitConverter.ToUInt64(msg, 4);
    }
    
    protected static ulong lastMsgID = 0;

    protected ulong MsgID = 0;
    protected static Dictionary<MessageType, ulong> lastExecutedMsgID = new Dictionary<MessageType, ulong>();
}

public class NetVector3 : BaseOrdenableMenssage<UnityEngine.Vector3>
{
    public NetVector3(byte[] msg) : base(msg)
    {

    }

    public override Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 12);
        outData.y = BitConverter.ToSingle(message, 16);
        outData.z = BitConverter.ToSingle(message, 20);

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
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        return outData.ToArray();
    }

    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }
}