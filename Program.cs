using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;

namespace Laba7
{
    public class procEventArgs : EventArgs
    {
        public int id{
            get; set; 
        }
    }
    public class Server
    {
        public int count_request = 0;
        public int count_process = 0;
        public int count_reject = 0;
        public static int n;
        static PoolRecord[] pool;
        object lock_thread = new object();
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
        public int getCount(int n)
        {
            return pool[n].count;
        }
        public int getWait(int n)
        {
            return pool[n].wait;
        }
        public static void Answer(object data)
        {
            Thread.Sleep(2);
            Console.WriteLine($"№{data} обслужена", Encoding.UTF8);
            for (int i = 0; i < n; i++)
                if (pool[i].thread == Thread.CurrentThread)
                {
                    pool[i].in_use = false;
                    break;
                }
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (lock_thread)
            {
                Console.WriteLine($"№{e.id}", Encoding.UTF8);
                count_request++;
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
                        count_process++;
                        return;
                    }
                }
                count_reject++;
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
            //string filepath = "C:\\Users\\Fenridan\\source\\repos\\Lab07\\Lab07\\results.txt";
            string filepath = "C:\\Users\\Maria\\Documents\\Учеба\\3 курс, 6 семестр\\Моделирование информационных процессов и систем (С++)\\Project_7\\result.txt";
            FileStream my_file;
            StreamWriter writer;
            TextWriter _out = Console.Out;
            my_file = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write);
            writer = new StreamWriter(my_file);
            Console.SetOut(writer);
            long fun(int n)
            {
                long x = 1;
                for (int i = 1; i <= n; i++)
                    x *= i;
                return x;
            }
            int cli = 100;
            int serv = 5;
            Server server = new Server(serv);
            Client[] client = new Client[100];
            for (int i = 0; i < cli; i++)
            {
                client[i] = new Client(server);
                client[i].sendRequest(i + 1);
            }
            Thread.Sleep(1000);
            Console.WriteLine($"Всего : {server.count_request}. Выполнено: {server.count_process}. Отклонено: {server.count_reject}", Encoding.UTF8);
            for (int i = 0; i < serv; ++i)
            {
                Console.WriteLine($"Потоком {i + 1} выполнено {server.getCount(i)}. Время простоя {server.getWait(i)} ", Encoding.UTF8);
            }

            double p = (double)cli / serv;
            double k0 = 0;
            for (int i = 0; i < serv; ++i)
            {
                k0 += Math.Pow(p, i) / fun(i);
            }
            k0 = Math.Pow(k0, -1);
            double Pn = (double)server.count_reject / cli;
            double Q = 1 - Pn;
            double A = cli * Q;
            double k = A / serv;
            Console.WriteLine($"Приведенная интенсивность потока заявок: {p}", Encoding.UTF8);
            Console.WriteLine($"Вероятность простоя системы: {k}", Encoding.UTF8);
            Console.WriteLine($"Вероятность отказа системы: {Pn}", Encoding.UTF8);
            Console.WriteLine($"Относительная пропускная способность: {Q}", Encoding.UTF8);
            Console.WriteLine($"Абсолютная пропускная способность: {A}", Encoding.UTF8);
            Console.WriteLine($"Среднее число занятых каналов: {k}", Encoding.UTF8);
            Console.SetOut(_out);
            writer.Close();
            my_file.Close();
        }
    }
}
