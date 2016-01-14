using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;


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
            string strRoadRoutingWebApiAddress = ConfigurationManager.AppSettings["RoadRoutingWebApiAddress"];
            clsRoadRoutingWebApi.SetBaseAddress(strRoadRoutingWebApiAddress);


            gameManager = new GameManager();
        }


        public void Dispose()
        {
            gameManager.Dispose();
        }
    }
}
