using OkamiNet.data;
using OkamiNet.Menssage;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Reflection;

namespace OkamiNet.Network
{
    public struct Client
    {
        public int id;
        public IPEndPoint ipEndPoint;

        public Client(IPEndPoint ipEndPoint, int id)
        {
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

    public struct FactoryData
    {
        public int id;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
        public int parentId;

        public FactoryData(int id, Vector3 pos, Quaternion rot, Vector3 scale, int parentId)
        {
            this.id = id;
            this.pos = pos;
            this.rot = rot; 
            this.scale = scale;
            this.parentId = parentId;
        }
    }

    public struct FactoryObj
    {
        public int clientId;
        public int instanceId;

        public FactoryObj(int clientId, int instanceId)
        {
            this.clientId = clientId;
            this.instanceId = instanceId;
        }
    }

    // Sintetizar, Separar en NetClient y NetServer
    public class NetworkManager : IReceiveData
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

        public static NetworkManager  Instance;

        public List<DateTime> lastPingSend;

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

        public List<Player> playerList;
        public Player player;

        private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

        public void StartServer(int port)
        {
            isServer = true;
            this.port = port;
            connection = new UdpConnection(port, this);

            playerList = new List<Player>();
            lastPingSend = new List<DateTime>();
        }

        public void StartClient(IPAddress ip, int port, string name)
        {
            isServer = false;

            this.port = port;
            this.ipAddress = ip;

            connection = new UdpConnection(ip, port, this);
            playerList = new List<Player>();

            player = new Player(-1, name);
            lastPingSend = new List<DateTime>();
        }

        public void AddClient(IPEndPoint ip, string name)
        {
            if (!ipToId.ContainsKey(ip))
            {
                UtilsTools.LOG?.Invoke("Adding client: " + ip.Address + " name: " + name);

                ipToId[ip] = idClient;

                Player player = new Player(ipToId[ip], name);

                clients.Add(player.id, new Client(ip, player.id));

                AddPlayer newPlayer = new AddPlayer();

                newPlayer.data = player;    

                Instance.Broadcast(newPlayer.Serialize(0));

                playerList.Add(player);

                lastPingSend.Add(DateTime.UtcNow);

                spawnPlayer?.Invoke(player.id);

                idClient++;

            }
        }

        public void RemoveClient(int id, IPEndPoint ip)
        {
            disconectPlayer?.Invoke(id);

            if(Instance.isServer)
            {
                clients.Remove(id);
                ipToId.Remove(ip);

                int ed = 0;

                for (int i = 0; i < Instance.playerList.Count; i++)
                {
                    if (Instance.playerList[i].id == id)
                        ed = i;
                }

                lastPingSend.Remove(lastPingSend[ed]);
                playerList.Remove(playerList[ed]);
            }

            if (Instance.player.id == id)
            {
                stopPlayer?.Invoke();
                connection.Close();
                playerList.Clear();
                lastPingSend.Clear();
                ipToId.Clear();
            }
        }
        public void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            if (data == null)
                return;

            if (!Checksum.ChecksumConfirm(data))
                return;

            NetMenssage aux = (NetMenssage)BitConverter.ToInt32(data, 0);
            int id;

            switch (aux)
            {
                case NetMenssage.String:

                    UtilsTools.LOG?.Invoke("New mensages");
                    NetString consoleMensajes = new NetString("");
                    string text = consoleMensajes.Deserialize(data);
                    newText?.Invoke(text);
                    UtilsTools.LOG?.Invoke(text);
                    if (Instance.isServer)
                        Instance.Broadcast(data);
                    break;
                case NetMenssage.Vector3:
                    NetVector3 pos = new NetVector3();
                    System.Numerics.Vector3 newPos = pos.Deserialize(data);

                    if (isServer && ipToId.Count != 0)
                    {
                        id = ipToId[ip];

                        Player player = Instance.playerList[id];

                        player.pos = newPos;

                        Instance.playerList[id] = player;
                        //updatePos?.Invoke(newPos, pos.GetOwner(data));
                    }

                    break;
                case NetMenssage.Rotation:
                    NetVector2 rot = new NetVector2();
                    if (Instance.isServer && ipToId.Count != 0)
                    {
                        id = ipToId[ip];
                        System.Numerics.Vector2 newRotation = rot.Deserialize(data);

                        updateRot?.Invoke(newRotation, id);
                    }
                    break;
                case NetMenssage.Disconect:
                    NetDisconect dis = new NetDisconect();

                    if(Instance.isServer)
                    {
                        id = ipToId[ip];
                        dis.data = id;
                        Broadcast(dis.Serialize(0));
                        RemoveClient(id, ip);
                    }
                    else
                    {
                        id = dis.Deserialize(data);
                        RemoveClient(id, ip);
                    }


                    break;
                case NetMenssage.AddPlayer:
                    AddPlayer addPlayer = new AddPlayer();
                    addPlayer.data = addPlayer.Deserialize(data);
                    break;
                case NetMenssage.C2S:
                    UtilsTools.LOG?.Invoke("New C2S");
                    C2SHandShake C2SHandShake = new C2SHandShake("");
                    DeniedNet temp = new DeniedNet();
                    string name = C2SHandShake.Deserialize(data);

                    temp.data = "Authorized";

                    if (idClient >= 5)
                        temp.data = "Full";

                    for (int i = 0; i < Instance.playerList.Count; i++)
                    {
                        if(Instance.playerList[i].name == name)
                            temp.data = "Name";
                    }

                    SendToClient(temp.Serialize(0), ip);

                    if(temp.data == "Authorized")
                    {
                        Instance.AddClient(ip, name);

                        S2CHandShake s2CHandShake = new S2CHandShake(Instance.playerList);
                        byte[] players = s2CHandShake.Serialize(0);
                        Instance.Broadcast(players);
                        UtilsTools.LOG?.Invoke("Send S2C");
                    }
                    break;
                case NetMenssage.S2C:
                    UtilsTools.LOG?.Invoke("New S2C");
                    S2CHandShake s2cHandShake = new S2CHandShake(Instance.playerList);
                    Instance.playerList = s2cHandShake.Deserialize(data);

                    for (int i = 0; i < Instance.playerList.Count; i++)
                        if (Instance.player.name == Instance.playerList[i].name)
                            Instance.player.id = Instance.playerList[i].id;

                    UtilsTools.LOG?.Invoke("Updating player list...");
                    StartMap?.Invoke();

                    NetPing Startping = new NetPing();
                    Startping.data = Instance.player.id;
                    Instance.lastPingSend.Add(DateTime.UtcNow);
                    Instance.SendToServer(Startping.Serialize(0));
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

                    if (Instance.isServer)
                    {
                        ping.data = 0;
                        Instance.SendToClient(ping.Serialize(0), ip);
                        UtilsTools.LOG?.Invoke("Cliente " + idPing + " : ping : " + latencyMilliseconds);
                    }
                    else
                    {
                        UtilsTools.LOG?.Invoke("ping : " + latencyMilliseconds);
                        ping.data = Instance.player.id;
                        Instance.SendToServer(ping.Serialize(0));
                    }
                    break;
                case NetMenssage.PlayerList:
                    NetPlayerListUpdate updating = new NetPlayerListUpdate(playerList);
                    playerList = updating.Deserialize(data);
                     break;
                case NetMenssage.Shoot:
                    NetInt shoot = new NetInt();

                    updateShoot?.Invoke(shoot.Deserialize(data));

                    break;
                case NetMenssage.Denied:

                    DeniedNet denied = new DeniedNet();

                    denied.data = denied.Deserialize(data);

                    deniedConnection?.Invoke(denied.data);

                    break;
                case NetMenssage.Float:
                    NetFloat timer = new NetFloat();
                    timer.data = timer.Deserialize(data);
                    updateTimer?.Invoke(timer.data);

                    break;
                case NetMenssage.FactoryMessage:
                    FactoryRequest factoryRequest = new FactoryRequest();
                    factoryRequest.data = factoryRequest.Deserialize(data);

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

        public void SendToClient(byte[] data, IPEndPoint ip)
        {
            if(connection != null)
                connection.Send(data, ip);
        }

        public void Disconnect()
        {
            stopPlayer?.Invoke();

            if (Instance.isServer)
            {
                NetDisconect dis = new NetDisconect();

                for (int i = 1; i < playerList.Count; i++)
                {
                    dis.data = playerList[i].id;
                    SendToClient(dis.Serialize(0), GetIpById(playerList[i].id));
                }
                connection.Close();
                playerList.Clear();
                lastPingSend.Clear();
                ipToId.Clear();
                idClient = 0;
            }
            else
            {
                NetDisconect dis = new NetDisconect();
                dis.data = Instance.player.id;
                Instance.SendToServer(dis.Serialize(0));
            }
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

        public void StartServer()
        {
            if (Instance == null)
                Instance = this;
         
            UtilsTools.LOG?.Invoke("----- Init Server V0.1 - Okami Industries -----");
            Instance.StartServer(55557);

        }

        public void StartClient()
        {
            if (Instance == null)
                Instance = this;
        }

        public void UpdateServer()
        {
            // Flush the data in main thread
            if (connection != null)
                connection.FlushReceiveData();

            DisconetForPing(Instance.isServer);

            NetPlayerListUpdate playerListData = new NetPlayerListUpdate(playerList);
            Broadcast(playerListData.Serialize(0));
        }

        public void UpdateClient()
        {
            if (connection != null)
                connection.FlushReceiveData();

            DisconetForPing(Instance.isServer);

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

                            if (Instance.playerList[i].id != Instance.player.id)
                            {
                                IPEndPoint ip = Instance.GetIpById(Instance.playerList[i].id);
                                dis.data = Instance.playerList[i].id;
                                Instance.Broadcast(dis.Serialize(0));
                                Instance.RemoveClient(Instance.playerList[i].id, ip);
                            }
                        }
                        else
                        {
                            stopPlayer?.Invoke();
                            connection.Close();
                            playerList.Clear();
                            lastPingSend.Clear();
                            ipToId.Clear();
                        }
                    }
                }
            }
        }
    }

    public class NetValue : Attribute
    {
        int id;
        public NetValue(int id)
        {
            this.id = id;
        }
    }
    
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

    public static class Reflection
    {
        public static void Inspect(Type type, object obj)
        {
            if (obj != null)
            {

                foreach (FieldInfo info in type.GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    ReadValue(info, obj);
                }

                if (type.BaseType != null)
                {
                    Inspect(type.BaseType, obj);
                }
            }
        }

        public static void ReadValue(FieldInfo info, object obj)
        {
            if (info.FieldType.IsValueType || info.FieldType == typeof(string) || info.FieldType.IsEnum)
            {
                UtilsTools.LOG(info.Name + ": " + info.GetValue(obj));
            }
            else if (typeof(System.Collections.ICollection).IsAssignableFrom(info.FieldType))
            {
                foreach (object item in (info.GetValue(obj) as System.Collections.ICollection))
                {
                    Inspect(item.GetType(), item);
                }
            }
            else
            {
                Inspect(info.FieldType, info.GetValue(obj));
            }
        }
    }
}