using System;
using System.Threading;

class Program
    {
        static void Main()
        {
        	int countFlow = 5;
        	int delay = 500;

            Server server = new Server(countFlow, delay);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(50); 
            }
            Console.WriteLine("\nВсего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);


            double intensityRequestFlow = 1.00 / 0.1; // 100 миллисекенд
            double intensityServiceFlow = 1.00 / 0.5; //(500) стреднее время обслуживания запроса
            double averageNumberRequests = (double)intensityRequestFlow / intensityServiceFlow;

            int Factorial(int n)
            {
                if (n <= 1) return 1;
                return n * Factorial(n - 1);
            }

            double sum = 0;

            for (int i = 0; i <= countFlow; i++)
            {
                sum += (double)Math.Pow(averageNumberRequests, i) / Factorial(i);
            }

            double probabilitySystemDowntime = (double)1.00 / sum;
            double probabilitySystemFailure = (double)(Math.Pow(averageNumberRequests, countFlow) / Factorial(countFlow)) * probabilitySystemDowntime;
            double relativeThroughput = 1 - probabilitySystemFailure;
            double absoluteThroughput = intensityRequestFlow * relativeThroughput;
            double averageNumberChannelsOccupied = (double)absoluteThroughput / intensityServiceFlow;

            Console.WriteLine();
            Console.WriteLine($"Вероятность простоя системы: {Math.Round(probabilitySystemDowntime, 2)}");
            Console.WriteLine($"Вероятность отказа системы: {Math.Round(probabilitySystemFailure, 2)}");
            Console.WriteLine($"Относительная пропускная способность: {Math.Round(relativeThroughput, 2)}");
            Console.WriteLine($"Абсолютная пропускная способность: {Math.Round(absoluteThroughput, 2)}");
            Console.WriteLine($"Среднее число занятых каналов: {Math.Round(averageNumberChannelsOccupied, 2)}");
        }
    }

    struct PoolRecord
    {
        public Thread thread;
        public bool in_use; // в процессе исполнения
    }

    class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0; //обработанные
        public int rejectedCount = 0; //откланенные
        public int countFlow;
        public int delay;

        public Server(int countFlow, int delay)
        {   
            this.countFlow = countFlow;
            this.delay = delay;
            pool = new PoolRecord[countFlow]; 
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < countFlow; i++)
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
            for (int i = 0; i < countFlow; i++)
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