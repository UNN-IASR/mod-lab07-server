using System;
using System.IO;
using System.Threading;

namespace Lab07
{
    internal class Program
    {
        public class procEventArgs : EventArgs
        {
            public int id { get; set; }

        }
        public class Server
        {
            static int threadsCount;
            public int requestCount;
            public int processedCount;
            public int rejectedCount;
            int delay;
            PoolRecord[] pool;
            object threadLock = new object();
            public Server(int threadsCount, int delay)
            {
                Server.threadsCount = threadsCount;
                requestCount = 0;
                processedCount = 0;
                rejectedCount = 0;
                pool = new PoolRecord[threadsCount]; //Создаем пул соразмерный количеству потоков
                this.delay = delay;
            }
            struct PoolRecord
            {
                public int id_request;
                public int id_thread;
                public Thread thread; //Поток
                public bool in_use; //Флаг обработки
            }
            public void process(object sender, procEventArgs e) //Метод обработки заявки
            {
                lock (threadLock)
                {
                    Console.WriteLine("Заявка с номером: {0}", e.id);
                    requestCount++; //Суммируем общее количество запросов

                    for (int i = 0; i < threadsCount; i++)
                    {
                        if (!pool[i].in_use) //Проверяем занятость потока, если не занят берем его и запускаем для обработки запроса
                        {
                            pool[i].id_request = e.id;
                            pool[i].id_thread = i;
                            pool[i].in_use = true;
                            pool[i].thread = new Thread(new ParameterizedThreadStart(Answer)); //Создаем поток и передаем ему метод Answer
                            pool[i].thread.Start(pool[i]); //Запускаем поток на обработку
                            processedCount++; //Добавляем к количеству обработанных запросов
                            return;
                        }
                    }
                    rejectedCount++; //Добавляем к количеству отклонённых запросов
                }
            }
            void Answer(object message) //Метод имитации ответа от сервера
            {
                PoolRecord pool1 = (PoolRecord)message;
                Thread.Sleep(delay); //Имитируем задержку сервера
                Console.WriteLine("Заявка {0} обработана", pool1.id_request);
                pool[pool1.id_thread].in_use = false; //Освобождаем поток
            }
        }
        public class Client
        {
            Server server;
            public event EventHandler<procEventArgs> request;
            procEventArgs arg;
            public Client(Server server)
            {
                this.server = server;
                request += server.process;
                arg = new procEventArgs();
            }
            protected virtual void OnProcess(procEventArgs e)
            {
                EventHandler<procEventArgs> handler = request;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            public void AddRequest()
            {
                arg.id++;
                OnProcess(arg);
            }
        }
        static public int Factorial(int n)
        {
            if (n == 0)
                return 1;
            else
            {
                int factorial = 1;
                for (int i = 1; i <= n; i++)
                    factorial = factorial * i;
                return factorial;
            }
        }
        static void Main()
        {
            int receiptIntensity = 10;
            int maintenanceIntensity = 2;
            int delay = 10;
            int threadsCount = 3;
            int n = 10;
            Server server = new Server(threadsCount, delay);
            Client client = new Client(server);

            for (int i = 0; i < n; i++) 
            { 
                client.AddRequest(); 
            }
            Console.WriteLine(server.processedCount.ToString());
            Console.WriteLine(server.requestCount.ToString());
            Console.WriteLine(server.rejectedCount.ToString());
            double ro = receiptIntensity / maintenanceIntensity;
            double denom = 0;
            for (int i = 0; i <= threadsCount; i++)
            {
                denom += Math.Pow(ro, i) / Factorial(i);
            }
            double P0 = 1 / denom;
            double Pn = Math.Pow(ro, threadsCount) * P0 / Factorial(threadsCount);
            double capacity = 1 - Pn;
            double absoluteCapacity = receiptIntensity * capacity;
            double busyCount = absoluteCapacity / maintenanceIntensity;
            string fale_name = "results.txt";
            StreamWriter stream = File.CreateText(fale_name);
            stream.WriteLine("Теоретические расчёты: \n" +
                            "Количество поступивших запросов {0}, из них: \n" +
                            " обработано - {1} \n" +
                            " отклонено - {2} \n" +
                            "Интенсивность потока заявок: {3} \n" +
                            "Интенсивность потока обслуживания: {4} \n" +
                            "Количество потоков: {5} \n" +
                            "Вероятность простоя: {6} \n" +
                            "Вероятность отказа: {7} \n" +
                            "Относительная пропускная способность системы: {8} \n" +
                            "Абсолютная пропускная способность системы: {9} \n" +
                            "Среднее число занятых каналов: {10}", n, server.processedCount, server.rejectedCount, receiptIntensity, maintenanceIntensity, threadsCount, P0, Pn, capacity, absoluteCapacity, busyCount);
            receiptIntensity = server.requestCount / 1;
            maintenanceIntensity = server.processedCount / threadsCount;

            ro = receiptIntensity / maintenanceIntensity;
            denom = 0;
            for (int i = 0; i <= threadsCount; i++)
            {
                denom = denom + Math.Pow(ro, i) / Factorial(i);
            }
            P0 = 1 / denom;
            Pn = Math.Pow(ro, threadsCount) * P0 / Factorial(threadsCount);
            capacity = 1 - Pn;
            absoluteCapacity = receiptIntensity * capacity;
            busyCount = absoluteCapacity / maintenanceIntensity;

            stream.WriteLine("\nФактические расчёты: \n" +
                            "Количество поступивших запросов {0}, из них: \n" +
                            " обработано - {1} \n" +
                            " отклонено - {2} \n" +
                            "Интенсивность потока заявок: {3} \n" +
                            "Интенсивность потока обслуживания: {4} \n" +
                            "Количество потоков: {5} \n" +
                            "Вероятность простоя: {6} \n" +
                            "Вероятность отказа: {7} \n" +
                            "Относительная пропускная способность системы: {8} \n" +
                            "Абсолютная пропускная способность системы: {9} \n" +
                            "Среднее число занятых каналов: {10}", n, server.processedCount, server.rejectedCount, receiptIntensity, maintenanceIntensity, threadsCount, P0, Pn, capacity, absoluteCapacity, busyCount);
            stream.Close();
        }
    }
}
