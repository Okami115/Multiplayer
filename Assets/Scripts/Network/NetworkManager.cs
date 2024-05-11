using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
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

    public Player(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
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

    public List<DateTime> lastPingSend;

    public Action<byte[], IPEndPoint> OnReceiveEvent;

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
        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip);
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

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }
}