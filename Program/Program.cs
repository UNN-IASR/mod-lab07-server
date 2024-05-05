using System;
using System.Threading;

namespace TTproj
{
  class Program {
    static void Main() {
    
    int countpotokov = 5;
    int timeserver= 500;
    int timeclient =50;
    
    Server server = new Server(countpotokov, timeserver);
    Client client = new Client(server);
    for(int id=1;id<=100;id++)
    {
      client.send(id);
      Thread.Sleep(timeclient);
    }
    Console.WriteLine("Всего заявок: {0}", server.requestCount);
    Console.WriteLine("Обработано заявок: {0}", server.processedCount);
    Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);
    
    //переводим в секунды
    double timeserver1 = (double)timeserver/1000.00;
    double timeclient1 = (double)timeclient/1000.00;
    
    double intensivnost_potoka_zayavok = (double)1.00/timeclient1;
    Console.WriteLine("Интенсивность потока заявок " + intensivnost_potoka_zayavok);
    double intensivnost_potoka_obslugivania = (double)1.00/timeserver1;
    Console.WriteLine("Интенсивность потока обслуживания " + intensivnost_potoka_obslugivania);
    
    double p = (double)intensivnost_potoka_zayavok/intensivnost_potoka_obslugivania;
    
    long factorial (int c)
    {
      long x = 1;
      for (int i = 1; i <= c; i++)
      x *= i;
      return x;
    }
    double veroyatnost_prostoya_sistemi = 0;
    for (int i=0; i <= countpotokov; i++)
    {
      veroyatnost_prostoya_sistemi += Math.Pow(p,i)/factorial(i);
    }
    veroyatnost_prostoya_sistemi = Math.Pow(veroyatnost_prostoya_sistemi,-1);
    Console.WriteLine("Вероятность простоя системы " + Math.Round(veroyatnost_prostoya_sistemi,2));
    
    double veroyatnost_otkaza_sistemi = ((double)(Math.Pow(p,countpotokov))/factorial(countpotokov))*veroyatnost_prostoya_sistemi;
    Console.WriteLine("Вероятность отказа системы " + Math.Round(veroyatnost_otkaza_sistemi,2));
    
    double otnositelnaya_propusknaya_sposobnost = 1-veroyatnost_otkaza_sistemi;
    Console.WriteLine("Относительная пропускная способность " + Math.Round(otnositelnaya_propusknaya_sposobnost,2));
    
    double absolutnaya_propusknaya_sposobnost = intensivnost_potoka_zayavok*otnositelnaya_propusknaya_sposobnost;
    Console.WriteLine("Абсолютная пропускная способность " + Math.Round(absolutnaya_propusknaya_sposobnost,2));
    
    double srednee_chislo_zanyatix_kanalov = (double)absolutnaya_propusknaya_sposobnost/intensivnost_potoka_obslugivania;
    Console.WriteLine("Среднее число занятых каналов " + Math.Round(srednee_chislo_zanyatix_kanalov,2));
    
    }
  }
  struct PoolRecord
  {
    public Thread thread;
    public bool in_use;
  }
  class Server {
    private PoolRecord[] pool;
    private object threadLock = new object();
    public int requestCount = 0;
    public int processedCount = 0;
    public int rejectedCount = 0;
    
    public int countpotokov;
    public int timeserver;
    
    public Server(int countpotokov, int timeserver) {
      this.countpotokov=countpotokov;
      this.timeserver = timeserver;
      pool=new PoolRecord[countpotokov];
      for (int i=0;i<countpotokov;++i)
      {
        pool[i].in_use = false;
      }
    }
    public void proc(object sender, procEventArgs e)
    {
      lock(threadLock)
      {
      Console.WriteLine("Заявка с номером: {0}", e.id);
      requestCount++;
      for(int i = 0; i < countpotokov; i++)
      {
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
      Console.WriteLine("Обработка заявки: {0}",id);
      Thread.Sleep(timeserver);
      for(int i = 0; i < countpotokov; i++)
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
