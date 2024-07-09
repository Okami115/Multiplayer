using OkamiNet.data;
using OkamiNet.Menssage;
using OkamiNet.Network;
using OkamiNet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class MatchMaker : IReceiveData
{
    public IPAddress ipAddress
    {
        get; private set;
    }

    public static int port { get; private set; } = 55555;

    private int portToRedirect = port + 1;

    public bool isServer
    {
        get; private set;
    }

    public int TimeOut = 30;

    public static MatchMaker Instance = new MatchMaker();

    private DateTime lastMessageImportantSend;

    private UdpConnection connection;

    public int idClient = 0;
    public int idObjs = 0;

    public Player player;

    public List<FactoryData> FactoryData;

    public List<string> netNames;
    private static readonly List<Process> processes = new List<Process>();
    public List<Process> servers = processes;
    private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

    static void Main(string[] args)
    {
        UtilsTools.LOG += DrawText;
        Instance.StartMatchMaker(port);

        while (true)
        {
            Instance.UpdateMatchMaker();
        }
        UtilsTools.LOG -= DrawText;
    }

    public void StartMatchMaker(int port)
    {
        Instance = this;

        Reflection.Init();
        Console.WriteLine("----- Init MatchMaker V0.1 - Okami Industries -----");

        isServer = true;
        connection = new UdpConnection(port, this);

        netNames = new List<string>();
        FactoryData = new List<FactoryData>();
    }

    public void AddClient(IPEndPoint ip, string name)
    {
        if (!ipToId.ContainsKey(ip))
        {
            Console.WriteLine("Add Client");

            ipToId[ip] = idClient;

            S2CHandShake s2CHandShake = new S2CHandShake(idClient);
            Instance.SendToClient(s2CHandShake.Serialize(), ip);

            NetFactoryDataSpawn netFactoryDataSpawn = new NetFactoryDataSpawn(FactoryData);
            Instance.SendToClient(netFactoryDataSpawn.Serialize(), ip);

            Player player = new Player(ipToId[ip], name);

            clients.Add(player.id, new Client(ip, player.id));

            AddPlayer newPlayer = new AddPlayer();

            newPlayer.data = player;

            Instance.Broadcast(newPlayer.Serialize());

            idClient++;

        }
    }

    public void RemoveClient(int id, IPEndPoint ip)
    {
        Console.WriteLine("Delete Client");

        NetDisconect netDisconect = new NetDisconect();

        Instance.SendToClient(netDisconect.Serialize(), ip);

        clients.Remove(id);
        ipToId.Remove(ip);
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        if (data == null || data.Count() <= 0)
            return;

        if (!Checksum.ChecksumConfirm(data))
            return;

        NetMenssage aux = (NetMenssage)BitConverter.ToInt32(data, 0);


        MenssageFlags flags = OkamiNet.Menssage.Flags.FlagsCheck(data);

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
            case NetMenssage.C2S:
                Console.WriteLine("New C2S");
                C2SHandShake C2SHandShake = new C2SHandShake("");
                DeniedNet temp = new DeniedNet();
                string name = C2SHandShake.Deserialize(data);

                temp.data = "Authorized";

                if (idClient >= 5)
                    temp.data = "Full";

                for (int i = 0; i < Instance.netNames.Count; i++)
                {
                    if (Instance.netNames[i] == name)
                        temp.data = "Name";
                }

                SendToClient(temp.Serialize(), ip);

                if (temp.data == "Authorized")
                {
                    Instance.AddClient(ip, name);
                }
                break;
            case NetMenssage.Ping:

                if (clients.Count >= 2)
                {
                    Instance.CreateServerProcess(portToRedirect++);

                    RemoveClient(clients[0].id, GetIpById(clients[0].id));
                    RemoveClient(clients[1].id, GetIpById(clients[1].id));
                }

                if (clients.Count <= 0)
                    return;

                NetPing ping = new NetPing();
                int latencyMilliseconds = 0;

                TimeSpan latency = DateTime.UtcNow - Instance.clients[ipToId[ip]].lastpingSend;
                latencyMilliseconds = (int)latency.TotalMilliseconds;

                Client auxClient = Instance.clients[ipToId[ip]];

                auxClient.lastpingSend = DateTime.UtcNow;

                Instance.clients[ipToId[ip]] = auxClient;

                ping.data = 0;
                Instance.SendToClient(ping.Serialize(), ip);
                Console.WriteLine("Cliente " + ipToId[ip] + " : ping : " + latencyMilliseconds);
                break;
            case NetMenssage.Disconect:
                NetDisconect dis = new NetDisconect();
                id = ipToId[ip];
                dis.data = id;
                Broadcast(dis.Serialize());
                RemoveClient(id, ip);
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
        NetDisconect dis = new NetDisconect();

        connection.Close();
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

    public void UpdateMatchMaker()
    {
        if (connection != null)
            connection.FlushReceiveData();

        //TryToReSendImportantMessage();
        //DisconetForPing(Instance.isServer);
    }

    public IPEndPoint GetIpById(int id)
    {
        foreach (KeyValuePair<IPEndPoint, int> port in ipToId)
        {
            if (port.Value == id)
            {
                return port.Key;
            }
        }
        return null;
    }

    private void DisconetForPing(bool isServer)
    {
        foreach(KeyValuePair<int, Client> client in clients)
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
        if (clients.Count == 0)
            return;

        TimeSpan latency = DateTime.UtcNow - lastMessageImportantSend;
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

    Process CreateServerProcess(int numberPort)
    {
        Process currentServer;
        ProcessStartInfo startInfo = new ProcessStartInfo();

        string serverPath = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName + "\\OkamiServer\\bin\\Debug\\net8.0\\OkamiServer.exe";

        startInfo.FileName = serverPath;
        startInfo.Arguments = numberPort.ToString() + " " + GetLocalIPAddress();

        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = false;
        startInfo.WindowStyle = ProcessWindowStyle.Normal;

        currentServer = Process.Start(startInfo);


        Thread.Sleep(1000);

        ChangePort changePort = new ChangePort();

        changePort.data.Item1 = numberPort;
        changePort.data.Item2 = GetLocalIPAddress();

        Broadcast(changePort.Serialize());

        return currentServer;
    }

    private static void DrawText(string text)
    {
        Console.WriteLine(text);
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}
