using OkamiNet.Network;
using System;
using System.Collections.Generic;

namespace OkamiNet.Menssage
{
    public enum NetMenssage
    {
        String = 0,
        Float,
        Bool,
        Int,
        Disconect,
        AddPlayer,
        C2S,
        S2C,
        FactoryDataSpawn,
        Ping,
        Denied,
        FactoryRequest,
        FactoryMessage,
        CheckMessage,
        ChangePort,
        Byte,
        SByte,
        Short,
        UShort,
        UInt,
        Long,
        ULong,
        Decimal,
        Double,
        Char
    }

    [Flags]
    public enum MenssageFlags
    {
        None = 0,
        Ordenable,
        Importants,
    }
    public abstract class BaseMenssaje<PayLoadType>
    {
        public PayLoadType data;
        public abstract NetMenssage GetMessageType();

        public static Action<PayLoadType> OnDispatch;
        public abstract byte[] Serialize();

        public abstract PayLoadType Deserialize(byte[] message);

        public abstract PayLoadType GetData();
    }

    public abstract class BaseNetMenssaje<PayLoadType>
    {
        public PayLoadType data;

        public static uint messageID = 0;

        public MenssageFlags flags;
        public abstract NetMenssage GetMessageType();

        public static Action<PayLoadType> OnDispatch;
        public abstract byte[] SerializeWithValueID(List<ParentsTree> parentsTree, int NetObjId, MenssageFlags menssageFlags);

        public abstract PayLoadType DeserializeWithNetValueId(byte[] message, out List<ParentsTree> parentsTree, out int objID);

        public abstract PayLoadType GetData();

        public abstract uint GetMessageID();

        public abstract void SetMessageID(uint id);

        public abstract void SetData(PayLoadType data);
    }

}
