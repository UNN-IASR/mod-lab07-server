using System;
using System.Threading;

namespace Program
{
    class Client
    {
        private int time;
        public event EventHandler<procEventArgs> request;
        private Server server;
        public Client(Server server, int n)
        {
            time = 1000 / n;
            this.server = server;
            request += server.proc;
        }

        public void proc(int num)
        {
            procEventArgs args = new procEventArgs();

            args.id = num;

            if (request != null)
            {
                request(this, args);
            }
        }
        
        public void Start() {
            for (int i = 1; i <= 100; i++)
            {
                this.proc(i);
                Thread.Sleep(time);
            }
        }
    }

    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }

    class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }

    class Server
    {
        private int n, time;
        private PoolRecord[] pool;
        private int processedCount = 0;
        private int requestCount = 0;
        private int rejectedCount = 0;
        private object threadLock = new object();

        public Server(int n, int t)
        {
            this.n = n;
            time = 1000 / t;
            pool = new PoolRecord[n];
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Request #{0}", e.id);
                requestCount++;
                for (int i = 0; i < n; i++)
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

            Console.WriteLine("Processing request #{0}", id);
            Thread.Sleep(time);

            for (int i = 0; i < n; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                }
            }
        }

        public int getRequestCount()
        {
            return requestCount;
        }

        public int getProcessedCount()
        {
            return processedCount;
        }

        public int getRejectedCount()
        {
            return rejectedCount;
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            int req_intensity = 20, service_intensity = 1, threads = 5;
            Server server = new Server(threads, service_intensity);
            Client client = new Client(server, req_intensity);
            client.Start();

            Console.WriteLine();
            Console.WriteLine("RESULTS:");
            Console.WriteLine("ALL: {0}", server.getRequestCount());
            Console.WriteLine("PROCESSED: {0}", server.getProcessedCount());
            Console.WriteLine("REJECTED: {0}", server.getRejectedCount());
        }
    }
}
