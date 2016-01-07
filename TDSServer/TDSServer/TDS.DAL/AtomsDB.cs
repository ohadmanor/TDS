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
    }
}
