using System;
using System.Diagnostics;
using System.Threading;
namespace Lab07 {
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            List<int> clientDelays = new List<int>();

            int requestsCount = 100;
            int streamsCount = 5;

            int clientIntensity = 20;
            int serverIntensity = 2*streamsCount;

            const int MILLISECONDS_IN_SECOND = 1000;
            int clientDelay = MILLISECONDS_IN_SECOND / clientIntensity;
            int serverDelay = MILLISECONDS_IN_SECOND / (serverIntensity/streamsCount);
            
            Server server = new Server(serverDelay);
            Client client = new Client(server);
            for (int id = 1; id <= requestsCount; id++)
            {
                var timer = new Stopwatch();
                timer.Start();
                client.send(id);
                Thread.Sleep(clientDelay);
                timer.Stop();
                clientDelays.Add(timer.Elapsed.Milliseconds);
            }
            
            using (StreamWriter writer = new StreamWriter("../../../results.txt"))
            {
                writer.WriteLine("Всего заявок: {0}", server.requestCount);
                writer.WriteLine("Обработано заявок: {0}", server.processedCount);
                writer.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
                writer.WriteLine("\nОжидаемые результаты:");
                PrintResults(clientIntensity, serverIntensity, streamsCount, writer);
                writer.WriteLine("\nФактические результаты:");
                double realClientIntensity = MILLISECONDS_IN_SECOND / clientDelays.Average();
                double realServerIntensity = MILLISECONDS_IN_SECOND / (server.serverDelays.Average() / streamsCount);
                PrintResults(realClientIntensity, realServerIntensity, streamsCount, writer);
            }   
        }
        private static void PrintResults(double clientIntensity, double serverIntensity, int streamsCount, StreamWriter writer)
        {
            double p = (double)clientIntensity / (double)serverIntensity;

            double sum = 0;
            for (int i = 0; i < streamsCount; i++)
            {
                sum += (double)Math.Pow(p, i) / Factorial(i);
            }
            double p0 = Math.Pow(sum, -1);

            double pn = p0 == 0
                ? 0
                : p0 * Math.Pow(p, streamsCount) / Factorial(streamsCount);

            var relativeThroughput = 1 - pn;
            var absoluteThroughput = clientIntensity * relativeThroughput;
            var averageBusyThreads = absoluteThroughput / serverIntensity;

            writer.WriteLine("Интенсивность потока заявок: {0:0.###}", p);
            writer.WriteLine("Вероятность простоя системы: {0:0.###}", p0);
            writer.WriteLine("Вероятность отказа системы: {0:0.###}", pn);
            writer.WriteLine("Относительная пропускная способность: {0:0.###}", relativeThroughput);
            writer.WriteLine("Абсолютная пропускная способность: {0:0.###}", absoluteThroughput);
            writer.WriteLine("Среднее число занятых каналов: {0:0.###}", averageBusyThreads);
        }
        private static int Factorial(int n)
        {
            if (n == 0) return 1;
            return n * Factorial(n - 1);
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
        public List<int> serverDelays = new List<int>();
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public int serverDelay;
        public Server(int serverDelay)
        {
            this.pool = new PoolRecord[5];
            this.serverDelay = serverDelay;
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
            var timer = new Stopwatch();
            timer.Start();
            int id = (int)arg;
            Console.WriteLine("Обработка заявки: {0}", id);
            Thread.Sleep(serverDelay);
            for (int i = 0; i < 5; i++)
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;
            timer.Stop();
            serverDelays.Add(timer.Elapsed.Milliseconds);
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
