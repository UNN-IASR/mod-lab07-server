using System;
using System.Threading;
namespace TPProj {
    class Program {
        static void Main() {
            double lamda=10; //интенивность потока запросов
            double T0=0.5; //интенсивность обслужитания(среднее время обслуживания запроса)
            double P0,Pn; //вероятность простоя и вероятность отказа
            int n=3; //количество потоков
            double Q; //относительная пропускная способность
            double A; //абсолютная пропускная способность 
            double k; //среднее число занятых процессов
            double ro =lamda*T0; //приведенная интенсивность
            double temp=0;
            for (int i=0; i<=n; i++)
                temp+=Math.Pow(ro,i)/Fact(i);  
            P0=1/temp;
            Pn=Math.Pow(ro,n)/Fact(n)*P0;
            Q=1-Pn;
            A=lamda*Q;
            k=A*T0;
            Console.WriteLine("Данные вычесленные по формулам:");
            Console.WriteLine("Интенивность потока запросов:    "+lamda);
            Console.WriteLine("Интенсивность обслуживания:      "+T0);
            Console.WriteLine("Количество потоков:              "+n);
            Console.WriteLine("Вероятность простоя сервера:          "+Math.Round(P0,2));
            Console.WriteLine("Вероятность отказа сервера:           "+Math.Round(Pn,2));
            Console.WriteLine("Относительная пропускная способность:  "+Math.Round(Q,2));
            Console.WriteLine("Абсолютная пропускная способность:    "+Math.Round(A,2));
            Console.WriteLine("Среднее число занятых процессов:      "+Math.Round(k,2));


            int delayTime = (int)(T0*1000);
            int poolSize = n;
        
            Server server = new Server(poolSize,delayTime);
            Client client = new Client(server);
            for(int id=1;id<=30*n;id++) {
                client.send(id);
                Thread.Sleep((int)(1000/lamda));
            }
            Console.WriteLine("\n\nДанные вычесленные по результатам работы программы:");
            Console.WriteLine("Всего заявок:      {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок:  {0}", server.rejectedCount);
            temp = 0;
            
            ro=(double)server.requestCount/(double)server.processedCount*poolSize;
            for (int i = 0; i <= poolSize; i++)
                temp += Math.Pow(ro, i) / Fact(i);

            P0 = 1 / temp;
            Pn = Math.Pow(ro, poolSize) * P0 / Fact(poolSize);
            // double P0=(double)server.processedCount/(double)server.requestCount;
            // double Pn=(double)server.rejectedCount/(double)server.requestCount;
            Console.WriteLine("Интенивность потока запросов:    "+lamda);
            Console.WriteLine("Интенсивность обслужитания:      "+T0);
            Console.WriteLine("Количество потоков:              "+n);
            Console.WriteLine("Вероятность простоя сервера:          "+Math.Round(P0,3));
            Console.WriteLine("Вероятность отказа сервера:           "+Math.Round(Pn,3));
            Console.WriteLine("Относительная пропускная способность:  "+Math.Round(1-Pn,2));
            Console.WriteLine("Абсолютная пропускная способность:    "+Math.Round((1-Pn)*lamda,2));
            Console.WriteLine("Среднее число занятых процессов:      "+Math.Round((1-Pn)*lamda*T0,2));
        }
        public static int Fact(int n) {
        if(n == 0)
            return 1;
        else
            return n * Fact(n - 1);
    }
    }

    struct PoolRecord {
        public Thread thread;
        public bool in_use;
    }
    
    class Server {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public int delayTime;
        private int poolSize;   
        public Server(int s,int time) {
            pool = new PoolRecord[s];
            delayTime = time;
            poolSize=s;
        }
        public void proc(object sender, procEventArgs e) {
            lock(threadLock) {
                //Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for(int i = 0; i < poolSize; i++) {
                if(!pool[i].in_use) {
                    pool[i].in_use = true;
                    pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                    pool[i].thread.Start(e.id);
                    processedCount++;
                    return;
                }
                }
                rejectedCount++;
            }
        }
    public void Answer(object arg) {
            int id = (int)arg;
            //Console.WriteLine("Обработка заявки: {0}",id);
            Thread.Sleep(delayTime);
            for(int i = 0; i < poolSize; i++)
            if(pool[i].thread==Thread.CurrentThread)
            pool[i].in_use = false;
        }
    }
    class Client {
        private Server server;
        public Client(Server server) {
            this.server=server;
            this.request += server.proc;
        }
        public void send(int id) {
            procEventArgs args = new procEventArgs();
            args.id = id;
            OnProc(args);
        }
        protected virtual void OnProc(procEventArgs e){
            EventHandler<procEventArgs> handler = request;
            if (handler != null) {
                handler(this, e);
            }
        }
    public event EventHandler<procEventArgs> request;
    }
    public class procEventArgs : EventArgs {
    public int id { get; set; }
}
}
