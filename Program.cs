using System;
using System.Threading;

namespace TPProj
{
    class Program
    {
        static int Factorial(int n)
        {
            if (n <= 1) return 1;
            return n * Factorial(n - 1);
        }
        static void Main()
        {
            int requestTime = 150;
            int serverTime = 700;
            int N = 2;
            double lambda = 1000.0 / requestTime;
            double mu = 1000.0 / serverTime;
            double ro = lambda / mu;
            double P0 = 0;
            for (int i = 0; i <= N; i++ )
            {
                P0 += Math.Pow(ro, i) / Factorial(i);
            }
            P0 = Math.Pow(P0, -1);
            double Pn = Math.Pow(ro, N) / Factorial(N) * P0;
            double Q = 1 - Pn;
            double A = lambda * Q;
            double k = A / mu;
            Console.WriteLine("Вероятность простоя системы: {0}", P0);
            Console.WriteLine("Вероятность отказа системы: {0}", Pn);
            Console.WriteLine("Относительная пропускная способность: {0}", Q);
            Console.WriteLine("Абсолютная пропускная способность: {0}", A);
            Console.WriteLine("Среднее число занятых каналов: {0}", k);
            Server server = new Server(N, serverTime);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(requestTime); 
            }
            Console.WriteLine("\nВсего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            Console.WriteLine("Вероятность отказа системы: {0}", (double)server.rejectedCount/server.requestCount);
            Console.WriteLine("Относительная пропускная способность: {0}", (double)server.processedCount/server.requestCount);
            Console.WriteLine("Абсолютная пропускная способность: {0}", (double)server.processedCount/server.requestCount * lambda);
        }
    }

    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }

    class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public int sleepTime;

        public Server(int n, int serverTime)
        {   
            pool = new PoolRecord[n];
            sleepTime = serverTime;
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                //Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < pool.Length; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(e.id);
                        processedCount++;
                        return;
                    }
                }
                rejectedCount++;
            }
        }

        public void Answer(object arg)
        {
            int id = (int)arg;
            //Console.WriteLine("Обработка заявки: {0}", id);
            Thread.Sleep(sleepTime);
            for (int i = 0; i < pool.Length; i++)
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;
        }
    }

    class Client
    {
        private Server server;
        public Client(Server server)
        {
            this.server = server;
            request += server.proc;
        }

        public void send(int id)
        {
            procEventArgs args = new procEventArgs();
            args.id = id;
            OnProc(args);
        }

        protected virtual void OnProc(procEventArgs e)
        {
            EventHandler<procEventArgs> handler = request;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<procEventArgs> request;
    }

    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }
}