using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab
{
    internal class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }

    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }

    internal class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();

        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public int p;
        public int zaderzhka;

        public Server(int i, int z)
        {
            p = i;
            zaderzhka = z;
            pool = new PoolRecord[p];
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < p; i++)
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

            Console.WriteLine("Обработка заявки: {0}", id);
            Thread.Sleep(zaderzhka);
            for (int i = 0; i < p; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                }
            }
        }
    }
}
