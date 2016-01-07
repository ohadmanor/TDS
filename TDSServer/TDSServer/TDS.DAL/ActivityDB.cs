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
    public class ActivityDB
    {
        static string strPostGISConnection = string.Empty;
        static ActivityDB()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            strPostGISConnection = appSettings["PostGISConnection"];
        }
        public static void SaveActivity(GeneralActivityDTO ActivityDTO)
        {
            try
            {
                int Actid = ActivityDTO.ActivityId;
                object ob = null;
                string sql;
      
                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                {
                    connection.Open();
                    NpgsqlTransaction sqlTran = connection.BeginTransaction();
                    try
                    {

                        //  ******  Atom  *******
                        if (string.IsNullOrEmpty(ActivityDTO.Atom.UnitGuid)) //new Atom
                        {
                            ActivityDTO.Atom.UnitGuid = Util.CretaeGuid().ToString();

                            sql = "insert into AtomObjects (atom_guid,atom_name,CountryId,pointX,pointY)" +
                                  "values ('" + ActivityDTO.Atom.UnitGuid + "','" +
                                    ActivityDTO.Atom.UnitName + "'," +
                                    0 + "," +
                                    ActivityDTO.Atom.Location.x + "," +
                                    ActivityDTO.Atom.Location.y +

                                    " )  ";
                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            sql = "update AtomObjects set atom_guid='" + ActivityDTO.Atom.UnitGuid + "'" +
                                    " ,atom_name='" + ActivityDTO.Atom.UnitName + "'" +
                                    " ,CountryId=" + 0 +
                                    " ,pointX=" + ActivityDTO.Atom.Location.x +
                                    " ,pointY=" + ActivityDTO.Atom.Location.y +
                                    " where atom_guid='" + ActivityDTO.Atom.UnitGuid + "'";
                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                command.ExecuteNonQuery();
                            }
                        }

                        // ******  Route  ******

                        if (string.IsNullOrEmpty(ActivityDTO.RouteActivity.RouteGuid))
                        {
                            ActivityDTO.RouteActivity.RouteGuid = Util.CretaeGuid().ToString();
                        }
                        RoutesDB.SaveRoute(ActivityDTO.RouteActivity, connection, sqlTran);



                        //  ******  Activity  *******
                        if (ActivityDTO.ActivityId == 0) //new Activity
                        {
                            sql = "insert into ActivityMovement (atom_guid,StartActivityOffset,DurationActivity,Speed,route_guid)" +
                                   "values ('" + ActivityDTO.Atom.UnitGuid + "','" +
                                                      ActivityDTO.StartActivityOffset.ToString() + "','" +
                                                      ActivityDTO.DurationActivity.ToString() + "'," +
                                                      ActivityDTO.Speed + ",'" +
                                                      ActivityDTO.RouteActivity.RouteGuid + "'" +

                                                       " )  RETURNING ActivityId";


                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                Actid = (int)command.ExecuteScalar();
                                ActivityDTO.ActivityId = Actid;
                            }
                        }
                        else
                        {
                            sql = "update ActivityMovement set atom_guid='" + ActivityDTO.Atom.UnitGuid + "'" +
                                     " ,StartActivityOffset='" + ActivityDTO.StartActivityOffset.ToString() + "'" +
                                     " ,DurationActivity='" + ActivityDTO.DurationActivity.ToString() + "'" +
                                     " ,Speed=" + ActivityDTO.Speed +
                                     " ,route_guid='" + ActivityDTO.RouteActivity.RouteGuid + "'" +
                                  " where ActivityId=" + ActivityDTO.ActivityId;

                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                command.ExecuteNonQuery();
                            }

                        }
                        //**********************************************************



                       



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
            catch (Exception ex)
            {

            }
        }

        public static IEnumerable<GeneralActivityDTO> GetMovementActivitesByAtom(string AtomGuid)
        {
            try
            {
                List<GeneralActivityDTO> Activites = new List<GeneralActivityDTO>();
                string sql = "select * from ActivityMovement  where atom_guid='"+AtomGuid+"' order by StartActivityOffset";
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
                        GeneralActivityDTO activity = new GeneralActivityDTO();
                        activity.ActivityType = enumActivity.MovementActivity;
                        activity.ActivityId = System.Convert.ToInt32(row["ActivityId"]);
                        activity.StartActivityOffset = TimeSpan.Parse(row["StartActivityOffset"].ToString());
                        activity.DurationActivity = TimeSpan.Parse(row["DurationActivity"].ToString());
                        activity.Speed = System.Convert.ToInt32(row["Speed"]);

                        string RouteGuid = row["route_guid"].ToString();
                        activity.RouteActivity = TDS.DAL.RoutesDB.GetRouteByGuid(RouteGuid);
                       

                        Activites.Add(activity);
                    }
                }
                return Activites;
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        public static void DeleteActivityById(int ActivityId)
        {
            string sql = string.Empty;


            using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
            {
                connection.Open();
                NpgsqlTransaction sqlTran = connection.BeginTransaction();
                try
                {


                    sql = "delete from ActivityMovement where ActivityId=" + ActivityId ;
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

        public static void DeleteActivitesByAtomGuid(string AtomGuid)
        {
            string sql = string.Empty;


            using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
            {
                connection.Open();
                NpgsqlTransaction sqlTran = connection.BeginTransaction();
                try
                {


                    sql = "delete from ActivityMovement where atom_guid='" + AtomGuid+ "'";
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

    }
}
