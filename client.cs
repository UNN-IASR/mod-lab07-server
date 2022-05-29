using System;
using server;

namespace client
{    public class Client
    {
        Server server;

        public event EventHandler<procEventArgs> request;
        public Client(Server server) {
            this.server = server;
            this.request += server.proc;
        }

        public virtual void OnProc(procEventArgs e){
            Console.WriteLine("Req getting ready");
            EventHandler<procEventArgs> handler = request;
            if (handler != null) {
                handler(this, e);
            }
        }
    }
}