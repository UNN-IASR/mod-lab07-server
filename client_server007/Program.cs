using System;
using System.Threading;

namespace client_server007
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(5, 200);
            Client client = new Client(server);

            for (int i = 1; i <= 100; i++)
            {
                client.proc(i);
                Thread.Sleep(40);
            }

            Console.WriteLine();
            Console.WriteLine("RESULTS:");
            Console.WriteLine("Number of all requests: {0}", server.getRequestCount());
            Console.WriteLine("Number of processed requests: {0}", server.getProcessedCount());
            Console.WriteLine("Number of rejected requests: {0}", server.getRejectedCount());
        }
    }
}