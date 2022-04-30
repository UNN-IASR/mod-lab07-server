using System;
using System.Collections.Generic;
using System.Text;


namespace lab07_server
{
    class Client
    {
        public event EventHandler<procEventArgs> request;
        private Server server;
        public Client(Server server)
        {
            this.server = server;
            this.request += server.proc;
        }
        protected virtual void OnProc(procEventArgs e)
        {
            EventHandler<procEventArgs> handler = request;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Send(int id)
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
