using System;
using client;
using server;
using System.Threading;

namespace mod_task07_server
{
    
    class Program
    {   
        static int factorial(int x)
        {
            if (x == 0)
            {
                return 1;
            }
            else
            {
                return x * factorial(x - 1);
            }
        }
        static int get_uniform_sample(int low_bound, int high_bound){
            Random random = new Random();
            return random.Next(low_bound, high_bound);
        }
        static void Main(string[] args)
        {
            int total_time = 10000;
            int number_of_threads = 10;
            double intensity =  (double)1 / 10;
            Server server = new Server(number_of_threads);

            int one_time_unit = 100;
            int req_per_one_unit = (int)Math.Round(one_time_unit * intensity);
            //One unit of time
            int cur_time = 0;
            int process_counter = 0;
            int generated_processes = 0;
            while(cur_time < total_time){
                generated_processes = get_uniform_sample(0, req_per_one_unit * 2);
                for(int i = 0; i < generated_processes; i++){
                    Client client = new Client(server);
                    procEventArgs request = new procEventArgs(process_counter);
                    client.OnProc(request);
                    process_counter ++;
                }
                cur_time += one_time_unit;
                Thread.Sleep(one_time_unit);
            }
            server.Stop();
            Console.WriteLine($"Total requests (intensivity): {server.stats.requestCount} Processed (intensivity): {server.stats.processedCount} Rejected: {server.stats.rejectedCount} Plain: {server.stats.plain}");
            double p = (double) server.stats.requestCount / server.stats.processedCount;
            double p0 = 0; 
            for(int i = 0; i < number_of_threads; i++) {
                p0 += Math.Pow(p, i) / factorial(i);
            }
            p0 = Math.Pow(p0, -1);
            Console.WriteLine($"Probabiblity of plain: {p0}");
            Console.WriteLine($"Probabiblity of rejection: {Math.Pow(p, number_of_threads) / factorial(number_of_threads) * p0}");
            Console.WriteLine($"Relative capacity {1 - Math.Pow(p, number_of_threads) / factorial(number_of_threads) * p}");
            Console.WriteLine($"Absolute capacity {server.stats.requestCount * (1 - Math.Pow(p, number_of_threads) / factorial(number_of_threads) * p)}");
            Console.WriteLine($"Mean busy threads {server.stats.requestCount * (1 - Math.Pow(p, number_of_threads) / factorial(number_of_threads) * p) / server.stats.processedCount}");
        }
    }
}
