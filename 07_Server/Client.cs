using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace _07_Server
{

        public class Client
        {
            public Server server { get; set; }
            public event EventHandler<procEventArgs> request;

            public Client(Server server)
            {
                this.server = server;
                this.request += server.proc;
            }
            public void send(int id)
            {
                procEventArgs args = new procEventArgs();
                args.id = id;
                OnProc(args);
            }
            protected virtual void OnProc(procEventArgs e)
            {
                EventHandler<procEventArgs> handler = request;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

        }

}
