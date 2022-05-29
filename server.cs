using System;
using System.Threading;

namespace server
{
    public class procEventArgs: EventArgs
    {
        public procEventArgs(int id){
            this.id = id;
        }
        public int id {get; set;}
    }

    struct PoolRecord 
    {
        public Thread thread;
        public bool in_use;
    }
    public struct ServerStat
    {
        public int processedCount;
        public int rejectedCount;
        public int requestCount;
        public int plain;
    }

    public class Server
    {
        PoolRecord[] pool;
        object threadlocker = new object();
        public ServerStat stats = new ServerStat();
        static int process_length = 10;
        static int thrd_monitoring_interval = 5;
        Thread thrd_monitor;
        bool is_running = false;
        public Server(int n){
            pool = new PoolRecord[n];
            is_running = true;
            thrd_monitor = new Thread(this.thrd_monitoring);
            thrd_monitor.Start();
        }
        public void Stop(){
            is_running = false;
        } 
        //Process assigned request
        public static void answer(object id){
            Console.WriteLine("Request number {0} processed", id);
            Thread.Sleep(process_length);
        }
        void thrd_monitoring(){
            while(is_running){
                for(int i = 0; i < pool.Length; i++) {
                    if (pool[i].in_use) {
                        if (!pool[i].thread.IsAlive) {
                            pool[i].in_use = false;
                            stats.plain += thrd_monitoring_interval;
                        }
                    }
                    else {
                        stats.plain += thrd_monitoring_interval;
                    }
                }
                Thread.Sleep(thrd_monitoring_interval);
            }
        }
        public void proc(object sender, procEventArgs e){
            lock(threadlocker) {
                Console.WriteLine("Request number {0} incoming", e.id);
                stats.requestCount++;
                // Try to assign request
                for(int i = 0; i < pool.Length; i++) {
                    if(!pool[i].in_use) {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(Server.answer);
                        pool[i].thread.Start(e.id);
                        stats.processedCount++;
                        return;
                    }
                }
                // If request is rejected
                Console.WriteLine("Request number {0} rejected", e.id);
                stats.rejectedCount++;
            }
        }
    }
}