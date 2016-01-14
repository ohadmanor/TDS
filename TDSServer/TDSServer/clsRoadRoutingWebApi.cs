using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using TerrainService;

namespace TDSServer
{

    public class shPath
    {
        public IList<shPoint> Points = new List<shPoint>();
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

        public static async Task<string> InitUserSession(string ScenarioId)
        {
            //VH Slow
            try
            {
               using( var client = new HttpClient())
               {
                   client.BaseAddress = new Uri(BaseAddress);
                   client.DefaultRequestHeaders.Accept.Clear();
                   client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                   string strUri = "api/Routing/initUserSession?ScenarioId=" + ScenarioId;
                   HttpResponseMessage response = await client.GetAsync(strUri);

                   if (response.StatusCode != System.Net.HttpStatusCode.OK)
                   {
                       return response.StatusCode.ToString();
                   }
                   HttpContent content = response.Content;
                   string v = await content.ReadAsStringAsync();
                   return "OK";
               }
                
            }
            catch(Exception ex)
            {
               
            }
            return "FALSE";
            
        }
        public static async Task<string> CloseUserSession(string ScenarioId)
        {           
            try
            {
                using( var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseAddress);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string strUri = "api/Routing/CloseUserSession?ScenarioId=" + ScenarioId;
                    HttpResponseMessage response = await client.GetAsync(strUri);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return response.StatusCode.ToString();
                    }

                    HttpContent content = response.Content;
                    string v = await content.ReadAsStringAsync();
                    return "OK";
                }
            }
            catch(Exception ex)
            {

            }
            return "FALSE";
           
        }

        public static async Task<shPath> FindShortPathWithArrayNodes(string ScenarioId, int[] arrNodeId, enOSMhighwayFilter[] arrHighwayFilter)
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                //   string strUri = "api/Routing/FindShortPathWithArrayNodes?ScenarioId=" + ScenarioId + "&highwayFilter=" + (int)highwayFilter + "&x=" + x + "&y=" + y + "&NodeidFromTo=" + NodeidFromTo + "&isPointFrom=" + isPointFrom;
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
                //  shPath tmp = null;
                shPath tmp = JsonConvert.DeserializeObject<shPath>(v);
                return tmp;
            }
        
        }


        public static async Task<shPath> FindShortPath(string ScenarioId, double StartX, double StartY, double DestinationX, double DestinationY, bool isPriorityAboveNormal)
        {
            try
            {
               using( var client = new HttpClient())
               {
                   client.BaseAddress = new Uri(BaseAddress);
                   client.DefaultRequestHeaders.Accept.Clear();
                   client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                   string strUri = "api/Routing/FindShortPath?ScenarioId=" + ScenarioId + "&StartX=" + StartX + "&StartY=" + StartY + "&DestinationX=" + DestinationX + "&DestinationY=" + DestinationY + "&isPriorityAboveNormal=" + isPriorityAboveNormal;

                   HttpResponseMessage response = null;
                   try
                   {
                       response = await client.GetAsync(strUri);
                       if (response.StatusCode != System.Net.HttpStatusCode.OK)
                       {
                           return null;
                       }

                   }
                   catch (System.Net.WebException e)
                   {
                       return null;
                   }
                   catch (TaskCanceledException e)
                   {
                       return null;
                   }
                   catch (Exception e)
                   {
                       return null;
                   }
                   HttpContent content = response.Content;
                   string v = await content.ReadAsStringAsync();

                   shPath tmp = JsonConvert.DeserializeObject<shPath>(v);
                   return tmp;
               }
               
            }
            catch(Exception ex)
            {

            }
            return null;

        }

        public static async Task<shPoint[]> GetRiversBordersCross(double x1, double y1, double x2, double y2)
        {
            using(  var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string strUri = "api/Routing/GetRiversBordersCross?x1=" + x1 + "&y1=" + y1 + "&x2=" + x2 + "&y2=" + y2;
                HttpResponseMessage response = await client.GetAsync(strUri);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }

                HttpContent content = response.Content;
                string v = await content.ReadAsStringAsync();

                shPoint[] tmp = JsonConvert.DeserializeObject<shPoint[]>(v);
                return tmp;

            }

        }

        public static async Task<List<GeoAreaBorder>> GetWorldLakesBorders()
        {
           using( var client = new HttpClient())
           {
               client.BaseAddress = new Uri(BaseAddress);
               client.DefaultRequestHeaders.Accept.Clear();
               client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

               string strUri = "api/Routing/getWorldLakesBorders";
               HttpResponseMessage response = await client.GetAsync(strUri);
               if (response.StatusCode != System.Net.HttpStatusCode.OK)
               {
                   return null;
               }

               HttpContent content = response.Content;
               string v = await content.ReadAsStringAsync();

               List<GeoAreaBorder> tmp = JsonConvert.DeserializeObject<List<GeoAreaBorder>>(v);
               return tmp;

           }

        }

        public static async Task<List<GeoAreaBorder>> GetWorldRiversBorders()
        {
            using( var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string strUri = "api/Routing/getWorldRiversBorders";
                HttpResponseMessage response = await client.GetAsync(strUri);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }

                HttpContent content = response.Content;
                string v = await content.ReadAsStringAsync();

                List<GeoAreaBorder> tmp = JsonConvert.DeserializeObject<List<GeoAreaBorder>>(v);
                return tmp;

            }

        }




        public static async Task<string> UpdateBridge(string ScenarioId, TerrainService.BridgeInfo Info)
        {
             using(var client = new HttpClient())
             {
                 client.BaseAddress = new Uri(BaseAddress);
                 client.DefaultRequestHeaders.Accept.Clear();
                 client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                 string strUri = "api/Routing/UpdateBridge?ScenarioId=" + ScenarioId;

                 HttpResponseMessage response = await client.PostAsJsonAsync(strUri, Info);

                 if (response.StatusCode != System.Net.HttpStatusCode.OK)
                 {
                     return response.StatusCode.ToString();
                 }


                 return "OK";

             }
        }


        public static async Task<string> AddUpdateScenarioPolygon(string ScenarioId, GeoAreaBorder AreaBorder)
        {
            using ( var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string strUri = "api/Routing/AddUpdateScenarioPolygon?ScenarioId=" + ScenarioId;

                HttpResponseMessage response = await client.PostAsJsonAsync(strUri, AreaBorder);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return response.StatusCode.ToString();
                }
                 return "OK";

            }
        }

        public static async Task<string> DeleteScenarioPolygon(string ScenarioId, enumPolygonType PolygonType, string PolygonName)
        {
             using (var client = new HttpClient())
             {
                 client.BaseAddress = new Uri(BaseAddress);
                 client.DefaultRequestHeaders.Accept.Clear();
                 client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                 string strUri = "api/Routing/DeleteScenarioPolygon?ScenarioId=" + ScenarioId + "&PolygonType=" + (int)PolygonType + "&PolygonName=" + PolygonName;
                 HttpResponseMessage response = await client.GetAsync(strUri);

                 if (response.StatusCode != System.Net.HttpStatusCode.OK)
                 {
                     return "FALSE";
                 }
                 return "OK";
             }

        }

        public static async Task<string> SetBridgeActiveStatus(string ScenarioId, string name, bool isActive)
        {
            using ( var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string strUri = "api/Routing/SetBridgeActiveStatus?ScenarioId=" + ScenarioId + "&name=" + name + "&isActive=" + isActive;
                HttpResponseMessage response = await client.GetAsync(strUri);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return "FALSE";
                }
                return "OK";
            }

        }

        public static async Task<string> RefreshObstacleOnRode(string ScenarioId, GeoAreaBorder[] Obstacles)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string strUri = "api/Routing/RefreshObstacleOnRode?ScenarioId=" + ScenarioId;

                HttpResponseMessage response = await client.PostAsJsonAsync(strUri, Obstacles);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return response.StatusCode.ToString();
                }

                return "OK";

            }
        }



        public static async Task<string> RemoveAllBridges(string ScenarioId)
        {
             using (var client = new HttpClient())
             {
                 client.BaseAddress = new Uri(BaseAddress);
                 client.DefaultRequestHeaders.Accept.Clear();
                 client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                 string strUri = "api/Routing/RemoveAllBridges?ScenarioId=" + ScenarioId;
                 HttpResponseMessage response = await client.GetAsync(strUri);

                 if (response.StatusCode != System.Net.HttpStatusCode.OK)
                 {
                     return "FALSE";
                 }

                 return "OK";

             }

        }


    }
}
