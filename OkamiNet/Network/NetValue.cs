using System;

namespace OkamiNet.Network
{
    public class NetValue : Attribute
    {
        public int id;
        public NetValue(int id)
        {
            this.id = id;
        }
    }
}