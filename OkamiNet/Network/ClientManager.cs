using OkamiNet.data;
using OkamiNet.Menssage;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

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

        public void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
        {
            if (data == null || data.Count() <= 0)
                return;

            if (!Checksum.ChecksumConfirm(data))
                return;

            NetMenssage aux = (NetMenssage)BitConverter.ToInt32(data, 0);
            int id;
            int ObjID;
            int AttributeID;

            switch (aux)
            {
                case NetMenssage.String:

                    UtilsTools.LOG?.Invoke("New mensages");
                    NetString consoleMensajes = new NetString("");
                    string text = consoleMensajes.Deserialize(data);
                    newText?.Invoke(text);
                    UtilsTools.LOG?.Invoke(text);
                    break;
                case NetMenssage.Vector3:
                    NetVector3 pos = new NetVector3();
                    System.Numerics.Vector3 newPos = pos.Deserialize(data);

                    break;
                case NetMenssage.Rotation:
                    NetVector2 rot = new NetVector2();
                    break;
                case NetMenssage.Shoot:
                    NetInt shoot = new NetInt();
                    updateShoot?.Invoke(shoot.Deserialize(data));

                    break;
                case NetMenssage.Float:
                    UtilsTools.LOG?.Invoke("Recive new netFloat");
                    NetFloat netFloat = new NetFloat();

                    netFloat.data = netFloat.DeserializeWithNetValueId(data, out ObjID, out AttributeID);
                    UtilsTools.LOG?.Invoke($"Recive attribute Id {AttributeID} : ObjId {ObjID}");

                    SetNetValue(netFloat.data, AttributeID, ObjID);

                    break;
                case NetMenssage.Int:
                    UtilsTools.LOG?.Invoke("Recive new netInt");
                    NetInt netInt = new NetInt();
                    netInt.data = netInt.DeserializeWithNetValueId(data, out ObjID, out AttributeID);
                    UtilsTools.LOG?.Invoke($"Recive attribute Id {AttributeID} : ObjId {ObjID}");

                    SetNetValue(netInt.data, AttributeID, ObjID);
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

        public void SendToServer(byte[] data)
        {
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

        public void SetNetValue(object value, int id, int objId)
        {
            if (Reflection.netObjets.Count <= 0)
                return;

            UtilsTools.LOG($"Value to Read : {value}, Value ID to Read : {id}, Obj ID to Read : {objId}");

            for (int i = 0;i < Reflection.netObjets.Count; i++)
            {
                if (Reflection.netObjets[i].getID() == objId && Reflection.netObjets[i].getOwner() != ClientManager.Instance.idClient)
                {
                    UtilsTools.LOG($"Try Read ID : {Reflection.netObjets[i].getID()}, Try read Owner : {Reflection.netObjets[i].getOwner()}");

                    Type type = Reflection.netObjets[i].GetType();
                    UtilsTools.LOG("Type To read : " + type.ToString());

                    foreach (FieldInfo info in type.GetFields(
                        BindingFlags.NonPublic |
                        BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        object obj = info.GetValue(Reflection.netObjets[i]);
                        NetValue netValueAttribute = info.GetCustomAttribute<NetValue>();

                        if (obj != null && netValueAttribute != null && netValueAttribute.id == id)
                        {
                            UtilsTools.LOG("Property To Read : " + info.ToString() + " : " + obj.GetType() + " Value : " + value);
                            info.SetValue(Reflection.netObjets[i], value);
                        }
                        else
                        {
                            UtilsTools.LOG("Property : " + info.ToString() + " : NULL : ID " + id);
                        }
                    }
                }
            }
        }
    }
}