using OkamiNet.Network;
using OkamiNet.Utils;

namespace OkamiServer
{
    internal class RunServer
    {
        static bool isRunning = true;

        static NetworkManager networkManager = new NetworkManager();

        static void Main(string[] args)
        {
            UtilsTools.LOG += DrawText;
            networkManager.StartServer();

            while(isRunning)
            {
                Console.Clear();
                networkManager.UpdateServer();
                Thread.Sleep(10);
            }
        }

        private static void DrawText(string text)
        {
            Console.WriteLine(text);
        }
    }
}
