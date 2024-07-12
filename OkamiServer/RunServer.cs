using OkamiNet.Network;
using OkamiNet.Utils;
using System.Diagnostics;

namespace OkamiServer
{
    internal class RunServer
    {
        static bool isRunning = true;

        static ServerManager serverManager = new ServerManager();

        static void Main(string[] args)
        {
            Console.WriteLine(args[0] + " " + args[1]);
            UtilsTools.LOG += DrawText;
            serverManager.StartServer(int.Parse(args[0]), args[1]);

            while(isRunning)
            {
                serverManager.UpdateServer();
                Thread.Sleep(150);
            }

            UtilsTools.LOG -= DrawText;
        }

        private static void DrawText(string text)
        {
            Console.WriteLine(text);
        }
    }
}
