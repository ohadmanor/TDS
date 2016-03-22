using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask.StateMashine
{
    public class RoutePlanner
    {
        public static typRoute planStraightLineRoute(DPoint source, DPoint dest, String routeName)
        {
            typRoute route = new typRoute();
            route.RouteName = routeName;
            route.arr_legs = new List<typLegSector>();

            typLegSector legSector = new typLegSector();
            legSector.FromLongn = source.x;
            legSector.FromLatn = source.y;
            legSector.ToLongn = dest.x;
            legSector.ToLatn = dest.y;
            legSector.LegDistance = (float)TerrainService.MathEngine.CalcDistance(legSector.FromLongn, legSector.FromLatn, legSector.ToLongn, legSector.ToLatn) / 1000f;
            route.arr_legs.Add(legSector);

            return route;
        }

        public static typRoute planRouteByShortestPath(DPoint source, DPoint dest, String routeName)
        {
            typRoute route = new typRoute();

            return route;
        }

        public static clsActivityMovement createActivityAndStart(clsGroundAtom atom, int speed, Route route)
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

            if (route != null && route.Points != null && route.Points.Count() > 0) {
                activity.ReferencePoint = new DPoint(route.Points.ElementAt(0).x, route.Points.ElementAt(0).y);
            }

            startActivity(activity);

            return activity;
        }

        public static void startActivity(clsActivityMovement activity)
        {
            activity.isActive = true;
            activity.isStarted = true;
            activity.isAborted = false;
            activity.isEnded = false;
        }

        public static Route createRoute(List<DPoint> points)
        {
            Route route = new Route();
            route.RouteGuid = Util.CretaeGuid();
            route.RouteName = "";
            route.Points = points;
            return route;
        }

        public static Route createRoute(DPoint source, DPoint dest)
        {
            List<DPoint> points = new List<DPoint>();
            points.Add(source);
            points.Add(dest);

            return createRoute(points);
        }
    }
}
