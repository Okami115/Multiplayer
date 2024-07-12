using OkamiNet.data;
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
        public Action StartClientScreen;
        public Action StartMatch;

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

            if (Menssage.Flags.FlagsCheck(data) == (MenssageFlags.Ordenable))
            {
                if (BitConverter.ToUInt32(data, 12) < MessageReciveHistorial[aux])
                    return;
            }

            bool isImportant = (Menssage.Flags.FlagsCheck(data) == (MenssageFlags.Importants));

            int id;
            int ObjID;
            int AttributeID;

            List<ParentsTree> parents = new List<ParentsTree>();

            switch (aux)
            {
                case NetMenssage.Float:
                    UtilsTools.LOG?.Invoke("Recive new NetFloat");
                    NetFloat netFloat = new NetFloat();

                    netFloat.data = netFloat.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netFloat.data, parents, ObjID);

                    break;
                case NetMenssage.Bool:
                    UtilsTools.LOG?.Invoke("Recive new NetBool");
                    NetBool netBool = new NetBool();

                    MessageReciveHistorial[aux] = BitConverter.ToUInt32(data, 12);

                    if (isImportant)
                    {

                    }

                    netBool.data = netBool.DeserializeWithNetValueId(data, out parents, out ObjID);
                    UtilsTools.LOG?.Invoke($"Recive Obj Id {ObjID} : Parent Tree Count {parents.Count} : {parents[^1].collectionPos} : {parents[^1].collectionSize} : {parents[^1].ID}");

                    SetNetValueTree(netBool.data, parents, ObjID);
                    break;
                case NetMenssage.Byte:
                    UtilsTools.LOG?.Invoke("Recive new NetByte");
                    NetByte netByte = new NetByte();

                    netByte.data = netByte.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netByte.data, parents, ObjID);

                    break;
                case NetMenssage.SByte:
                    UtilsTools.LOG?.Invoke("Recive new NetSByte");
                    NetSbyte netSbyte = new NetSbyte();

                    netSbyte.data = netSbyte.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netSbyte.data, parents, ObjID);

                    break;
                case NetMenssage.Short:
                    UtilsTools.LOG?.Invoke("Recive new NetShort");
                    NetShort netShort = new NetShort();

                    netShort.data = netShort.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netShort.data, parents, ObjID);

                    break;
                case NetMenssage.UShort:
                    UtilsTools.LOG?.Invoke("Recive new NetUshort");
                    NetUShort netUShort = new NetUShort();

                    netUShort.data = netUShort.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netUShort.data, parents, ObjID);

                    break;
                case NetMenssage.Int:
                    UtilsTools.LOG?.Invoke("Recive new NetInt");
                    NetInt netInt = new NetInt();

                    netInt.data = netInt.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netInt.data, parents, ObjID);

                    break;
                case NetMenssage.UInt:
                    UtilsTools.LOG?.Invoke("Recive new NetUInt");
                    NetUInt netUInt = new NetUInt();

                    netUInt.data = netUInt.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netUInt.data, parents, ObjID);

                    break;
                case NetMenssage.Long:
                    UtilsTools.LOG?.Invoke("Recive new NetLong");
                    NetLong netLong = new NetLong();

                    netLong.data = netLong.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netLong.data, parents, ObjID);

                    break;
                case NetMenssage.ULong:
                    UtilsTools.LOG?.Invoke("Recive new NetULong");
                    NetULong netULong = new NetULong();

                    netULong.data = netULong.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netULong.data, parents, ObjID);

                    break;
                case NetMenssage.Decimal:
                    UtilsTools.LOG?.Invoke("Recive new NetDecimal");
                    NetDecimal netDecimal = new NetDecimal();

                    netDecimal.data = netDecimal.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netDecimal.data, parents, ObjID);

                    break;
                case NetMenssage.Double:
                    UtilsTools.LOG?.Invoke("Recive new NetDouble");
                    NetDouble netDouble = new NetDouble();

                    netDouble.data = netDouble.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netDouble.data, parents, ObjID);

                    break;
                case NetMenssage.Char:
                    UtilsTools.LOG?.Invoke("Recive new NetChar");
                    NetChar netChar = new NetChar();

                    netChar.data = netChar.DeserializeWithNetValueId(data, out parents, out ObjID);


                    SetNetValueTree(netChar.data, parents, ObjID);

                    break;
                case NetMenssage.String:
                    UtilsTools.LOG?.Invoke("Recive new NetString");
                    NetString netString = new NetString();

                    netString.data = netString.DeserializeWithNetValueId(data, out parents, out ObjID);


                    UtilsTools.LOG?.Invoke("Recive : " + netString.data);
                    SetNetValueTree(netString.data, parents, ObjID);

                    break;
                case NetMenssage.NullOrEmpty:
                    UtilsTools.LOG?.Invoke("Recive new NullOrEmpty");
                    NullOrEmpty nullOrEmpty = new NullOrEmpty();

                    nullOrEmpty.data = nullOrEmpty.DeserializeWithNetValueId(data, out parents, out ObjID);

                    UtilsTools.LOG?.Invoke("Recive : " + nullOrEmpty.data);
                    SetNetNullOrEmptyValueTree(nullOrEmpty.data, parents, ObjID);
                    break;
                case NetMenssage.S2C:
                    UtilsTools.LOG?.Invoke("New S2C");
                    S2CHandShake s2cHandShake = new S2CHandShake(0);
                    Instance.idClient = s2cHandShake.Deserialize(data);

                    UtilsTools.LOG?.Invoke("Start Player...");
                    StartClientScreen?.Invoke();

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

                    //stopPlayer?.Invoke();
                    //connection.Close();
                    //lastPingSend.Clear();
                    //ipToId.Clear();

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
                case NetMenssage.ChangePort:
                    ChangePort changePort = new ChangePort();
                    UtilsTools.LOG?.Invoke("Recive Change The port");
                    changePort.data = changePort.Deserialize(data);
                    Instance.connection.Close();
                    Instance.connection = null;
                    UtilsTools.LOG?.Invoke("Close Connection");

                    C2SHandShake c2SHandShake = new C2SHandShake(player.name);

                    IPAddress ipAddress = IPAddress.Parse(changePort.data.Item2);
                    UtilsTools.LOG?.Invoke($"IP : " + changePort.data.Item2);

                    this.port = changePort.data.Item1;
                    this.ipAddress = ipAddress;
                    UtilsTools.LOG?.Invoke($"Port : " + changePort.data.Item1);

                    connection = new UdpConnection(ipAddress, port, this);
                    UtilsTools.LOG?.Invoke($"Init Connection");
                    ClientManager.Instance.SendToServer(c2SHandShake.Serialize());
                    UtilsTools.LOG?.Invoke($"Send C2S");
                    StartMatch?.Invoke();

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

        public void SetNetNullOrEmptyValueTree(bool data, List<ParentsTree> parrentTree, int objId)
        {
            if (Reflection.netObjets.Count <= 0)
                return;

            for (int i = 0; i < Reflection.netObjets.Count; i++)
            {
                if (Reflection.netObjets[i].getID() == objId && Reflection.netObjets[i].getOwner() != ClientManager.Instance.idClient)
                {
                    Reflection.netObjets[i] = (INetObj)InspectNullOrEmptyDataToChange(Reflection.netObjets[i].GetType(), Reflection.netObjets[i], data, parrentTree, 0);
                }
            }
        }

        private object InspectDataToChange(Type type, object obj, object data, List<ParentsTree> parentTree, int iterator)
        {
            if (obj == null)
                return obj;

            foreach (MessageData info in Reflection.GetFieldsFromType(type))
            {
                UtilsTools.LOG?.Invoke($"Need Debug :: info :: {info.GetType().ToString()}");

                if (info != null && info.ID == parentTree[iterator].ID)
                {
                    object goNext = null;

                    if (parentTree[iterator].collectionSize == -1)
                    {
                        UtilsTools.LOG?.Invoke($"Non ICollection Debug :: Info is : {info} : {info.GetType()} : {info.ID} : {info.FieldInfo.Name}");
                        UtilsTools.LOG?.Invoke($"Non ICollection Debug :: Obj is : {obj} : {obj.GetType()} : {obj.ToString()}");

                        iterator++;

                        if (iterator >= parentTree.Count)
                        {
                            obj = SetValue(info.FieldInfo, obj, data);
                        }
                        else
                        {
                            goNext = info.FieldInfo.GetValue(obj);

                            if (goNext == null)
                            {
                                goNext = CreateByConstructor(info.FieldInfo.FieldType);
                            }

                            goNext = InspectDataToChange(info.FieldInfo.FieldType, goNext, data, parentTree, iterator);
                            info.FieldInfo.SetValue(obj, goNext);
                            UtilsTools.LOG?.Invoke($"Non ICollection Debug :: Go next is : {goNext} : {goNext.GetType()} : {goNext.ToString()}");
                        }

                        iterator--;
                    }
                    else
                    {

                        if (typeof(System.Collections.ICollection).IsAssignableFrom(info.FieldInfo.FieldType))
                        {
                            UtilsTools.LOG?.Invoke($"ICollection Debug :: Info is : {info} : {info.GetType()} : {info.ID} : {info.FieldInfo.Name}");
                            UtilsTools.LOG?.Invoke($"ICollection Debug :: Obj is : {obj} : {obj.GetType()} : {obj.ToString()}");

                            goNext = info.FieldInfo.GetValue(obj);

                            if(goNext == null)
                            {
                                goNext = CreateByConstructor(info.FieldInfo.FieldType);
                            }

                            UtilsTools.LOG?.Invoke($"ICollection Debug :: Go next is : {goNext} : {goNext.GetType()} : {goNext.ToString()}");

                            int collectionSize = (goNext as ICollection).Count;
                            UtilsTools.LOG?.Invoke($"ICollection Debug :: colletion Size is : {collectionSize}");

                            int auxIterator = 0;

                            object[] objects = new object[parentTree[iterator].collectionSize];
                            UtilsTools.LOG?.Invoke($"ICollection Debug :: objects Length is : {objects.Length}");


                            if (parentTree[iterator].collectionSize == collectionSize)
                            {
                                (info.FieldInfo.GetValue(obj) as ICollection).CopyTo(objects, 0);
                            }
                            else
                            {
                                for (int i = 0; i < objects.Length; i++)
                                {
                                    if (i < collectionSize)
                                    {
                                        IEnumerator enumerator = (goNext as ICollection).GetEnumerator();

                                        int count = 0;

                                        while (enumerator.MoveNext())
                                        {
                                            if (count == i)
                                            {
                                                objects[i] = enumerator.Current;
                                                break;
                                            }

                                            count++;
                                        }
                                    }
                                    else
                                    {
                                        objects[i] = Activator.CreateInstance(GetElementType(info.FieldInfo.FieldType));
                                    }
                                }
                            }


                            for (int i = 0; i < collectionSize; i++)
                            {
                                if (i == parentTree[iterator].collectionPos)
                                {
                                    if (((objects[i].GetType().IsValueType && objects[i].GetType().IsPrimitive) || objects[i].GetType() == typeof(string) || objects[i].GetType().IsEnum || objects[i].GetType() == typeof(decimal)))
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

        private object InspectNullOrEmptyDataToChange(Type type, object obj, bool data, List<ParentsTree> parentTree, int iterator)
        {
            if (obj == null)
                return obj;

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
                            if (data)
                            {
                                goNext = info.FieldInfo.GetValue(obj);

                                if (type.IsArray)
                                {
                                    goNext = Array.CreateInstance(info.FieldInfo.FieldType.GetElementType(), 0);
                                }
                                else if (typeof(System.Collections.ICollection).IsAssignableFrom(info.FieldInfo.FieldType) && info.FieldInfo.FieldType.IsGenericType)
                                {
                                    goNext = Activator.CreateInstance(typeof(List<>).MakeGenericType(GetElementType(info.FieldInfo.FieldType)));
                                }

                                info.FieldInfo.SetValue(obj, goNext);
                                return obj;
                            }
                            else
                            {
                                obj = SetValue(info.FieldInfo, obj, null);
                            }

                        }
                        else
                        {
                            UtilsTools.LOG?.Invoke($"ICollection Debug :: Go next is : {goNext}");
                            goNext = info.FieldInfo.GetValue(obj);
                            goNext = InspectNullOrEmptyDataToChange(info.FieldInfo.FieldType, goNext, data, parentTree, iterator);
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


                            if (parentTree[iterator].collectionSize == collectionSize)
                            {
                                (info.FieldInfo.GetValue(obj) as ICollection).CopyTo(objects, 0);
                            }
                            else
                            {
                                for (int i = 0; i < objects.Length; i++)
                                {
                                    if (i < collectionSize)
                                    {
                                        IEnumerator enumerator = (goNext as ICollection).GetEnumerator();

                                        int count = 0;

                                        while (enumerator.MoveNext())
                                        {
                                            if (count == i)
                                                objects[i] = enumerator.Current;

                                            count++;
                                        }
                                    }
                                    else
                                    {
                                        objects[i] = Activator.CreateInstance(GetElementType(info.FieldInfo.FieldType));
                                    }
                                }
                            }


                            for (int i = 0; i < collectionSize; i++)
                            {
                                if (i == parentTree[iterator].collectionPos)
                                {
                                    if (((objects[i].GetType().IsValueType && objects[i].GetType().IsPrimitive) || objects[i].GetType() == typeof(string) || objects[i].GetType().IsEnum || objects[i].GetType() == typeof(decimal)))
                                    {
                                        objects[i] = data;
                                    }
                                    else
                                    {
                                        object item = InspectNullOrEmptyDataToChange(objects[i].GetType(), objects[i], data, parentTree, iterator + 1);
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

        private object SetValue(FieldInfo fieldInfo, object obj, object? data)
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

        private object CreateByConstructor(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            ConstructorInfo constructorInfo = null;

            foreach (ConstructorInfo constructor in constructors)
            {
                if (constructor.GetParameters().Length == 0)
                {
                    return constructor.Invoke(new object[0]);
                }
                else
                {
                    foreach (ParameterInfo param in constructor.GetParameters())
                    {
                        if (param.ParameterType == type)
                        {
                            continue;
                        }

                    }

                    constructorInfo = constructor;
                }
            }

            ParameterInfo[] parametersInfos = constructorInfo.GetParameters();
            object[] parameters = new object[parametersInfos.Length];

            for (int i = 0; parametersInfos.Length > i; i++)
            {
                if (parametersInfos[i].ParameterType.IsValueType || parametersInfos[i].ParameterType == typeof(string) || parametersInfos[i].ParameterType.IsEnum)
                {
                    parameters[i] = default;
                }
                else
                {
                    parameters[i] = CreateByConstructor(parametersInfos[i].ParameterType);
                }

            }

            return constructorInfo.Invoke(parameters);

        }

        private Type GetElementType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return typeof(object);
        }
    }
}