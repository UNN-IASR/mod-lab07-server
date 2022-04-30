using System;
using System.Threading;

namespace lab07_server
{
    public class GetStat
    {
        double l;
        double u;
        double p;
        double P0;
        double Pn;
        double Q;
        double A;
        double k;
        public GetStat(int thCount, int thPause, int reqCount, int reqTimer)
        {
            double P0temp = 0.0;
            l = 1.0 / ((double)reqTimer / 1000.0);
            u = 1.0 / ((double)thPause / 1000.0);
            p = l / u;
            
            for (int i=0; i<=thCount; i++)
            {
                P0temp = P0temp + Math.Pow(p, i) / (double)Factorial(i);
            }
            P0 = 1 / P0temp;

            Pn = Math.Pow(p, thCount) * P0 / (double)Factorial(thCount);

            Q = 1 - Pn;

            A = l * Q;

            k = A / u;

        }
        public int Factorial(int n)
        {
            if (n == 0)
                return 1;
            else
            {
                int temp = 1;
                for (int i=0; i<n;i++)
                {
                    temp = temp * (i+1);
                }
                return temp;
            }
        }

        public void Show()
        {
            Console.WriteLine("Интенсивность поступления требований - {0}", l);
            Console.WriteLine("Интенсивность обслуживания требований - {0}", u);
            Console.WriteLine("Интенсивность потока заявок - {0}", p);
            Console.WriteLine("Вероятность простоя системы - {0}", P0);
            Console.WriteLine("Вероятность отказа системы - {0}", Pn);
            Console.WriteLine("Относительная пропускная способность - {0}", Q);
            Console.WriteLine("Абсолютная пропускная способность - {0}", A);
            Console.WriteLine("Среднее число занятых каналов - {0}", k);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            int threadsCount = 5;
            int threadPause = 1500;
            int requestCount = 500;
            int requestTimer = 50;
            Server tempServer = new Server(threadsCount, threadPause);
            Client tempClient = new Client(tempServer);
            GetStat test = new GetStat(threadsCount, threadPause, requestCount, requestTimer);
            for (int i=0; i< requestCount; i++)
            {
                tempClient.Send(i);
                Thread.Sleep(requestTimer);
            }
            Console.WriteLine();
            Console.WriteLine("Всего запросов - {0}", tempServer.requestCount);
            Console.WriteLine("Обработанных запросов - {0}", tempServer.processedCount);
            Console.WriteLine("Отмененных запросов - {0}", tempServer.rejectedCount);
            Console.WriteLine();
            test.Show();
        }
    }
}
