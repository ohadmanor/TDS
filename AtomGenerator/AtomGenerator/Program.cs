using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace AtomGenerator
{
    class Activity
    {
        public int activityId;
        public String atomGuid;
        public int activitySeqNumber;
        public int activityType;
        public String startActivityOffset;
        public String durationActivity;
        public int speed;
        public String routeGuid;
        public double refX;
        public double refY;

        public Activity(int activityId, String atomGuid, int activitySeqNumber, int activityType, String startActivityOffset, String durationActivity,
                        int speed, String routeGuid, double refX, double refY)
        {
            this.activityId = activityId;
            this.atomGuid = atomGuid;
            this.activitySeqNumber = activitySeqNumber;
            this.activityType = activityType;
            this.startActivityOffset = startActivityOffset;
            this.durationActivity = durationActivity;
            this.speed = speed;
            this.routeGuid = routeGuid;
            this.refX = refX;
            this.refY = refY;
        }

    }

    class AtomObject
    {
        public AtomObject(String name, int countryId, double pointX, double pointY)
        {
            guid = Util.CreateGuid();
            this.name = name;
            this.countryId = countryId;
            this.pointX = pointX;
            this.pointY = pointY;
        }

        public String guid;
        public String name;
        public int countryId;
        public double pointX;
        public double pointY;
    }

    class Route
    {
        public String guid;
        public String name;
        public int countryId;
        public int routeTypeId;
        public String owner;
        public List<DPoint> routePoints;
    }

    class DPoint
    {
        public double x;
        public double y;

        public DPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    class RoutesReader
    {
        private String connectionParams;
        private NpgsqlConnection connection;

        public RoutesReader(String connectionParams)
        {
            this.connectionParams = connectionParams;
        }

        public List<Route> readAllRoutes()
        {
            List<Route> routes = new List<Route>();

            NpgsqlCommand command = query("SELECT * FROM routes");
            NpgsqlDataReader reader = command.ExecuteReader();

            // read the routes themselves
            while (reader.Read())
            {
                Route route = new Route();
                route.routePoints = new List<DPoint>();
                route.guid = (reader[0] == DBNull.Value) ? null : (String)reader[0];
                route.name = (reader[1] == DBNull.Value) ? null : (String)reader[1];
                route.countryId = (reader[2] == DBNull.Value) ? -1 : (int)reader[2];
                route.routeTypeId = (reader[3] == DBNull.Value) ? -1 : (int)reader[3];
                route.owner = (reader[4] == DBNull.Value) ? null : (String)reader[4];

                routes.Add(route);
            }

            connection.Close();

            // now for all route add his points
            foreach (Route route in routes)
            {
                NpgsqlCommand selectRoutePoints = query("SELECT * FROM routes_points WHERE route_guid='" + route.guid + "'");

                reader = selectRoutePoints.ExecuteReader();
                while (reader.Read())
                {
                    DPoint point = new DPoint((double)reader[2], (double)reader[3]);
                    route.routePoints.Add(point);
                }

                connection.Close();
            }

            return routes;
        }

        public Route readRouteByName(String name)
        {
            NpgsqlCommand command = query("SELECT * FROM routes WHERE route_name=:name");
            command.Parameters.Add(new NpgsqlParameter("name", name));

            NpgsqlDataReader reader = command.ExecuteReader();

            // get route (if there is one, if not YOU GET NOTHING! DEAL WITH IT!)
            if (!reader.Read()) return null;

            Route route = new Route();
            route.routePoints = new List<DPoint>();
            route.guid = (reader[0] == DBNull.Value) ? null : (String)reader[0];
            route.name = (reader[1] == DBNull.Value) ? null : (String)reader[1];
            route.countryId = (reader[2] == DBNull.Value) ? -1 : (int)reader[2];
            route.routeTypeId = (reader[3] == DBNull.Value) ? -1 : (int)reader[3];
            route.owner = (reader[4] == DBNull.Value) ? null : (String)reader[4];

            connection.Close();

            // get route points
            NpgsqlCommand selectRoutePoints = query("SELECT * FROM routes_points WHERE route_guid=:guid");
            selectRoutePoints.Parameters.Add(new NpgsqlParameter("guid", route.guid));

            reader = selectRoutePoints.ExecuteReader();
            while (reader.Read())
            {
                DPoint point = new DPoint((double)reader[2], (double)reader[3]);
                route.routePoints.Add(point);
            }

            connection.Close();

            return route;
        }

        private NpgsqlCommand query(String queryString)
        {
            connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlCommand command = new NpgsqlCommand(queryString, connection);

            return command;
        }
    }

    class RouteGenerator
    {
        private String connectionParams;

        public RouteGenerator(String connectionParams)
        {
            this.connectionParams = connectionParams;
        }

        public void generateReversedRoute(String routeName)
        {
            generateReversedRoute(routeName, routeName + "_reversed");
        }

        public void generateReversedRoute(String routeName, String reversedRouteName)
        {
            RoutesReader routesReader = new RoutesReader(connectionParams);
            Route route = routesReader.readRouteByName(routeName);
            Route reversedRoute = new Route();
            reversedRoute.guid = Util.CreateGuid();
            reversedRoute.name = reversedRouteName;
            reversedRoute.owner = route.owner;
            reversedRoute.routeTypeId = route.routeTypeId;
            reversedRoute.routePoints = new List<DPoint>();

            // copy points to reversed route
            foreach (DPoint point in route.routePoints)
            {
                reversedRoute.routePoints.Add(point);
            }

            // reverse route points
            reversedRoute.routePoints.Reverse();

            //after reading route generate a new GUID for it, change its name and reverse its route points
            saveRouteToDB(reversedRoute);
        }

        public void saveRouteToDB(Route route)
        {
            // add the route itself
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlCommand addRouteCommand = new NpgsqlCommand("INSERT INTO routes(route_guid, route_name, countryid, routetypeid, owner) VALUES (:guid, :name, :countryId, :typeid, :owner)", connection);
            addRouteCommand.Parameters.Add(new NpgsqlParameter("guid", route.guid));
            addRouteCommand.Parameters.Add(new NpgsqlParameter("name", route.name));
            addRouteCommand.Parameters.Add(new NpgsqlParameter("countryId", route.countryId));
            addRouteCommand.Parameters.Add(new NpgsqlParameter("typeid", route.routeTypeId));
            addRouteCommand.Parameters.Add(new NpgsqlParameter("owner", route.owner != null ? route.owner : ""));
            addRouteCommand.ExecuteNonQuery();
            connection.Close();

            // add its points
            for (int i = 0; i < route.routePoints.Count; i++)
            {
                connection = new NpgsqlConnection(connectionParams);
                connection.Open();
                String query = "INSERT INTO routes_points(route_guid, point_num, pointx, pointy)"
                             + " VALUES (:guid, :point_num, :x, :y)";
                NpgsqlCommand addRoutePointsCommand = new NpgsqlCommand(query, connection);
                addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("guid", route.guid));
                addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("point_num", i));
                addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("x", route.routePoints[i].x));
                addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("y", route.routePoints[i].y));
                addRoutePointsCommand.ExecuteNonQuery();
                connection.Close();
            }
        }
    }

    class AtomGenerator
    {
        private String connectionParams;

        public AtomGenerator(String connectionParams)
        {
            this.connectionParams = connectionParams;
        }

        public void deleteAllAtomsAndActivities()
        {
            // delete all atoms
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            String query = "TRUNCATE table atomobjects";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.ExecuteNonQuery();
            connection.Close();

            // delete all activities
            connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            query = "TRUNCATE table activites";
            command = new NpgsqlCommand(query, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void createAtom(AtomObject atom)
        {
            // save the object to the database
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            String query = "INSERT INTO atomobjects(atom_guid, atom_name, countryid, pointx, pointy) VALUES (:guid, :name, :countryId, :x, :y)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("guid", atom.guid));
            command.Parameters.Add(new NpgsqlParameter("name", atom.name));
            command.Parameters.Add(new NpgsqlParameter("countryId", atom.countryId));
            command.Parameters.Add(new NpgsqlParameter("x", atom.pointX));
            command.Parameters.Add(new NpgsqlParameter("y", atom.pointY));
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void createActivityToAtom(Activity activity, AtomObject atom)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            String query = "INSERT INTO activites(activityid, atom_guid, activity_seqnumber, activitytype,"
                + " startactivityoffset, durationactivity, speed, route_guid, referencepointx, referencepointy)"
                + " VALUES (:id, :atomGuid, :activitySeq, :activityType, :startOffset, :duration, :speed, :routeGuid, :refX, :refY)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("id", activity.activityId));
            command.Parameters.Add(new NpgsqlParameter("atomGuid", activity.atomGuid));
            command.Parameters.Add(new NpgsqlParameter("activitySeq", activity.activitySeqNumber));
            command.Parameters.Add(new NpgsqlParameter("activityType", activity.activityType));
            command.Parameters.Add(new NpgsqlParameter("startOffset", activity.startActivityOffset));
            command.Parameters.Add(new NpgsqlParameter("duration", activity.durationActivity));
            command.Parameters.Add(new NpgsqlParameter("speed", activity.speed));
            command.Parameters.Add(new NpgsqlParameter("routeGuid", activity.routeGuid));
            command.Parameters.Add(new NpgsqlParameter("refX", activity.refX));
            command.Parameters.Add(new NpgsqlParameter("refY", activity.refY));
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void addAtomToTreeObject(AtomObject atom)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            String query = "INSERT INTO treeobject(identification, guid, parentguid, countryid, platformcategoryid, platformtype, formationtypeid)"
                         + " VALUES (:identification, :guid, :parentguid, :countryid, :platformcategoryid, :platformtype, :formationtypeid)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("identification", atom.name));
            command.Parameters.Add(new NpgsqlParameter("guid", atom.guid));
            command.Parameters.Add(new NpgsqlParameter("parentguid", ""));
            command.Parameters.Add(new NpgsqlParameter("countryid", atom.countryId));
            command.Parameters.Add(new NpgsqlParameter("platformcategoryid", 1));
            command.Parameters.Add(new NpgsqlParameter("platformtype", ""));
            command.Parameters.Add(new NpgsqlParameter("formationtypeid", 1));
            command.ExecuteNonQuery();
            connection.Close();
        }
    }

    class Util
    {
        public static readonly Random rand = new Random();

        public static string CreateGuid()
        {
            Guid g = Guid.NewGuid();
            string guid = g.ToString();

            return guid.Replace("-", "");
        }
    }

    class Program
    {
        static void addAtomsToRoute(Route route, AtomGenerator generator)
        {
            for (int i = 0; i < 50; i++)
            {
                double scatterLength = 0.0002;
                double offset = Util.rand.NextDouble();

                double firstLegDeltaX = route.routePoints[1].x - route.routePoints[0].x;
                double firstLegDeltaY = route.routePoints[1].y - route.routePoints[0].y;

                double length = Math.Sqrt(firstLegDeltaX * firstLegDeltaX + firstLegDeltaY * firstLegDeltaY);

                double scatterX = (scatterLength / length) * firstLegDeltaX;
                double scatterY = (scatterLength / length) * firstLegDeltaY;

                AtomObject atom = new AtomObject(route.name + i, 0, route.routePoints[0].x - scatterX*offset, route.routePoints[0].y - scatterY*offset);

                // generate new random number for start time - for now between 0:01 to 1:30
                int minutes = Util.rand.Next(2);
                int seconds = Util.rand.Next(1, 60);
                int speed = Util.rand.Next(3, 11);
                String secondsString = seconds >= 10 ? seconds.ToString() : "0" + seconds;
                Activity activity = new Activity(100 + i, atom.guid, 1, 1, "00:0" + minutes + ":" + secondsString,
                                                 "00:00:00", speed, route.guid, route.routePoints[0].x, route.routePoints[0].y);
                generator.createAtom(atom);
                generator.createActivityToAtom(activity, atom);
                //generator.addAtomToTreeObject(atom);
            }
        }
        static void Main(string[] args)
        {
            String connectionParams = "Server=localhost;Port=5432;User Id=postgres;Password=yy11yy11;Database=TDS;";
            RoutesReader routesReader = new RoutesReader(connectionParams);
            AtomGenerator generator = new AtomGenerator(connectionParams);
            RouteGenerator routeGenerator = new RouteGenerator(connectionParams);

            generator.deleteAllAtomsAndActivities();

            Route rightRoute = routesReader.readRouteByName("RightToLeft");
            Route leftRoute = routesReader.readRouteByName("LeftToRight");

            addAtomsToRoute(rightRoute, generator);
            addAtomsToRoute(leftRoute, generator);
        }
    }
}
