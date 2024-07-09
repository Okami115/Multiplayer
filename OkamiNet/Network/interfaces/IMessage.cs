using OkamiNet.Network;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace OkamiNet.Menssage
{
    [NetValueTypeMessage(NetMenssage.Float, typeof(float))]
    public class NetFloat : BaseNetMenssaje<float>
    {
        public override float GetData()
        {
            return data;
        }

        public override void SetData(float data)
        {
            this.data = data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Float;
        }

        public MenssageFlags GetFlags()
        {
            return flags;
        }

        public override byte[] SerializeWithValueID(List<ParentsTree> parentsTree, int NetObjId, MenssageFlags menssageFlags)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data));
            outData.AddRange(BitConverter.GetBytes(NetObjId));
            outData.AddRange(BitConverter.GetBytes(messageID++));
            outData.AddRange(BitConverter.GetBytes(parentsTree.Count));

            foreach (ParentsTree tree in parentsTree)
            {
                outData.AddRange(BitConverter.GetBytes(tree.ID));
                outData.AddRange(BitConverter.GetBytes(tree.collectionPos));
                outData.AddRange(BitConverter.GetBytes(tree.collectionSize));
            }

            outData.AddRange(BitConverter.GetBytes((int)menssageFlags));

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

        public override float DeserializeWithNetValueId(byte[] message, out List<ParentsTree> parentsTree, out int objID)
        {
            float outData;
            outData = BitConverter.ToSingle(message, 4);
            objID = BitConverter.ToInt32(message, 8);
            int countList = BitConverter.ToInt32(message, 16);

            parentsTree = new List<ParentsTree>();
            int offset = 0;

            for (int i = 0; i < countList; i++)
            {
                int id = BitConverter.ToInt32(message, 20 + offset);
                int colPos = (BitConverter.ToInt32(message, 24 + offset));
                int colSize = (BitConverter.ToInt32(message, 28 + offset));

                offset += 12;
                parentsTree.Add(new ParentsTree(id, colPos, colSize));
            }

            return outData;
        }

        public override uint GetMessageID()
        {
            return messageID;
        }

        public override void SetMessageID(uint id)
        {
            messageID = id;
        }
    }

    [NetValueTypeMessage(NetMenssage.Bool, typeof(bool))]
    public class NetBool : BaseNetMenssaje<bool>
    {
        public override bool DeserializeWithNetValueId(byte[] message, out List<ParentsTree> parentsTree, out int objID)
        {
            bool outData;
            outData = BitConverter.ToBoolean(message, 4);//2
            UtilsTools.LOG(outData.ToString());
            objID = BitConverter.ToInt32(message, 5);//3
            UtilsTools.LOG(objID.ToString());
            int countList = BitConverter.ToInt32(message, 13);//5
            UtilsTools.LOG(countList.ToString());

            parentsTree = new List<ParentsTree>();
            int offset = 0;

            for (int i = 0; i < countList; i++)
            {
                int id = BitConverter.ToInt32(message, 17 + offset);
                int colPos = (BitConverter.ToInt32(message, 21 + offset));
                int colSize = (BitConverter.ToInt32(message, 25 + offset));

                offset += 12;
                parentsTree.Add(new ParentsTree(id, colPos, colSize));
            }

            return outData;
        }

        public override bool GetData()
        {
            return data;
        }

        public override uint GetMessageID()
        {
            return messageID;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.Bool;
        }

        public override byte[] SerializeWithValueID(List<ParentsTree> parentsTree, int NetObjId, MenssageFlags menssageFlags)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data));//2
            outData.AddRange(BitConverter.GetBytes(NetObjId));//3
            outData.AddRange(BitConverter.GetBytes(messageID++));//4
            outData.AddRange(BitConverter.GetBytes(parentsTree.Count));

            foreach (ParentsTree tree in parentsTree)
            {
                outData.AddRange(BitConverter.GetBytes(tree.ID));
                outData.AddRange(BitConverter.GetBytes(tree.collectionPos));
                outData.AddRange(BitConverter.GetBytes(tree.collectionSize));
            }

            outData.AddRange(BitConverter.GetBytes((int)menssageFlags));

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

        public override void SetData(bool data)
        {
            this.data = data;
        }

        public override void SetMessageID(uint id)
        {
            messageID = id;
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

        public override byte[] Serialize()
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

        public override byte[] Serialize()
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

        public override byte[] Serialize()
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data.Length));
            outData.AddRange(Encoding.UTF8.GetBytes(data));
            outData.AddRange(BitConverter.GetBytes((int)MenssageFlags.None));

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

    public class S2CHandShake : BaseMenssaje<int>
    {
        public S2CHandShake(int data)
        {
            this.data = data;
        }

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
            return NetMenssage.TuVieja;
        }

        public override byte[] Serialize()
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data));
            outData.AddRange(BitConverter.GetBytes((int)MenssageFlags.None));

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

        public override byte[] Serialize()
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data));
            outData.AddRange(BitConverter.GetBytes((int)MenssageFlags.None));

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

    public class NetFactoryDataSpawn : BaseMenssaje<List<FactoryData>>
    {
        public NetFactoryDataSpawn(List<FactoryData> data)
        {
            this.data = data;
        }

        public override List<FactoryData> Deserialize(byte[] message)
        {
            List<FactoryData> aux = new List<FactoryData>();

            int playersAmmount = BitConverter.ToInt32(message, 4);
            int offset = 0;
            for (int i = 0; i < playersAmmount; i++)
            {
                FactoryData temp = new FactoryData();
                temp.netObj = new NetObj();

                temp.netObj.id = BitConverter.ToInt32(message, offset + 8);
                temp.netObj.owner = BitConverter.ToInt32(message, offset + 12);
                temp.pos.X = BitConverter.ToSingle(message, offset + 16);
                temp.pos.Y = BitConverter.ToSingle(message, offset + 20);
                temp.pos.Z = BitConverter.ToSingle(message, offset + 24);
                temp.rot.X = BitConverter.ToSingle(message, offset + 28);
                temp.rot.Y = BitConverter.ToSingle(message, offset + 32);
                temp.rot.Z = BitConverter.ToSingle(message, offset + 36);
                temp.rot.W = BitConverter.ToSingle(message, offset + 40);
                temp.scale.X = BitConverter.ToSingle(message, offset + 44);
                temp.scale.Y = BitConverter.ToSingle(message, offset + 48);
                temp.scale.Z = BitConverter.ToSingle(message, offset + 52);
                temp.parentId = BitConverter.ToInt32(message, offset + 56);
                temp.prefabId = BitConverter.ToInt32(message, offset + 60);

                aux.Add(temp);
                offset += 64;
            }

            return aux;
        }

        public override List<FactoryData> GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.FactoryDataSpawn;
        }

        public override byte[] Serialize()
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data.Count));

            foreach (var data in data)
            {
                outData.AddRange(BitConverter.GetBytes(data.netObj.id));
                outData.AddRange(BitConverter.GetBytes(data.netObj.owner));
                outData.AddRange(BitConverter.GetBytes(data.pos.X));
                outData.AddRange(BitConverter.GetBytes(data.pos.Y));
                outData.AddRange(BitConverter.GetBytes(data.pos.Z));
                outData.AddRange(BitConverter.GetBytes(data.rot.X));
                outData.AddRange(BitConverter.GetBytes(data.rot.Y));
                outData.AddRange(BitConverter.GetBytes(data.rot.Z));
                outData.AddRange(BitConverter.GetBytes(data.rot.W));
                outData.AddRange(BitConverter.GetBytes(data.scale.X));
                outData.AddRange(BitConverter.GetBytes(data.scale.Y));
                outData.AddRange(BitConverter.GetBytes(data.scale.Z));
                outData.AddRange(BitConverter.GetBytes(data.parentId));
                outData.AddRange(BitConverter.GetBytes(data.prefabId));

            }
            outData.AddRange(BitConverter.GetBytes((int)MenssageFlags.None));

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

        public override byte[] Serialize()
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data.Length));
            outData.AddRange(Encoding.UTF8.GetBytes(data));
            outData.AddRange(BitConverter.GetBytes((int)MenssageFlags.None));

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

    public class FactoryRequest : BaseMenssaje<FactoryData>
    {
        public override FactoryData Deserialize(byte[] message)
        {
            FactoryData data;
            data.netObj = new NetObj();

            data.netObj.id = BitConverter.ToInt32(message, 4);
            data.netObj.owner = BitConverter.ToInt32(message, 8);
            data.pos.X = BitConverter.ToSingle(message, 12);
            data.pos.Y = BitConverter.ToSingle(message, 16);
            data.pos.Z = BitConverter.ToSingle(message, 20);
            data.rot.X = BitConverter.ToSingle(message, 24);
            data.rot.Y = BitConverter.ToSingle(message, 28);
            data.rot.Z = BitConverter.ToSingle(message, 32);
            data.rot.W = BitConverter.ToSingle(message, 36);
            data.scale.X = BitConverter.ToSingle(message, 40);
            data.scale.Y = BitConverter.ToSingle(message, 44);
            data.scale.Z = BitConverter.ToSingle(message, 48);
            data.parentId = BitConverter.ToInt32(message, 52);
            data.prefabId = BitConverter.ToInt32(message, 56);

            return data;
        }

        public override FactoryData GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.FactoryRequest;
        }

        public override byte[] Serialize()
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            //outData.AddRange(BitConverter.GetBytes(lastMsgID++));
            outData.AddRange(BitConverter.GetBytes(data.netObj.id));
            outData.AddRange(BitConverter.GetBytes(data.netObj.owner));
            outData.AddRange(BitConverter.GetBytes(data.pos.X));
            outData.AddRange(BitConverter.GetBytes(data.pos.Y));
            outData.AddRange(BitConverter.GetBytes(data.pos.Z));
            outData.AddRange(BitConverter.GetBytes(data.rot.X));
            outData.AddRange(BitConverter.GetBytes(data.rot.Y));
            outData.AddRange(BitConverter.GetBytes(data.rot.Z));
            outData.AddRange(BitConverter.GetBytes(data.rot.W));
            outData.AddRange(BitConverter.GetBytes(data.scale.X));
            outData.AddRange(BitConverter.GetBytes(data.scale.Y));
            outData.AddRange(BitConverter.GetBytes(data.scale.Z));
            outData.AddRange(BitConverter.GetBytes(data.parentId));
            outData.AddRange(BitConverter.GetBytes(data.prefabId));
            outData.AddRange(BitConverter.GetBytes((int)MenssageFlags.None));

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

    public class FactoryMenssage : BaseMenssaje<FactoryData>
    {
        public override FactoryData Deserialize(byte[] message)
        {
            FactoryData data;
            data.netObj = new NetObj();

            data.netObj.id = BitConverter.ToInt32(message, 4);
            data.netObj.owner = BitConverter.ToInt32(message, 8);
            data.pos.X = BitConverter.ToSingle(message, 12);
            data.pos.Y = BitConverter.ToSingle(message, 16);
            data.pos.Z = BitConverter.ToSingle(message, 20);
            data.rot.X = BitConverter.ToSingle(message, 24);
            data.rot.Y = BitConverter.ToSingle(message, 28);
            data.rot.Z = BitConverter.ToSingle(message, 32);
            data.rot.W = BitConverter.ToSingle(message, 36);
            data.scale.X = BitConverter.ToSingle(message, 40);
            data.scale.Y = BitConverter.ToSingle(message, 44);
            data.scale.Z = BitConverter.ToSingle(message, 48);
            data.parentId = BitConverter.ToInt32(message, 52);
            data.prefabId = BitConverter.ToInt32(message, 56);

            return data;
        }

        public override FactoryData GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.FactoryMessage;
        }

        public override byte[] Serialize()
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            //outData.AddRange(BitConverter.GetBytes(lastMsgID++));
            outData.AddRange(BitConverter.GetBytes(data.netObj.id));
            outData.AddRange(BitConverter.GetBytes(data.netObj.owner));
            outData.AddRange(BitConverter.GetBytes(data.pos.X));
            outData.AddRange(BitConverter.GetBytes(data.pos.Y));
            outData.AddRange(BitConverter.GetBytes(data.pos.Z));
            outData.AddRange(BitConverter.GetBytes(data.rot.X));
            outData.AddRange(BitConverter.GetBytes(data.rot.Y));
            outData.AddRange(BitConverter.GetBytes(data.rot.Z));
            outData.AddRange(BitConverter.GetBytes(data.rot.W));
            outData.AddRange(BitConverter.GetBytes(data.scale.X));
            outData.AddRange(BitConverter.GetBytes(data.scale.Y));
            outData.AddRange(BitConverter.GetBytes(data.scale.Z));
            outData.AddRange(BitConverter.GetBytes(data.parentId));
            outData.AddRange(BitConverter.GetBytes(data.prefabId));
            outData.AddRange(BitConverter.GetBytes((int)MenssageFlags.None));

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

    public class CheckMessage : BaseNetMenssaje<(NetMenssage, uint)>
    {
        public override (NetMenssage, uint) DeserializeWithNetValueId(byte[] message, out List<ParentsTree> parentsTree, out int objID)
        {
            (NetMenssage, uint) data;

            parentsTree = null;
            objID = 0;

            data.Item1 = (NetMenssage)BitConverter.ToInt32(message, 4);
            data.Item2 = BitConverter.ToUInt32(message, 8);

            return data;
        }

        public override (NetMenssage, uint) GetData()
        {
            return data;
        }

        public override uint GetMessageID()
        {
            return messageID;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.CheckMessage;
        }

        public override byte[] SerializeWithValueID(List<ParentsTree> parentsTree, int NetObjId, MenssageFlags menssageFlags)
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes((int)data.Item1));
            outData.AddRange(BitConverter.GetBytes(data.Item2));
            outData.AddRange(BitConverter.GetBytes(messageID++));

            outData.AddRange(BitConverter.GetBytes((int)menssageFlags));

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

        public override void SetData((NetMenssage, uint) data)
        {
            this.data = data;
        }

        public override void SetMessageID(uint id)
        {
            messageID = id;
        }
    }

    public class ChangePort : BaseMenssaje<(int, string)>
    {
        public override (int, string) Deserialize(byte[] message)
        {
            (int, string) data;

            data.Item1 = BitConverter.ToInt32(message, 4);
            int count = BitConverter.ToInt32(message, 8);
            data.Item2 = Encoding.UTF8.GetString(message, 12, count);


            return data;
        }

        public override (int, string) GetData()
        {
            return data;
        }

        public override NetMenssage GetMessageType()
        {
            return NetMenssage.ChangePort;
        }

        public override byte[] Serialize()
        {
            List<byte> outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(data.Item1));
            outData.AddRange(BitConverter.GetBytes(data.Item2.ToString().Length));
            outData.AddRange(Encoding.UTF8.GetBytes(data.Item2.ToString()));
            outData.AddRange(BitConverter.GetBytes((int)MenssageFlags.None));

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

    public static class Flags
    {
        public static MenssageFlags FlagsCheck(byte[] data)
        {
            return (MenssageFlags)BitConverter.ToInt32(data, data.Length - 12);
        }
    }

}