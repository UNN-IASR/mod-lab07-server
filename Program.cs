using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Laba7
{
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }
    public class Server
    {
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public static int n;
        static PoolRecord[] pool;
        struct PoolRecord
        {
            public Thread thread;
            public bool in_use;
            public int count;
            public int wait;
        }
        public Server(int value)
        {
            n = value;
            pool = new PoolRecord[n];
            for (int i = 0; i < n; ++i)
            {
                pool[i].in_use = false;
                pool[i].count = 0;
                pool[i].wait = 0;
            }
        }
        public static void Answer(object data)
        {
            Thread.Sleep(2);
            Console.WriteLine($"Заявка номер {data} обслужена", Encoding.UTF8);
            for (int i = 0; i < n; i++)
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                    break;
                }
        }
        public int getCount(int n)
        {
            return pool[n].count;
        }
        public int getWait(int n)
        {
            return pool[n].wait;
        }
        object threadLock = new object();
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine($"Заявка с номером: {e.id}", Encoding.UTF8);
                requestCount++;
                for (int i = 0; i < n; ++i)
                {
                    if (pool[i].in_use == false)
                    {
                        pool[i].wait++;
                    }
                }
                for (int i = 0; i < n; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].count++;
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
    public class Client
    {
        public Server server;
        public event EventHandler<procEventArgs> request;
        public Client(Server server)
        {
            this.server = server;
            this.request += server.proc;
        }
        protected virtual void OnProc(procEventArgs e)
        {
            request?.Invoke(this, e);
        }
        public void sendRequest(int id)
        {
            procEventArgs ev = new procEventArgs();
            ev.id = id;
            OnProc(ev);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string filepath = "C:\\Users\\Fenridan\\source\\repos\\Lab07\\Lab07\\results.txt";
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            ostrm = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
            writer = new StreamWriter(ostrm);
            Console.SetOut(writer);
            long fun(int n)
            {
                long x = 1;
                for (int i = 1; i <= n; i++)
                    x *= i;
                return x;
            }
            int clIntens = 100;
            int svIntens = 5;
            Server server = new Server(svIntens);
            Client[] client = new Client[100];
            for (int i = 0; i < clIntens; i++)
            {
                client[i] = new Client(server);
                client[i].sendRequest(i + 1);
            }
            Thread.Sleep(200);
            Console.WriteLine($"Всего : {server.requestCount}. Выполнено: {server.processedCount}. Отклонено: {server.rejectedCount}", Encoding.UTF8);
            for (int i = 0; i < svIntens; ++i)
            {
                Console.WriteLine($"Потоком {i + 1} выполнено {server.getCount(i)} заявок. Время простоя {server.getWait(i)} ", Encoding.UTF8);
            }

            double p = (double)clIntens / svIntens;
            double P0 = 0;
            for (int i = 0; i < svIntens; ++i)
            {
                P0 += Math.Pow(p, i) / fun(i);
            }
            P0 = Math.Pow(P0, -1);
            double Pn = (double)server.rejectedCount / clIntens;
            double Q = 1 - Pn;
            double A = clIntens * Q;
            double k = A / svIntens;
            Console.WriteLine($"Приведенная интенсивность потока заявок: {p}", Encoding.UTF8);
            Console.WriteLine($"Вероятность простоя системы: {P0}", Encoding.UTF8);
            Console.WriteLine($"Вероятность отказа системы: {Pn}", Encoding.UTF8);
            Console.WriteLine($"Относительная пропускная способность: {Q}", Encoding.UTF8);
            Console.WriteLine($"Абсолютная пропускная способность: {A}", Encoding.UTF8);
            Console.WriteLine($"Среднее число занятых каналов: {k}", Encoding.UTF8);
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
        }
    }
}
