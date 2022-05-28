using System;
using System.Threading;

namespace Lab7
{
    public class Server
    {
        public readonly int threadNumber;
        public readonly int serviceTime;
        public int requestCount = 0;
        public int servicedCount = 0;
        public int rejectedCount = 0;
        PoolRecord[] pool;
        object threadLock = new object();

        public Server(int number, int time)
        {
            threadNumber = number;
            serviceTime = time;
            pool = new PoolRecord[number];
        }

        public void Proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < threadNumber; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(e.id);
                        servicedCount++;
                        return;
                    }
                }
                rejectedCount++;
            }
        }

        public void Answer(object obj)
        {
            int client_id = (int)obj;
            Console.WriteLine("Обработка заявки: {0}", client_id);
            Thread.Sleep(100);
            for (int i = 0; i < threadNumber; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                }
            }
        }
    }
}
