using System;

namespace OkamiNet.Network
{
    public class NetValue : Attribute
    {
        int id;
        public NetValue(int id)
        {
            this.id = id;
        }
    }
}