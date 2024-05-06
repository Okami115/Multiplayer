using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


// Ping : Cuando el Cliente recibe el S2C manda el primer ping
// El servidor devuelve un "pong"
public enum MessageType
{
    Console = 0,
    Position = 1,
    Disconect = 2,
    C2S = 3,
    S2C = 4,
}

public abstract class BaseMenssaje<PayLoadType>
{
    public PayLoadType data;
    public abstract MessageType GetMessageType();

    public static Action<PayLoadType> OnDispatch;
    public abstract byte[] Serialize();

    public abstract PayLoadType Deserialize(byte[] message);

    public abstract PayLoadType GetData();
}

public class NetConsole : BaseMenssaje<string>
{
    public NetConsole(string data) 
    { 
        this.data = data;
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

        return outData.ToArray();
    }
}

public class NetDisconect : BaseMenssaje<bool>
{
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
}