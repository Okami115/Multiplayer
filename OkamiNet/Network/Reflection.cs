using OkamiNet.Menssage;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OkamiNet.Network
{
    public static class Reflection
    {
        public static List<INetObj> netObjets = new List<INetObj>();

        private static Dictionary<NetMenssage, Type> messagesNetDictionary;
        private static Dictionary<Type, Type> messagesTypeDictionary;

        public static void Inspect(Type type, object obj)
        {
            if (obj != null)
            {
                UtilsTools.LOG("Exist");
                foreach (FieldInfo info in type.GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    UtilsTools.LOG("Can read");
                    ReadValue(info, obj);
                }

                if (type.BaseType != null)
                {
                    UtilsTools.LOG("not null");
                    Inspect(type.BaseType, obj);
                }
            }
        }

        public static void ReadValue(FieldInfo info, object obj)
        {
            if (info.FieldType.IsValueType || info.FieldType == typeof(string) || info.FieldType.IsEnum)
            {
                UtilsTools.LOG(info.Name + ": " + info.GetValue(obj) + "Funca");
            }
            else if (typeof(System.Collections.ICollection).IsAssignableFrom(info.FieldType))
            {
                UtilsTools.LOG("Insert Collection");
                foreach (object item in (info.GetValue(obj) as System.Collections.ICollection))
                {
                    Inspect(item.GetType(), item);
                }
            }
            else
            {
                UtilsTools.LOG("Insert");
                Inspect(info.FieldType, info.GetValue(obj));
            }
        }

        public static void Init()
        {
            messagesNetDictionary = new Dictionary<NetMenssage, Type>();
            messagesTypeDictionary = new Dictionary<Type, Type>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                List<Attribute> attribute = new List<Attribute>(type.GetCustomAttributes<NetValueTypeMessage>());

                if(attribute.Count > 0)
                {
                    NetValueTypeMessage netMessageValue = attribute[0] as NetValueTypeMessage;

                    messagesNetDictionary.Add(netMessageValue.netMenssage, type);
                    messagesTypeDictionary.Add(netMessageValue.type, type);

                    UtilsTools.LOG("Type Message Cache : " + type.ToString());
                }
            }
        }

        public static void UpdateNetObjts()
        {
            for (int i = 0; i < netObjets.Count; i++)
            {
                try
                {
                    UtilsTools.LOG("Obj : " + netObjets[i].getID() + " : Owner " + netObjets[i].getOwner() + " : I = " + i);
                    Type type = netObjets[i].GetType();
                    UtilsTools.LOG("Type : " + type.ToString());

                    foreach (FieldInfo info in type.GetFields(
                        BindingFlags.NonPublic |
                        BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        object obj = info.GetValue(netObjets[i]);
                        NetValue netValueAttribute = info.GetCustomAttribute<NetValue>();

                        if (obj != null && netValueAttribute != null && netObjets[i].getNetObj().id == ClientManager.Instance.idClient)
                        {
                            UtilsTools.LOG("Property : " + info.ToString() + " : " + obj.GetType());
                            try
                            {
                                UtilsTools.LOG($"Attribute id : {netValueAttribute.id} ");

                                UtilsTools.LOG($"Field Name : {info.Name}, Value : {obj}, Type : {obj.GetType().Name}");

                                UtilsTools.LOG($"Obj Name : {obj.ToString()}, Owner : {netObjets[i].getNetObj().owner}, ID : {netObjets[i].getNetObj().id}");

                                UtilsTools.LOG($"Messge to send : " + messagesTypeDictionary[obj.GetType()].ToString());

                                Type netMsg = messagesTypeDictionary[obj.GetType()];
                                UtilsTools.LOG($"Get class : " + netMsg.ToString());

                                object netMsgInstance = Activator.CreateInstance(netMsg);
                                UtilsTools.LOG($"Get new instance : " + netMsgInstance.ToString());

                                MethodInfo setValueMethod = netMsg.GetMethod("SetData");
                                UtilsTools.LOG($"Get method : " + setValueMethod.ToString());

                                setValueMethod.Invoke(netMsgInstance, new object[] { obj});
                                UtilsTools.LOG($"Set Value with method : " + setValueMethod.ToString() + " to : " + obj);

                                MethodInfo serializerMethod = netMsg.GetMethod("SerializeWithValueID");
                                UtilsTools.LOG($"Get method : " + serializerMethod.ToString());

                                object serializedMessage = serializerMethod.Invoke(netMsgInstance, new object[] { netValueAttribute.id, netObjets[i].getID()});
                                UtilsTools.LOG($"send netObj  net attribute id : {netValueAttribute.id} : net obj id : {netObjets[i].getID()}");

                                ClientManager.Instance.SendToServer(serializedMessage as byte[]);
                            }
                            catch (Exception ex)
                            {
                                UtilsTools.LOG("Error en ReadValue: " + ex.Message);
                            }
                        }
                        else
                        {
                            UtilsTools.LOG("Property : " + info.ToString() + " : NULL");
                        }
                    }
                }
                catch (Exception ex)
                {
                    UtilsTools.LOG("¡¡ERROR!! : " + ex.Message);
                }
            }
        }
    }
}