using System;
using System.Threading;

namespace client_server
{
    struct PoolRecord
    {
        public Thread thread; // объект потока
        public bool in_use; // флаг занятости
    }

    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }

    class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        int n = 2;

        public Server()
        {
            pool = new PoolRecord[n];
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
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
            Console.WriteLine("Обработка заявки: {0}", id);
            Thread.Sleep(100);
            for (int i = 0; i < n; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                }
            }
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

        public void send(int id)
        {
            procEventArgs args = new procEventArgs();
            args.id = id;
            OnProc(args);
        }

        protected virtual void OnProc(procEventArgs e)
        {
            request?.Invoke(this, e);
        }
        public event EventHandler<procEventArgs> request;
    }


    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Client client = new Client(server);

            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(5);
            }

            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Откланено заявок: {0}", server.rejectedCount);
        }
    }
}

