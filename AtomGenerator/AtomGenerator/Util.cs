using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using System.Net.Http;
//using System.Net.Http.Headers;

using Newtonsoft.Json;
//using TerrainService;

namespace DBUtils
{
    class Util
    {
        public static readonly Random rand = new Random();

        public static string CreateGuid()
        {
            Guid g = Guid.NewGuid();
            string guid = g.ToString();

            return guid.Replace("-", "");
        }

        //public static async Task<shPointId> GetNearestPointIdOnRoad(string ScenarioId, enOSMhighwayFilter highwayFilter, double x, double y)
        //{

        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.BaseAddress = new Uri(BaseAddress);
        //            client.DefaultRequestHeaders.Accept.Clear();
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //            string strUri = "api/Routing/GetNearestPointIdOnRoad?ScenarioId=" + ScenarioId + "&highwayFilter=" + (int)highwayFilter + "&x=" + x + "&y=" + y;
        //            HttpResponseMessage response = await client.GetAsync(strUri);

        //            if (response.StatusCode != System.Net.HttpStatusCode.OK)
        //            {
        //                return null;
        //            }

        //            HttpContent content = response.Content;
        //            string v = await content.ReadAsStringAsync();
        //            shPointId tmp = JsonConvert.DeserializeObject<shPointId>(v);
        //            return tmp;

        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return null;

        //}
    }
}
