// Sintetizar

using System;

namespace OkamiNet.Menssage
{
    public enum NetMenssage
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
        public abstract NetMenssage GetMessageType();

        public static Action<PayLoadType> OnDispatch;
        public abstract byte[] Serialize();

        public abstract PayLoadType Deserialize(byte[] message);

        public abstract PayLoadType GetData();
    }

}
