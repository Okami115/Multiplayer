namespace OkamiNet.Network
{
    public class NetObj
    {
        public int id;
        public int owner;
    }

    public interface INetObj
    {
        void getID();
        void getOwner();
        void getNetObj();
        void SetID();
        void SetOwner();
    }

}

