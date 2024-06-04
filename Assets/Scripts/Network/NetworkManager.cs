using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using UnityEngine;


// Go to DLL
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

// Sintetizar
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
        this.pos = System.Numerics.Vector3.Zero;
        this.rotation = System.Numerics.Vector2.Zero;
    }
}

// Sintetizar, Independizar de Unity y Go to DLL
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

        player = new Player(idClient, "Server");
        lastPingSend.Add(DateTime.UtcNow);
        idClient++;
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
            Console.WriteLine("Adding client: " + ip.Address + " name: " + name);

            ipToId[ip] = idClient;

            Player player = new Player(ipToId[ip], name);

            clients.Add(player.id, new Client(ip, player.id));

            AddPlayer newPlayer = new AddPlayer();

            newPlayer.data = player;    

            Instance.Broadcast(newPlayer.Serialize());

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

        MessageType aux = (MessageType)BitConverter.ToInt32(data, 0);
        int id;

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
                if(Instance.isServer && ipToId.Count != 0)
                {
                    id = ipToId[ip];
                    System.Numerics.Vector3 newPos = pos.Deserialize(data);

                    updatePos?.Invoke(newPos, id);
                }
                break;
            case MessageType.Rotation:
                NetVector2 rot = new NetVector2();
                if (Instance.isServer && ipToId.Count != 0)
                {
                    id = ipToId[ip];
                    System.Numerics.Vector2 newRotation = rot.Deserialize(data);

                    updateRot?.Invoke(newRotation, id);
                }
                break;
            case MessageType.Disconect:
                NetDisconect dis = new NetDisconect();

                if(Instance.isServer)
                {
                    id = ipToId[ip];
                    dis.data = id;
                    Broadcast(dis.Serialize());
                    RemoveClient(id, ip);
                }
                else
                {
                    id = dis.Deserialize(data);
                    RemoveClient(id, ip);
                }


                break;
            case MessageType.AddPlayer:
                AddPlayer addPlayer = new AddPlayer();
                addPlayer.data = addPlayer.Deserialize(data);
                break;
            case MessageType.C2S:
                UnityEngine.Debug.Log("New C2S");
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

                SendToClient(temp.Serialize(), ip);

                if(temp.data == "Authorized")
                {
                    Instance.AddClient(ip, name);

                    S2CHandShake s2CHandShake = new S2CHandShake(Instance.playerList);
                    byte[] players = s2CHandShake.Serialize();
                    Instance.Broadcast(players);
                    UnityEngine.Debug.Log("Send S2C");
                }
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
            case MessageType.Shoot:
                NetShoot shoot = new NetShoot();

                updateShoot?.Invoke(shoot.Deserialize(data));

                break;
            case MessageType.Denied:

                DeniedNet denied = new DeniedNet();

                denied.data = denied.Deserialize(data);

                deniedConnection?.Invoke(denied.data);

                break;
            case MessageType.Timer:
                NetTimer timer = new NetTimer();
                timer.data = timer.Deserialize(data);
                updateTimer?.Invoke(timer.data);

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
                SendToClient(dis.Serialize(), GetIpById(playerList[i].id));
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
            Instance.SendToServer(dis.Serialize());
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

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();

        if(lastPingSend != null)
        {
            for(int i = 0; i < lastPingSend.Count; i++)
            {
                TimeSpan latency = DateTime.UtcNow - Instance.lastPingSend[i];
                int latencySeconds = (int)latency.TotalSeconds;

                if (latencySeconds > TimeOut)
                {
                    if (Instance.isServer)
                    {
                        NetDisconect dis = new NetDisconect();

                        if(Instance.playerList[i].id != Instance.player.id)
                        {
                            IPEndPoint ip = Instance.GetIpById(Instance.playerList[i].id);
                            dis.data = Instance.playerList[i].id;
                            Instance.Broadcast(dis.Serialize());
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


        if(Instance.isServer && playerList != null)
        {
            NetPlayerListUpdate updating = new NetPlayerListUpdate(playerList);
            byte[] players = updating.Serialize();
            Instance.Broadcast(players);
        }
    }

    public IPEndPoint GetIpById(int id)
    {
        foreach (var kvp in ipToId)
        {
            if (kvp.Value == id)
            {
                return kvp.Key;
            }
        }
        return null;
    }


}