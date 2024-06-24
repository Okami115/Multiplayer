// Sintetizar

using System;

namespace OkamiNet.Menssage
{
    public enum NetMenssage
    {
        String = 0,
        Vector3,
        Rotation,
        Shoot,
        Disconect,
        AddPlayer,
        C2S,
        S2C,
        PlayerList,
        Ping,
        Denied,
        Float,
    }
    public abstract class BaseMenssaje<PayLoadType>
    {
        public PayLoadType data;
        public abstract NetMenssage GetMessageType();

        public static Action<PayLoadType> OnDispatch;
        public abstract byte[] Serialize(int Owner);

        public abstract PayLoadType Deserialize(byte[] message);

        public abstract PayLoadType GetData();
    }

}
