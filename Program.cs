using System;
using System.Threading;

namespace smo
{
    class Program
    {
        static void Main()
        {
            int flow = 5;
            int cli = 100;
            int ser = 500;

            Server server = new Server(flow, ser);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id); 
                Thread.Sleep(50);
            }

            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            double inten_client_flow = 1.00 / (cli / 1000.00);
            double inten_server_flow = 1.00 / (ser / 1000.00);
            double average = (double)inten_client_flow * inten_server_flow;

            int Fac(int n)
            {
                if (n <= 1) return 1;
                return n * Fac(n - 1);
            }

            double sum = 0;

            for (int i = 0; i <= flow; i++)
            {
                sum += (double)Math.Pow(average, i) / Fac(i);
            }

            double downtime = (double)1.00 / sum;
            double failure = (double)(Math.Pow(average, flow) / Fac(flow)) * downtime;
            double relat = 1 - failure;
            double absol = inten_client_flow * relat;
            double averageNumberChannelsOccupied = (double)absol * inten_server_flow;
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
