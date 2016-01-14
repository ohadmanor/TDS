using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

using TerrainService;

namespace TDSClient
{

    public class shPath
    {
        public IList<shPoint> Points = new List<shPoint>();
    }

    public class shPointId
    {
        public shPoint point;
        public int nodeId;
    }

    public class ArrNodes
    {
       public string ScenarioId;
       public int[] arrNodeId;
       public enOSMhighwayFilter[] arrHighwayFilter;
    }


    public static class clsRoadRoutingWebApi
    {
        public static string BaseAddress = string.Empty;

        public static void SetBaseAddress(string WebApiAddress)
        {
            BaseAddress = WebApiAddress;
        }

        public static async Task<bool> PingRoutingWeb()
        {

            try
            {
               using( var client = new HttpClient())
               {
                   client.BaseAddress = new Uri(BaseAddress);
                   client.DefaultRequestHeaders.Accept.Clear();
                   client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                   string strUri = "api/Routing/Ping";
                   HttpResponseMessage response = await client.GetAsync(strUri);
                   if (response.StatusCode != System.Net.HttpStatusCode.OK)
                   {
                       return false;
                   }


                   return true;

               }
            }
            catch(Exception ex)
            {
                return false;
            }
        }



        public static async Task<shPointId> GetNearestPointIdOnRoad(string ScenarioId, enOSMhighwayFilter highwayFilter, double x, double y)
        {

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseAddress);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string strUri = "api/Routing/GetNearestPointIdOnRoad?ScenarioId=" + ScenarioId + "&highwayFilter=" + (int)highwayFilter + "&x=" + x + "&y=" + y;
                    HttpResponseMessage response = await client.GetAsync(strUri);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return null;
                    }

                    HttpContent content = response.Content;
                    string v = await content.ReadAsStringAsync();
                    shPointId tmp = JsonConvert.DeserializeObject<shPointId>(v);
                    return tmp;

                }
            }
            catch(Exception ex)
            {

            }
            return null;
            
        }

        public static async Task<shPointId> GetNearestRoadNodeWithCondition(string ScenarioId, enOSMhighwayFilter highwayFilter, double x, double y, int NodeidFromTo, bool isPointFrom)
        {
             using(var client = new HttpClient())
             {
                 client.BaseAddress = new Uri(BaseAddress);
                 client.DefaultRequestHeaders.Accept.Clear();
                 client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                 string strUri = "api/Routing/GetNearestRoadNodeWithCondition?ScenarioId=" + ScenarioId + "&highwayFilter=" + (int)highwayFilter + "&x=" + x + "&y=" + y + "&NodeidFromTo=" + NodeidFromTo + "&isPointFrom=" + isPointFrom;
                 HttpResponseMessage response = await client.GetAsync(strUri);

                 if (response.StatusCode != System.Net.HttpStatusCode.OK)
                 {
                     return null;
                 }

                 HttpContent content = response.Content;
                 string v = await content.ReadAsStringAsync();
                 shPointId tmp = JsonConvert.DeserializeObject<shPointId>(v);
                 return tmp;
             }
        
        }

        public static async Task<shPath> FindShortPathWithArrayNodes(string ScenarioId, int[] arrNodeId, enOSMhighwayFilter[] arrHighwayFilter)
        {
             using(var client = new HttpClient())
             {
                 client.BaseAddress = new Uri(BaseAddress);
                 client.DefaultRequestHeaders.Accept.Clear();
                 client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                 ArrNodes nodes = new ArrNodes();
                 nodes.ScenarioId = ScenarioId;
                 nodes.arrNodeId = arrNodeId;
                 nodes.arrHighwayFilter = arrHighwayFilter;

                 HttpResponseMessage response = await client.PostAsJsonAsync<ArrNodes>("api/Routing/FindShortPathWithArrayNodes/", nodes); //FindShortPathWithArrayNodes

                 if (response.StatusCode != System.Net.HttpStatusCode.OK)
                 {
                     return null;
                 }

                 HttpContent content = response.Content;
                 string v = await content.ReadAsStringAsync();
                 
                 shPath tmp = JsonConvert.DeserializeObject<shPath>(v);
                 return tmp;
             }    
              
        }

        public static async Task<shPath> FindShortPath(string ScenarioId, double StartX, double StartY, double DestinationX, double DestinationY, bool isPriorityAboveNormal)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                string strUri = "api/Routing/FindShortPath?ScenarioId=" + ScenarioId + "&StartX=" + StartX + "&StartY=" + StartY + "&DestinationX=" + DestinationX + "&DestinationY=" + DestinationY + "&isPriorityAboveNormal=" + isPriorityAboveNormal;

                HttpResponseMessage response = await client.GetAsync(strUri);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }

                HttpContent content = response.Content;
                string v = await content.ReadAsStringAsync();

                shPath tmp = JsonConvert.DeserializeObject<shPath>(v);
                return tmp;
            }

        }
    }
}
