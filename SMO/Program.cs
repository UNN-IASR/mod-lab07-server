using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace TPProj
{
    class Monitoring
    {
        private double P0;
        private double Pn;
        private double Q;
        private double A;
        private double k;
        public void CollectEvidence(int numOfWorkers, double requestRate, double serverRate)
        {
            double p = serverRate / requestRate;
            P0 = CalculateP0(p, numOfWorkers);
            Pn = Math.Pow(p, numOfWorkers) / Factorial(numOfWorkers) * P0;
            Q = 1 - Pn;
            A = Q / requestRate;
            k = A * serverRate;
        }
        private int Factorial(int n) => n == 0 ? 1 : n * Factorial(n-1);
        private double CalculateP0(double ro, int i) => i == 0 ? 1 : 1 / (Math.Pow(ro, i) / Factorial(i) + 1 / CalculateP0(ro, i - 1));
        private double RoundToPrint(double value) => Math.Round(value, 4);
        public void Counter(Server server, double requestRate)
        {
            double activeThreads, busyThreads, passingThreads, count;
            busyThreads = passingThreads = count = 0;

            do {
            Thread.Sleep(50);
            activeThreads = server.GetRunningThreads();
            busyThreads += activeThreads;
            passingThreads += activeThreads == 0 ? 1 : 0;
            count++;
            } while (activeThreads > 0);

            P0 = passingThreads / count;
            Pn = (double)server.rejectedCount / server.requestCount;
            Q = 1 - Pn;
            A = Q / requestRate;
            k = busyThreads / count;
        }
        public void Log()
        {
            Console.WriteLine($"Вероятность простоя системы: {RoundToPrint(P0)}");
            Console.WriteLine($"Вероятность отказа системы: {RoundToPrint(Pn)}");
            Console.WriteLine($"Относительная пропускная способность: {RoundToPrint(Q)}");
            Console.WriteLine($"Абсолютная пропускная способность: {RoundToPrint(A)}");
            Console.WriteLine($"Среднее число занятых каналов: {RoundToPrint(k)}");
        }
    }
    class Program
    {
        const int numOfWorkers = 3;
        const double REQUESTRATE = 0.2;
        const double SERVERRATE = 0.7;
        static void Main()
        {
            Server server = new Server(numOfWorkers, SERVERRATE);
            Client client= new Client(server);

            Monitoring exporters = new Monitoring();

            exporters.CollectEvidence(numOfWorkers, REQUESTRATE, SERVERRATE);
            exporters.Log();

            Thread counterThread = new Thread(() =>
            {
                exporters.Counter(server, REQUESTRATE);
            });

            counterThread.Start();
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(Convert.ToInt32(REQUESTRATE * 1000));
            }
            counterThread.Join();

            exporters.Log();
            Console.WriteLine("Bceгo заявок: {0}", server.requestCount);
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
        private int numOfWorkers;
        private PoolRecord[] pool;
        private object threadLock = new object();
        private int sleepTime;
        private int runningThread = 0;
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public Server(int _poolSize, double _procTime)
        {
            numOfWorkers = _poolSize;
            sleepTime = Convert.ToInt32(_procTime * 1000);

            pool = new PoolRecord[numOfWorkers];
        }        
        public void proc(object sender, procEventArgs e)
        {
            lock(threadLock)
            {
                Console.WriteLine("Заявка c номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < numOfWorkers; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        runningThread++;
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
            Thread.Sleep(sleepTime);
            for (int i = 0; i < numOfWorkers; i++) 
                if(pool[i].thread==Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                    runningThread--;
                }
        }
        public int GetRunningThreads() => runningThread;
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