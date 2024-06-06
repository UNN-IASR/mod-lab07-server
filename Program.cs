using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace lab07
{
    internal class Program
    {
        const int MILLISECONDS_IN_SECOND = 1000;
        static void Main(string[] args)
        {
            int requests = 100;
            int streams = 5;

            Server server = new Server(streams);
            Client client = new Client(server);
            for (int id = 1; id <= requests; id++)
            {
                var timer = new Stopwatch();
                timer.Start();
                client.send(id);
                Thread.Sleep(50);
                timer.Stop();
                server.requestTime += timer.ElapsedMilliseconds;

            }

            double l = 20;
            double m = 10;

            StreamWriter sw = new StreamWriter("../../Result.txt", true, Encoding.UTF8);

            sw.WriteLine("Ожидаемые результаты");
            Result(l, m, requests, streams, server,sw);

            l = (double)server.requestCount * MILLISECONDS_IN_SECOND / server.requestTime;
            m = (double)server.processedCount * MILLISECONDS_IN_SECOND * streams / server.handlingTime;

            sw.WriteLine();

            sw.WriteLine("Фактические результаты");
            Result(l, m, requests, streams, server, sw);

            sw.Close();
            Console.WriteLine("OK");
            Console.ReadLine();
        }

        public static void Result(double l, double m,int requests,int streams, Server server, StreamWriter sw)
        {
            double p = l / m;

            double downtimeProbability = 0;
            int factorial = 1;
            for (int i = 0; i < streams;)
            {
                downtimeProbability += Math.Pow(p, i) / factorial;
                i++;
                factorial *= i;
            }
            downtimeProbability = 1 / downtimeProbability;

            double failureProbability = downtimeProbability * Math.Pow(p, streams) / factorial;
            double relativePool = 1 - failureProbability;
            double absolutePool = l * relativePool;
            double avgUsedchannel = absolutePool / m;
            sw.WriteLine("Всего заявок: {0}", server.requestCount);
            sw.WriteLine("Обработано заявок: {0}", server.processedCount);
            sw.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
            sw.WriteLine();
            sw.WriteLine("вероятность простоя системы: {0}", downtimeProbability);
            sw.WriteLine("вероятность отказа системы: {0}", failureProbability);
            sw.WriteLine("относительная пропускная способность: {0}", relativePool);
            sw.WriteLine("абсолютная пропускная способность: {0}", absolutePool);
            sw.WriteLine("среднее число занятых каналов: {0}", avgUsedchannel);

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
            public double handlingTime = 0;
            public double requestTime = 0;
            public Server(int streams)
            {
                pool = new PoolRecord[streams];
            }
            public void proc(object sender, procEventArgs e)
            {
                lock (threadLock)
                {
                    //Console.WriteLine("Заявка с номером: {0}", e.id);
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
                // Console.WriteLine("Обработка заявки: {0}", id);
                //Console.WriteLine("{0}",Thread.CurrentThread.Name);
                var timer = new Stopwatch();
                timer.Start();
                Thread.Sleep(500);
                //}
                for (int i = 0; i < pool.Length; i++)
                    if (pool[i].thread == Thread.CurrentThread)
                        pool[i].in_use = false;
                timer.Stop();
                this.handlingTime += timer.ElapsedMilliseconds;
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
}
