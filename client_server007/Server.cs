using System;
using System.Threading;

namespace client_server007
{
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }

    internal class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }

    internal class Server
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
            time = t;
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
}