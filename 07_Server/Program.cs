using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace _07_Server
{
        public class procEventArgs : EventArgs
        {
            public int id { get; set; }
        }

        public struct PoolRecord
        {
            public Thread thread;
            public bool in_use; 
        }

        public class Analyze
        {
            public double Lambd { get; set; }
            public double T0 { get; set; }
            public int n { get; set; }
            private double P0;
            private double Pn;
            private double Q;
            private double A;
            private double K;
            public Analyze(double L, double obr_k_mu, int n)
            {
                this.Lambd = L;
                this.T0 = obr_k_mu;
                this.n = n;
            }
            public void calculate()
            {
                P0 = 0.0;
                Pn = 0.0;
                Q = 0.0;
                A = 0.0;
                K = 0.0;
                double p = Lambd * T0;

                for (int i = 0; i <= n; i++)
                {
                    P0 += Math.Pow(p, i) / (double)Factorial(i);
                }
                P0 = 1 / P0;
                Pn = P0 * Math.Pow(p, n) / (double)Factorial(n);
                Q = 1 - Pn;
                A = Lambd * Q;
                K = A * T0;



                Console.WriteLine("Лямбда = {0}", Lambd);
                Console.WriteLine("Мю = {0}", 1 / T0);

                Console.WriteLine("Приведенная интенсивность потока заявок: {0:0.##}", p);
                Console.WriteLine("Вероятность простоя сервера: {0}", P0);
                Console.WriteLine("Вероятность отказа сервера: {0}", Pn);
                Console.WriteLine("Относительная пропускная способность: {0}", Q);
                Console.WriteLine("Абсолютная пропускная способность: {0}", A);
                Console.WriteLine("Среднее число занятых каналов: {0}", K);
            }
            int Factorial(int n)
            {
                if (n == 0) return 1;
                if (n == 1) return 1;

                return n * Factorial(n - 1);
            }
        }

    class Program
    {
        static void Main(string[] args)
        {
            int number = 10;
            int processingtime = 125;
            int stoptime = 25;
            Server server = new Server(number, processingtime);
            Client client = new Client(server);
            for (int id = 1; id <= 70; id++)
            {
                client.send(id);
                Thread.Sleep(stoptime);
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            Console.WriteLine("------------------\nРезультаты:\n");
            Analyze analyze1 = new Analyze((double)1000 / stoptime, (double)processingtime / 1000, number);
            analyze1.calculate();

            Console.ReadKey();
        }
    }

}
