using System;
using System.Threading;

namespace TPProj {
    class Program {
        static void Main() {
            int lambda, mu;
            lambda = Convert.ToInt32(Console.ReadLine());
            mu  = Convert.ToInt32(Console.ReadLine());
            int t1 = (int)(1000*1.0/lambda);
            int t2 = (int)(1000*1.0/mu);
            Server server = new Server(t2);
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++) {
                client.send(id);
                Thread.Sleep(t1);
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
        }
    }
    struct PoolRecord {
        public Thread thread;
        public bool in_use;
    }


    class Server {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        private int time2;
        public Server(int t2) {
            pool = new PoolRecord[5];
            time2 = t2;
        }
        public void proc(object sender, procEventArgs e) {
            lock (threadLock) {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < 5; i++) {
                    if (pool[i].in_use == false) {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        ThreadParam tp = new ThreadParam(e.id, time2);
                        pool[i].thread.Start(tp);
                        processedCount++;
                        return;
                    }
                }
                rejectedCount++;
            }
        }
        public void Answer(object arg) {
            if (arg is ThreadParam param)
            {
                int id = param.ID;
                Console.WriteLine("     Обработка заявки: {0}", id);
                Console.WriteLine("{0}", Thread.CurrentThread.Name);
                Thread.Sleep(param.t2);
                for (int i = 0; i < 5; i++)
                    if (pool[i].thread == Thread.CurrentThread)
                        pool[i].in_use = false;
            }
        }
    }

    
    class Client {
        private Server server;
        public event EventHandler<procEventArgs> request;
        public Client(Server server) {
            this.server = server;
            this.request += server.proc;
        }
        public void send(int id) {
            procEventArgs args = new procEventArgs();
            args.id = id;
            OnProc(args);
        }
        protected virtual void OnProc(procEventArgs e) {
            EventHandler<procEventArgs> handler = request;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public class procEventArgs : EventArgs {
        public int id { get; set; }
    }  
    record class ThreadParam(int ID, int t2);
}
