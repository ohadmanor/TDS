using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask.StateMashine
{
    public class RouteUtils
    {
        // Plan a straight line route
        public static typRoute planStraightLineRoute(DPoint source, DPoint dest, String routeName)
        {
            typRoute route = new typRoute();
            route.RouteName = routeName;
            route.arr_legs = new List<typLegSector>();

            // only one leg exists in a straight line
            typLegSector legSector = new typLegSector();
            legSector.FromLongn = source.x;
            legSector.FromLatn = source.y;
            legSector.ToLongn = dest.x;
            legSector.ToLatn = dest.y;
            legSector.LegDistance = (float)TerrainService.MathEngine.CalcDistance(legSector.FromLongn, legSector.FromLatn, legSector.ToLongn, legSector.ToLatn) / 1000f;

            // add the leg to the route
            route.arr_legs.Add(legSector);

            return route;
        }

        // create an activity, start it and return it
        public static clsActivityMovement createActivityAndStart(clsGroundAtom atom, int speed, Route route)
        {
            clsActivityMovement activity = createActivity(atom, speed, route);
            startActivity(activity);

            return activity;
        }

        // create an activity with default fields
        public static clsActivityMovement createActivity(clsGroundAtom atom, int speed, Route route)
        {
            clsActivityMovement activity = new clsActivityMovement();
            activity.ActivityId = 0;
            activity.AtomGuid = atom.GUID;
            activity.AtomName = atom.MyName;
            activity.DurationActivity = TimeSpan.FromSeconds(1);
            activity.TimeFrom = atom.m_GameObject.Ex_clockDate;
            activity.TimeTo = activity.TimeFrom.Add(TimeSpan.FromDays(365));
            activity.StartActivityOffset = TimeSpan.Zero;
            activity.Speed = speed;
            activity.ActivityType = enumActivity.MovementActivity;
            activity.RouteActivity = route;

            if (route != null && route.Points != null && route.Points.Count() > 0)
            {
                activity.ReferencePoint = new DPoint(route.Points.ElementAt(0).x, route.Points.ElementAt(0).y);
            }

            return activity;
        }
        
        // start an activity
        public static void startActivity(clsActivityMovement activity)
        {
            activity.isActive = true;
            activity.isStarted = true;
            activity.isAborted = false;
            activity.isEnded = false;
        }

        // create a route from a list of long/lat coordinates
        public static Route createRoute(List<DPoint> points)
        {
            Route route = new Route();
            route.RouteGuid = Util.CretaeGuid();
            route.RouteName = "";
            route.Points = points;
            return route;
        }

        // create a straight line route from source to dest
        public static Route createRoute(DPoint source, DPoint dest)
        {
            List<DPoint> points = new List<DPoint>();
            points.Add(source);
            points.Add(dest);

            return createRoute(points);
        }

        // convert from typRoute to Route
        public static Route typRouteToRoute(typRoute typRoute)
        {
            Route route = new Route();
            List<DPoint> points = new List<DPoint>();

            // add starting points of each leg sector
            foreach (typLegSector legSector in typRoute.arr_legs)
            {
                points.Add(new DPoint(legSector.FromLongn, legSector.FromLatn));
            }

            // add destination point from last leg sector
            typLegSector lastLegSector = typRoute.arr_legs[typRoute.arr_legs.Count() - 1];
            points.Add(new DPoint(lastLegSector.ToLongn, lastLegSector.ToLatn));

            // create the route
            return createRoute(points);
        }

        // create a typRoute from a list of points
        public static typRoute createTypRoute(List<DPoint> points, String routeName)
        {
            typRoute typRoute = new typRoute();
            typRoute.RouteName = routeName;
            typRoute.arr_legs = new List<typLegSector>();
            
            // construct leg sectors from points
            for (int i = 0; i < points.Count() - 1; i++)
            {
                typLegSector legSector = new typLegSector();
                legSector.FromLongn = points.ElementAt(i).x;
                legSector.FromLatn = points.ElementAt(i).y;
                legSector.ToLongn = points.ElementAt(i+1).x;
                legSector.ToLatn = points.ElementAt(i+1).y;
                legSector.LegDistance = (float)TerrainService.MathEngine.CalcDistance(legSector.FromLongn, legSector.FromLatn, legSector.ToLongn, legSector.ToLatn) / 1000f;
                typRoute.arr_legs.Add(legSector);
            }

            return typRoute;
        }

        // find the index of the closest point in a route to a specific point
        public static int findClosestPointIndexInRoute(DPoint point, Route route)
        {
            double minDistance = Double.MaxValue;
            int minDistanceIndex = 0;

            for (int i = 0; i < route.Points.Count(); i++)
            {
                double distance = TerrainService.MathEngine.CalcDistance(point.x, point.y, route.Points.ElementAt(i).x, route.Points.ElementAt(i).y);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minDistanceIndex = i;
                }
            }

            return minDistanceIndex;
        }
    }
}
