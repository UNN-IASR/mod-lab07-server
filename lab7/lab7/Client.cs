using System;

namespace lab7
{
    internal class Client
    {
        private Server server;
        public Client(Server server)
        {
            this.server = server;
            request += server.proc;
        }

        public void proc(int num)
        {
            procEventArgs args = new procEventArgs();

            args.id = num;

            if (request != null)
            {
                request(this, args);
            }
        }

        public event EventHandler<procEventArgs> request;
    }
}

