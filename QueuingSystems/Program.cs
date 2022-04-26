using System;
using System.Threading;

namespace QueuingSystems
{
  class Program
  {
    static void Main(string[] args)
    {
      int aRequestFlowRate = 150;
      int aServiceFlowRate = 1000;

      Server aServer = new Server(5, aServiceFlowRate);
      Client aClient = new Client(aServer);

      for (int anId = 1; anId <= 100; anId++)
      {
        aClient.Send(anId);
        Thread.Sleep(aRequestFlowRate);
      }

      Console.WriteLine($"Total requests: {aServer.myRequestCount}");
      Console.WriteLine($"Processed requests: {aServer.myProcessedCount}");
      Console.WriteLine($"Rejected requests: {aServer.myRejectedCount}");
    }
  }
}
