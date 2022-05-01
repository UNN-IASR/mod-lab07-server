using System;

namespace Lab7
{
    public class Client
    {
        private readonly Server server;

        public Client(Server _server)
        {
            server = _server;
            request += server.Proc;
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
