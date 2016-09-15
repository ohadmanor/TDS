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
    class BarriersDB
    {
        static string strPostGISConnection = string.Empty;

        static BarriersDB()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            strPostGISConnection = appSettings["PostGISConnection"];
        }

        public static List<Barrier> getAllBarriers()
        {
            try
            {
                List<Barrier> barriers = new List<Barrier>();
                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM barriers", connection);

                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // barrier(guid, x, y, angle)
                        barriers.Add(new Barrier((String)reader[0], (double)reader[1], (double)reader[2], (double)reader[3]));
                    }

                    reader.Close();
                    connection.Close();
                }

                return barriers;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
