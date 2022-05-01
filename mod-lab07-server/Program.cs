using System;
using System.Threading;

namespace Project
{
    public static class Parameters
    {
        public const double requestFreq = 10;
        public const double responseFreq = 0.28;
        public const int ClientRequestInterval  = (int)(1000.0 / requestFreq);
        public const int ServerResponseInterval = (int)(1000.0 / responseFreq);

        public const int TotalThreads = 5;
    }

    public class Program
    {
        static void Main()
        {
            Server server = new Server();
            Client client = new Client(server);

            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(Parameters.ClientRequestInterval);
            }

            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            double yield_request_intensity = Parameters.requestFreq / Parameters.responseFreq;

            double pIdle = 1;

            double pDenial_of_service = 0;

            {
                double acc = 0;
                double fact = 1; // factorial

                for (int i = 1; i <= Parameters.TotalThreads; i++)
                {
                    if (i != 0)
                        fact *= i;
                    acc += Math.Pow(yield_request_intensity, i) / fact;
                }

                pIdle /= acc;

                pDenial_of_service = (Math.Pow(yield_request_intensity, Parameters.TotalThreads) / fact) * pIdle;
            }

            double relative_throughput = 1 - pDenial_of_service;
            double absolute_throughput = Parameters.requestFreq * relative_throughput;
            double mean_busy_threads = absolute_throughput / Parameters.responseFreq;

            /*
                вероятность простоя системы
                вероятность отказа системы
                относительная пропускная способность
                абсолютная пропускная способность
                среднее число занятых каналов
             */

            Console.WriteLine("Вероятность простоя системы:          {0}", pIdle);
            Console.WriteLine("Вероятность отказа системы:           {0}", pDenial_of_service);
            Console.WriteLine("Относительная пропускная способность: {0}", relative_throughput);
            Console.WriteLine("Абсолютная пропускная способность:    {0}", absolute_throughput);
            Console.WriteLine("Cреднее число занятых каналов:        {0}", mean_busy_threads);
        }
    }

    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }

    public class Server
    {
        private const int NTHREADS = Parameters.TotalThreads;

        private PoolRecord[] pool;
        private object threadLock = new object();

        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;

        public Server() 
        {
            pool = new PoolRecord[NTHREADS];
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < NTHREADS; i++)
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
            
            Thread.Sleep(Parameters.ServerResponseInterval);
            for (int i = 0; i < NTHREADS; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;
            }
        }
    }

    public class Client
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
