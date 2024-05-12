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
        static void Counter(object? obj)
        {
            if (obj is null) return;
            var (server, ct) = ((Server, CancellationToken))obj;
            double inUse = 0;
            double idle = 0;
            double c = 0;
            while (!ct.IsCancellationRequested)
            {
                inUse += server.inUse;
                if (server.inUse == 0)
                    idle++;
                c++;
                Thread.Sleep(1);
            }
            Console.WriteLine($"Вероятность простоя системы: {idle/c}");
            Console.WriteLine($"Среднее число занятых каналов: {inUse/c}");
        }
        static void Main()
        {
            double requestsSec = 115;
            double serverSec = 15;
            int threadsNum = 3;

            double ro = (double)requestsSec / serverSec;
            double P0 = 0;
            for (int i = 0; i <= threadsNum; i++ )
            {
                P0 += Math.Pow(ro, i) / Factorial(i);
            }
            P0 = 1 / P0;
            double Pn = Math.Pow(ro, threadsNum) / Factorial(threadsNum) * P0;
            double Q = 1 - Pn;
            double A = requestsSec * Q;
            double k = A / serverSec;

            Console.WriteLine($"Вероятность простоя системы: {P0}");
            Console.WriteLine($"Вероятность отказа системы: {Pn}");
            Console.WriteLine($"Относительная пропускная способность: {Q}");
            Console.WriteLine($"Абсолютная пропускная способность: {A}");
            Console.WriteLine($"Среднее число занятых каналов: {k}");

            Server server = new Server(threadsNum, (int)(1000.0 / serverSec));
            Client client = new Client(server);

            var cts = new CancellationTokenSource();
            var data = (Server: server, Token: cts.Token);
            Thread t = new Thread(new ParameterizedThreadStart(Counter));
            t.Start(data);
            for (int id = 1; id <= 500; id++)
            {
                client.send(id);
                Thread.Sleep((int)(1000.0 / requestsSec)); 
            }
            
            Console.WriteLine($"\nВсего заявок: {server.requestCount}");
            Console.WriteLine($"Обработано заявок: {server.processedCount}");
            Console.WriteLine($"Отклонено заявок: {server.rejectedCount}");
            cts.Cancel();
            Thread.Sleep(10);
            Console.WriteLine($"Вероятность отказа системы: {(double)server.rejectedCount/server.requestCount}");
            Console.WriteLine($"Относительная пропускная способность: {(double)server.processedCount/server.requestCount}");
            Console.WriteLine($"Абсолютная пропускная способность: {(double)server.processedCount/server.requestCount * requestsSec}");
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
        public int inUse
        {
            get
            {
                int res = 0;
                for (int i = 0; i < pool.Length; i++)
                    if (pool[i].in_use)
                        res++;
                return res;
            }
        }
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