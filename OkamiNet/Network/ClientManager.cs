﻿using OkamiNet.data;
using OkamiNet.Menssage;
using OkamiNet.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace OkamiNet.Network
{
    public class ClientManager : IReceiveData
    {
        public IPAddress ipAddress
        {
            get; private set;
        }
        public int port
        {
            get; private set;
        }

        public int TimeOut = 30;

        public static ClientManager Instance;

        public List<DateTime> lastPingSend;

        public Action<string> newText;
        public Action<int> spawnPlayer;
        public Action StartMap;

        public Action<System.Numerics.Vector3, int> updatePos;
        public Action<System.Numerics.Vector2, int> updateRot;
        public Action<int> updateShoot;
        public Action<float, int> reciveFloat;

        public Action<int> disconectPlayer;
        public Action stopPlayer;
        public Action<int> connectPlayer;
        public Action<string> deniedConnection;

        private UdpConnection connection;

        public int idClient = 0;
        public int idObjs = 0;

        public Player player;

        public List<FactoryData> FactoryData;

        public List<string> netNames;

        private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

        public Dictionary<NetMenssage, uint> MessageHistorial = new Dictionary<NetMenssage, uint>();
        public Dictionary<NetMenssage, uint> MessageReciveHistorial = new Dictionary<NetMenssage, uint>();

        public void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
        {
            if (data == null || data.Count() <= 0)
                return;

            if (!Checksum.ChecksumConfirm(data))
                return;

            NetMenssage aux = (NetMenssage)BitConverter.ToInt32(data, 0);

            if (Menssage.Flags.FlagsCheck(data) == (MenssageFlags.Ordenable | MenssageFlags.Descartables))
            {
                if (BitConverter.ToUInt32(data, 12) < MessageReciveHistorial[aux])
                    return;
            }

            int id;
            int ObjID;
            int AttributeID;

            switch (aux)
            {
                case NetMenssage.Float:
                    UtilsTools.LOG?.Invoke("Recive new netFloat");
                    NetFloat netFloat = new NetFloat();

                    List<ParentsTree> parents = new List<ParentsTree>();

                    MessageReciveHistorial[aux] = BitConverter.ToUInt32(data, 12);

                    int objID;

                    netFloat.data = netFloat.DeserializeWithNetValueId(data, out parents, out objID);
                    UtilsTools.LOG?.Invoke($"Recive Obj Id {objID} : Parent Tree Count {parents.Count} : {parents[^1].collectionPos} : {parents[^1].collectionSize} : {parents[^1].ID}");

                    SetNetValueTree(netFloat.data, parents, objID);

                    break;
                case NetMenssage.S2C:
                    UtilsTools.LOG?.Invoke("New S2C");
                    S2CHandShake s2cHandShake = new S2CHandShake(0);
                    Instance.idClient = s2cHandShake.Deserialize(data);

                    UtilsTools.LOG?.Invoke("Start Player...");
                    StartMap?.Invoke();

                    NetPing Startping = new NetPing();
                    Startping.data = Instance.idClient;
                    Instance.lastPingSend.Add(DateTime.UtcNow);
                    Instance.SendToServer(Startping.Serialize());
                    break;
                case NetMenssage.Ping:
                    NetPing ping = new NetPing();

                    int idPing = ping.Deserialize(data);
                    int latencyMilliseconds = 0;

                    if (Instance.lastPingSend.Count >= idPing + 1)
                    {
                        TimeSpan latency = DateTime.UtcNow - Instance.lastPingSend[idPing];
                        latencyMilliseconds = (int)latency.TotalMilliseconds;
                        Instance.lastPingSend[idPing] = DateTime.UtcNow;
                    }
                    UtilsTools.LOG?.Invoke("ping : " + latencyMilliseconds);
                    ping.data = Instance.idClient;
                    Instance.SendToServer(ping.Serialize());

                    break;
                case NetMenssage.Denied:

                    DeniedNet denied = new DeniedNet();

                    denied.data = denied.Deserialize(data);

                    deniedConnection?.Invoke(denied.data);

                    break;
                case NetMenssage.AddPlayer:
                    AddPlayer addPlayer = new AddPlayer();
                    addPlayer.data = addPlayer.Deserialize(data);
                    break;
                case NetMenssage.Disconect:
                    NetDisconect dis = new NetDisconect();
                    id = dis.Deserialize(data);


                    break;
                case NetMenssage.FactoryMessage:
                    UtilsTools.LOG?.Invoke("Recive Factory Message");
                    FactoryMenssage factoryMenssageClient = new FactoryMenssage();

                    factoryMenssageClient.data = factoryMenssageClient.Deserialize(data);

                    UtilsTools.Intanciate?.Invoke(factoryMenssageClient.data);
                    break;
                case NetMenssage.FactoryDataSpawn:
                    UtilsTools.LOG("Recive Factroy Data Spawn");
                    NetFactoryDataSpawn netFactoryDataSpawn = new NetFactoryDataSpawn(FactoryData);
                    FactoryData = netFactoryDataSpawn.Deserialize(data);

                    for (int i = 0; i < FactoryData.Count; i++)
                    {
                        UtilsTools.LOG("Objet : " + i);
                        UtilsTools.Intanciate?.Invoke(FactoryData[i]);
                    }

                    break;
                default:
                    break;
            }
        }

        private Client GetClient(IPEndPoint ip)
        {
            foreach (Client client in clients.Values)
            {
                if (client.id == ipToId[ip])
                {
                    return client;
                }
            }

            return new Client();
        }

        public void SendToServer(byte[] data)
        {
            UtilsTools.LOG($"Connection : {connection != null}");

            if (connection != null)
                connection.Send(data);
        }

        public void StartClient(IPAddress ip, int port, string name)
        {
            this.port = port;
            this.ipAddress = ip;

            connection = new UdpConnection(ip, port, this);

            player = new Player(-1, name);
            lastPingSend = new List<DateTime>();
            FactoryData = new List<FactoryData>();
        }

        public void StartClient()
        {
            if (Instance == null)
                Instance = this;

            Reflection.Init();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                List<Attribute> attribute = new List<Attribute>(type.GetCustomAttributes<NetValueTypeMessage>());

                if (attribute.Count > 0)
                {
                    NetValueTypeMessage netMessageValue = attribute[0] as NetValueTypeMessage;

                    MessageHistorial.Add(netMessageValue.netMenssage, 0);
                    MessageReciveHistorial.Add(netMessageValue.netMenssage, 0);

                    UtilsTools.LOG("Net Message Cache : " + netMessageValue.netMenssage);
                }
            }
            UtilsTools.LOG?.Invoke("----- Init Client V0.1 - Okami Industries -----");
        }

        public void UpdateClient()
        {
            if (connection != null)
                connection.FlushReceiveData();

            DisconetForPing(false);

            Reflection.UpdateNetObjts();
        }

        private void DisconetForPing(bool isServer)
        {
            if (lastPingSend != null)
            {
                for (int i = 0; i < lastPingSend.Count; i++)
                {
                    TimeSpan latency = DateTime.UtcNow - Instance.lastPingSend[i];
                    int latencySeconds = (int)latency.TotalSeconds;

                    if (latencySeconds > TimeOut)
                    {
                        if (isServer)
                        {
                            NetDisconect dis = new NetDisconect();

                        }
                        else
                        {
                            stopPlayer?.Invoke();
                            connection.Close();
                            lastPingSend.Clear();
                            ipToId.Clear();
                        }
                    }
                }
            }
        }

        public void SetNetValue(object value, int parentID, List<ParentsTree> parentTree)
        {
            if (Reflection.netObjets.Count <= 0)
                return;

            UtilsTools.LOG($"Value to Read : {value}, Value ID to Read : {parentID}, Obj ID to Read : {parentTree.Count}");

            for (int i = 0; i < Reflection.netObjets.Count; i++)
            {
                if (Reflection.netObjets[i].getID() == parentID && Reflection.netObjets[i].getOwner() != ClientManager.Instance.idClient)
                {
                    UtilsTools.LOG($"Try Read ID : {Reflection.netObjets[i].getID()}, Try read Owner : {Reflection.netObjets[i].getOwner()}");

                    Type type = Reflection.netObjets[i].GetType();
                    UtilsTools.LOG("Type To read : " + type.ToString());

                    foreach (FieldInfo info in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        object obj = info.GetValue(Reflection.netObjets[i]);
                        NetValue netValueAttribute = info.GetCustomAttribute<NetValue>();

                        if (obj != null && netValueAttribute != null && netValueAttribute.id == parentID)
                        {
                            UtilsTools.LOG("Property To Read : " + info.ToString() + " : " + obj.GetType() + " Value : " + value);
                            info.SetValue(Reflection.netObjets[i], value);
                        }
                        else
                        {
                            UtilsTools.LOG("Property : " + info.ToString() + " : NULL : ID " + parentID);
                        }
                    }
                }
            }
        }

        public void SetNetValueTree(object data, List<ParentsTree> parrentTree, int objId)
        {
            if (Reflection.netObjets.Count <= 0)
                return;

            for (int i = 0; i < Reflection.netObjets.Count; i++)
            {
                if (Reflection.netObjets[i].getID() == objId && Reflection.netObjets[i].getOwner() != ClientManager.Instance.idClient)
                {
                    Reflection.netObjets[i] = (INetObj)InspectDataToChange(Reflection.netObjets[i].GetType(), Reflection.netObjets[i], data, parrentTree, 0);
                }
            }
        }

        private object InspectDataToChange(Type type, object obj, object data, List<ParentsTree> parentTree, int iterator)
        {
            if (obj == null)
                return obj;
            /*
            if (iterator >= parentTree.Count)
            {
                foreach (FieldInfo info in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public |
                                          BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    UtilsTools.LOG($"Value to Read A : {info.FieldType}");
                    UtilsTools.LOG($"Value to Read B : {type.GenericTypeArguments[0]}");
                    if (info.FieldType == type.GenericTypeArguments[0])
                    {
                        UtilsTools.LOG($"Value to Read : {info.Name}");
                        obj = SetValue(info, obj, data);
                        break;
                    }
                }

            }
            else
            {
            }
             */

            foreach (MessageData info in Reflection.GetFieldsFromType(type))
            {
                if (info != null && info.ID == parentTree[iterator].ID)
                {
                    object goNext = null;

                    UtilsTools.LOG?.Invoke($"ICollection Debug :: Info is : {info} : {info.GetType()} : {info.ID} : {info.FieldInfo.Name}");
                    if (parentTree[iterator].collectionSize == -1)
                    {
                        iterator++;

                        if (iterator >= parentTree.Count)
                        {
                            obj = SetValue(info.FieldInfo, obj, data);
                        }
                        else
                        {
                            UtilsTools.LOG?.Invoke($"ICollection Debug :: Go next is : {goNext}");
                            goNext = info.FieldInfo.GetValue(obj);
                            goNext = InspectDataToChange(info.FieldInfo.FieldType, goNext, data, parentTree, iterator);
                            info.FieldInfo.SetValue(obj, goNext);
                        }

                        iterator--;
                    }
                    else
                    {
                        if (typeof(System.Collections.ICollection).IsAssignableFrom(info.FieldInfo.FieldType))
                        {
                            goNext = info.FieldInfo.GetValue(obj);

                            UtilsTools.LOG?.Invoke($"ICollection Debug :: Go next is : {goNext}");

                            int collectionSize = (goNext as ICollection).Count;
                            UtilsTools.LOG?.Invoke($"ICollection Debug :: colletion Size is : {collectionSize}");

                            int auxIterator = 0;

                            object[] objects = new object[parentTree[iterator].collectionSize];
                            UtilsTools.LOG?.Invoke($"ICollection Debug :: objects Length is : {objects.Length}");

                            (info.FieldInfo.GetValue(obj) as ICollection).CopyTo(objects, 0);


                            for (int i = 0; i < collectionSize; i++)
                            {
                                if (i == parentTree[iterator].collectionPos)
                                {
                                    if (((objects[i].GetType().IsValueType && objects[i].GetType().IsPrimitive) || objects[i].GetType() == typeof(string) || objects[i].GetType().IsEnum))
                                    {
                                        objects[i] = data;
                                    }
                                    else
                                    {
                                        object item = InspectDataToChange(objects[i].GetType(), objects[i], data, parentTree, iterator + 1);
                                        UtilsTools.LOG?.Invoke($"ICollection Debug :: item type is : {objects[i].GetType()} : {objects[i]} : {data}");
                                        UtilsTools.LOG?.Invoke($"ICollection Debug :: item parrent Tree is : {parentTree.Count} : {parentTree[^1].collectionPos} : {parentTree[^1].collectionSize} : {parentTree[^1].ID}");

                                        objects[i] = item;
                                    }

                                    break;
                                }
                            }

                            object arrayAsGeneric;

                            if (info.FieldInfo.FieldType.IsArray)
                            {
                                arrayAsGeneric = typeof(ClientManager).GetMethod(nameof(TranslateArray),
                                    BindingFlags.Instance | BindingFlags.NonPublic).
                                    MakeGenericMethod(info.FieldInfo.FieldType.GetElementType()).
                                    Invoke(this, new[] { objects });

                                goNext = Array.CreateInstance(info.FieldInfo.FieldType.GetElementType(), ((Array)arrayAsGeneric).Length);

                                Array.Copy((Array)arrayAsGeneric, (Array)goNext, (arrayAsGeneric as ICollection).Count);
                            }
                            else
                            {
                                arrayAsGeneric = typeof(ClientManager).GetMethod(nameof(TranslateICollection),
                                                    BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(info.FieldInfo.FieldType.GenericTypeArguments[0]).
                                                    Invoke(this, new[] { objects });

                                goNext = Activator.CreateInstance(info.FieldInfo.FieldType, arrayAsGeneric as ICollection);
                            }


                        }

                        info.FieldInfo.SetValue(obj, goNext);
                    }
                }

            }

            return obj;
        }

        private object SetValue(FieldInfo fieldInfo, object obj, object data)
        {
            fieldInfo.SetValue(obj, data);

            return obj;
        }

        private object TranslateICollection<T>(object[] objectsToCopy)
        {
            List<T> list = new List<T>();

            foreach (object obj in objectsToCopy)
            {
                list.Add((T)obj);
            }

            return list;
        }

        private object TranslateArray<T>(object[] objectsToCopy)
        {
            T[] array = new T[objectsToCopy.Length];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (T)objectsToCopy[i];
            }

            return array;
        }
    }
}