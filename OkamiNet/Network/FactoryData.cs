using System.Numerics;

namespace OkamiNet.Network
{
    public struct FactoryData : INetObj
    {
        public NetObj netObj;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
        public int parentId;
        public int prefabId;

        public FactoryData(NetObj netObj, Vector3 pos, Quaternion rot, Vector3 scale, int parentId, int prefabId)
        {
            this.netObj = netObj;
            this.pos = pos;
            this.rot = rot; 
            this.scale = scale;
            this.parentId = parentId;
            this.prefabId = prefabId;
        }

        public int getID()
        {
            return netObj.id;
        }

        public NetObj getNetObj()
        {
            return netObj;
        }

        public int getOwner()
        {
            return netObj.owner;
        }

        public void SetID(int id)
        {
            netObj.id = id;
        }

        public void SetOwner(int owner)
        {
            netObj.owner = owner;
        }
    }
}