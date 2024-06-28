using OkamiNet.Network;
using OkamiNet.Utils;

namespace OkamiServer
{
    internal class RunServer
    {
        static bool isRunning = true;

        static ServerManager serverManager = new ServerManager();

        static void Main(string[] args)
        {
            UtilsTools.LOG += DrawText;
            serverManager.StartServer();

            while(isRunning)
            {
                serverManager.UpdateServer();
                Thread.Sleep(10);
            }
        }

        private static void DrawText(string text)
        {
            Console.WriteLine(text);
        }
    }
}
