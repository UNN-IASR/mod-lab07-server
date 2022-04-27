using System;
using System.Threading;

namespace lab7mipis
{
    public class Server
    {
        public static int thread_number;
        public static int proc_time;
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        PoolRecord[] pool;
        object threadLock = new object();

        public Server(int n, int t)
        {
            thread_number = n;
            proc_time = t;
            pool = new PoolRecord[n];
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < thread_number; i++)
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

        public void Answer(object obj)
        {
            int client_id = (int)obj;
            Console.WriteLine("Client ID request is " + client_id);
            Thread.Sleep(proc_time);
            for (int i = 0; i < thread_number; ++i)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                }
            }
        }
    }
}