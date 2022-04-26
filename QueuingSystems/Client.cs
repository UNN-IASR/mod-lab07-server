using System;
using System.Threading.Tasks;

namespace QueuingSystems
{
  internal class Client
  {
    private Server myServer;

    public Client(Server theServer)
    {
      myServer = theServer;
      myRequest += myServer.Process;
    }

    public void Send(int theId)
    {
      ProcessEventArgs anArgs = new ProcessEventArgs();

      anArgs.Id = theId;

      if (myRequest != null)
      {
        myRequest(this, anArgs);
      }
    }

    public event EventHandler<ProcessEventArgs> myRequest;
  }
}
