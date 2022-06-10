using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Threading;

namespace lab7
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
            Console.WriteLine("Итого:");
            Console.WriteLine("Количество запросов: {0}", server.getRequestCount());
            Console.WriteLine("Обработанные запросы: {0}", server.getProcessedCount());
            Console.WriteLine("Отклоненные запросы: {0}", server.getRejectedCount());
        }
    }
}