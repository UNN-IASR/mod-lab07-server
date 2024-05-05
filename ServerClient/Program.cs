using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace ServerClient
{
    class Statistics
    {
        private double await_possibility;
        private double decline_possibility;
        private double relative_error;
        private double absolute_error;
        private double taken_chanels;
        public void CalculateStatistics(int poolSize, double requestRate, double serverRate)
        {
            await_possibility = CalculateP0(serverRate / requestRate, poolSize);
            decline_possibility = Math.Pow(serverRate / requestRate, poolSize) / Factorial(poolSize) * await_possibility;
            relative_error = 1 - decline_possibility;
            absolute_error = relative_error / requestRate;
            taken_chanels = absolute_error * serverRate;
        }
        private int Factorial(int n) => n == 0 ? 1 : n * Factorial(n - 1);
        private double CalculateP0(double ro, int i) => i == 0 ? 1 : 1 / (Math.Pow(ro, i) / Factorial(i) + 1 / CalculateP0(ro, i - 1));
        private double RoundToPrint(double value) => Math.Round(value, 4);
        public void Counter(Server server, double requestRate)
        {
            double activeThreads, busyThreads, passingThreads, count;
            busyThreads = passingThreads = count = 0;

            do
            {
                Thread.Sleep(50);
                activeThreads = server.GetActiveThreads();
                busyThreads += activeThreads;
                passingThreads += activeThreads == 0 ? 1 : 0;
                count++;
            } while (activeThreads > 0);

            await_possibility = passingThreads / count;
            decline_possibility = (double)server.rejectedCount / server.requestCount;
            relative_error = 1 - decline_possibility;
            absolute_error = relative_error / requestRate;
            taken_chanels = busyThreads / count;
        }
        public void PrintStatistics()
        {
            Console.WriteLine($"вероятность простоя системы: {RoundToPrint(await_possibility)}");
            Console.WriteLine($"вероятность отказа системы: {RoundToPrint(decline_possibility)}");
            Console.WriteLine($"относительная пропускная способность: {RoundToPrint(relative_error)}");
            Console.WriteLine($"абсолютная пропускная способность: {RoundToPrint(absolute_error)}");
            Console.WriteLine($"среднее число занятых каналов: {RoundToPrint(taken_chanels)}");
        }
    }
    class Program
    {
        const int POOLSIZE = 3;
        const double REQUESTRATE = 0.2;
        const double SERVERRATE = 0.7;
        public static void SimulateRequests(Client client, int totalRequests)
        {
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(Convert.ToInt32(REQUESTRATE * 1000));
            }
        }
        static void Main()
        {
            Server server = new Server(POOLSIZE, SERVERRATE);
            Client client = new Client(server);

            Statistics stat = new Statistics();

            stat.CalculateStatistics(POOLSIZE, REQUESTRATE, SERVERRATE);
            stat.PrintStatistics();

            Thread counterThread = new Thread(() =>
            {
                stat.Counter(server, REQUESTRATE);
            });

            counterThread.Start();
            SimulateRequests(client, 100);
            counterThread.Join();

            stat.PrintStatistics();
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
        private int poolSize;
        private PoolRecord[] pool;
        private object threadLock = new object();
        private int procTime;
        private int activeThreads = 0;
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public Server(int _poolSize, double _procTime)
        {
            poolSize = _poolSize;
            procTime = Convert.ToInt32(_procTime * 1000);

            pool = new PoolRecord[poolSize];
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка c номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < poolSize; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        activeThreads++;
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
            Thread.Sleep(procTime);
            for (int i = 0; i < poolSize; i++)
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                    activeThreads--;
                }
        }
        public int GetActiveThreads() => activeThreads;
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