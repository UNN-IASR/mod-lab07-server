using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading;
namespace TPProj{
    class Program
    {
        static int factorial(int n){
            if (n == 0)
                return 1;
            return n*factorial(n-1);
        }
        static void Main()
        {
            double applications_intensity = 15;
            double service_intensity = 1;
            int working_time = 1000;
            int capacity = 5;
            Server server = new Server(service_intensity, working_time, capacity);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(Convert.ToInt32(Math.Round(working_time/applications_intensity)));
            }

            double ro = applications_intensity / service_intensity;
            double some_sum = 0;
            for (int i = 0; i <= capacity; i++)
                some_sum += Math.Pow(ro, i) / factorial(i);
            double P0 = 1 / some_sum;
            double Pn = Math.Pow(ro, capacity) / factorial(capacity) * P0;


            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            Console.WriteLine("Временной шаг: {0}", working_time);
            Console.WriteLine("Интенсивность потока заявок: {0}", applications_intensity);
            Console.WriteLine("Интенсивность потока обслуживаний: {0}", service_intensity);

            Console.WriteLine("Расчет по формулам:");

            Console.WriteLine("Вероятность простоя системы: {0}", Math.Round(P0, 4));
            Console.WriteLine("Вероятность отказа системы: {0}", Math.Round(Pn, 4));
            Console.WriteLine("Относительная пропускная способность: {0}", Math.Round(1 - Pn, 4));
            Console.WriteLine("Абсолютная пропускная способность: {0}", Math.Round(applications_intensity * (1 - Pn), 4));
            Console.WriteLine("Среднее число занятых каналов: {0}", Math.Round(applications_intensity * (1 - Pn) / service_intensity, 4));
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

        private int working_time = 0;
        private double service_intensity = 0.0;
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public Server()
        {
            pool = new PoolRecord[5];
        }

        public Server(double service_intensity, int working_time=500, int capacity=5){
            pool = new PoolRecord[capacity];
            this.service_intensity = service_intensity;
            this.working_time = working_time;
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                //Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < pool.Length; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
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
            //for (int i = 1; i < 9; i++)
            //{
                //Console.WriteLine("Обработка заявки: {0}", id);
                //Console.WriteLine("{0}",Thread.CurrentThread.Name);
            Thread.Sleep(Convert.ToInt32(Math.Round(working_time/service_intensity)));
            //}
            for (int i = 0; i < pool.Length; i++)
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
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            this.request += server.proc;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
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