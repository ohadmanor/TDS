using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainService;

namespace TDSServer
{
    public class clsTerrain
    {
        private readonly static clsTerrain _instance = new clsTerrain();

        public static clsTerrain Instance
        {
            get
            {
                return _instance;
            }
        }
        private  clsTerrain()
        {
        }

        public  async Task<typRoute> CreateRoute(double StartX, double StartY, double ReferencePointX, double ReferencePointY, string RouteGuid)
        {

            try
            {
                List<DPoint> Result = new List<DPoint>();

                Route route = TDS.DAL.RoutesDB.GetRouteByGuid(RouteGuid);
                DPoint ReferPoint = new DPoint(ReferencePointX, ReferencePointY);

                shPath Path = await clsRoadRoutingWebApi.FindShortPath("0", StartX, StartY, ReferencePointX, ReferencePointY, false);
                if (Path != null && Path.Points.Count > 0)
                {
                    shPoint refPoint = Path.Points[Path.Points.Count - 1];
                    ReferPoint = new DPoint(refPoint.x, refPoint.y);
                   
                    foreach(shPoint p in Path.Points)
                    {
                        Result.Add(new DPoint(p.x, p.y));
                    }

                }

                if(Result.Count==0)
                {
                    Result.Add(new DPoint(StartX, StartY));
                }
                else
                {
                    if (Result[0].x != StartX || Result[0].y != StartY)
                    {
                        Result.Insert(0, new DPoint(StartX, StartY));
                    }
                }

                int leg = 0;
                DPoint minP = null;
                double mainDist = double.MaxValue;
                int i = -1;
                foreach (DPoint p in route.Points)
                {
                    i++;
                   // double dist = MathEngine.CalcDistanceForCompare(ReferPoint.x, ReferPoint.y, p.x, p.y);
                    double dist = MathEngine.GreatCircleDistance(ReferPoint.x, ReferPoint.y, p.x, p.y);
                    if (dist < mainDist)
                    {
                        mainDist = dist;
                        minP = p;
                        leg = i;
                    }
                }
                if(mainDist!=0.0)
                {
                    List<DPoint> R = route.Points.ToList<DPoint>().GetRange(leg, route.Points.Count() - leg);
                    Result.AddRange(R);
                }
                else
                {
                    if(leg<route.Points.Count()-1) // Not Last element
                    {
                        List<DPoint> R = route.Points.ToList<DPoint>().GetRange(leg+1, route.Points.Count() - (leg+1));
                        Result.AddRange(R);
                    }
                }

                Route routeResult = new Route();
                routeResult.Points = Result;


                typRoute tRoute = new typRoute(routeResult);
                return tRoute;
            }
            catch(Exception ex)
            {

            }

          

            return null;
        }

        public async Task<typRoute> createRouteByShortestPathOnly(double StartX, double StartY, double ReferencePointX, double ReferencePointY)
        {
            List<DPoint> Result = new List<DPoint>();

            DPoint ReferPoint = new DPoint(ReferencePointX, ReferencePointY);
            shPath Path = await clsRoadRoutingWebApi.FindShortPath("0", StartX, StartY, ReferencePointX, ReferencePointY, false);

            if (Path != null && Path.Points.Count > 0)
            {
                shPoint refPoint = Path.Points[Path.Points.Count - 1];
                ReferPoint = new DPoint(refPoint.x, refPoint.y);

                foreach (shPoint p in Path.Points)
                {
                    Result.Add(new DPoint(p.x, p.y));
                }

            }

            if (Result.Count == 0)
            {
                Result.Add(new DPoint(StartX, StartY));
            }
            else
            {
                if (Result[0].x != StartX || Result[0].y != StartY)
                {
                    Result.Insert(0, new DPoint(StartX, StartY));
                }
            }

            Route routeResult = new Route();
            routeResult.Points = Result;


            typRoute tRoute = new typRoute(routeResult);

            return tRoute;
        }

    }
}

