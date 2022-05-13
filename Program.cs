using System;
using System.Threading;

namespace lab7
{
    struct PoolRecord
    {
        public Thread T;
        public bool u;
    }

    internal class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }

    internal class Server
    {
        private int n, time;
        private PoolRecord[] R;
        private int proc = 0;
        private int reque = 0;
        private int rej = 0;
        private object threadLock = new object();

        public Server(int n, int t)
        {
            this.n = n;
            time = t;
            R = new PoolRecord[n];
        }

        public int getReque()
        {
            return reque;
        }

        public int getProc()
        {
            return proc;
        }

        public int getRej()
        {
            return rej;
        }

        public void Requests(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Запрос #{0}", e.id);
                reque++;
                for (int i = 0; i < n; i++)
                {
                    if (!R[i].u)
                    {
                        R[i].u = true;
                        R[i].T = new Thread(new ParameterizedThreadStart(ShowProcRequest));
                        R[i].T.Start(e.id);
                        proc++;
                        return;
                    }
                }
                rej++;
            }
        }

        public void ShowProcRequest(object arg)
        {
            int id = (int)arg;

            Console.WriteLine("Запрос #{0} в процессе", id);
            Thread.Sleep(time);

            for (int i = 0; i < n; i++)
            {
                if (R[i].T == Thread.CurrentThread)
                {
                    R[i].u = false;
                }
            }
        }


    }






    internal class Client
    {
        private Server s;
        public Client(Server s)
        {
            this.s = s;
            request += s.Requests;
        }

        public void Requests(int num)
        {
            procEventArgs args = new procEventArgs();

            args.id = num;

            if (request != null)
            {
                request(this, args);
            }
        }

        public event EventHandler<procEventArgs> request;
    }


    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server(5, 2000);
            Client c = new Client(s);

            for (int i = 1; i <= 100; i++)
            {
                c.Requests(i);
                Thread.Sleep(200);
            }

            Console.WriteLine();
            Console.WriteLine("Результат:");
            Console.WriteLine("Все запросы: {0}", s.getReque());
            Console.WriteLine("Обработанные: {0}", s.getProc());
            Console.WriteLine("Необработанные: {0}", s.getRej());
        }
    }
}
