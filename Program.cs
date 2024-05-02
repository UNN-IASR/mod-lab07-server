using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;
namespace TPProj{
    class Program
    {
        static void Main()
        {
            Server server = new Server();
            Client client = new Client(server);
            int CountFlow = 100;
            float intensityFlowRequirement = 0f;//интенсивностьТребований
            float intensityFlowService = 0f;//Интенсивность Обслудживания
            float intensityFlowApplication = 0f;//
            float PStopSystem = 0f;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int id = 1; id <= CountFlow; id++)
            {
                client.send(id);
                Thread.Sleep(50);
            }
            stopwatch.Stop();
            intensityFlowRequirement = CountFlow / (float)stopwatch.ElapsedMilliseconds;
            intensityFlowService = server.processedCount / (float)stopwatch.ElapsedMilliseconds;
            intensityFlowApplication = intensityFlowRequirement / intensityFlowService;
            PStopSystem=Pstop(5, intensityFlowApplication);
            float PRefusalSystem = ((float)Math.Pow(intensityFlowApplication, 5) / Factorial(5)) * PStopSystem;
            float RelativeThroughput = 1 - PRefusalSystem;
            float AbsolutlyThroughput = intensityFlowRequirement * RelativeThroughput;
            float BusyChannel = AbsolutlyThroughput / intensityFlowService;
            Console.WriteLine("Вероятность простоя" + PStopSystem.ToString());
            Console.WriteLine("Вероятность Отказа" + PRefusalSystem.ToString());
            Console.WriteLine("относительная пропускная способность" + RelativeThroughput.ToString());
            Console.WriteLine("абсолютная пропускная способность" + AbsolutlyThroughput.ToString());
            Console.WriteLine("среднее число занятых каналов" + BusyChannel.ToString());
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
        }
        public static long Factorial(int n)
        {
            long result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
        static float Pstop(int Count,float Numbers)
        {
            float result = 0;
            for(int i=1;i<Count+1;i++)
            {
                result += (float)Math.Pow(Numbers, i)/Factorial(i);
            }
            return (float)Math.Pow(result, -1);
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
        public int requestCount = 0;//кол-во запросов
        public int processedCount = 0;//обработанные
        public int rejectedCount = 0;//отклоненные
        public Server()
        {
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
            Console.WriteLine("Обработка заявки: {0}", id);
            //Console.WriteLine("{0}",Thread.CurrentThread.Name);
            Thread.Sleep(500);
            //}
            for (int i = 0; i < 5; i++)
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;
        }
    }
    class Client
    {
        private Server server;        
        public event EventHandler<procEventArgs> request;
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

    }
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }
}
