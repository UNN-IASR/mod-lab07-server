using System;
using System.Collections.Generic;
using System.Text;

namespace sh7
{
    internal class CL
    {
        private Server s;
        public CL(Server s)
        {
            this.s = s;
            request += s.PUL;
        }

        public void PUL(int num)
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
