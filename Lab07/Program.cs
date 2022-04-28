using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Lab07
{
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }

    public struct InfoAboutAlgo
    {
        public int numberOfRequests { get; set; }
        public int receiptTime { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int intensityFlowApplications = 3500;
            int serviceFlowRate = 100;

            InfoAboutServer infoAboutServer = new InfoAboutServer { numberOfThreads = 5, timeOfProcessing = intensityFlowApplications };
            InfoAboutAlgo infoAboutAlgo = new InfoAboutAlgo { numberOfRequests = 100, receiptTime = serviceFlowRate };

            Server server = new Server(infoAboutServer);
            Client client = new Client(server);

            for (int id = 1; id <= infoAboutAlgo.numberOfRequests; id++)
            {
                client.send(id);
                Thread.Sleep(infoAboutAlgo.receiptTime);
            }
            Thread.Sleep(1000);

            Console.WriteLine();
            Console.WriteLine("Number of applications received: {0}", server.serverStatistics.request);
            Console.WriteLine("Number of processed applications: {0}", server.serverStatistics.processed);
            Console.WriteLine("Number of rejected applications: {0}", server.serverStatistics.rejected);
            Console.WriteLine("-------");
            AnalyzeWork analyze = new AnalyzeWork(intensityFlowApplications, serviceFlowRate, infoAboutServer.numberOfThreads);
            Console.WriteLine(analyze.ToString());
        }
    }

    public class AnalyzeWork
    {
        private int intensityFlowApplications;
        private int serviceFlowRate;
        private int numberOfThreads;

        public double p;
        public double p0;
        public double pn;
        public double Q;
        public double A;
        public double k;

        public AnalyzeWork(int intensityFlowApplications, int serviceFlowRate, int numberOfThreads)
        {
            this.intensityFlowApplications = intensityFlowApplications;
            this.serviceFlowRate = serviceFlowRate;
            this.numberOfThreads = numberOfThreads;

            p = getReducedFlowRate();
            p0 = systemDowntimeProbability();
            pn = systemFailureProbability();
            Q = 1 - pn;
            A = intensityFlowApplications * Q;
            k = A / serviceFlowRate;
        }

        public double getReducedFlowRate()
        {
            return intensityFlowApplications / serviceFlowRate;
        }

        public double systemDowntimeProbability()
        {
            double sum = 0;
            for (int i = 0; i <= numberOfThreads; i++)
            {
                sum += Math.Pow(p, i) / factorial(i);
            }
            return 1 / sum;
        }

        public double systemFailureProbability()
        {
            return (Math.Pow(p, numberOfThreads) / factorial(numberOfThreads)) * p0;

        }

        private int factorial(int n)
        {
            if (n == 1 || n == 0) return 1;

            return n * factorial(n - 1);
        }

        public override string ToString()
        {
            string[] str = { "p", "P0", "Pn", "Q", "A", "k" };
            double[] result = { p, p0, pn, Q, A, k };
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                sb.Append(str[i] + " = " + result[i] + "\n");
            }
            return sb.ToString();
        }
    }
}
