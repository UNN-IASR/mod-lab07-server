using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace _07_Server
{
    public class Server
    {
        private PoolRecord[] pool;
        private object threadLock;
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        private int number = 0;
        private int processingtime = 0;
        public Server(int number, int processingtime)
        {
            pool = new PoolRecord[number];
            threadLock = new object();
            this.number = number;
            this.processingtime = processingtime;
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                //Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < number; i++)
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
            //Console.WriteLine("{0"}, Thread.CurrentThread.Name);
            Thread.Sleep(this.processingtime);
            for (int i = 0; i < number; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                }
            }
        }
    }
}
