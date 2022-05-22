using System;
using System.Threading;

namespace MSS
{
    class Program
    {
        static long Factorial(long n)
        {
            if (n == 0)
            {
                return 1;
            }
            long res = 1;
            for (long i = 1; i <= n; i++)
            {
                res *= i;
            }
            return res;
        }
        static void Main(string[] args)
        {
            int callDelay = 5;
            int serverDelay = 40;
            UInt32 serverSize = 3;
            UInt32 clientCount = 50;
            Server server = new Server(serverSize, serverDelay);
            Client[] clients = new Client[clientCount];
            for (UInt32 i = 0; i < clientCount; i++)
            {
                clients[i] = new Client(i, server);
            }

            for (UInt32 i = 0; i < clientCount; i++)
            {
                clients[i].call();
                Thread.Sleep(callDelay);
            }

            Thread.Sleep(serverDelay);
            Console.WriteLine("=========================================");
            server.PrintStatistics();
            Console.WriteLine("=========================================");
            //Statistics Analizing
            double HandlingIntencity = 1f / (double)serverDelay;
            double RequestIntencity = 1f / (double)callDelay;
            double StreamIntencity = RequestIntencity / HandlingIntencity;

            double IdlingProbability = 0;
            for (int i = 0; i <= serverSize; i++)
            {
                IdlingProbability += Math.Pow(StreamIntencity, i) / Factorial(i);
            }
            IdlingProbability = 1 / IdlingProbability;
            double DeclineProbability = IdlingProbability * (Math.Pow(StreamIntencity, serverSize) / Factorial(serverSize));
            double RelativeThroughput = 1 - DeclineProbability;
            double AbsoluteThroughput = RelativeThroughput * RequestIntencity;
            double MeanBusyHandlers = AbsoluteThroughput / HandlingIntencity;

            Console.WriteLine("Analizing:");
            Console.WriteLine($"RequestIntencity: {RequestIntencity}");
            Console.WriteLine($"HandlingIntencity: {HandlingIntencity}");
            Console.WriteLine($"StreamIntencity: {StreamIntencity}");
            Console.WriteLine($"IdlingProbability: {IdlingProbability}");
            Console.WriteLine($"DeclineProbability: {DeclineProbability}");
            Console.WriteLine($"RelativeThroughput: {RelativeThroughput}");
            Console.WriteLine($"AbsoluteThroughput: {AbsoluteThroughput}");
            Console.WriteLine($"MeanBusyHandlers: {MeanBusyHandlers}");
        }
    }
}
