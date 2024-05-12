using System;
using System.Diagnostics;
using System.Threading;

namespace Program
{
    class Program
    {
        static void Main()
        {
            int delayServer = 500;
            int delayClient = 50;
            Server server = new Server(delayServer);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(delayClient);
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            double intensityClientFlow = 1.00 / (delayClient / 1000.00);
            double intensityServerFlow = 1.00 / (delayServer / 1000.00);
            double p=(double)intensityClientFlow/intensityServerFlow;
            double P, Pn, Q, A, K;
            double sumP = 0;
            int factorial = 1;
            for (int i = 0; i <= 5; i++)
            {
                
                sumP += (double)Math.Pow(p, i) / factorial;
                factorial *= i+1;
            }
            factorial = factorial / (5 + 1);
            P = (double)1 / sumP;
            Console.WriteLine("Вероятность простоя системы: " + P);
            Pn = (double)(Math.Pow(p, 5) / factorial) * P;
            Console.WriteLine("Вероятность отказа системы: " + Pn);
            Q = 1 - Pn;
            Console.WriteLine("Относительная пропускная способность: " + Q);
            A = intensityClientFlow * Q;
            Console.WriteLine("Абсолютная пропускная способность: " + A);
            K = A / intensityServerFlow;
            Console.WriteLine("Среднее число занятых каналов: " + K);
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
        public int delay = 0;
        public Server(int delay)
        {
            this.delay = delay;
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
            //for (int i = 1; i < 9; i++)
            //{
            Console.WriteLine("Обработка заявки: {0}", id);
            //Console.WriteLine("{0}",Thread.CurrentThread.Name);
            Thread.Sleep(delay);
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
