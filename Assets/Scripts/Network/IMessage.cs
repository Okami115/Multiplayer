using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.UI;


public enum MessageType
{
    Console = 0,
    Position,
    Rotation,
    Shoot,
    Disconect,
    AddPlayer,
    C2S,
    S2C,
    PlayerList,
    Ping,
    Denied,
    Timer,
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

public class NetDisconect : BaseMenssaje<int>
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
        return MessageType.Disconect;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }
}

public class AddPlayer : BaseMenssaje<Player>
{
    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override Player Deserialize(byte[] message)
    {
        Player temp = new Player();
        temp.id = BitConverter.ToInt32(message, 4);
        int stringlength = BitConverter.ToInt32(message, 8);
        temp.HP = BitConverter.ToInt32(message, 12);
        temp.pos.x = BitConverter.ToSingle(message, 16);
        temp.pos.y = BitConverter.ToSingle(message, 20);
        temp.pos.z = BitConverter.ToSingle(message, 24);
        temp.rotation.x = BitConverter.ToSingle(message, 28);
        temp.rotation.y = BitConverter.ToSingle(message, 32);
        temp.name = Encoding.UTF8.GetString(message, 36, stringlength);

        return temp;
    }

    public override Player GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.AddPlayer;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.id));
        outData.AddRange(BitConverter.GetBytes(data.name.Length));
        outData.AddRange(BitConverter.GetBytes(data.HP));
        outData.AddRange(BitConverter.GetBytes(data.pos.x));
        outData.AddRange(BitConverter.GetBytes(data.pos.y));
        outData.AddRange(BitConverter.GetBytes(data.pos.z));
        outData.AddRange(BitConverter.GetBytes(data.rotation.x));
        outData.AddRange(BitConverter.GetBytes(data.rotation.y));
        outData.AddRange(Encoding.UTF8.GetBytes(data.name));

        return outData.ToArray();
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
            temp.HP = BitConverter.ToInt32(message, offset + 16);
            temp.pos.x = BitConverter.ToSingle(message, offset + 20);
            temp.pos.y = BitConverter.ToSingle(message, offset + 24);
            temp.pos.z = BitConverter.ToSingle(message, offset + 28);
            temp.rotation.x = BitConverter.ToSingle(message, offset + 32);
            temp.rotation.y = BitConverter.ToSingle(message, offset + 36);
            temp.name = Encoding.UTF8.GetString(message, offset + 40, stringlength);
            aux.Add(temp);
            offset += 32 + stringlength;
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
            outData.AddRange(BitConverter.GetBytes(player.HP));
            outData.AddRange(BitConverter.GetBytes(player.pos.x));
            outData.AddRange(BitConverter.GetBytes(player.pos.y));
            outData.AddRange(BitConverter.GetBytes(player.pos.z));
            outData.AddRange(BitConverter.GetBytes(player.rotation.x));
            outData.AddRange(BitConverter.GetBytes(player.rotation.y));
            outData.AddRange(Encoding.UTF8.GetBytes(player.name));
        }

        return outData.ToArray();
    }
}

public class NetShoot : BaseMenssaje<int>
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
        return MessageType.Shoot;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data));

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
        outData.x = BitConverter.ToSingle(message, 4);
        outData.y = BitConverter.ToSingle(message, 8);
        outData.z = BitConverter.ToSingle(message, 12);

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
        //outData.AddRange(BitConverter.GetBytes(lastMsgID++));
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

public class NetRotation : BaseMenssaje<Vector2>
{
    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override Vector2 Deserialize(byte[] message)
    {
        Vector2 outData;
        outData.x = BitConverter.ToSingle(message, 4);
        outData.y = BitConverter.ToSingle(message, 8);

        return outData;
    }

    public override Vector2 GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Rotation;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        //outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));

        return outData.ToArray();
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
            temp.HP = BitConverter.ToInt32(message, offset + 16);
            temp.pos.x = BitConverter.ToSingle(message, offset + 20);
            temp.pos.y = BitConverter.ToSingle(message, offset + 24);
            temp.pos.z = BitConverter.ToSingle(message, offset + 28);
            temp.rotation.x = BitConverter.ToSingle(message, offset + 32);
            temp.rotation.y = BitConverter.ToSingle(message, offset + 36);
            temp.name = Encoding.UTF8.GetString(message, offset + 40, stringlength);
            aux.Add(temp);
            offset += 32 + stringlength;
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
            outData.AddRange(BitConverter.GetBytes(player.HP));
            outData.AddRange(BitConverter.GetBytes(player.pos.x));
            outData.AddRange(BitConverter.GetBytes(player.pos.y));
            outData.AddRange(BitConverter.GetBytes(player.pos.z));
            outData.AddRange(BitConverter.GetBytes(player.rotation.x));
            outData.AddRange(BitConverter.GetBytes(player.rotation.y));
            outData.AddRange(Encoding.UTF8.GetBytes(player.name));
        }
        return outData.ToArray();
    }
}

public class DeniedNet : BaseMenssaje<string>
{
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
        return MessageType.Denied;
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

public class NetTimer : BaseMenssaje<float>
{
    public override bool Checksum(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override float Deserialize(byte[] message)
    {
        float outData;
        outData = BitConverter.ToSingle(message, 4);

        return outData;
    }

    public override float GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Timer;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }
}