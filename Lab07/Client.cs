using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Lab07
{
    public class Client
    {
        private Server server;
        public event EventHandler<procEventArgs> request;

        public Client(Server server)
        {
            this.server = server;
            request += server.proc;
        }

        public void send(int id)
        {
            procEventArgs args = new procEventArgs();
            args.id = id;
            if (request != null)
            {
                request(this, args);
            }
        }
    }
}
