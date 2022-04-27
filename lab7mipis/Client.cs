using System;

namespace lab7mipis
{
    public class Client
    {
        private Server server;

        public Client(Server _server)
        {
            server = _server;
            request += server.proc;
        }

        public virtual void OnProc(int Id)
        {
            procEventArgs handler = new procEventArgs
            {
                id = Id
            };

            request?.Invoke(this, handler);
        }

        public event EventHandler<procEventArgs> request;

    }
}