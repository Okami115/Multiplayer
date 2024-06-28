namespace OkamiNet.Network
{
    public class NetObj
    {
        public int id;
        public int owner;
    }

    public interface INetObj
    {
        int getID();
        int getOwner();
        NetObj getNetObj();
        void SetID(int id);
        void SetOwner(int owner);
    }

}

