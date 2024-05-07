using System;
using System.Diagnostics;
using System.Threading;
namespace TPProj
{
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

        public List<long> periods = new List<long>();

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
            var timer = Stopwatch.StartNew();
            int id = (int)arg;

            Console.WriteLine("Обработка заявки: {0}", id);

            Thread.Sleep(500);

            for (int i = 0; i < 5; i++)
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;

            timer.Stop();
            this.periods.Add(timer.Elapsed.Milliseconds);
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

    class Program
    {
        public static int streams = 5;
        static void Main()
        {
            Calculator1();

            Server server = new Server();
            Client client = new Client(server);

            List<long> periods = new List<long>();

            for (int id = 1; id <= 200; id++)
            {
                var timer = Stopwatch.StartNew();
                client.send(id);
                Thread.Sleep(30);
                timer.Stop();
                periods.Add(timer.Elapsed.Milliseconds);
            }

            double avr = Math.Pow(GetAverage(periods) / 1000, -1);

            double vl = streams * Math.Pow(GetAverage(server.periods) / 1000, -1);
            Console.WriteLine();
            Console.WriteLine("Requests amount: {0}", server.requestCount);
            Console.WriteLine("Processed requests: {0}", server.processedCount);
            Console.WriteLine("Rejected requests: {0}", server.rejectedCount);
            Console.WriteLine();

            TotalResultsByPprogram(avr, vl);
        }
        static double GetAverage(List<long> array)
        {
            return array.Average();
        }

        public static int GetFactorial(int n)
        {
            if (n != 0) return n * GetFactorial(n - 1);
            else return 1;
        }

        public static double CountP0(double p)
        {
            double sum = 0;

            for (int i = 0; i <= streams; i++)
            {
                sum += (double)Math.Pow(p, i) / GetFactorial(i);
            }
            return Math.Pow(sum, -1); ;
        }

        //Подсчёт Pn
        public static double CountPn(double p, double P0)
        {
            double Pn = (double)(P0 * Math.Pow(p, streams) / GetFactorial(streams));
            return Pn;
        }


        static void Calculator1()
        {
            double avr = 27;
            double vl = 10;
            double p = avr / vl;
            double P0 = CountP0(p);
            double Pn = CountPn(p, P0);
            double Q = 1 - Pn;
            double A = avr * Q;
            double k = A / vl;

            Console.WriteLine("Theory:");
            Console.WriteLine("The level of practical use: " + avr);
            Console.WriteLine("System rate: " + vl);
            Console.WriteLine("The diminished level of usage: " + p);
            Console.WriteLine("The likelihood of system interruptions: " + P0);
            Console.WriteLine("Likelihood of breakdown: " + Pn);
            Console.WriteLine("Relative throughput: " + Q);
            Console.WriteLine("Absolute throughput: " + A);
            Console.WriteLine("Mean count of active threads: " + k);
            Console.WriteLine("\n");
        }

        static void TotalResultsByPprogram(in double avr, in double vl)
        {
            double p = avr / vl;
            double P0 = CountP0(p);
            double Pn = CountPn(p, P0);
            double Q = 1 - Pn;
            double A = avr * Q;
            double k = A / vl;

            Console.WriteLine("Final results:");
            Console.WriteLine("The level of practical use: " + avr);
            Console.WriteLine("System rate: " + vl);
            Console.WriteLine("The diminished level of usage: " + p);
            Console.WriteLine("The likelihood of system interruptions: " + P0);
            Console.WriteLine("Likelihood of breakdown: " + Pn);
            Console.WriteLine("Relative throughput: " + Q);
            Console.WriteLine("Absolute throughput: " + A);
            Console.WriteLine("Mean count of active threads: " + k);
            Console.WriteLine("\n");
        }
    }
}