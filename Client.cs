using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
namespace Lab07
{
    class Client
    {
        int port; // порт сервера
        static string address = "100.75.31.173"; // адрес сервера
        public float intensity = 1.0f;
        int currentMessageID = 0;
        int handledMessages = 0;
        int ignoredMessages = 0;
        Server server;
        public void Run(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                Thread.Sleep((int)(1000 / intensity));
                try
                {

                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address),
                        port);
                    Socket socket = new Socket(AddressFamily.InterNetwork,
                                                SocketType.Stream,
                                                ProtocolType.Tcp);

                    // подключаемся к удаленному хосту
                    socket.Connect(ipPoint);
                    Console.WriteLine("Send message");
                    string message = "Message number " + currentMessageID.ToString();
                    currentMessageID++;
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    socket.Send(data);
                    // получаем ответ
                    data = new byte[256]; // буфер для ответа
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байт

                    do
                    {
                        bytes = socket.Receive(data, data.Length, 0);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (socket.Available > 0);

                    bool handled = builder.ToString().Equals("handled");
                    Console.WriteLine("ответ сервера: " + builder.ToString());

                    if (handled) handledMessages++;
                    else ignoredMessages++;

                    // закрываем сокет
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public Client(Server server)
        {
            port = server.port;
            this.server = server;
        }
        public void Statistics(out int rejected, out int handled)
        {
            server.running = false;
            rejected = ignoredMessages;
            handled = handledMessages;
        }
    }
}