using System;
using System.Threading;

namespace lab7
{
    class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0; //поступившие на запрос
        public int processedCount = 0; //принятые
        public int rejectedCount = 0; //отклонённые

        public int serverPause = 0;
        public int countThread = 0;
        public Server(int serverPause, int countThread)
        {
            pool = new PoolRecord[5];
            this.serverPause = serverPause;
            this.countThread = countThread;
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < countThread; i++)
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
            Thread.Sleep(serverPause);
            for (int i = 0; i < countThread; i++)
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
        static void Main()
        {

            int mainPause = 50;
            int serverPause = 500;
            int countThread = 5;

            //многоканальное 
            Server server = new Server(serverPause, countThread);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(mainPause);
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            //лямбда - интенсивность поступления требований  // 1 / (mainPause / 1000)
            double L = (double)1.00 / ((double)mainPause / (double)1000.00);
            Console.WriteLine($"Интенсивность поступления требований: {Math.Round(L,2)}");

            //ню - интенсивность обслуживания требований  // 1 / (500 / 1000)
            double Nu = (double)1 / ((double)serverPause / (double)1000);
            Console.WriteLine($"Интенсивность обслуживания требований: {Math.Round(Nu,2)}");



            int Factorial(int i)
            {
                if (i <= 1) return 1;
                return i * Factorial(i - 1);
            }

            //P0 - вероятность простоя системы
            double Po = 0;
            for (int i = 0; i <= countThread; i++)
            {
                Po += (double)Math.Pow(L / Nu, i) / Factorial(i);
            }
            Po = (double)1 / Po;
            Console.WriteLine($"Вероятность простоя системы: {Math.Round(Po,7)}");

            //Pn - вероятность отказа системы
            double Pn = Po * ((double)Math.Pow((double)L / Nu, countThread) / Factorial(countThread));
            Console.WriteLine($"Вероятность отказа системы: {Math.Round(Pn,7)}");

            //Q - относительная пропускная способность
            double Q = 1 - Pn;
            Console.WriteLine($"Относительная пропускная способность: {Math.Round(Q,2)}");

            // A - абсолютная пропускная способность
            double A = L * Q;
            Console.WriteLine($"Абсолютная пропускная способность: {Math.Round(A,2)}");

            //k - среднее число занятых каналов
            double k = (double)A / (double)Nu;
            Console.WriteLine($"Среднее число занятых каналов: {Math.Round(k,2)}");




            Console.Read();
        }
    }
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }

}
