using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
namespace TPProj{
    class Program
    {
        const int n = 5;
        static double factorial(int num) {
            double fact = 1;
            for (int j = 1; j <= num; j++) { fact *= j; }
            return fact;
        }

        static double mid_arifm(in long[] arr) {
            int len = 0;
            long sum = 0;
            foreach (long num in arr) {
                sum += num;
                len++;
            }
            return (double)sum / len;
        }

        static void Test_manual() {
            //inputted manually
            int lamb = 20;
            int myu = 10;
            double ro = (double)lamb / myu;
            double sum=0;
            for (int i = 0; i <= n; i++) { sum += (double)Math.Pow(ro, i) / factorial(i); }
            double P0 = Math.Pow(sum, -1);
            double Pn = P0 * Math.Pow(ro, n) / factorial(n);
            double Q = 1 - Pn;
            double A = Q * lamb;
            double k = A / myu;

            Console.WriteLine("Результат теоретического расчета : ");
            Console.WriteLine("  Количество каналов : " + n);
            Console.WriteLine("  Интенсивность заявок : " + lamb);
            Console.WriteLine("  Интенсивность обслуживания : " + myu);
            Console.WriteLine("  Приведенная интенсивность заявок : " + ro);
            Console.WriteLine("  Вероятность простоя : " + P0);
            Console.WriteLine("  Вероятность отказа : " + Pn);
            Console.WriteLine("  Относительная пропускная способность : " + Q);
            Console.WriteLine("  Абсолютная пропускная способность : " + A);
            Console.WriteLine("  Среднее число занятых каналов : " + k);
            Console.WriteLine();
        }

        static void Test_machine(in double lamb, in double myu)
        {
            double ro = (double)lamb / myu;
            double sum = 0;
            for (int i = 0; i <= n; i++) { sum += (double)Math.Pow(ro, i) / factorial(i); }
            double P0 = Math.Pow(sum, -1);
            double Pn = P0 * Math.Pow(ro, n) / factorial(n);
            double Q = 1 - Pn;
            double A = Q * lamb;
            double k = A / myu;

            Console.WriteLine("  Количество каналов : " + n);
            Console.WriteLine("  Интенсивность заявок : " + lamb);
            Console.WriteLine("  Интенсивность обслуживания : " + myu);
            Console.WriteLine("  Приведенная интенсивность заявок : " + ro);
            Console.WriteLine("  Вероятность простоя : " + P0);
            Console.WriteLine("  Вероятность отказа : " + Pn);
            Console.WriteLine("  Относительная пропускная способность : " + Q);
            Console.WriteLine("  Абсолютная пропускная способность : " + A);
            Console.WriteLine("  Среднее число занятых каналов : " + k);

        }

        static void Main()
        {

            Test_manual();

            long[] times = new long[100];
            Server server = new Server();
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                var watch = Stopwatch.StartNew();
                client.send(id);
                Thread.Sleep(50);
                watch.Stop();
                times[id - 1] = watch.Elapsed.Milliseconds;
            }

            double lamb = Math.Pow(mid_arifm(times) / 1000, -1);
            double myu = (double)n * Math.Pow(mid_arifm(server.times) / 1000, -1);

            Console.WriteLine("\nРезультат работы программы : ");
            Console.WriteLine(" Всего заявок: {0}", server.requestCount);
            Console.WriteLine(" Обработано заявок: {0}", server.processedCount);
            Console.WriteLine(" Отклонено заявок: {0}", server.rejectedCount);
            Console.WriteLine();

            Test_machine(lamb, myu);
        }
    }
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }
    class Server
    {
        public long[] times = new long[0];
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
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
            var watch = Stopwatch.StartNew();

            int id = (int)arg;

            Console.WriteLine("Обработка заявки: {0}", id);

            Thread.Sleep(500);

            for (int i = 0; i < 5; i++)
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;

            watch.Stop();
            Array.Resize(ref times, times.Length + 1);
            this.times[this.times.Length - 1] = watch.Elapsed.Milliseconds;
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
