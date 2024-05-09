using System.Diagnostics;
namespace TPProj
{
    class Program
    {
        public static int countStreams = 5;

        public static void Main()
        {
            Theoretical();

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

            Console.WriteLine();
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            Console.WriteLine();

            double lambda = Math.Pow(periods.Average() / 1000, -1);
            double mew = countStreams * Math.Pow(server.periods.Average() / 1000, -1);
            Actual(lambda, mew);
        }

        public static int Factorial(int n)
        {
            int factorial = 1;
            for (int i = 1; i <= n; i++) {
                factorial *= i;
            }
            return factorial;
        }

        public static double ProbabilityOfDowntime(double p)
        {
            double sum = 0;
            for (int i = 0; i <= countStreams; i++)
            {
                sum += (double)Math.Pow(p, i) / Factorial(i);
            }
            return Math.Pow(sum, -1); ;
        }

        public static double ProbabilityOfFailure(double p, double P0)
        {
            return (double)(P0 * Math.Pow(p, countStreams) / Factorial(countStreams));
        }

        public static void Theoretical()
        {
            double lambda = 16;
            double mew = 10;
            double p = lambda / mew;
            double P0 = ProbabilityOfDowntime(p);
            double Pn = ProbabilityOfFailure(p, P0);
            double Q = 1 - Pn;
            double A = lambda * Q;
            double k = A / mew;

            Console.WriteLine("Теоретические рассчеты:");
            Console.WriteLine("Интенсивность поступления требований: {0:0.0000}", lambda);
            Console.WriteLine("Интенсивность обслуживания требований: {0:0.0000}", mew);
            Console.WriteLine("Приведенная интенсивность потока заявок: {0:0.0000}", p);
            Console.WriteLine("Вероятность простоя системы: {0:0.0000}", P0);
            Console.WriteLine("Вероятность отказа системы: {0:0.0000}", Pn);
            Console.WriteLine("Относительная пропускная способность: {0:0.0000}", Q);
            Console.WriteLine("Абсолютная пропускная способность: {0:0.0000}", A);
            Console.WriteLine("Среднее число занятых каналов: {0:0.0000}", k);
            Console.WriteLine("\n");
        }

        public static void Actual(in double lambda, in double mew)
        {
            double p = lambda / mew;
            double P0 = ProbabilityOfDowntime(p);
            double Pn = ProbabilityOfFailure(p, P0);
            double Q = 1 - Pn;
            double A = lambda * Q;
            double k = A / mew;

            Console.WriteLine("Практические результаты:");
            Console.WriteLine("Интенсивность поступления требований: {0:0.0000}", lambda);
            Console.WriteLine("Интенсивность обслуживания требований: {0:0.0000}", mew);
            Console.WriteLine("Приведенная интенсивность потока заявок: {0:0.0000}", p);
            Console.WriteLine("Вероятность простоя системы: {0:0.0000}", P0);
            Console.WriteLine("Вероятность отказа системы: {0:0.0000}", Pn);
            Console.WriteLine("Относительная пропускная способность: {0:0.0000}", Q);
            Console.WriteLine("Абсолютная пропускная способность: {0:0.0000}", A);
            Console.WriteLine("Среднее число занятых каналов: {0:0.0000}", k);
            Console.WriteLine("\n");
        }
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

    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
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