namespace Lab07
{
    internal class Program
    {
        static public int Factorial(int f)
        {
            if (f == 0)
                return 1;
            else
                return f * Factorial(f - 1);
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            const int client_intensity = 7;
            const int server_intensity = 3;
            const int n = 2;
            Server server = new Server(n, server_intensity);
            Thread myThread = new Thread(server.Run);
            myThread.Start();
            Client client = new Client(server);
            client.intensity = client_intensity;
            client.Run(100);
            int rejected;
            int handled;
            client.Statistics(out rejected, out handled);
            int idle;
            int sum_active_threads;
            int overall_responded;
            server.Statistics(out idle, out sum_active_threads, out overall_responded);

            Console.WriteLine($"rejected {rejected}");
            Console.WriteLine($"handled {handled}");
            Console.WriteLine($"idle {idle}");
            Console.WriteLine($"sum_active_threads {sum_active_threads}");
            Console.WriteLine($"overall_responded {overall_responded}");
            
            Console.WriteLine();

            Console.WriteLine($"Вероятность простоя системы: {(float)idle / overall_responded}");
            Console.WriteLine($"Вероятность отказа системы: {(float)rejected / (rejected + handled)}");
            Console.WriteLine($"Относительная пропускная способность: {(float)handled / (rejected + handled)}");
            Console.WriteLine($"Абсолютная пропускная способность: {(float)handled * client_intensity / (rejected + handled)}");
            Console.WriteLine($"Среднее число занятых каналов: {(float)sum_active_threads / overall_responded}");

            Console.WriteLine("\nОжидаемые:\n");
            double v1 = 0;
            for (int i = 0; i <= n; i++)
            {
                v1 += Math.Pow(((float)client_intensity / server_intensity), i) / Factorial(i);
            }
            v1 = 1 / v1;
            Console.WriteLine($"Вероятность простоя системы: {v1}");

            double v2 = v1 * Math.Pow(((float)client_intensity / server_intensity), n) / Factorial(n);
            Console.WriteLine($"Вероятность отказа системы: {v2}");

            double v3 = 1 - v2;
            Console.WriteLine($"Относительная пропускная способность: {v3}");

            double v4 = v3 * client_intensity;
            Console.WriteLine($"Абсолютная пропускная способность: {v4}");

            double v5 = v4 / server_intensity;
            Console.WriteLine($"Среднее число занятых каналов: {v5}");
        }
    }
}
