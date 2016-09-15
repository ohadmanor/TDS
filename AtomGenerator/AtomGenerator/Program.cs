using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace DBUtils
{
    class Program
    {
        static void addAtomsToRoute(Route route, AtomGenerator generator, int count)
        {
            for (int i = 0; i < count; i++)
            {
                double scatterLength = 0.0000;
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
                                                 "00:00:01", speed, route.guid, route.routePoints[0].x, route.routePoints[0].y);
                generator.createAtom(atom);
                generator.createActivityToAtom(activity, atom);
                generator.addAtomToTreeObject(atom);
            }
        }

        private static void addAtoms(String connectionParams)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            try
            {

                RoutesReader routesReader = new RoutesReader(connection);
                AtomGenerator generator = new AtomGenerator(connection);
                RouteGenerator routeGenerator = new RouteGenerator(connection);

                generator.deleteAllAtomsAndActivities();
                Route source1 = routesReader.readRouteByName("Escape3");
                Route source2 = routesReader.readRouteByName("Escape3_reversed");
                //Route source3 = routesReader.readRouteByName("Source3");
                //Route cornerRoute = routesReader.readRouteByName("Corner");

                //addAtomsToRoute(source1, generator, 100);
                //addAtomsToRoute(source2, generator, 100);
                //addAtomsToRoute(source3, generator, 100);
                //addAtomsToRoute(cornerRoute, generator);
                //AtomObject ambulance = new AtomObject("Ambulance1", -1, 34.8514473088014, 32.1008536878526);
                //generator.createAtom(ambulance);
                //generator.addAtomToTreeObject(ambulance);

                //routeGenerator.generateReversedRoute("Escape3");

                transaction.Commit();
            }
            catch (Exception exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    Console.WriteLine("Rollback failed :(");
                }
            }

            connection.Close();
        }

        private static void addDubekPolygonOpenings(String connectionParams)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            int[] edgesToAdd = new int[] { 0, 4, 5 };

            try
            {
                // get polygon points
                PolygonDB db = new PolygonDB(connection);
                Polygon polygon = db.getPolygonByName("Polygon1");
                List<PolygonPoint> points = db.getPolygonPointsByPolygonGUID(polygon.guid);
                
                // add an opening
                foreach (int i in edgesToAdd)
                {
                    double openingX = (points[i].x + points[(i + 1) % points.Count()].x) / 2;
                    double openingY = (points[i].y + points[(i + 1) % points.Count()].y) / 2;
                    PolygonOpening opening = new PolygonOpening(polygon.guid, i, openingX, openingY, 3);
                    db.addPolygonOpeningToPolygon(opening);
                }

                transaction.Commit();
            }
            catch (Exception exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    Console.WriteLine("Rollback failed :(");
                }
            }

            connection.Close();
        }

        static void addMalamPolygon(String connectionParams)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            try
            {
                Polygon polygon = new Polygon(Guid.NewGuid().ToString(), "Malam");
                List<PolygonPoint> points = new List<PolygonPoint>();
                points.Add(new PolygonPoint(polygon.guid, 0, 34.850942194461823, 32.098860770959512));
                points.Add(new PolygonPoint(polygon.guid, 1, 34.85137939453125, 32.098815327220322));
                points.Add(new PolygonPoint(polygon.guid, 2, 34.851290881633759, 32.098213195541732));
                points.Add(new PolygonPoint(polygon.guid, 3, 34.850845634937286, 32.098260911781828));
                points.Add(new PolygonPoint(polygon.guid, 4, 34.85086977481842, 32.098408604747945));
                points.Add(new PolygonPoint(polygon.guid, 5, 34.851129949092865, 32.098381338372178));
                points.Add(new PolygonPoint(polygon.guid, 6, 34.8511728644371, 32.098667634911841));
                points.Add(new PolygonPoint(polygon.guid, 7, 34.850918054580688, 32.098697173392637));


                PolygonDB db = new PolygonDB(connection);

                db.addPolygonToDB(polygon);
                db.addPolygonPoints(polygon, points);

                int[] edgesToAdd = new int[] { 1 };

                foreach (int i in edgesToAdd)
                {
                    double openingX = (points[i].x + points[(i + 1) % points.Count()].x) / 2;
                    double openingY = (points[i].y + points[(i + 1) % points.Count()].y) / 2;
                    PolygonOpening opening = new PolygonOpening(polygon.guid, i, openingX, openingY, 3);
                    db.addPolygonOpeningToPolygon(opening);
                }

                transaction.Commit();
            }
            catch (Exception exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    Console.WriteLine("Rollback failed :(");
                }
            }

            connection.Close();
        }

        static void addOpeningEscapeRoutes(String connectionParams)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            try
            {
                PolygonDB db = new PolygonDB(connection);
                db.addPolygonEscapePoint("Polygon1", 0, new DPoint(34.849061965942383, 32.099617405894811));
                db.addPolygonEscapePoint("Polygon1", 4, new DPoint(34.84963595867157, 32.098076863289826));
                db.addPolygonEscapePoint("Polygon1", 5, new DPoint(34.84862744808197, 32.0989584749222));
                db.addPolygonEscapePoint("Malam", 1, new DPoint(34.851486682891846, 32.098517670169642));
                transaction.Commit();
            }
            catch (Exception exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    Console.WriteLine("Rollback failed :(");
                }
            }

            connection.Close();
        }

        static void addBarriers(String connectionParams)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            try
            {
                BarriersDB db = new BarriersDB(connection);
                Barrier b1 = new Barrier(Util.CreateGuid(), 34.8514759540558, 32.0996901152282, 0);
                Barrier b2 = new Barrier(Util.CreateGuid(), 34.8514759540558, 32.1002763321138, 0);
                Barrier b3 = new Barrier(Util.CreateGuid(), 34.8511058092117, 32.0973406656135, 0);
                Barrier b4 = new Barrier(Util.CreateGuid(), 34.8504620790482, 32.0961000227717, 0);
                db.addBarriers(new Barrier[] { b1, b2, b3, b4 });
                transaction.Commit();
            }
            catch (Exception exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    Console.WriteLine("Rollback failed :(");
                }
            }

            connection.Close();
        }

        static async Task addWaypointRoutes(String connectionParams)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // travel waypoints
                //DPoint[] travelCoordinates = new DPoint[] { new DPoint(34.8496, 32.0996), new DPoint(34.8506, 32.099), new DPoint(34.8486, 32.099),
                //                                        new DPoint(34.8503, 32.0981), new DPoint(34.8487, 32.0983),
                //                                        new DPoint(34.8486, 32.099), new DPoint(34.851, 32.0997),
                //                                        new DPoint(34.8515, 32.0992), new DPoint(34.8514, 32.0983),
                //                                        new DPoint(34.8511, 32.0973), new DPoint(34.8501, 32.0977),
                //                                        new DPoint(34.8487, 32.0982), new DPoint(34.8478, 32.0986)};

                DPoint[] travelCoordinates = new DPoint[] { new DPoint(34.848627448082, 32.0995901398799), new DPoint(34.8495876789093, 32.0996264945646),
                                                        new DPoint(34.8505747318268, 32.0996492162353),
                                                        new DPoint(34.850612282753, 32.0982768171897), new DPoint(34.8492550849915, 32.0980950409352),
                                                        new DPoint(34.8486435413361, 32.098308627997), new DPoint(34.8514652252197, 32.0996855708965),
                                                        new DPoint(34.851508140564, 32.0986403686134), new DPoint(34.8511004447937, 32.0973452100618),
                                                        new DPoint(34.8498612642288, 32.0977905648985), new DPoint(34.8485255241394, 32.0982631839831),
                                                        new DPoint(34.8479408025742, 32.0971543430385), new DPoint(34.8504567146301, 32.096090933751)};

                // read all barrier coordinates
                BarriersDB barriersDB = new BarriersDB(connection);
                List<Barrier> barriers = barriersDB.readAllBarriers();

                RouteGenerator generator = new RouteGenerator(connection);

                // generate routes from waypoint to other waypoints
                for (int i = 0; i < travelCoordinates.Count(); i++)
                {
                    for (int j = 0; j < travelCoordinates.Count(); j++)
                    {
                        if (i == j) continue;
                        Route waypointRoute = await generator.generateRouteByShortestPath("WaypointToWaypoint_" + i + "," + j, travelCoordinates[i], travelCoordinates[j]);
                        generator.saveRouteToDB(waypointRoute);
                    }
                }

                // generate routes from waypoint to barriers
                for (int i = 0; i < travelCoordinates.Count(); i++)
                {
                    for (int j = 0; j < barriers.Count(); j++)
                    {
                        if (i == j) continue;

                        DPoint barrierCoordinates = new DPoint(barriers[j].x, barriers[j].y);
                        Route waypointRoute = await generator.generateRouteByShortestPath("WaypointToBarrier_" + i + "," + j, travelCoordinates[i], barrierCoordinates);
                        generator.saveRouteToDB(waypointRoute);
                    }
                }

                transaction.Commit();
            }
            catch (Exception exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    Console.WriteLine("Rollback failed :(");
                }
            }

            connection.Close();

        }

        static void removeWaypointRoutes(String connectionParams)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            try
            {
                RoutesReader reader = new RoutesReader(connection);
                RouteGenerator generator = new RouteGenerator(connection);

                // read all waypoint routes
                List<Route> waypointRoutes = reader.readRoutesStartingWith("Waypoint");

                // delete them
                generator.deleteRoutes(waypointRoutes);

                transaction.Commit();
            }
            catch (Exception exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    Console.WriteLine("Rollback failed :(");
                }
            }

            connection.Close();
        }

        static void query(String connectionParams, transactionFunction queryFunction)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionParams);
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            try
            {
                queryFunction(connectionParams);
                transaction.Commit();
            }
            catch (Exception exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackException)
                {
                    Console.WriteLine("Rollback failed :(");
                }
            }

            connection.Close();
        }

        public delegate void transactionFunction(String connectionParams);

        static void Main(string[] args)
        {
            transactionFunction function;
            String connectionParams = "Server=localhost;Port=5432;User Id=postgres;Password=yy11yy11;Database=TDS;";
            //addAtoms(connectionParams);
            //addDubekPolygonOpenings(connectionParams);
            //addMalamPolygon(connectionParams);
            //addOpeningEscapeRoutes(connectionParams);
            //addBarriers(connectionParams);
            //function += 
            addWaypointRoutes(connectionParams).Wait();
            //removeWaypointRoutes(connectionParams);
        }
    }
}
