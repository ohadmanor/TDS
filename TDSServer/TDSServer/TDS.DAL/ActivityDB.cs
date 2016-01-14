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
        public static void SaveActivityOLD(GeneralActivityDTO ActivityDTO)
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



        public static GeneralActivityDTO SaveActivity(GeneralActivityDTO ActivityDTO)
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
                        AtomData atomdata=DAL.AtomsDB.GetAtomByName(ActivityDTO.Atom.UnitName);
                        ActivityDTO.Atom.UnitGuid = atomdata.UnitGuid;
                        //  ******  Activity  *******
                        if (ActivityDTO.ActivityId == 0) //new Activity
                        {
                            sql = "insert into Activites (   atom_guid,StartActivityOffset,DurationActivity,ActivityType,Speed,route_guid,ReferencePointX,ReferencePointY,Activity_SeqNumber)" +
                                   "values ('" + atomdata.UnitGuid + "','" +
                                                 ActivityDTO.StartActivityOffset.ToString() + "','" +
                                                 ActivityDTO.DurationActivity.ToString() + "'," +
                                                 (int)ActivityDTO.ActivityType+ ","+
                                                 ActivityDTO.Speed + ",'" +
                                                 ActivityDTO.RouteActivity.RouteGuid + "'," +
                                                 ActivityDTO.ReferencePoint.x+"," +
                                                 ActivityDTO.ReferencePoint.y+ ","+
                                                 ActivityDTO.Activity_SeqNumber+
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
                            sql = "update Activites set atom_guid='" + atomdata.UnitGuid + "'" +
                                     " ,StartActivityOffset='" + ActivityDTO.StartActivityOffset.ToString() + "'" +
                                     " ,DurationActivity='" + ActivityDTO.DurationActivity.ToString() + "'" +
                                     " ,ActivityType=" + (int)ActivityDTO.ActivityType +
                                     " ,Speed=" + ActivityDTO.Speed +
                                     " ,route_guid='" + ActivityDTO.RouteActivity.RouteGuid + "'" +

                                     " ,ReferencePointX=" + ActivityDTO.ReferencePoint.x +
                                     " ,ReferencePointY=" + ActivityDTO.ReferencePoint.y +
                                     " ,Activity_SeqNumber=" + ActivityDTO.Activity_SeqNumber + 
                                  " where ActivityId=" + ActivityDTO.ActivityId;

                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                command.ExecuteNonQuery();
                            }

                        }
                        //**********************************************************







                        sqlTran.Commit();

                        return ActivityDTO;
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


            return null;
        }



        /*
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
        */

        public static IEnumerable<GeneralActivityDTO> GetActivitesByAtom(string AtomGuid)
        {
            try
            {
                List<GeneralActivityDTO> Activites = new List<GeneralActivityDTO>();
                string sql = "select * from Activites  where atom_guid='" + AtomGuid + "' order by Activity_SeqNumber";
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
                        activity.ActivityType = (enumActivity)Convert.ToInt32(row["ActivityType"]);                                         //  enumActivity.MovementActivity;
                        activity.ActivityId = System.Convert.ToInt32(row["ActivityId"]);
                        activity.Activity_SeqNumber = System.Convert.ToInt32(row["Activity_SeqNumber"]);
                        activity.StartActivityOffset = TimeSpan.Parse(row["StartActivityOffset"].ToString());
                        activity.DurationActivity = TimeSpan.Parse(row["DurationActivity"].ToString());
                        activity.Speed = System.Convert.ToInt32(row["Speed"]);

                        activity.ReferencePoint= new DPoint();
                        activity.ReferencePoint.x = System.Convert.ToDouble(row["ReferencePointX"]);
                        activity.ReferencePoint.y = System.Convert.ToDouble(row["ReferencePointY"]);

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




        public static IEnumerable<GeneralActivityDTO> GetAllActivites()
        {
            try
            {
                List<GeneralActivityDTO> Activites = new List<GeneralActivityDTO>();
                string sql = "select * from Activites order by ActivityId,Activity_SeqNumber";
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
                        activity.ActivityType = (enumActivity)Convert.ToInt32(row["ActivityType"]);                                         //  enumActivity.MovementActivity;
                        activity.ActivityId = System.Convert.ToInt32(row["ActivityId"]);
                        activity.Activity_SeqNumber = System.Convert.ToInt32(row["Activity_SeqNumber"]);
                        activity.StartActivityOffset = TimeSpan.Parse(row["StartActivityOffset"].ToString());
                        activity.DurationActivity = TimeSpan.Parse(row["DurationActivity"].ToString());
                        activity.Speed = System.Convert.ToInt32(row["Speed"]);

                        activity.ReferencePoint = new DPoint();
                        activity.ReferencePoint.x = System.Convert.ToDouble(row["ReferencePointX"]);
                        activity.ReferencePoint.y = System.Convert.ToDouble(row["ReferencePointY"]);

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


                    sql = "delete from Activites where ActivityId=" + ActivityId;
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


                    sql = "delete from Activites where atom_guid='" + AtomGuid + "'";
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
