using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Configuration;
using System.Collections.Specialized;

using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;

namespace TDSServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
          
            string SocketPort = string.Empty;
            try
            {              


                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                SocketPort = appSettings["SocketPort"];

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Loading Config File \n " +                                
                                 "DTS Server Will Be Closed ", "DTS Server Error");
                return;

            }

            string url = string.Empty;
            if (string.IsNullOrEmpty(SocketPort))
            {
                url = "http://+:8070";
            }
            else
            {
                url = "http://+:" + SocketPort;
            }
            WebApp.Start(url);
            Form1 frm = new Form1();
            SessionManager sessionManager = new SessionManager();
            frm.sessionManager = sessionManager;
            GlobalHost.DependencyResolver.Register(typeof(SessionManager), () => sessionManager);

            Application.Run(frm);
        }
    }
}
