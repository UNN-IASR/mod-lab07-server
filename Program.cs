using System;
using System.Threading;

namespace Lab_07
{
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }

    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int requestsIntensity = 100;
            int serviceIntensity = 2;
            int numberThread = 4;
            Server server = new Server(numberThread, requestsIntensity);
            Client client = new Client(server);

            for (int id = 1; id <= 100; id++)
            {
                client.OnProc(id);
                Thread.Sleep(serviceIntensity);
            }

            Console.WriteLine("All Request: {0}", server.requestCount);
            Console.WriteLine("Accepted Request: {0}", server.servicedCount);
            Console.WriteLine("Declined Request: {0}", server.rejectedCount);
        }
    }
}
