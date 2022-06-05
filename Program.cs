using System;
using System.IO;
using System.Threading;

namespace lab
{
    class Program
    {
        static void Main(string[] args)
        {
            long factorial(long n)
            {
                if (n == 0)
                    return 1;
                else
                    return n * factorial(n - 1);
            }

            int countPool = 5;
            int mu = 10; //  intensivnost obsluzhivanija trebovanij (obratnaya)
            int zaderzhkamu = 100;
            int la = 10; // intensivnost postuplenija trebovanij
            double dla = 10.0;
            int zaderzhkala = 100;
            int countTreb = 100;

            Server server = new Server(countPool, zaderzhkamu);
            Client client = new Client(server);

            for (int id = 1; id <= countTreb; id++)
            {

                client.send(id);
                Thread.Sleep(zaderzhkala);
            }
            Thread.Sleep(1000);
            Console.WriteLine("\n___________________________________\n");

            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            Console.WriteLine("\n___________________________________\n");
            Console.WriteLine("Лямбда: " + la + " мю: " + mu + " количество потоков: " + countPool);
            double p = dla / mu;

            double temp = 0;
            for (long i = 0; i <= countPool; i++)
                temp = temp + Math.Pow(p, i) / factorial(i);

            double p0 = 1 / temp;
            Console.WriteLine("Вероятность простоя системы: " + $"{p0:f6}");
            double pn = Math.Pow(p, countPool) * p0 / factorial(countPool);
            Console.WriteLine("Вероятность отказа системы: " + $"{pn:f6}");
            Console.WriteLine("Относительная пропускная способность: " + $"{(1 - pn):f6}");
            Console.WriteLine("Абсолютная пропускная способность: " + $"{(la * (1 - pn)):f6}");
            Console.WriteLine("Среднее число занятых каналов: " + $"{((la * (1 - pn)) / mu):f6}");
        }
    }
}
