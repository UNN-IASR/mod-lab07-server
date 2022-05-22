using System;
using System.Collections.Generic;
using System.Text;

namespace MSS
{
    class Client
    {
        public Client(UInt32 id, Server server)
        {
            this.id = id;
            this.server = server;
            Notify += server.handleCall;
        }

        public void call()
        {
            bool status = false;
            if(Notify != null)
            {
                status = Notify.Invoke(this.id);
            }
            if(!status)
            {
                Console.WriteLine($"<{id}>: Server declined call request");
            }
        }

        private delegate bool CallHandler(UInt32 id);
        private event CallHandler Notify;
        private Server server;
        private UInt32 id;
    }
}
