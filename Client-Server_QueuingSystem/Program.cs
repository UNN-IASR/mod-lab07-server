using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client_Server_QueuingSystem;

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
        procEventArgs args = new procEventArgs()
        {
            id = id
        };
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
class Server
{
    private PoolRecord[] pool;
    private object threadLock = new object();
    public int ThreadsInUse => pool.Count(t => t.in_use);

    //Параметры
    public int capasity = 0;
    public int requestDelay = 0;

    //Запись результатов
    public int requestCount = 0;
    public int processedCount = 0;
    public int rejectedCount = 0;
    public Server(int capasity, double serviсeIntensity)
    {
        pool = new PoolRecord[capasity];
        this.capasity = capasity;
        this.requestDelay = (int)(1/serviсeIntensity);
    }
    public void proc(object sender, procEventArgs e)
    {
        lock (threadLock)
        {
            //Console.WriteLine($"3aявка с номером: {e.id}");
            requestCount++;
            for (int i = 0; i < capasity; i++)
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
        //for(int i = 1; i < 9; i++) 
        //{
        //Console.WriteLine($"Обработка заявки: {id} ");
        //Console.WriteLine("{0}",Thread.CurrentThread.Name);
        Thread.Sleep(requestDelay);
        // }
        for (int i = 0; i < capasity; i++)
            if (pool[i].thread == Thread.CurrentThread)
                pool[i].in_use = false;
    }
}
public class procEventArgs : EventArgs
{
    public int id { get; init; }
}

struct PoolRecord
{
    public Thread thread;
    public bool in_use;
}
static class Helper
{
    public record AvgThreadsUse(double AvgBusyThreads, double AvgIdleThreads);
    public static async Task<AvgThreadsUse> CountAvgThreadsUseAsync(Server server, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        double busyThreads = 0;
        double idleThreads = 0;
        int counter = 0;
        while (!token.IsCancellationRequested)
        {
            busyThreads += server.ThreadsInUse;
            idleThreads += server.ThreadsInUse == 0 ? 1 : 0;
            counter++;
            await Task.Delay(1);
        }

        return new AvgThreadsUse(busyThreads / counter, idleThreads / counter);
    }
}
class Program
{
    static void Main(string[] args)
    {
        //Парметры
        double serviceIntensity = 20.0 / 1000;
        double requestIntensity = 100.0 / 1000;
        int requestCount = 1000;
        int serverCapacity = 3;

        //Теоретические результаты
        Console.WriteLine("Теоретические резульаты: ");
        TheoreticalResults(serviceIntensity, requestIntensity, serverCapacity);


        //Вычислительный эксперимент
        int requestDelay = (int)(1 / requestIntensity);
        Server server = new Server(serverCapacity, serviceIntensity);
        Client client = new Client(server);
        var CTS = new CancellationTokenSource();
        var statisticTask = Helper.CountAvgThreadsUseAsync(server, CTS.Token);
        for (int id = 1; id <= requestCount; id++)
        {
            client.send(id);
            Thread.Sleep(requestDelay);
        }
        CTS.Cancel();
        var data = statisticTask.Result;

        //Результаты вычислительного эксперимента
        Console.WriteLine("\nРезультаты вычислительного эксперимента: \n");
        Console.WriteLine($"Bcero заявок: {server.requestCount}");
        Console.WriteLine($"Обработано заявок: {server.processedCount}");
        Console.WriteLine($"Отклонено заявок: {server.rejectedCount}");

        double P0 = data.AvgIdleThreads;
        double Pn = (double)server.rejectedCount / server.requestCount;
        double Q = (double)server.processedCount / server.requestCount;
        double A = serviceIntensity * (double)server.processedCount / server.requestCount;
        double k = data.AvgBusyThreads;
        Console.WriteLine($"Вероятность простоя системы         : {Math.Round(P0, 6)}");
        Console.WriteLine($"Вероятность отказа системы          : {Math.Round(Pn, 6)}");
        Console.WriteLine($"Относительная пропускная способность: {Math.Round(Q, 6)}");
        Console.WriteLine($"Абсолютная пропускная способность   : {Math.Round(A, 6)}");
        Console.WriteLine($"Среднее число занятых каналов       : {Math.Round(k, 6)}");
    }
    static void TheoreticalResults(double serviceIntensity, double requestIntensity, int serverCapacity)
    {
        double lambda = requestIntensity;
        double nu = serviceIntensity;
        int n = serverCapacity;

        //Приведенная интенсивность потока заявок
        double p = lambda/nu;

        //Вероятность простоя системы
        double P0 = 1;
        int j = 1;
        for (int i = 1; i <= n; i++, j*=i)
            P0 += Math.Pow(p, i) / j;
        P0 = 1 / P0;
        //Вероятность отказа системы
        double Pn = Math.Pow(p, n)*P0 / (j/(n+1));
        //Относительная пропускная способность
        double Q = 1 - Pn;
        //Абсолютная пропускная способность
        double A = lambda * Q;
        //Среднее число занятых каналов
        double k = A / nu;
        Console.WriteLine($"Вероятность простоя системы         : {Math.Round(P0, 6)}");
        Console.WriteLine($"Вероятность отказа системы          : {Math.Round(Pn, 6)}");
        Console.WriteLine($"Относительная пропускная способность: {Math.Round(Q, 6)}");
        Console.WriteLine($"Абсолютная пропускная способность   : {Math.Round(A, 6)}");
        Console.WriteLine($"Среднее число занятых каналов       : {Math.Round(k, 6)}");
    }
}

