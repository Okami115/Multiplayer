using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Linq;


// Ping : Cuando el Cliente recibe el S2C manda el primer ping
// El servidor devuelve un "pong"
public enum MessageType
{
    HandShake = -1,
    Console = 0,
    Position = 1
}

public interface IMessage<T>
{
    public MessageType GetMessageType();
    public byte[] Serialize();
    public T Deserialize(byte[] message);
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

public class NetDisconect : IMessage<string>
{
    public string Deserialize(byte[] message)
    {
        throw new NotImplementedException();
    }

    public MessageType GetMessageType()
    {
        throw new NotImplementedException();
    }

    public byte[] Serialize()
    {
        throw new NotImplementedException();
    }
}

public class NetC2SHandShake : IMessage<string>
{
    // el servidor deberia de enviar el ID del juegador que se va a conectar
    // Y el cliente deberia enviar su IP con su puerto

    string data;
    public string Deserialize(byte[] message)
    {
        string outData;

        outData = BitConverter.ToString(message, 4);

        return outData;
    }

    public MessageType GetMessageType()
    {
       return MessageType.HandShake;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(Encoding.UTF8.GetBytes(data));

        return outData.ToArray();
    }
}

public class NetS2CHandShake : IMessage<List<Player>>
{
    // el servidor deberia de enviar el ID del juegador que se va a conectar
    // Y el cliente deberia enviar su IP con su puerto

    List<Player> data;

    public List<Player> Deserialize(byte[] message)
    {
        List<Player> outData;

        outData = BitConverter.ToInt64(message, 4);

        return outData;
    }

    public MessageType GetMessageType()
    {
        return MessageType.HandShake;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(Encoding.UTF8.GetBytes(data));
        outData.AddRange(Utils.Serializer(data));


        return outData.ToArray();
    }
}

public class NetConsole : IMessage<string>
{
    string data;
    public string Deserialize(byte[] message)
    {
        string outData;

        outData = BitConverter.ToString(message, 4);

        return outData;
    }

    public MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.Length));

        //outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }
}

public class NetVector3 : IMessage<UnityEngine.Vector3>
{
    private static ulong lastMsgID = 0;
    private Vector3 data;

    public NetVector3(Vector3 data)
    {
        this.data = data;
    }

    public Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 12);
        outData.y = BitConverter.ToSingle(message, 16);
        outData.z = BitConverter.ToSingle(message, 20);

        return outData;
    }

    public MessageType GetMessageType()
    {
        return MessageType.Position;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        return outData.ToArray();
    }

    //Dictionary<Client,Dictionary<msgType,int>>
}