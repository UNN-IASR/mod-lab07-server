using System;
using System.Threading;

namespace lab7mipis
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
            int requests_intensity = 100;
            int service_intensity = 20;

            Server server = new Server(5, requests_intensity);
            Client client = new Client(server);

            for (int id = 1; id <= 100; id++)
            {
                client.OnProc(id);
                Thread.Sleep(service_intensity);
            }


            Console.WriteLine("All requests: {0}", server.requestCount);
            Console.WriteLine("Processed requests: {0}", server.processedCount);
            Console.WriteLine("Rejected requests: {0}", server.rejectedCount);
        }
    }
}