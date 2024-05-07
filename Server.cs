using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Lab07
{
    struct Message
    {
        public string message;
        public int id;
    }
    class MessageHandler
    {
        public delegate void StringHandler(Message message);
        public event StringHandler? Notify;
        int id;
        bool free = true;
        int wait_msec;
        public MessageHandler(int id, int wait_msec)
        {
            this.id = id;
            this.wait_msec = wait_msec;
        }
        public void Run(string message)
        {
            string current_message = message;
            free = false;
            Thread.Sleep(wait_msec);
            Message msg = new Message()
            { 
                message = current_message,
                id = id
            };
            free = true;
            Notify.Invoke(msg);
        }
        public bool isFree()
        {
            return free;
        }
    }

    class Server
    {
        public readonly int port = 8005;
        Thread[] threads;
        MessageHandler[] handlers;
        public delegate void StringHandler(Message message);
        public event StringHandler? Notify;
        string current = "";
        public bool running = false;
        int overall_idle = 0;
        int sum_active_threads = 0;
        int overall_responded = 0;

        public Server(int threadCount, int intensity)
        {
            threads = new Thread[threadCount];
            handlers = new MessageHandler[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                handlers[i] = new MessageHandler(i, (int)(1000 / intensity));
                handlers[i].Notify += MessageDone;
                var halr = handlers[i];
                threads[i] = new Thread(() => halr.Run(current));
            }
        }
        public void MessageDone(Message message)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + message.message + " by thread " + message.id);
            int id = message.id;
            var halr = handlers[id];
            threads[id] = new Thread(() => halr.Run(current));
        }
        public void Run()
        { // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, port);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Stream,
                                            ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой,
                // по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                Console.WriteLine("Сервер запущен. Ожидание подключений...");
                running = true;
                while (running)
                {
                    Socket handler = listenSocket.Accept();
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных
                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);

                    bool idle = true;
                    int active_threads = 0;
                    foreach (MessageHandler hler in handlers)
                    {
                        if (!hler.isFree())
                            active_threads++;
                        else
                            idle = false;
                    }

                    overall_responded++;
                    if (idle)
                        overall_idle++;
                    sum_active_threads += active_threads;

                    bool handled = false;
                    current = builder.ToString();
                    for (int i = 0; i < threads.Length; i++)
                    {
                        if (handlers[i].isFree() && !threads[i].IsAlive)
                        {
                            handled = true;
                            threads[i].Start();
                            break;
                        }
                    }

                    string response;
                    if (handled)
                        response = "handled";
                    else
                        response = "not handled";

                    // отправляем отчёт
                    Console.WriteLine(response);
                    data = Encoding.Unicode.GetBytes(response);
                    handler.Send(data);
                    // закрываем сокет
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void Statistics(out int idle, out int sum_active_threads, out int overall_responded)
        {
            idle = this.overall_idle;
            sum_active_threads = this.sum_active_threads;
            overall_responded = this.overall_responded;
        }
    }
}
