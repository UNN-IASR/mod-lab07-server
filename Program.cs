using System;
using System.Threading;

namespace mod_lab07_server
{
    class Program
    {
        static void Main()
        {
            int serverFlow = 5;
            int clientDelay = 50; // milliseconds
            int numberRequests = 100;
            int serverDelay = 500; // milliseconds

            Server server = new Server(serverFlow, serverDelay);
            Client client = new Client(server);
            for (int id = 1; id <= numberRequests; id++)
            {
                client.send(id);
                Thread.Sleep(clientDelay); // intensity of the request flow
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            // Statistics

            double intensityClientFlow = 1.00 / (clientDelay / 1000.00);
            double intensityServerFlow = 1.00 / (serverDelay / 1000.00);
            double averageRequestsPerServiceTime = (double)intensityClientFlow * intensityServerFlow;

            int Factorial(int n)
            {
                if (n <= 1) return 1;
                return n * Factorial(n - 1);
            }

            void WriteLine(string s, double n)
            {
                if (Double.IsNaN(n))

                    Console.WriteLine($"{s}: нет");
                else
                    Console.WriteLine($"{s}: {Math.Round(n, 2)}");
            }

            double sum = 0;

            for (int i = 0; i <= serverFlow; i++)
            {
                sum += (double)Math.Pow(averageRequestsPerServiceTime, i) / Factorial(i);
            }

            double probabilitySystemDowntime = (double)1.00 / sum;
            double probabilitySystemFailure = (double)(Math.Pow(averageRequestsPerServiceTime, serverFlow) / Factorial(serverFlow)) * probabilitySystemDowntime;
            double relativeThroughput = 1 - probabilitySystemFailure;
            double absoluteThroughput = intensityClientFlow * relativeThroughput;
            double averageNumberChannelsOccupied = (double)absoluteThroughput * intensityServerFlow;

            Console.WriteLine();
            WriteLine("Вероятность простоя системы", probabilitySystemDowntime);
            WriteLine("Вероятность отказа системы", probabilitySystemFailure);
            WriteLine("Относительная пропускная способность", relativeThroughput);
            WriteLine("Абсолютная пропускная способность", absoluteThroughput);
            WriteLine("Среднее число занятых каналов", averageNumberChannelsOccupied);
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
        public int serverFlow = 0;
        public int delay = 0;
        public Server(int serverFlow, int delay)
        {   
            this.serverFlow = serverFlow;
            this.delay = delay;
            pool = new PoolRecord[serverFlow]; 
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < serverFlow; i++) // the intensity of the service flow
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
            Thread.Sleep(delay);
            for (int i = 0; i < serverFlow; i++)
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
            request += server.proc;
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
