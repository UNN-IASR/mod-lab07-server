using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace sh7
{
    struct PoolRecord
    {
        public Thread T;
        public bool usin;
    }

    internal class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }

    internal class Server
    {
        private int n, time;
        private PoolRecord[] R;
        private int CP = 0;
        private int Crec = 0;
        private int Crej = 0;
        private object threadLock = new object();

        public Server(int n, int t)
        {
            this.n = n;
            time = t;
            R = new PoolRecord[n];
        }

        public int getCrec()
        {
            return Crec;
        }

        public int getCP()
        {
            return CP;
        }

        public int getCrej()
        {
            return Crej;
        }

        public void PUL(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Запрос #{0}", e.id);
                Crec++;
                for (int i = 0; i < n; i++)
                {
                    if (!R[i].usin)
                    {
                        R[i].usin = true;
                        R[i].T = new Thread(new ParameterizedThreadStart(Answer));
                        R[i].T.Start(e.id);
                        CP++;
                        return;
                    }
                }
                Crej++;
            }
        }

        public void Answer(object arg)
        {
            int id = (int)arg;

            Console.WriteLine("Processing request #{0}", id);
            Thread.Sleep(time);

            for (int i = 0; i < n; i++)
            {
                if (R[i].T == Thread.CurrentThread)
                {
                    R[i].usin = false;
                }
            }
        }

        
    }

}
