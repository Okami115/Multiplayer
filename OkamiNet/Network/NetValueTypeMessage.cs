using OkamiNet.Menssage;
using System;

namespace OkamiNet.Network
{
    public class NetValueTypeMessage : Attribute
    {
        public NetMenssage netMenssage;
        public Type type;  
        public NetValueTypeMessage(NetMenssage netmessage, Type type)
        {
            this.netMenssage = netmessage;
            this.type = type;
        }
    }
}