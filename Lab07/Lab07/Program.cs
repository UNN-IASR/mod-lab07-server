using System;
using System.IO;
using System.Threading;

namespace ClientServerSimulation
{
    class Program
    {
        static void Main()
        {
            // Параметры моделирования
            int poolSize = 5; // Количество потоков в пуле (число каналов)
            int requestIntensity = 50; // Интервал между заявками в миллисекундах
            int serviceIntensity = 500; // Время обработки одной заявки в миллисекундах

            // Интенсивность потока заявок (λ)
            double lambda = 1000.0 / requestIntensity;

            // Интенсивность обслуживания (µ)
            double mu = 1000.0 / serviceIntensity;

            // Создание объекта сервера и клиента
            Server server = new Server(poolSize, serviceIntensity);
            Client client = new Client(server);

            // Генерация заявок
            for (int id = 1; id <= 100; id++)
            {
                client.Send(id);
                Thread.Sleep(requestIntensity); // Интервал между заявками
            }

            // Вывод результатов моделирования
            Console.WriteLine("Всего заявок: {0}", server.RequestCount);
            Console.WriteLine("Обработано заявок: {0}", server.ProcessedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.RejectedCount);

            // Вычисление теоретических показателей СМО
            double rho = lambda / mu; // Приведенная интенсивность потока заявок
            double P0 = server.CalculateIdleProbability(poolSize, rho); // Вероятность простоя системы
            double Pn = server.CalculateFailureProbability(poolSize, rho, P0); // Вероятность отказа системы
            double Q = 1 - Pn; // Относительная пропускная способность
            double A = lambda * Q; // Абсолютная пропускная способность
            double k = A / mu; // Среднее число занятых каналов

            // Вычисление фактических показателей СМО
            double actualP0 = (double)server.IdleCount / server.RequestCount; // Фактическая вероятность простоя системы
            double actualPn = (double)server.RejectedCount / server.RequestCount; // Фактическая вероятность отказа системы
            double actualQ = (double)server.ProcessedCount / server.RequestCount; // Фактическая относительная пропускная способность
            double actualA = actualQ * lambda; // Фактическая абсолютная пропускная способность
            double actualK = actualA / mu; // Фактическое среднее число занятых каналов

            // Создание таблицы результатов
            string[] results = {
                "Сравнение теоретических и фактических показателей СМО:",
                "---------------------------------------------------------------------------------------------",
                "| Показатель                          | Теоретическое значение   | Фактическое значение     |",
                "---------------------------------------------------------------------------------------------",
                $"| Вероятность простоя системы         | {P0:F4}                   | {actualP0:F4}                   |",
                $"| Вероятность отказа системы          | {Pn:F4}                   | {actualPn:F4}                   |",
                $"| Относительная пропускная способность| {Q:F4}                   | {actualQ:F4}                   |",
                $"| Абсолютная пропускная способность   | {A:F2}                     | {actualA:F2}                    |",
                $"| Среднее число занятых каналов       | {k:F2}                     | {actualK:F2}                     |",
                "---------------------------------------------------------------------------------------------"
            };

            // Вывод таблицы в консоль
            foreach (string line in results)
            {
                Console.WriteLine(line);
            }

            // Сохранение таблицы в файл
            File.WriteAllLines("./results.txt", results);
        }
    }

    struct PoolRecord
    {
        public Thread Thread; // Поток, обслуживающий заявку
        public bool InUse; // Флаг занятости потока
    }

    class Server
    {
        private PoolRecord[] pool; // Пул потоков
        private object threadLock = new object(); // Объект для синхронизации потоков
        private int serviceIntensity; // Время обработки одной заявки

        public int RequestCount { get; private set; } = 0; // Общее количество заявок
        public int ProcessedCount { get; private set; } = 0; // Количество обработанных заявок
        public int RejectedCount { get; private set; } = 0; // Количество отклоненных заявок
        public int IdleCount { get; private set; } = 0; // Количество раз, когда все потоки были свободны

        public Server(int poolSize, int serviceIntensity)
        {
            pool = new PoolRecord[poolSize]; // Инициализация пула потоков
            this.serviceIntensity = serviceIntensity; // Установка времени обработки заявки
        }

        // Метод обработки входящей заявки
        public void ProcessRequest(object sender, ProcEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Получена заявка с номером: {0}", e.Id);
                RequestCount++;
                for (int i = 0; i < pool.Length; i++)
                {
                    if (!pool[i].InUse)
                    {
                        pool[i].InUse = true;
                        pool[i].Thread = new Thread(Answer);
                        pool[i].Thread.Start(e.Id);
                        ProcessedCount++;
                        return;
                    }
                }
                RejectedCount++;
            }
        }

        // Метод имитации обработки заявки (временная задержка)
        private void Answer(object arg)
        {
            int id = (int)arg;
            Console.WriteLine("Обработка заявки: {0}", id);
            Thread.Sleep(serviceIntensity); // Задержка для имитации обработки заявки
            lock (threadLock)
            {
                for (int i = 0; i < pool.Length; i++)
                {
                    if (pool[i].Thread == Thread.CurrentThread)
                    {
                        pool[i].InUse = false;
                        IdleCount++;
                        break;
                    }
                }
            }
        }

        // Метод вычисления вероятности простоя системы (P0)
        public double CalculateIdleProbability(int poolSize, double rho)
        {
            double sum = 0;
            for (int i = 0; i <= poolSize; i++)
            {
                sum += Math.Pow(rho, i) / Factorial(i);
            }
            return 1 / sum;
        }

        // Метод вычисления вероятности отказа системы (Pn)
        public double CalculateFailureProbability(int poolSize, double rho, double P0)
        {
            return (Math.Pow(rho, poolSize) / Factorial(poolSize)) * P0;
        }

        // Метод вычисления факториала
        private int Factorial(int n)
        {
            if (n == 0 || n == 1)
                return 1;
            int result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
    }

    class Client
    {
        private Server server; // Объект сервера

        public Client(Server server)
        {
            this.server = server;
            this.Request += server.ProcessRequest;
        }

        // Метод отправки заявки
        public void Send(int id)
        {
            ProcEventArgs args = new ProcEventArgs { Id = id };
            OnRequest(args);
        }

        // Метод вызова события заявки
        protected virtual void OnRequest(ProcEventArgs e)
        {
            EventHandler<ProcEventArgs> handler = Request;
            handler?.Invoke(this, e);
        }

        // Событие отправки заявки
        public event EventHandler<ProcEventArgs> Request;
    }

    public class ProcEventArgs : EventArgs
    {
        public int Id { get; set; } // Идентификатор заявки
    }
}
