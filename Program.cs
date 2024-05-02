using System;
using System.Threading;

namespace TPProj {
    class Program {
        static void Main() {
            int serverDelay = 700;
            int clientDelay = 100;
            int n = 2;
            Server server = new Server(serverDelay, n);
            Client client = new Client(server);
            double threadRequestIntensity = 1.0/(clientDelay/1000.0);
            double threadMaintenanceIntensity = 1.0/(serverDelay/1000.0);
            double ro = threadRequestIntensity / threadMaintenanceIntensity;
            double P0 = 0;
            double Pn;
            double Q;
            double A;
            double k;

            for(int i=0; i<=n; i++) {
                P0 += Math.Pow(ro, i) / Fact(i);
            }

            P0 = 1 / P0;
            Pn = P0 * Math.Pow(ro, n) / Fact(n);
            Q = 1 - Pn;
            A = threadRequestIntensity * Q;
            k = A / threadMaintenanceIntensity;

            Console.WriteLine("Интенивность потока запросов:    "+threadRequestIntensity);
            Console.WriteLine("Интенсивность обслуживания:      "+threadMaintenanceIntensity);
            Console.WriteLine("Количество потоков:              "+n);
            Console.WriteLine("Вероятность простоя системы:          "+Math.Round(P0,5));
            Console.WriteLine("Вероятность отказа системы:           "+Math.Round(Pn,5));
            Console.WriteLine("Относительная пропускная способность:  "+Math.Round(Q,5));
            Console.WriteLine("Абсолютная пропускная способность:    "+Math.Round(A,5));
            Console.WriteLine("Cреднее число занятых каналов:      "+Math.Round(k,5));

            double a = 0, b = 0;
            Thread counter = new Thread(() => {
                (a, b) = Stat.count(server);
            });
            counter.Start();
            for(int id=1;id<=100;id++) {
                client.send(id);
                Thread.Sleep(clientDelay);
            }
            counter.Join();

            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            Console.WriteLine("Вероятность простоя системы:          "+Math.Round(b,5));
            Console.WriteLine("Вероятность отказа системы:           "+Math.Round((double)server.rejectedCount/server.requestCount,5));
            Console.WriteLine("Относительная пропускная способность:  "+Math.Round((double)server.processedCount/server.requestCount,5));
            Console.WriteLine("Абсолютная пропускная способность:    "+Math.Round((double)server.processedCount/server.requestCount*threadRequestIntensity,5));
            Console.WriteLine("Cреднее число занятых каналов:      "+Math.Round(a,5));
        }
        public static int Fact(int n) {
            if(n == 0)
                return 1;
            else
                return n * Fact(n - 1);
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
        private int serverDelay = 0;
        public int n = 0;

        public int activeThreads {get {
            return pool.Count(x => x.in_use);
        }}
 
        public Server(int _serverDelay, int _n) {
            pool = new PoolRecord[_n];
            serverDelay = _serverDelay;
            n = _n;
        }
        public void proc(object sender, procEventArgs e) {
            lock(threadLock) {
                //Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for(int i = 0; i < n; i++) {
                    if(!pool[i].in_use) {
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
        public void Answer(object arg) {
            int id = (int)arg;
            //Console.WriteLine("Обработка заявки: {0}",id);
            Thread.Sleep(serverDelay);
            for(int i = 0; i < n; i++)
                if(pool[i].thread==Thread.CurrentThread)
                    pool[i].in_use = false;
        }
    }
    class Client {
        private Server server;
        public Client(Server _server) {
            server = _server;
            request += server.proc;
        }
        public void send(int id) {
            procEventArgs args = new procEventArgs();
            args.id = id;
            OnProc(args);
        }
        protected virtual void OnProc(procEventArgs e){
            EventHandler<procEventArgs> handler = request;
            if (handler != null) {
                handler(this, e);
            }
        }
        public event EventHandler<procEventArgs> request;
    }
    public class procEventArgs : EventArgs {
        public int id { get; set; }
    }
    static class Stat {
        public static (double a, double b) count(Server server) {
            double activeThreads = 0;
            double passiveThreads = 0;
            int counter = 0;
            do {
                activeThreads += server.activeThreads;
                passiveThreads += server.activeThreads == 0 ? 1 : 0;
                counter++;
            } while (server.activeThreads != 0);
            return (activeThreads/counter, passiveThreads/counter);
        }
    }
}