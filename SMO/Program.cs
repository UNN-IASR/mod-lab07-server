using System;
using System.Threading;
using System.IO;

namespace SMO {
    public class procEventArgs : EventArgs {
        public int id { get; set; }
    }
    struct PoolRecord {
        public Thread thread;
        public bool in_use;
        public int wait;
        public int work;
    }
    class Server {
        public int requestCount;
        public int processedCount;
        public int rejectedCount;
        public int poolcount;

        public PoolRecord[] pool;
        object threadLock;

        public Server(int count, PoolRecord[] pool) {
            requestCount = 0;
            processedCount = 0;
            requestCount = 0;
            this.pool = pool;
            this.poolcount = count;
            threadLock = new object();
            for (int i = 0; i < poolcount; i++)
                pool[i].in_use = false;
        }
        void Answer(object e) {
            Console.WriteLine("Выполняется заявка с номером {0}", e);
            Thread.Sleep(10);
            Console.WriteLine("Заявка с номером {0} выполнена", e);
            for (int i = 0; i < poolcount; i++) {
                if (pool[i].thread == Thread.CurrentThread) {
                    pool[i].in_use = false;
                    pool[i].thread = null;
                    break;
                }
            }
        }
        public void proc(object sender, procEventArgs e) {
            lock (threadLock) {
                Console.WriteLine("Заявка с номером {0}", e.id);
                requestCount++;
                for (int i = 0; i < poolcount; i++) {
                    if (!pool[i].in_use)
                        pool[i].wait++;
                }
                for (int i = 0; i < poolcount; i++) {
                    if (!pool[i].in_use) {
                        pool[i].work++;
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
    }
    class Client {
        public event EventHandler<procEventArgs> request;
        Server server;

        int index = 0;

        public Client(Server server) {
            this.server = server;
            this.request += server.proc;
            index = 0;
        }
        protected virtual void OnProc(procEventArgs e) {
            EventHandler<procEventArgs> handler = request;
            if (handler != null)
                handler(this, e);
        }
        public void Work() {
            procEventArgs e = new procEventArgs();
            index++;
            e.id = index;
            this.OnProc(e);
        }
    }
    class Program {
        static long Fact(long n) {
            if (n == 0)
                return 1;
            else
                return n * Fact(n - 1);
        }
        static string Conclusion(Server server, int requests) {
            string output = "";

            double p = requests / server.poolcount;
            double temp = 0;
            for (int i = 0; i < server.poolcount; i++)
                temp += Math.Pow(p, i) / Fact(i);
            double p0 = 1 / temp;
            double pn = Math.Pow(p, server.poolcount) * p0 / Fact(server.poolcount);

            output += "Количество потоков: " + server.poolcount + '\n' + "Всего запросов: " + server.requestCount + '\n' + "Выполнено запросов: " + server.processedCount + '\n' + "Отклонено запросов: " + server.rejectedCount + '\n';
            for (int i = 0; i < server.poolcount; i++)
                output += "Потоком с номером " + (i + 1) + " выполнено запросов: " + server.pool[i].work + "; тактов ожидания: " + server.pool[i].wait + '\n';
            output += "Интенсивность потока заявок: " + p + '\n';
            output += "Вероятность простоя системы: " + p0 + '\n';
            output += "Вероятность отказа системы: " + pn + '\n';
            output += "Относительная пропускная способность: " + (1 - pn) + '\n';
            output += "Абсолютная пропускная способность: " + (requests * (1 - pn)) + '\n';
            output += "Среднее число занятых каналов: " + ((requests * (1 - pn)) / server.poolcount) + '\n';

            return output;
        }

        static int ThreadCount = 16;
        static int RequestCount = 100;
        static PoolRecord[] pool = new PoolRecord[ThreadCount];

        static void Main(string[] args) {
            Server server = new Server(ThreadCount, pool);
            Client client = new Client(server);
            for (int i = 0; i < RequestCount; i++)
                client.Work();
            Thread.Sleep(1000);
            Console.WriteLine("\n--------\n");
            string output = Conclusion(server, RequestCount);
            Console.WriteLine(output);
            File.WriteAllText("Output.txt", output);
        }
    }
}
