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
    public enum CreateUserEnum
    {
        CreationFailed = 0,
        CreationSuccess = 1,
        UserAlreadyExists = 2
    }
    public class UsersDB
    {
        static string strPostGISConnection = string.Empty;
        static UsersDB()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            strPostGISConnection = appSettings["PostGISConnection"];

        }
        public static UserMaps GetUserMaps(string userName)
        {

            try
            {
                UserMaps maps = new UserMaps();
                maps.User = userName;
               
                List<UserMapPreference> MapPreferenceList = new List<UserMapPreference>();
                string sql = "select * from user_maps where user_guid= '" + userName + "' Order By layer_order ";
                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
                {
                    DataSet ds = new DataSet();
                    DataTable dt = new DataTable();
                    ds.Reset();
                    da.Fill(ds);
                    dt = ds.Tables[0];
                    foreach (DataRow row in dt.Rows)
                    {
                        UserMapPreference MapPreference = new UserMapPreference();
                        MapPreference.MapName = row["map_name"].ToString();

                        MapPreference.MinZoom = System.Convert.ToInt32(row["min_zoom"]);
                        MapPreference.MaxZoom = System.Convert.ToInt32(row["max_zoom"]); 

                        MapPreferenceList.Add(MapPreference);
                    }
                    maps.maps = MapPreferenceList.ToArray<UserMapPreference>();
                    return maps;
                }
            }
            catch (Exception ex)
            {

            }


            return null;
        }

        public static void SaveUserMaps(UserMaps usrMaps)
        {
            try
            {
               
                string sql = "delete from user_maps where user_guid='" + usrMaps.User + "'";
                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                {
                    connection.Open();


                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        int rows = command.ExecuteNonQuery();
                    }
                    int n = 0;
                    foreach (UserMapPreference map in usrMaps.maps)
                    {
                        n++;
                        sql = "insert into user_maps  values('" + usrMaps.User + "','" + n.ToString() + "','" + map.MapName + "'," + map.MinZoom.ToString()+"," + map.MaxZoom.ToString()+","+"0,0)";
                        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                        {
                            int rows = command.ExecuteNonQuery();
                        }
                    }

                }

            }
            catch (Exception ex)
            {

            }
        }

    }
}
