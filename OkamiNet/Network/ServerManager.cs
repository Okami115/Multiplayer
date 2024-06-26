﻿using OkamiNet.data;
using OkamiNet.Menssage;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

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

        public static ServerManager  Instance;

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
        public int idObjs = 0;

        public Player player;

        public List<FactoryData> FactoryData;

        public List<string> netNames;

        private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

        public void StartServer(int port)
        {
            isServer = true;
            this.port = port;
            connection = new UdpConnection(port, this);

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
            disconectPlayer?.Invoke(id);

            if(Instance.isServer)
            {
                clients.Remove(id);
                ipToId.Remove(ip);

                int ed = 0;

            }

            if (Instance.player.id == id)
            {
                stopPlayer?.Invoke();
                connection.Close();
                lastPingSend.Clear();
                ipToId.Clear();
            }
        }

        public void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            if (data == null || data.Count() <= 0)
                return;

            if (!Checksum.ChecksumConfirm(data))
                return;

            NetMenssage aux = (NetMenssage)BitConverter.ToInt32(data, 0);
            int id;

            switch (aux)
            {
                case NetMenssage.String:
                    NetString consoleMensajes = new NetString("");
                    string text = consoleMensajes.Deserialize(data);
                    UtilsTools.LOG?.Invoke("New mensages : " + text);
                    Instance.Broadcast(data);
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
                    break;
                case NetMenssage.Float:
                    UtilsTools.LOG?.Invoke("New NetFloat");
                    Broadcast(data);

                    break;
                case NetMenssage.C2S:
                    UtilsTools.LOG?.Invoke("New C2S");
                    C2SHandShake C2SHandShake = new C2SHandShake("");
                    DeniedNet temp = new DeniedNet();
                    string name = C2SHandShake.Deserialize(data);

                    temp.data = "Authorized";

                    if (idClient >= 5)
                        temp.data = "Full";

                    for (int i = 0; i < Instance.netNames.Count; i++)
                    {
                        if(Instance.netNames[i] == name)
                            temp.data = "Name";
                    }

                    SendToClient(temp.Serialize(), ip);

                    if(temp.data == "Authorized")
                    {
                        Instance.AddClient(ip, name);
                    }
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
                default:
                    break;
            }
        }

        public void SendToClient(byte[] data, IPEndPoint ip)
        {
            if(connection != null)
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

        public void StartServer()
        {
            if (Instance == null)
                Instance = this;

            Reflection.Init();
            UtilsTools.LOG?.Invoke("----- Init Server V0.1 - Okami Industries -----");
            Instance.StartServer(55555);

        }

        public void UpdateServer()
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
    }
}