using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab
{
    internal class Client
    {
        private Server server;

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
                handler(this, e);
        }
        public event EventHandler<procEventArgs> request;
    }
}
