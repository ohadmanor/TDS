using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TDSServer
{
    public class SessionManager : IDisposable
    {
       
        public GameManager gameManager;
       

        public SessionManager()
        {
            Initialize();
        }
        public void Initialize()
        {
            gameManager = new GameManager();
        }


        public void Dispose()
        {
            gameManager.Dispose();
        }
    }
}
