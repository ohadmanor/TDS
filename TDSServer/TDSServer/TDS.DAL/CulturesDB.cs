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
    class CulturesDB
    {
        static string strPostGISConnection = string.Empty;

        static CulturesDB()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            strPostGISConnection = appSettings["PostGISConnection"];
        }

        public static List<CultureData> getAllCulturalData()
        {
            try
            {
                List<CultureData> culturalDataSet = new List<CultureData>();
                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM cultural_data", connection);

                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // barrier(guid, x, y, angle)
                        culturalDataSet.Add(new CultureData((String)reader[0], (int)reader[1], (String)reader[2], (String)reader[3], 
                                            (String)reader[4], (int)reader[5], (int)reader[6], (double)reader[7],
                                            (double)reader[8], (double)reader[9], (double)reader[10], (double)reader[11]));
                    }

                    reader.Close();
                    connection.Close();
                }

                return culturalDataSet;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public static List<CultureData> getCultureDataByCountry(String country)
        {
            try
            {
                List<CultureData> culturalDataSet = new List<CultureData>();
                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM cultural_data WHERE country=:country", connection);
                    command.Parameters.Add(new NpgsqlParameter("country", country));

                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // barrier(guid, x, y, angle)
                        culturalDataSet.Add(new CultureData((String)reader[0], (int)reader[1], (String)reader[2], (String)reader[3],
                                            (String)reader[4], (int)reader[5], (int)reader[6], (double)reader[7],
                                            (double)reader[8], (double)reader[9], (double)reader[10], (double)reader[11]));
                    }

                    reader.Close();
                    connection.Close();
                }

                return culturalDataSet;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public static CultureGenderBiasData getCultureGenderBiasByCountry(String country)
        {
            try
            {
                List<CultureGenderBiasData> culturalDataSet = new List<CultureGenderBiasData>();
                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM cultural_gender_bias WHERE country=:country", connection);
                    command.Parameters.Add(new NpgsqlParameter("country", country));

                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        culturalDataSet.Add(new CultureGenderBiasData((String)reader[0], (String)reader[1], (double)reader[2]));
                    }

                    reader.Close();
                    connection.Close();
                }

                return (culturalDataSet != null && culturalDataSet.Count > 0) ? culturalDataSet.ElementAt(0) : null;
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
