using System.Runtime.CompilerServices;

namespace TPProj {
    class Program {
        const int POOLSIZE = 3;
        const double REQUESTRATE = 0.2;
        const double SERVERRATE = 0.7;
        static int Factorial(int n) => n == 0 ? 1 : n * Factorial(n-1);
        static double r4(double n) => Math.Round(n, 4);
        static (int, int) pseudoPoissTime(double lambda) {
            Random rand = new Random();
            double r = rand.NextDouble();
            double temp = 0;
            int k = 0;
            while (r > temp) {
                temp += Math.Pow(lambda, k) * Math.Pow(Math.E, -lambda) / Factorial(k);
                k++;
            }
            return (k, Convert.ToInt32(1000.0 / k));
        }
        static void Main() {
            double λ = 1.0 / REQUESTRATE;
            double μ = 1.0 / SERVERRATE;
            double p = λ / μ;

            double Po = 1 / Enumerable.Range(0, POOLSIZE+1).Aggregate(0.0, (x, y) => x += Math.Pow(p, y) / Factorial(y));
            double Pn = Po * Math.Pow(p, POOLSIZE) / Factorial(POOLSIZE);
            double Q = 1 - Pn;
            double A = λ * Q;
            double k = A / μ;

            Console.WriteLine($"интенсивность поступления запросов: {r4(λ)}");
            Console.WriteLine($"интенсивность обслуживания запросов: {r4(μ)}");
            Console.WriteLine($"интенсивность потока запросов: {r4(p)}");
            Console.WriteLine();
            Console.WriteLine($"вероятность простоя системы: {r4(Po)}");
            Console.WriteLine($"вероятность отказа системы: {r4(Pn)}");
            Console.WriteLine($"относительная пропускная способность: {r4(Q)}");
            Console.WriteLine($"абсолютная пропускная способность: {r4(A)}");
            Console.WriteLine($"среднее число занятых каналов: {r4(k)}");
            Console.WriteLine();

            Server server = new Server(POOLSIZE, SERVERRATE, log: false);
            Client client = new Client(server);
            Thread counter = new Thread(new ThreadStart(ThreadCounter.Counter));
            counter.Start();
            for (int id = 0; id < 100; id++) {
                client.Send(id);
                Thread.Sleep(Convert.ToInt32(REQUESTRATE * 1000));
            }
            counter.Join();

            Console.WriteLine($"всего запросов: {r4(server.requestCount)}");
            Console.WriteLine($"принято запросов: {r4(server.processedCount)}");
            Console.WriteLine($"отклонено запросов: {r4(server.rejectedCount)}");
            Console.WriteLine();
            Console.WriteLine($"вероятность простоя системы: {r4((double)ThreadCounter.passingThreads / ThreadCounter.count)}");
            Console.WriteLine($"вероятность отказа системы: {r4((double)server.rejectedCount / server.requestCount)}");
            Console.WriteLine($"относительная пропускная способность: {r4((double)server.processedCount / server.requestCount)}");
            Console.WriteLine($"абсолютная пропускная способность: {r4(λ * server.processedCount / server.requestCount)}");
            Console.WriteLine($"среднее число занятых каналов: {r4((double)ThreadCounter.busyThreads / ThreadCounter.count)}");
            Console.WriteLine();
            
            server = new Server(POOLSIZE, SERVERRATE, log: false);
            client = new Client(server);
            counter = new Thread(new ThreadStart(ThreadCounter.Counter));
            ThreadCounter.Reset();

            List<int> times = new List<int>();
            while (times.Count < 100) {
                (int c, int time) = pseudoPoissTime(λ);
                for (int i = 0; i < c; i++)
                    times.Add(time);
            }
            times = times.OrderBy(_ => Random.Shared.Next()).ToList();
            counter.Start();
            for (int id = 0; id < times.Count; id++) {
                client.Send(id);
                Thread.Sleep(times[id]);
            }
            counter.Join();

            Console.WriteLine($"всего запросов: {r4(server.requestCount)}");
            Console.WriteLine($"принято запросов: {r4(server.processedCount)}");
            Console.WriteLine($"отклонено запросов: {r4(server.rejectedCount)}");
            Console.WriteLine();
            Console.WriteLine($"вероятность простоя системы: {r4((double)ThreadCounter.passingThreads / ThreadCounter.count)}");
            Console.WriteLine($"вероятность отказа системы: {r4((double)server.rejectedCount / server.requestCount)}");
            Console.WriteLine($"относительная пропускная способность: {r4((double)server.processedCount / server.requestCount)}");
            Console.WriteLine($"абсолютная пропускная способность: {r4(λ * server.processedCount / server.requestCount)}");
            Console.WriteLine($"среднее число занятых каналов: {r4((double)ThreadCounter.busyThreads / ThreadCounter.count)}");
        }
    }
    class Server {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public int processingTime;
        public bool logging;
        public Server(int poolSize, double serverRate, bool log = false) {
            pool = new PoolRecord[poolSize];
            processingTime = Convert.ToInt32(serverRate * 1000);
            logging = log;
        }
        public void Process(object sender, ProcEventArgs e) {
            lock(threadLock) {
                requestCount++;
                for (int i = 0; i < pool.Length; i++) {
                    if (!pool[i].in_use) {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(e.id);
                        processedCount++;
                        return;
                    }
                }
                if (logging) Console.WriteLine($"запрос {e.id} отклонен");
                rejectedCount++;
            }
        }
        public void Answer(object arg) {
            ThreadCounter.activeThreads++;
            int id = (int)arg;
            if (logging) Console.WriteLine($"запрос {id} принят");
            Thread.Sleep(processingTime);
            for (int i = 0; i < pool.Length; i++)
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;
            ThreadCounter.activeThreads--;
        } 
    }
    class Client {
        private Server server;
        public Client(Server _server) {
            server = _server;
            request += server.Process;
        }
        public void Send(int id) {
            ProcEventArgs args = new ProcEventArgs { id = id };
            if (request != null)
                request(this, args);
        }
        public event EventHandler<ProcEventArgs> request;
    }
    struct PoolRecord {
        public Thread thread;
        public bool in_use;
    }
    public class ProcEventArgs : EventArgs {
        public int id;
    }
    public static class ThreadCounter {
        public static int activeThreads = 0;
        public static int passingThreads = 0;
        public static int busyThreads = 0;
        public static int count = 0;
        public static void Reset() {
            activeThreads = 0;
            passingThreads = 0;
            busyThreads = 0;
            count = 0;
        }
        public static void Counter() {
            do {
                Thread.Sleep(50);
                busyThreads += activeThreads;
                passingThreads += activeThreads == 0 ? 1 : 0;
                count++;
            } while (activeThreads > 0);
        }
    }
}