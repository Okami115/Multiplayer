using OkamiNet.Network;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OkamiNet.Menssage
{
    public class NetString : BaseMenssaje<string>
    {
        public NetString(string data) 
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

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.String;
        }

        public override byte[] Serialize(int Owner)
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

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    [NetValueTypeMessage(NetMenssage.Float, typeof(float))]
    public class NetFloat : BaseMenssaje<float>
    {
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

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Float;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data));

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    public class NetInt : BaseMenssaje<int>
    {
        public override int Deserialize(byte[] message)
        {
            return BitConverter.ToInt32(message, 4);
        }

        public override int GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Shoot;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data));

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    public class NetVector3 : BaseMenssaje<Vector3>
    {
        public override Vector3 Deserialize(byte[] message)
        {
            Vector3 outData;
            outData.X = BitConverter.ToSingle(message, 4);
            outData.Y = BitConverter.ToSingle(message, 8);
            outData.Z = BitConverter.ToSingle(message, 12);

            return outData;
        }

        public override Vector3 GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Vector3;
        }

        public int GetOwner(byte[] message)
        {
            int id = BitConverter.ToInt32(message, message.Length - 12);
            UtilsTools.LOG("Owner : " + id);
            return id;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            //outData.AddRange(BitConverter.GetBytes(lastMsgID++));
            outData.AddRange(BitConverter.GetBytes(data.X));
            outData.AddRange(BitConverter.GetBytes(data.Y));
            outData.AddRange(BitConverter.GetBytes(data.Z));

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }
            outData.AddRange(BitConverter.GetBytes(Owner));
            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    public class NetVector2 : BaseMenssaje<Vector2>
    {
        public override Vector2 Deserialize(byte[] message)
        {
            Vector2 outData;
            outData.X = BitConverter.ToSingle(message, 4);
            outData.Y = BitConverter.ToSingle(message, 8);

            return outData;
        }

        public override Vector2 GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Rotation;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            //outData.AddRange(BitConverter.GetBytes(lastMsgID++));
            outData.AddRange(BitConverter.GetBytes(data.X));
            outData.AddRange(BitConverter.GetBytes(data.Y));

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    public class NetDisconect : BaseMenssaje<int>
    {
        public override int Deserialize(byte[] message)
        {
            return BitConverter.ToInt32(message, 4);
        }

        public override int GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Disconect;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data));

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    public class AddPlayer : BaseMenssaje<Player>
    {
        public override Player Deserialize(byte[] message)
        {
            Player temp = new Player();
            temp.id = BitConverter.ToInt32(message, 4);
            int stringlength = BitConverter.ToInt32(message, 8);
            temp.HP = BitConverter.ToInt32(message, 12);
            temp.pos.X = BitConverter.ToSingle(message, 16);
            temp.pos.Y = BitConverter.ToSingle(message, 20);
            temp.pos.Z = BitConverter.ToSingle(message, 24);
            temp.rotation.X = BitConverter.ToSingle(message, 28);
            temp.rotation.Y = BitConverter.ToSingle(message, 32);
            temp.name = Encoding.UTF8.GetString(message, 36, stringlength);

            return temp;
        }

        public override Player GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.AddPlayer;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data.id));
            outData.AddRange(BitConverter.GetBytes(data.name.Length));
            outData.AddRange(BitConverter.GetBytes(data.HP));
            outData.AddRange(BitConverter.GetBytes(data.pos.X));
            outData.AddRange(BitConverter.GetBytes(data.pos.Y));
            outData.AddRange(BitConverter.GetBytes(data.pos.Z));
            outData.AddRange(BitConverter.GetBytes(data.rotation.X));
            outData.AddRange(BitConverter.GetBytes(data.rotation.Y));
            outData.AddRange(Encoding.UTF8.GetBytes(data.name));

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
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

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.C2S;
        }

        public override byte[] Serialize(int Owner)
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

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

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
                temp.HP = BitConverter.ToInt32(message, offset + 16);
                temp.pos.X = BitConverter.ToSingle(message, offset + 20);
                temp.pos.Y = BitConverter.ToSingle(message, offset + 24);
                temp.pos.Z = BitConverter.ToSingle(message, offset + 28);
                temp.rotation.X = BitConverter.ToSingle(message, offset + 32);
                temp.rotation.Y = BitConverter.ToSingle(message, offset + 36);
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

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.S2C;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data.Count));

            foreach (var player in data)
            {
                outData.AddRange(BitConverter.GetBytes(player.id));
                outData.AddRange(BitConverter.GetBytes(player.name.Length));
                outData.AddRange(BitConverter.GetBytes(player.HP));
                outData.AddRange(BitConverter.GetBytes(player.pos.X));
                outData.AddRange(BitConverter.GetBytes(player.pos.Y));
                outData.AddRange(BitConverter.GetBytes(player.pos.Z));
                outData.AddRange(BitConverter.GetBytes(player.rotation.X));
                outData.AddRange(BitConverter.GetBytes(player.rotation.Y));
                outData.AddRange(Encoding.UTF8.GetBytes(player.name));
            }

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    public class NetPing : BaseMenssaje<int>
    {
        public override int Deserialize(byte[] message)
        {
           return BitConverter.ToInt32(message, 4);
        }

        public override int GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Ping;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data));

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    public class NetPlayerListUpdate : BaseMenssaje<List<Player>>
    {
        public NetPlayerListUpdate(List<Player> data)
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
                temp.HP = BitConverter.ToInt32(message, offset + 16);
                temp.pos.X = BitConverter.ToSingle(message, offset + 20);
                temp.pos.Y = BitConverter.ToSingle(message, offset + 24);
                temp.pos.Z = BitConverter.ToSingle(message, offset + 28);
                temp.rotation.X = BitConverter.ToSingle(message, offset + 32);
                temp.rotation.Y = BitConverter.ToSingle(message, offset + 36);
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

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.PlayerList;
        }

        public override byte[] Serialize(int Owner)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data.Count));

            foreach (var player in data)
            {
                outData.AddRange(BitConverter.GetBytes(player.id));
                outData.AddRange(BitConverter.GetBytes(player.name.Length));
                outData.AddRange(BitConverter.GetBytes(player.HP));
                outData.AddRange(BitConverter.GetBytes(player.pos.X));
                outData.AddRange(BitConverter.GetBytes(player.pos.Y));
                outData.AddRange(BitConverter.GetBytes(player.pos.Z));
                outData.AddRange(BitConverter.GetBytes(player.rotation.X));
                outData.AddRange(BitConverter.GetBytes(player.rotation.Y));
                outData.AddRange(Encoding.UTF8.GetBytes(player.name));
            }

            int result1 = 0;
            int result2 = 0;

            for (int i = 0; i < outData.Count; i++)
            {
                result1 += outData[i];
                result2 -= outData[i];
            }

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));
            return outData.ToArray();
        }
    }

    public class DeniedNet : BaseMenssaje<string>
    {
        public override string Deserialize(byte[] message)
        {
            int stringlength = BitConverter.ToInt32(message, 4);

            return Encoding.UTF8.GetString(message, 8, stringlength);
        }

        public override string GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Denied;
        }

        public override byte[] Serialize(int Owner)
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

            outData.AddRange(BitConverter.GetBytes(result1));
            outData.AddRange(BitConverter.GetBytes(result2));

            return outData.ToArray();
        }
    }

    public static class Checksum
{
    public static bool ChecksumConfirm(byte[] data)
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

        return checkSum1 == result1 && checkSum2 == result2;
    }
}

}