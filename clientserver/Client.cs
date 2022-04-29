using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clientserver7
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
