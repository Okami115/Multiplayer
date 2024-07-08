using OkamiNet.Menssage;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OkamiNet.Network
{
    public static class Reflection
    {
        public static List<INetObj> netObjets = new List<INetObj>();

        private static Dictionary<Type, NetMenssage> messagesNetDictionary;
        private static Dictionary<Type, Type> messagesTypeDictionary;

        public static void InspectMessage(Type type, object obj, int objId, List<ParentsTree> parentTrees, bool isIcollection)
        {
            if (obj != null)
            {
                UtilsTools.LOG($"Para leer : {GetFieldsFromType(type).Count} de : {type.Name}");
                foreach (MessageData data in GetFieldsFromType(type))
                {
                    parentTrees.Add(new ParentsTree(data.ID, -1, -1));
                    ReadValue(data.FieldInfo, obj, objId, parentTrees);
                    UtilsTools.LOG($"Se read Valuo");
                    parentTrees.RemoveAt(parentTrees.Count - 1);
                }

                if (isIcollection)
                {
                    foreach (FieldInfo info in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public |
                                          BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        ReadValue(info, obj, objId, parentTrees);
                    }
                }
            }
        }

        public static void ReadValue(FieldInfo info, object obj, int objId, List<ParentsTree> parentTrees)
        {
            if ((info.FieldType.IsValueType && info.FieldType.IsPrimitive) || info.FieldType == typeof(string) || info.FieldType.IsEnum)
            {
                UtilsTools.LOG(info.Name + ": " + info.GetValue(obj) + " : ¡Funca!");
                SendNetValues(info, objId, obj, parentTrees);
            }
            else if (typeof(System.Collections.ICollection).IsAssignableFrom(info.FieldType))
            {
                UtilsTools.LOG($"Insert Collection : Count = {(info.GetValue(obj) as System.Collections.ICollection).Count}");

                int collectionSize = (info.GetValue(obj) as System.Collections.ICollection).Count;

                int iterator = 0;

                foreach (object item in info.GetValue(obj) as System.Collections.ICollection)
                {
                    ParentsTree currentParent = parentTrees[^1];

                    currentParent.collectionPos = iterator++;
                    currentParent.collectionSize = collectionSize;

                    parentTrees[^1] = currentParent;

                    UtilsTools.LOG($"Insert Collection in Foreach : {item.GetType()} : {item} : id obj {objId} : {parentTrees.Count}");

                    InspectMessage(item.GetType(), item, objId, parentTrees, true);
                }
            }
            else
            {
                UtilsTools.LOG("Insert");
                InspectMessage(info.FieldType, info.GetValue(obj), objId, parentTrees, false);
            }
        }

        public static void Init()
        {
            messagesNetDictionary = new Dictionary<Type, NetMenssage>();
            messagesTypeDictionary = new Dictionary<Type, Type>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                List<Attribute> attribute = new List<Attribute>(type.GetCustomAttributes<NetValueTypeMessage>());

                if (attribute.Count > 0)
                {
                    NetValueTypeMessage netMessageValue = attribute[0] as NetValueTypeMessage;

                    messagesNetDictionary.Add(type, netMessageValue.netMenssage);
                    messagesTypeDictionary.Add(netMessageValue.type, type);

                    UtilsTools.LOG("Type Message Cache : " + type.ToString());
                    UtilsTools.LOG("Type Message Cache : " + netMessageValue.netMenssage.ToString());
                }
            }
        }

        public static void UpdateNetObjts()
        {
            for (int i = 0; i < netObjets.Count; i++)
            {
                try
                {
                    UtilsTools.LOG($"Try N : {i}");

                    Type type = netObjets[i].GetType();

                    INetObj currentNetObj = netObjets[i];

                    List<ParentsTree> currentTree = new List<ParentsTree>();

                    if (netObjets[i].getNetObj().id == ClientManager.Instance.idClient)
                        InspectMessage(type, currentNetObj, currentNetObj.getID(), currentTree, false);
                }
                catch (Exception ex)
                {
                    UtilsTools.LOG("¡¡ERROR!! : " + ex.Message);
                }
            }
        }

        private static void SendNetValues(FieldInfo info, int ObjId, object obj, List<ParentsTree> parents)
        {
            object data = info.GetValue(obj);

            if (obj == null)
                return;

            try
            {
                Type netMsg = messagesTypeDictionary[data.GetType()];

                object netMsgInstance = Activator.CreateInstance(netMsg);

                MethodInfo setValueMethod = netMsg.GetMethod("SetData");

                setValueMethod.Invoke(netMsgInstance, new object[] { data });

                MethodInfo serializerMethod = netMsg.GetMethod("SerializeWithValueID");
                UtilsTools.LOG($"Find this method : {serializerMethod.Name} : parameters {serializerMethod.GetParameters().Length}");

                UtilsTools.LOG($"Type del : {ClientManager.Instance.MessageHistorial[messagesNetDictionary[netMsg]].GetType().Name}");
                object serializedMessage = serializerMethod.Invoke(netMsgInstance, new object[] { parents, ObjId, (MenssageFlags.Ordenable)});
                UtilsTools.LOG($"ID del proximo mensaje : {ClientManager.Instance.MessageHistorial[messagesNetDictionary[netMsg]]}");

                ClientManager.Instance.MessageHistorial[messagesNetDictionary[netMsg]]++;

                UtilsTools.LOG($"Send : {info.Name} : parants {parents.Count} : {parents[^1].collectionPos} : {parents[^1].collectionSize} : {parents[^1].ID}");
                ClientManager.Instance.SendToServer(serializedMessage as byte[]);
            }
            catch (Exception ex)
            {
                UtilsTools.LOG("Error en ReadValue: " + ex.Message);
            }
        }

        public static List<MessageData> GetFieldsFromType(Type type)
        {
            List<MessageData> output = new List<MessageData>();
            foreach (FieldInfo info in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public |
                                                      BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                NetValue netValue = info.GetCustomAttribute<NetValue>();
                if (netValue != null)
                {
                    output.Add(new MessageData(info, netValue.id));
                }
            }

            /*
            if (extensionMethods.TryGetValue(type, out MethodInfo? method))
            {
                consoleMessage.Invoke($"{type.Name}");
                object uninitializedObject = FormatterServices.GetUninitializedObject(type);

                object fields = method.Invoke(null, new object[] { uninitializedObject });
                if (fields != null)
                {
                    output.AddRange((fields as List<MessageData>));
                }
            }
            */

            return output;
        }
    }


    public struct ParentsTree
    {
        public int ID;
        public int collectionPos;
        public int collectionSize;

        public ParentsTree(int id, int collectionPos, int collectionSize)
        {
            this.ID = id;
            this.collectionPos = collectionPos;
            this.collectionSize = collectionSize;
        }
    }

    public class MessageData
    {
        public int ID;
        public FieldInfo FieldInfo;

        public MessageData(FieldInfo fieldInfo, int id)
        {
            this.FieldInfo = fieldInfo;
            ID = id;
        }
    }
}