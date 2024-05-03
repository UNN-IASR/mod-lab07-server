namespace ClientServerQueuingSystem;

internal class Program
{
    static void Main(string[] args)
    {
        double serviceIntensity = 20.0 / 1000;
        double requestIntensity = 100.0 / 1000;
        int requestCount = 100;
        int threadsCount = 3;
        
        PrintTheoreticalCalculations(serviceIntensity, requestIntensity, threadsCount);

        Server server = new Server(serviceIntensity, threadsCount);
        var producer = new RequestProducer(requestIntensity, server);
        var cts = new CancellationTokenSource();
        var statisticTask = Helper.CountAvgThreadsUseAsync(server, cts.Token);
        producer.Produce(requestCount);
        cts.Cancel();
        var stats = statisticTask.Result;

        double P0 = stats.AvgIdleThreads;
        double Pn = (double)server.RejectedCount / server.RequestCount;
        double Q = (double)server.ProcessedCount / server.RequestCount;
        double A = serviceIntensity * (double)server.ProcessedCount / server.RequestCount;
        double k = stats.AvgBusyThreads;
        Console.WriteLine();
        Console.WriteLine("Практические данные:");
        Console.WriteLine();
        Console.WriteLine("Всего заявок: {0}", server.RequestCount);
        Console.WriteLine("Обработано заявок: {0}", server.ProcessedCount);
        Console.WriteLine("Отклонено заявок: {0}", server.RejectedCount);
        Console.WriteLine($"Вероятность простоя системы         : {Math.Round(P0, 6)}");
        Console.WriteLine($"Вероятность отказа системы          : {Math.Round(Pn, 6)}");
        Console.WriteLine($"Относительная пропускная способность: {Math.Round(Q, 6)}");
        Console.WriteLine($"Абсолютная пропускная способность   : {Math.Round(A, 6)}");
        Console.WriteLine($"Среднее число занятых каналов       : {Math.Round(k, 6)}");
    }

    static void PrintTheoreticalCalculations(
        double serviceIntensity, 
        double requestIntensity,
        int threadsCount)
    {
        double rho = requestIntensity / serviceIntensity;

        double sum = Enumerable
            .Range(0, threadsCount + 1)
            .Sum(x => Math.Pow(rho, x) / x.Factorial());
        double P0 = 1 / sum;
        double Pn = (Math.Pow(rho, threadsCount) / threadsCount.Factorial()) * P0;
        double Q = 1 - Pn;
        double A = requestIntensity * Q;
        double k = A / serviceIntensity;

        Console.WriteLine("Теоретические расчеты");
        Console.WriteLine();
        Console.WriteLine($"Вероятность простоя системы         : {Math.Round(P0,6)}");
        Console.WriteLine($"Вероятность отказа системы          : {Math.Round(Pn,6)}");
        Console.WriteLine($"Относительная пропускная способность: {Math.Round(Q, 6)}");
        Console.WriteLine($"Абсолютная пропускная способность   : {Math.Round(A, 6)}");
        Console.WriteLine($"Среднее число занятых каналов       : {Math.Round(k, 6)}"); 
    }
}

public static class IntExtensions
{
    public static int Factorial(this int src)
    {
        int result = 1;
        for (int i = 1; i <= src; i++)
        {
            result *= i;
        }

        return result;
    }
}

class RequestProducer
{
    private readonly int requestDelay;
    Client client;

    public RequestProducer(double requestIntensity, Server server)
    {
        client = new Client(server);
        requestDelay = (int)(1 / requestIntensity);
    }

    public void Produce(int count)
    {
        for (int id = 1; id <= count; id++)
        {
            client.Send(id);
            Thread.Sleep(requestDelay);
        }
    }
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
        while(!token.IsCancellationRequested)
        {
            busyThreads += server.ThreadsInUse;
            idleThreads += server.ThreadsInUse == 0 ? 1 : 0;
            counter++;
            await Task.Delay(1);
        }

        return new AvgThreadsUse(busyThreads / counter, idleThreads / counter);
    }
}

struct PoolRecord
{
    public Thread Thread;
    public bool IsBusy;
}
class Server
{
    private readonly int processingTime = 1000;
    private PoolRecord[] pool;
    private object threadLocker = new object();

    public int RequestCount { get; private set; } = 0;
    public int ProcessedCount { get; private set; } = 0;
    public int RejectedCount { get; private set; } = 0;

    public int ThreadsInUse => pool.Count(t => t.IsBusy);

    public Server(double serviceIntensity, int poolSize)
    {
        pool = new PoolRecord[poolSize];
        processingTime = (int)(1 / serviceIntensity);
    }

    public void Process(object sender, ProcEventArgs e)
    {
        lock (threadLocker)
        {
            //Console.WriteLine("Заявка с номером: {0}", e.Id);
            RequestCount++;
            for (int i = 0; i < pool.Length; i++)
            {
                if (!pool[i].IsBusy)
                {
                    pool[i].IsBusy = true;
                    pool[i].Thread = new Thread(new ParameterizedThreadStart(Answer));
                    pool[i].Thread.Start(e.Id);
                    ProcessedCount++;
                    return;
                }
            }
            RejectedCount++;
        }
    }
    public void Answer(object arg)
    {
        int id = (int)arg;

        //Console.WriteLine("Обработка заявки: {0}", id);
        Thread.Sleep(processingTime);

        for (int i = 0; i < pool.Length; i++)
            if (pool[i].Thread == Thread.CurrentThread)
                pool[i].IsBusy = false;
    }
}
class Client
{
    public event EventHandler<ProcEventArgs> Request;

    public Client(Server server)
    {
        this.Request += server.Process;
    }
    public void Send(int id)
    {
        ProcEventArgs args = new ProcEventArgs()
        {
            Id = id
        };
        OnProc(args);
    }
    protected virtual void OnProc(ProcEventArgs e)
    {
        EventHandler<ProcEventArgs> handler = Request;
        if (handler != null)
        {
            handler(this, e);
        }
    }
}
public class ProcEventArgs : EventArgs
{
    public int Id { get; init; }
}
