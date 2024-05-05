using System;
using System.Data.Common;
using System.IO;
using System.Threading;

namespace Lab07
{
    internal class Program
    {
        public class procEventArgs : EventArgs
        {
            public int id { get; set; }
            
        }
        public class Client
        {
            Server server;
            public event EventHandler<procEventArgs> request;
            procEventArgs arg;
            public Client(Server server)
            {
                this.server = server;
                this.request += server.proc;
                arg = new procEventArgs();
                

            }
            protected virtual void OnProc(procEventArgs e)
            {
                EventHandler<procEventArgs> handler = request;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            public void AddRequest()
            {
                arg.id++;
                OnProc(arg);
                
            }

        }
        public class Server
        {
            static int n;
            public int requestCount;
            public int processedCount;
            public int rejectedCount;
            int delay;
            PoolRecord[] pool;
            object threadLock = new object();
            public Server(int n, int delay)
            {
                Server.n = n;
                requestCount = 0;
                processedCount = 0;
                rejectedCount = 0;
                pool = new PoolRecord[n];
                this.delay = delay;
            }
            struct PoolRecord
            {
                public int id_request;
                public int id_thread;
                public Thread thread;
                public bool in_use;
            }

            public void proc(object sender, procEventArgs e)
            {
                lock (threadLock)
                {
                    Console.WriteLine("Заявка с номером: {0}", e.id);
                    requestCount++;
                    
                    for (int i = 0; i < n; i++)
                    {
                        if (!pool[i].in_use)
                        {
                            pool[i].id_request=e.id;
                            pool[i].id_thread = i;
                            pool[i].in_use = true;
                            pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                            pool[i].thread.Start(pool[i]);
                            processedCount++;
                            return;
                        }
                    }
                    rejectedCount++;
                    
                }

            }

            void Answer(object message)
            {
                PoolRecord pool1 = (PoolRecord)message;
                Thread.Sleep(delay);
                Console.WriteLine("Заявка {0} обработана", pool1.id_request);
                pool[pool1.id_thread].in_use = false;
            }

        }
        static public int Fac (int n)
        {
            if (n == 0)
                return 1;
            else
            {
                int fac = 1;
                for (int i = 1; i<=n; i++)
                    fac = fac * i;
                return fac;
            }
        }
        static void Main()
        {
            int la = 10;
            int mu = 2;
            int delay = 10;
            int threads = 3;
            int n = 10;
            Server server = new Server(threads, delay);
            Client client = new Client(server);
            
            for (int i = 0;i<n;i++) { client.AddRequest(); }
            Console.WriteLine(server.processedCount.ToString());
            Console.WriteLine(server.requestCount.ToString());
            Console.WriteLine(server.rejectedCount.ToString());
            double ro = la / mu;
            double znam = 0;
            for (int i = 0;i<=threads;i++)
            {
                znam += Math.Pow(ro, i) / Fac(i);
            }
            double P_0 = 1 / znam;
            double P_n = Math.Pow(ro, threads) * P_0 / Fac(threads);
            double Q = 1 - P_n;
            double A = la * Q;
            double k = A / mu;
            string fale_name = "results.txt";
            StreamWriter stream = File.CreateText(fale_name);
            stream.WriteLine("Теоретические расчёты \n");
            stream.WriteLine("Количество запросов {0}, из них: ", n);
            stream.WriteLine("   обработано - {0}", server.processedCount);
            stream.WriteLine("   отклонено - {0}", server.rejectedCount);
            stream.WriteLine("Интенсивность поступления требований: {0}", la);
            stream.WriteLine("Интенсивность обслуживания требований: {0}", mu);
            stream.WriteLine("Количество потоков: {0}", threads);
            stream.WriteLine("Относительная пропускная способность системы: {0}", Q);
            stream.WriteLine("Вероятность отказа: {0}", P_n);
            stream.WriteLine("Абсолютная пропускная способность системы: {0}", A);
            stream.WriteLine("Вероятность простоя системы: {0}", P_0);
            stream.WriteLine("Среднее число занятых каналов: {0}" , k);
            stream.WriteLine("_____________________________________________");
            la = server.requestCount / 1;
            mu = server.processedCount / threads;

            ro = la / mu;
            znam = 0;
            for (int i = 0; i <= threads; i++)
                znam = znam + Math.Pow(ro, i) / Fac(i);
            P_0 = 1 / znam;
            P_n = Math.Pow(ro, threads) * P_0 / Fac(threads);
            Q = 1 - P_n;
            A = la * Q;
            k = A / mu;
            stream.WriteLine("Фактические расчёты \n");
            stream.WriteLine("Количество запросов {0}, из них: ", n);
            stream.WriteLine("   обработано - {0}", server.processedCount);
            stream.WriteLine("   отклонено - {0}", server.rejectedCount);
            stream.WriteLine("Интенсивность поступления требований: {0}", la);
            stream.WriteLine("Интенсивность обслуживания требований: {0}", mu);
            stream.WriteLine("Количество потоков: {0}", threads);
            stream.WriteLine("Относительная пропускная способность системы: {0}", Q);
            stream.WriteLine("Вероятность отказа: {0}", P_n);
            stream.WriteLine("Абсолютная пропускная способность системы: {0}", A);
            stream.WriteLine("Вероятность простоя системы: {0}", P_0);
            stream.WriteLine("Среднее число занятых каналов: {0}", k);
            stream.WriteLine("_____________________________________________");
            stream.Close();
        }
    }
}
