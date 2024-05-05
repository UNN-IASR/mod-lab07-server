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
        public static int active_streams = 5;
        static void Main()
        {
            Theoretical_calculation();

            List<long> periods = new List<long>();

            Server server = new Server();
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                var timer = Stopwatch.StartNew();
                client.send(id);
                Thread.Sleep(50);
                timer.Stop();
                periods.Add(timer.Elapsed.Milliseconds);
            }

            //Интенсивность поступления требований
            double lambda = Math.Pow(Calculate_Average(periods) / 1000, -1);
            ////Интенсивность обслуживания требований
            //double mew = (double)active_streams * Math.Pow(Calculate_Average(server.periods) / 1000, -1);
            double mew = active_streams * Math.Pow(Calculate_Average(server.periods) / 1000, -1);
            Console.WriteLine();
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            Console.WriteLine();

            The_results_obtained_by_the_program(lambda, mew);
        }

        //Подсчёт среднего значения
        static double Calculate_Average(List<long> array)
        {
            return array.Average();
        }

        //Функция подсчёта факториала
        public static int Factorial_Recursive(int n)
        {
            //Рекурсивный вызов, при i != 0
            if (n != 0) return n * Factorial_Recursive(n - 1);
            //Базовый случай: факториал от 0 равен 1
            else return 1;
        }

        //Подсчёт P0
        public static double Count_P0(double p)
        {
            double sum = 0;

            for (int i = 0; i <= active_streams; i++)
            {
                sum += (double)Math.Pow(p, i) / Factorial_Recursive(i);
            }
            return Math.Pow(sum, -1); ;
        }

        //Подсчёт Pn
        public static double Count_Pn(double p, double P0)
        {
            double Pn = (double)(P0 * Math.Pow(p, active_streams) / Factorial_Recursive(active_streams));
            return Pn;
        }

        //Теоретическими рассчеты
        static void Theoretical_calculation()
        {
            //Интенсивность поступления требований
            double lambda = 16;
            //Интенсивность обслуживания требований
            double mew = 10;
            //Приведенная интенсивность потока заявок
            double p = lambda / mew;
            //Вероятность простоя системы
            double P0 = Count_P0(p);
            //Вероятность отказа системы
            double Pn = Count_Pn(p, P0);
            //Относительная пропускная способность
            double Q = 1 - Pn;
            //Абсолютная пропускная способность
            double A = lambda * Q;
            //Среднее число занятых каналов
            double k = A / mew;

            Console.WriteLine("Theoretical calculation:");
            Console.WriteLine("The intensity of applications: " + lambda);
            Console.WriteLine("Service intensity: " + mew);
            Console.WriteLine("The reduced intensity of applications: " + p);
            Console.WriteLine("The probability of downtime: " + P0);
            Console.WriteLine("Probability of failure: " + Pn);
            Console.WriteLine("Relative throughput: " + Q);
            Console.WriteLine("Absolute throughput: " + A);
            Console.WriteLine("Average number of busy threads: " + k);
            Console.WriteLine("\n");
        }

        static void The_results_obtained_by_the_program(in double lambda, in double mew)
        {
            //Приведенная интенсивность потока заявок
            double p = lambda / mew;
            //Вероятность простоя системы
            double P0 = Count_P0(p);
            //Вероятность отказа системы
            double Pn = Count_Pn(p, P0);
            //Относительная пропускная способность
            double Q = 1 - Pn;
            //Абсолютная пропускная способность
            double A = lambda * Q;
            //Среднее число занятых каналов
            double k = A / mew;

            Console.WriteLine("The results obtained by the program:");
            Console.WriteLine("The intensity of applications: " + lambda);
            Console.WriteLine("Service intensity: " + mew);
            Console.WriteLine("The reduced intensity of applications: " + p);
            Console.WriteLine("The probability of downtime: " + P0);
            Console.WriteLine("Probability of failure: " + Pn);
            Console.WriteLine("Relative throughput: " + Q);
            Console.WriteLine("Absolute throughput: " + A);
            Console.WriteLine("Average number of busy threads: " + k);
            Console.WriteLine("\n");
        }
    }
}
