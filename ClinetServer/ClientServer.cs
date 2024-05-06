using System;
using System.Threading;

namespace TPProj
{
    class Program
    {
        static void Main()
        {
            double T0 = 0.7;
            const int NUMBER_OF_THREADS = 4;
            const int REQUEST_INTENSITIVE = 8;
            var calculations = CalculateByFormulas(REQUEST_INTENSITIVE, T0, NUMBER_OF_THREADS);


            int delayTime = (int)(T0 * 1000);
            // int poolSize = NUMBER_OF_THREADS;

            PrintCalculations("Теоритические данные:\n", REQUEST_INTENSITIVE, T0, NUMBER_OF_THREADS, calculations);


            Server server = new Server(NUMBER_OF_THREADS, delayTime);
            Client client = new Client(server);
            for (int id = 1; id <= 30 * NUMBER_OF_THREADS; id++)
            {
                client.Send(id);
                Thread.Sleep((int)(1000 / REQUEST_INTENSITIVE));
            }

            var results = CalculateByResults(server, REQUEST_INTENSITIVE, T0, NUMBER_OF_THREADS);
            PrintCalculations("\n\nПрактические данные:\n", REQUEST_INTENSITIVE, T0, NUMBER_OF_THREADS, results);
        }

        static (double P0, double Pn, double Q, double A, double k) CalculateByFormulas(double REQUEST_INTENSITIVE, double T0, int NUMBER_OF_THREADS)
        {
            double ro = REQUEST_INTENSITIVE * T0;
            double temp = 0;
            for (int i = 0; i <= NUMBER_OF_THREADS; i++)
                temp += Math.Pow(ro, i) / Fact(i);

            double P0 = 1 / temp;
            double Pn = Math.Pow(ro, NUMBER_OF_THREADS) / Fact(NUMBER_OF_THREADS) * P0;
            double Q = 1 - Pn;
            double A = REQUEST_INTENSITIVE * Q;
            double k = A * T0;

            return (P0, Pn, Q, A, k);
        }

        static void PrintCalculations(string title, double REQUEST_INTENSITIVE, double T0, int n, (double P0, double Pn, double Q, double A, double k) calculations)
        {
            Console.WriteLine(title);
            Console.WriteLine($"Интенсивность потока запросов:    {REQUEST_INTENSITIVE}");
            Console.WriteLine($"Интенсивность обслуживания:      {T0}");
            Console.WriteLine($"Количество потоков:              {n}");
            Console.WriteLine($"Вероятность простоя сервера:          {Math.Round(calculations.P0, 2)}");
            Console.WriteLine($"Вероятность отказа сервера:           {Math.Round(calculations.Pn, 2)}");
            Console.WriteLine($"Относительная пропускная способность:  {Math.Round(calculations.Q, 2)}");
            Console.WriteLine($"Абсолютная пропускная способность:    {Math.Round(calculations.A, 2)}");
            Console.WriteLine($"Среднее число занятых процессов:      {Math.Round(calculations.k, 2)}");
        }

        static (double P0, double Pn, double Q, double A, double k) CalculateByResults(Server server, double REQUEST_INTENSITIVE, double T0, int n)
        {
            double ro = (double)server.RequestCount / (double)server.ProcessedCount * n;
            double temp = 0;
            for (int i = 0; i <= n; i++)
                temp += Math.Pow(ro, i) / Fact(i);

            double P0 = 1 / temp;
            double Pn = Math.Pow(ro, n) * P0 / Fact(n);
            double Q = 1 - Pn;
            double A = REQUEST_INTENSITIVE * Q;
            double k = A * T0;

            return (P0, Pn, Q, A, k);
        }

        static int Fact(int n)
        {
            if (n == 0)
                return 1;
            else
                return n * Fact(n - 1);
        }
    }

    class Server
    {
        private readonly PoolRecord[] _pool;
        private readonly object _threadLock = new object();
        public int RequestCount { get; private set; } = 0;
        public int ProcessedCount { get; private set; } = 0;
        public int RejectedCount { get; private set; } = 0;
        public int DelayTime { get; }
        private readonly int _poolSize;

        public Server(int s, int time)
        {
            _pool = new PoolRecord[s];
            DelayTime = time;
            _poolSize = s;
        }
        public void Process(object sender, ProcEventArgs e)
        {
            lock (_threadLock)
            {
                RequestCount++;
                for (int i = 0; i < _poolSize; i++)
                {
                    if (!_pool[i].InUse)
                    {
                        _pool[i].InUse = true;
                        _pool[i].Thread = new Thread(new ParameterizedThreadStart(Answer));
                        _pool[i].Thread.Start(e.Id);
                        ProcessedCount++;
                        return;
                    }
                }
                RejectedCount++;
            }
        }
        public void Answer(object arg)
        {
            int id = (int)arg;
            Thread.Sleep(DelayTime);
            for (int i = 0; i < _poolSize; i++)
                if (_pool[i].Thread == Thread.CurrentThread)
                    _pool[i].InUse = false;
        }
    }

    class Client
    {
        private readonly Server _server;
        public Client(Server server)
        {
            _server = server;
            Request += server.Process;
        }
        public void Send(int id)
        {
            var args = new ProcEventArgs { Id = id };
            OnProc(args);
        }
        protected virtual void OnProc(ProcEventArgs e)
        {
            EventHandler<ProcEventArgs> handler = Request;
            handler?.Invoke(this, e);
        }
        public event EventHandler<ProcEventArgs> Request;
    }

    public class ProcEventArgs : EventArgs
    {
        public int Id { get; set; }
    }

    struct PoolRecord
    {
        public Thread Thread;
        public bool InUse;
    }
}
