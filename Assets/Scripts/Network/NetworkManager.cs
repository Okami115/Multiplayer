using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public struct Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;

        // Subscribir los tipos de mensajes en el constructor del cliente
    }
}

public struct Player
{
    public int id;
    public string name;
    public Vector3 pos;
    public Quaternion rotation;

    public Player(int id, string name)
    {
        this.id = id;
        this.name = name;
        this.pos = Vector3.zero;
        this.rotation = Quaternion.identity;
    }
}

public class NetworkManager : MonoBehaviour, IReceiveData
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

    public static NetworkManager Instance;

    public List<DateTime> lastPingSend;

    public Action<string> newText;
    public Action<int> spawnPlayer;
    public Action StartMap;
    public Action<Vector3, Quaternion, int> updatePos;

    private UdpConnection connection;

    public int idClient = 1;

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

        player = new Player(0, "Server");
        playerList.Add(player);
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
            Debug.Log("Adding client: " + ip.Address + " name: " + name);

            ipToId[ip] = idClient;

            Player player = new Player(ipToId[ip], name);

            playerList.Add(player);

            clients.Add(player.id, new Client(ip, player.id, Time.realtimeSinceStartup));
            lastPingSend.Add(DateTime.UtcNow);

            spawnPlayer?.Invoke(player.id);

            idClient++;
        }
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        MessageType aux = (MessageType)BitConverter.ToInt32(data, 0);

        switch (aux)
        {
            case MessageType.Console:

                UnityEngine.Debug.Log("New mensages");
                NetConsole consoleMensajes = new NetConsole("");
                string text = consoleMensajes.Deserialize(data);
                newText?.Invoke(text);
                UnityEngine.Debug.Log(text);
                if (Instance.isServer)
                    Instance.Broadcast(data);
                break;
            case MessageType.Position:
                NetVector3 pos = new NetVector3();
                if(Instance.isServer)
                {
                    int id = ipToId[ip];
                    updatePos?.Invoke(pos.Deserialize(data), Quaternion.identity, id);
                }

                break;
            case MessageType.Disconect:
                UnityEngine.Debug.Log("Disconect");
                break;
            case MessageType.C2S:
                UnityEngine.Debug.Log("New C2S");
                C2SHandShake C2SHandShake = new C2SHandShake("");
                string name = C2SHandShake.Deserialize(data);
                NetworkManager.Instance.AddClient(ip, name);

                S2CHandShake s2CHandShake = new S2CHandShake(NetworkManager.Instance.playerList);
                byte[] players = s2CHandShake.Serialize();
                NetworkManager.Instance.Broadcast(players);
                UnityEngine.Debug.Log("Send S2C");
                break;
            case MessageType.S2C:
                UnityEngine.Debug.Log("New S2C");
                S2CHandShake s2cHandShake = new S2CHandShake(Instance.playerList);
                Instance.playerList = s2cHandShake.Deserialize(data);

                for (int i = 0; i < Instance.playerList.Count; i++)
                    if (Instance.player.name == Instance.playerList[i].name)
                        Instance.player.id = Instance.playerList[i].id;

                UnityEngine.Debug.Log("Updating player list...");
                StartMap?.Invoke();

                NetPing Startping = new NetPing();
                Startping.data = Instance.player.id;
                Instance.lastPingSend.Add(DateTime.UtcNow);
                Instance.SendToServer(Startping.Serialize());
                break;
            case MessageType.Ping:
                NetPing ping = new NetPing();

                int idPing = ping.Deserialize(data);

                TimeSpan latency = DateTime.UtcNow - Instance.lastPingSend[idPing];
                int latencyMilliseconds = (int)latency.TotalMilliseconds;

                Instance.lastPingSend[idPing] = DateTime.UtcNow;

                if (Instance.isServer)
                {
                    ping.data = 0;
                    Instance.SendToClient(ping.Serialize(), ip);
                    UnityEngine.Debug.Log("Cliente " + idPing + " : ping : " + latencyMilliseconds);
                }
                else
                {
                    UnityEngine.Debug.Log("ping : " + latencyMilliseconds);
                    ping.data = Instance.player.id;
                    Instance.SendToServer(ping.Serialize());
                }
                break;
            case MessageType.PlayerList:
                NetPlayerListUpdate updating = new NetPlayerListUpdate(playerList);
                playerList = updating.Deserialize(data);
                 break;
            default:
                break;
        }
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public void SendToClient(byte[] data, IPEndPoint ip)
    {
        connection.Send(data, ip);
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

    void Start()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();

        if(Instance.isServer)
        {
            NetPlayerListUpdate updating = new NetPlayerListUpdate(playerList);
            byte[] players = updating.Serialize();
            Instance.Broadcast(players);
        }
    }
}