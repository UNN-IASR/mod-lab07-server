using System;
using System.Threading;

namespace QueuingSystems
{
  struct RecordPool
  {
    public bool inUse;
    public Thread thread;
  }

  internal class ProcessEventArgs: EventArgs
  {
    public int Id { get; set; }
  }
  internal class Server
  {
    private RecordPool[] myRecordPool;
    private object myThreadLock = new object();

    public int myProcessingTime;

    public int myRequestCount = 0;
    public int myProcessedCount = 0;
    public int myRejectedCount = 0;

    public Server(int theChannelsNumber, int theProcessingTime)
    {
      myRecordPool = new RecordPool[theChannelsNumber];
      myProcessingTime = theProcessingTime;
    }

    public void Process(object theSender, ProcessEventArgs e)
    {
      lock(myThreadLock)
      {
        Console.WriteLine($"Request number: {e.Id}");
        myRequestCount++;

        for (int i = 0; i < myRecordPool.Length; ++i)
        {
          if (!myRecordPool[i].inUse)
          {
            myRecordPool[i].inUse = true;
            myRecordPool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
            myRecordPool[i].thread.Start(e.Id);
            myProcessedCount++;
            return;
          }
        }

        myRejectedCount++;
      }
    }

    public void Answer(object theArg)
    {
      int anId = (int)theArg;

      Console.WriteLine($"Processing request: {anId}");
      Thread.Sleep(myProcessingTime);

      for (int i = 0; i < myRecordPool.Length; ++i)
      {
        if (myRecordPool[i].thread == Thread.CurrentThread)
        {
          myRecordPool[i].inUse = false;
        }
      }
    }
  }
}
