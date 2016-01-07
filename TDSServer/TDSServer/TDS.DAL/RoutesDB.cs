using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Npgsql;

namespace TDSServer.TDS.DAL
{
    public class RoutesDB
    {
        static string strPostGISConnection = string.Empty;
        static RoutesDB()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            strPostGISConnection = appSettings["PostGISConnection"];
        }



        private static Route GetRouteBySql(string sql)
        {
            try
            {
                Route route = new Route();

                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
                {
                    DataSet dsPol = new DataSet();
                    DataTable dtPol = new DataTable();
                    dsPol.Reset();
                    da.Fill(dsPol);
                    dtPol = dsPol.Tables[0];

                    if (dtPol == null || dtPol.Rows == null || dtPol.Rows.Count == 0)
                    {
                        return null;
                    }

                    foreach (DataRow rowPol in dtPol.Rows)
                    {
                        route.RouteName = rowPol["route_name"].ToString();

                        route.RouteGuid = rowPol["route_guid"].ToString();  //Pol.polygon_guid;                    

                    }
                }


                return route;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public static IEnumerable<DPoint> getRoutePoints(string route_guid)
        {
            List<DPoint> pntsList = new System.Collections.Generic.List<DPoint>();
            string sql = "select * from routes_points where route_guid='" + route_guid + "' order by point_num";

            using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];

                if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                foreach (DataRow row in dt.Rows)
                {

                    DPoint mp = new DPoint();
                    mp.x = System.Convert.ToDouble(row["pointx"]);
                    mp.y = System.Convert.ToDouble(row["pointy"]);
                    pntsList.Add(mp);
                }
            }


            return pntsList;
        }
        public static Route GetRouteByName(string Name)
        {
            string sql = "select * from routes where route_name='" + Name + "'";
            Route route = GetRouteBySql(sql);
            return route;
        }
        public static Route GetRouteByGuid(string RouteGuid)
        {
            string sql = "select * from routes where route_guid='" + RouteGuid + "'";
            Route route = GetRouteBySql(sql);
            route.Points = getRoutePoints(RouteGuid);
            return route;
        }
        public static void DeleteRouteByGuid(string RouteGuid)
        {
            string sql = string.Empty;
            using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
            {
                connection.Open();
                NpgsqlTransaction sqlTran = connection.BeginTransaction();
                try
                {
                    sql = "delete from routes_points where route_guid='" + RouteGuid + "'";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }

                    sql = "delete from routes where route_guid='" + RouteGuid + "'";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }
                    sqlTran.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        sqlTran.Rollback();
                    }
                    catch (Exception exRollback)
                    {

                    }
                   
                }
            }
        }

        public CreateUserEnum AddUpdateRoute(Route polObject)    
        {
            try
            {              
                if (string.IsNullOrEmpty(polObject.RouteGuid))    // //Add new polygon
                {
                    Route Record = RoutesDB.GetRouteByName(polObject.RouteName);
                    if (Record != null)
                    {
                        return CreateUserEnum.UserAlreadyExists;                                              

                    }

                    polObject.RouteGuid = Util.CretaeGuid().ToString();

                }

                CreateUserEnum result = RoutesDB.SaveRoute(polObject);

                if (result == DAL.CreateUserEnum.CreationSuccess)
                {
                    return CreateUserEnum.CreationSuccess;
                }
                else
                {
                    return CreateUserEnum.CreationFailed;
                }

            }
            catch (Exception ex)
            {
                return CreateUserEnum.CreationFailed;
            }

        }


        public static CreateUserEnum SaveRoute(Route route, NpgsqlConnection connection, NpgsqlTransaction sqlTran)
        {
               string sql = string.Empty;
           
               
               
                try
                {
                    sql = "delete from routes_points where route_guid='" + route.RouteGuid + "'";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }

                    sql = "delete from routes where route_guid='" + route.RouteGuid + "'";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }

                    if (route.Points != null)
                    {
                        int n = -1;
                        foreach (var p in route.Points)
                        {
                            n++;
                            sql = "insert into routes_points (route_guid,point_num,pointx,pointy)" +
                                "values ('" + route.RouteGuid + "'," +
                                                   n + "," +
                                                   p.x + "," +
                                                   p.y +
                                                    " )";

                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                int rows = command.ExecuteNonQuery();
                            }



                        }
                    }

                    sql = "insert into routes (route_guid,route_name)" +
                                "values ('" + route.RouteGuid + "','" +
                                              route.RouteName + "'" +

                                                    " )";


                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }

                  

                    return CreateUserEnum.CreationSuccess;

                }
                catch (Exception ex)
                {
                    
                    return CreateUserEnum.CreationFailed;
                }
        

        }

        public static CreateUserEnum SaveRoute(Route route)
        {
            string sql = string.Empty;
            using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
            {
                connection.Open();
                NpgsqlTransaction sqlTran = connection.BeginTransaction();
                try
                {
                    sql = "delete from routes_points where route_guid='" + route.RouteGuid + "'";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }

                    sql = "delete from routes where route_guid='" + route.RouteGuid + "'";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }

                    if (route.Points != null)
                    {
                        int n = -1;
                        foreach (var p in route.Points)
                        {
                            n++;
                            sql = "insert into routes_points (route_guid,point_num,pointx,pointy)" +
                                "values ('" + route.RouteGuid + "'," +
                                                   n + "," +
                                                   p.x + "," +
                                                   p.y +
                                                    " )";

                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                int rows = command.ExecuteNonQuery();
                            }



                        }
                    }

                    sql = "insert into routes (route_guid,route_name)" +
                                "values ('" + route.RouteGuid + "','" +
                                              route.RouteName + "'" +

                                                    " )";


                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }

                    sqlTran.Commit();

                    return CreateUserEnum.CreationSuccess;

                }
                catch (Exception ex)
                {
                    try
                    {
                        sqlTran.Rollback();
                    }
                    catch (Exception exRollback)
                    {

                    }
                    return CreateUserEnum.CreationFailed;
                }
            }

        }

    }
}
