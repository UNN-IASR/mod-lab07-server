using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TPProj
{
    public class Teoretic
    {
        public void Estimation(int n, double lambda)
        {
            double nu = 1.0/0.5;
            double ro = lambda/nu; 
            double intp = 0;
            for (int i=0; i<=n; i++)
            {
                intp+=(double)Math.Pow(ro,i)/Factorial(i); 
            } 
            double p0 = (double)1/intp;
            double pn = (double)Math.Pow(ro,n)/Factorial(n)*p0;
            double Q = 1-pn;
            double A = lambda*Q;
            double k = (double)A/nu;

            Console.WriteLine("Вероятность простоя системы:{0}",p0);
            Console.WriteLine("Вероятность отказа системы:{0}",pn);
            Console.WriteLine("Отностельная пропускная способность:{0}",Q);
            Console.WriteLine("Абсолютная пропускная способность:{0}",A);
            Console.WriteLine("Среднее число занятых процессов:{0}",k);
        }
        public static int Factorial(int n) 
        {
            int res = (n == 0) ? 1 : n * Factorial(n - 1);
            return res;
        }
    }
    public class SystemProcessing
    {
        public void Estimation(Server server, double lambda)
        {
            Console.WriteLine("Вероятность простоя системы:{0}",(double)server.passive/server.sum);
            Console.WriteLine("Вероятность отказа системы:{0}",(double)server.rejectedCount/server.requestCount);
            Console.WriteLine("Отностельная пропускная способность:{0}",(double)server.processedCount/server.requestCount);
            Console.WriteLine("Абсолютная пропускная способность:{0}",lambda*server.processedCount/server.requestCount);
            Console.WriteLine("Среднее число занятых процессов:{0}",(double)server.busy/server.sum);             
        }
    }
    class Program
    {
        static void Main()
        {
            int n = 5;
            double lambda = 1.0/0.3;
            Teoretic teoria = new Teoretic();
            teoria.Estimation(n, lambda);

            Console.WriteLine();

            Server server = new Server(n);
            Client client = new Client(server);

            Thread counter = new Thread(server.CountThread);
            counter.Start();
            for(int id=1;id<=100;id++)
            {
                client.send(id);
                Thread.Sleep(300);
            }
            counter.Join();

            SystemProcessing systemProcessing = new SystemProcessing();
            systemProcessing.Estimation(server, lambda);

            Console.WriteLine("Bcego заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
        }

    }
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }
    public class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public int kolvo = 0;
        public int active = 0;
        public int sum = 0;
        public int busy = 0;
        public int passive = 0;
        public Server(int n)
        {
            pool = new PoolRecord[n];
            kolvo = n;
        }
        
        public void proc(object sender, procEventArgs e)
        {
            lock(threadLock)
            {
                Console.WriteLine("Заявка c номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < kolvo; i++)
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
            active++;
            int id = (int)arg;
            Console.WriteLine("Обработка заявки: {0}", id);
            Thread.Sleep(500);
            for (int i = 0; i < kolvo; i++) 
                if(pool[i].thread==Thread.CurrentThread)
                    pool[i].in_use = false;
            active--;
        }

        public void CountThread()
        {
            do
            {
                Thread.Sleep(30);
                if (active == 0)
                    passive += 1;
                else
                    passive += 0;
                busy += active;
                sum++;
            } while (active != 0);
        }
    }
    class Client
        {
            private Server server;
            public Client(Server server) 
            {
                this.server=server;
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