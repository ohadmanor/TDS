using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;

namespace TDSServer
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
          

            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);

                var hubConfiguration = new HubConfiguration
                {
                    EnableDetailedErrors = true,
                    EnableJSONP = true



                };



                //********************
                //  JsonSerializerSettings serializerSettings = GlobalHost.Configuration.JsonFormatter.SerializerSettings;
                //  serializerSettings.TypeNameHandling = TypeNameHandling.Auto;

                //*********************




                //GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(180);


                GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromHours(2);

                GlobalHost.Configuration.DefaultMessageBufferSize = 50; //500;

                map.RunSignalR(hubConfiguration);
            });

        }
    }
}
