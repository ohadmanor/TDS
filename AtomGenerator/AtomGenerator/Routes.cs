using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json;
using TerrainService;
using Npgsql;

namespace DBUtils
{
    class RouteWebAPI
    {
        public static async Task<shPath> FindShortPath(string ScenarioId, double StartX, double StartY, double DestinationX, double DestinationY, bool isPriorityAboveNormal)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:9000/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string strUri = "api/Routing/FindShortPath?ScenarioId=" + ScenarioId + "&StartX=" + StartX + "&StartY=" + StartY + "&DestinationX=" + DestinationX + "&DestinationY=" + DestinationY + "&isPriorityAboveNormal=" + isPriorityAboveNormal;

                    HttpResponseMessage response = null;
                    try
                    {
                        response = await client.GetAsync(strUri);
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            Console.WriteLine("NOT OK: " + response.StatusCode.ToString());
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
            catch (Exception ex)
            {

            }
            return null;

        }
    }


    class RoutesReader
    {
        private NpgsqlConnection connection;

        public RoutesReader(NpgsqlConnection connection)
        {
            this.connection = connection;
        }

        public List<Route> readAllRoutes()
        {
            List<Route> routes = new List<Route>();

            NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM routes", connection);
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

            reader.Close();

            // now for all route add his points
            foreach (Route route in routes)
            {
                command.CommandText = "SELECT * FROM routes_points WHERE route_guid=:guid";
                command.Parameters.Add(new NpgsqlParameter("guid", route.guid));

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DPoint point = new DPoint((double)reader[2], (double)reader[3]);
                    route.routePoints.Add(point);
                }

                reader.Close();
            }

            return routes;
        }

        public List<Route> readRoutesStartingWith(String name)
        {
            List<Route> routes = new List<Route>();

            NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM routes WHERE route_name LIKE :name", connection);
            command.Parameters.AddWithValue(":name", name + "%");

            NpgsqlDataReader reader = command.ExecuteReader();

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

            reader.Close();

            foreach (Route route in routes)
            {
                // get route points
                command.CommandText = "SELECT * FROM routes_points WHERE route_guid=:guid";
                command.Parameters.Add(new NpgsqlParameter("guid", route.guid));

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DPoint point = new DPoint((double)reader[2], (double)reader[3]);
                    route.routePoints.Add(point);
                }

                reader.Close();
            }

            return routes;
        }

        public Route readRouteByName(String name)
        {
            NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM routes WHERE route_name=:name", connection);
            command.Parameters.Add(new NpgsqlParameter("name", name));

            NpgsqlDataReader reader = command.ExecuteReader();

            // get route (if there is one, if not YOU GET NOTHING! DEAL WITH IT!)
            if (!reader.Read())
            {
                reader.Close();
                return null;
            }

            Route route = new Route();
            route.routePoints = new List<DPoint>();
            route.guid = (reader[0] == DBNull.Value) ? null : (String)reader[0];
            route.name = (reader[1] == DBNull.Value) ? null : (String)reader[1];
            route.countryId = (reader[2] == DBNull.Value) ? -1 : (int)reader[2];
            route.routeTypeId = (reader[3] == DBNull.Value) ? -1 : (int)reader[3];
            route.owner = (reader[4] == DBNull.Value) ? null : (String)reader[4];

            reader.Close();

            // get route points
            command.CommandText = "SELECT * FROM routes_points WHERE route_guid=:guid";
            command.Parameters.Add(new NpgsqlParameter("guid", route.guid));

            reader = command.ExecuteReader();

            while (reader.Read())
            {
                DPoint point = new DPoint((double)reader[2], (double)reader[3]);
                route.routePoints.Add(point);
            }

            reader.Close();

            return route;
        }
    }

    class RouteGenerator
    {
        private NpgsqlConnection connection;

        public RouteGenerator(NpgsqlConnection connection)
        {
            this.connection = connection;
        }

        public void generateReversedRoute(String routeName)
        {
            generateReversedRoute(routeName, routeName + "_reversed");
        }

        public void generateReversedRoute(String routeName, String reversedRouteName)
        {
            RoutesReader routesReader = new RoutesReader(connection);
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
            NpgsqlCommand addRouteCommand = new NpgsqlCommand("INSERT INTO routes(route_guid, route_name, countryid, routetypeid, owner) VALUES (:guid, :name, :countryId, :typeid, :owner)", connection);
            addRouteCommand.Parameters.Add(new NpgsqlParameter("guid", route.guid));
            addRouteCommand.Parameters.Add(new NpgsqlParameter("name", route.name));
            addRouteCommand.Parameters.Add(new NpgsqlParameter("countryId", route.countryId));
            addRouteCommand.Parameters.Add(new NpgsqlParameter("typeid", route.routeTypeId));
            addRouteCommand.Parameters.Add(new NpgsqlParameter("owner", route.owner != null ? route.owner : ""));
            addRouteCommand.ExecuteNonQuery();

            // add its points
            for (int i = 0; i < route.routePoints.Count; i++)
            {
                String query = "INSERT INTO routes_points(route_guid, point_num, pointx, pointy)"
                             + " VALUES (:guid, :point_num, :x, :y)";
                NpgsqlCommand addRoutePointsCommand = new NpgsqlCommand(query, connection);
                addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("guid", route.guid));
                addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("point_num", i));
                addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("x", route.routePoints[i].x));
                addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("y", route.routePoints[i].y));
                addRoutePointsCommand.ExecuteNonQuery();
            }
        }

        public async Task<Route> generateRouteByShortestPath(String routeName, DPoint source, DPoint dest)
        {
            // assumption: source and dest are on a sidewalk
            shPath path = await RouteWebAPI.FindShortPath("0", source.x, source.y, dest.x, dest.y, false);
            Route route = new Route();
            route.countryId = 0;
            route.guid = Util.CreateGuid();
            route.name = routeName;
            route.owner = null;
            route.routeTypeId = 0;
            route.routePoints = new List<DPoint>();
            foreach (shPoint point in path.Points)
            {
                route.routePoints.Add(new DPoint(point.x, point.y));
            }

            return route;
        }

        public void deleteRoutes(List<Route> routes)
        {
            foreach (Route route in routes)
            {
                // delete all route points
                NpgsqlCommand deleteRoutePointsCommand = new NpgsqlCommand("DELETE FROM routes_points WHERE route_guid=:route_guid", connection);
                deleteRoutePointsCommand.Parameters.Add(new NpgsqlParameter("route_guid", route.guid));
                deleteRoutePointsCommand.ExecuteNonQuery();

                // delete the route itself
                NpgsqlCommand deleteRouteCommand = new NpgsqlCommand("DELETE FROM routes WHERE route_guid=:route_guid", connection);
                deleteRouteCommand.Parameters.Add(new NpgsqlParameter("route_guid", route.guid));
                deleteRouteCommand.ExecuteNonQuery();
            }
        }

        //public void removeAllRoutesWithNameStartingWith(String name)
        //{
        //    String query = "DELETE FROM products WHERE price = 10";
        //    NpgsqlCommand addRoutePointsCommand = new NpgsqlCommand(query, connection);
        //    addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("guid", route.guid));
        //    addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("point_num", i));
        //    addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("x", route.routePoints[i].x));
        //    addRoutePointsCommand.Parameters.Add(new NpgsqlParameter("y", route.routePoints[i].y));
        //    addRoutePointsCommand.ExecuteNonQuery();
        //}
    }
}
