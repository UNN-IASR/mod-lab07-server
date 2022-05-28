using System;
using System.Threading;

namespace Lab7
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
            int requestsIntensity = 10;
            int serviceIntensity = 10;
            int numberThread = 5;
            Server server = new Server(numberThread, requestsIntensity);
            Client client = new Client(server);

            for (int id = 1; id <= 100; id++)
            {
                client.OnProc(id);
                Thread.Sleep(serviceIntensity);
            }

            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.servicedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
        }
    }
}
