using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Lab07
{
    delegate void WorkWithClient(object id, object index);

    public struct InfoAboutServerWork
    {
        public int request { get; set; }
        public int processed { get; set; }
        public int rejected { get; set; }
    }

    public struct InfoAboutServer
    {
        public int numberOfThreads { get; set; }
        public int timeOfProcessing { get; set; }
    }

    public struct InfoAboutClient
    {
        public int id { get; set; }
        public int threadIndex { get; set; }
    }

    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }

    public class Server
    {
        public InfoAboutServer serverInfo;
        public InfoAboutServerWork serverStatistics;
        PoolRecord[] pool;
        object threadLock = new object();

        public Server(InfoAboutServer infoAboutServer)
        {
            serverInfo = infoAboutServer;
            pool = new PoolRecord[serverInfo.numberOfThreads];
        }

        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Application number received: " + e.id);
                serverStatistics.request++;

                for (int i = 0; i < serverInfo.numberOfThreads; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(new InfoAboutClient { id = e.id, threadIndex = i });
                        serverStatistics.processed++;
                        return;
                    }
                }
                serverStatistics.rejected++;
            }
        }

        public void Answer(object obj)
        {
            int client_id = ((InfoAboutClient)obj).id;
            int indexOfThread = ((InfoAboutClient)obj).threadIndex;
            Console.WriteLine("Client ID request is " + client_id + " served by the thread: " + indexOfThread);
            Thread.Sleep(serverInfo.timeOfProcessing);
            pool[indexOfThread].in_use = false;
        }
    }
}
