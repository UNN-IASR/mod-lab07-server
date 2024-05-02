using System;
using System.Threading;
using static System.Math;
namespace mod_lab07_server
{
    class Program
    {
        static int active_potocs = 6;
        static int waitingtimeclients = 50;// говорим о милиссикундах
        static int waitingServer = 500;
        static void Main()
        {
            Server server = new(active_potocs, waitingServer);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(waitingtimeclients);
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            double lymbda = 1.00 / (waitingtimeclients/ 1000.00);//интенсивностью поступления требований
            double nu = 1.00 / (waitingServer / 1000.00);//интенсивностью обслуживания требований
            //float Q = nu / (lymbda + nu);//Относительная пропускная способность системы
            //double P_0 =lymbda/ (lymbda + nu);// Вероятность отказа
            /*float A = lymbda * Q;*///Абсолютная пропускная способность системы
            double p = lymbda / nu  ;// интенсивность потока заявок
            double p_0 = P0(p, active_potocs);//Вероятность простоя системы
            p_0 =(double)Math.Pow(p_0, -1);
            double promegutocnoe = (double)(Math.Pow(p, active_potocs) / factorial(active_potocs));
            double P_n = (double)promegutocnoe * p_0;//Вероятность отказа системы
            double Q = 1 - P_n;//Относительная пропускная способность
            double A = lymbda * Q;//Абсолютная пропускная способность
            double medium_count_canals = A / nu;// Среднее число занятых каналов
            Console.WriteLine("Вероятность простоя системы: {0}", Math.Round(p_0, 2));
            Console.WriteLine("Вероятность отказа системы: {0}", Math.Round(P_n, 2));
            Console.WriteLine("Относительная пропускная способность: {0}", Math.Round(Q, 2));
            Console.WriteLine("Абсолютная пропускная способность: {0}", Math.Round(A,2));


            Console.WriteLine("Среднее число занятых каналов: {0}", Math.Round(medium_count_canals, 2));
            string path = "results.txt";
            StreamWriter stream = new StreamWriter(path);
            stream.Write(    "|__________________________________|\n");
            stream.WriteLine("|Результаты полученные программой  |");
            stream.Write(    "|__________________________________|\n");
            stream.WriteLine("|Вероятность простоя системы: {0}  ", Math.Round(p_0, 2));
            stream.WriteLine("|Вероятность отказа системы: {0}    ", Math.Round(P_n, 2));
            stream.WriteLine("|Относительная пропускная способность: {0}", Math.Round(Q, 2));
            stream.WriteLine("|Абсолютная пропускная способность: {0}", Math.Round(A, 2));
            stream.WriteLine("|Среднее число занятых каналов: {0}", Math.Round(medium_count_canals, 2));
            stream.Write("|__________________________________|\n");
            stream.Write("   |__________________________________|\n\n\n");
            stream.Write(   " |_________________________________|\n");
            stream.WriteLine("|Результаты полученные вручную    |");
            stream.Write("    |_________________________________|\n");
            stream.WriteLine("|Вероятность простоя системы:   ");
            stream.WriteLine("|Вероятность отказа системы:     ");
            stream.WriteLine("|Относительная пропускная способность:");
            stream.WriteLine("|Абсолютная пропускная способность: ");
            stream.WriteLine("Среднее число занятых каналов:  ", Math.Round(medium_count_canals, 2));
            stream.Write("|__________________________________|\n");
            stream.Write("|__________________________________|\n\n\n");
            stream.Close();




        }
        public static double  P0(double p, int count_canals)
        {
            float p_0 = 0;
            int i = 0;
            while (i <= count_canals)
            {
                p_0 += (float)(Math.Pow(p, i) / factorial(i) );
                i++;
            }
            return p_0; 
        }
        public static int factorial(int i)
        {
            if (i == 0) return 1;
            return i * factorial(i - 1);


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
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public int count_potok=0;
        int waiting = 0;
        public Server(int count_potok, int waitingServer)
        {
            this.count_potok = count_potok;
            this.waiting = waitingServer;
            pool = new PoolRecord[count_potok];
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < count_potok; i++)
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
            //for (int i = 1; i < 9; i++)
            //{
            Console.WriteLine("Обработка заявки: {0}", id);
            //Console.WriteLine("{0}",Thread.CurrentThread.Name);
            Thread.Sleep(waiting);
            //}
            for (int i = 0; i < 5; i++)
            {
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;
            }
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