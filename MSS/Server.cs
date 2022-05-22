using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace MSS
{
    class Server
    {
        protected class statistics
        {
            public Stopwatch InactiveTimer = new Stopwatch();
            public UInt32 RequestRecieved = 0;
            public UInt32 RequestHandled = 0;
        }

        protected class eventHandler
        {
            private UInt32 id;
            private bool isAvailable = true;
            private Thread thread;
            private statistics stat = new statistics();
            private int Delay;
            public Mutex mut = new Mutex();

            public eventHandler(UInt32 id, int Delay)
            {
                this.id = id;
                this.Delay = Delay;
            }

            public void handle(UInt32 id)
            {
                if(thread != null && thread.IsAlive)
                {
                    thread.Join();
                }

                isAvailable = false;
                thread = new Thread(Run);
                thread.Start(id);

                stat.RequestRecieved++;
            }
            private void Run(object id)
            {
                stat.InactiveTimer.Stop();
                Console.WriteLine($"Client with id <{(UInt32)id}> is handled by handler <{this.id}>");
                Thread.Sleep(Delay);
                mut.WaitOne();
                isAvailable = true;
                stat.RequestHandled++;
                stat.InactiveTimer.Start();
                mut.ReleaseMutex();
            }

            public UInt32 ID
            {
                get { return id; }
            }
            public statistics Statistics
            {
                get { return stat; }
            }
            public bool IsAvailable
            {
                get { return isAvailable; }
            }
        }
        
        public Server(UInt32 HandlersCount, int HandlingDelay)
        {
            size = HandlersCount;
            for(UInt32 i = 0; i <HandlersCount; i++)
            {
                handlers.Add(new eventHandler(i, HandlingDelay));
            }
        }

        public bool handleCall(UInt32 id)
        {
            stat.RequestRecieved++;
            foreach (var handler in handlers)
            {
                handler.mut.WaitOne();
                if(handler.IsAvailable)
                {
                    handler.handle(id);
                    handler.mut.ReleaseMutex();
                    return true;
                }
                handler.mut.ReleaseMutex();
            }
            return false;
        }

        public void PrintStatistics()
        {
            statistics stat;
            Console.WriteLine("Server work statistics:");
            foreach(var handler in handlers)
            {
                stat = handler.Statistics;
                Console.WriteLine($"Handler <{handler.ID}>:");
                Console.WriteLine($"Request recieved: {stat.RequestRecieved}");
                Console.WriteLine($"Request handled: {stat.RequestHandled}");
                Console.WriteLine($"Inactive time: {stat.InactiveTimer.ElapsedMilliseconds} milliseconds");
                this.stat.RequestHandled += stat.RequestHandled;
            }
            Console.WriteLine("Server work statistics:");
            Console.WriteLine($"Total request recieved: {this.stat.RequestRecieved}");
            Console.WriteLine($"Total request handled: {this.stat.RequestHandled}");
            Console.WriteLine($"Total request declined: {this.stat.RequestRecieved - this.stat.RequestHandled}");
        }

        private statistics stat = new statistics();
        private UInt32 size;
        private List<eventHandler> handlers = new List<eventHandler>();
    }
}
