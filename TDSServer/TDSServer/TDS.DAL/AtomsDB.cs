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
using TDSServer;

namespace TDSServer.TDS.DAL
{
    public class AtomsDB
    {
        static string strPostGISConnection = string.Empty;
        static AtomsDB()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            strPostGISConnection = appSettings["PostGISConnection"];
        }
        public void SaveAtom()
        {

        }

        private static AtomData GetAtomBySql(string sql)
        {
            try
            {
                AtomData atom = new AtomData();

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

                    foreach (DataRow row in dtPol.Rows)
                    {
                      
                        atom.UnitGuid = row["atom_guid"].ToString();
                        atom.UnitName = row["atom_name"].ToString();
                        atom.Location = new DPoint();
                        atom.Location.x = System.Convert.ToDouble(row["pointX"]);
                        atom.Location.y = System.Convert.ToDouble(row["pointY"]);              

                    }
                }


                return atom;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public static AtomData GetAtomByName(string Name)
        {
            string sql = "select * from AtomObjects where atom_name='" + Name + "'";
            AtomData atom = GetAtomBySql(sql);
            return atom;
        }
        public static IEnumerable<AtomData> GetAllAtoms()
        {
            try
            {
                List<AtomData> atoms = new List<AtomData>();
                string sql = "select * from AtomObjects";
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
                        AtomData atom = new AtomData();
                        atom.UnitGuid = row["atom_guid"].ToString();
                        atom.UnitName = row["atom_name"].ToString();
                        atom.Location = new DPoint();
                        atom.Location.x = System.Convert.ToDouble(row["pointX"]);
                        atom.Location.y = System.Convert.ToDouble(row["pointY"]);
                        atoms.Add(atom);
                    }
                }
                return atoms;
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        public static void DeleteAtomByGuid(string AtomGuid)
        {
            string sql = string.Empty;
            using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
            {
                connection.Open();
                NpgsqlTransaction sqlTran = connection.BeginTransaction();
                try
                {

                    sql = "delete from AtomObjects where atom_guid='" + AtomGuid + "'";
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

        public static void AddAtom(AtomData Atom)
        {
            try
            {
                string sql;
                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                {
                    connection.Open();

                    sql = "insert into AtomObjects (atom_guid,atom_name,CountryId,pointX,pointY)" +
                                 "values ('" + Atom.UnitGuid + "','" +
                                   Atom.UnitName + "'," +
                                   0 + "," +
                                   Atom.Location.x + "," +
                                   Atom.Location.y +

                                   " )  ";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {                     
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch(Exception ex)
            {

            }
        }


        public static void UpdateAtomPositionByGuid(string AtomGuid,double x,double y)
        {
            try
            {
                 using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                 {
                     connection.Open();
                     NpgsqlTransaction sqlTran = connection.BeginTransaction();
                     try
                     {
                         string sql = "update AtomObjects set pointX=" + x +
                                                           " ,pointY=" + y +
                                                           " where atom_guid='" + AtomGuid + "'";
                         using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                         {
                             command.Transaction = sqlTran;
                             command.ExecuteNonQuery();
                         }
                         sqlTran.Commit();
                     }
                     catch(Exception e)
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
            catch(Exception ex)
            {
               
            }
        }




        private static FormationTree GetAtomObjectFromTreeBySql(string sql)
        {
            try
            {
                FormationTree atom = new FormationTree();

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

                    foreach (DataRow row in dtPol.Rows)
                    {

                        atom.Identification = row["Identification"].ToString();
                        atom.GUID = row["GUID"].ToString();
                        atom.ParentGUID = row["ParentGUID"].ToString();
                        atom.PlatformCategoryId = (enumPlatformId)System.Convert.ToInt32(row["PlatformCategoryId"]);
                        atom.PlatformType = row["PlatformType"].ToString();

                    }
                }


                return atom;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        
        public static FormationTree GetAtomObjectFromTreeByName(string Name)
        {
            string sql = "select * from TreeObject where Identification='" + Name + "'";
            FormationTree atom = GetAtomObjectFromTreeBySql(sql);
            return atom;
        }
        public static FormationTree SaveTreeObject(FormationTree atomDTO)
        {
            try
            {
                //int Actid = ActivityDTO.ActivityId;
                //object ob = null;
                string sql;

                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                {
                    connection.Open();
                    NpgsqlTransaction sqlTran = connection.BeginTransaction();
                    try
                    {

                        //  ******  Atom  *******
                        if (string.IsNullOrEmpty(atomDTO.GUID)) //new Atom
                        {
                            atomDTO.GUID = Util.CretaeGuid().ToString();

                            sql = "insert into TreeObject (Identification,GUID,ParentGUID,PlatformCategoryId,PlatformType)" +
                                  "values ('" + atomDTO.Identification+ "','" +
                                    atomDTO.GUID + "','" +
                                    atomDTO.ParentGUID + "'," +
                                    (int)atomDTO.PlatformCategoryId+ ",'" +
                                    atomDTO.PlatformType+ "'" +                                   
                                    " )  ";
                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            sql = "update TreeObject set Identification='" + atomDTO.Identification + "'" +
                                    " ,GUID='" + atomDTO.GUID + "'" +
                                    " ,ParentGUID='" + atomDTO.GUID + "'" +
                                    " ,PlatformCategoryId=" + (int)atomDTO.PlatformCategoryId + 
                                    " ,PlatformType='" + atomDTO.PlatformType+ "'" +
                                    " where GUID='" + atomDTO.GUID + "'";
                            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                            {
                                command.Transaction = sqlTran;
                                command.ExecuteNonQuery();
                            }
                        }                   

                        sqlTran.Commit();

                        return atomDTO;
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

        public static IEnumerable<FormationTree> GetAllAtomsFromTree()
        {
            try
            {
                List<FormationTree> atoms = new List<FormationTree>();
                string sql = "select * from TreeObject order by Identification";
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
                        FormationTree atom = new FormationTree();
                        atom.Identification = row["Identification"].ToString();
                        atom.GUID = row["GUID"].ToString();
                        atom.ParentGUID = row["ParentGUID"].ToString();
                        atom.PlatformCategoryId = (enumPlatformId)System.Convert.ToInt32(row["PlatformCategoryId"]);
                        atom.PlatformType = row["PlatformType"].ToString();
                        atoms.Add(atom);
                    }
                }
                return atoms;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static void DeleteAtomFromTreeByGuid(string AtomGuid)
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



                    sql = "delete from AtomObjects where atom_guid='" + AtomGuid + "'";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Transaction = sqlTran;
                        int rows = command.ExecuteNonQuery();
                    }


                    sql = "delete from TreeObject where GUID='" + AtomGuid + "'";
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
