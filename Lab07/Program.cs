using System;
using System.Threading;

namespace Lab07
{
    class Program
    {
        static void Main()
        {
            int requestIntensity = 50; // delay between requests
            int serviceIntensity = 500; // processing time of one request by a thread
            Server server = new Server();
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id, serviceIntensity);
                Thread.Sleep(requestIntensity);
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
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
        public Server()
        {
            pool = new PoolRecord[5];
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < 5; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(e);
                        processedCount++;
                        return;
                    }
                }
                rejectedCount++;
            }
        }
        public void Answer(object argsObject)
        {
            procEventArgs args = (procEventArgs)argsObject;
            int id = (int)args.id;
            int serviceIntensity = (int)args.serviceIntensity;
            //for (int i = 1; i < 9; i++)
            //{
            Console.WriteLine("Обработка заявки: {0}", id);
            //Console.WriteLine("{0}",Thread.CurrentThread.Name);
            Thread.Sleep(serviceIntensity);
            //}
            for (int i = 0; i < 5; i++)
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
            this.request += server.proc;
        }
        public void send(int id, int serviceIntensity)
        {
            procEventArgs args = new procEventArgs();
            args.id = id;
            args.serviceIntensity = serviceIntensity;
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
        public int serviceIntensity { get; set; }
    }
}
