using System;
using System.Threading;
namespace SMO
{
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }
    public struct PoolRecord
    {
        public Thread thread; // объект потока
        public bool in_use; // флаг занятости
    }
    public class Server
    {
        private PoolRecord[] pool;
        private object threadLock;
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        private int number = 0;
        private int processingtime = 0;
        public Server (int number, int processingtime)
        {
            pool = new PoolRecord[number];
            threadLock = new object();
            this.number = number;
            this.processingtime = processingtime;
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                //Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < number; i++)
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
            //Console.WriteLine("Обработка заявки: {0}", id);
            //Console.WriteLine("{0"}, Thread.CurrentThread.Name);
            Thread.Sleep(this.processingtime);
            for (int i = 0; i < number; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                }
            }
        }
    }
    public class Client
    {
        public Server server { get; set; }
        public event EventHandler<procEventArgs> request;

        public Client(Server server)
        {
            this.server = server;
            this.request += server.proc;
        }
        public void send (int id)
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

    }
    public class Theory
    {
        public double L { get; set; }
        public double T0 { get; set; }
        public int n { get; set; }
        private double P0;
        private double Pn;
        private double Q;
        private double A;
        private double K;
        public Theory(double L, double T0, int n)
        {
            this.L = L;
            this.T0 = T0;
            this.n = n;
        }
        public void calculate()
        {
            P0 = 0.0;
            Pn = 0.0;
            Q = 0.0;
            A = 0.0;
            K = 0.0;
            double p = L * T0;
            for (int i = 0; i <= n; i++)
            {
                P0 += Math.Pow(p,i)/(double)Factorial(i);
            }
            P0 = 1 / P0;
            Pn = P0 * Math.Pow(p, n) / (double)Factorial(n);
            Q = 1 - Pn;
            A = L * Q;
            K = A * T0;
            Console.WriteLine("Вероятность простоя сервера: {0:0.##}", P0);
            Console.WriteLine("Вероятность отказа сервера: {0:0.##}", Pn);
            Console.WriteLine("Относительная пропускная способность: {0:0.##}", Q);
            Console.WriteLine("Абсолютная пропускная способность: {0:0.##}", A);
            Console.WriteLine("Среднее число занятых каналов: {0:0.##}", K);
        }
        int Factorial(int n)
        {
            if (n == 0) return 1;
            if (n == 1) return 1;

            return n * Factorial(n - 1);
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            int number = 5;
            int processingtime = 350;
            int stoptime = 4;
            Server server = new Server(number, processingtime);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(stoptime);
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            Theory theory = new Theory((double)1000 / stoptime, (double)processingtime / 1000, number);
            theory.calculate();
        }
    }
}
