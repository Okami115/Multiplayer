using OkamiNet.data;
using OkamiNet.Menssage;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

namespace OkamiNet.Network
{
    public struct Client
    {
        public int id;
        public IPEndPoint ipEndPoint;

        public Dictionary<NetMenssage, uint> MessageHistorial;
        public Dictionary<NetMenssage, uint> MessageReciveHistorial;
        public Dictionary<NetMenssage, List<DataCache>> messageCache;

        public DateTime lastpingSend;

        public Client(IPEndPoint ipEndPoint, int id)
        {
            MessageHistorial = new Dictionary<NetMenssage, uint>();
            MessageReciveHistorial = new Dictionary<NetMenssage, uint>();
            messageCache = new Dictionary<NetMenssage, List<DataCache>>();

            lastpingSend = DateTime.MinValue;

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                List<Attribute> attribute = new List<Attribute>(type.GetCustomAttributes<NetValueTypeMessage>());

                if (attribute.Count > 0)
                {
                    NetValueTypeMessage netMessageValue = attribute[0] as NetValueTypeMessage;

                    MessageHistorial.Add(netMessageValue.netMenssage, 0);
                    MessageReciveHistorial.Add(netMessageValue.netMenssage, 0);
                }
            }

            this.id = id;
            this.ipEndPoint = ipEndPoint;
        }
    }

    public struct Player
    {
        public int id;
        public string name;
        public int HP;
        public System.Numerics.Vector3 pos;
        public System.Numerics.Vector2 rotation;

        public Player(int id, string name)
        {
            this.id = id;
            this.name = name;
            HP = 3;
            pos = System.Numerics.Vector3.Zero;
            rotation = System.Numerics.Vector2.Zero;
        }
    }

    public class ServerManager : IReceiveData
    {
        public IPAddress ipAddress
        {
            get; private set;
        }
        public int port
        {
            get; private set;
        }
        public bool isServer
        {
            get; private set;
        }

        public int TimeOut = 30;

        public static ServerManager Instance;

        public List<DateTime> lastPingSend;

        private DateTime lastMessageImportantSend;

        public Action<string> newText;
        public Action<int> spawnPlayer;
        public Action StartMap;

        public Action<System.Numerics.Vector3, int> updatePos;
        public Action<System.Numerics.Vector2, int> updateRot;
        public Action<int> updateShoot;
        public Action<float> updateTimer;

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

        public void StartServer(int port, string ip)
        {
            StartServer();

            isServer = true;
            this.port = port;

            IPAddress ipAddress = IPAddress.Parse(ip);
            connection = new UdpConnection(ipAddress, port, this);

            netNames = new List<string>();
            lastPingSend = new List<DateTime>();
            FactoryData = new List<FactoryData>();
        }

        public void AddClient(IPEndPoint ip, string name)
        {
            if (!ipToId.ContainsKey(ip))
            {
                UtilsTools.LOG?.Invoke("Adding client: " + ip.Address + " name: " + name);

                ipToId[ip] = idClient;

                S2CHandShake s2CHandShake = new S2CHandShake(idClient);
                UtilsTools.LOG?.Invoke("Send S2C");
                Instance.SendToClient(s2CHandShake.Serialize(), ip);

                NetFactoryDataSpawn netFactoryDataSpawn = new NetFactoryDataSpawn(FactoryData);
                Instance.SendToClient(netFactoryDataSpawn.Serialize(), ip);

                Player player = new Player(ipToId[ip], name);

                clients.Add(player.id, new Client(ip, player.id));

                AddPlayer newPlayer = new AddPlayer();

                newPlayer.data = player;

                Instance.Broadcast(newPlayer.Serialize());

                lastPingSend.Add(DateTime.UtcNow);

                spawnPlayer?.Invoke(player.id);

                idClient++;

            }
        }

        public void RemoveClient(int id, IPEndPoint ip)
        {
            NetDisconect netDisconect = new NetDisconect();

            Instance.SendToClient(netDisconect.Serialize(), ip);

            clients.Remove(id);
            ipToId.Remove(ip);
        }

        public void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            UtilsTools.LOG?.Invoke("Llego");
            if (data == null || data.Count() <= 0)
                return;
            UtilsTools.LOG?.Invoke("Tiene algo");

            if (!Checksum.ChecksumConfirm(data))
                return;
            UtilsTools.LOG?.Invoke("No esta roto");

            NetMenssage aux = (NetMenssage)BitConverter.ToInt32(data, 0);
            UtilsTools.LOG?.Invoke("Es un " + aux.ToString());


            MenssageFlags flags = Menssage.Flags.FlagsCheck(data);

            bool isOrdenable = flags.HasFlag(MenssageFlags.Ordenable);
            bool isImportant = flags.HasFlag(MenssageFlags.Importants);
            uint idMessage = 0;

            if (isOrdenable)
            {
                idMessage = BitConverter.ToUInt32(data, 12);
            }

            int id;

            switch (aux)
            {
                case NetMenssage.Float:
                    UtilsTools.LOG?.Invoke("New NetFloat");

                    if (isImportant)
                    {
                        DataCache auxDataCache = new DataCache(data, DateTime.UtcNow);

                        if (isOrdenable)
                        {
                            if (idMessage > clients[ipToId[ip]].MessageHistorial[aux])
                            {
                                clients[ipToId[ip]].MessageHistorial[aux] = idMessage;

                                NetFloat netFloat = new NetFloat();

                                List<ParentsTree> parents = new List<ParentsTree>();

                                int objID;

                                netFloat.data = netFloat.DeserializeWithNetValueId(data, out parents, out objID);

                                Broadcast(netFloat.SerializeWithValueID(parents, objID, flags));
                            }
                        }
                    }
                    else if (isOrdenable)
                    {
                        if (idMessage > clients[ipToId[ip]].MessageHistorial[aux])
                        {
                            clients[ipToId[ip]].MessageHistorial[aux] = idMessage;

                            NetFloat netFloat = new NetFloat();

                            List<ParentsTree> parents = new List<ParentsTree>();

                            int objID;

                            netFloat.data = netFloat.DeserializeWithNetValueId(data, out parents, out objID);

                            Broadcast(netFloat.SerializeWithValueID(parents, objID, flags));
                        }
                    }
                    else
                    {
                        Broadcast(data);
                    }

                    break;
                case NetMenssage.C2S:
                    UtilsTools.LOG?.Invoke("New C2S");
                    C2SHandShake C2SHandShake = new C2SHandShake("");
                    DeniedNet temp = new DeniedNet();
                    string name = C2SHandShake.Deserialize(data);

                    temp.data = "Authorized";

                    SendToClient(temp.Serialize(), ip);
                    Instance.AddClient(ip, name);

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

                    ping.data = 0;
                    Instance.SendToClient(ping.Serialize(), ip);
                    UtilsTools.LOG?.Invoke("Cliente " + idPing + " : ping : " + latencyMilliseconds);
                    break;
                case NetMenssage.AddPlayer:
                    AddPlayer addPlayer = new AddPlayer();
                    addPlayer.data = addPlayer.Deserialize(data);
                    break;
                case NetMenssage.Disconect:
                    NetDisconect dis = new NetDisconect();
                    id = ipToId[ip];
                    dis.data = id;
                    Broadcast(dis.Serialize());
                    RemoveClient(id, ip);
                    break;
                case NetMenssage.FactoryRequest:
                    UtilsTools.LOG?.Invoke("Recive Factory Request");
                    FactoryRequest factoryRequest = new FactoryRequest();
                    factoryRequest.data = factoryRequest.Deserialize(data);

                    factoryRequest.data.netObj.owner = id = ipToId[ip];
                    factoryRequest.data.netObj.id = idObjs;
                    idObjs++;

                    FactoryMenssage factoryMenssage = new FactoryMenssage();
                    factoryMenssage.data = factoryRequest.data;
                    FactoryData.Add(factoryMenssage.data);
                    UtilsTools.LOG?.Invoke("Send Factory Message");
                    Broadcast(factoryMenssage.Serialize());
                    break;
                case NetMenssage.ChangePort:
                    ChangePort changePort = new ChangePort();
                    changePort.data = changePort.Deserialize(data);

                    StartServer(changePort.data.Item1, changePort.data.Item2);
                    break;
                default:
                    break;
            }
        }

        public void SendToClient(byte[] data, IPEndPoint ip)
        {
            if (connection != null)
                connection.Send(data, ip);
        }

        public void Disconnect()
        {
            stopPlayer?.Invoke();

            NetDisconect dis = new NetDisconect();

            connection.Close();
            lastPingSend.Clear();
            ipToId.Clear();
            idClient = 0;
        }

        public void Broadcast(byte[] data)
        {
            using (var iterator = clients.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    connection.Send(data, iterator.Current.Value.ipEndPoint);
                }
            }
        }

        public void Broadcast(byte[] data, int clientsID)
        {
            connection.Send(data, clients[clientsID].ipEndPoint);
        }

        public void StartServer()
        {
            if (Instance == null)
                Instance = this;

            Reflection.Init();
            UtilsTools.LOG?.Invoke("----- Init Server V0.1 - Okami Industries -----");
        }

        public void UpdateServer()
        {
            if (connection != null)
            {

                UtilsTools.LOG?.Invoke("Updating....");
                connection.FlushReceiveData();
                TryToReSendImportantMessage();
                DisconetForPing(Instance.isServer);

            }

        }

        public IPEndPoint GetIpById(int id)
        {
            foreach (var aux in ipToId)
            {
                if (aux.Value == id)
                {
                    return aux.Key;
                }
            }
            return null;
        }

        private void DisconetForPing(bool isServer)
        {
            foreach (KeyValuePair<int, Client> client in clients)
            {
                TimeSpan latency = DateTime.UtcNow - client.Value.lastpingSend;
                int latencyMilliseconds = (int)latency.TotalMilliseconds;

                if (latencyMilliseconds > TimeOut)
                {
                    RemoveClient(client.Value.id, client.Value.ipEndPoint);
                }
            }
        }

        private void TryToReSendImportantMessage()
        {
            TimeSpan latency = DateTime.UtcNow - Instance.lastMessageImportantSend;
            int latencySeconds = (int)latency.TotalSeconds;

            Dictionary<int, Dictionary<NetMenssage, List<int>>> delete = new Dictionary<int, Dictionary<NetMenssage, List<int>>>();

            foreach (KeyValuePair<int, Client> client in clients)
            {
                if (delete[client.Key] == null)
                    delete[client.Key] = new Dictionary<NetMenssage, List<int>>();

                foreach (KeyValuePair<NetMenssage, List<DataCache>> cache in client.Value.messageCache)
                {
                    if (delete[client.Key][cache.Key] == null)
                        delete[client.Key][cache.Key] = new List<int>();

                    for (int i = 0; i < cache.Value.Count; i++)
                    {
                        if ((cache.Value[i].sendTime - DateTime.UtcNow).TotalSeconds > 5 || cache.Value[i].checkClient.Count == clients.Count)
                        {
                            delete[client.Key][cache.Key].Add(i);
                        }
                    }
                }
            }

            foreach (KeyValuePair<int, Dictionary<NetMenssage, List<int>>> ToDelete in delete)
            {
                foreach (KeyValuePair<NetMenssage, List<int>> ToDeletePerClient in ToDelete.Value)
                {
                    foreach (int DeleteMessageID in ToDeletePerClient.Value)
                    {
                        clients[ToDelete.Key].messageCache[ToDeletePerClient.Key].RemoveAt(DeleteMessageID);
                    }
                }
            }

            foreach (KeyValuePair<int, Client> client in clients)
            {
                foreach (KeyValuePair<NetMenssage, List<DataCache>> cache in client.Value.messageCache)
                {
                    for (int i = 0; i < cache.Value.Count; i++)
                    {
                        if ((cache.Value[i].sendTime - DateTime.UtcNow).TotalSeconds > 0.5f)
                        {
                            if (!cache.Value[i].checkClient.Contains(client.Key))
                                Broadcast(cache.Value[i].data, client.Key);
                        }
                    }
                }
            }
        }
    }

    public struct DataCache
    {
        public byte[] data;
        public DateTime sendTime;
        public List<int> checkClient;

        public DataCache(byte[] data, DateTime sendTime)
        {
            checkClient = new List<int>();
            this.data = data;
            this.sendTime = sendTime;
        }

    }
}