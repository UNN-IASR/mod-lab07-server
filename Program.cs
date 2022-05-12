using System;
using System.Threading;

namespace sh7
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server(5, 200);
            CL c = new CL(s);

            for (int i = 1; i <= 100; i++)
            {
                c.PUL(i);
                Thread.Sleep(40);
            }

            Console.WriteLine();
            Console.WriteLine("Результат:");
            Console.WriteLine("Все запросы: {0}", s.getCrec());
            Console.WriteLine("Обработанные: {0}", s.getCP());
            Console.WriteLine("Необработанные: {0}", s.getCrej());
        }
    }
}
