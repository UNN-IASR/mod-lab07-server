using System;
using System.Threading;

namespace server
{
    class Program
    {
        static void Main()
        {
            int ser_flow = 5;
            int cli_del = 70; 
            int ser_del = 600; 

            Server server = new Server(ser_flow, ser_del);
            Client client = new Client(server);

            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(cli_del); 
            }

            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            double in_cli_flow = 1.00 / (cli_del / 1000.00);
            double in_ser_flow = 1.00 / (ser_del / 1000.00);
            double aver = (double)in_cli_flow * in_ser_flow;

            int factor(int n)
            {
                if (n <= 1) return 1;
                return n * factor(n - 1);
            }

            double sum = 0;

            for (int i = 0; i <= ser_flow; i++)
            {
                sum += (double)Math.Pow(aver, i) / factor(i);
            }

            double down_time = (double)1.00 / sum;
            double prob_fail = (double)(Math.Pow(aver, ser_flow) / factor(ser_flow)) * down_time;
            double relatives = 1 - prob_fail;
            double abs = in_cli_flow * relatives;
            double occup = (double)abs * in_ser_flow;
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
        int flow = 0;
        int del = 0; 
        public Server(int flow, int del)
        {
            this.flow = flow;
            this.del = del;
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
            int id = (int)arg;         
            //for (int i = 1; i < 9; i++)         
            //{
            Console.WriteLine("Обработка заявки: {0}",id);             
            //Console.WriteLine("{0}",Thread.CurrentThread.Name);
            Thread.Sleep(500);          
            //}
            for(int i = 0; i < 5; i++)              
                if(pool[i].thread==Thread.CurrentThread)                
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
    public class procEventArgs : EventArgs { 
        public int id { get; set; } 
    }
}
