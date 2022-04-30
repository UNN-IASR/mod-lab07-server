using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace lab07_server
{
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
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

        private int procTime, threads;

        public int requestCount;
        public int processedCount;
        public int rejectedCount;

        public Server(int threads, int procTime)
        {
            pool = new PoolRecord[threads];
            this.procTime = procTime;
            this.threads = threads;
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id + 1);
                requestCount++;
                for (int i = 0; i < threads; i++)
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
            Thread.Sleep(this.procTime);
            for (int i = 0; i < threads; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                }
            }
        }
    }
}
